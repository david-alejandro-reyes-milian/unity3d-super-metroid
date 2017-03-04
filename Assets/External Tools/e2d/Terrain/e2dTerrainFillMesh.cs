/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Takes care of the fill mesh and material data. The mesh is created between the terrain surface curve and the terrain
/// boundary. The mesh and material are stored in a sub-object of the main game object.
public class e2dTerrainFillMesh: e2dTerrainMesh
{
	// NOTE: any data here are not serizalised by Unity.

	/// Default constructor.
	public e2dTerrainFillMesh(e2dTerrain terrain): base(terrain)
	{
	}

	/// Returns the renderer component of the mesh.
	public MeshRenderer renderer
	{
		get
		{
			EnsureMeshComponentsExist();
			return transform.FindChild(e2dConstants.FILL_MESH_NAME).GetComponent<MeshRenderer>();
		}
	}

	/// Returns the object carrying the mesh.
	public GameObject gameObject { get { return transform.FindChild(e2dConstants.FILL_MESH_NAME).gameObject; } }


#region Mesh

	/// Rebuilds the mesh from scratch deleting the old one if necessary.
	public void RebuildMesh()
	{
		if (TerrainCurve.Count < 2 || Terrain.CurveIntercrossing)
		{
			DestroyMesh();
			return;
		}

		EnsureMeshComponentsExist();
		ResetMeshObjectsTransforms();

		// create the shape polygon
		List<Vector2> polygon = GetShapePolygon();

		// triangulate the polygon
		e2dTriangulator triangulator = new e2dTriangulator(polygon.ToArray());
		List<int> triangleList = triangulator.Triangulate();
		triangleList.Reverse();
		int[] triangles = triangleList.ToArray();

		// generate 3d vertices and UVs
		Vector3[] vertices = new Vector3[polygon.Count];
		Vector3[] normals = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];

		for (int i = 0; i < polygon.Count; i++)
		{
			vertices[i] = polygon[i];
			normals[i] = Vector3.back;
			uvs[i] = GetPointFillUV(polygon[i]);
		}

		// set the result to the mesh
		MeshFilter filter = transform.FindChild(e2dConstants.FILL_MESH_NAME).GetComponent<MeshFilter>();
		filter.sharedMesh.Clear();
		filter.sharedMesh.vertices = vertices;
		filter.sharedMesh.normals = normals;
		filter.sharedMesh.uv = uvs;
		filter.sharedMesh.triangles = triangles;


		if (SomeMaterialsMissing()) RebuildMaterial();
	}

	/// Returns true if the mesh is valid.
	public bool IsMeshValid()
	{
		Transform meshObject = transform.FindChild(e2dConstants.FILL_MESH_NAME);
		if (meshObject == null) return false;
		MeshFilter filter = meshObject.GetComponent<MeshFilter>();
		if (filter == null) return false;
		if (filter.sharedMesh == null) return false;
		if (filter.sharedMesh.vertexCount == 0) return false;
		
		return true;
	}

	/// Creates a polygon for the shape of the fill mesh.
	public List<Vector2> GetShapePolygon()
	{
		List<Vector2> polygon = new List<Vector2>(TerrainCurve.Count + 3);

		// project the curve ends to the boundary
		Vector2[] boundary = Boundary.GetBoundaryRect();
		Vector2 startBorderPoint = TerrainCurve[0].position;
		Vector2 endBorderPoint = TerrainCurve[TerrainCurve.Count - 1].position;
		int startEdge = Boundary.ProjectStartPointToBoundary(ref startBorderPoint);
		int endEdge = Boundary.ProjectEndPointToBoundary(ref endBorderPoint);

		// first add the border point of the end of the curve (we're going clock-wise)
		if (!Terrain.CurveClosed && endBorderPoint != TerrainCurve[TerrainCurve.Count - 1].position)
		{
			polygon.Add(endBorderPoint);
		}

		// now try to walk along the edges towards the beginning of the curve adding corners
		if (!Terrain.CurveClosed)
		{
			bool skipFirst = (endBorderPoint - boundary[endEdge + 1]).sqrMagnitude <= (startBorderPoint - boundary[endEdge + 1]).sqrMagnitude;
			for (int edge = endEdge; skipFirst || edge != startEdge; edge = (edge + 1) % 4)
			{
				skipFirst = false;
				polygon.Add(boundary[edge + 1]);
			}
		}

		// add the border point of the beginning of the curve
		if (!Terrain.CurveClosed && startBorderPoint != TerrainCurve[0].position)
		{
			polygon.Add(startBorderPoint);
		}

		// finally, add the curve itself
		foreach (e2dCurveNode node in TerrainCurve)
		{
			polygon.Add(node.position);
		}

		// check if the first and last points are not the same
		if (polygon[polygon.Count - 1] == polygon[0])
		{
			polygon.RemoveAt(polygon.Count - 1);
		}

		return polygon;
	}

	/// Returns the UV parameters of a mesh vertex at the given point in the terrain space. This is used to map
	/// the texture to the mesh.
	private Vector2 GetPointFillUV(Vector2 curvePoint)
	{
		float u = (curvePoint.x - Terrain.FillTextureTileOffsetX) / Terrain.FillTextureTileWidth;
		float v = (curvePoint.y - Terrain.FillTextureTileOffsetY) / Terrain.FillTextureTileHeight;
		return new Vector2(u, v);
	}

	/// Destroys the mesh data.
	public void DestroyMesh()
	{
		EnsureMeshObjectsExist();

		MeshFilter filter = transform.FindChild(e2dConstants.FILL_MESH_NAME).GetComponent<MeshFilter>();
		if (filter && filter.sharedMesh != null)
		{
			Object.DestroyImmediate(filter.sharedMesh);
		}
		// NOTE: can't do that since the objects are used after destroying the mesh
		//Object.DestroyImmediate(filter);

		MeshRenderer renderer = transform.FindChild(e2dConstants.FILL_MESH_NAME).GetComponent<MeshRenderer>();
		if (renderer && renderer.sharedMaterials != null)
		{
			foreach (Material material in renderer.sharedMaterials)
			{
				Object.DestroyImmediate(material);
			}
		}
		// NOTE: can't do that since the objects are used after destroying the mesh
		//Object.DestroyImmediate(renderer);
	}

#endregion


#region Material

	/// Rebuilds the material from scratch deleting the old one if needed.
	public void RebuildMaterial()
	{
		EnsureMeshComponentsExist();

		MeshRenderer renderer = transform.FindChild(e2dConstants.FILL_MESH_NAME).GetComponent<MeshRenderer>();

		Material[] materials = renderer.sharedMaterials;
		if (materials != null)
		{
			foreach (Material material in materials)
			{
				Object.DestroyImmediate(material, true);
			}
		}

		materials = new Material[1];
		materials[0] = new Material(Shader.Find("e2d/Fill"));
		if (!Terrain.FillTexture)
		{
			Terrain.FillTexture = (Texture)Resources.Load("defaultFillTexture", typeof(Texture));
		}
		materials[0].mainTexture = Terrain.FillTexture;

		renderer.materials = materials;
	}

	/// Returns true if some of the materials needed by the mesh are missing.
	public bool SomeMaterialsMissing()
	{
		return transform.FindChild(e2dConstants.FILL_MESH_NAME).GetComponent<MeshRenderer>().sharedMaterial == null;
	}

#endregion
}