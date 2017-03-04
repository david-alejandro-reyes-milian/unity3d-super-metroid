/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using UnityEditor;
using System.Collections;
using System;

/// Scene editor of the terrain component. It is invoked from e2dTerrainEditor.
public class e2dTerrainSceneEditor
{
	/// Plane used for detecting the mouse cursor position.
	private static Plane sXyPlane;
	/// Inspector editor.
	private e2dTerrainEditor mInspector;
	/// Current world position of the cursor.
	private Vector3 mCursorPosition;
	/// Position of the cursor when a tool was activated.
	private Vector3 mInitCursorPosition;
	/// True if the mouse is being dragged.
	private bool mMouseDragged;
	// True if the mouse is being pressed down.
	private bool mMousePressed;
	/// True if shift is currently pressed.
	private bool mShiftPressed;
	/// True if control is currently pressed.
	private bool mControlPressed;
	/// True if both control and shift were pressed. Remains active until BOTH are released again.
	private bool mShiftAndControlPressed;
	/// Initial value of the brush size before a tool was activated.
	private int mInitBrushSize;
	/// Initial value of the brush angle before a tool was activated.
	private float mInitBrushAngle;
	/// Initial value of the brush opacity before a tool was activated.
	private int mInitBrushOpacity;
	/// Initial index of the selected texture before a tool was activated.
	private int mInitTextureIndex;
	/// Last position brush was applied to.
	private Vector3 mLastAppliedPosition;
	/// If true the run-time data of the terrain will be rebuilt when the mouse is released.
	private bool mRebuildDataOnMouseUp;

	/// Size of the current brush in the world units.
	private static int sBrushSize = e2dConstants.INIT_BRUSH_SIZE;
	/// Defines how hard will be the brush applied.
	private static int sBrushOpacity = e2dConstants.INIT_BRUSH_OPACITY;
	/// Defines the rotation of the brush (if needed).
	private static float sBrushAngle = e2dConstants.INIT_BRUSH_ANGLE;


#region Properties
	/// The main terrain object we're editing.
	private e2dTerrain Terrain
	{
		get { return (e2dTerrain)mInspector.target; }
	}
	/// Inspector editor.
	private e2dTerrainEditor Inspector
	{
		get { return mInspector; }
	}
	/// Size of the current brush in the world units.
	public int BrushSize
	{
		get { return sBrushSize; }
		set { sBrushSize = Mathf.Max(value, e2dConstants.BRUSH_SIZE_MIN); }
	}
	/// Defines how hard will be the brush applied.
	public int BrushOpacity
	{
		get { return sBrushOpacity; }
		set { sBrushOpacity = Mathf.Max(value, e2dConstants.BRUSH_OPACITY_MIN); }
	}
	/// Defines the rotation of the brush (if needed).
	public float BrushAngle
	{
		get { return sBrushAngle; }
		set { sBrushAngle = Mathf.Repeat(value, 360); }
	}
#endregion


#region Scene

	/// Basic constructor.
	public e2dTerrainSceneEditor(e2dTerrainEditor inspector)
	{
		mInspector = inspector;
		mCursorPosition = Vector3.zero;
		mMouseDragged = false;
		mShiftPressed = false;
		mControlPressed = false;
		mShiftAndControlPressed = false;
		mRebuildDataOnMouseUp = false;
	}

	/// Called when the scene window is to be redrawn. Handles GUI drawing and input.
	public void OnSceneGUI()
	{
		if (!PrepareSceneView()) return;

		UpdateCursorPosition();
		UpdateInput();

		switch (Inspector.Tool)
		{
			case e2dTerrainTool.EDIT_NODES:
				ToolEditNodes();
				break;
			case e2dTerrainTool.ADJUST_HEIGHT:
				ToolAdjustHeight();
				break;
			case e2dTerrainTool.FILL_TEXTURE:
				DrawCurve();
				DrawBoundaryWithHandles();
				break;
			case e2dTerrainTool.CURVE_TEXTURE:
				ToolCurveTexture();
				break;
			case e2dTerrainTool.GRASS:
				ToolGrass();
				break;
			case e2dTerrainTool.NONE:
				break;
		}
	}

