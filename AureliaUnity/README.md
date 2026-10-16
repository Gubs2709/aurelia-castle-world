# Aurelia — Beyond the Falls

**Current version is Reference Castle V8**, saved at `Assets/Scenes/ReferenceCastle.unity`. Run `D:/GAME/Play Reference Castle.cmd` or `D:/GAME/Builds/ReferenceCastle/Citadel.exe`. See `D:/GAME/HANDOFF_V8.md` for current controls, assets, validation and limitations. The older Aurelia scene and commands below describe a historical iteration; do not run its full-scene rebuild over ReferenceCastle.

A local Windows fantasy world built with Unity 6000.6.5f1 and URP 17.6.0.

Open this folder in Unity Hub, then open `Assets/Scenes/Aurelia.unity`. Use Play in the Game view. The existing browser version remains in the parent folder.

## Play

The standalone executable is `D:\GAME\Builds\Aurelia\Aurelia.exe`. Keep its entire directory together, including `Aurelia_Data` and Unity DLLs.

WASD/arrows move relative to the camera, Shift sprints, Space jumps, mouse looks, and the wheel zooms. Escape pauses and releases the cursor. F switches to free flight, with Q/E descending/ascending. T changes the lighting. Tab shows the place shortcuts: 1 village, 2 viaduct, 3 grove, 4 castle gardens. R returns to spawn. H hides the interface.

## Scope

World first, as requested: an 800 m terrain with mountains, dense lantern street, elevated castle and gardens, arched approach, waterfalls, a river, a forest guardian, illustrated grove, flower meadows, and an ambient dragon. Walking, jumping, third-person camera, pause, fly camera, viewpoints, and atmosphere controls are implemented.

No combat, quests, inventory, NPC dialogue, multiplayer, monetization, cloud services, or save system. Those were not added after the user selected world-first development. There is no audio yet. Structures have exterior collision; interiors are not implemented. The guardian and dragon are scenery.

This is an original stylized environment. It does not reproduce the production fidelity or breadth of Wuthering Waves or the reference illustrations. Current limitations include repeated town-house geometry, procedural architectural textures, simple waterfall effects, and a simple ambient dragon/guardian. The UI is a minimal inspection interface.

## Art direction

The 19 original JPEGs in the parent folder are the primary references: monumental pale castles, blue spires, waterfalls, lantern markets, magical forests, gentle guardians, flowers, and peaceful travel. The additional Wuthering Waves/adventure-manhwa request informed layered vistas, destination silhouettes, warm/cool contrast, atmospheric distance, and an illustrated grove palette.

Kuro Games' developer interview informed lighting and colour-grading decisions: https://www.unrealengine.com/developer-interviews/exploring-the-post-apocalyptic-charm-of-asg-open-worlds-in-wuthering-waves . No game assets or characters from Wuthering Waves were copied.

## Editing and validation

- `Assets/Scripts/ValleyExplorer.cs`: controller, camera, atmosphere, local interface.
- `Assets/Scripts/ValleyMotion.cs`: environmental animation.
- `Assets/Editor/KingdomBuilder.cs`: base scene construction, asset persistence, screenshots, Windows build.
- `Assets/Editor/WorldArtPass.cs`: architectural and landscape refinements.
- `Assets/Editor/WorldFinishing.cs`: traveller costume and illustrated grove materials.
- `Assets/Editor/WorldValidation.cs`: CharacterController route and jump checks plus the entry ramp.
- `Assets/Shaders/`: meadow, sky, river.
- `Assets/World/`: saved generated mesh, material, texture, profile and controller assets. Keep their `.meta` files.
- Parent `artifacts/unity/`: validation outputs and rendered views.

The saved scene includes refinements beyond the base builder. Use the complete rebuild workflow documented in `UNITY_HANDOFF.md` in the parent folder if regenerating. A rebuild replaces the active scene; save manual changes elsewhere first.

The complete rebuild command is **Aurelia > Rebuild complete world (replaces scene)**. It includes the art, costume, forest route, ramp and navigation checks. The native build result is recorded in the parent `artifacts/unity/build-result.json`. Four nonfatal build warnings cover collision pre-baking for a future Unity version, disabled runtime CLI control, and stripped internal debug shaders.

Unity's library, temporary imports, logs, builds, and IDE files are generated and ignored. Runtime content is local. No account, server, or network is required to play the built game.
