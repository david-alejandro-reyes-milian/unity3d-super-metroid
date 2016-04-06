/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Reflection;
using System;

/// Tools for editing the terrain.
public enum e2dTerrainTool { NONE = -1, EDIT_NODES = 0, ADJUST_HEIGHT, FILL_TEXTURE, CURVE_TEXTURE, GRASS }

/// Editor for the terrain component. It only takes care of the inspector part; the scene editor is delegated to
/// e2dTerrainSceneEditor.
[CustomEditor(typeof(e2dTerrain))]
public class e2dTerrainEditor : Editor
{
	/// Currently selected tool.
	private static e2dTerrainTool mTool = e2dTerrainTool.NONE;
	/// Scene editor taking care about the scene part of the terrain editor.
	private e2dTerrainSceneEditor mSceneEditor;
	/// Scroll position of the info box.
	private Vector2 mInfoBoxScroll;
	
	/// Currently selected texture.
	public int SelectedTexture;


#region Properties
	
	/// The terrain object the editor is manipulating.
	public e2dTerrain Terrain { get { return (e2dTerrain)this.target; } }
	
	/// Current tool.
	public e2dTerrainTool Tool 
	{ 
		get { return mTool; }
		set { mTool = value; }
	}
	
	/// Returns true if the brush is currently active.
	public bool IsBrushActive
	{
		get
		{
			return mTool == e2dTerrainTool.EDIT_NODES
				|| mTool == e2dTerrainTool.ADJUST_HEIGHT
				|| mTool == e2dTerrainTool.CURVE_TEXTURE;
		}
	}

	/// Returns the terrain generator editor instance if it exists.
	public e2dTerrainGeneratorEditor GeneratorEditor 
	{
		get 
		{
			if (Terrain.GetComponent<e2dTerrainGenerator>() == null) return null;
			return (e2dTerrainGeneratorEditor)Terrain.GetComponent<e2dTerrainGenerator>().EditorReference; 
		}
	}

	/// Returns the scene editor instance.
	public e2dTerrainSceneEditor SceneEditor { get { return mSceneEditor; } }

#endregion


#region Initialization

	/// Inits the editor.
	public e2dTerrainEditor()
	{
		mSceneEditor = new e2dTerrainSceneEditor(this);

		// setup the undo handler
		FieldInfo undoCallback = typeof(EditorApplication).GetField("undoRedoPerformed", BindingFlags.NonPublic | BindingFlags.Static);
		undoCallback.SetValue(null, (EditorApplication.CallbackFunction)OnUndoRedo);
	}

#endregion


#region Events

	/// Called when the scene window is to be redrawn.
	void OnSceneGUI()
	{
		mSceneEditor.OnSceneGUI();

		bool mouseEvent = Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag;
		// Note: we need to redraw the handles even when the brush is not active because the generator editor needs it as well
		if (/*IsBrushActive && */mouseEvent)
		{
			HandleUtility.Repaint();
		}

		// Note: this would disable controls of the editor camera
		//Event.current.Use();
	}
	
