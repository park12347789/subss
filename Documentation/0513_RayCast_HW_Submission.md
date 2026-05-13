# 05-13 RayCast HW Submission

## Applied Base

- Applied `onPlex/3D_Base` branch `0513_RaySimple` into this Unity project.
- Submission scene is `Assets/01.Scenes/Raycast.unity`.
- Build settings now point to `Assets/01.Scenes/Raycast.unity`.

## Feature Checklist

- RayCast
  - `CenterRaycastShooter` keeps the class raycast demo.
  - Its hittable mask now targets `Enemy` and `Obstacle`, so the demo ray excludes the player and can hit the authored wall targets.
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
  - Three scene enemies use `HealthComponent`, `IDamageable`, and `DestroyOnDeathComponent`.
  - Enemy objects are destroyed when HP reaches 0.

- Animation
  - Existing player prefab keeps the Starter Assets locomotion animator.
  - Added authored attack clips under `Assets/01.Scenes/Raycast/Animations/Attacks`.
  - `StarterAssetsThirdPerson.controller` now has an `Attack Motion` overlay layer with trigger-driven attack states.
  - Attack triggers are split by action: `AttackRangedTrig`, `AttackMeleeTrig`, `AttackMagicTrig`, and `AttackAreaTrig`.
  - `AttackAnimationFeedback` now drives Animator triggers first and only falls back to the old transform pulse if the controller lacks the trigger.
  - `IAttackFeedback` keeps attack feedback decoupled from the damage components.

## Structure Check

- Interface scripts start with `I`: `IInteractable`, `IDamageable`, `IAttackFeedback`.
- Interaction, combat, movement, and raycast scripts are separated by folder and responsibility.
- Scene wiring is authored in `Raycast.unity`: player, interaction targets, three enemy dummies, attack point, camera references, and layer masks.
- `AttackAnimationFeedback` is wired to the scene `PlayerArmature` Animator and `PlayerArmature/Geometry` animated root.
- `PlayerArmature` is on the `Player` layer, enemy dummies are on `Enemy`, NPC/chest are on `Interactable`, wall targets are on `Obstacle`, and the floor is on `Ground`.
- `ThirdPersonController.GroundLayers` includes both `Default` and `Ground` to preserve TPS movement after the floor layer cleanup.

## Verification Boundary

Unity Editor live play verification was not performed in this environment. Verification was done by checking code structure, serialized scene/prefab references, layer masks, input actions, and build-scene configuration.

## Final MCP Check

- Active scene: `Assets/01.Scenes/Raycast.unity`.
- C# compilation errors: 0.
- Unity console errors: 0.
- Scene missing references: 0.
- Asset missing references: 0.

## Third Party Model Pass

- Replaced the heavy unity-chan sample import path with a lightweight CC0 Kenney character asset pack.
- Imported the model, idle/run/jump animation FBX files, four skin PNG files, and matching URP Lit materials under `Assets/04.ThirdParty/Kenney/AnimatedCharacters1`.
- Removed the duplicated StarterAssets tutorial `Readme` folder that conflicted with the project-level tutorial `Readme` script.
- Cleared stale baked `LightingData` references from `TPS_Base.unity` and `Raycast.unity` so Unity can regenerate lighting cleanly for the current editor version.
