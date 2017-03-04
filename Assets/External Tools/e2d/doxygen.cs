
/** 

@mainpage e2d Reference Manual
  

@section intro Introduction
  
@subsection info Project Info
Version: @htmlinclude "VERSION.txt" <br/>
Author: Ondrej Mocny (ondrej.mocny@gmail.com)


@subsection s1 What is this?
This is a reference manual for <i>e2d</i> - a 2D terrain editor and generator for Unity. If you only want to use the tool then take a look at the <a href="../UserGuide.pdf">user guide</a>. If you want to read about the background behind the development of the tool then read the <a href="../Thesis.pdf">thesis</a>.


@subsection s2 Project Structure

The tool is split into two main parts: the terrain (e2dTerrain) and the generator (e2dTerrainGenerator). Both of these are represented by a component and the latter depends on the former. Each of them also has an attached editor: e2dTerrainEditor and e2dTerrainGeneratorEditor. The editor implements the inspector appearance but also displays GUI in the scene. All styles used in the editors are located in e2dStyles. All constants used in the whole tool are placed in e2dConstants to make it easier to tweak them. All strings are extracted in the e2dStrings class to make translations easier. Different debugging tools can be turned on/off in e2dUtils.


@subsection s3 Where to start?
If you haven't done so yet you should read the <a href="../UserGuide.pdf">user guide</a> and play with the tool a bit to see what it can do. Then you can take a look at e2dTerrain to see how it is organized and implemented. Or take a look at the generator (e2dTerrainGenerator) to adjust the methods used for terrain generation.



@section tasks Tasks

@subsection todos TODOs
- Allow to paint specific grass texture only.
- Add objects attachable to the terrain surface.
  - Some cloning or brush placement shold be possible.
  - Useful for stones etc.
- Subdivide node segments if the height brush stretches it too much.
- While dragging corners of the target area change its position as well.
  - Right now it only changes the size but it's stupid because it influences the opposite corners as well.
- Add the option to define "root size" for each grass texture.
- Try calling Mesh.Optimize() on the meshes to increase run-time speed.
- Create terrain manager which would allow sharing of settings and properties of multiple terrain objects.
- Implement terrain splitting.
  - This could be done by creating a special kind of nodes meaning "separator".
  - Special care would have to be taken to solve closed curve terrains.
- More fill textures and blend them together using some random parametrized rule.
  - This could break the pattern quite well.
- Add tooltips to everything.
- Make it possible to display the inspectors in a separate window.
  - It would work with the terrain components of the currently selected object.
- Allow to use custom physics material for the mesh collider.
- Make it possible to move more nodes at once.


@subsection bugs Known Bugs
- The collider mesh should be moved into the main game object.
  - Because now the main game object doesn't receive collision events.
- While changing the target area the peaks moved to the center of the area.
  - They should remain in their position
  - Perhaps the check of their position should be only done when the generation process starts.
- The surface curve shader only works with 4 textures now and it can't be stacked.
  - Also, the shader should work on iPhone.
  - The problem is caused by the shader adjusting alpha.
  - The alpha could perhaps be adjusted in another pass after all shaders blend colors together.
- The Voronoi generator should scatter the peaks more uniformly.
  - Controlled by some parameter.
  - The problem is that now some peaks are very close to each other and it produces spikes.
- Hide the wireframe of the mesh collider.
  - Unity 3.4 allows to hide this from GUI but it doesn't seem to be possible from scripts.
  - http://answers.unity3d.com/questions/129870/hide-meshcollider-wireframe-when-selected.html
- Hide mesh sub-objects from the Hierarchy.
  - The "hideflags" property is bugged as of now, so can't do it.
  - http://answers.unity3d.com/questions/50150/hideflags-on-children-of-visible-objects.html


*/