/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.

using UnityEngine;

/// Global storage for all numeric and other constants.
public class e2dConstants
{

#region General Settings

	/// If true the surface curve is checked for intercrossing each time it is updated.
	public static readonly bool CHECK_CURVE_INTERCROSSING = false;

#endregion

#region Handles

	/// Color of the middle part of a handle.
	public static readonly Color COLOR_HANDLE_CENTER = new Color(1, 1, 1, 0.6f);
	/// Color of the middle part of a handle.
	public static readonly Color COLOR_HANDLE_X_SLIDER = new Color(1, 0.2f, 0.2f, 0.9f);
	/// Color of the middle part of a handle.
	public static readonly Color COLOR_HANDLE_Y_SLIDER = new Color(0.2f, 1f, 0.2f, 0.9f);
	/// Scale ratio of the middle part of a handle.
	public static readonly float SCALE_HANDLE_CENTER = 0.15f;
	/// Scale ratio of the dot in the middle part of a handle.
	public static readonly float SCALE_HANDLE_CENTER_DOT = 0.05f;
	/// Scale ratio of the slider part of a handle.
	public static readonly float SCALE_HANDLE_SLIDER = 1.0f;
	/// Scale ratio of the middle part of a rotation handle.
	public static readonly float SCALE_HANDLE_ROTATION = 1.1f;

#endregion

#region Inspector

	/// Padding between elements of a rectangle field.
	public static readonly float RECT_FIELD_PADDING = 2;
	/// Margin around a label in a rectangle field.
	public static readonly float RECT_FIELD_LABEL_MARGIN = 2;

	/// Padding between elements of a vector field.
	public static readonly float VECTOR_FIELD_PADDING = 5;
	/// Margin around a label in a vector field.
	public static readonly float VECTOR_FIELD_LABEL_MARGIN = 10;

#endregion

#region Terrain

	/// Width/depth of the collision mesh.
	public static readonly float COLLISION_MESH_Z_DEPTH = 10.0f;

	/// Number of splat textures per shader. When more textures are used more instances of the shader are created.
	public static readonly int NUM_TEXTURES_PER_STRIPE_SHADER = 4;

	/// Maximum number of grass textures (limited by Grass.shader).
	public static readonly int MAX_GRASS_TEXTURES = 4;
	/// Influences the density of grass.
	public static readonly float GRASS_DENSITY_RATIO = 5.0f;
	/// Influences the amount of scattering of grass bushes.
	public static readonly float GRASS_SCATTER_RARTIO = 1.0f;
	/// Amount of the V coordinate to be considered as roots of the grass.
	public static readonly float GRASS_ROOT_SIZE = 0.1f;

	/// Names of the sub-object of the main game object holding the fill mesh data.
	public static readonly string FILL_MESH_NAME = "_fill";
	/// Names of the sub-object of the main game object holding the curve mesh data.
	public static readonly string CURVE_MESH_NAME = "_curve";
	/// Names of the sub-object of the main game object holding the grass mesh data.
	public static readonly string GRASS_MESH_NAME = "_grass";
	/// Names of the sub-object of the main game object holding the collider mesh data.
	public static readonly string COLLIDER_MESH_NAME = "_collider";

	/// Initial size of the fill texture.
	public static readonly float INIT_FILL_TEXTURE_WIDTH = 1;
	/// Initial size of the fill texture.
	public static readonly float INIT_FILL_TEXTURE_HEIGHT = 1;
	/// Initial offset of the fill texture.
	public static readonly float INIT_FILL_TEXTURE_OFFSET_X = 0;
	/// Initial offset of the fill texture.
	public static readonly float INIT_FILL_TEXTURE_OFFSET_Y = 0;
	/// Initial setting of whether the curve is closed or not.
	public static readonly bool INIT_CURVE_CLOSED = false;

#endregion

#region Terrain Inspector

