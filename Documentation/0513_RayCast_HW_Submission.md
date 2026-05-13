# 05-13 RayCast HW Submission

## Applied Base

- Applied `onPlex/3D_Base` branch `0513_RaySimple` into this Unity project.
- Submission scene is `Assets/01.Scenes/Raycast.unity`.
- Build settings now point to `Assets/01.Scenes/Raycast.unity`.

## Feature Checklist

- RayCast
  - `CenterRaycastShooter` keeps the class raycast demo.
  - `TpsRayInteractor` uses the camera center ray to find interactable objects.
  - `CombatComponent` uses a two-step camera-to-muzzle ray for ranged hit-scan attack.

- Interaction
  - `IInteractable` defines the interaction contract.
  - `DebugInteractable` is applied to NPC and chest/world-item dummy prefabs.
  - `Interactable_NPC_Dummy.prefab` and `Interactable_Chest_Dummy.prefab` are on the `Interactable` layer.
  - `E` triggers interaction.

- Attack
  - Left click: ranged hit-scan damage through `CombatComponent`.
  - `F`: melee overlap attack through `TpsMeleeAttackComponent`.
  - `Q`: ranged magic sphere cast through `TpsMagicSphereCastComponent`.
  - `R`: ground raycast AOE through `TpsGroundAoeSkillComponent`.
  - Enemy dummy uses `HealthComponent` and `IDamageable`.

- Animation
  - Existing player prefab keeps the Starter Assets locomotion animator.
  - `AttackAnimationFeedback` adds a short visual attack pulse for ranged, melee, magic, and AOE actions.
  - `IAttackFeedback` keeps attack feedback decoupled from the damage components.

## Structure Check

- Interface scripts start with `I`: `IInteractable`, `IDamageable`, `IAttackFeedback`.
- Interaction, combat, movement, and raycast scripts are separated by folder and responsibility.
- Scene wiring is authored in `Raycast.unity`: player, interaction targets, enemy dummy, attack point, camera references, and layer masks.

## Verification Boundary

Unity Editor live play verification was not performed in this environment. Verification was done by checking code structure, serialized scene/prefab references, layer masks, input actions, and build-scene configuration.

## Third Party Model Pass

- Replaced the heavy unity-chan sample import path with a lightweight CC0 Kenney character asset pack.
- Imported the model, idle/run/jump animation FBX files, four skin PNG files, and matching URP Lit materials under `Assets/04.ThirdParty/Kenney/AnimatedCharacters1`.
- Removed the duplicated StarterAssets tutorial `Readme` folder that conflicted with the project-level tutorial `Readme` script.
- Cleared stale baked `LightingData` references from `TPS_Base.unity` and `Raycast.unity` so Unity can regenerate lighting cleanly for the current editor version.
