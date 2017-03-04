/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(e2dTerrain))]

/// Main class of the generator of the terrain. Attach it to a game object to allow it to generate the terrain data.
/// The generator can generate the terrain surface line, textures and objects (e.g. trees).
public class e2dTerrainGenerator : MonoBehaviour
{
	/// If true the terrain is destroyed and rebuilt from scratch.
	public bool FullRebuild = e2dConstants.INIT_FULL_REBUILD;
	/// Space between two nodes of the terrain (local units). The number of nodes is influenced by this value.
	public float NodeStepSize = e2dConstants.INIT_NODE_STEP_SIZE;
	/// Position of the target area of the generator (local units).
	public Vector2 TargetPosition = e2dConstants.INIT_TARGET_POSITION;
	/// Size of the target area of the generator (local units).
	public Vector2 TargetSize = e2dConstants.INIT_TARGET_SIZE;
	/// Angle of the target area. This is used to change the output of the generator.
	public float TargetAngle = e2dConstants.INIT_TARGET_ANGLE;
	/// Settings of the Perlin generator.
	public e2dCurvePerlinPreset Perlin = (e2dCurvePerlinPreset)e2dPresets.GetDefault(e2dGeneratorCurveMethod.PERLIN);
	/// Settings of the Voronoi generator.
	public e2dCurveVoronoiPreset Voronoi = (e2dCurveVoronoiPreset)e2dPresets.GetDefault(e2dGeneratorCurveMethod.VORONOI);
	/// Settings of the Midpoint generator.
	public e2dCurveMidpointPreset Midpoint = (e2dCurveMidpointPreset)e2dPresets.GetDefault(e2dGeneratorCurveMethod.MIDPOINT);
	/// Settings of the Walk generator.
	public e2dCurveWalkPreset Walk = (e2dCurveWalkPreset)e2dPresets.GetDefault(e2dGeneratorCurveMethod.WALK);
	/// List of user defined peaks to be used by the generator.
	public List<e2dGeneratorPeak> Peaks = new List<e2dGeneratorPeak>();
	/// Weights for blending the curve generator methods together.
	public float[] CurveBlendingWeights = new float[(int)e2dGeneratorCurveMethod.PEAKS];
	/// Number of iterations of the smoother.
	public int SmoothIterations;
	/// Index of the curve texture used as the cliff.
	public int CliffTextureIndex;
	/// Slope where the texture changes to cliff.
	public float CliffStartAngle;
	/// Heights of curve textures where they're supposed to appear.
	public List<float> TextureHeights;
	/// Angle when the grass stops to be generated.
	public float GrassStopAngle = e2dConstants.INIT_GRASS_STOP_ANGLE;
	/// Height range where the grass is generated.
	public Vector2 GrassHeightRange = e2dConstants.INIT_GRASS_HEIGHT_RANGE;
	/// Density of grass being generated.
	public float GrassDensity = e2dConstants.INIT_GRASS_DENSITY;


#region Properties

	private e2dTerrain mTerrain;

	/// Target component of the generator.
	public e2dTerrain Terrain 
	{ 
		get 
		{ 
			if (mTerrain == null) mTerrain = GetComponent<e2dTerrain>();
			if (mTerrain == null)
			{
				e2dUtils.Error("Can't find the terrain component.");
			}
			return mTerrain;
		} 
	}

	/// Reference to the editor object if it exists.
	[System.NonSerialized]
	public Object EditorReference;

	/// Number of nodes of the terrain to be generated.
	public int NodeCount { get { return 1 + Mathf.RoundToInt(TargetSize.x / NodeStepSize); } }

#endregion


#region Helper Methods

	/// Called when the the editor reloads the script or when the action starts.
	void OnEnable()
	{
		EditorReference = null;
	}

	/// Sets the given array of points to TerrainCurve possibly replacing some (or all) of its existing nodes.
	private void SetPointsToCurve(Vector2[] points, bool fullRebuild)
	{
		// rebuild all?
		if (fullRebuild || Terrain.TerrainCurve.Count == 0)
		{
			Terrain.AddCurvePoints(points, 0, Terrain.TerrainCurve.Count - 1);
			return;
		}

		int[] indices = GetCurveIndicesInTargetArea();

		// if the boundary doesn't cross the curve, see to which of the ends of the curve should we attach
		// the generated terrain
		if (indices[0] == -2)
		{
			float firstToLastDistSq = (Terrain.TerrainCurve[0].position - points[points.Length - 1]).sqrMagnitude;
			float lastToFirstDistSq = (Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 1].position - points[0]).sqrMagnitude;
			if (firstToLastDistSq < lastToFirstDistSq)
			{
				indices[0] = 0;
				indices[1] = -1;
			}
			else
			{
				indices[0] = Terrain.TerrainCurve.Count;
				indices[1] = Terrain.TerrainCurve.Count - 1;
			}
		}

