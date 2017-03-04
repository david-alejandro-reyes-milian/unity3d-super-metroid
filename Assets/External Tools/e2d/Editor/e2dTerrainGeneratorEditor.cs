/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// Tools to generate different parts of the terrain.
public enum e2dGeneratorTool { NONE=-1, CURVE, SMOOTH, TEXTURE, GRASS }

/// Editor for the generator of the terrain data. It displays GUI elements in the inspector view and handles their input.
/// Note that it depends on the e2dTerrain editors as they prepare the scene controls and handle the mouse cursor.
[CustomEditor(typeof(e2dTerrainGenerator))]
public class e2dTerrainGeneratorEditor : Editor
{
	/// Currently selected tool.
	private static e2dGeneratorTool mTool = e2dGeneratorTool.NONE;
	/// Currently selected tool for generating curve.
	private static e2dGeneratorCurveMethod mCurveTool = e2dGeneratorCurveMethod.PERLIN;
	/// Current preset of the generator parameters.
	private int mPreset;
	/// List of heights created at the last time the generator was executed.
	private float[] mDebugHeightmap;
	/// True if the user defined peaks array is visible.
	private bool mPeaksUnfolded;
	/// Textures of different Voronoi generator peak types.
	private GUIContent[] mPeakTypeTextures;


#region Properties

	/// Main object of the generator itself.
	private e2dTerrainGenerator Generator { get { return (e2dTerrainGenerator)this.target; } }

	/// Main object of the terrain.
	private e2dTerrain Terrain { get { return ((e2dTerrainGenerator)this.target).Terrain; } }

	/// Current tool used for generation.
	public e2dGeneratorTool Tool { 
		get { return mTool; }
		set { mTool = value; }
	}

	/// Returns the terrain editor instance if it exists.
	public e2dTerrainEditor TerrainEditor { get { return (e2dTerrainEditor)Terrain.EditorReference; } }

	/// Returns the terrain scene editor instance if it exists.
	public e2dTerrainSceneEditor TerrainSceneEditor { get { return TerrainEditor.SceneEditor; } }

#endregion


	/// Inits the generator.
	public e2dTerrainGeneratorEditor()
	{
		mPreset = -1;
		mPeaksUnfolded = false;
		mPeakTypeTextures = new GUIContent[3];
		for (int i=0; i<mPeakTypeTextures.Length; i++)
		{
			Texture2D tex = (Texture2D)Resources.Load("voronoipeak" + (i + 1), typeof(Texture2D));
			mPeakTypeTextures[i] = new GUIContent(tex);
		}
	}


#region Inspector

	/// Called when the inspector GUI is to be drawn and handled.
	public override void OnInspectorGUI()
	{
		Generator.EditorReference = this;

		e2dStyles.Init();

		EditorGUIUtility.LookLikeControls();
		EditorGUILayout.Separator();


		// checks
		if (!e2dStyles.Inited)
		{
			EditorGUILayout.BeginVertical("HelpBox");
			if (e2dStyles.ErrorText != null) GUILayout.Label(e2dStrings.ERROR_CANT_FIND_ASSETS, e2dStyles.ErrorText);
			else GUILayout.Label(e2dStrings.ERROR_CANT_FIND_ASSETS);
			EditorGUILayout.EndVertical();
			return;
		}
		if (e2dEditorUtils.IsAnySceneToolSelected())
		{
			// the user selected some other tools
			mTool = e2dGeneratorTool.NONE;
		}


		// display toolbox
		GUI.changed = false;
		EditorGUILayout.BeginHorizontal();
		string[] toolOptions = e2dStrings.GENERATOR_TOOLS;
		mTool = (e2dGeneratorTool)GUILayout.Toolbar((int)mTool, toolOptions, GUILayout.MinWidth(0));
		EditorGUILayout.EndHorizontal();
		if (GUI.changed)
		{
			if (mTool == e2dGeneratorTool.CURVE)
			{
				mDebugHeightmap = null;
			}
			if (mTool != e2dGeneratorTool.NONE)
			{
				e2dEditorUtils.DeselectSceneTools();
				if (TerrainEditor) TerrainEditor.Tool = e2dTerrainTool.NONE;
				SceneView.RepaintAll();
			}
		}

		// tools
		switch (mTool)
		{
			case e2dGeneratorTool.CURVE:
				DrawTargetAreaTool();
				DrawCurveTool();
				break;
			case e2dGeneratorTool.SMOOTH:
				DrawTargetAreaTool();
				DrawSmoothTool();
				break;
			case e2dGeneratorTool.TEXTURE:
				DrawTargetAreaTool();
				DrawTextureTool();
				break;
			case e2dGeneratorTool.GRASS:
				DrawTargetAreaTool();
				DrawGrassTool();
				break;
		}


		EditorGUIUtility.LookLikeInspector();
	}