	/// Makes the Scene view ready for the GUI of the editor. Returns false if something went wrong and the GUI
	/// can't be drawn.
	private bool PrepareSceneView()
	{
		Terrain.EditorReference = Inspector;

		if (!Terrain.enabled) return false;

		// disable wireframe of our meshes
		EditorUtility.SetSelectedWireframeHidden(Terrain.CurveMesh.renderer, !e2dUtils.DEBUG_SHOW_WIREFRAME);
		EditorUtility.SetSelectedWireframeHidden(Terrain.GrassMesh.renderer, !e2dUtils.DEBUG_SHOW_WIREFRAME);
		EditorUtility.SetSelectedWireframeHidden(Terrain.FillMesh.renderer, !e2dUtils.DEBUG_SHOW_WIREFRAME);
		// TODO: hide the collider wireframe
		// apparently this is not possible from scripts
		// http://answers.unity3d.com/questions/129870/hide-meshcollider-wireframe-when-selected.html
		//EditorUtility.SetSelectedWireframeHidden(Terrain.ColliderMesh.collider, !e2dUtils.DEBUG_SHOW_WIREFRAME);


		// checks
		if (e2dEditorUtils.IsAnySceneToolSelected() && Inspector.Tool != e2dTerrainTool.NONE) return false;
		if (!e2dStyles.Inited) return false;

		// to prevent mouse selecting in the Scene view
		HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

		// terrain error
		if (Terrain.TerrainCurve.Count > 0 && Terrain.CurveIntercrossing)
		{
			e2dEditorUtils.DrawErrorLabel(e2dStrings.ERROR_CURVE_IS_CROSSING);
		}

		// update mouse state
		if (Event.current.type == EventType.MouseDown) mMouseDragged = true;
		if (Event.current.type == EventType.MouseUp) mMouseDragged = false;

		// setup util functions
		e2dEditorUtils.transform = Terrain.transform;

		return true;
	}

#endregion


#region Tools

	/// Adjusting brush settings using mouse and keyboard modifiers.
	private void ToolAdjustBrushSize()
	{
		if (ShiftPressed() && !ShiftAndControlPressed())
		{
			float delta = (mCursorPosition - mInitCursorPosition).x;
			BrushSize = mInitBrushSize + Mathf.RoundToInt(e2dConstants.BRUSH_SIZE_INC_RATIO * delta / HandleUtility.GetHandleSize(mCursorPosition));
			mCursorPosition = mInitCursorPosition;
			Inspector.Repaint();
		}
		else
		{
			mInitBrushSize = BrushSize;
		}
	}

	/// Adjusting brush settings using mouse and keyboard modifiers.
	private void ToolAdjustBrushAngle()
	{
		if (ControlPressed() && !ShiftAndControlPressed())
		{
			float delta = (mCursorPosition - mInitCursorPosition).x;
			BrushAngle = mInitBrushAngle + Mathf.RoundToInt(e2dConstants.BRUSH_ANGLE_INC_RATIO * delta / HandleUtility.GetHandleSize(mCursorPosition));
			mCursorPosition = mInitCursorPosition;
			Inspector.Repaint();
		}
		else
		{
			mInitBrushAngle = BrushAngle;
		}
	}

	/// Adjusting brush settings using mouse and keyboard modifiers.
	private void ToolAdjustBrushTexture()
	{
		if (ControlPressed() && !ShiftAndControlPressed())
		{
			float delta = (mCursorPosition - mInitCursorPosition).x;
			Inspector.SelectedTexture = mInitTextureIndex + Mathf.RoundToInt(e2dConstants.BRUSH_TEXTURE_INC_RATIO * delta / HandleUtility.GetHandleSize(mCursorPosition));
			mCursorPosition = mInitCursorPosition;
			Inspector.Repaint();
		}
		else
		{
			mInitTextureIndex = Inspector.SelectedTexture;
		}
	}

	/// Adjusting brush settings using mouse and keyboard modifiers.
	private void ToolAdjustBrushOpacity()
	{
		if (ShiftAndControlPressed())
		{
			float delta = (mCursorPosition - mInitCursorPosition).x;
			BrushOpacity = mInitBrushOpacity + Mathf.RoundToInt(e2dConstants.BRUSH_OPACITY_INC_RATIO * delta / HandleUtility.GetHandleSize(mCursorPosition));
			mCursorPosition = mInitCursorPosition;
			Inspector.Repaint();
		}
		else
		{
			mInitBrushOpacity = BrushOpacity;
		}
	}

