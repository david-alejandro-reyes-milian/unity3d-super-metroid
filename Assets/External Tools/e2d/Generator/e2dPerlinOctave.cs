/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;

/// Implementation of one Perlin noise octave. The octave is a simple noise function. The full Perlin noise function
/// is a certain sum of the octaves. See http://freespace.virgin.net/hugo.elias/models/m_perlin.htm for details.
public class e2dPerlinOctave
{
	private float mAmplitude;
	private int mFrequency;
	private float[] mNoise;

	/// Constructs the octave. The amplitude is how big the values might be. The frequency defines the number of values
	/// to generate.
	public e2dPerlinOctave(float amplitude, int frequency)
	{
		mAmplitude = amplitude;
		mFrequency = frequency;
		mNoise = new float[mFrequency];
	}

	/// Produces new set of noise data.
	public void Regenerate()
	{
		for (int i = 0; i < mFrequency; i++)
		{
			mNoise[i] = mAmplitude * UnityEngine.Random.value;
		}
	}

	/// Returns the value of the noise function at x01. The parameter x01 must come from the interval [0, 1].
	public float GetValue(float x01)
	{
		float x = x01 * (mFrequency - 1);
		int i1 = Mathf.FloorToInt(x);
		int i2 = Mathf.CeilToInt(x);
		float delta = (x - i1);
		return InterpolateCosine(mNoise[i1], mNoise[i2], delta);
		// Note: cosine interpolation has almost as good results as cubic and is much faster
		/*int i0 = Mathf.Max(0, i1 - 1);
		int i3 = Mathf.Min(mFrequency - 1, i2 + 1);
		return InterpolateCubic(mNoise[i0], mNoise[i1], mNoise[i2], mNoise[i3], delta);*/
	}

	/// Interpolates a to b directed by the parameter x from [0, 1] using cosine interpolation. The interpolation
	/// prescribes the shape of the noise curve.
	private float InterpolateCosine(float a, float b, float x)
	{
		// cosine interpolation gives smoother results then linear
		float ft = x * Mathf.PI;
		float f = (1 - Mathf.Cos(ft)) * 0.5f;
		return a * (1 - f) + b * f;
	}

	/// Interpolates v1 to v2 directed by the parameter x from [0, 1] using cubic interpolation. The parameter v0 is
	/// the point before v1 while v3 is the point after v2.
	private float InterpolateCubic(float v0, float v1, float v2, float v3, float x)
	{
		float p = (v3 - v2) - (v0 - v1);
		float q = (v0 - v1) - p;
		float r = v2 - v0;
		float s = v1;

		return p * x * x * x + q * x * x + r * x + s;
	}
}