	/// Called when the inspector window is to be drawn. It manages all the GUI drawing and input.
	public override void OnInspectorGUI()
	{
		if (!PrepareInspector()) return;

		// tools
		GUI.changed = false;
		EditorGUILayout.BeginHorizontal();
		string[] toolOptions = e2dStrings.EDITOR_TOOLS;
		mTool = (e2dTerrainTool)GUILayout.Toolbar((int)mTool, toolOptions, GUILayout.MinWidth(0));
		EditorGUILayout.EndHorizontal();
		if (GeneratorEditor && GeneratorEditor.Tool != e2dGeneratorTool.NONE && mTool == e2dTerrainTool.NONE && Terrain.CurveIntercrossing)
		{
			GUI.changed = true;
			mTool = e2dTerrainTool.EDIT_NODES;
		}
		if (GUI.changed && mTool != e2dTerrainTool.NONE)
		{
			e2dEditorUtils.DeselectSceneTools();
			if (GeneratorEditor) GeneratorEditor.Tool = e2dGeneratorTool.NONE;
			SceneView.RepaintAll();
		}


		// scene view checks
		if (mTool != e2dTerrainTool.NONE && mTool != e2dTerrainTool.EDIT_NODES && !Terrain.IsEditable)
		{
			mTool = e2dTerrainTool.NONE;
			DrawToolError(e2dStrings.ERROR_TERRAIN_NOT_CREATED);
		}
		else
		{
			DrawToolInfo();

			switch (mTool)
			{
				case e2dTerrainTool.EDIT_NODES:
					DrawNodesTool();
					break;
				case e2dTerrainTool.ADJUST_HEIGHT:
					DrawHeightTool();
					break;
				case e2dTerrainTool.FILL_TEXTURE:
					DrawFillTextureTool();
					break;
				case e2dTerrainTool.CURVE_TEXTURE:
					DrawCurveTextureTool();
					break;
				case e2dTerrainTool.GRASS:
					DrawGrassTool();
					break;
			}

			EditorGUILayout.Separator();
		}

		EditorGUIUtility.LookLikeInspector();


		if (e2dUtils.DEBUG_INSPECTOR)
		{
			EditorGUILayout.Separator();
			EditorGUILayout.BeginHorizontal("box", GUILayout.ExpandWidth(true));
			EditorGUILayout.EndHorizontal();

			if (GUILayout.Button("Delete Subobjects", GUILayout.ExpandWidth(false))) Terrain.FillMesh.DeleteAllSubobjects();

			EditorGUILayout.Separator();
			DrawDefaultInspector();
		}

		if (e2dUtils.DEBUG_DUMP_STYLES.Length > 0)
		{
			Debug.Log("---------------------");
			foreach (GUIStyle style in GUI.skin.customStyles)
			{
				if (style.name.Contains(e2dUtils.DEBUG_DUMP_STYLES)) Debug.Log(style.name);
			}
			Debug.Log("--------------------");
		}
	}

	/// Makes the inspector ready for the GUI of the editor. Returns false if something went wrong and the GUI
	/// can't be drawn.
	private bool PrepareInspector()
	{
		Terrain.EditorReference = this;

		// make sure styles are ready
		e2dStyles.Init();

		// change the look of the elements to something more pretty then the default inspector
		EditorGUIUtility.LookLikeControls();
		EditorGUILayout.Separator();

		if (!e2dStyles.Inited)
		{
			EditorGUILayout.BeginVertical("HelpBox");
			if (e2dStyles.ErrorText != null) GUILayout.Label(e2dStrings.ERROR_CANT_FIND_ASSETS, e2dStyles.ErrorText);
			else GUILayout.Label(e2dStrings.ERROR_CANT_FIND_ASSETS);
			EditorGUILayout.EndVertical();
			return false;
		}

		if (e2dEditorUtils.IsAnySceneToolSelected())
		{
			// the user selected some other tools
			mTool = e2dTerrainTool.NONE;
		}

		return true;
	}

	/// Called when the undo or redo actions are performed.
	void OnUndoRedo()
	{
		// Note: we're processing the undo even if it's not ours
		Terrain.FixCurve();
		Terrain.FixBoundary();
		Terrain.RebuildAllMaterials();
		Terrain.RebuildAllMeshes();
	}

#endregion


#region Tools