		// set the points to the curve
		Terrain.AddCurvePoints(points, indices[0], indices[1]);
	}

	/// Returns indices of curve points contained in the target area. Returns -2 if there are no such points.
	private int[] GetCurveIndicesInTargetArea()
	{
		Vector2[] boundary = GetTargetAreaBoundary();

		// find the left index
		int first = -2;
		for (int i = 1; i < Terrain.TerrainCurve.Count; i++)
		{
			if (e2dUtils.SegmentIntersectsPolygon(Terrain.TerrainCurve[i - 1].position, Terrain.TerrainCurve[i].position, boundary))
			{
				first = i;
				break;
			}
		}

		// check the first point
		if (e2dUtils.PointInConvexPolygon(Terrain.TerrainCurve[0].position, boundary))
		{
			first = 0;
		}

		// find the right index
		int last = -2;
		for (int i = Terrain.TerrainCurve.Count - 1; i >= 1; i--)
		{
			if (e2dUtils.SegmentIntersectsPolygon(Terrain.TerrainCurve[i - 1].position, Terrain.TerrainCurve[i].position, boundary))
			{
				last = i - 1;
				break;
			}
		}

		// check the last point
		if (e2dUtils.PointInConvexPolygon(Terrain.TerrainCurve[Terrain.TerrainCurve.Count - 1].position, boundary))
		{
			last = Terrain.TerrainCurve.Count - 1;
		}

		return new int[] { first, last };
	}

#endregion


#region Target Area

	/// Finds the bounding box of the target area.
	public Rect GetTargetAreaBoundingBox()
	{
		Rect result = new Rect();
		result.xMin = float.MaxValue;
		result.yMin = float.MaxValue;
		result.xMax = float.MinValue;
		result.yMax = float.MinValue;

		foreach (Vector2 point in GetTargetAreaBoundary())
		{
			if (point.x < result.xMin) result.xMin = point.x;
			if (point.y < result.yMin) result.yMin = point.y;
			if (point.x > result.xMax) result.xMax = point.x;
			if (point.y > result.yMax) result.yMax = point.y;
		}

		return result;
	}

	/// Returns the boundary polygon of the target area in local coords of the object.
	public Vector2[] GetTargetAreaBoundary()
	{
		Vector2[] points = new Vector2[4];
		points[0] = TransformPointFromTargetArea(new Vector3(- 0.5f * TargetSize.x, - 0.5f * TargetSize.y));
		points[1] = TransformPointFromTargetArea(new Vector3(- 0.5f * TargetSize.x, + 0.5f * TargetSize.y));
		points[2] = TransformPointFromTargetArea(new Vector3(+ 0.5f * TargetSize.x, + 0.5f * TargetSize.y));
		points[3] = TransformPointFromTargetArea(new Vector3(+ 0.5f * TargetSize.x, - 0.5f * TargetSize.y));
		return points;
	}

	/// Returns rectangle representing the target area in its local space (without rotation).
	public Rect GetTargetAreaLocalBox()
	{
		return new Rect(- 0.5f * TargetSize.x, - 0.5f * TargetSize.y, TargetSize.x, TargetSize.y);
	}

	/// Transforms point in local coords of the target area into local coords of the object.
	/// Note that scale of the target area is not take into account here, so you need to
	/// normalize the coords from [0, 1] by yourself.
	public Vector3 TransformPointFromTargetArea(Vector3 point)
	{
		point = Quaternion.Euler(0, 0, TargetAngle) * point;
		point.x += TargetPosition.x;
		point.y += TargetPosition.y;
		return point;
	}

	/// Transforms point in local coords of the object into local coords of the target area.
	/// Note that scale of the target area is not take into account here, so you need to
	/// normalize the coords into [0, 1] by yourself.
	public Vector3 TransformPointIntoTargetArea(Vector3 point)
	{
		point.x -= TargetPosition.x;
		point.y -= TargetPosition.y;
		point = Quaternion.Euler(0, 0, -TargetAngle) * point;
		return point;
	}

	/// Makes sure the target area parameters are valid.
	public void FixTargetArea()
	{
		// fix the bounds
		TargetSize.x = Mathf.Max(TargetSize.x, float.Epsilon);
		TargetSize.y = Mathf.Max(TargetSize.y, float.Epsilon);

		// now fix the peaks so that they're still inside the target area
		Rect box = GetTargetAreaLocalBox();
		for (int i=0; i<Peaks.Count; i++)
		{
			Vector3 p = Peaks[i].position;
			if (p.x < box.xMin) p.x = box.xMin;
			if (p.y < box.yMin) p.y = box.yMin;
			if (p.x > box.xMax) p.x = box.xMax;
			if (p.y > box.yMax) p.y = box.yMax;
			Peaks[i].position = p;
		}
	}

