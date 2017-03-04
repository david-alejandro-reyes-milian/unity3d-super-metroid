/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;

/// Node of the terrain surface curve (polygonal chain).
[System.Serializable]
public class e2dCurveNode
{
	/// Position in the local space of the terrain.
	public Vector2 position;
	/// Index into the array of the curve textures (see e2dTerrainCurveMesh).
	public int texture;
	/// Amount of grass at this node [0, 1].
	public float grassRatio;

	/// Constructs the node from the position using the default texture.
	public e2dCurveNode(Vector2 _position)
	{
		position = _position;
		texture = 0;
		grassRatio = 0;
	}

	/// Copies data from another node.
	public void Copy(e2dCurveNode other)
	{
		position = other.position;
		texture = other.texture;
		grassRatio = other.grassRatio;
	}

	/// Returns true if the other object is another node and they are equal.
	public override bool Equals(object obj)
	{
		if (!(obj is e2dCurveNode)) return false;

		return this == (e2dCurveNode)obj;
	}

	/// Returns the hash code of the object.
	public override int GetHashCode()
	{
		return Mathf.RoundToInt(1000.0f * position.x + 1000.0f * position.y + texture + 1000.0f * grassRatio);
	}

	/// Returns true if the nodes have the same position.
	public static bool operator ==(e2dCurveNode a, e2dCurveNode b)
	{
		return a.position == b.position;
	}

	/// Returns true if the nodes don't have the same position.
	public static bool operator !=(e2dCurveNode a, e2dCurveNode b)
	{
		return !(a == b);
	}
}


/// Carries data related to a texture of the terrain surface stripe mesh.
[System.Serializable]
public class e2dCurveTexture
{
	/// Texture data.
	public Texture texture;
	/// Size in the local game object space.
	public Vector2 size;
	/// If true the texture is not aligned to the surface curve.
	public bool fixedAngle;
	/// Threshold of the V parameter of the mesh when the alpha is to be faded to zero. Lies in [0,1]. This helps
	/// the stripe to blend better into the fill mesh.
	public float fadeThreshold;

	/// Constructor from the texture data. The rest is inited using default values.
	public e2dCurveTexture(Texture _texture)
	{
		texture = _texture;
		size = new Vector2(1, 1);
		fixedAngle = false;
		fadeThreshold = 0.3f;
	}

	/// Copy constructor.
	public e2dCurveTexture(e2dCurveTexture other)
	{
		texture = other.texture;
		size = other.size;
		fixedAngle = other.fixedAngle;
		fadeThreshold = other.fadeThreshold;
	}
}


/// Carries data related to a texture of the terrain grass.
[System.Serializable]
public class e2dGrassTexture
{
	/// Texture data.
	public Texture texture;
	/// Size in world coordinates.
	public Vector2 size;
	/// Randomness of the size in world units.
	public Vector2 sizeRandomness;
	/// Influences how much the grass waves.
	public float waveAmplitude;

	/// Constructor from the texture data. The rest is inited using default values.
	public e2dGrassTexture(Texture _texture)
	{
		texture = _texture;
		size = new Vector2(1, 1);
		sizeRandomness = new Vector2(0.5f, 0.5f);
		waveAmplitude = 0.5f;
	}

	/// Copy constructor.
	public e2dGrassTexture(e2dGrassTexture other)
	{
		texture = other.texture;
		size = other.size;
		sizeRandomness = other.sizeRandomness;
		waveAmplitude = other.waveAmplitude;
	}
}


/// Defines a peak the user wishes to have in the generated terrain.
[System.Serializable]
public class e2dGeneratorPeak
{
	/// Position of the peak in local coordinates of the game objects.
	public Vector2 position;

	/// Creates the peak at a certain position.
	public e2dGeneratorPeak(Vector2 _position)
	{
		position = _position;
	}
}