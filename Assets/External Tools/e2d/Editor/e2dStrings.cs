/// @file
/// @author Ondrej Mocny http://www.hardwire.cz
/// See LICENSE.txt for license information.


/// Storage of all the string constants in the library.
public class e2dStrings
{
	public const string ERROR_CANT_FIND_ASSETS = "Error while loading resources.";
	public const string ERROR_TERRAIN_NOT_CREATED = "You must create the terrain first!";
	public const string ERROR_CURVE_IS_CROSSING = "The curve edges are crossing!";

	public const string INFO_NO_TOOL_SELECTED = "No tool is selected.";
	public const string INFO_PEAKS = "Left-click to add new peak.\nHold shift and click to delete a peak.";

	public const string LABEL_RECT_XMIN = "X1";
	public const string LABEL_RECT_XMAX = "X2";
	public const string LABEL_RECT_YMIN = "Y1";
	public const string LABEL_RECT_YMAX = "Y2";
	public const string LABEL_VECTOR2_X = "X";
	public const string LABEL_VECTOR2_Y = "Y";
	public const string LABEL_BRUSH = "Brush";
	public const string LABEL_GLOBAL_PARAMETERS = "Global Parameters";
	public const string LABEL_TEXTURE_SETTINGS = "Texture Settings";
	public const string LABEL_BRUSH_SIZE = "Brush Size";
	public const string LABEL_BRUSH_OPACITY = "Brush Opacity";
	public const string LABEL_BRUSH_ANGLE = "Brush Angle";
	public const string LABEL_TERRAIN_BOUNDARY = "Boundary Rect";
	public const string LABEL_CURVE_CLOSED = "Close Curve";
	public const string LABEL_RESET_TERRAIN = "Reset Terrain";
	public const string LABEL_REBUILD_DATA = "Rebuild Data";
	public const string LABEL_CURVE_TEXTURE = "Texture";
	public const string LABEL_FILL_TEXTURE = "Fill Texture";
	public const string LABEL_FILL_TEXTURE_TILE_WIDTH = "Tile Width";
	public const string LABEL_FILL_TEXTURE_TILE_HEIGHT = "Tile Height";
	public const string LABEL_FILL_TEXTURE_TILE_OFFSET_X = "Tile Offset X";
	public const string LABEL_FILL_TEXTURE_TILE_OFFSET_Y = "Tile Offset Y";
	public const string LABEL_CURVE_TEXTURE_FADE_THRESHOLD = "Fade Threshold";
	public const string LABEL_CURVE_TEXTURE_WIDTH = "World Width";
	public const string LABEL_CURVE_TEXTURE_HEIGHT = "World Height";
	public const string LABEL_CURVE_TEXTURE_FIXED_ANGLE = "Fixed Angle";
	public const string LABEL_GRASS_SCATTER_RATIO = "Random Scatter";
	public const string LABEL_GRASS_WAVE_SPEED = "Wave Speed";
	public const string LABEL_GRASS_SIZE = "World Size";
	public const string LABEL_GRASS_WIDTH_RANDOMNESS = "Width Random";
	public const string LABEL_GRASS_HEIGHT_RANDOMNESS = "Height Random";
	public const string LABEL_GRASS_WAVE_AMPLITUDE = "Wave Strength";
	public const string LABEL_TEXTURE = "Texture";
	public const string LABEL_PLASTIC_EDGES = "Plastic Edges";

