/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;

/// Helper class of the library. Contains logging methods and some math functions.
public static class e2dUtils
{

#region Debug

	/// Turns on/off all debug stuff.
	private static bool DEBUG = false;

	/// Rebuilds the run-time data each time the component is reloaded (action start/stop and script reload).
	public static bool DEBUG_REBUILD_ON_ENABLE { get { return DEBUG && true; } }
	/// Displays debug information in the inspector view of the components.
	public static bool DEBUG_INSPECTOR { get { return DEBUG && false; } }
	/// Displays a curve produced by the generator last time it was executed.
	public static bool DEBUG_GENERATOR_CURVE { get { return DEBUG && false; } }
	/// Displays control textures in the Terrain component inspector.
	public static bool DEBUG_CONTROL_TEXTURES { get { return DEBUG && false; } }
	/// Displays cursor related information at the current cursor position.
	public static bool DEBUG_CURSOR_INFO { get { return DEBUG && false; } }
	/// Displays points defining the curve stripe.
	public static bool DEBUG_STRIPE_POINTS { get { return DEBUG && false; } }
	/// Displays points defining the curve stripe.
	public static bool DEBUG_SHOW_SUBOBJECTS { get { return DEBUG && false; } }
	/// Displays points projected from the curve endpoints to the boundary.
	public static bool DEBUG_BOUNDARY_PROJECTIONS { get { return DEBUG && false; } }
	/// Displays position of the nodes next to each of them.
	public static bool DEBUG_NODE_VALUES { get { return DEBUG && false; } }
	/// Shows the mesh wireframe of terrain objects.
	public static bool DEBUG_SHOW_WIREFRAME { get { return DEBUG && false; } }
	/// Hides target area of the generator.
	public static bool DEBUG_NO_TARGET_AREA { get { return DEBUG && false; } }
	/// Uses fixed seed for the generator.
	public static bool DEBUG_FIXED_GENERATOR_SEED { get { return DEBUG && false; } }
	/// Style to dump from the current skin.
	public static string DEBUG_DUMP_STYLES { get { return DEBUG ? "" : ""; } }


	/// Asserts the condition variable is true.
	public static void Assert(bool variable)
	{
		if (!variable)
		{
			Debug.LogError("!!! ASSERTION FAILED !!!");
			Object fail = null;
			fail.GetHashCode();
		}
	}

	/// Timer start time.
	private static float sTimerStartTime;
	/// Timer label.
	private static string sTimerLabel;

	/// Starts the internal performance timer.
	public static void StartTimer(string label)
	{
		sTimerLabel = label;
		sTimerStartTime = Time.realtimeSinceStartup;
	}

	/// Stops the timer and logs the time passed.
	public static void StopTimer()
	{
		Debug.Log(sTimerLabel + " finished in " + (Time.realtimeSinceStartup - sTimerStartTime));
	}

#endregion


#region Logging

	/// Writes a message into the log. The messages are filtered based on their source.
	public static void Log(string message)
	{
		System.Type callerType = new System.Diagnostics.StackTrace(1).GetFrame(0).GetMethod().DeclaringType;
		
		// this is the place to disable logging of certain parts of the library
		//if (callerType == typeof(e2dMaterialAtlas)) return;

		Debug.Log(callerType.Name + ": " + message);
	}

	/// Reports and error into the log. The messages are filtered based on their source.
	public static void Error(string message)
	{
		Debug.LogError(message);
	}

	/// Reports a warning into the log. The messages are filtered based on their source.
	public static void Warning(string message)
	{
		Debug.LogWarning(message);
	}

#endregion


#region Math

	/// Returns the cross product of two 2D vectors.
	public static float Cross(Vector2 a, Vector2 b)
	{
		return a.x * b.y - a.y * b.x;
	}

	/// Returns true if P is in the triangle defined by A, B, C.
	/// Taken from http://www.blackpawn.com/texts/pointinpoly/default.html (based on Real-Time Collision Detection)
	public static bool PointInTriangle(Vector2 P, Vector2 A, Vector2 B, Vector2 C)
	{
		// Compute vectors        
		Vector2 v0 = C - A;
		Vector2 v1 = B - A;
		Vector2 v2 = P - A;

		// Compute dot products
		float dot00 = Vector2.Dot(v0, v0);
		float dot01 = Vector2.Dot(v0, v1);
		float dot02 = Vector2.Dot(v0, v2);
		float dot11 = Vector2.Dot(v1, v1);
		float dot12 = Vector2.Dot(v1, v2);

		// Compute barycentric coordinates
		float invDenom = 1.0f / (dot00 * dot11 - dot01 * dot01);
		float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
		float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

		// Check if point is in triangle
		return (u > 0) && (v > 0) && (u + v < 1);
	}

	/// Returns true if segment (a, b) intersects segment (c, d).
	public static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
	{
		Vector2 intersection;
		return SegmentsIntersect(a, b, c, d, out intersection);
	}