	/// Tool for editing surface curve nodes.
	private void ToolEditNodes()
	{
		DrawCurve();
		DrawNodesCursor();
		DrawBoundaryWithoutHandles();

		if (!ShiftPressed())
		{
			DrawCurveHandles();
		}

		// this must come after any handles since we want them to have precedence in input handling
		if (MouseClickedUp())
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_EDIT_NODES);
			if (ShiftPressed())
			{
				DeleteCurvePoint(ControlPressed());
			}
			else
			{
				AddCurvePoint(ControlPressed());
			}
			EditorUtility.SetDirty(Terrain);
		}
	}

	/// Tool for adjusting height.
	private void ToolAdjustHeight()
	{
		ToolAdjustBrushSize();
		ToolAdjustBrushAngle();
		ToolAdjustBrushOpacity();

		DrawCurve();
		DrawCurveBrush();
		DrawBrush();

		if (!ShiftPressed() && !ControlPressed() && MousePressed())
		{
			float delta = (mCursorPosition - mLastAppliedPosition).magnitude;
			if (MouseClickedDown() || delta > e2dConstants.BRUSH_APPLY_STEP_RATIO * HandleUtility.GetHandleSize(mCursorPosition))
			{
				Undo.RegisterUndo(Terrain, e2dStrings.UNDO_HEIGHT_BRUSH);
				ApplyBrush();
				mLastAppliedPosition = mCursorPosition;
				EditorUtility.SetDirty(Terrain);
			}
		}
	}

	/// Tool for painting curve textures.
	private void ToolCurveTexture()
	{
		ToolAdjustBrushSize();
		ToolAdjustBrushTexture();

		DrawCurveBrush();
		DrawBrush();

		if (!ShiftPressed() && !ControlPressed() && MousePressed())
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_CURVE_TEXTURE_BRUSH);
			ApplyBrush();
			EditorUtility.SetDirty(Terrain);
		}
	}

	/// Tool for painting grass.
	private void ToolGrass()
	{
		ToolAdjustBrushSize();
		ToolAdjustBrushOpacity();

		DrawCurveBrush();

		if (!ShiftPressed() && !ShiftAndControlPressed() && MousePressed())
		{
			Undo.RegisterUndo(Terrain, e2dStrings.UNDO_GRASS_TEXTURE);
			ApplyBrush();
			EditorUtility.SetDirty(Terrain);
		}
	}

#endregion