#endregion


#region Curve Generation

	/// Generates the terrain surface curve (TerrainCurve) using all methods blending them together.
	/// The algorithm is controlled by variables: Voronoi, Perlin, Midpoint, Walk, Peaks.
	/// See e2dGeneratorCurveMethod for details.
	public void GenerateCurve()
	{
		float[] debugCurve = null;
		GenerateCurve(ref debugCurve);
	}

	/// Generates the terrain surface curve (TerrainCurve) using all methods blending them together.
	/// The algorithm is controlled by variables: Voronoi, Perlin, Midpoint, Walk, Peaks.
	/// The debugHeightmap parameter is used for debug to display the generated curve (with higher precision).
	/// See e2dGeneratorCurveMethod for details.
	public void GenerateCurve(ref float[] debugHeightmap)
	{
		// checks
		if (NodeCount < 2)
		{
			e2dUtils.Error("Map too small for generating.");
			return;
		}

		// take a look at the weights and fix them if needed
		float totalWeight = 0;
		foreach (float w in CurveBlendingWeights) totalWeight += w;

		// init the heightmap
		float[] heightMap = new float[NodeCount];
		float[] tmpMap = new float[NodeCount];
		for (int i = 0; i < NodeCount; i++) heightMap[i] = 0.5f;
		bool normalize;
		bool normalizeAfterBlend = true;

		// fill the heightmap
		for (int method=0; method<(int)e2dGeneratorCurveMethod.PEAKS; method++)
		{
			if (totalWeight == 0) continue;
			if (CurveBlendingWeights[method] == 0) continue;

			if (e2dUtils.DEBUG_FIXED_GENERATOR_SEED)
			{
				UnityEngine.Random.seed = 12345;
			}

			switch ((e2dGeneratorCurveMethod)method)
			{
				case e2dGeneratorCurveMethod.PERLIN:
					normalize = GenerateCurvePerlin(tmpMap, GetTargetAreaLocalBox(), ref debugHeightmap);
					if (normalize) NormalizeHeightmap(tmpMap); // needs renormalizing before being mixed
					if (!normalize) normalizeAfterBlend = false;
					break;
				case e2dGeneratorCurveMethod.VORONOI:
					normalize = GenerateCurveVoronoi(tmpMap, GetTargetAreaLocalBox(), ref debugHeightmap);
					// no need to normalize Voronoi
					if (!normalize) normalizeAfterBlend = false;
					break;
				case e2dGeneratorCurveMethod.MIDPOINT:
					normalize = GenerateCurveMidpoint(tmpMap, GetTargetAreaLocalBox(), ref debugHeightmap);
					if (normalize) NormalizeHeightmap(tmpMap); // needs renormalizing before being mixed
					if (!normalize) normalizeAfterBlend = false;
					break;
				case e2dGeneratorCurveMethod.WALK:
					normalize = GenerateCurveWalk(tmpMap, GetTargetAreaLocalBox(), ref debugHeightmap);
					if (normalize) NormalizeHeightmap(tmpMap); // needs renormalizing before being mixed
					if (!normalize) normalizeAfterBlend = false;
					break;
			}

			// blend the heights
			for (int i=0; i<heightMap.Length; i++)
			{
				heightMap[i] += (tmpMap[i] - 0.5f) * CurveBlendingWeights[method] / totalWeight;
			}
		}

		// normalize the heightmap, so that the height is actually what was requested
		if (normalizeAfterBlend)
		{
			NormalizeHeightmap(heightMap);
		}

		// create the curve points
		Vector2[] targetArea = GetTargetAreaBoundary();
		Vector2[] points = new Vector2[NodeCount];
		for (int i = 0; i < NodeCount; i++)
		{
			float r = (float)i / (float)(NodeCount - 1);
			points[i] = (1 - r) * targetArea[0] + r * targetArea[3];
			points[i] += heightMap[i] * (targetArea[1] - targetArea[0]);
		}
		
		// set the points to the curve
		SetPointsToCurve(points, FullRebuild);

		// rebuild the terrain
		Terrain.CurveClosed = false;
		Terrain.FixCurve();
		Terrain.FixBoundary();
		Terrain.RebuildAllMaterials();
		Terrain.RebuildAllMeshes();
	}

	/// Normalizes the heightmap so that its lowest point lies at 0 and the highest point lies at 1.
	private void NormalizeHeightmap(float[] heightMap)
	{
		// find the extremes
		float minHeight = float.MaxValue;
		float maxHeight = float.MinValue;
		foreach (float value in heightMap)
		{
			if (value < minHeight) minHeight = value;
			if (value > maxHeight) maxHeight = value;
		}

		// prevent division by zero
		if (maxHeight - minHeight <= float.Epsilon) return;

		// adjust the heights
		for (int i=0; i<heightMap.Length; i++)
		{
			heightMap[i] = (heightMap[i] - minHeight) / (maxHeight - minHeight);
		}
	}

	/// Initializes the array with peaks and generates more peaks if necessary.
	private List<Vector2> PreparePeaks(int totalPeakCount, float peakRatio, bool includeCustomPeaks, Rect targetRect)
	{
		// init the peaks
		List<Vector2> peaks = new List<Vector2>();
		totalPeakCount = Mathf.Max(totalPeakCount, 2);
		bool normalizeRandomPeaks = true;
		if (includeCustomPeaks)
		{
			foreach (e2dGeneratorPeak peak in Peaks)
			{
				float x = (peak.position.x - targetRect.xMin) / targetRect.width;
				float y = (peak.position.y - targetRect.yMin) / targetRect.height;
				peaks.Add(new Vector2(x, y));

				// if any of the user generated peaks is almost at the top of the target area we don't need
				// to normalize peaks generated by us
				if (Mathf.Approximately(y, 1)) normalizeRandomPeaks = false;
			}
		}
		while (peaks.Count < totalPeakCount)
		{
			float y = 0;
			if (UnityEngine.Random.value <= peakRatio) y = UnityEngine.Random.value;
			peaks.Add(new Vector2(UnityEngine.Random.value, y));
		}

		// normalize random generated peaks so that we have peaks reaching to the ceiling of the target area
		if (normalizeRandomPeaks)
		{
			float maxHeight = float.MinValue;
			int customPeakCount = includeCustomPeaks ? Peaks.Count : 0;
			for (int i = customPeakCount; i < peaks.Count; i++)
			{
				if (peaks[i].y > maxHeight) maxHeight = peaks[i].y;
			}
			if (maxHeight > float.Epsilon)
			{
				for (int i = customPeakCount; i < peaks.Count; i++)
				{
					Vector2 peak = peaks[i];
					peak.y /= maxHeight;
					peaks[i] = peak;
				}
			}
		}

		return peaks;
	}

