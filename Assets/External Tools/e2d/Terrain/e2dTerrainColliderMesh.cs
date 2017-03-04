/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Takes care of the collider mesh data. The mesh is created in a similar way as the fill mesh but is 3d to support
/// MeshCollider. The mesh is stored in a sub-object of the main game object.
public class e2dTerrainColliderMesh : e2dTerrainMesh
{
	// NOTE: any data here are not serialized by Unity.

	/// Default constructor.
	public e2dTerrainColliderMesh(e2dTerrain terrain)
		: base(terrain)
	{
	}

	/// Returns the collider component of the mesh.
	public MeshCollider collider
	{
		get
		{
			EnsureMeshComponentsExist();
			return transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).GetComponent<MeshCollider>();
		}
	}

	/// Returns the object carrying the mesh.
	public GameObject gameObject { get { return transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).gameObject; } }


	/// Rebuilds the mesh from scratch deleting the old one if necessary.
	public void RebuildMesh()
	{
		if (TerrainCurve.Count < 2 || Terrain.CurveIntercrossing)
		{
			DestroyMesh();
			return;
		}

		EnsureMeshComponentsExist();

		// we're sharing the same shape as the fill mesh
		List<Vector2> polygon = Terrain.FillMesh.GetShapePolygon();

		// create a stripe of triangles along the polygon
		Vector3[] vertices = new Vector3[2 * polygon.Count];
		Vector3[] normals = new Vector3[vertices.Length];
		int[] triangles = new int[3 * 2 * polygon.Count];
		for (int i = 0; i < polygon.Count; i++)
		{
			int vertexIndex = 2 * i;
			vertices[vertexIndex + 0] = new Vector3(polygon[i].x, polygon[i].y, -0.5f * e2dConstants.COLLISION_MESH_Z_DEPTH);
			vertices[vertexIndex + 1] = new Vector3(polygon[i].x, polygon[i].y, 0.5f * e2dConstants.COLLISION_MESH_Z_DEPTH);

			int prev = i - 1;
			if (prev < 0) prev += polygon.Count;
			int next = i + 1;
			if (next >= polygon.Count) next -= polygon.Count;
			Vector2 normal1 = new Vector2(polygon[i].y - polygon[prev].y, polygon[prev].x - polygon[prev].x);
			Vector2 normal2 = new Vector2(polygon[next].y - polygon[i].y, polygon[i].x - polygon[next].x);
			Vector3 normal = 0.5f * (normal1 + normal2);
			normal.Normalize();
			normals[vertexIndex + 0] = normal;
			normals[vertexIndex + 1] = normal;
			

			int triangleIndex = 6 * i;
			triangles[triangleIndex + 0] = (vertexIndex + 0) % vertices.Length;
			triangles[triangleIndex + 1] = (vertexIndex + 1) % vertices.Length;
			triangles[triangleIndex + 2] = (vertexIndex + 2) % vertices.Length;
			triangles[triangleIndex + 3] = (vertexIndex + 2) % vertices.Length;
			triangles[triangleIndex + 4] = (vertexIndex + 1) % vertices.Length;
			triangles[triangleIndex + 5] = (vertexIndex + 3) % vertices.Length;
		}

		// set the result to the mesh
		MeshCollider collider = transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).GetComponent<MeshCollider>();
		collider.sharedMesh.Clear();
		collider.sharedMesh.vertices = vertices;
		collider.sharedMesh.triangles = triangles;

		// we need this to make sure the collision mesh is updated
		ResetMeshObjectsTransforms();
	}

	/// Destroys the mesh data.
	public void DestroyMesh()
	{
		EnsureMeshObjectsExist();

		MeshCollider collider = transform.FindChild(e2dConstants.COLLIDER_MESH_NAME).GetComponent<MeshCollider>();
		if (collider && collider.sharedMesh != null)
		{
			Object.DestroyImmediate(collider.sharedMesh);
		}
		// NOTE: can't do that since the objects are used after destroying the mesh
		//Object.DestroyImmediate(collider);
	}
	
}
