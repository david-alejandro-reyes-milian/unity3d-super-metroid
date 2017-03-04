/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections.Generic;

/// Height value generator based on the 3d terrain Voronoi generator.
public class e2dVoronoi
{
	/// Peaks which are to be satisfied by the function.
	private List<Vector2> mPeaks;
	/// Low points between peaks and also on the edges.
	private List<Vector2> mValleys;
	/// Type of the peaks.
	private e2dVoronoiPeakType mPeakType;
	/// Width of the peaks.
	private float mPeakWidth;

	/// Constructs the noise function but doesn't generate anything.
	/// Peak coordinates are assumed to be in [0, 1].
	public e2dVoronoi(List<Vector2> peaks, e2dVoronoiPeakType peakType, float peakWidth)
	{
		mPeaks = peaks;
		mPeakType = peakType;
		mPeakWidth = peakWidth;

		// sort the peaks along the X axis
		mPeaks.Sort(new Vector2XComparer());

		// compute valleys
		mValleys = new List<Vector2>(mPeaks.Count + 1);
		// left edge
		mValleys.Add(new Vector2(0, -mPeaks[0].x));
		// inside points
		for (int i = 1; i < mPeaks.Count; i++)
		{
			float dist = Mathf.Abs(mPeaks[i].x - mPeaks[i - 1].x);
			float x = 0.5f * (mPeaks[i - 1].x + mPeaks[i].x);
			mValleys.Add(new Vector2(x, -0.5f * dist));
		}
		// right edge
		mValleys.Add(new Vector2(1, -(1 - mPeaks[mPeaks.Count - 1].x)));

		// normalize the valleys
		float minValley = float.MaxValue;
		foreach (Vector2 valley in mValleys)
		{
			if (valley.y < minValley) minValley = valley.y;
		}
		for (int i = 0; i < mValleys.Count; i++)
		{
			Vector2 valley = mValleys[i];

			// normalize into [0, 1]
			valley.y = (valley.y - minValley) / -minValley;

			// make the valley depend on the heights of the surrounding peaks
			if (i == 0) valley.y *= mPeaks[i].y;
			else if (i == mValleys.Count - 1) valley.y *= mPeaks[i - 1].y;
			else valley.y *= Mathf.Min(mPeaks[i - 1].y, mPeaks[i].y);

			// in case we use linear interpolation we make the peaks wider by moving the valleys up
			if (mPeakType == e2dVoronoiPeakType.LINEAR)
			{
				if (mPeakWidth <= 0.5f)
				{
					valley.y *= 2 * mPeakWidth;
				}
				else if (i > 0 && i < mValleys.Count - 1)
				{
					float delta = 2 * (mPeakWidth - 0.5f);
					valley.y = Mathf.Lerp(valley.y, Mathf.Min(mPeaks[i - 1].y, mPeaks[i].y), delta);
				}
			}

			mValleys[i] = valley;
		}
	}

	/// Returns value of the function at x. The parameter x must comes from the [0, 1] interval.
	public float GetValue(float x)
	{
		// find the right peak and valley where the point is
		// Note: binary search could be used instead of linear since the peaks are sorted
		int peakIndex = mPeaks.Count - 1;
		int valleyIndex = mPeaks.Count;
		for (int i=0; i<mPeaks.Count; i++)
		{
			if (x < mPeaks[i].x)
			{
				if (x < mValleys[i].x)
				{
					peakIndex = i - 1;
					valleyIndex = i;
					break;
				}
				else
				{
					peakIndex = i;
					valleyIndex = i;
					break;
				}
			}
		}

		// compute the height based on the peak type used
		float delta = (x - mValleys[valleyIndex].x) / (mPeaks[peakIndex].x - mValleys[valleyIndex].x); // [0, 1]
		if (float.IsNaN(delta)) delta = 0;
		float height = 0;
		float radians, amplitude, offset;

		switch (mPeakType)
		{
			case e2dVoronoiPeakType.LINEAR:
				height = Mathf.Lerp(mValleys[valleyIndex].y, mPeaks[peakIndex].y, delta);
				break;
			case e2dVoronoiPeakType.SINE:
				delta = 1 - Mathf.Pow(1 - delta, Mathf.Lerp(e2dConstants.VORONOI_SIN_POWER_MIN, e2dConstants.VORONOI_SIN_POWER_MAX, mPeakWidth));
				radians = -0.5f * Mathf.PI + delta * Mathf.PI;
				amplitude = 0.5f * (mPeaks[peakIndex].y - mValleys[valleyIndex].y);
				offset = 1;
				height = mValleys[valleyIndex].y + (Mathf.Sin(radians) + offset) * amplitude;
				break;
			case e2dVoronoiPeakType.QUADRATIC:
				delta *= Mathf.Lerp(1, e2dConstants.VORONOI_QUADRATIC_PEAK_WIDTH_RATIO, mPeakWidth);
				if (delta > 1)
				{
					height = mPeaks[peakIndex].y;
				}
				else
				{
					amplitude = mPeaks[peakIndex].y - mValleys[valleyIndex].y;
					height = mValleys[valleyIndex].y + delta * delta * amplitude;
				}
				break;
		}

		return height;
	}

	/// Comparer of two Vector2 values based on their X coordinates.
	class Vector2XComparer : IComparer<Vector2>
	{
		/// Compares two Vector2 values based on their X coordinates.
		public int Compare(Vector2 x, Vector2 y)
		{
			return x.x < y.x ? -1 : x.x > y.x ? 1 : 0;
		}
	}

}