	/// Target area settings drawn in the inspector.
	void DrawTargetAreaTool()
	{
		GUI.changed = false;
		GUILayout.Label(e2dStrings.LABEL_TARGET_AREA, e2dStyles.Header);
		Generator.TargetPosition = EditorGUILayout.Vector2Field(e2dStrings.LABEL_TARGET_POSITION, Generator.TargetPosition);
		Generator.TargetSize = EditorGUILayout.Vector2Field(e2dStrings.LABEL_TARGET_SIZE, Generator.TargetSize);
		Generator.TargetAngle = EditorGUILayout.Slider(e2dStrings.LABEL_TARGET_ANGLE, Generator.TargetAngle, 0, 360);
		if (GUI.changed)
		{
			Generator.FixTargetArea();
			EditorUtility.SetDirty(Generator);
		}
	}

	/// Tool for generating the terrain curve.
	private void DrawCurveTool()
	{
		GUILayout.Label(e2dStrings.LABEL_TOOL_SETTINGS, e2dStyles.Header);

		// global settings
		Generator.FullRebuild = EditorGUILayout.Toggle(e2dStrings.LABEL_REBUILD_ALL, Generator.FullRebuild);
		Generator.NodeStepSize = Mathf.Max(e2dConstants.GENERATOR_STEP_NODE_SIZE_MIN, EditorGUILayout.FloatField(e2dStrings.LABEL_STEP_SIZE, Generator.NodeStepSize));

		// generator methods
		GUI.changed = false;
		string[] toolOptions = e2dStrings.GENERATOR_CURVE_TOOLS;
		mCurveTool = (e2dGeneratorCurveMethod)GUILayout.Toolbar((int)mCurveTool, toolOptions, e2dStyles.TabButton, GUILayout.ExpandWidth(false));
		if (GUI.changed)
		{
			mPreset = -1;
			SceneView.RepaintAll();
		}

		// tab
		EditorGUILayout.BeginVertical(e2dStyles.TabBox);

		// blending
		if (mCurveTool != e2dGeneratorCurveMethod.PEAKS)
		{
			Generator.CurveBlendingWeights[(int)mCurveTool] = EditorGUILayout.Slider(e2dStrings.LABEL_BLEND_WEIGHT, Generator.CurveBlendingWeights[(int)mCurveTool], 0, 1);
		}

		// presets
		if (mCurveTool != e2dGeneratorCurveMethod.PEAKS)
		{
			GUI.changed = false;
			e2dPreset[] presets = e2dPresets.GetCurvePresets(mCurveTool);
			string[] presetNames = new string[presets.Length + 1];
			presetNames[0] = e2dStrings.LABEL_NO_PRESET;
			for (int i = 0; i < presets.Length; i++) presetNames[i + 1] = presets[i].name;
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel(e2dStrings.LABEL_PRESET);
			mPreset = EditorGUILayout.Popup(mPreset + 1, presetNames) - 1;
			EditorGUILayout.EndHorizontal();
			if (GUI.changed && mPreset >= 0)
			{
				presets[mPreset].UpdateValues(Generator);
			}
		}

		// parameters
		DrawCurveParameters();

		// tab end
		EditorGUILayout.EndVertical();

		// generate button
		if (GUILayout.Button(e2dStrings.BUTTON_GENERATE, GUILayout.ExpandWidth(false)))
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_GENERATE);
			Generator.GenerateCurve(ref mDebugHeightmap);
			EditorUtility.SetDirty(Generator);
		}
	}

	/// Draws the settings of parameters of the curve generator.
	private void DrawCurveParameters()
	{
		GUI.changed = false;

		switch (mCurveTool)
		{
			case e2dGeneratorCurveMethod.PERLIN:
				Generator.Perlin.frequencyPerUnit = EditorGUILayout.Slider(e2dStrings.LABEL_FREQUENCY_PER_UNIT, Generator.Perlin.frequencyPerUnit, e2dConstants.PERLIN_FREQUENCY_MIN, e2dConstants.PERLIN_FREQUENCY_MAX);
				Generator.Perlin.octaves = EditorGUILayout.IntSlider(e2dStrings.LABEL_PERLIN_OCTAVES, Generator.Perlin.octaves, e2dConstants.PERLIN_OCTAVES_MIN, e2dConstants.PERLIN_OCTAVES_MAX);
				Generator.Perlin.persistence = EditorGUILayout.Slider(e2dStrings.LABEL_PERLIN_PERSISTENCE, Generator.Perlin.persistence, e2dConstants.PERLIN_PERSISTENCE_MIN, e2dConstants.PERLIN_PERSISTENCE_MAX);
				break;
			case e2dGeneratorCurveMethod.VORONOI:
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.PrefixLabel(e2dStrings.LABEL_PEAK_TYPE);
				Generator.Voronoi.peakType = (e2dVoronoiPeakType)GUILayout.Toolbar((int)Generator.Voronoi.peakType, mPeakTypeTextures, GUILayout.ExpandWidth(false));
				EditorGUILayout.EndHorizontal();
				Generator.Voronoi.frequencyPerUnit = EditorGUILayout.Slider(e2dStrings.LABEL_FREQUENCY_PER_UNIT, Generator.Voronoi.frequencyPerUnit, e2dConstants.VORONOI_FREQUENCY_MIN, e2dConstants.VORONOI_FREQUENCY_MAX);
				Generator.Voronoi.peakRatio = EditorGUILayout.Slider(e2dStrings.LABEL_PEAK_RATIO, Generator.Voronoi.peakRatio, e2dConstants.VORONOI_PEAK_RATIO_MIN, e2dConstants.VORONOI_PEAK_RATIO_MAX);
				Generator.Voronoi.peakWidth = EditorGUILayout.Slider(e2dStrings.LABEL_PEAK_WIDTH, Generator.Voronoi.peakWidth, e2dConstants.VORONOI_PEAK_WIDTH_MIN, e2dConstants.VORONOI_PEAK_RATIO_MAX);
				Generator.Voronoi.usePeaks = EditorGUILayout.Toggle(e2dStrings.LABEL_USE_PEAKS, Generator.Voronoi.usePeaks);
				break;
			case e2dGeneratorCurveMethod.MIDPOINT:
				Generator.Midpoint.frequencyPerUnit = EditorGUILayout.Slider(e2dStrings.LABEL_FREQUENCY_PER_UNIT, Generator.Midpoint.frequencyPerUnit, e2dConstants.MIDPOINT_FREQUENCY_MIN, e2dConstants.MIDPOINT_FREQUENCY_MAX);
				Generator.Midpoint.roughness = EditorGUILayout.Slider(e2dStrings.LABEL_MIDPOINT_ROUGHNESS, Generator.Midpoint.roughness, e2dConstants.MIDPOINT_ROUGHNESS_MIN, e2dConstants.MIDPOINT_ROUGHNESS_MAX);
				Generator.Midpoint.usePeaks = EditorGUILayout.Toggle(e2dStrings.LABEL_USE_PEAKS, Generator.Midpoint.usePeaks);
				break;
			case e2dGeneratorCurveMethod.WALK:
				Generator.Walk.frequencyPerUnit = EditorGUILayout.Slider(e2dStrings.LABEL_FREQUENCY_PER_UNIT, Generator.Walk.frequencyPerUnit, e2dConstants.WALK_FREQUENCY_MIN, e2dConstants.WALK_FREQUENCY_MAX);
				Generator.Walk.angleChangePerUnit = EditorGUILayout.Slider(e2dStrings.LABEL_WALK_CHANGE_PER_UNIT, Generator.Walk.angleChangePerUnit, e2dConstants.WALK_ANGLE_CHANGE_MIN, e2dConstants.WALK_ANGLE_CHANGE_MAX);
				Generator.Walk.cohesionPerUnit = EditorGUILayout.Slider(e2dStrings.LABEL_WALK_COHESION, Generator.Walk.cohesionPerUnit, e2dConstants.WALK_COHESION_MIN, e2dConstants.WALK_COHESION_MAX);
				break;
			case e2dGeneratorCurveMethod.PEAKS:
				DrawPeakParameters();
				break;
		}

		if (GUI.changed)
		{
			mPreset = -1;
			EditorUtility.SetDirty(Generator);
		}
	}

	/// Draws the inspector for the array of peaks.
	private void DrawPeakParameters()
	{
		EditorGUILayout.BeginVertical(e2dStyles.HelpBox);
		GUILayout.Label(e2dStrings.INFO_PEAKS, e2dStyles.InfoText);
		EditorGUILayout.EndVertical();

		mPeaksUnfolded = EditorGUILayout.Foldout(mPeaksUnfolded, e2dStrings.LABEL_PEAKS + " (" + Generator.Peaks.Count + ")");
		if (mPeaksUnfolded)
		{
			GUI.changed = false;
			for (int i=0; i<Generator.Peaks.Count; i++)
			{
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				GUILayout.Label("X");
				Generator.Peaks[i].position.x = EditorGUILayout.FloatField(Generator.Peaks[i].position.x);
				EditorGUILayout.Space();
				GUILayout.Label("Y");
				Generator.Peaks[i].position.y = EditorGUILayout.FloatField(Generator.Peaks[i].position.y);
				EditorGUILayout.EndHorizontal();
			}
			if (GUI.changed)
			{
				Generator.FixTargetArea();
			}
			EditorGUILayout.Separator();
		}
	}

	/// Tool for smoothing the terrain curve.
	private void DrawSmoothTool()
	{
		GUILayout.Label(e2dStrings.LABEL_TOOL_SETTINGS, e2dStyles.Header);

		// parameters
		Generator.SmoothIterations = EditorGUILayout.IntSlider(e2dStrings.LABEL_SMOOTH_ITERATIONS, Generator.SmoothIterations, e2dConstants.SMOOTH_ITERATIONS_MIN, e2dConstants.SMOOTH_ITERATIONS_MAX);

		EditorGUILayout.Separator();

		// smooth button
		if (GUILayout.Button(e2dStrings.BUTTON_SMOOTH, GUILayout.ExpandWidth(false)))
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_SMOOTH);
			Generator.SmoothCurve();
			EditorUtility.SetDirty(Generator);
		}
	}

	/// Tool for texturing.
	private void DrawTextureTool()
	{
		GUILayout.Label(e2dStrings.LABEL_TOOL_SETTINGS, e2dStyles.Header);

		// cliff
		EditorGUILayout.BeginHorizontal();

		// params
		EditorGUILayout.BeginVertical();
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(e2dStrings.LABEL_CLIFF_TEXTURE);
		Generator.CliffTextureIndex = Mathf.RoundToInt(GUILayout.HorizontalSlider(Generator.CliffTextureIndex, 0, Terrain.CurveTextures.Count - 1));
		Generator.CliffTextureIndex = Mathf.Clamp(Generator.CliffTextureIndex, 0, Terrain.CurveTextures.Count - 1);
		EditorGUILayout.EndHorizontal();
		Generator.CliffStartAngle = EditorGUILayout.Slider(e2dStrings.LABEL_CLIFF_SLOPE_RANGE, Generator.CliffStartAngle, 0, 90);
		EditorGUILayout.EndVertical();

		// texture preview
		Rect previewRect = GUILayoutUtility.GetAspectRect(1, GUILayout.MinWidth(37), GUILayout.MaxWidth(37));
		EditorGUI.DrawPreviewTexture(previewRect, Terrain.CurveTextures[Generator.CliffTextureIndex].texture);
		GUILayout.Space(GUI.skin.button.margin.right);
	
		EditorGUILayout.EndHorizontal();
		

		// texture heights
		List<Texture> curveTextures = new List<Texture>(Terrain.CurveTextures.Count);
		foreach (e2dCurveTexture curveTexture in Terrain.CurveTextures) curveTextures.Add(curveTexture.texture);
		while (Generator.TextureHeights.Count > Terrain.CurveTextures.Count)
		{
			Generator.TextureHeights.RemoveAt(Generator.TextureHeights.Count - 1);
		}
		while (Generator.TextureHeights.Count < Terrain.CurveTextures.Count)
		{
			Generator.TextureHeights.Add((float)Generator.TextureHeights.Count / (float)(Terrain.CurveTextures.Count - 1));
		}
		e2dEditorUtils.VerticalMultiSlider(e2dStrings.LABEL_TEXTURE_HEIGHTS, ref Generator.TextureHeights, curveTextures, -0.1f, 1, 0, e2dStrings.LABEL_TEXTURE_GROUND, 200, true);


		EditorGUILayout.Separator();

		// generate button
		if (GUILayout.Button(e2dStrings.BUTTON_TEXTURE, GUILayout.ExpandWidth(false)))
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_TEXTURE);
			Generator.TextureTerrain();
			EditorUtility.SetDirty(Generator);
		}
	}

	/// Tool for generating grass.
	private void DrawGrassTool()
	{
		GUILayout.Label(e2dStrings.LABEL_TOOL_SETTINGS, e2dStyles.Header);

		Generator.GrassHeightRange.x = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_HEIGHT_MIN, Generator.GrassHeightRange.x, 0, 1);
		Generator.GrassHeightRange.y = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_HEIGHT_MAX, Generator.GrassHeightRange.y, 0, 1);
		Generator.GrassHeightRange.y = Mathf.Max(Generator.GrassHeightRange.x, Generator.GrassHeightRange.y);
		Generator.GrassStopAngle = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_STOP_ANGLE, Generator.GrassStopAngle, 0, 90);
		Generator.GrassDensity = EditorGUILayout.Slider(e2dStrings.LABEL_GRASS_DENSITY, Generator.GrassDensity, 0, 1);

		// generate button
		if (GUILayout.Button(e2dStrings.BUTTON_GENERATE, GUILayout.ExpandWidth(false)))
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_GENERATE_GRASS);
			Generator.GenerateGrass();
			EditorUtility.SetDirty(Generator);
		}
	}

