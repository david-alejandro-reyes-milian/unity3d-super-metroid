/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections.Generic;

/// Height value generator using the Midpoint generator.
public class e2dMidpoint
{
	/// Heightmap.
	private float[] mCells;
	/// Initial number of cells.
	private int mInitialCellCount;
	/// Initial step size
	private int mInitialStep;
	/// Roughness.
	private float mRoughness;
	/// List of custom peaks.
	private List<Vector2> mPeaks;

	/// Constructs the noise function but doesn't generate anything.
	public e2dMidpoint(int cellCount, int initialStep, float roughness, List<Vector2> peaks)
	{
		mRoughness = roughness;
		mInitialCellCount = cellCount;
		cellCount = Mathf.NextPowerOfTwo(cellCount - 1) + 1;
		mCells = new float[cellCount];
		mInitialStep = Mathf.ClosestPowerOfTwo(initialStep);
		mInitialStep = Mathf.Clamp(mInitialStep, 1, cellCount - 1);
		mPeaks = peaks;
	}

	/// Renegerates the values in the heightmap.
	public void Regenerate()
	{
		// control variables
		int step = mInitialStep;
		float heightRange = 0.9f;
		float heightRangeMultiplier = Mathf.Pow(2, Mathf.Lerp(e2dConstants.MIDPOINT_ROUGHNESS_POWER_MIN, e2dConstants.MIDPOINT_ROUGHNESS_POWER_MAX, mRoughness));

		// initial values in the cells
		for (int i = 0; i < mCells.Length; i++)
		{
			mCells[i] = -1;
		}
		for (int i = 0; i < mCells.Length; i += mInitialStep)
		{
			mCells[i] = 0.5f -0.5f * heightRange + UnityEngine.Random.value * heightRange;
		}
		heightRange *= heightRangeMultiplier;

		// predefine values in the heightmap based on the custom peaks
		if (mPeaks != null)
		{
			foreach (Vector2 peak in mPeaks)
			{
				int i = Mathf.RoundToInt(peak.x * (mInitialCellCount - 1) / mInitialStep);
				i *= mInitialStep;
				mCells[i] = peak.y;
			}
		}

		// iterate until there are no cells to fill
		while (step > 1)
		{
			for (int i = 0; i + step < mCells.Length; i += step)
			{
				int midpointIndex = i + (step >> 1);
				int j = i + step;

				float midpoint = 0.5f * (mCells[i] + mCells[j]);
				midpoint += -0.5f * heightRange + UnityEngine.Random.value * heightRange;

				mCells[midpointIndex] = midpoint;
			}

			// decrease the step
			step >>= 1;
			// decrease the random range for heights
			heightRange *= heightRangeMultiplier;
		}
	}

	/// Returns value of the heightmap at give index.
	public float GetValueAt(int i)
	{
		return mCells[i];
	}
}