#region Controls

	/// Updates some internal input related variables.
	private void UpdateInput()
	{
		if (Event.current.shift && !mShiftPressed)
		{
			mInitCursorPosition = mCursorPosition;
		}
		mShiftPressed = Event.current.shift;

		if (Event.current.control && !mControlPressed)
		{
			mInitCursorPosition = mCursorPosition;
		}
		mControlPressed = Event.current.control;

		if (mShiftPressed && mControlPressed)
		{
			mShiftAndControlPressed = true;
		}
		else if (!mShiftPressed && !mControlPressed)
		{
			mShiftAndControlPressed = false;
		}

		// this will update the pressed mouse state
		MouseClickedDown();
		if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
		{
			mMousePressed = false;

			if (mRebuildDataOnMouseUp)
			{
				mRebuildDataOnMouseUp = false;
				Terrain.CurveMesh.UpdateControlTextures();
				Terrain.RebuildAllMeshes();
			}
		}
	}

	/// True if the mouse is currently dragged.
	public bool MouseDragged()
	{
		return mMouseDragged;
	}

	/// True if the mouse was clicked this frame and not yet released.
	public bool MouseClickedDown()
	{
		// left button up?
		if (Event.current.type != EventType.MouseDown || Event.current.button != 0) return false;

		// check if we're not playing with gizmos
		Vector2 mousePos = Event.current.mousePosition;
		if (mousePos.x >= Screen.width - e2dConstants.SCENE_GIZMOS_SIZE && mousePos.y <= e2dConstants.SCENE_GIZMOS_SIZE) return false;

		mMousePressed = true;

		return true;
	}

	/// True if the mouse was clicked and released this frame.
	public bool MouseClickedUp()
	{
		// left button up?
		if (Event.current.type != EventType.MouseUp || Event.current.button != 0) return false;

		// check if we're not playing with gizmos
		Vector2 mousePos = Event.current.mousePosition;
		if (mousePos.x >= Screen.width - e2dConstants.SCENE_GIZMOS_SIZE && mousePos.y <= e2dConstants.SCENE_GIZMOS_SIZE) return false;

		return true;
	}

	/// True if the mouse is pressed down.
	public bool MousePressed()
	{
		return mMousePressed;
	}

	/// True if control is currently pressed.
	public bool ControlPressed()
	{
		return Event.current.control;
	}

	/// True if shift is currently pressed.
	public bool ShiftPressed()
	{
		return Event.current.shift;
	}

	/// True if both shift and control are pressed.
	public bool ShiftAndControlPressed()
	{
		return mShiftAndControlPressed;
	}

	/// Updates the position on the cursor in the local space of the terrain based on the screen space cursor position.
	private void UpdateCursorPosition()
	{
		if (sXyPlane.normal == Vector3.zero) sXyPlane = new Plane(Vector3.forward, 0);

		// cast a ray into the plane with the terrain
		Vector2 screenCursorPos = Event.current.mousePosition;
		Ray cursorRay = HandleUtility.GUIPointToWorldRay(screenCursorPos);
		// Note: The origin is not transformed as a point because we don't want scale to be taken into account.
		cursorRay.origin = Terrain.transform.InverseTransformDirection(cursorRay.origin - Terrain.transform.position);
		cursorRay.direction = Terrain.transform.InverseTransformDirection(cursorRay.direction);
		float enterDistance;
		if (sXyPlane.Raycast(cursorRay, out enterDistance))
		{
			// we need to scale the intersection now
			Matrix4x4 scaleMatrix = Matrix4x4.Inverse(Matrix4x4.Scale(Terrain.transform.lossyScale));
			mCursorPosition = scaleMatrix.MultiplyPoint(cursorRay.GetPoint(enterDistance));
		}

		// draw the cursor for debug
		if (e2dUtils.DEBUG_CURSOR_INFO)
		{
			Vector3 pos = mCursorPosition;
			float moveSize = 0.25f * e2dEditorUtils.GetHandleSize(mCursorPosition);
			pos.y += moveSize;
			e2dEditorUtils.DrawLabel(pos, mCursorPosition.ToString(), e2dStyles.SceneLabel);
		}
	}

	/// Returns the cursor position in local object coordinates.
	public Vector3 GetCursorPosition()
	{
		return mCursorPosition;
	}

#endregion


