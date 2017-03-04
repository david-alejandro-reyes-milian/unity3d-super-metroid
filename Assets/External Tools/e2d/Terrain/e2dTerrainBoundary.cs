/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// Takes care of the boundary data of the terrain. The boundary is represented by a rectangle around the terrain object.
/// It functions as the edges for the fill mesh.
public class e2dTerrainBoundary: e2dTerrainMesh
{
	/// Default constructor.
	public e2dTerrainBoundary(e2dTerrain terrain): base(terrain)
	{
	}

	/// Fixes the boundary if necessary.
	public void FixBoundary()
	{
		foreach (e2dCurveNode node in TerrainCurve)
		{
			Vector2 point = node.position;
			if (point.x < Terrain.TerrainBoundary.xMin) Terrain.TerrainBoundary.xMin = point.x;
			if (point.x > Terrain.TerrainBoundary.xMax) Terrain.TerrainBoundary.xMax = point.x;
			if (point.y < Terrain.TerrainBoundary.yMin) Terrain.TerrainBoundary.yMin = point.y;
			if (point.y > Terrain.TerrainBoundary.yMax) Terrain.TerrainBoundary.yMax = point.y;
		}
	}

	/// Ensures the given point lies in the boundary areay.
	public void EnsurePointIsInBoundary(ref Vector3 point)
	{
		if (point.x < Terrain.TerrainBoundary.xMin) point.x = Terrain.TerrainBoundary.xMin;
		if (point.x > Terrain.TerrainBoundary.xMax) point.x = Terrain.TerrainBoundary.xMax;
		if (point.y < Terrain.TerrainBoundary.yMin) point.y = Terrain.TerrainBoundary.yMin;
		if (point.y > Terrain.TerrainBoundary.yMax) point.y = Terrain.TerrainBoundary.yMax;
	}

	/// Returns the rectangle defining the boundary. The vertices go as follows:
	/// 1) bottom right
	/// 2) bottom left
	/// 3) top left
	/// 4) top right
	public Vector2[] GetBoundaryRect()
	{
		Vector2[] boundingRect = new Vector2[5];
		boundingRect[0] = new Vector2(Terrain.TerrainBoundary.xMax, Terrain.TerrainBoundary.yMin);
		boundingRect[1] = new Vector2(Terrain.TerrainBoundary.xMin, Terrain.TerrainBoundary.yMin);
		boundingRect[2] = new Vector2(Terrain.TerrainBoundary.xMin, Terrain.TerrainBoundary.yMax);
		boundingRect[3] = new Vector2(Terrain.TerrainBoundary.xMax, Terrain.TerrainBoundary.yMax);
		boundingRect[4] = boundingRect[0];
		return boundingRect;
	}

	/// Projects the starting point of the terrain curve to the boundary edges. The projected point is returned in
	/// the point parameter. The return value is the index of the edge of the boundary the point was projected to.
	/// It goes as follows:
	/// 0 - bottom edge
	/// 1 - left edge
	/// 2 - top edge
	/// 3 - right edge
	public int ProjectStartPointToBoundary(ref Vector2 point)
	{
		return ProjectPointToBoundary(ref point, 1, TerrainCurve.Count - 1);
	}

	/// Projects the ending point of the terrain curve to the boundary edges. The projected point is returned in
	/// the point parameter. The return value is the index of the edge of the boundary the point was projected to.
	/// It goes as follows:
	/// 0 - bottom edge
	/// 1 - left edge
	/// 2 - top edge
	/// 3 - right edge
	public int ProjectEndPointToBoundary(ref Vector2 point)
	{
		return ProjectPointToBoundary(ref point, 0, TerrainCurve.Count - 2);
	}

	/// Projects the starting point of the terrain curve to the boundary edges. The projected point is returned in
	/// the point parameter. The return value is the index of the edge of the boundary the point was projected to.
	/// It goes as follows:
	/// 0 - bottom edge
	/// 1 - left edge
	/// 2 - top edge
	/// 3 - right edge
	/// The startCurveIndex and endCurveIndex parameters define the portion of the terrain curve to test for
	/// intersections. If there's an intersection while trying to project the point that way it is skipped and another
	/// direction is attempted. This prevents the curve from intersecting the fill mesh.
	public int ProjectPointToBoundary(ref Vector2 point, int startCurveIndex, int endCurveIndex)
	{
		int nearestBorder = -1;
		float minDelta = float.MaxValue;
		float delta;

		// find the nearest edge taking the curve into account (to avoid intersection with it)
		delta = Mathf.Abs(Terrain.TerrainBoundary.yMin - point.y);
		if (delta < minDelta && !Terrain.IntersectsCurve(startCurveIndex, endCurveIndex, point, ProjectPointToBoundaryEdge(point, 0)))
		{
			minDelta = delta;
			nearestBorder = 0;
		}
		delta = Mathf.Abs(Terrain.TerrainBoundary.xMin - point.x);
		if (delta < minDelta && !Terrain.IntersectsCurve(startCurveIndex, endCurveIndex, point, ProjectPointToBoundaryEdge(point, 1)))
		{
			minDelta = delta;
			nearestBorder = 1;
		}
		delta = Mathf.Abs(Terrain.TerrainBoundary.yMax - point.y);
		if (delta < minDelta && !Terrain.IntersectsCurve(startCurveIndex, endCurveIndex, point, ProjectPointToBoundaryEdge(point, 2)))
		{
			minDelta = delta;
			nearestBorder = 2;
		}
		delta = Mathf.Abs(Terrain.TerrainBoundary.xMax - point.x);
		if (delta < minDelta && !Terrain.IntersectsCurve(startCurveIndex, endCurveIndex, point, ProjectPointToBoundaryEdge(point, 3)))
		{
			minDelta = delta;
			nearestBorder = 3;
		}

		// try again without the intersection test
		if (nearestBorder == -1)
		{
			delta = Mathf.Abs(Terrain.TerrainBoundary.yMin - point.y);
			if (delta < minDelta)
			{
				minDelta = delta;
				nearestBorder = 0;
			}
			delta = Mathf.Abs(Terrain.TerrainBoundary.xMin - point.x);
			if (delta < minDelta)
			{
				minDelta = delta;
				nearestBorder = 1;
			}
			delta = Mathf.Abs(Terrain.TerrainBoundary.yMax - point.y);
			if (delta < minDelta)
			{
				minDelta = delta;
				nearestBorder = 2;
			}
			delta = Mathf.Abs(Terrain.TerrainBoundary.xMax - point.x);
			if (delta < minDelta)
			{
				minDelta = delta;
				nearestBorder = 3;
			}
		}

		point = ProjectPointToBoundaryEdge(point, nearestBorder);

		return nearestBorder;
	}

	/// Projects the point to the specified edge of the boundary. See ProjectPointToBoundary() for details about
	/// the indices.
	private Vector2 ProjectPointToBoundaryEdge(Vector2 point, int edgeIndex)
	{
		switch (edgeIndex)
		{
			case 0:
				return new Vector2(point.x, Terrain.TerrainBoundary.yMin);
			case 1:
				return new Vector2(Terrain.TerrainBoundary.xMin, point.y);
			case 2:
				return new Vector2(point.x, Terrain.TerrainBoundary.yMax);
			case 3:
				return new Vector2(Terrain.TerrainBoundary.xMax, point.y);
			default:
				e2dUtils.Error("unknown edge " + edgeIndex);
				break;
		}
		return Vector2.zero;
	}

}