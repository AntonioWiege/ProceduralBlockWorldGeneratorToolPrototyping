	Procedural Block Landscape System Creation Tool Prototype
	by Antonio Wiege 28.09.2023

Supported Platforms: Windows (10 & 11), Mac (Intel & M1)
Remember to set the build platform. 
Originally 2022.2.11f1; Test with latest 2023 version show possible shader errors and crashes with open tool inspector on play mode.

Not a functioning tool. No save, load, export, etc.
An online copy of code to share privately. NOT free for use or open source, but good luck if you're learning this stuff.
Had a bit of a time crunch on this, given it's rapid prototyping nature the code may contain pestiferous gordian knots.
Perhaps I'll revisit this work and at least add the proper commentary and bug list. Structure and naming aren't ok either. This project needs a SOLID DRY KISS


	Table of Contents:
- How to use the Tool
- - Setup
- - Component
- - Brush
- - etc.
- - Script Definition Symbols

	How to use the Tool:

		Quick Setup in project:
1) Copy the Assets folder or specifically move the "ProceduralBlockWorldGeneratorToolPrototyping" folder into your Assets.
2) Within a scene, drag & drop the prefab "Landscape Tool Prefab" into the hierarchy and hit play.
3) Profit

		Component:
- For the component to work, the compute shaders and materials need to be assigned. Each have an element in the assets, with the same, corresponding name.
- Certain features are easy to break; You are advised to start out with a copy of a working instance in a scene, e.g. the default prefab and go from there.
- If the sphere and cone mesh aren't assigned in the inspector, the tree generation won't work.
- Using noise without GPU enabled is for comparison only. Without the GPU, the tool lacks the performance to be usable.

		Brush & EffectorObject:
- The brush should be configured after pressing play. This is because the values get reset to standard on play, which is also to make understand, that the edits are temporary in this prototype version of the tool.
- It may seem like you cannot edit deep beneath the ocean plane, but that is only because of it having a collider. If you move bellow the ocean plane, you can dig however deep you want to, within custom set bounds. The benefit of keeping the plane with collision, is to aim with the mouse whereever you want to generate new chunks, without having to fly there. You may disable the collider and or remove the plane.
- Effector Object is a component that can be put on any object with a collider. Put it into the corresponding list in the tool component and set it up to your needs. It will function just like a mesh brush, stamping an edit into the world, by its voxelized mesh. Default shape has the value 0, which cuts holes, to turn any collider mesh into a visible structure in the world, you need to change it to a value higher than the defined border in the tool (>= 1 should do in any regular case).

		etc.:
- Most properties in the component show additional information, when hovering over them with the cursor.
- The first noise setup in the list should have the application ID 0.
- The noise settings are unrestricted, so that any complex interactions can be designed. Unless extremes are balanced out, it may be easy to make a setup unusable. Keeping close to the default values should help.
- An important note on 2D noise is orientation. Standard 2D orientation follows XY with texture and world coordinates. Default planar terrain should follow XZ instead. For this to work, the noise setup needs to rotate either around the X or Z axis by 90°. The values in the rotation component represent full rotations, meaning 90° would be .25 in any orientation.
- Noise with strong differences can yield visible seems, due to limited blending ranges and chunk content restriction for performance reasons.

	Global Script Definition Symbols:
- Deactivate_Profiling		| Removes custom profiling entries
- Deactivate_Async		| Switches from the asychronous to the synchronous code sections
- Deactivate_Gizmos		| Removes some additional Gizmos; In particular voxelization related ones

If symbols do not immediately update on apply:
	Add another, leave empty and apply again.
	If still nothing restart the editor.
	This is a common Unity error.