#endregion


#region Scene

	/// Called when the scene window is to be redrawn.
	void OnSceneGUI()
	{
		Generator.EditorReference = this;
		if (!Terrain.enabled) Terrain.enabled = true;
		if (!TerrainEditor) return;

		// set up the util for drawing handles and other scene stuff
		e2dEditorUtils.transform = Generator.transform;

		// tools
		switch (mTool)
		{
			case e2dGeneratorTool.CURVE:
				DrawTargetArea(mCurveTool != e2dGeneratorCurveMethod.PEAKS);
				bool showPeaks = mCurveTool != e2dGeneratorCurveMethod.PERLIN;
				showPeaks = showPeaks && mCurveTool != e2dGeneratorCurveMethod.WALK;
				showPeaks = showPeaks && (mCurveTool != e2dGeneratorCurveMethod.VORONOI || Generator.Voronoi.usePeaks);
				showPeaks = showPeaks && (mCurveTool != e2dGeneratorCurveMethod.MIDPOINT || Generator.Midpoint.usePeaks);
				if (showPeaks)
				{
					DrawPeaks(mCurveTool == e2dGeneratorCurveMethod.PEAKS);
				}
				break;
			case e2dGeneratorTool.SMOOTH:
				DrawTargetArea(true);
				break;
			case e2dGeneratorTool.TEXTURE:
				DrawTargetArea(true);
				break;
			case e2dGeneratorTool.GRASS:
				DrawTargetArea(true);
				break;
		}

		// draw the curve of the generated terrain for debug
		if (e2dUtils.DEBUG_GENERATOR_CURVE && mTool == e2dGeneratorTool.CURVE && mDebugHeightmap != null && mDebugHeightmap.Length > 0)
		{
			Rect area = Generator.GetTargetAreaLocalBox();
			Handles.color = new Color(1, 0, 0, 1);
			for (int i = 1; i < mDebugHeightmap.Length; i++)
			{
				float xa = (i - 1) * area.width / (mDebugHeightmap.Length - 1) + area.xMin;
				Vector2 a = Generator.TransformPointFromTargetArea(new Vector2(xa, mDebugHeightmap[i - 1] - 0.5f * area.height));
				float xb = (i) * area.width / (mDebugHeightmap.Length - 1) + area.xMin;
				Vector2 b = Generator.TransformPointFromTargetArea(new Vector2(xb, mDebugHeightmap[i] - 0.5f * area.height));
				e2dEditorUtils.DrawLine(a, b);
			}
		}
	}

	/// Draws the rectangle area where the terrain will be generated. Handles are drawn to allow
	/// the user to adjust the area.
	private void DrawTargetArea(bool drawHandles)
	{
		if (e2dUtils.DEBUG_NO_TARGET_AREA) return;

		Vector2[] points = Generator.GetTargetAreaBoundary();

		// draw the rectangle
		Handles.color = e2dConstants.COLOR_TARGET_AREA;
		float lineWidth = e2dConstants.SCALE_TARGET_AREA_LINE_WIDTH * e2dEditorUtils.GetHandleSize(Generator.TargetPosition);
		e2dEditorUtils.DrawLine(points[0], points[1], lineWidth);
		e2dEditorUtils.DrawLine(points[1], points[2], lineWidth);
		e2dEditorUtils.DrawLine(points[2], points[3], lineWidth);
		e2dEditorUtils.DrawLine(points[3], points[0], lineWidth);

		// undo
		Undo.SetSnapshotTarget(Generator, e2dStrings.UNDO_GENERATOR_AREA);

		// rotation
		if (drawHandles)
		{
			Generator.TargetAngle = e2dEditorUtils.RotationHandle2d(Generator.TargetAngle, Generator.TargetPosition);
		}

		// corner controllers
		if (drawHandles)
		{
			Vector3 xDirection = (points[3] - points[0]).normalized;
			Vector3 yDirection = (points[1] - points[0]).normalized;
			for (int i = 0; i < 4; i++)
			{
				GUI.changed = false;
				points[i] = e2dEditorUtils.PositionHandle2d(points[i], false);
				if (GUI.changed)
				{
					Generator.TargetSize.x = 2.0f * Mathf.Abs(Vector3.Dot(xDirection, points[i]) - Vector3.Dot(xDirection, Generator.TargetPosition));
					Generator.TargetSize.y = 2.0f * Mathf.Abs(Vector3.Dot(yDirection, points[i]) - Vector3.Dot(yDirection, Generator.TargetPosition));
				}
			}
		}

		// position
		if (drawHandles)
		{
			Generator.TargetPosition = e2dEditorUtils.PositionHandle2d(Generator.TargetPosition);
		}

		Generator.FixTargetArea();
	}

	/// Draws all the user defined peaks with handles to move them.
	private void DrawPeaks(bool drawHandles)
	{
		bool delete = TerrainSceneEditor.ShiftPressed();

		// draw the points
		if (!drawHandles || delete)
		{
			Handles.color = e2dConstants.COLOR_GENERATOR_PEAKS;
			foreach (e2dGeneratorPeak peak in Generator.Peaks)
			{
				e2dEditorUtils.DrawSphere(Generator.TransformPointFromTargetArea(peak.position), e2dConstants.SCALE_GENERATOR_PEAKS * e2dEditorUtils.GetHandleSize(peak.position));
			}
		}

		if (!drawHandles) return;

		// handles to move the peaks
		if (!delete)
		{
			Undo.SetSnapshotTarget(Generator, e2dStrings.UNDO_EDIT_PEAKS);
			GUI.changed = false;
			for (int i = 0; i < Generator.Peaks.Count; i++)
			{
				Generator.Peaks[i].position = Generator.TransformPointIntoTargetArea(e2dEditorUtils.PositionHandle2d(Generator.TransformPointFromTargetArea(Generator.Peaks[i].position)));
			}
			if (GUI.changed)
			{
				EditorUtility.SetDirty(Generator);
			}
		}

		// cursor
		if (delete)
		{
			Handles.color = e2dConstants.COLOR_NODE_CURSOR;
			int pointIndex = GetPeakIndexToDelete();
			if (pointIndex == -1) return;
			Vector2 pos = Generator.TransformPointFromTargetArea(Generator.Peaks[pointIndex].position);
			e2dEditorUtils.DrawSphere(pos, e2dConstants.SCALE_NODES_CURSOR * e2dEditorUtils.GetHandleSize(pos));
		}
		else
		{
			if (TerrainSceneEditor.MouseDragged()) return;

			Handles.color = e2dConstants.COLOR_NODE_CURSOR;
			e2dEditorUtils.DrawSphere(TerrainSceneEditor.GetCursorPosition(), e2dConstants.SCALE_NODES_CURSOR * e2dEditorUtils.GetHandleSize(TerrainSceneEditor.GetCursorPosition()));
		}

		// this must come after any handles since we want them to have precedence in input handling
		if (TerrainSceneEditor.MouseClickedUp())
		{
			if (delete)
			{
				Undo.RegisterUndo(Generator, e2dStrings.UNDO_EDIT_PEAKS);
				Generator.Peaks.RemoveAt(GetPeakIndexToDelete());
			}
			else if (e2dUtils.PointInConvexPolygon(TerrainSceneEditor.GetCursorPosition(), Generator.GetTargetAreaBoundary()))
			{
				Undo.RegisterUndo(Generator, e2dStrings.UNDO_EDIT_PEAKS);
				Generator.Peaks.Add(new e2dGeneratorPeak(Generator.TransformPointIntoTargetArea(TerrainSceneEditor.GetCursorPosition())));
			}
			EditorUtility.SetDirty(Generator);
		}
	}

	/// Returns the index of the generator peak to be deleted. Returns -1 if there is none.
	private int GetPeakIndexToDelete()
	{
		int index = -1;
		float minDistSq = float.MaxValue;
		Vector2 cursorPos = TerrainSceneEditor.GetCursorPosition();

		for (int i=0; i<Generator.Peaks.Count; i++)
		{
			Vector2 peakPosition = Generator.TransformPointFromTargetArea(Generator.Peaks[i].position);
			float distSq = (peakPosition - cursorPos).sqrMagnitude;
			if (distSq < minDistSq)
			{
				index = i;
				minDistSq = distSq;
			}
		}

		return index;
	}

#endregion

}
