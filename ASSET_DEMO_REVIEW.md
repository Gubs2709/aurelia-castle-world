# asset_demo review — 16 October 2026

All ten files are **real 3D GLB models**, not sprite sheets. Static glTF inspection found **zero skins, animation clips or texture images in every file**. They use simple geometry and material colours. A Blender review sheet is saved at `artifacts/asset-demo-review/models.png`; machine-readable inventory: `artifacts/asset_demo_inventory.json`.

| Model | Base vertices | Assessment |
|---|---:|---|
| Ancient giant tree | 2,450 | Low-poly primitive canopy; cannot match the supplied dense painted forest without rebuilding branches, roots, leaves and bark. |
| Mountain peak | 225 | Could serve as a distant silhouette. |
| Fantasy castle | 632 | Simple blockout, less detailed than the existing citadel. |
| Medieval timber house | 118 | Blockout; existing Blender houses have more detail. |
| Harbour ship | 98 | Very coarse distant prop; no sail/rig animation. |
| Stone golem | 1,398 | Potential stylized base; would need rigging and animation. |
| Red dragon | 447 | Very simple base; requires anatomy, wings, materials and rigging. |
| Sky whale | 1,052 | Potential distant creature base; needs animation and art revision. |
| Crystal cluster | 64 | Suitable simple prop with material polish. |
| Stone bridge | 336 | Simple blockout, less detailed than the current vaulted bridge. |

The supplied demos appear to use Z-up source coordinates without the expected glTF conversion. The review script corrects orientation for display. Verify orientation and scale before any future import.

They can technically be added, but dropping them in unchanged would move the visual quality back toward the basic shapes the user rejected. None was substituted into the game in this pass. The existing detailed village/boats/bridge were reused, and new forest/knight assets were built instead. Keep original files untouched; original provenance/licensing was not supplied.