	/// Minimum size of a tile of the fill texture.
	public static readonly float FILL_TEXTURE_SIZE_MIN = float.Epsilon;
	/// Maximum size of a tile of the fill texture.
	public static readonly float FILL_TEXTURE_SIZE_MAX = 20.0f;
	/// Maximum offset of a tile of the fill texture.
	public static readonly float FILL_TEXTURE_OFFSET_MAX = 20.0f;
	
	/// Minimum width of a texture.
	public static readonly float TEXTURE_WIDTH_MIN = 0.1f;
	/// Maximum width of a texture.
	public static readonly float TEXTURE_WIDTH_MAX = 10.0f;
	/// Minimum height of a texture.
	public static readonly float TEXTURE_HEIGHT_MIN = 0.1f;
	/// Maximum height of a texture.
	public static readonly float TEXTURE_HEIGHT_MAX = 5.0f;

	/// Initial random grass scatter ratio.
	public static readonly float INIT_GRASS_SCATTER_RATIO = 0.0f;
	/// Minimum random grass scatter ratio.
	public static readonly float GRASS_SCATTER_RATIO_MIN = 0.0f;
	/// Maximum random grass scatter ratio.
	public static readonly float GRASS_SCATTER_RATIO_MAX = 1.0f;
	/// Initial speed of grass waving.
	public static readonly float INIT_GRASS_WAVE_SPEED = 1.0f;
	/// Minimum speed of grass waving.
	public static readonly float GRASS_WAVE_SPEED_MIN = 0.1f;
	/// Maximum speed of grass waving.
	public static readonly float GRASS_WAVE_SPEED_MAX = 2.0f;
	/// Minimum size of grass.
	public static readonly float GRASS_SIZE_MIN = 0.1f;
	/// Maximum size of grass.
	public static readonly float GRASS_SIZE_MAX = 10.0f;
	/// Minimum size randomness of grass.
	public static readonly float GRASS_SIZE_RANDOMNESS_MIN = 0.0f;
	/// Maximum size randomness of grass.
	public static readonly float GRASS_SIZE_RANDOMNESS_MAX = 1.0f;
	/// Minimum wave amplitude of grass.
	public static readonly float GRASS_WAVE_AMPLITUDE_MIN = 0.0f;
	/// Maximum wave amplitude of grass.
	public static readonly float GRASS_WAVE_AMPLITUDE_MAX = 1.0f;

	/// Size of the control texture displayed in the inspector.
	public static readonly float CONTROL_TEXTURE_SIZE = 50;
	
#endregion

#region Terrain Scene

	/// Size of the gizmos in the upper right corner of the Scene view (screen coords).
	public static readonly float SCENE_GIZMOS_SIZE = 78.0f;

	/// Color of the curve.
	public static readonly Color COLOR_CURVE_NORMAL = new Color(0.8f, 0, 0);
	/// Color of the selected part of the curve.
	public static readonly Color COLOR_CURVE_BRUSH_LINE = new Color(0, 0.3f, 1, 0.3f);
	/// Color of the spheres displayed at the points of the curve.
	public static readonly Color COLOR_CURVE_BRUSH_SPHERE = new Color(0, 0, 1, 0.3f);
	/// Color of the terrain boundary rectangle.
	public static readonly Color COLOR_BOUNDARY_RECT = new Color(0, 0.5f, 0.5f);
	/// Color of the nodes cursor.
	public static readonly Color COLOR_NODE_CURSOR = new Color(0, 0, 1, 0.5f);
	/// Color of the brush arrow.
	public static readonly Color COLOR_BRUSH_ARROW = new Color(1, 0, 0, 0.8f);