	/// Tool for adding, deleting and moving terrain curve nodes.
	private void DrawNodesTool()
	{
		EditorGUILayout.BeginHorizontal();

		if (GUILayout.Button(e2dStrings.LABEL_RESET_TERRAIN, GUILayout.ExpandWidth(false)))
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_RESET_TERRAIN);
			Terrain.Reset();
			SceneView.RepaintAll();
		}

		if (GUILayout.Button(e2dStrings.LABEL_REBUILD_DATA, GUILayout.ExpandWidth(false)))
		{
			Terrain.FixCurve();
			Terrain.FixBoundary();
			Terrain.RebuildAllMaterials();
			Terrain.RebuildAllMeshes();
		}

		EditorGUILayout.EndHorizontal();


		GUI.changed = false;
		bool close = EditorGUILayout.Toggle(e2dStrings.LABEL_CURVE_CLOSED, Terrain.CurveClosed);
		if (GUI.changed)
		{
			Undo.RegisterUndo(Terrain, close ? e2dStrings.UNDO_CLOSE_CURVE : e2dStrings.UNDO_OPEN_CURVE);
			Terrain.CurveClosed = close;
			Terrain.FixCurve();
			Terrain.CurveMesh.UpdateControlTextures();
			Terrain.RebuildAllMeshes();
			EditorUtility.SetDirty(Terrain);
		}

		GUI.changed = false;
		bool plastic = EditorGUILayout.Toggle(e2dStrings.LABEL_PLASTIC_EDGES, Terrain.PlasticEdges);
		if (GUI.changed)
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_TERRAIN_PROPERTIES);
			Terrain.PlasticEdges = plastic;
			Terrain.RebuildAllMeshes();
			EditorUtility.SetDirty(Terrain);
		}
	}

	/// Tool for adjusting the height of a group of nodes using a brush in the scene window.
	private void DrawHeightTool()
	{
		DrawBrush(true, true);
	}

	/// Tool for setting the parameters of the filling of the terrain inside and the terrain boundary.
	private void DrawFillTextureTool()
	{
		GUI.changed = false;
		Texture fillTexture = (Texture)EditorGUILayout.ObjectField(e2dStrings.LABEL_FILL_TEXTURE, Terrain.FillTexture, typeof(Texture), !EditorUtility.IsPersistent(target));
		if (GUI.changed)
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_FILL_TEXTURE);
			Terrain.FillTexture = fillTexture;
			Terrain.FillMesh.RebuildMaterial();
			EditorUtility.SetDirty(Terrain);
		}

		GUI.changed = false;
		float fillTextureTileWidth = EditorGUILayout.Slider(e2dStrings.LABEL_FILL_TEXTURE_TILE_WIDTH, Terrain.FillTextureTileWidth, e2dConstants.FILL_TEXTURE_SIZE_MIN, e2dConstants.FILL_TEXTURE_SIZE_MAX);
		float fillTextureTileHeight = EditorGUILayout.Slider(e2dStrings.LABEL_FILL_TEXTURE_TILE_HEIGHT, Terrain.FillTextureTileHeight, e2dConstants.FILL_TEXTURE_SIZE_MIN, e2dConstants.FILL_TEXTURE_SIZE_MAX);
		float fillTextureTileOffsetX = EditorGUILayout.Slider(e2dStrings.LABEL_FILL_TEXTURE_TILE_OFFSET_X, Terrain.FillTextureTileOffsetX, 0, e2dConstants.FILL_TEXTURE_OFFSET_MAX);
		float fillTextureTileOffsetY = EditorGUILayout.Slider(e2dStrings.LABEL_FILL_TEXTURE_TILE_OFFSET_Y, Terrain.FillTextureTileOffsetY, 0, e2dConstants.FILL_TEXTURE_OFFSET_MAX);
		if (GUI.changed)
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_FILL_TEXTURE);
			Terrain.FillTextureTileWidth = fillTextureTileWidth;
			Terrain.FillTextureTileHeight = fillTextureTileHeight;
			Terrain.FillTextureTileOffsetX = fillTextureTileOffsetX;
			Terrain.FillTextureTileOffsetY = fillTextureTileOffsetY;
			Terrain.FillMesh.RebuildMesh();
			EditorUtility.SetDirty(Terrain);
		}

		GUI.changed = false;
		Rect terrainBoundary = e2dEditorUtils.RectField(e2dStrings.LABEL_TERRAIN_BOUNDARY, Terrain.TerrainBoundary);
		if (GUI.changed)
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_BOUNDARY);
			Terrain.TerrainBoundary = terrainBoundary;
			Terrain.FixBoundary();
			Terrain.FillMesh.RebuildMesh();
			EditorUtility.SetDirty(Terrain);
		}
	}
	
	/// Tool for texturing the terrain surface using a brush.
	private void DrawCurveTextureTool()
	{
		DrawBrush(false, false);

		DrawCurveTextureSelector();

		bool rebuildMaterial = false;
		bool rebuildMesh = false;

		e2dCurveTexture selectedTexture = new e2dCurveTexture(Terrain.CurveTextures[SelectedTexture]);

		// texture
		EditorGUILayout.BeginHorizontal(e2dStyles.TextureField);
		GUI.changed = false;
		EditorGUILayout.PrefixLabel(e2dStrings.LABEL_TEXTURE);
		selectedTexture.texture = (Texture)EditorGUILayout.ObjectField(selectedTexture.texture, typeof(Texture), !EditorUtility.IsPersistent(target));
		GUILayoutUtility.GetRect(70, 0, GUI.skin.label);
		if (GUI.changed) rebuildMaterial = true;
		EditorGUILayout.EndHorizontal();

		// size
		GUI.changed = false;
		selectedTexture.size.x = EditorGUILayout.Slider(e2dStrings.LABEL_CURVE_TEXTURE_WIDTH, selectedTexture.size.x, e2dConstants.TEXTURE_WIDTH_MIN, e2dConstants.TEXTURE_WIDTH_MAX);
		selectedTexture.size.y = EditorGUILayout.Slider(e2dStrings.LABEL_CURVE_TEXTURE_HEIGHT, selectedTexture.size.y, e2dConstants.TEXTURE_HEIGHT_MIN, e2dConstants.TEXTURE_HEIGHT_MAX);
		if (GUI.changed) rebuildMesh = rebuildMaterial = true;

		// fade
		GUI.changed = false;
		selectedTexture.fadeThreshold = EditorGUILayout.Slider(e2dStrings.LABEL_CURVE_TEXTURE_FADE_THRESHOLD, selectedTexture.fadeThreshold, 0, 1);
		if (GUI.changed) rebuildMaterial = true;

		// fixed angle
		GUI.changed = false;
		selectedTexture.fixedAngle = EditorGUILayout.Toggle(e2dStrings.LABEL_CURVE_TEXTURE_FIXED_ANGLE, selectedTexture.fixedAngle);
		if (GUI.changed) rebuildMaterial = true;

		// update the structure
		if (rebuildMaterial || rebuildMesh)
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_CURVE_TEXTURE);
			Terrain.CurveTextures[SelectedTexture] = selectedTexture;
		}

		if (rebuildMaterial)
		{
			Terrain.CurveMesh.RebuildMaterial();
			EditorUtility.SetDirty(Terrain);
		}
		if (rebuildMesh)
		{
			Terrain.CurveMesh.RebuildMesh();
			EditorUtility.SetDirty(Terrain);
		}

		if (e2dUtils.DEBUG_CONTROL_TEXTURES) DrawCurveControlTextures();
	}

	/// Selector for choosing from a list of textures.
	private void DrawCurveTextureSelector()
	{
		// texture box
		// Note: turning ExpandWidth off doesn't work here because the box contains more horizontal boxes which
		// stretch the parent box width
		EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));

		// texture selecting
		SelectedTexture = Mathf.Max(0, Mathf.Min(SelectedTexture, Terrain.CurveTextures.Count - 1));
		Texture[] textures = new Texture[Terrain.CurveTextures.Count];
		for (int i = 0; i < textures.Length; i++)
		{
			textures[i] = Terrain.CurveTextures[i].texture;
		}
		SelectedTexture = GUILayout.SelectionGrid(SelectedTexture, textures, Mathf.Min(5, Terrain.CurveTextures.Count), e2dStyles.TextureSelector, GUILayout.ExpandWidth(false));

		// adding and deleting textures
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
		if (GUILayout.Button(e2dStrings.BUTTON_ADD_TEXTURE, GUILayout.ExpandWidth(false)))
		{
			// TODO: currently the shader doesn't work with more than 4 textures, so that's why it's limited
			if (Terrain.CurveTextures.Count == 4) GUIUtility.ExitGUI();

			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_CURVE_TEXTURE);
			Terrain.CurveMesh.AppendCurveTexture();
			SelectedTexture = Terrain.CurveTextures.Count - 1;
			Terrain.CurveMesh.RebuildMaterial();
			EditorUtility.SetDirty(Terrain);
			GUIUtility.ExitGUI();

		}
		if (GUILayout.Button(e2dStrings.BUTTON_DELETE_TEXTURE, GUILayout.ExpandWidth(false)))
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_CURVE_TEXTURE);
			Terrain.CurveMesh.RemoveCurveTexture(SelectedTexture);
			Terrain.CurveMesh.RebuildMaterial();
			Terrain.CurveMesh.RebuildMesh();
			EditorUtility.SetDirty(Terrain);
			GUIUtility.ExitGUI();
		}
		EditorGUILayout.EndHorizontal();


		EditorGUILayout.EndVertical();
	}

	/// Debug draw of control textures of the curve mesh.
	private void DrawCurveControlTextures()
	{
		Rect rect = EditorGUILayout.BeginHorizontal();
		GUIStyle style = new GUIStyle(GUI.skin.button);
		style.fixedWidth = e2dConstants.CONTROL_TEXTURE_SIZE + 1;
		style.fixedHeight = e2dConstants.CONTROL_TEXTURE_SIZE + 1;
		GUILayout.SelectionGrid(-1, Terrain.CurveMesh.ControlTextures.ToArray(), Terrain.CurveMesh.ControlTextures.Count, style);
		EditorGUILayout.EndHorizontal();

		for (int i = 0; i < Terrain.CurveMesh.ControlTextures.Count; i++)
		{
			float x = rect.xMin + i * (e2dConstants.CONTROL_TEXTURE_SIZE + 1);
			float y = rect.yMin;
			EditorGUI.DrawPreviewTexture(new Rect(x, y, e2dConstants.CONTROL_TEXTURE_SIZE, 0.5f * e2dConstants.CONTROL_TEXTURE_SIZE), Terrain.CurveMesh.ControlTextures[i], null, ScaleMode.StretchToFill);
			y += 0.5f * e2dConstants.CONTROL_TEXTURE_SIZE + 1;
			EditorGUI.DrawTextureAlpha(new Rect(x, y, e2dConstants.CONTROL_TEXTURE_SIZE, 0.5f * e2dConstants.CONTROL_TEXTURE_SIZE), Terrain.CurveMesh.ControlTextures[i], ScaleMode.StretchToFill);
			y += 0.25f * e2dConstants.CONTROL_TEXTURE_SIZE;
			EditorGUI.DropShadowLabel(new Rect(x, y, e2dConstants.CONTROL_TEXTURE_SIZE, 0.25f * e2dConstants.CONTROL_TEXTURE_SIZE), "" + Terrain.CurveMesh.ControlTextures[i].width);
		}
	}

	/// Tool for drawing grass using a brush.
	private void DrawGrassTool()
	{
		// brush
		GUILayout.Label(e2dStrings.LABEL_BRUSH, e2dStyles.Header);

		DrawBrush(true, false);

		bool rebuildMaterial = false;
		bool rebuildMesh = false;


		// global parameters
		GUILayout.Label(e2dStrings.LABEL_GLOBAL_PARAMETERS, e2dStyles.Header);
		GUI.changed = false;
		Terrain.GrassScatterRatio = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_SCATTER_RATIO, Terrain.GrassScatterRatio, e2dConstants.GRASS_SCATTER_RATIO_MIN, e2dConstants.GRASS_SCATTER_RATIO_MAX);
		Terrain.GrassWaveSpeed = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_WAVE_SPEED, Terrain.GrassWaveSpeed, e2dConstants.GRASS_WAVE_SPEED_MIN, e2dConstants.GRASS_WAVE_SPEED_MAX);
		if (GUI.changed) rebuildMesh = rebuildMaterial = true;


		// texture settings
		GUILayout.Label(e2dStrings.LABEL_TEXTURE_SETTINGS, e2dStyles.Header);

		DrawGrassTextureSelector();
		e2dGrassTexture selectedTexture = new e2dGrassTexture(Terrain.GrassTextures[SelectedTexture]);

		// texture
		EditorGUILayout.BeginHorizontal(e2dStyles.TextureField);
		GUI.changed = false;
		EditorGUILayout.PrefixLabel(e2dStrings.LABEL_TEXTURE);
		selectedTexture.texture = (Texture)EditorGUILayout.ObjectField(selectedTexture.texture, typeof(Texture), !EditorUtility.IsPersistent(target));
		GUILayoutUtility.GetRect(70, 0, GUI.skin.label);
		if (GUI.changed) rebuildMaterial = true;
		EditorGUILayout.EndHorizontal();

		// params
		GUI.changed = false;
		selectedTexture.waveAmplitude = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_WAVE_AMPLITUDE, selectedTexture.waveAmplitude, e2dConstants.GRASS_WAVE_AMPLITUDE_MIN, e2dConstants.GRASS_WAVE_AMPLITUDE_MAX);
		selectedTexture.sizeRandomness.x = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_WIDTH_RANDOMNESS, selectedTexture.sizeRandomness.x, e2dConstants.GRASS_SIZE_RANDOMNESS_MIN, e2dConstants.GRASS_SIZE_RANDOMNESS_MAX);
		selectedTexture.sizeRandomness.y = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_HEIGHT_RANDOMNESS, selectedTexture.sizeRandomness.y, e2dConstants.GRASS_SIZE_RANDOMNESS_MIN, e2dConstants.GRASS_SIZE_RANDOMNESS_MAX);
		selectedTexture.size = e2dEditorUtils.Vector2Field(e2dStrings.LABEL_GRASS_SIZE, selectedTexture.size);
		selectedTexture.size.x = Mathf.Max(0, selectedTexture.size.x);
		selectedTexture.size.y = Mathf.Max(0, selectedTexture.size.y);
		if (GUI.changed) rebuildMesh = true;

		// update the structure
		if (rebuildMaterial || rebuildMesh)
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_GRASS_TEXTURE);
			Terrain.GrassTextures[SelectedTexture] = selectedTexture;
		}

		if (rebuildMaterial)
		{
			Terrain.GrassMesh.RebuildMaterial();
			EditorUtility.SetDirty(Terrain);
		}
		if (rebuildMesh)
		{
			Terrain.GrassMesh.RebuildMesh();
			EditorUtility.SetDirty(Terrain);
		}
	}

	/// Selector for choosing from a list of textures.
	private void DrawGrassTextureSelector()
	{
		// texture box
		// Note: turning ExpandWidth off doesn't work here because the box contains more horizontal boxes which
		// stretch the parent box width
		EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(false));

		// texture selecting
		SelectedTexture = Mathf.Max(0, Mathf.Min(SelectedTexture, Terrain.GrassTextures.Count - 1));
		Texture[] textures = new Texture[Terrain.GrassTextures.Count];
		for (int i = 0; i < textures.Length; i++)
		{
			textures[i] = Terrain.GrassTextures[i].texture;
		}
		SelectedTexture = GUILayout.SelectionGrid(SelectedTexture, textures, Mathf.Min(5, Terrain.GrassTextures.Count), e2dStyles.TextureSelector, GUILayout.ExpandWidth(false));

		// adding and deleting textures
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
		if (GUILayout.Button(e2dStrings.BUTTON_ADD_TEXTURE, GUILayout.ExpandWidth(false)))
		{
			if (Terrain.GrassTextures.Count == e2dConstants.MAX_GRASS_TEXTURES) GUIUtility.ExitGUI();

			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_GRASS_TEXTURE);
			Terrain.GrassMesh.AppendGrassTexture();
			SelectedTexture = Terrain.GrassTextures.Count - 1;
			Terrain.GrassMesh.RebuildMaterial();
			EditorUtility.SetDirty(Terrain);
			GUIUtility.ExitGUI();

		}
		if (GUILayout.Button(e2dStrings.BUTTON_DELETE_TEXTURE, GUILayout.ExpandWidth(false)))
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_GRASS_TEXTURE);
			Terrain.GrassMesh.RemoveGrassTexture(SelectedTexture);
			Terrain.GrassMesh.RebuildMaterial();
			Terrain.GrassMesh.RebuildMesh();
			EditorUtility.SetDirty(Terrain);
			GUIUtility.ExitGUI();
		}
		EditorGUILayout.EndHorizontal();


		EditorGUILayout.EndVertical();
	}

	/// Brush tool settings.
	private void DrawBrush(bool opacity, bool angle)
	{
		mSceneEditor.BrushSize = EditorGUILayout.IntSlider(e2dStrings.LABEL_BRUSH_SIZE, mSceneEditor.BrushSize, e2dConstants.BRUSH_SIZE_MIN, e2dConstants.BRUSH_SIZE_MAX);
		if (opacity)
		{
			mSceneEditor.BrushOpacity = EditorGUILayout.IntSlider(e2dStrings.LABEL_BRUSH_OPACITY, mSceneEditor.BrushOpacity, e2dConstants.BRUSH_OPACITY_MIN, e2dConstants.BRUSH_OPACITY_MAX);
		}
		if (angle)
		{
			mSceneEditor.BrushAngle = EditorGUILayout.Slider(e2dStrings.LABEL_BRUSH_ANGLE, mSceneEditor.BrushAngle, 0, 360);
		}
	}
	
	/// Draws the description of the current tool.
	private void DrawToolInfo()
	{
		if (mTool == e2dTerrainTool.NONE)
		{
			EditorGUILayout.BeginVertical(e2dStyles.HelpBox);
			GUILayout.Label(e2dStrings.INFO_NO_TOOL_SELECTED, e2dStyles.InfoText);
			EditorGUILayout.EndVertical();
			return;
		}

		EditorGUILayout.BeginVertical(e2dStyles.HelpBox);
		GUILayout.Label(e2dStrings.EDITOR_TOOLS[(int)mTool], e2dStyles.InfoHeadline);
		GUILayout.Label(e2dStrings.EDITOR_TOOL_DESCRIPTIONS[(int)mTool], e2dStyles.InfoText);
		EditorGUILayout.EndVertical();
	}
	
	/// Displays an error message in the inspector.
	private void DrawToolError(string message)
	{
		EditorGUILayout.BeginVertical(e2dStyles.HelpBox);
		GUILayout.Label(message, e2dStyles.ErrorText);
		EditorGUILayout.EndVertical();
	}

#endregion

}
