/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;

/// Implementation of the Perlin noise function. Based on http://freespace.virgin.net/hugo.elias/models/m_perlin.htm .
public class e2dPerlinNoise
{
	private int mOctavesCount;
	private e2dPerlinOctave[] mOctaves;

	/// Constructs the noise function but doesn't generate anything. For details on the parameters see
	/// http://freespace.virgin.net/hugo.elias/models/m_perlin.htm .
	public e2dPerlinNoise(int octaves, float amplitude, int frequency, float persistence)
	{
		mOctavesCount = octaves;

		// checks
		if (frequency < 2)
		{
			e2dUtils.Error("Perlin Frequency must be at least 2");
			frequency = 2;
		}
		if (amplitude <= 0)
		{
			e2dUtils.Error("Perlin Amplitude must be bigger then 0");
			amplitude = 0.1f;
		}
		if (mOctavesCount < 1)
		{
			e2dUtils.Warning("Perlin Octaves Count must be at least 1");
			mOctavesCount = 1;
		}

		// init the octaves
		int octaveFrequency = frequency;
		float octaveAmplitude = amplitude;
		mOctaves = new e2dPerlinOctave[mOctavesCount];
		for (int i = 0; i < mOctavesCount; i++)
		{
			mOctaves[i] = new e2dPerlinOctave(octaveAmplitude, octaveFrequency);
			octaveAmplitude *= persistence;
			octaveFrequency *= 2;
		}
	}

	/// Generates the data and makes them ready to retrieve.
	public void Regenerate()
	{
		for (int i = 0; i < mOctavesCount; i++)
		{
			mOctaves[i].Regenerate();
		}
	}

	/// Returns value of the function at x. The parameter x must comes from the [0, 1] interval.
	public float GetValue(float x)
	{
		float value = 0;
		for (int i = 0; i < mOctavesCount; i++)
		{
			value += mOctaves[i].GetValue(x);
		}
		return value;
	}
}