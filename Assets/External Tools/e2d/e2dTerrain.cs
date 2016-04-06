/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode()]

/// This is the main class of the library. Attach it to a game object so that it can represent the terrain in the game.
/// It holds the data representing the terrain and takes care about the internal data structures to be generated
/// for Unity.
///
/// The terrain is represented by a polygonal chain called "surface curve" describing the surface of the terrain.
/// That is filled under the chain to fill the predefined rectangular border. The chain can be of any shape creating
/// caves or ceilings if necessary. But it must not be intersecting itself.
/// 
/// According to the chain the meshes are generated. The are separated into four sub-objects: curve mesh, grass mesh,
/// fill mesh and collider mesh. The curve mesh represents the thin stripe attached to the surface of the terrain.
/// It is the upper part of the terrain consisting of a grass or sand texture. The grass mesh consists of many clumps
/// of grass quads sitting on top of the terrain surface. The fill mesh fills the area from the surface to the boundary
/// of the terrain. The collider mesh is the physical representation of the terrain in the physics subsystem.
public class e2dTerrain : MonoBehaviour
{
	/// Polygonal chain representing the surface of the terrain.
	public List<e2dCurveNode> TerrainCurve = new List<e2dCurveNode>();
	/// Rectangle defining the edges (boundary) of the terrain.
	public Rect TerrainBoundary = new Rect(0, 0, 0, 0);
	/// Texture used to fill the terrain from the curve to the boundary.
	public Texture FillTexture;
	/// Width of one tile of FillTexture (world units).
	public float FillTextureTileWidth = e2dConstants.INIT_FILL_TEXTURE_WIDTH;
	/// Height of one tile of FillTexture (world units).
	public float FillTextureTileHeight = e2dConstants.INIT_FILL_TEXTURE_HEIGHT;
	/// X offset of FillTexture (world units).
	public float FillTextureTileOffsetX = e2dConstants.INIT_FILL_TEXTURE_OFFSET_X;
	/// Y offset of FillTexture (world units).
	public float FillTextureTileOffsetY = e2dConstants.INIT_FILL_TEXTURE_OFFSET_Y;
	/// If true the surface curve is closed (the position of the last point is forced to be the same as the position
	/// of the first point). The terrain is then filled only within the surface curve. The terrain looks like an island.
	public bool CurveClosed = e2dConstants.INIT_CURVE_CLOSED;
	/// Textures used for the stripe near the terrain surface.
	/// Note: they must reside here in order to be serialized by Unity.
	public List<e2dCurveTexture> CurveTextures = new List<e2dCurveTexture>();
	/// Textures used for grass.
	/// Note: they must reside here in order to be serialized by Unity.
	public List<e2dGrassTexture> GrassTextures = new List<e2dGrassTexture>();
	/// Influences the speed of grass waving.
	public float GrassWaveSpeed = e2dConstants.INIT_GRASS_WAVE_SPEED;
	/// Influences the amount of random scattering.
	public float GrassScatterRatio = e2dConstants.INIT_GRASS_SCATTER_RATIO;
	/// If true the edges of the terrain surface have modified normals to look more plastic.
	public bool PlasticEdges = true;


	// private vars
	private e2dTerrainFillMesh mFillMesh;
	private e2dTerrainCurveMesh mCurveMesh;
	private e2dTerrainGrassMesh mGrassMesh;
	private e2dTerrainColliderMesh mColliderMesh;
	private e2dTerrainBoundary mBoundary;
	private bool mCurveIntercrossing;
	

#region Properties

	/// True if the terrain can be edited by the tools.
	public bool IsEditable { get { return TerrainCurve != null && TerrainCurve.Count >= 2; } }

	/// Object taking care of the boundary data.
	public e2dTerrainBoundary Boundary { get { return mBoundary; } }

	/// Object taking care of the mesh data of the terrain surface stripe.
	public e2dTerrainCurveMesh CurveMesh { get { return mCurveMesh; } }

	/// Object taking care of the mesh data of the grass.
	public e2dTerrainGrassMesh GrassMesh { get { return mGrassMesh; } }

	/// Object taking care of the mesh filling the area from the surface to the boundary of the terrain.
	public e2dTerrainFillMesh FillMesh { get { return mFillMesh; } }

	/// Object taking care of the mesh serving as a physics collider.
	public e2dTerrainColliderMesh ColliderMesh { get { return mColliderMesh; } }

	/// True if the curve is currently intersecting itself.
	public bool CurveIntercrossing { get { return mCurveIntercrossing; } }

	/// Reference to the editor object if it exists.
	[System.NonSerialized]
	public Object EditorReference;

#endregion


#region Events

	/// Called when the the editor reloads the script or when the action starts.
	void OnEnable()
	{
		EditorReference = null;

		mBoundary = new e2dTerrainBoundary(this);
		mFillMesh = new e2dTerrainFillMesh(this);
		mCurveMesh = new e2dTerrainCurveMesh(this);
		mGrassMesh = new e2dTerrainGrassMesh(this);
		mColliderMesh = new e2dTerrainColliderMesh(this);

		if (!mFillMesh.IsMeshValid() || (e2dUtils.DEBUG_REBUILD_ON_ENABLE && !Application.isPlaying))
		{
			FixCurve();
			FixBoundary();
			RebuildAllMaterials();
			RebuildAllMeshes();
		}
		else
		{
			// make sure the curve control textures are ready
			CurveMesh.UpdateControlTextures(true);
		}
	}

	/// Called when the script is unloaded.
	void OnDisable()
	{
		mCurveMesh.DestroyTemporaryAssets();
	}