#endregion


#region Curve Generation Methods

	/// Produces a heightmap using the Perlin noise generator.
	/// Returns true if the map must be renormalized.
	/// The debugHeightmap parameter is used for debug to display the generated curve (with higher precision).
	/// See e2dGeneratorCurveMethod for details.
	private bool GenerateCurvePerlin(float[] heightMap, Rect targetRect, ref float[] debugHeightmap)
	{
		// init the noise
		int frequency = 1 + Mathf.RoundToInt(Perlin.frequencyPerUnit * targetRect.width);
		frequency = Mathf.Max(frequency, 2);
		e2dPerlinNoise function = new e2dPerlinNoise(Perlin.octaves, 1.0f, frequency, Perlin.persistence);

		// produce some values
		function.Regenerate();

		// fill the heightmap
		for (int i = 0; i < heightMap.Length; i ++)
		{
			float x = (float)i / (float)(heightMap.Length - 1);
			heightMap[i] = function.GetValue(x);
		}

		// fill the heightmap for debug
		if (e2dUtils.DEBUG_GENERATOR_CURVE)
		{
			debugHeightmap = new float[10 * heightMap.Length];
			for (int i = 0; i < debugHeightmap.Length; i++)
			{
				float x = (float)i / (float)(debugHeightmap.Length - 1);
				debugHeightmap[i] = function.GetValue(x) * targetRect.height;
			}
		}

		return true;
	}

	/// Produces a heightmap using the Voronoi generator.
	/// Returns true if the map must be renormalized.
	/// The debugHeightmap parameter is used for debug to display the generated curve (with higher precision).
	/// See e2dGeneratorCurveMethod for details.
	private bool GenerateCurveVoronoi(float[] heightMap, Rect targetRect, ref float[] debugHeightmap)
	{
		// init the peaks
		int peakCount = Mathf.RoundToInt(Voronoi.frequencyPerUnit * targetRect.width);
		List<Vector2> peaks = PreparePeaks(peakCount, Voronoi.peakRatio, Voronoi.usePeaks, targetRect);

		// init the function
		e2dVoronoi function = new e2dVoronoi(peaks, Voronoi.peakType, Voronoi.peakWidth);

		// produce heights
		for (int i=0; i<heightMap.Length; i++)
		{
			float x = (float)i / (float)(heightMap.Length - 1);
			heightMap[i] = function.GetValue(x);
		}

		// fill the heightmap for debug
		if (e2dUtils.DEBUG_GENERATOR_CURVE)
		{
			debugHeightmap = new float[10 * heightMap.Length];
			for (int i = 0; i < debugHeightmap.Length; i++)
			{
				float x = (float)i / (float)(debugHeightmap.Length - 1);
				debugHeightmap[i] = function.GetValue(x) * targetRect.height;
			}
		}

		return !Voronoi.usePeaks;
	}

	/// Produces a heightmap using the Midpoint generator.
	/// Returns true if the map must be renormalized.
	/// The debugHeightmap parameter is used for debug to display the generated curve (with higher precision).
	/// See e2dGeneratorCurveMethod for details.
	private bool GenerateCurveMidpoint(float[] heightMap, Rect targetRect, ref float[] debugHeightmap)
	{
		// prepare and normalize the custom peaks
		List<Vector2> peaks = null;
		if (Midpoint.usePeaks)
		{
			peaks = PreparePeaks(Peaks.Count, 0, true, targetRect);
		}

		// init the function
		int step = Mathf.RoundToInt(heightMap.Length / (Midpoint.frequencyPerUnit * targetRect.width));
		e2dMidpoint function = new e2dMidpoint(heightMap.Length, step, Midpoint.roughness, peaks);

		// generate values
		function.Regenerate();

		// fill the heightmap
		for (int i=0; i<heightMap.Length; i++)
		{
			heightMap[i] = function.GetValueAt(i);
			if (Midpoint.usePeaks)
			{
				heightMap[i] = Mathf.Clamp01(heightMap[i]);
			}
		}

		// fill the heightmap for debug
		if (e2dUtils.DEBUG_GENERATOR_CURVE)
		{
			debugHeightmap = new float[heightMap.Length];
			for (int i=0; i<heightMap.Length; i++)
			{
				debugHeightmap[i] = function.GetValueAt(i) * targetRect.height;
			}
		}

		return !Midpoint.usePeaks;
	}

	/// Produces a heightmap using the Walk generator.
	/// Returns true if the map must be renormalized.
	/// The debugHeightmap parameter is used for debug to display the generated curve (with higher precision).
	/// See e2dGeneratorCurveMethod for details.
	private bool GenerateCurveWalk(float[] heightMap, Rect targetRect, ref float[] debugHeightmap)
	{
		float cellWidth = 1.0f / (heightMap.Length - 1);
		float actualCellWidth = targetRect.width / (heightMap.Length - 1);
		float turnDistance = 1.0f / (Walk.frequencyPerUnit * targetRect.width);
		turnDistance = Mathf.Clamp01(turnDistance);
		
		// start
		float height = 0.5f;
		float angle = 0;
		float minHeight = height;
		float maxHeight = height;
		heightMap[0] = height;

		// walk
		for (int i=1; i<heightMap.Length; i++)
		{
			// find the distance to the nearest turning point
			float position = (float)i / (float)(heightMap.Length - 1); // [0, 1]
			float delta = Mathf.Min(position % turnDistance, turnDistance - position % turnDistance);
			
			// normalize the distance
			delta = 1 - delta * 2 / turnDistance;

			// skew the distance using some suitable function
			delta = Mathf.Pow(delta, 10);

			// compute the change of the angle based on the distance from the turning point
			float angleDelta = delta * Walk.angleChangePerUnit * actualCellWidth * (2 * UnityEngine.Random.value - 1);
			angle = Mathf.Repeat(angle + angleDelta + 180, 360) - 180; // this brings it into [-180, 180]
			angle = Mathf.Clamp(angle, -80, 80);

			// compute the height displacement using the current angle
			float displacement = Mathf.Tan(angle * Mathf.Deg2Rad) * cellWidth;
			height += displacement;

			// check the cohesion - if the height is not too low or high
			float heightLimit = (e2dConstants.WALK_COHESION_MAX - Walk.cohesionPerUnit) * i * cellWidth * 0.5f;
			float clampedHeight = Mathf.Clamp(height, maxHeight - heightLimit, minHeight + heightLimit);
			if (height != clampedHeight) angle = 0; // reset the angle when we limit the height to avoid flat areas
			height = clampedHeight;

			// update the current range
			minHeight = Mathf.Min(minHeight, height);
			maxHeight = Mathf.Max(maxHeight, height);

			heightMap[i] = height;
		}

		// fill the heightmap for debug
		if (e2dUtils.DEBUG_GENERATOR_CURVE)
		{
			debugHeightmap = new float[heightMap.Length];
			for (int i = 0; i < heightMap.Length; i++)
			{
				debugHeightmap[i] = heightMap[i] * targetRect.height;
			}
		}

		return true;
	}

