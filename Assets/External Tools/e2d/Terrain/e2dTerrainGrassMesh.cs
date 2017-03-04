/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Takes care of the mesh and material data of the terrain grass. The mesh is composed of many grass triangles attached
/// to the surface of the terrain. The waving is done in Grass.shader.
/// The mesh is stored in a sub-object of the main terrain game object.
public class e2dTerrainGrassMesh : e2dTerrainMesh
{
	// NOTE: the following variables are not serialized by Unity.


	/// Default constructor.
	public e2dTerrainGrassMesh(e2dTerrain terrain)
		: base(terrain)
	{

	}

	/// Returns the renderer component of the mesh.
	public MeshRenderer renderer
	{
		get
		{
			EnsureMeshComponentsExist();
			return transform.FindChild(e2dConstants.GRASS_MESH_NAME).GetComponent<MeshRenderer>();
		}
	}

	/// Returns the object carrying the mesh.
	public GameObject gameObject { get { return transform.FindChild(e2dConstants.GRASS_MESH_NAME).gameObject; } }


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

		// compute total amount of grass to be produced
		int totalCount = 0;
		for (int i = 0; i < TerrainCurve.Count - 1; i++) totalCount += GetGrassCount(i);

		// containers
		Vector3[] vertices = new Vector3[totalCount * 4];
		Vector3[] normals = new Vector3[vertices.Length];
		Vector2[] uvs = new Vector2[vertices.Length];
		Vector2[] uvs2 = new Vector2[vertices.Length];
		Color[] colors = new Color[vertices.Length];
		int[] triangles = new int[totalCount * 2 * 3];

		int oldSeed = UnityEngine.Random.seed;
		int grassIndex = 0;
		float totalOffset = 0;

		// generate the grass stripe
		for (int segmentIndex = 0; segmentIndex < TerrainCurve.Count - 1; segmentIndex++)
		{
			float segmentLength = (TerrainCurve[segmentIndex + 1].position - TerrainCurve[segmentIndex].position).magnitude;

			UnityEngine.Random.seed = 12345 + segmentIndex;
			int count = GetGrassCount(segmentIndex);
			for (int i = 0; i < count; i++)
			{
				int pointIndex = segmentIndex;
				int vertexIndex = 4 * grassIndex;
				int triangleIndex = 6 * grassIndex;

				// grass type
				int type = Mathf.RoundToInt(UnityEngine.Random.value * (GrassTextures.Count - 1));

				// dimensions of the grass
				float width = GrassTextures[type].size.x + (2 * UnityEngine.Random.value - 1) * GrassTextures[type].sizeRandomness.x * GrassTextures[type].size.x;
				float height = GrassTextures[type].size.y + (2 * UnityEngine.Random.value - 1) * GrassTextures[type].sizeRandomness.y * GrassTextures[type].size.y;
				float normalizedWidth = width / segmentLength;

				// placement limits
				float min = 0.5f * normalizedWidth;
				float max = 1 - 0.5f * normalizedWidth;

				// check segments around and adjust the min and max delta
				if (pointIndex > 0 && TerrainCurve[pointIndex - 1].grassRatio > 0)
				{
					min -= GrassTextures[type].size.y * (1 - GrassTextures[type].sizeRandomness.y); // minimum height possible
					min = Mathf.Clamp01(min);
				}
				if (pointIndex < TerrainCurve.Count - 2 && TerrainCurve[pointIndex + 2].grassRatio > 0)
				{
					max += GrassTextures[type].size.y * (1 - GrassTextures[type].sizeRandomness.y);
					max = Mathf.Clamp01(max);
				}
				if (min > max) min = max = 0.5f * (min + max);

				// place the grass
				float delta = 0.5f;
				if (count > 1) delta = min + (max - min) * (float)i / (float)(count - 1);
				delta += (UnityEngine.Random.value - 0.5f) * normalizedWidth * Terrain.GrassScatterRatio;
				delta = Mathf.Clamp(delta, min, max);

				// custom data passed to the shader
				float data_type = type * 0.01f;
				float data_amplitude = GrassTextures[type].waveAmplitude * height * 0.1f;
				Vector2 data_direction = (TerrainCurve[pointIndex + 1].position - TerrainCurve[pointIndex].position).normalized;
				float data_offset = 0; // computed later

				// direction setup
				Vector3 segmentDirection = TerrainCurve[pointIndex + 1].position - TerrainCurve[pointIndex].position;
				Vector3 segmentNormal = new Vector2(-segmentDirection.y, segmentDirection.x).normalized;

				// normals
				Vector3 topNormal = Vector3.back;
				Vector3 bottomNormal = Vector3.back;
				if (Terrain.PlasticEdges)
				{
					bottomNormal = Vector3.up;
				}

				// compute the vertices, UVs and custom data
				vertices[vertexIndex + 0] = e2dUtils.Lerp(TerrainCurve[pointIndex].position, TerrainCurve[pointIndex + 1].position, delta - 0.5f * normalizedWidth);
				normals[vertexIndex + 0] = bottomNormal;
				uvs[vertexIndex + 0] = new Vector2(0, e2dConstants.GRASS_ROOT_SIZE);
				uvs2[vertexIndex + 0] = data_direction;
				colors[vertexIndex + 0] = new Color(data_type, data_amplitude, 0, 0);

				vertices[vertexIndex + 1] = vertices[vertexIndex + 0] + height * segmentNormal;
				normals[vertexIndex + 1] = topNormal;
				uvs[vertexIndex + 1] = new Vector2(0, 1);
				uvs2[vertexIndex + 1] = data_direction;
				data_offset = totalOffset + (delta - 0.5f * normalizedWidth) * segmentLength;
				data_offset = (data_offset % (2 * 3.14f)) * 0.1f;
				colors[vertexIndex + 1] = new Color(data_type, data_amplitude, data_offset, 0);

				vertices[vertexIndex + 2] = e2dUtils.Lerp(TerrainCurve[pointIndex].position, TerrainCurve[pointIndex + 1].position, delta + 0.5f * normalizedWidth);
				normals[vertexIndex + 2] = bottomNormal;
				uvs[vertexIndex + 2] = new Vector2(1, e2dConstants.GRASS_ROOT_SIZE);
				uvs2[vertexIndex + 2] = data_direction;
				colors[vertexIndex + 2] = new Color(data_type, data_amplitude, 0, 0);

				vertices[vertexIndex + 3] = vertices[vertexIndex + 2] + height * segmentNormal;
				normals[vertexIndex + 3] = topNormal;
				uvs[vertexIndex + 3] = new Vector2(1, 1);
				uvs2[vertexIndex + 3] = data_direction;
				data_offset = totalOffset + (delta + 0.5f * normalizedWidth) * segmentLength;
				data_offset = (data_offset % (2 * 3.14f)) * 0.1f;
				colors[vertexIndex + 3] = new Color(data_type, data_amplitude, data_offset, 0);

				triangles[triangleIndex + 0] = vertexIndex;
				triangles[triangleIndex + 1] = vertexIndex + 1;
				triangles[triangleIndex + 2] = vertexIndex + 2;

				triangles[triangleIndex + 3] = vertexIndex + 1;
				triangles[triangleIndex + 4] = vertexIndex + 3;
				triangles[triangleIndex + 5] = vertexIndex + 2;

				grassIndex++;
			}

			totalOffset += segmentLength;
		}