#region Curve

	/// Draws the cursor position for the node tool.
	private void DrawNodesCursor()
	{
		if (ShiftPressed()) // delete
		{
			Handles.color = e2dConstants.COLOR_CURVE_NORMAL;
			for (int i = 0; i < Terrain.TerrainCurve.Count; i++)
			{
				if (Terrain.CurveClosed && i == Terrain.TerrainCurve.Count - 1) continue;

				e2dEditorUtils.DrawSphere(Terrain.TerrainCurve[i].position, e2dConstants.SCALE_NODES_CURVE_SPHERE * e2dEditorUtils.GetHandleSize(Terrain.TerrainCurve[i].position));
			}

			Handles.color = e2dConstants.COLOR_NODE_CURSOR;
			int pointIndex = GetCurvePointIndexToDelete();
			if (pointIndex == -1) return;
			Vector2 pos = Terrain.TerrainCurve[pointIndex].position;
			e2dEditorUtils.DrawSphere(pos, e2dConstants.SCALE_NODES_CURSOR * e2dEditorUtils.GetHandleSize(pos));
		}
		else // add new
		{
			if (mMouseDragged) return;

			Handles.color = e2dConstants.COLOR_CURVE_NORMAL;

			int pointIndex = GetCurvePointIndexWhereToAddNew(ControlPressed());
			if (pointIndex > 0)
			{
				e2dEditorUtils.DrawLine(mCursorPosition, Terrain.TerrainCurve[pointIndex - 1].position);
			}
			if (pointIndex < Terrain.TerrainCurve.Count)
			{
				e2dEditorUtils.DrawLine(mCursorPosition, Terrain.TerrainCurve[pointIndex].position);
			}

			Handles.color = e2dConstants.COLOR_NODE_CURSOR;
			e2dEditorUtils.DrawSphere(mCursorPosition, e2dConstants.SCALE_NODES_CURSOR * e2dEditorUtils.GetHandleSize(mCursorPosition));
		}
	}

	/// Draws the curve polygonal chain.
	private void DrawCurve()
	{
		if (Terrain.TerrainCurve.Count < 2) return;

		Handles.color = new Color(e2dConstants.COLOR_CURVE_NORMAL.r, e2dConstants.COLOR_CURVE_NORMAL.g, e2dConstants.COLOR_CURVE_NORMAL.b, 1);

		// curve outline
		Vector2 lastPoint = Vector2.zero;
		bool first = true;
		foreach (e2dCurveNode node in Terrain.TerrainCurve)
		{
			if (first)
			{
				first = false;
			}
			else
			{
				e2dEditorUtils.DrawLine(lastPoint, node.position);
			}

			lastPoint = node.position;
		}


		// node values
		if (e2dUtils.DEBUG_NODE_VALUES)
		{
			foreach (e2dCurveNode node in Terrain.TerrainCurve)
			{
				Vector2 pos = node.position;
				float moveSize = 0.15f * e2dEditorUtils.GetHandleSize(pos);
				pos.x += moveSize;
				pos.y += 2.5f * moveSize;
				e2dEditorUtils.DrawLabel(pos, node.position.ToString(), e2dStyles.SceneLabel);
			}
		}


		// stripe points
		if (e2dUtils.DEBUG_STRIPE_POINTS)
		{
			Vector2 lastVertex = Vector2.zero;
			foreach (Vector3 stripeVertex in Terrain.CurveMesh.StripeVertices)
			{
				Vector2 vertex = stripeVertex;
				if (lastVertex != Vector2.zero)
				{
					e2dEditorUtils.DrawLine(lastVertex, vertex);
				}
				e2dEditorUtils.DrawSphere(vertex, 0.07f * e2dEditorUtils.GetHandleSize(vertex));
				lastVertex = vertex;
			}
		}
	}

	/// Draws the handles for moving individual nodes of the curve.
	private void DrawCurveHandles()
	{
		// this is here to make scene view camera movement faster while dragging the mouse
		if (MouseDragged() && Event.current.type == EventType.Layout) return;

		GUI.changed = false;

		Undo.SetSnapshotTarget(Terrain, e2dStrings.UNDO_MOVE_NODES);

		// compute the scale of handles
		// first we get the average distance between nodes
		float sum = 0;
		for (int i = 1; i < Terrain.TerrainCurve.Count; i++)
		{
			sum += (Terrain.transform.TransformPoint(Terrain.TerrainCurve[i].position) - Terrain.transform.TransformPoint(Terrain.TerrainCurve[i - 1].position)).magnitude;
		}
		float avgDistance = sum / (Terrain.TerrainCurve.Count - 1);
		float handleScale = e2dConstants.SCALE_NODE_HANDLES * avgDistance;
		handleScale = Mathf.Min(handleScale, e2dEditorUtils.GetHandleSize(Vector2.zero));

		// draw the handles
		for (int i = 0; i < Terrain.TerrainCurve.Count; i++)
		{
			if (Terrain.CurveClosed && i == Terrain.TerrainCurve.Count - 1) continue;

			Terrain.TerrainCurve[i].position = e2dEditorUtils.PositionHandle2dScaled(Terrain.TerrainCurve[i].position, handleScale, false);
		}

		// update data
		if (GUI.changed)
		{
			Terrain.FixCurve();
			Terrain.FixBoundary();
			mRebuildDataOnMouseUp = true;
			EditorUtility.SetDirty(Terrain);
		}
	}

	/// Returns the index of the curve point where the new point should be added. The position
	/// of the point is the current cursor position. If forceAddToEnd is true the nearest end
	/// of the curve will be selected.
	private int GetCurvePointIndexWhereToAddNew(bool forceAddToEnd)
	{
		if (Terrain.CurveClosed) forceAddToEnd = false;

		Vector2 cursor2d = mCursorPosition;

		if (Terrain.TerrainCurve.Count == 0)
		{
			return 0;
		}

		if (Terrain.TerrainCurve.Count == 1)
		{
			return 1;
		}

		if (forceAddToEnd)
		{
			// we're adding the point to one of the curve ends
			float sqrDist1 = (Terrain.TerrainCurve[0].position - cursor2d).sqrMagnitude;
			float sqrDist2 = (Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 1].position - cursor2d).sqrMagnitude;
			if (sqrDist1 < sqrDist2) return 0;
			else return Terrain.TerrainCurve.Count;
		}

		int minIndex = -1;
		float minDistSq = float.MaxValue;

		// start with the end point
		float endDistSq = (Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 1].position - cursor2d).sqrMagnitude;
		float endDot = Vector2.Dot(Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 2].position - Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 1].position, cursor2d - Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 1].position);
		if (!Terrain.CurveClosed && endDistSq < minDistSq && endDot <= 0)
		{
			minIndex = Terrain.TerrainCurve.Count;
			minDistSq = endDistSq;
		}

		// go through all the segments
		for (int i = Terrain.TerrainCurve.Count - 1; i > 0; i--)
		{
			Vector2 middle = 0.5f * (Terrain.TerrainCurve[i - 1].position + Terrain.TerrainCurve[i].position);
			float distSq = (middle - cursor2d).sqrMagnitude;
			if (distSq < minDistSq)
			{
				minIndex = i;
				minDistSq = distSq;
			}
		}

		// check the start point
		float startDistSq = (Terrain.TerrainCurve[0].position - cursor2d).sqrMagnitude;
		float startDot = Vector2.Dot(Terrain.TerrainCurve[1].position - Terrain.TerrainCurve[0].position, cursor2d - Terrain.TerrainCurve[0].position);
		if (!Terrain.CurveClosed && startDistSq < minDistSq && startDot <= 0)
		{
			minIndex = 0;
			minDistSq = startDistSq;
		}

		// if nothing was found add it to the end of the list
		if (minIndex == -1) minIndex = Terrain.TerrainCurve.Count;

		return minIndex;
	}

	/// Adds new point to the terrain curve. The position of the point is the current cursor position.
	/// If forceAddToEnd is true the new point will be added to the nearest end of the curve (either the first
	/// or the last node).
	private void AddCurvePoint(bool forceAddToEnd)
	{
		Terrain.AddPointOnCurve(GetCurvePointIndexWhereToAddNew(forceAddToEnd), mCursorPosition);
		Terrain.FixCurve();
		Terrain.FixBoundary();
		Terrain.RebuildAllMeshes();
	}

	/// Returns the index of the curve point to be deleted. It is the point nearest to the cursor position.
	private int GetCurvePointIndexToDelete()
	{
		// find the closes point to the cursor
		int index = -1;
		float curMinDistSq = float.MaxValue;
		Vector2 cursorPos = new Vector2(mCursorPosition.x, mCursorPosition.y);
		for (int i = 0; i < Terrain.TerrainCurve.Count; i++)
		{
			float distSq = (Terrain.TerrainCurve[i].position - cursorPos).sqrMagnitude;
			if (distSq < curMinDistSq)
			{
				index = i;
				curMinDistSq = distSq;
			}
		}

		return index;
	}

	/// Deletes the point on the curve nearest to the cursor position. If moveTheRest is true the rest of the nodes
	/// after the deleted one will be moved so that the first of them is at the position of the deleted node.
	private void DeleteCurvePoint(bool moveTheRest)
	{
		if (Terrain.CurveClosed) moveTheRest = false;

		Terrain.RemovePointOnCurve(GetCurvePointIndexToDelete(), moveTheRest);
		Terrain.FixCurve();
		Terrain.RebuildAllMeshes();
	}

