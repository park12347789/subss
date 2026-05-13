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
  - Left click: visible ranged projectile from `AimMuzzlePoint` through `CombatComponent`.
  - `F`: melee overlap attack through `TpsMeleeAttackComponent`.
  - `Q`: visible round magic projectile through `TpsMagicSphereCastComponent` and `MagicProjectileComponent`.
  - `R`: ground raycast AOE through `TpsGroundAoeSkillComponent`.
  - Three scene enemies use `HealthComponent`, `IDamageable`, and `DestroyOnDeathComponent`.
  - Enemy objects are destroyed when HP reaches 0.

- Animation
  - Existing player prefab keeps the Starter Assets locomotion animator.
  - Imported Quaternius Universal Animation Library Standard under `Assets/04.ThirdParty/Quaternius/UniversalAnimationLibrary`.
  - The imported motion pack is CC0 and includes Unity-ready Humanoid clips for retargeting.
  - Attack motions are connected on the Base Layer only through trigger states:
    - Left click/ranged: `Rig|Pistol_Shoot`
    - `F` melee: `Rig|Sword_Attack`
    - `Q` magic: `Rig|Spell_Simple_Shoot`
    - `R` area attack: `Rig|Punch_Cross`
  - `AttackAnimationFeedback` triggers the Animator first so imported Humanoid clips play through Mecanim retargeting.
  - The attempted `Attack Motion` Animator overlay layer was removed because it overrode the base idle pose.
  - Scene clip fallback references are cleared; `AttackAnimationFeedback` still keeps the old transform pulse as a final fallback if Animator trigger setup is missing.
  - `IAttackFeedback` keeps attack feedback decoupled from the damage components.

## Structure Check

- Interface scripts start with `I`: `IInteractable`, `IDamageable`, `IAttackFeedback`.
- Interaction, combat, movement, and raycast scripts are separated by folder and responsibility.
- Scene wiring is authored in `Raycast.unity`: player, interaction targets, three enemy dummies, attack point, camera references, and layer masks.
- `AimReticleCanvas` provides a centered mouse aim point.
- `PlayerFollowCamera` is tuned to a close right-shoulder aim view.
- `AimMuzzlePoint` is placed near the player's right shoulder; both left-click projectile and `Q` magic projectile spawn from that point and travel toward the camera-center reticle ray.
- `AttackAnimationFeedback` is wired to the `PlayerArmature` Animator and `PlayerArmature/Geometry` animated root.
- `StarterAssetsThirdPerson.controller` keeps `Idle Walk Run Blend` as the Base Layer default state and adds four downloaded attack states through Any State trigger transitions.
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
- Imported Quaternius Universal Animation Library Standard from OpenGameArt/itch.io under `Assets/04.ThirdParty/Quaternius/UniversalAnimationLibrary` for proper CC0 Humanoid attack motions.
- Removed the duplicated StarterAssets tutorial `Readme` folder that conflicted with the project-level tutorial `Readme` script.
- Cleared stale baked `LightingData` references from `TPS_Base.unity` and `Raycast.unity` so Unity can regenerate lighting cleanly for the current editor version.