	/// Size of the curve brush lines.
	public static readonly float SCALE_CURVE_BRUSH_LINE = 0.1f;
	/// Size of the curve brush spheres.
	public static readonly float SCALE_CURVE_BRUSH_SPHERE = 0.2f;
	/// Scale of the handles of the terrain nodes.
	public static readonly float SCALE_NODE_HANDLES = 2.5f;
	/// Size of the cursor while editing nodes.
	public static readonly float SCALE_NODES_CURSOR = 0.2f;
	/// Size of the cursor while editing nodes.
	public static readonly float SCALE_NODES_CURVE_SPHERE = 0.1f;
	/// Size of the arrow displaying the direction of the brush.
	public static readonly float SCALE_BRUSH_ARROW = 2.0f;

	/// Strength of the height brush.
	public static readonly float BRUSH_HEIGHT_RATIO = 0.05f;
	/// Strength of the grass brush.
	public static readonly float BRUSH_GRASS_RATIO = 0.001f;
	/// How fast the size of the brush changes when the mouse moves.
	public static readonly float BRUSH_SIZE_INC_RATIO = 40.0f;
	/// How fast the angle of the brush changes when the mouse moves.
	public static readonly float BRUSH_ANGLE_INC_RATIO = 150.0f;
	/// How fast the selected texture changes when the mouse moves.
	public static readonly float BRUSH_TEXTURE_INC_RATIO = 2.0f;
	/// How fast the opacity of the brush changes when the mouse moves.
	public static readonly float BRUSH_OPACITY_INC_RATIO = 40.0f;
	/// Distance the mouse must travel for the brush to be applied again.
	public static readonly float BRUSH_APPLY_STEP_RATIO = 0.3f;

	/// Minimum size of the brush.
	public static readonly int BRUSH_SIZE_MIN = 1;
	/// Maximum size of the brush.
	public static readonly int BRUSH_SIZE_MAX = 50;
	/// Initial size of the brush.
	public static readonly int INIT_BRUSH_SIZE = 15;
	/// Minimum opacity of the brush.
	public static readonly int BRUSH_OPACITY_MIN = 0;
	/// Maximum opacity of the brush.
	public static readonly int BRUSH_OPACITY_MAX = 100;
	/// Initial opacity of the brush.
	public static readonly int INIT_BRUSH_OPACITY = 50;
	/// Initial angle of the brush.
	public static readonly float INIT_BRUSH_ANGLE = 0;


#endregion

#region Generator

	/// Minimum size of the step between two nodes.
	public static readonly float GENERATOR_STEP_NODE_SIZE_MIN = 0.001f;

