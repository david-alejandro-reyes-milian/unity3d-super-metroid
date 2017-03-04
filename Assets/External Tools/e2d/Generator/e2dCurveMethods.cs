/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;

/// Different methods used for the generation of the terrain surface curve (TerrainCurve).
public enum e2dGeneratorCurveMethod { 
	/// Generates a heightmap using the Perlin noise and then creates the curve according to the heightmap.
	/// See http://freespace.virgin.net/hugo.elias/models/m_perlin.htm for details.
	PERLIN = 0, 
	/// Generates a number of peak points (some are set to 0). Then computes valleys between the peaks based
	/// on the distance between the peaks. Then interpolates values between the peak and the valley using
	/// a specified function (linear, sinus, etc.). It is possible to predefine peaks to be used by the generator.
	MIDPOINT,
	/// The heights are generated as a random walk originating at the left edge of the target area. The walk
	/// is directed by parameters controlling the slope of the terrain.
	VORONOI,
	/// Generates heights using the midpoint method. First some values are randomly seeded and then recursively
	/// values in between of those already generated are computed as the average of their neighbors plus some
	/// random displacement. The displacement random range is reduced at each level. It is possible to predefine
	/// peaks and the generator will use their values to seed the first level of the heights.
	WALK,
	/// User defined peaks that the generator tries to satisfy.
	PEAKS,
	
	NUM_METHODS 
}

/// Static class holding all preset data for the generator.
public class e2dPresets
{
	/// Returns the array of presets of the specified generator method.
	public static e2dPreset[] GetCurvePresets(e2dGeneratorCurveMethod method)
	{
		if (sCurvePresets == null)
		{
			sCurvePresets = new e2dPreset[(int)e2dGeneratorCurveMethod.NUM_METHODS][];

			sCurvePresets[(int)e2dGeneratorCurveMethod.PERLIN] = new e2dPreset[]
			{
				new e2dCurvePerlinPreset()
				{
					name = "Rolling Hills",
					octaves = 3,
					frequencyPerUnit = 0.02f,
					persistence = 0.3f
				} ,
				new e2dCurvePerlinPreset()
				{
					name = "Jagged Plains",
					octaves = 11,
					frequencyPerUnit = 0.001f,
					persistence = 0.51f
				},
				new e2dCurvePerlinPreset()
				{
					name = "Spiky Mountains",
					octaves = 2,
					frequencyPerUnit = 0.117f,
					persistence = 0.155f
				},
			};

			sCurvePresets[(int)e2dGeneratorCurveMethod.VORONOI] = new e2dPreset[]
			{
				new e2dCurveVoronoiPreset()
				{
					name = "Rolling Hills",
					frequencyPerUnit = 0.025f,
					peakRatio = 0.755f,
					peakWidth = 0.210f,
					peakType = e2dVoronoiPeakType.SINE
				},
				new e2dCurveVoronoiPreset()
				{
					name = "Highland Plateaus",
					frequencyPerUnit = 0.012f,
					peakRatio = 0.664f,
					peakWidth = 0.210f,
					peakType = e2dVoronoiPeakType.QUADRATIC
				},
				new e2dCurveVoronoiPreset()
				{
					name = "Scattered Mountains",
					frequencyPerUnit = 0.037f,
					peakRatio = 0.237f,
					peakWidth = 0.210f,
					peakType = e2dVoronoiPeakType.SINE
				},
			};

			sCurvePresets[(int)e2dGeneratorCurveMethod.MIDPOINT] = new e2dPreset[]
			{
				new e2dCurveMidpointPreset()
				{
					name = "Spiky Hills",
					frequencyPerUnit = 0.025f,
					roughness = 0.082f
				},
				new e2dCurveMidpointPreset()
				{
					name = "Jagged Plains",
					frequencyPerUnit = 0.001f,
					roughness = 0.709f
				},
				new e2dCurveMidpointPreset()
				{
					name = "Jagged Mountains",
					frequencyPerUnit = 0.084f,
					roughness = 0.327f
				},
			};

			sCurvePresets[(int)e2dGeneratorCurveMethod.WALK] = new e2dPreset[]
			{
				new e2dCurveWalkPreset()
				{
					name = "Rolling Hills",
					frequencyPerUnit = 0.5f,
					angleChangePerUnit = 56,
					cohesionPerUnit = 0.855f
				},
				new e2dCurveWalkPreset()
				{
					name = "Large Plains",
					frequencyPerUnit = 0.269f,
					angleChangePerUnit = 39,
					cohesionPerUnit = 2
				},
			};
		}

		return sCurvePresets[(int)method];
	}

	/// Returns the default preset for the given method. This preset will be used when the component is first created.
	public static e2dPreset GetDefault(e2dGeneratorCurveMethod method)
	{
		return GetCurvePresets(method)[0].Clone();
	}

	private static e2dPreset[][] sCurvePresets;
}

/// Preset definition for the Perlin noise curve generator.
/// See http://freespace.virgin.net/hugo.elias/models/m_perlin.htm for details on the parameters.
[System.Serializable]
public class e2dCurvePerlinPreset : e2dPreset
{
	/// Defines the amount of detail.
	public int octaves;
	/// Scale of the generated heights along the X axis.
	public float frequencyPerUnit;
	/// Roughness of the terrain.
	public float persistence;