		UnityEngine.Random.seed = oldSeed;

		// set the result to the mesh
		MeshFilter filter = transform.FindChild(e2dConstants.GRASS_MESH_NAME).GetComponent<MeshFilter>();
		filter.sharedMesh.Clear();
		filter.sharedMesh.vertices = vertices;
		filter.sharedMesh.normals = normals;
		filter.sharedMesh.uv = uvs;
		filter.sharedMesh.uv2 = uvs2;
		filter.sharedMesh.colors = colors;
		filter.sharedMesh.triangles = triangles;

		if (SomeMaterialsMissing()) RebuildMaterial();
	}

	/// Returns the number of grass pieces for the given segment.
	private int GetGrassCount(int segmentIndex)
	{
		float segmentLength = (TerrainCurve[segmentIndex].position - TerrainCurve[segmentIndex + 1].position).magnitude;
		float grassRatio = 0.5f * (TerrainCurve[segmentIndex].grassRatio + TerrainCurve[segmentIndex + 1].grassRatio);
		grassRatio = Mathf.Clamp01(grassRatio);
		int count = Mathf.RoundToInt(grassRatio * segmentLength * e2dConstants.GRASS_DENSITY_RATIO);
		if (grassRatio > 0 && count == 0) count = 1;
		return count;
	}

	/// Destroys the mesh data and prepares the object for the creation of new mesh.
	public void DestroyMesh()
	{
		EnsureMeshObjectsExist();

		MeshFilter filter = transform.FindChild(e2dConstants.GRASS_MESH_NAME).GetComponent<MeshFilter>();
		if (filter && filter.sharedMesh != null)
		{
			Object.DestroyImmediate(filter.sharedMesh);
		}
		// NOTE: can't do that since the objects are used after destroying the mesh
		//Object.DestroyImmediate(filter);

		MeshRenderer renderer = transform.FindChild(e2dConstants.GRASS_MESH_NAME).GetComponent<MeshRenderer>();
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

		// init the material
		materials = new Material[1];
		materials[0] = new Material(Shader.Find("e2d/Grass"));
		materials[0].SetFloat("_WaveFrequency", Terrain.GrassWaveSpeed);

		// texture params
		for (int i=0; i<GrassTextures.Count; i++)
		{
			materials[0].SetTexture("_Grass" + i, GrassTextures[i].texture);
		}

		// set the new materials to the renderer
		renderer.materials = materials;
	}

	/// Makes sure the curve textures and control textures are in a consistent state.
	private void EnsureTexturesInited()
	{
		if (GrassTextures.Count == 0)
		{
			GrassTextures.Add(GetDefaultGrassTexture());
		}
	}

	/// Returns the default texture used when no other is defined by the user.
	public e2dGrassTexture GetDefaultGrassTexture()
	{
		e2dGrassTexture result = new e2dGrassTexture((Texture)Resources.Load("defaultCurveTexture", typeof(Texture)));
		return result;
	}

	/// Returns true if any materials needed by the mesh are missing in the game objects.
	public bool SomeMaterialsMissing()
	{
		return transform.FindChild(e2dConstants.GRASS_MESH_NAME).GetComponent<MeshRenderer>().sharedMaterial == null;
	}

	/// Adds new texture to the end of the list.
	public void AppendGrassTexture()
	{
		GrassTextures.Add(GetDefaultGrassTexture());
	}

	/// Destroy a texture from the list at the given position.
	public void RemoveGrassTexture(int index)
	{
		GrassTextures.RemoveAt(index);
	}

#endregion

}