#endregion


#region Smoothing

	/// Smoothes the terrain surface curve (TerrainCurve) using an averaging method.
	/// The smoothing is controlled by parameters: SmoothIterations.
	public void SmoothCurve()
	{
		// init variables
		int[] indices = GetCurveIndicesInTargetArea();
		int nodeCount = indices[1] - indices[0] + 1;
		if (indices[0] == -2 || nodeCount < 2) return; // no curve points to smooth

		// grab the heights from the curve in the target area
		float[] heightMap = new float[nodeCount];
		float[] tmpMap = new float[nodeCount];
		for (int i=0; i<nodeCount; i++)
		{
			Vector2 point = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i].position);
			heightMap[i] = point.y;
		}

		// smoothing
		for (int iteration = 0; iteration < SmoothIterations; iteration++)
		{
			for (int i=1; i<nodeCount-1; i++)
			{
				float average = 0.5f * (heightMap[i - 1] + heightMap[i + 1]);
				average = 0.5f * (average + heightMap[i]);
				tmpMap[i] = average;
			}

			for (int i=1; i<nodeCount-1; i++)
			{
				heightMap[i] = tmpMap[i];
			}
		}

		// update the curve points
		for (int i=0; i<nodeCount; i++)
		{
			Vector2 point = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i].position);
			point.y = heightMap[i];
			Terrain.TerrainCurve[indices[0] + i].position = TransformPointFromTargetArea(point);
		}

		// rebuild the terrain
		Terrain.FixCurve();
		Terrain.FixBoundary();
		Terrain.RebuildAllMaterials();
		Terrain.RebuildAllMeshes();
	}