	/// Minimum Perlin frequency per unit.
	public static readonly float PERLIN_FREQUENCY_MIN = 0.001f;
	/// Maximum Perlin frequency per unit.
	public static readonly float PERLIN_FREQUENCY_MAX = 0.2f;
	/// Minimum number of Perlin octaves.
	public static readonly int PERLIN_OCTAVES_MIN = 1;
	/// Maximum number of Perlin octaves.
	public static readonly int PERLIN_OCTAVES_MAX = 20;
	/// Minimum Perlin persistence.
	public static readonly float PERLIN_PERSISTENCE_MIN = 0.001f;
	/// Maximum Perlin persistence.
	public static readonly float PERLIN_PERSISTENCE_MAX = 1.0f;
	/// Minimum Voronoi frequency per unit.
	public static readonly float VORONOI_FREQUENCY_MIN = 0.001f;
	/// Maximum Voronoi frequency per unit.
	public static readonly float VORONOI_FREQUENCY_MAX = 0.2f;
	/// Minimum Voronoi peak ratio.
	public static readonly float VORONOI_PEAK_RATIO_MIN = 0.001f;
	/// Maximum Voronoi peak ratio.
	public static readonly float VORONOI_PEAK_RATIO_MAX = 1.0f;
	/// Minimum Voronoi peak width ratio.
	public static readonly float VORONOI_PEAK_WIDTH_MIN = 0.001f;
	/// Maximum Voronoi peak width ratio.
	public static readonly float VORONOI_PEAK_WIDTH_MAX = 1.0f;
	/// Minimum power coefficient of the sinus function parameter in the Voronoi generator.
	public static readonly float VORONOI_SIN_POWER_MIN = 0.6f;
	/// Maximum power coefficient of the sinus function parameter in the Voronoi generator.
	public static readonly float VORONOI_SIN_POWER_MAX = 2.5f;
	/// Scale ratio of the width of the top of a peak while using the quadratic function in the Voronoi generator.
	public static readonly float VORONOI_QUADRATIC_PEAK_WIDTH_RATIO = 4.0f;
	/// Minimum Midpoint frequency per unit.
	public static readonly float MIDPOINT_FREQUENCY_MIN = 0.001f;
	/// Maximum Midpoint frequency per unit.
	public static readonly float MIDPOINT_FREQUENCY_MAX = 0.2f;
	/// Minimum Midpoint roughness.
	public static readonly float MIDPOINT_ROUGHNESS_MIN = 0.0f;
	/// Maximum Midpoint roughness.
	public static readonly float MIDPOINT_ROUGHNESS_MAX = 1.0f;
	/// Minimum Midpoint roughness power coefficient.
	public static readonly float MIDPOINT_ROUGHNESS_POWER_MIN = -2.0f;
	/// Maximum Midpoint roughness power coefficient.
	public static readonly float MIDPOINT_ROUGHNESS_POWER_MAX = -0.5f;
	/// Minimum Walk frequency per unit.
	public static readonly float WALK_FREQUENCY_MIN = 0.001f;
	/// Maximum Walk frequency per unit.
	public static readonly float WALK_FREQUENCY_MAX = 0.5f;
	/// Minimum angle change of the walk generator.
	public static readonly float WALK_ANGLE_CHANGE_MIN = 0.0f;
	/// Maximum angle change of the walk generator.
	public static readonly float WALK_ANGLE_CHANGE_MAX = 100.0f;
	/// Minimum cohesion of the walk generator.
	public static readonly float WALK_COHESION_MIN = 0.0f;
	/// Maximum cohesion of the walk generator.
	public static readonly float WALK_COHESION_MAX = 2.0f;

	/// Minimum number of smooth iterations.
	public static readonly int SMOOTH_ITERATIONS_MIN = 1;
	/// Maximum number of smooth iterations.
	public static readonly int SMOOTH_ITERATIONS_MAX = 100;

	/// Initial setting of whether to rebuild the whole terrain or not.
	public static readonly bool INIT_FULL_REBUILD = true;
	/// Initial size of a step between two nodes.
	public static readonly float INIT_NODE_STEP_SIZE = 0.5f;
	/// Initial position of the target area.
	public static readonly Vector2 INIT_TARGET_POSITION = new Vector2(50, 50);
	/// Initial size of the target area.
	public static readonly Vector2 INIT_TARGET_SIZE = new Vector2(100, 100);
	/// Initial angle of the target area.
	public static readonly float INIT_TARGET_ANGLE = 0;

	/// Initial grass cliff angle.
	public static readonly float INIT_GRASS_STOP_ANGLE = 80;
	/// Initial grass height range.
	public static readonly Vector2 INIT_GRASS_HEIGHT_RANGE = new Vector2(0, 1);
	/// Initial grass density.
	public static readonly float INIT_GRASS_DENSITY = 1.0f;


#endregion

#region Generator Scene

	/// Color of the target area rectangle.
	public static readonly Color COLOR_TARGET_AREA = new Color(0, 0.5f, 1, 0.7f);
	/// Color of the user defined peaks for the generator.
	public static readonly Color COLOR_GENERATOR_PEAKS = new Color(1, 0, 0, 0.9f);

	/// Scale ratio of the width of the target area rectangle.
	public static readonly float SCALE_TARGET_AREA_LINE_WIDTH = 0.01f;
	/// Size of the user defined peaks for the generator.
	public static readonly float SCALE_GENERATOR_PEAKS = 0.15f;

#endregion
}