	public const string LABEL_REBUILD_ALL = "Rebuild All";
	public const string LABEL_TARGET_AREA = "Target Area";
	public const string LABEL_TARGET_POSITION = "Center";
	public const string LABEL_TARGET_SIZE = "Size";
	public const string LABEL_TARGET_ANGLE = "Angle";
	public const string LABEL_TOOL_SETTINGS = "Parameters";
	public const string LABEL_PRESET = "Preset";
	public const string LABEL_NO_PRESET = "<no preset>";
	public const string LABEL_STEP_SIZE = "Step Size";
	public const string LABEL_PERLIN_OCTAVES = "Octaves";
	public const string LABEL_FREQUENCY_PER_UNIT = "Freq. / Meter";
	public const string LABEL_PERLIN_PERSISTENCE = "Persistence";
	public const string LABEL_MIDPOINT_ROUGHNESS = "Roughness";
	public const string LABEL_WALK_CHANGE_PER_UNIT = "Change / Meter";
	public const string LABEL_WALK_COHESION = "Cohesion";
	public const string LABEL_PEAKS = "User Defined Peaks";
	public const string LABEL_PEAK_WIDTH = "Peak Width";
	public const string LABEL_PEAK_RATIO = "Peak Ratio";
	public const string LABEL_PEAK_TYPE = "Peak Type";
	public const string LABEL_USE_PEAKS = "Use User Peaks";
	public const string LABEL_BLEND_WEIGHT = "Blend Weight";
	public const string LABEL_SMOOTH_ITERATIONS = "Iterations";
	public const string LABEL_CLIFF_TEXTURE = "Cliff Texture";
	public const string LABEL_CLIFF_SLOPE_RANGE = "Cliff Slope";
	public const string LABEL_TEXTURE_HEIGHTS = "Texture Heights";
	public const string LABEL_TEXTURE_GROUND = "ground";
	public const string LABEL_GRASS_HEIGHT_MIN = "Min Height";
	public const string LABEL_GRASS_HEIGHT_MAX = "Max Height";
	public const string LABEL_GRASS_STOP_ANGLE = "Cliff Slope";
	public const string LABEL_GRASS_DENSITY = "Density";

	public const string BUTTON_ADD_TEXTURE = "+ Add";
	public const string BUTTON_DELETE_TEXTURE = "- Del";
	public const string BUTTON_GENERATE = "Generate";
	public const string BUTTON_SMOOTH = "Smooth";
	public const string BUTTON_TEXTURE = "Generate Texturing";

	public const string UNDO_TERRAIN_PROPERTIES = "Edit e2dTerrain Properties";
	public const string UNDO_EDIT_NODES = "Edit e2dTerrain Nodes";
	public const string UNDO_HEIGHT_BRUSH = "e2dTerrain Height Brush";
	public const string UNDO_MOVE_NODES = "Move e2dTerrain Nodes";
	public const string UNDO_BOUNDARY = "Change e2dTerrain Boundary";
	public const string UNDO_CURVE_TEXTURE_BRUSH = "e2dTerrain Texture Brush";
	public const string UNDO_CURVE_TEXTURE = "Edit e2dTerrain Curve Texture";
	public const string UNDO_GRASS_TEXTURE = "Edit e2dTerrain Grass";
	public const string UNDO_FILL_TEXTURE = "Change e2dTerrain Fill Texture";
	public const string UNDO_RESET_TERRAIN = "Reset e2dTerrain Nodes";
	public const string UNDO_GENERATE = "Generate e2dTerrain";
	public const string UNDO_SMOOTH = "Smooth e2dTerrain";
	public const string UNDO_TEXTURE = "Texture e2dTerrain";
	public const string UNDO_GENERATE_GRASS = "Generate e2dTerrain Grass";
	public const string UNDO_GENERATOR_AREA = "Change e2dTerrainGenerator Area";
	public const string UNDO_EDIT_PEAKS = "Edit e2dTerrainGenerator Peaks";
	public const string UNDO_CLOSE_CURVE = "Close e2dTerrain Curve";
	public const string UNDO_OPEN_CURVE = "Open e2dTerrain Curve";

	public static readonly string[] EDITOR_TOOLS = 
	{ 
		"Nodes", 
		"Brush", 
		"Filling", 
		"Textures",
		"Grass",
	};
	public static readonly string[] EDITOR_TOOL_DESCRIPTIONS = 
	{
		"Left-click to place a node.\nHold ctrl and click to append a node to the end of the curve.\nHold shift and click to delete a node. Hold ctrl as well to move the rest of the nodes towards the deleted one.",
		"Left-click to raise the terrain.\nHold shift to adjust brush size.\nHold ctrl to adjust brush direction.\nHold both to adjust brush opacity.",
		"Use the handles to adjust the boundary rectangle.",
		"Left-click to paint the selected texture.\nHold shift to adjust the brush size.\nHold ctrl to change the current texture.",
		"Left-click to paint grass.\nHold ctrl and click to remove grass.\nHold shift to adjust the brush size.\nHold both to adjust brush opacity."
	};

	public static readonly string[] GENERATOR_TOOLS = { "Create", "Smooth", "Texture", "Grass" };

	public static readonly string[] GENERATOR_CURVE_TOOLS = { "Perlin", "Midpoint", "Voronoi", "Walk", "Peaks" };

}