	/// Returns true if segment (a, b) intersects segment (c, d).
	public static bool SegmentsIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 intersection)
	{
		intersection = Vector2.zero;

		float cross = (a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x);
		if (Mathf.Abs(cross) <= float.Epsilon)
		{
			return false; // near parallel
		}

		intersection.x = ((c.x - d.x) * (a.x * b.y - a.y * b.x) - (a.x - b.x) * (c.x * d.y - c.y * d.x)) / cross;
		intersection.y = ((c.y - d.y) * (a.x * b.y - a.y * b.x) - (a.y - b.y) * (c.x * d.y - c.y * d.x)) / cross;

		// attempt to prevent imprecision errors
		float delta1 = 0;
		if (Mathf.Abs(a.x - b.x) <= float.Epsilon || Mathf.Abs(a.y - b.y) <= float.Epsilon) delta1 = 0.01f;
		if (intersection.x < Mathf.Min(a.x, b.x) - delta1 || intersection.x > Mathf.Max(a.x, b.x) + delta1)
		{
			return false;
		}
		if (intersection.y < Mathf.Min(a.y, b.y) - delta1 || intersection.y > Mathf.Max(a.y, b.y) + delta1)
		{
			return false;
		}

		// attempt to prevent imprecision errors
		float delta2 = 0;
		if (Mathf.Abs(c.x - d.x) <= float.Epsilon || Mathf.Abs(c.y - d.y) <= float.Epsilon) delta2 = 0.01f;
		if (intersection.x < Mathf.Min(c.x, d.x) - delta2 || intersection.x > Mathf.Max(c.x, d.x) + delta2)
		{
			return false;
		}
		if (intersection.y < Mathf.Min(c.y, d.y) - delta2 || intersection.y > Mathf.Max(c.y, d.y) + delta2)
		{
			return false;
		}

		return true;
	}

	/// Returns true if line (a, b) intersects line (c, d). The intersection is returned in result.
	public static bool LinesIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 result)
	{
		result = Vector2.zero;

		float cross = (a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x);
		if (Mathf.Abs(cross) < float.MinValue) return false; // near parallel

		float xi = ((c.x - d.x) * (a.x * b.y - a.y * b.x) - (a.x - b.x) * (c.x * d.y - c.y * d.x)) / cross;
		float yi = ((c.y - d.y) * (a.x * b.y - a.y * b.x) - (a.y - b.y) * (c.x * d.y - c.y * d.x)) / cross;

		result.x = xi;
		result.y = yi;

		return true;
	}

	/// Returns true if half line (a, b) intersects line (c, d). The intersection is returned in result.
	public static bool HalfLineAndLineIntersect(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 result)
	{
		result = Vector2.zero;

		float cross = (a.x - b.x) * (c.y - d.y) - (a.y - b.y) * (c.x - d.x);
		if (Mathf.Abs(cross) < float.MinValue) return false; // near parallel

		result.x = ((c.x - d.x) * (a.x * b.y - a.y * b.x) - (a.x - b.x) * (c.x * d.y - c.y * d.x)) / cross;
		result.y = ((c.y - d.y) * (a.x * b.y - a.y * b.x) - (a.y - b.y) * (c.x * d.y - c.y * d.x)) / cross;

		// check if the result is in the right half plane
		if (Vector2.Dot(result, b - a) < 0) return false;

		return true;
	}

	/// Returns true if the segment intersects the convex polygon.
	public static bool SegmentIntersectsPolygon(Vector2 a, Vector2 b, Vector2[] poly)
	{
		bool intersect = false;

		Vector2 lastVertex = poly[poly.Length - 1];
		foreach (Vector2 vertex in poly)
		{
			intersect = intersect || SegmentsIntersect(a, b, lastVertex, vertex);
			lastVertex = vertex;
		}

		return intersect;
	}

	/// Returns true if the point is in the convex polygon.
	public static bool PointInConvexPolygon(Vector2 p, Vector2[] poly)
	{
		bool inside = true;

		Vector2 lastVertex = poly[poly.Length - 1];
		foreach (Vector2 vertex in poly)
		{
			float cross = Cross(vertex - lastVertex, p - lastVertex);
			inside = inside && cross <= 0;
			lastVertex = vertex;
		}

		return inside;
	}

	/// Returns a 2D vector having the specified angle (between itself and the X axis).
	public static Vector2 Vector2dFromAngle(float angle)
	{
		// angle is in degrees
		angle *= Mathf.Deg2Rad;
		return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
	}

	/// Linear interpolation without limits on the parameter.
	public static float Lerp(float a, float b, float t)
	{
		return a * (1 - t) + b * t;
	}

	/// Linear interpolation of vectors.
	public static Vector3 Lerp(Vector3 a, Vector3 b, float t)
	{
		return new Vector3(Lerp(a.x, b.x, t), Lerp(a.y, b.y, t), Lerp(a.z, b.z, t));
	}

#endregion

}