#endregion


#region Texturing

	/// Comparer of two height values.
	private class HeightComparer : IComparer<KeyValuePair<float, int>>
	{
		/// Compares two height values.
		public int Compare(KeyValuePair<float, int> x, KeyValuePair<float, int> y)
		{
			return x.Key < y.Key ? -1 : x.Key > y.Key ? 1 : 0;
		}
	}

	/// Generates texturing for the terrain curve based on heights and slopes of the curve.
	/// The texturing algorithm is controlled by parameters: TextureHeights, CliffStartAngle, CliffTextureIndex.
	public void TextureTerrain()
	{
		// checks
		if (TextureHeights.Count != Terrain.CurveTextures.Count)
		{
			e2dUtils.Error("TextureTerrain: TextureHeights and CurveTextures have different number of elements");
			return;
		}

		// normalize the heights
		List<KeyValuePair<float,int>> normalizedHeights = new List<KeyValuePair<float,int>>();
		float max = float.MinValue;
		foreach (float h in TextureHeights) max = Mathf.Max(max, h);
		if (max <= 0)
		{
			e2dUtils.Error("TextureTerrain: no texture heights defined above ground");
			return;
		}
		for (int i=0; i<TextureHeights.Count; i++)
		{
			if (TextureHeights[i] > 0)
			{
				normalizedHeights.Add(new KeyValuePair<float,int>(TextureHeights[i] / max, i));
			}
		}
		normalizedHeights.Sort(new HeightComparer());

		// init variables
		int[] indices = GetCurveIndicesInTargetArea();
		int nodeCount = indices[1] - indices[0] + 1;
		if (indices[0] == -2 || nodeCount < 1) return; // no curve points to process

		// go through the nodes and decide on their textures
		Rect targetArea = GetTargetAreaLocalBox();
		for (int i = 0; i < nodeCount; i++)
		{
			Vector2 point = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i].position);

			// decide based on the height of the node
			float height = (point.y + 0.5f * targetArea.height) / targetArea.height;
			foreach (KeyValuePair<float, int> textureHeight in normalizedHeights)
			{
				if (height <= textureHeight.Key)
				{
					Terrain.TerrainCurve[indices[0] + i].texture = textureHeight.Value;
					break;
				}
			}

			// take the slope into account
			if (i > 0 && i < nodeCount - 1)
			{
				Vector2 prev = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i - 1].position);
				Vector2 next = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i + 1].position);
				float angle1 = Mathf.Atan2(point.y - prev.y, point.x - prev.x) * Mathf.Rad2Deg;
				float angle2 = Mathf.Atan2(next.y - point.y, next.x - point.x) * Mathf.Rad2Deg;
				if ((angle1 >= CliffStartAngle && angle2 >= CliffStartAngle)
					|| (angle1 <= -CliffStartAngle && angle2 <= -CliffStartAngle))
				{
					Terrain.TerrainCurve[indices[0] + i].texture = CliffTextureIndex;
				}
			}
		}

		// rebuild the terrain
		Terrain.RebuildAllMaterials();
		Terrain.RebuildAllMeshes();
	}

