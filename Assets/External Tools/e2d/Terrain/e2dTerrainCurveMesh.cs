/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Takes care of the mesh and material data of the terrain surface curve. The mesh creates a thin stripe attached to
/// the surface of the terrain. The mesh is stored in a sub-object of the main terrain game object.
/// The textures of the stripe are blended together using a custom shader Curve.shader.
public class e2dTerrainCurveMesh: e2dTerrainMesh
{
	// NOTE: the following variables are not serialized by Unity.

	/// Control textures for shaders. They are created when needed and are not stored anywhere.
	public List<Texture2D> ControlTextures;
	/// Vertices of the stripe mesh. They are displayed for debug.
	public List<Vector3> StripeVertices;


	/// Default constructor.
	public e2dTerrainCurveMesh(e2dTerrain terrain)
		: base(terrain)
	{
		ControlTextures = new List<Texture2D>();
		StripeVertices = new List<Vector3>();
	}

	/// Returns the renderer component of the mesh.
	public MeshRenderer renderer
	{
		get
		{
			EnsureMeshComponentsExist();
			return transform.FindChild(e2dConstants.CURVE_MESH_NAME).GetComponent<MeshRenderer>();
		}
	}

	/// Returns the object carrying the mesh.
	public GameObject gameObject { get { return transform.FindChild(e2dConstants.CURVE_MESH_NAME).gameObject; } }


#region Mesh

	/// Rebuilds the mesh from scratch destroying the old one.
	public void RebuildMesh()
	{
		if (TerrainCurve.Count < 2 || Terrain.CurveIntercrossing)
		{
			DestroyMesh();
			return;
		}

		EnsureMeshComponentsExist();
		ResetMeshObjectsTransforms();

		// containers
		Vector3[] vertices = new Vector3[TerrainCurve.Count * 2];
		Vector3[] normals = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];
		Vector2[] uvs2 = new Vector2[vertices.Length];
		Color[] colors = new Color[vertices.Length];
		int[] triangles = new int[(TerrainCurve.Count - 1) * 2 * 3];

		// generate stripe vertices
		StripeVertices = ComputeStripeVertices();

		// initial vertices
		vertices[0] = TerrainCurve[0].position;
		vertices[1] = StripeVertices[0];
		normals[0] = Vector3.back;
		normals[1] = Vector3.back;
		uvs[0] = new Vector2(0, 0);
		uvs[1] = new Vector2(0, 0);
		uvs2[0] = vertices[0];
		uvs2[1] = vertices[1];
		colors[0] = new Color(1, 0, 0, 0);
		colors[1] = new Color(0, 0, 0, 0);

		// generate the curve stripe
		float curvePosition = 0;
		for (int nodeIndex = 1; nodeIndex < TerrainCurve.Count; nodeIndex++)
		{
			int pointIndex = nodeIndex;
			int vertexIndex = 2 * pointIndex;
			int triangleIndex = 6 * (nodeIndex - 1);

			float segmentLength = (TerrainCurve[nodeIndex].position - TerrainCurve[nodeIndex - 1].position).magnitude;
			curvePosition += segmentLength;

			Vector3 normal = Vector3.back;
			if (Terrain.PlasticEdges && nodeIndex < TerrainCurve.Count - 1)
			{
				Vector2 tangent1 = (TerrainCurve[nodeIndex].position - TerrainCurve[nodeIndex - 1].position).normalized;
				Vector2 tangent2 = (TerrainCurve[nodeIndex + 1].position - TerrainCurve[nodeIndex].position).normalized;
				Vector2 tangent = (tangent1 + tangent2).normalized;
				normal = new Vector3(-tangent.y, tangent.x, -1);
			}
	
			vertices[vertexIndex + 0] = TerrainCurve[pointIndex].position;
			normals[vertexIndex + 0] = normal;
			uvs[vertexIndex + 0] = new Vector2(curvePosition, pointIndex);
			uvs2[vertexIndex + 0] = vertices[vertexIndex + 0];
			colors[vertexIndex + 0] = new Color(1, 0, 0, 0);

			vertices[vertexIndex + 1] = StripeVertices[nodeIndex];
			normals[vertexIndex + 1] = Vector3.back;
			uvs[vertexIndex + 1] = new Vector2(curvePosition, pointIndex);
			uvs2[vertexIndex + 1] = vertices[vertexIndex + 1];
			colors[vertexIndex + 1] = new Color(0, 0, 0, 0);


			if (e2dUtils.PointInTriangle(vertices[vertexIndex + 1], vertices[vertexIndex - 2], vertices[vertexIndex], vertices[vertexIndex - 1]))
			{
				// the quad is concave - splitting this way should sort it out
				triangles[triangleIndex + 0] = vertexIndex - 2;
				triangles[triangleIndex + 1] = vertexIndex + 1;
				triangles[triangleIndex + 2] = vertexIndex - 1;

				triangles[triangleIndex + 3] = vertexIndex - 2;
				triangles[triangleIndex + 4] = vertexIndex;
				triangles[triangleIndex + 5] = vertexIndex + 1;
			}
			else
			{
				triangles[triangleIndex + 0] = vertexIndex - 2;
				triangles[triangleIndex + 1] = vertexIndex;
				triangles[triangleIndex + 2] = vertexIndex - 1;

				triangles[triangleIndex + 3] = vertexIndex - 1;
				triangles[triangleIndex + 4] = vertexIndex;
				triangles[triangleIndex + 5] = vertexIndex + 1;
			}
		}


