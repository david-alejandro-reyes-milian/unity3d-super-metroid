/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

/// Class managing all styles used in the editor.
public class e2dEditorUtils
{

#region Inspector

	/// Index of the currently dragged slider in the vertical multi-slider GUI element.
	private static int SliderDraggedIndex = -1;

	/// Deselects the current scene tool.
	public static void DeselectSceneTools()
	{
		Tools.current = UnityEditor.Tool.None;
	}

	/// Returns true if any of the default scene tools is selected.
	public static bool IsAnySceneToolSelected()
	{
		return Tools.current > UnityEditor.Tool.None;
	}

	/// Draws fields for editing Vector2 in the current editor window.
	public static Vector2 Vector2Field(string label, Vector2 vector)
	{
		Vector2 v = vector;

		EditorGUILayout.BeginHorizontal();

		// label
		if (label.Length > 0) EditorGUILayout.PrefixLabel(label);

		// fields area
		Rect drawArea = EditorGUILayout.BeginHorizontal(e2dStyles.RectArea, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		Rect area = drawArea;

		float width = (drawArea.width - 1 * e2dConstants.VECTOR_FIELD_PADDING) / 2;
		float labelWidth = 0;

		// ensure there's enough height in the horizontal area
		GUILayoutUtility.GetRect(1, GUI.skin.label.CalcHeight(new GUIContent("A"), 1));

		// draw fields
		area.width = width;
		GUI.Label(area, e2dStrings.LABEL_VECTOR2_X);
		labelWidth = e2dConstants.VECTOR_FIELD_LABEL_MARGIN + GUI.skin.label.CalcSize(new GUIContent(e2dStrings.LABEL_VECTOR2_X)).x;
		area.xMin += labelWidth;
		v.x = EditorGUI.FloatField(area, v.x, e2dStyles.RectField);
		area.xMin += e2dConstants.VECTOR_FIELD_PADDING + width - labelWidth;

		area.width = width;
		GUI.Label(area, e2dStrings.LABEL_VECTOR2_Y);
		labelWidth = e2dConstants.VECTOR_FIELD_LABEL_MARGIN + GUI.skin.label.CalcSize(new GUIContent(e2dStrings.LABEL_VECTOR2_Y)).x;
		area.xMin += labelWidth;
		v.y = EditorGUI.FloatField(area, v.y, e2dStyles.RectField);
		area.xMin += e2dConstants.VECTOR_FIELD_PADDING + width - labelWidth;

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndHorizontal();

		return v;
	}

	/// Draws fields for editing a rectangle in the current editor window.
	public static Rect RectField(string label, Rect rectangle)
	{
		Rect rect = rectangle;

		EditorGUILayout.BeginHorizontal();

		// label
		if (label.Length > 0) EditorGUILayout.PrefixLabel(label);

		// fields area
		Rect drawArea = EditorGUILayout.BeginHorizontal(e2dStyles.RectArea, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
		Rect area = drawArea;

		float width = (drawArea.width - 3 * e2dConstants.RECT_FIELD_PADDING) / 4;
		float labelWidth = 0;

		// ensure there's enough height in the horizontal area
		GUILayoutUtility.GetRect(1, GUI.skin.label.CalcHeight(new GUIContent("A"), 1));

		// draw fields
		area.width = width;
		GUI.Label(area, e2dStrings.LABEL_RECT_XMIN);
		labelWidth = e2dConstants.RECT_FIELD_LABEL_MARGIN + GUI.skin.label.CalcSize(new GUIContent(e2dStrings.LABEL_RECT_XMIN)).x;
		area.xMin += labelWidth;
		rect.xMin = EditorGUI.FloatField(area, rect.xMin, e2dStyles.RectField);
		area.xMin += e2dConstants.RECT_FIELD_PADDING + width - labelWidth;

		area.width = width;
		GUI.Label(area, e2dStrings.LABEL_RECT_XMAX);
		labelWidth = e2dConstants.RECT_FIELD_LABEL_MARGIN + GUI.skin.label.CalcSize(new GUIContent(e2dStrings.LABEL_RECT_XMAX)).x;
		area.xMin += labelWidth;
		rect.xMax = EditorGUI.FloatField(area, rect.xMax, e2dStyles.RectField);
		area.xMin += e2dConstants.RECT_FIELD_PADDING + width - labelWidth;

		area.width = width;
		GUI.Label(area, e2dStrings.LABEL_RECT_YMIN);
		labelWidth = e2dConstants.RECT_FIELD_LABEL_MARGIN + GUI.skin.label.CalcSize(new GUIContent(e2dStrings.LABEL_RECT_YMIN)).x;
		area.xMin += labelWidth;
		rect.yMin = EditorGUI.FloatField(area, rect.yMin, e2dStyles.RectField);
		area.xMin += e2dConstants.RECT_FIELD_PADDING + width - labelWidth;

		area.width = width;
		GUI.Label(area, e2dStrings.LABEL_RECT_YMAX);
		labelWidth = e2dConstants.RECT_FIELD_LABEL_MARGIN + GUI.skin.label.CalcSize(new GUIContent(e2dStrings.LABEL_RECT_YMAX)).x;
		area.xMin += labelWidth;
		rect.yMax = EditorGUI.FloatField(area, rect.yMax, e2dStyles.RectField);
		area.xMin += e2dConstants.RECT_FIELD_PADDING + width - labelWidth;


		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndHorizontal();

		return rect;
	}

	/// Draws a multi-slider in the current editor window. Image previews are used to describe the handles.
	public static void VerticalMultiSlider(string label, ref List<float> values, List<Texture> images, float minValue, float maxValue, float threshold, string thresholdLabel, float size, bool upsideDown)
	{
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.PrefixLabel(label);

		if (images != null && images.Count != values.Count)
		{
			e2dUtils.Error("VerticalMultiSlider: values and images have different number of elements");
		}

		Event currentEvent = new Event(Event.current);

		// init the area we're going to draw into
		Rect area = GUILayoutUtility.GetRect(0, float.MaxValue, size, size, GUI.skin.button);
		const float THUMB_Y_OFFSET = 5;
		float imageSize = size / values.Count / 2;
		area.yMin += 0.5f * imageSize;
		area.yMax -= 0.5f * imageSize;

		// init styles we'll use
		GUIStyle sliderStyle = new GUIStyle("MiniMinMaxSliderVertical");
		sliderStyle.fixedWidth *= 1.5f;
		GUIStyle thumbStyle = new GUIStyle("MinMaxHorizontalSliderThumb");

		// draw the slider background rect
		Rect sliderRect = area;
		sliderRect.xMin += 0.5f * area.width - 0.5f * sliderStyle.fixedWidth;
		sliderRect.xMax -= 0.5f * area.width - 0.5f * sliderStyle.fixedWidth;
		GUI.Box(sliderRect, GUIContent.none, sliderStyle);

		// threshold level
		float thresholdValue = Mathf.Clamp01((threshold - minValue) / (maxValue - minValue));
		if (upsideDown) thresholdValue = 1 - thresholdValue;
		float thresholdY = area.y + THUMB_Y_OFFSET + thresholdValue * (area.height - 2 * THUMB_Y_OFFSET);
		GUI.Box(new Rect(area.x, thresholdY, area.width, 1), GUIContent.none);

		// threshold label
		Vector2 thresholdLabelSize = e2dStyles.MiniLabel.CalcSize(new GUIContent(thresholdLabel));
		Rect thresholdLabelRect = new Rect(area.x - thresholdLabelSize.x - 2, thresholdY - 0.5f * thresholdLabelSize.y - 2, thresholdLabelSize.x, thresholdLabelSize.y);
		GUI.Label(thresholdLabelRect, thresholdLabel, e2dStyles.MiniLabel);

		// draw the thumbs
		for (int i=0; i<values.Count; i++)
		{
			if (float.IsNaN(values[i])) values[i] = 0;

			// the thumb
			float xPos = area.x + 0.5f * area.width;
			float value = Mathf.Clamp01((values[i] - minValue) / (maxValue - minValue));
			if (upsideDown) value = 1 - value;
			float yPos = area.y + THUMB_Y_OFFSET + value * (area.height - 2 * THUMB_Y_OFFSET);
			Rect thumbRect = new Rect(xPos - 0.5f * sliderStyle.fixedWidth, yPos - 0.5f * thumbStyle.fixedHeight - 3, sliderStyle.fixedWidth, thumbStyle.fixedHeight);
			GUI.Button(thumbRect, GUIContent.none, thumbStyle);

			// image preview
			if (images != null && images[i] != null)
			{
				float imageX = xPos - 0.5f * thumbRect.width - imageSize;
				float imageY = yPos - 0.5f * imageSize;
				Rect imageRect = new Rect(imageX, imageY, imageSize, imageSize);
				GUI.DrawTexture(imageRect, images[i]);
			}

			// number values
			string numberString = "" + values[i];
			Vector2 numberSize = e2dStyles.MiniLabel.CalcSize(new GUIContent(numberString));
			float numberX = xPos + 0.5f * thumbRect.width;
			float numberY = yPos - 0.5f * numberSize.y - 1;
			GUI.Label(new Rect(numberX, numberY, numberSize.x, numberSize.y), numberString, e2dStyles.MiniLabel);

			// mouse control
			if (currentEvent.type == EventType.MouseDown && thumbRect.Contains(currentEvent.mousePosition))
			{
				SliderDraggedIndex = i;
			}
		}

		// mouse dragging of a thumb
		if (SliderDraggedIndex != -1 && currentEvent.type == EventType.MouseDrag)
		{
			float value = (currentEvent.mousePosition.y - area.y - THUMB_Y_OFFSET) / (area.height - 2 * THUMB_Y_OFFSET);
			value = Mathf.Clamp01(value);
			if (upsideDown) value = 1 - value;
			values[SliderDraggedIndex] = value * (maxValue - minValue) + minValue;
		}

		// release thumb on mouse up
		if (currentEvent.type == EventType.MouseUp)
		{
			SliderDraggedIndex = -1;
		}

		EditorGUILayout.EndHorizontal();
	}

#endregion


#region Scene

	/// Transform used for all scene drawing functions and handles.
	public static Transform transform;

	/// Returns the size of a handle at a given point in space of the current transform. The current camera is considered.
	public static float GetHandleSize(Vector2 position)
	{
		return HandleUtility.GetHandleSize(transform.TransformPoint(position));
	}

	/// Draws a 2D line.
	public static void DrawLine(Vector2 a, Vector2 b)
	{
		Vector3 a3d = transform.TransformPoint(a);
		Vector3 b3d = transform.TransformPoint(b);
		Handles.DrawLine(a3d, b3d);
	}

	/// Draws a 2D line.
	public static void DrawLine(Vector2 a, Vector2 b, float width)
	{
		Vector2 direction = (a - b);
		Vector2 normal = new Vector2(-direction.y, direction.x);
		normal.Normalize();
		Vector3 a3d = transform.TransformPoint(a);
		Vector3 b3d = transform.TransformPoint(b);
		Vector3 normal3d = transform.TransformDirection(normal);
		
		// Note: DrawAAPolyLine is bugged as of 3.3.0f4
		// The Z part of the second point is ignored.
		//Handles.DrawAAPolyLine(b3d, a3d);

		Vector3[] poly = new Vector3[] { a3d - width * normal3d, a3d + width * normal3d, b3d + width * normal3d, b3d - width * normal3d };
		Handles.DrawSolidRectangleWithOutline(poly, Handles.color, Handles.color);
	}

	/// Draws a 2D sphere (shaded circle).
	public static void DrawSphere(Vector2 center, float size)
	{
		Handles.SphereCap(0, transform.TransformPoint(center), Quaternion.identity, size);
	}

	/// Draws a 2D arrow.
	public static void DrawArrow(Vector2 origin, float angle, float size)
	{
		Quaternion rot = Quaternion.Euler(-transform.eulerAngles.z + angle - 90, 90, 0);
		Handles.ArrowCap(0, transform.TransformPoint(origin), rot, size);
	}

	/// Draws a text label.
	public static void DrawLabel(Vector2 position, string label, GUIStyle style)
	{
		Handles.Label(transform.TransformPoint(position), label, style);
	}

	/// Draws a 2D handle for manipulating 2D vectors.
	public static Vector2 PositionHandle2d(Vector2 position)
	{
		return PositionHandle2dScaled(position, GetHandleSize(position), true);
	}

	/// Draws a 2D handle for manipulating 2D vectors.
	public static Vector2 PositionHandle2d(Vector2 position, bool showSliders)
	{
		return PositionHandle2dScaled(position, GetHandleSize(position), showSliders);
	}

	/// Draws a 2D handle for manipulating 2D vectors. The scale must take into account the camera distance from the object.
	/// To make it independent from the camera distance call PositionHandle2d() or use HandleUtility.GetHandleSize().
	public static Vector2 PositionHandle2dScaled(Vector2 position, float scale, bool showSliders)
	{
		Vector3 position3d = transform.TransformPoint(position);

		Handles.color = e2dConstants.COLOR_HANDLE_CENTER;
		position3d = Handles.FreeMoveHandle(position3d, Quaternion.identity, e2dConstants.SCALE_HANDLE_CENTER * scale, Vector3.zero, Handles.RectangleCap);
		Handles.DotCap(0, position3d, Quaternion.identity, e2dConstants.SCALE_HANDLE_CENTER_DOT * scale);
		if (showSliders)
		{
			Handles.color = e2dConstants.COLOR_HANDLE_X_SLIDER;
			position3d = Handles.Slider(position3d, transform.TransformDirection(Vector3.right), e2dConstants.SCALE_HANDLE_SLIDER * scale, Handles.ArrowCap, 0);
			Handles.color = e2dConstants.COLOR_HANDLE_Y_SLIDER;
			position3d = Handles.Slider(position3d, transform.TransformDirection(Vector3.up), e2dConstants.SCALE_HANDLE_SLIDER * scale, Handles.ArrowCap, 0);
		}

		return transform.InverseTransformPoint(position3d);
	}

	/// Draws a 2D cube handle for manipulating 2D vectors.
	public static Vector2 PositionHandle2dCube(Vector2 position, Color color, float scale)
	{
		Vector3 position3d = transform.TransformPoint(position);

		Handles.color = color;
		position3d = Handles.FreeMoveHandle(position3d, Quaternion.identity, scale * GetHandleSize(position), Vector3.zero, Handles.CubeCap);

		return transform.InverseTransformPoint(position3d);
	}

	/// Draws a 2D handle for manipulating an angle.
	public static float RotationHandle2d(float angle, Vector2 position)
	{
		Handles.color = e2dConstants.COLOR_HANDLE_CENTER;
		Quaternion rotation = Quaternion.Euler(0, 0, angle);
		Vector3 position3d = transform.TransformPoint(position);
		Vector3 direction3d = transform.TransformDirection(Vector3.forward);
		rotation = Handles.Disc(rotation, position3d, direction3d, e2dConstants.SCALE_HANDLE_ROTATION * GetHandleSize(position), false, 0);
		return rotation.eulerAngles.z;
	}

#endregion


#region Scene GUI

	/// Draws an error label in the middle of the screen.
	public static void DrawErrorLabel(string message)
	{
		Handles.BeginGUI();
		GUILayout.FlexibleSpace();
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label(message, e2dStyles.SceneError);
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.FlexibleSpace();
		Handles.EndGUI();
	}

#endregion

}