	/// Copies data from another preset. Assumes both presets are of the same kind.
	public override void Copy(e2dPreset other)
	{
		e2dCurvePerlinPreset preset = (e2dCurvePerlinPreset)other;
		octaves = preset.octaves;
		frequencyPerUnit = preset.frequencyPerUnit;
		persistence = preset.persistence;
	}

	/// Updates the parameters of the generator based on the settings of the preset.
	public override void UpdateValues(e2dTerrainGenerator generator)
	{
		generator.Perlin.Copy(this);
	}

	/// Clones the instance.
	public override e2dPreset Clone()
	{
		e2dCurvePerlinPreset result = new e2dCurvePerlinPreset();
		result.Copy(this);
		return result;
	}
}

/// Type of peaks of the Voronoi generator.
public enum e2dVoronoiPeakType { LINEAR, SINE, QUADRATIC };

/// Preset definition for the Voronoi curve generator.
[System.Serializable]
public class e2dCurveVoronoiPreset : e2dPreset
{
	/// Type of the peaks.
	public e2dVoronoiPeakType peakType;
	/// Scale of the generated heights along the X axis.
	public float frequencyPerUnit;
	/// Amount of generated peaks. In [0, 1].
	public float peakRatio;
	/// Width ratio of the peaks. In [0, 1].
	public float peakWidth;
	/// If true the generator will try to satisfy the user defined peak points.
	public bool usePeaks;

	/// Copies data from another preset. Assumes both presets are of the same kind.
	public override void Copy(e2dPreset other)
	{
		e2dCurveVoronoiPreset preset = (e2dCurveVoronoiPreset)other;
		peakType = preset.peakType;
		frequencyPerUnit = preset.frequencyPerUnit;
		peakRatio = preset.peakRatio;
		peakWidth = preset.peakWidth;
		usePeaks = preset.usePeaks;
	}

	/// Updates the parameters of the generator based on the settings of the preset.
	public override void UpdateValues(e2dTerrainGenerator generator)
	{
		generator.Voronoi.Copy(this);
	}

	/// Clones the instance.
	public override e2dPreset Clone()
	{
		e2dCurveVoronoiPreset result = new e2dCurveVoronoiPreset();
		result.Copy(this);
		return result;
	}
}

/// Preset definition for the Midpoint curve generator.
[System.Serializable]
public class e2dCurveMidpointPreset : e2dPreset
{
	/// Scale of the generated heights along the X axis.
	public float frequencyPerUnit;
	/// Roughness of the terrain.
	public float roughness;
	/// If true the generator will try to satisfy the user defined peak points.
	public bool usePeaks;

	/// Copies data from another preset. Assumes both presets are of the same kind.
	public override void Copy(e2dPreset other)
	{
		e2dCurveMidpointPreset preset = (e2dCurveMidpointPreset)other;
		frequencyPerUnit = preset.frequencyPerUnit;
		roughness = preset.roughness;
		usePeaks = preset.usePeaks;
	}

	/// Updates the parameters of the generator based on the settings of the preset.
	public override void UpdateValues(e2dTerrainGenerator generator)
	{
		generator.Midpoint.Copy(this);
	}

	/// Clones the instance.
	public override e2dPreset Clone()
	{
		e2dCurveMidpointPreset result = new e2dCurveMidpointPreset();
		result.Copy(this);
		return result;
	}
}

/// Preset definition for the Walk curve generator.
[System.Serializable]
public class e2dCurveWalkPreset : e2dPreset
{
	/// Controls how fast the walk turns per unit. [0, 90].
	public float angleChangePerUnit;
	/// Controls the distance between turns.
	public float frequencyPerUnit;
	/// Maximum uphill and downhill slope. [0, 1].
	public float cohesionPerUnit;

	/// Copies data from another preset. Assumes both presets are of the same kind.
	public override void Copy(e2dPreset other)
	{
		e2dCurveWalkPreset preset = (e2dCurveWalkPreset)other;
		angleChangePerUnit = preset.angleChangePerUnit;
		frequencyPerUnit = preset.frequencyPerUnit;
		cohesionPerUnit = preset.cohesionPerUnit;
	}

	/// Updates the parameters of the generator based on the settings of the preset.
	public override void UpdateValues(e2dTerrainGenerator generator)
	{
		generator.Walk.Copy(this);
	}

	/// Clones the instance.
	public override e2dPreset Clone()
	{
		e2dCurveWalkPreset result = new e2dCurveWalkPreset();
		result.Copy(this);
		return result;
	}
}

/// Abstract class for all presets of the generator. It allows uniform access to all presets, so that the generator
/// doesn't need to distinguish between them.
public abstract class e2dPreset
{
	/// Name of the preset.
	public string name;

	/// Updates the parameters of the generator based on the settings of the preset.
	public abstract void UpdateValues(e2dTerrainGenerator generator);

	/// Copies data from another preset. Assumes both presets are of the same kind.
	public abstract void Copy(e2dPreset other);

	/// Clones the instance.
	public abstract e2dPreset Clone();
}