#endregion


#region Boundary

	/// Draws the boundary definition of the terrain including handles for adjusting it.
	private void DrawBoundaryWithHandles() { DrawBoundaryRect(true); }

	/// Draws the boundary definition of the terrain without handles for adjusting it.
	private void DrawBoundaryWithoutHandles() { DrawBoundaryRect(false); }

	/// Draws the boundary definition of the terrain.
	private void DrawBoundaryRect(bool drawHandles)
	{
		Vector2 topleft = new Vector2(Terrain.TerrainBoundary.xMin, Terrain.TerrainBoundary.yMax);
		Vector2 topright = new Vector2(Terrain.TerrainBoundary.xMax, Terrain.TerrainBoundary.yMax);
		Vector2 bottomleft = new Vector2(Terrain.TerrainBoundary.xMin, Terrain.TerrainBoundary.yMin);
		Vector2 bottomright = new Vector2(Terrain.TerrainBoundary.xMax, Terrain.TerrainBoundary.yMin);

		if (!IsBoundaryNull())
		{
			Handles.color = e2dConstants.COLOR_BOUNDARY_RECT;
			e2dEditorUtils.DrawLine(topleft, topright);
			e2dEditorUtils.DrawLine(topright, bottomright);
			e2dEditorUtils.DrawLine(bottomright, bottomleft);
			e2dEditorUtils.DrawLine(bottomleft, topleft);
		}

		if (drawHandles)
		{

			Undo.SetSnapshotTarget(Terrain, e2dStrings.UNDO_BOUNDARY);

			bool moved = false;
		
			GUI.changed = false;
			topleft = e2dEditorUtils.PositionHandle2d(topleft);
			if (GUI.changed)
			{
				moved = true;
				Terrain.TerrainBoundary.xMin = topleft.x;
				Terrain.TerrainBoundary.yMax = topleft.y;
			}

			GUI.changed = false;
			topright = e2dEditorUtils.PositionHandle2d(topright);
			if (GUI.changed)
			{
				moved = true;
				Terrain.TerrainBoundary.xMax = topright.x;
				Terrain.TerrainBoundary.yMax = topright.y;
			}

			GUI.changed = false;
			bottomleft = e2dEditorUtils.PositionHandle2d(bottomleft);
			if (GUI.changed)
			{
				moved = true;
				Terrain.TerrainBoundary.xMin = bottomleft.x;
				Terrain.TerrainBoundary.yMin = bottomleft.y;
			}

			GUI.changed = false;
			bottomright = e2dEditorUtils.PositionHandle2d(bottomright);
			if (GUI.changed)
			{
				moved = true;
				Terrain.TerrainBoundary.xMax = bottomright.x;
				Terrain.TerrainBoundary.yMin = bottomright.y;
			}
			
			if (moved)
			{
				Terrain.FixBoundary();
				Terrain.RebuildAllMeshes();
				EditorUtility.SetDirty(Terrain);
			}
		}

		// projections of the curve to the boundary
		if (e2dUtils.DEBUG_BOUNDARY_PROJECTIONS && Terrain.TerrainCurve.Count >= 2)
		{
			Vector2 startBorderPoint = Terrain.TerrainCurve[0].position;
			Vector2 endBorderPoint = Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 1].position;
			Terrain.Boundary.ProjectStartPointToBoundary(ref startBorderPoint);
			Terrain.Boundary.ProjectEndPointToBoundary(ref endBorderPoint);
			e2dEditorUtils.DrawSphere(startBorderPoint, 2.0f);
			e2dEditorUtils.DrawSphere(endBorderPoint, 2.0f);
		}
	}

	/// Returns true if the boundary is null or not defined.
	private bool IsBoundaryNull()
	{
		return Terrain.TerrainBoundary.width == 0 && Terrain.TerrainBoundary.height == 0;
	}