		// set the result to the mesh
		MeshFilter filter = transform.FindChild(e2dConstants.CURVE_MESH_NAME).GetComponent<MeshFilter>();
		filter.sharedMesh.Clear();
		filter.sharedMesh.vertices = vertices;
		filter.sharedMesh.normals = normals;
		filter.sharedMesh.uv = uvs;
		filter.sharedMesh.uv2 = uvs2;
		filter.sharedMesh.colors = colors;
		filter.sharedMesh.triangles = triangles;


		if (SomeMaterialsMissing()) RebuildMaterial();
	}

	/// Computes the list of all stripe vertices. There's the same amount of them as the nodes. So for each node
	/// there exists exactly one stripe vertex and has the same index.
	private List<Vector3> ComputeStripeVertices()
	{
		List<Vector3> vertices = new List<Vector3>(TerrainCurve.Count);

		// compute the vertices
		vertices.Add(ComputeFirstStripeVertex());
		for (int i=1; i<TerrainCurve.Count-1; i++)
		{
			vertices.Add(ComputeStripeVertex(i));
		}
		vertices.Add(ComputeLastStripeVertex());

		// now the vertices may inter-cross themselves and we have to fix this; we do it by moving all vertices
		// in the part which is inter-crossing to the same position
		// NOTE: this is the most time-consuming part of terrain meshes rebuilding
		for (int i=0; i<vertices.Count-1; i++)
		{
			// check against segments further along the way
			for (int j=i+2; j<vertices.Count-1; j++)
			{
				Vector2 intersection;
				if (e2dUtils.SegmentsIntersect(vertices[i], vertices[i+1], vertices[j], vertices[j+1], out intersection))
				{
					// the vertices are inter-crossing, so we move all the vertices between them to the intersection position
					for (int k=i+1; k<=j; k++)
					{
						vertices[k] = intersection;
					}
					break;
				}
			}
		}

		return vertices;
	}

	/// Returns the stripe vertex of the first point of the curve. The vertex lies on the way from the point to
	/// the terrain boundary. The distance is computed so that it corresponds to the desired thickness of the stripe.
	private Vector3 ComputeFirstStripeVertex()
	{
		// boundary point
		Vector2 startBorderPoint = TerrainCurve[0].position;
		Boundary.ProjectStartPointToBoundary(ref startBorderPoint);
		Vector3 firstStripeVertex = startBorderPoint;

		if (startBorderPoint != TerrainCurve[0].position)
		{
			Vector2 delta1 = TerrainCurve[1].position - TerrainCurve[0].position;
			Vector2 move1 = GetNodeStripeSize(0) * new Vector2(delta1.y, -delta1.x).normalized;
			Vector2 borderDirection = startBorderPoint - TerrainCurve[0].position;
			Vector2 move;
			if (!e2dUtils.HalfLineAndLineIntersect(Vector2.zero, borderDirection, move1, move1 + delta1, out move))
			{
				// the segment is perpendicular to the border or is in concave angle to the border
				move = Vector2.zero;
			}
			firstStripeVertex = TerrainCurve[0].position + move;
			Boundary.EnsurePointIsInBoundary(ref firstStripeVertex);
		}

		return firstStripeVertex;
	}

	/// Returns the stripe vertex of the last point of the curve. The vertex lies on the way from the point to
	/// the terrain boundary. The distance is computed so that it corresponds to the desired thickness of the stripe.
	private Vector3 ComputeLastStripeVertex()
	{
		// boundary point
		Vector2 endBorderPoint = TerrainCurve[TerrainCurve.Count - 1].position;
		Boundary.ProjectEndPointToBoundary(ref endBorderPoint);
		Vector3 lastStripeVertex = endBorderPoint;

		if (endBorderPoint != TerrainCurve[TerrainCurve.Count - 1].position)
		{
			Vector2 delta2 = TerrainCurve[TerrainCurve.Count - 1].position - TerrainCurve[TerrainCurve.Count - 2].position;
			Vector2 move2 = GetNodeStripeSize(TerrainCurve.Count - 1) * new Vector2(delta2.y, -delta2.x).normalized;
			Vector2 borderDirection = endBorderPoint - TerrainCurve[TerrainCurve.Count - 1].position;
			Vector2 move;
			if (!e2dUtils.HalfLineAndLineIntersect(Vector2.zero, borderDirection, move2, move2 + delta2, out move))
			{
				// the segment is perpendicular to the border or is in concave angle to the border
				move = Vector2.zero;
			}
			lastStripeVertex = TerrainCurve[TerrainCurve.Count - 1].position + move;
			Boundary.EnsurePointIsInBoundary(ref lastStripeVertex);
		}

		return lastStripeVertex;
	}

	/// Returns the stripe vertex of the given segment of the curve. The vertex goes towards the inside of the terrain.
	/// The direction and distance is computed so that it complies to the desired thickness of the stripe.
	private Vector3 ComputeStripeVertex(int nodeIndex)
	{
		// compute the stripe point from the segment stripes around this point
		Vector2 delta1 = TerrainCurve[nodeIndex + 0].position - TerrainCurve[nodeIndex - 1].position;
		Vector2 delta2 = TerrainCurve[nodeIndex + 1].position - TerrainCurve[nodeIndex + 0].position;
		Vector2 move1 = new Vector2(delta1.y, -delta1.x).normalized;
		Vector2 move2 = new Vector2(delta2.y, -delta2.x).normalized;
		Vector2 move = GetNodeStripeSize(nodeIndex) * (move1 + move2).normalized;

		Vector3 stripeVertex = TerrainCurve[nodeIndex].position + move;

		// NOTE: by disabling this we allowed the stripe to reach out of the terrain boundary. The user will fix
		// the boundary if he doesn't like that.
		// But right now it seems better when it's enabled.
		Boundary.EnsurePointIsInBoundary(ref stripeVertex);

		return stripeVertex;
	}

	/// Returns the desired size of the stripe for the specified node of the curve. The size is determined from
	/// the settings of the curve textures used in the segment.
	private float GetNodeStripeSize(int nodeIndex)
	{
		e2dCurveTexture curveTexture = CurveTextures[TerrainCurve[nodeIndex].texture];
		float size = curveTexture.size.y;

		return size;
	}

	/// Destroys the mesh data and prepares the object for the creation of new mesh.
	public void DestroyMesh()
	{
		EnsureMeshObjectsExist();

		DestroyTemporaryAssets();

		MeshFilter filter = transform.FindChild(e2dConstants.CURVE_MESH_NAME).GetComponent<MeshFilter>();
		if (filter && filter.sharedMesh != null)
		{
			Object.DestroyImmediate(filter.sharedMesh);
		}
		// NOTE: can't do that since the objects are used after destroying the mesh
		//Object.DestroyImmediate(filter);

		MeshRenderer renderer = transform.FindChild(e2dConstants.CURVE_MESH_NAME).GetComponent<MeshRenderer>();
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

	/// Rebuilds the material for the mesh from scratch deleting the old one if there is any.
	public void RebuildMaterial()
	{
		EnsureMeshComponentsExist();

		MeshRenderer renderer = transform.FindChild(e2dConstants.CURVE_MESH_NAME).GetComponent<MeshRenderer>();

		Material[] materials = renderer.sharedMaterials;
		if (materials != null)
		{
			foreach (Material material in materials)
			{
				Object.DestroyImmediate(material, true);
			}
		}

		// make sure the textures are ready to use
		EnsureTexturesInited();

		// init the materials in as many instances as necessary
		int materialCount = GetMaterialsNeededCount();
		materials = new Material[materialCount];
		if (ControlTextures.Count != materialCount) UpdateControlTextures();

		// init each shader
		int textureIndex = 0;
		for (int material = 0; material < materialCount; material++)
		{
			materials[material] = new Material(Shader.Find("e2d/Curve"));

			// texture params
			materials[material].SetFloat("_ControlSize", ControlTextures[material].width);
			materials[material].SetTexture("_Control", ControlTextures[material]);

			for (int i = 0; i < e2dConstants.NUM_TEXTURES_PER_STRIPE_SHADER; i++, textureIndex++)
			{
				if (textureIndex >= CurveTextures.Count) break;
				materials[material].SetTexture("_Splat" + i, CurveTextures[textureIndex].texture);
				Vector4 prms = new Vector4(CurveTextures[textureIndex].size.x, CurveTextures[textureIndex].size.y, CurveTextures[textureIndex].fixedAngle ? 1 : 0, CurveTextures[textureIndex].fadeThreshold);
				materials[material].SetVector("_SplatParams" + i, prms);
			}
		}

		// set the new materials to the renderer
		renderer.materials = materials;
	}

	/// Makes sure the curve textures and control textures are in a consistent state.
	private void EnsureTexturesInited()
	{
		if (CurveTextures.Count == 0)
		{
			CurveTextures.Clear();
			CurveTextures.Add(GetDefaultCurveTexture());
			foreach (Texture texture in ControlTextures) Object.DestroyImmediate(texture, true);
			ControlTextures.Clear();
			ControlTextures.Add(CreateControlTexture(new Color(1, 0, 0, 0)));
		}
	}

	/// Updates the control textures of the shaders.
	public void UpdateControlTextures()
	{
		UpdateControlTextures(false);
	}

	/// Updates the control textures of the shaders. If forceRecreate is true all control textures are created anew even
	/// if there is no need to do so.
	public void UpdateControlTextures(bool forceRecreate)
	{
		// remove control texture if there are too many of them
		while (ControlTextures.Count > GetMaterialsNeededCount())
		{
			Object.DestroyImmediate(ControlTextures[ControlTextures.Count - 1], true);
			ControlTextures.RemoveAt(ControlTextures.Count - 1);
		}

		// add new control textures if necessary
		while (ControlTextures.Count < GetMaterialsNeededCount())
		{
			ControlTextures.Add(CreateControlTexture(new Color(0, 0, 0, 0)));
		}

		// update each of the materials with the control texture
		for (int i = 0; i < ControlTextures.Count; i++)
		{
			// recreate the texture if necessary
			if (forceRecreate || ControlTextures[i] == null || ControlTextures[i].width != GetControlTextureSize())
			{
				Object.DestroyImmediate(ControlTextures[i], true);
				ControlTextures[i] = CreateControlTexture(new Color(0, 0, 0, 0));

				EnsureMeshComponentsExist();
				MeshRenderer renderer = transform.FindChild(e2dConstants.CURVE_MESH_NAME).GetComponent<MeshRenderer>();
				if (renderer.sharedMaterials != null && i == renderer.sharedMaterials.Length - 1 && renderer.sharedMaterials[i])
				{
					renderer.sharedMaterials[i].SetFloat("_ControlSize", ControlTextures[i].width);
					renderer.sharedMaterials[i].SetTexture("_Control", ControlTextures[i]);
				}
			}

			// are there any data at all?
			if (TerrainCurve.Count == 0) break;

			// set the color values according to the texture mixing
			Color[] colors = new Color[TerrainCurve.Count];
			for (int j = 0; j < TerrainCurve.Count; j++)
			{
				colors[j] = new Color(0, 0, 0, 0);
				if (TerrainCurve[j].texture / e2dConstants.NUM_TEXTURES_PER_STRIPE_SHADER == i)
				{
					switch (TerrainCurve[j].texture % e2dConstants.NUM_TEXTURES_PER_STRIPE_SHADER)
					{
						case 0: colors[j].r = 1; break;
						case 1: colors[j].g = 1; break;
						case 2: colors[j].b = 1; break;
						case 3: colors[j].a = 1; break;
					}
				}
			}

			// now set the data to the texture
			ControlTextures[i].SetPixels(0, 0, colors.Length, 1, colors);
			ControlTextures[i].Apply();
		}
	}

	/// Returns the number of materials (and shaders) needed for the textures we're using. Each shader can work only
	/// with a limited number of textures, so that's why.
	private int GetMaterialsNeededCount()
	{
		int materialCount = CurveTextures.Count / e2dConstants.NUM_TEXTURES_PER_STRIPE_SHADER;
		if (CurveTextures.Count % e2dConstants.NUM_TEXTURES_PER_STRIPE_SHADER != 0) materialCount++;
		return materialCount;
	}

	/// Returns the size of the control textures. This depends on the current terrain data.
	private int GetControlTextureSize()
	{
		int textureSize = Mathf.NextPowerOfTwo(TerrainCurve.Count);
		if (textureSize == 0) textureSize = 1;
		return textureSize;
	}

	/// Creates new control texture filling it with the given color.
	private Texture2D CreateControlTexture(Color color)
	{
		int size = GetControlTextureSize();
		Texture2D texture = new Texture2D(size, 1, TextureFormat.ARGB32, false);
		texture.filterMode = FilterMode.Bilinear;
		texture.wrapMode = TextureWrapMode.Clamp;
		texture.anisoLevel = 1;

		Color[] colors = new Color[size];
		for (int i = 0; i < size; i++)
		{
			colors[i] = color;
		}
		texture.SetPixels(colors);
		texture.Apply();
		return texture;
	}

	/// Returns the default texture used when no other is defined by the user.
	public e2dCurveTexture GetDefaultCurveTexture()
	{
		e2dCurveTexture result = new e2dCurveTexture((Texture)Resources.Load("defaultCurveTexture", typeof(Texture)));
		return result;
	}

	/// Returns true if any materials needed by the mesh are missing in the game objects.
	public bool SomeMaterialsMissing()
	{
		return transform.FindChild(e2dConstants.CURVE_MESH_NAME).GetComponent<MeshRenderer>().sharedMaterial == null;
	}

	/// Adds new texture to the end of the list.
	public void AppendCurveTexture()
	{
		Terrain.CurveTextures.Add(Terrain.CurveMesh.GetDefaultCurveTexture());
		UpdateControlTextures();
	}

	/// Destroy a texture from the list at the given position.
	public void RemoveCurveTexture(int index)
	{
		Terrain.CurveTextures.RemoveAt(index);

		// go through the nodes and fix the texture indices
		foreach (e2dCurveNode node in TerrainCurve)
		{
			if (node.texture == index)
			{
				node.texture = 0;
			}
			else if (node.texture > index)
			{
				node.texture--;
			}
		}

		UpdateControlTextures();
	}

	/// Destroys temporary assets created by this script.
	public void DestroyTemporaryAssets()
	{
		EnsureMeshObjectsExist();

		foreach (Texture2D texture in ControlTextures)
		{
			Object.DestroyImmediate(texture, true);
		}
		ControlTextures.Clear();
	}

#endregion

}