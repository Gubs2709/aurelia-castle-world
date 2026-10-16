# Citadel of Falling Water

**Current revision: World V8, 16 October 2026.** Read [HANDOFF_V8.md](HANDOFF_V8.md) first. The saved scene and Windows build include the rigged 3D knight, winding coastal paths replacing the huge harbour stairs,24 detailed cliff houses,five landing stages,13 boats,white lighthouse,green river canopy/arched bridge/picnic,sea-tree headland,snow peaks,moving clouds and the corrected runtime glare. Press5 for the bay,7 for the river and8 for the sea tree. Latest evidence is under `artifacts/world-v8/`. Older details below describe prior iterations.

The older descriptions and measurements below are retained as development history and do not describe the current forest or character.

Launch `Play Reference Castle.cmd`. Unity project: `AureliaUnity`. Scene: `Assets/Scenes/ReferenceCastle.unity`. Windows build: `Builds/ReferenceCastle/Citadel.exe`.

This dedicated scene follows `WhatsApp Image 2026-10-15 at 12.22.44 PM.jpeg`. The original `Aurelia.unity` scene and its executable are preserved separately. The current build settings select the new castle scene.

The latest art revision responds to the user's review of the static sky and basic models/materials. It contains four large cloud towers and seventeen smaller fair-weather clouds with independent drift, 691 branching oak models with detailed transparent leaf clusters, 46 timber-and-stone cottages with doors/shutters/side windows/porches/slate roofs, irregular textured boulders and cliff outcrops, textured limestone castle walls, 999 framed gothic window placements with leaded reflective glass, planar river reflections/refraction/depth colouring, and eight waterfall spray emitters. The blue sky colours are retained. Close-up renders are in `artifacts/reference-castle/realism`.

The latest standalone build succeeded with zero errors and four warnings (197,336,318 bytes). Its rendered 1600x1000 vista test averaged 442.7 FPS, p95 2.37 ms, on the RTX 5060 Ti. Walking, free flight and viewpoint reset passed. All 21 clouds moved more than two metres during the native check; comparison of the actual five-second sky captures found changes in 4.86% of sampled sky pixels, excluding the central castle. Reflections are visible in the native capture. No runtime exceptions or shader errors were found in `artifacts/reference-castle/realism/native.log`. These measurements describe this view on this machine; they do not establish integrated-GPU support.

## Controls

- F: toggle free flight. Hold right mouse button and drag to look.
- WASD: move; Q/E: fly down/up; Shift: move faster.
- Enter: begin walking on the southern bridge. Space: jump.
- P: return to the composed reference viewpoint.
- H: hide/show the help strip. Escape: pause/resume movement.
- Close the window to quit.

## Reference coverage

| Image element | Scene implementation |
| --- | --- |
| Very tall ivory castle | Nested curtain walls, palaces, upper keep, galleries, cornices, lancet windows, balconies and buttresses |
| Numerous turquoise spires | More than thirty varied towers/turrets, stepped roof courses, gold finials and coloured pennants |
| Planted cliff | Sculpted plateau, faceted cliff ledges and bushes |
| Broad waterfalls | Eight animated falling-water curtains, feeder streams and plunge pools |
| Turquoise winding water | Meandering river and connected basin beneath the falls |
| Diagonal stone viaduct | Long bridge with repeated open arches, piers, parapets and post capitals |
| Stacked gate structures | Lower river gate, intermediate gate and upward causeway |
| Rural valley | Farmhouses, fences, crop rows, trees and grass |
| Snowy mountain backdrop | Two 3D ridged ranges with elevation-dependent blue and snow colours |
| Tall cream clouds and blue sky | Separate transparent large and small cloud layers move at different speeds against the retained blue sky |
| White birds | Flock geometry around upper towers |
| Foreground overlook | Flowers, grass, framing rocks, imported brown horse and scenic armoured rider |
| Text and carousel arrows | Deliberately excluded because these are screenshot interface overlays |

## Validation and limits

The scene was saved and reopened with no missing scripts or broken material/shader references. Bridge and uphill causeway collision sampling passed. Reports and actual rendered images are in `artifacts/reference-castle`.

Windows build succeeded with zero errors and four Unity warnings. The warnings concern mesh collision prebaking, the disabled runtime Pipeline connection, and unused debug shaders. Native controls/performance results are recorded separately after executing the player.

Native validation passed free-flight movement (16.02 m in one second), reference-view reset and bridge walking (10.01 m in two seconds). Earlier close-up inspection also caught open roof undersides and sky edge stretching; those were corrected before this revision.

This is an explorable reference-inspired environment. It is still visibly simpler than the supplied illustration: repeated tower forms, less elaborate stone carving, and different background/cloud composition. Do not describe it as a pixel-perfect recreation or finished commercial RPG. There are no castle interiors, combat, quests, rideable-horse system or NPC schedules. The rider and birds are scenic geometry. Water, spray, leaf movement and clouds animate. Clouds use distant textured cards rather than a volumetric simulation. Reflections add a rendering cost; integrated-graphics performance has not been tested, so GPU-less compatibility is not established.

## Editing and reproduction

Use Unity 6000.6.5f1. Open the saved scene to inspect or edit it. For the current V7 passes, follow HANDOFF_V7.md. The old initial ReferenceCastleBuilder/ReferenceRealismPass rebuild sequence replaces scene assets and must not be run over the current saved world. No source-control publication or cloud services were configured.

Nature and horse assets are CC0 from Quaternius; see `Assets/Art/Animals/License.txt`, `SOURCE.md` and the existing art provenance. Sky-generation provenance and prompt are in `Assets/ReferenceCastle/SKY_ASSET.md`.

New cloud cutouts, oak-branch cutout, limestone and rock textures were generated with the built-in image generation tool and are saved in `Assets/Art/Realism`. Full prompts are recorded in `PROVENANCE.json`; file hashes are in `artifacts/reference-castle/realism/asset-hashes.json`. No new third-party asset packages were admitted during this revision. Houses, window surrounds, tree branching and rock shapes are authored procedural 3D meshes.
# Latest world pass — 16 October 2026

See [HANDOFF_V7.md](HANDOFF_V7.md) for the current implementation and evidence. The game now has a rigged 3D knight, taller multi-variant forests with painted bark and dense coloured foliage, completed town/gate passes, and a seaward port with an open ocean. Keys **5** and **6** visit the ocean port and autumn canopy. Review renders/reports: `artifacts/world-v7/`. Earlier descriptions and performance numbers above predate this pass.