#endregion


#region Brush

	/// Returns the influence of the brush at the given point on the curve. The influence lies in [0,1].
	private float GetBrushInfluence(Vector2 curvePoint)
	{
		Vector2 delta = new Vector2(mCursorPosition.x, mCursorPosition.y) - curvePoint;
		float brushSize = 0.5f * BrushSize;
		float influence = (brushSize - delta.magnitude) / brushSize;
		if (influence < 0) influence = 0;
		if (influence > 0 && Inspector.Tool == e2dTerrainTool.CURVE_TEXTURE) influence = 1;
		return influence;
	}

	/// Applies brush to the terrain at the current cursor position.
	private void ApplyBrush()
	{
		bool change = false;

		for (int i = 0; i < Terrain.TerrainCurve.Count; i++)
		{
			Vector2 point = Terrain.TerrainCurve[i].position;
			float influence = GetBrushInfluence(point);
			switch (Inspector.Tool)
			{
				case e2dTerrainTool.ADJUST_HEIGHT:
					Vector2 delta = e2dConstants.BRUSH_HEIGHT_RATIO * influence * BrushOpacity * e2dUtils.Vector2dFromAngle(-BrushAngle + 90);
					Terrain.TerrainCurve[i].position = point + delta;
					change = true;
					break;
				case e2dTerrainTool.CURVE_TEXTURE:
					if (influence > 0 && Terrain.TerrainCurve[i].texture != Inspector.SelectedTexture)
					{
						Terrain.TerrainCurve[i].texture = Inspector.SelectedTexture;
						change = true;
					}
					break;
				case e2dTerrainTool.GRASS:
					float d = influence * BrushOpacity * e2dConstants.BRUSH_GRASS_RATIO;
					if (ControlPressed()) d = -d;
					Terrain.TerrainCurve[i].grassRatio = Mathf.Clamp01(Terrain.TerrainCurve[i].grassRatio + d);
					if (influence > 0) change = true;
					break;
			}
		}

		// Note: undo and dirty are sorted out outside of this method

		if (change)
		{
			switch (Inspector.Tool)
			{
				case e2dTerrainTool.ADJUST_HEIGHT:
					Terrain.FixCurve();
					Terrain.FixBoundary();
					mRebuildDataOnMouseUp = true;
					break;
				case e2dTerrainTool.CURVE_TEXTURE:
					mRebuildDataOnMouseUp = true;
					break;
				case e2dTerrainTool.GRASS:
					mRebuildDataOnMouseUp = true;
					break;
			}
		}
	}

	/// Highlights the part of the terrain curve affected by the brush.
	private void DrawCurveBrush()
	{
		if (Terrain.TerrainCurve.Count < 2) return;

		// lines
		Vector2 lastPoint = Vector2.zero;
		float lastInfluence = 0;
		bool first = true;
		foreach (e2dCurveNode node in Terrain.TerrainCurve)
		{
			float influence = GetBrushInfluence(node.position);
			if (first)
			{
				first = false;
			}
			else
			{
				float lineInfluence = 0.5f * (influence + lastInfluence);
				if (lineInfluence > 0 && lineInfluence < 1 && Inspector.Tool == e2dTerrainTool.CURVE_TEXTURE)
				{
					lineInfluence = 0.2f;
				}

				if (lineInfluence != 0)
				{
					Handles.color = new Color(e2dConstants.COLOR_CURVE_BRUSH_LINE.r, e2dConstants.COLOR_CURVE_BRUSH_LINE.g, e2dConstants.COLOR_CURVE_BRUSH_LINE.b, e2dConstants.COLOR_CURVE_BRUSH_LINE.a * lineInfluence);
					e2dEditorUtils.DrawLine(lastPoint, node.position, e2dConstants.SCALE_CURVE_BRUSH_LINE * e2dEditorUtils.GetHandleSize(node.position));
				}
			}

			lastPoint = node.position;
			lastInfluence = influence;
		}


		// points
		foreach (e2dCurveNode node in Terrain.TerrainCurve)
		{
			float influence = GetBrushInfluence(node.position);
			if (influence != 0)
			{
				Handles.color = new Color(e2dConstants.COLOR_CURVE_BRUSH_SPHERE.r, e2dConstants.COLOR_CURVE_BRUSH_SPHERE.g, e2dConstants.COLOR_CURVE_BRUSH_SPHERE.b, e2dConstants.COLOR_CURVE_BRUSH_SPHERE.a * influence);
				e2dEditorUtils.DrawSphere(node.position, e2dConstants.SCALE_CURVE_BRUSH_SPHERE * e2dEditorUtils.GetHandleSize(node.position));
			}
		}
	}

	/// Displays the brush in the scene view.
	private void DrawBrush()
	{
		if (Inspector.Tool == e2dTerrainTool.ADJUST_HEIGHT)
		{
			Handles.color = e2dConstants.COLOR_BRUSH_ARROW;
			e2dEditorUtils.DrawArrow(mCursorPosition, BrushAngle, e2dConstants.SCALE_BRUSH_ARROW * e2dEditorUtils.GetHandleSize(mCursorPosition) * BrushOpacity / e2dConstants.BRUSH_OPACITY_MAX);
		}
	}

#endregion

}