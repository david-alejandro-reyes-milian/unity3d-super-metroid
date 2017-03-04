/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;
using System.Collections.Generic;

/// Triangulates any 2D polygon into a set of triangles.
/// Based on http://www.unifycommunity.com/wiki/index.php?title=Triangulator
public class e2dTriangulator
{
	private List<Vector2> mPoints = new List<Vector2>();

	/// Creates the triangulator assigning the polygon vertices.
	public e2dTriangulator(Vector2[] points)
	{
		mPoints = new List<Vector2>(points);
	}

	/// Triangulates and returns the list of triangles. The triangles are expressed as 3 subsequent values
	/// in the list each of them being an index into the array of polygon vertices.
	public List<int> Triangulate()
	{
		List<int> indices = new List<int>();

		int n = mPoints.Count;
		if (n < 3)
			return indices;

		int[] V = new int[n];
		if (Area() > 0)
		{
			for (int v = 0; v < n; v++)
				V[v] = v;
		}
		else
		{
			for (int v = 0; v < n; v++)
				V[v] = (n - 1) - v;
		}

		int nv = n;
		int count = 2 * nv;
		for (int m = 0, v = nv - 1; nv > 2; )
		{
			if ((count--) <= 0)
				return indices;

			int u = v;
			if (nv <= u)
				u = 0;
			v = u + 1;
			if (nv <= v)
				v = 0;
			int w = v + 1;
			if (nv <= w)
				w = 0;

			if (Snip(u, v, w, nv, V))
			{
				int a, b, c, s, t;
				a = V[u];
				b = V[v];
				c = V[w];
				indices.Add(a);
				indices.Add(b);
				indices.Add(c);
				m++;
				for (s = v, t = v + 1; t < nv; s++, t++)
					V[s] = V[t];
				nv--;
				count = 2 * nv;
			}
		}

		return indices;
	}

	/// Returns the size of the current polygon.
	private float Area()
	{
		int n = mPoints.Count;
		float A = 0.0f;
		for (int p = n - 1, q = 0; q < n; p = q++)
		{
			Vector2 pval = mPoints[p];
			Vector2 qval = mPoints[q];
			A += pval.x * qval.y - qval.x * pval.y;
		}
		return (A * 0.5f);
	}

	/// Snips a triangle away from the polygon.
	private bool Snip(int u, int v, int w, int n, int[] V)
	{
		int p;
		Vector2 A = mPoints[V[u]];
		Vector2 B = mPoints[V[v]];
		Vector2 C = mPoints[V[w]];
		if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
			return false;
		for (p = 0; p < n; p++)
		{
			if ((p == u) || (p == v) || (p == w))
				continue;
			Vector2 P = mPoints[V[p]];
			if (InsideTriangle(A, B, C, P))
				return false;
		}
		return true;
	}

	/// Returns true if P is inside triangle A, B, C.
	private bool InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
		float cCROSSap, bCROSScp, aCROSSbp;

		ax = C.x - B.x; ay = C.y - B.y;
		bx = A.x - C.x; by = A.y - C.y;
		cx = B.x - A.x; cy = B.y - A.y;
		apx = P.x - A.x; apy = P.y - A.y;
		bpx = P.x - B.x; bpy = P.y - B.y;
		cpx = P.x - C.x; cpy = P.y - C.y;

		aCROSSbp = ax * bpy - ay * bpx;
		cCROSSap = cx * apy - cy * apx;
		bCROSScp = bx * cpy - by * cpx;

		return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
	}
}