#endregion


#region Grass Generation

	/// Generates grass on the terrain curve based on heights and slopes of the curve.
	/// The algorithm is controlled by parameters: GrassStopAngle, GrassHeightRange, GrassDensity.
	public void GenerateGrass()
	{
		// init variables
		int[] indices = GetCurveIndicesInTargetArea();
		int nodeCount = indices[1] - indices[0] + 1;
		if (indices[0] == -2 || nodeCount < 1) return; // no curve points to process

		// go through the nodes and decide on the grass
		Rect targetArea = GetTargetAreaLocalBox();
		for (int i = 0; i < nodeCount; i++)
		{
			Vector2 point = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i].position);

			// decide based on the height of the node
			float height = (point.y + 0.5f * targetArea.height) / targetArea.height;
			if (height >= GrassHeightRange.x && height <= GrassHeightRange.y)
			{
				Terrain.TerrainCurve[indices[0] + i].grassRatio = GrassDensity;
			}
			else
			{
				Terrain.TerrainCurve[indices[0] + i].grassRatio = 0;
			}

			// take the slope into account
			if (i > 0 && i < nodeCount - 1)
			{
				Vector2 prev = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i - 1].position);
				Vector2 next = TransformPointIntoTargetArea(Terrain.TerrainCurve[indices[0] + i + 1].position);
				float angle1 = Mathf.Atan2(point.y - prev.y, point.x - prev.x) * Mathf.Rad2Deg;
				float angle2 = Mathf.Atan2(next.y - point.y, next.x - point.x) * Mathf.Rad2Deg;
				if ((angle1 >= GrassStopAngle && angle2 >= GrassStopAngle)
					|| (angle1 <= -GrassStopAngle && angle2 <= -GrassStopAngle))
				{
					Terrain.TerrainCurve[indices[0] + i].grassRatio = 0;
				}
			}
		}

		// rebuild the terrain
		Terrain.RebuildAllMaterials();
		Terrain.RebuildAllMeshes();
	}

#endregion

}