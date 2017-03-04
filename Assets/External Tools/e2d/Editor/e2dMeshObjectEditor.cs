/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using UnityEditor;
using System.Collections;

/// Custom editor for the e2dMeshObject component.
[CustomEditor(typeof(e2dMeshObject))]
public class e2dMeshObjectEditor : Editor
{
	/// Called when the scene window is to be redrawn.
	void OnSceneGUI()
	{
		FixSelection();
	}

	/// Called when the inspector window is to be drawn. It manages all the GUI drawing and input.
	public override void OnInspectorGUI()
	{
		FixSelection();
	}

	/// Fixes the current selection in the scene editor. If this object is selected it selects the main
	/// terrain object instead.
	private void FixSelection()
	{
		if (!e2dUtils.DEBUG_SHOW_SUBOBJECTS)
		{
			Selection.activeGameObject = ((e2dMeshObject)target).transform.parent.gameObject;
			// TODO: hide these objects altogether when they fix HideFlags
			// we want to allow the user to attach other objects under Terrain
		}
	}
}