	/// Clears everything and prepares the terrain for new data.
	public void Reset()
	{
		TerrainCurve.Clear();
		TerrainBoundary = new Rect(0, 0, 0, 0);
		mFillMesh.DestroyMesh();
		mCurveMesh.DestroyMesh();
		mGrassMesh.DestroyMesh();
		mColliderMesh.DestroyMesh();
	}

#endregion


#region Curve

	/// Maximum number of nodes of the terrain curve.
	public int GetMaxNodesCount()
	{
		return 8192; // limited by control texture size limit
	}

	/// Adds new point to the curve. The new node will have the default texture.
	public void AddPointOnCurve(int beforeWhichIndex, Vector2 toAdd)
	{
		e2dCurveNode node = new e2dCurveNode(toAdd);
		if (beforeWhichIndex > 0) node.texture = TerrainCurve[beforeWhichIndex - 1].texture;
		else if (beforeWhichIndex < TerrainCurve.Count) node.texture = TerrainCurve[beforeWhichIndex].texture;
		TerrainCurve.Insert(beforeWhichIndex, node);
		mCurveMesh.UpdateControlTextures();
	}

	/// Removes one point from the curve. If moveTheRest is true the rest of the nodes after the deleted one will be
	/// shifted, so that the first node after the deleted one is at the position of the deleted one.
	public void RemovePointOnCurve(int index, bool moveTheRest)
	{
		Vector2 delta = Vector2.zero;
		if (index < TerrainCurve.Count-1)
		{
			delta = TerrainCurve[index + 1].position - TerrainCurve[index].position;
		}
		TerrainCurve.RemoveAt(index);
		if (moveTheRest)
		{
			for (int i=index; i<TerrainCurve.Count; i++)
			{
				TerrainCurve[i].position = TerrainCurve[i].position - delta;
			}
		}

		mCurveMesh.UpdateControlTextures();
	}

	/// Adds a list of new points to the curve. The interval of nodes [firstToReplace, lastToReplace] is deleted and
	/// the new points are inserted at their position.
	public void AddCurvePoints(Vector2[] points, int firstToReplace, int lastToReplace)
	{
		int targetNodeCount = TerrainCurve.Count + points.Length - (lastToReplace - firstToReplace + 1);
		if (targetNodeCount > GetMaxNodesCount())
		{
			e2dUtils.Error("Too many nodes for the terrain.");
			return;
		}

		TerrainCurve.RemoveRange(firstToReplace, lastToReplace - firstToReplace + 1);
		TerrainCurve.Capacity = TerrainCurve.Count + points.Length;

		int addIndex = firstToReplace;
		foreach (Vector2 point in points) TerrainCurve.Insert(addIndex++, new e2dCurveNode(point));

		mCurveMesh.UpdateControlTextures();
	}

	/// Fixes the curve if needed and detects any problems with it, so that they can be reported to the user.
	public void FixCurve()
	{
		// fix NaNs
		for (int i=0; i<TerrainCurve.Count; i++)
		{
			if (float.IsNaN(TerrainCurve[i].position.x)) TerrainCurve[i].position.x = 0;
			if (float.IsNaN(TerrainCurve[i].position.y)) TerrainCurve[i].position.y = 0;
		}

		// fix the endings
		if (TerrainCurve.Count >= 3 && !CurveClosed && TerrainCurve[TerrainCurve.Count - 1] == TerrainCurve[0])
		{
			Vector2 delta = TerrainCurve[TerrainCurve.Count - 1].position - TerrainCurve[TerrainCurve.Count - 2].position;
			TerrainCurve[TerrainCurve.Count - 1].position -= 0.5f * delta;
		}

		// fix the endings
		if (TerrainCurve.Count >= 3 && CurveClosed)
		{
			TerrainCurve[TerrainCurve.Count - 1].Copy(TerrainCurve[0]);
		}

		if (e2dConstants.CHECK_CURVE_INTERCROSSING)
		{
			// detect curve intersections
			mCurveIntercrossing = false;
			int count = TerrainCurve.Count;
			if (CurveClosed) count--;
			for (int i = 3; i < count; i++)
			{
				if (IntersectsCurve(0, i - 2, TerrainCurve[i - 1].position, TerrainCurve[i].position))
				{
					mCurveIntercrossing = true;
				}
			}
		}
	}

	/// Fixes the boundary of the terrain if necessary.
	public void FixBoundary()
	{
		Boundary.FixBoundary();
	}

	/// True if the segment (a, b) intersects the curve anywhere from startIndex to endIndex.
	public bool IntersectsCurve(int startIndex, int endIndex, Vector2 a, Vector2 b)
	{
		for (int i=startIndex; i<endIndex; i++)
		{
			if (e2dUtils.SegmentsIntersect(a, b, TerrainCurve[i].position, TerrainCurve[i + 1].position)) return true;
		}

		return false;
	}

#endregion


#region Meshes

	/// Deletes all meshes and creates them again.
	public void RebuildAllMeshes()
	{
		mFillMesh.RebuildMesh();
		mCurveMesh.RebuildMesh();
		mGrassMesh.RebuildMesh();
		mColliderMesh.RebuildMesh();
	}
	
	/// Deletes all materials and creates them again.
	public void RebuildAllMaterials()
	{
		mFillMesh.RebuildMaterial();
		mCurveMesh.UpdateControlTextures(true);
		mCurveMesh.RebuildMaterial();
		mGrassMesh.RebuildMaterial();
	}

#endregion

}
