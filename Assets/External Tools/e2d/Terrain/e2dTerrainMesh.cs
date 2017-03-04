/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Base class for all subparts of the main terrain class. It provides access to the main class and settings and
/// functionality shared by the subparts.
public abstract class e2dTerrainMesh
{
	// private vars
	private e2dTerrain mTerrain;


#region Properties

	/// The main class of the terrain object.
	protected e2dTerrain Terrain { get { return mTerrain; } }

	/// Current transform of the terrain object.
	protected Transform transform { get { return mTerrain.transform; } }

	/// Definition of the curve of the terrain.
	protected List<e2dCurveNode> TerrainCurve { get { return mTerrain.TerrainCurve; } }

	/// Textures used for the curve mesh.
	protected List<e2dCurveTexture> CurveTextures { get { return mTerrain.CurveTextures; } }

	/// Textures used for grass.
	protected List<e2dGrassTexture> GrassTextures { get { return mTerrain.GrassTextures; } }

	/// Control textured for shaders used by the curve mesh.
	protected List<Texture2D> CurveControlTextures { get { return mTerrain.CurveMesh.CurveControlTextures; } }

	/// Object taking care of the boundary definition of the terrain.
	protected e2dTerrainBoundary Boundary { get { return mTerrain.Boundary; } }

#endregion

	/// Default constructor.
	public e2dTerrainMesh(e2dTerrain terrain)
	{
		mTerrain = terrain;
	}

	/// Makes sure the sub-objects of the main game object are read to use.
	protected void ResetMeshObjectsTransforms()
	{
		transform.FindChild(e2dConstants.FILL_MESH_NAME).transform.localPosition = Vector3.zero;
		transform.FindChild(e2dConstants.FILL_MESH_NAME).transform.localRotation = Quaternion.identity;
		transform.FindChild(e2dConstants.FILL_MESH_NAME).transform.localScale = Vector3.one;

		transform.FindChild(e2dConstants.CURVE_MESH_NAME).transform.localPosition = Vector3.zero;
		transform.FindChild(e2dConstants.CURVE_MESH_NAME).transform.localRotation = Quaternion.identity;
		transform.FindChild(e2dConstants.CURVE_MESH_NAME).transform.localScale = Vector3.one;

		transform.FindChild(e2dConstants.GRASS_MESH_NAME).transform.localPosition = Vector3.zero;
		transform.FindChild(e2dConstants.GRASS_MESH_NAME).transform.localRotation = Quaternion.identity;
		transform.FindChild(e2dConstants.GRASS_MESH_NAME).transform.localScale = Vector3.one;

		transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).transform.localPosition = Vector3.zero;
		transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).transform.localRotation = Quaternion.identity;
		transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).transform.localScale = Vector3.one;
	}

	/// Makes sure the sub-objects of the main game object exist. They carry the mesh data.
	protected void EnsureMeshObjectsExist()
	{
		if (transform.FindChild(e2dConstants.FILL_MESH_NAME) == null)
		{
			GameObject go = new GameObject(e2dConstants.FILL_MESH_NAME);
			go.transform.parent = transform;
		}

		if (transform.FindChild(e2dConstants.CURVE_MESH_NAME) == null)
		{
			GameObject go = new GameObject(e2dConstants.CURVE_MESH_NAME);
			go.transform.parent = transform;
		}

		if (transform.FindChild(e2dConstants.GRASS_MESH_NAME) == null)
		{
			GameObject go = new GameObject(e2dConstants.GRASS_MESH_NAME);
			go.transform.parent = transform;
		}

		if (transform.FindChild(e2dConstants.COLLIDER_MESH_NAME) == null)
		{
			GameObject go = new GameObject(e2dConstants.COLLIDER_MESH_NAME);
			go.transform.parent = transform;
		}
	}

	/// Makes sure the components carrying the mesh and material data in the sub-objects exist.
	protected void EnsureMeshComponentsExist()
	{
		EnsureMeshObjectsExist();

		GameObject meshObject;

		meshObject = transform.FindChild(e2dConstants.FILL_MESH_NAME).gameObject;
		EnsureMeshFilterExists(meshObject);
		EnsureMeshRendererExists(meshObject);
		EnsureScriptsAttached(meshObject);

		meshObject = transform.FindChild(e2dConstants.CURVE_MESH_NAME).gameObject;
		EnsureMeshFilterExists(meshObject);
		EnsureMeshRendererExists(meshObject);
		EnsureScriptsAttached(meshObject);

		meshObject = transform.FindChild(e2dConstants.GRASS_MESH_NAME).gameObject;
		EnsureMeshFilterExists(meshObject);
		EnsureMeshRendererExists(meshObject);
		EnsureScriptsAttached(meshObject);

		meshObject = transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).gameObject;
		EnsureMeshColliderExists(meshObject);
		EnsureScriptsAttached(meshObject);
	}

	/// Makes sure the script components are attached to the game object.
	protected void EnsureScriptsAttached(GameObject meshObject)
	{
		if (meshObject.GetComponent<e2dMeshObject>() == null)
		{
			meshObject.AddComponent<e2dMeshObject>();
		}
	}

	/// Makes sure the mesh filter in the given game object exists.
	protected void EnsureMeshFilterExists(GameObject meshObject)
	{
		if (meshObject.GetComponent<MeshFilter>() == null)
		{
			// NOTE: this mesh can leak if someone destroys the component without explicitly
			// destroying the mesh
			Mesh mesh = new Mesh();
			mesh.name = meshObject.name;
			meshObject.AddComponent<MeshFilter>().mesh = mesh;
		}
		else if (meshObject.GetComponent<MeshFilter>().sharedMesh == null)
		{
			Mesh mesh = new Mesh();
			mesh.name = meshObject.name;
			meshObject.GetComponent<MeshFilter>().mesh = mesh;
		}
	}

	/// Makes sure the mesh renderer in the given game object exists.
	protected void EnsureMeshRendererExists(GameObject meshObject)
	{
		if (meshObject.GetComponent<MeshRenderer>() == null)
		{
			meshObject.AddComponent<MeshRenderer>();
		}
	}

	/// Makes sure the mesh collider in the given game object exists.
	protected void EnsureMeshColliderExists(GameObject meshObject)
	{
		if (meshObject.GetComponent<MeshCollider>() == null)
		{
			// NOTE: this mesh can leak if someone destroys the component without explicitly
			// destroying the mesh
			Mesh mesh = new Mesh();
			mesh.name = meshObject.name;
			meshObject.AddComponent<MeshCollider>().sharedMesh = mesh;
		}
		else if (meshObject.GetComponent<MeshCollider>().sharedMesh == null)
		{
			Mesh mesh = new Mesh();
			mesh.name = meshObject.name;
			meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
		}
	}

	/// Deletes all sub-objects holding mesh and material data.
	public void DeleteAllSubobjects()
	{
		EnsureMeshObjectsExist();
		Object.DestroyImmediate(transform.FindChild(e2dConstants.FILL_MESH_NAME).gameObject);
		Object.DestroyImmediate(transform.FindChild(e2dConstants.CURVE_MESH_NAME).gameObject);
		Object.DestroyImmediate(transform.FindChild(e2dConstants.GRASS_MESH_NAME).gameObject);
		Object.DestroyImmediate(transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).gameObject);
	}

}