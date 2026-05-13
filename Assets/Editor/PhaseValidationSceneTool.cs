using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using SystemicOverload.Combat;
using SystemicOverload.Phase1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// Phase별 Validation Scene을 생성/정렬하는 에디터 전용 Tool입니다.
    /// </summary>
    public static class PhaseValidationSceneTool
    {
        private const string PhaseValidationFolder = "Assets/01.Scenes/PhaseValidation";
        private const string Phase1ScenePath = "Assets/01.Scenes/PhaseValidation/Phase_01_MovementValidation.unity";
        private const string Phase2ScenePath = "Assets/01.Scenes/PhaseValidation/Phase_02_DamageWeaponValidation.unity";

        [MenuItem("Tools/Systemic Overload/Phase Validation/Build Phase 1 Movement Scene")]
        public static void BuildPhase1MovementScene()
        {
            EnsureFolderPath(PhaseValidationFolder);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Phase1SceneObjects built = SetupPhase1MovementSceneCore(useMouseRaycastRotation: false);
            TryAttachPlayerAnimatorStack(built.Player, built.MovementComponent);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Phase1ScenePath);
            AddSceneToBuildSettings(Phase1ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[PhaseValidationSceneTool] Phase 1 Validation Scene 생성 및 Build Settings 등록이 완료되었습니다.");
        }

        [MenuItem("Tools/Systemic Overload/Phase Validation/Build Phase 2 Damage Weapon Scene")]
        public static void BuildPhase2DamageWeaponScene()
        {
            EnsureFolderPath(PhaseValidationFolder);

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Phase1SceneObjects built = SetupPhase1MovementSceneCore(useMouseRaycastRotation: true);
            TryAttachPlayerAnimatorStack(built.Player, built.MovementComponent);

            HealthComponent playerHealth = built.Player.AddComponent<HealthComponent>();
            SetPrivateField(playerHealth, "maxHealth", 100.0f);
            SetPrivateField(playerHealth, "currentHealth", 100.0f);

            CombatComponent combatComponent = built.Player.AddComponent<CombatComponent>();
            Animator playerAnimator = built.Player.GetComponent<Animator>();
            SetPrivateField(combatComponent, "movementComponent", built.MovementComponent);
            SetPrivateField(combatComponent, "animator", playerAnimator);
            SetPrivateField(combatComponent, "maxRange", 50.0f);
            SetPrivateField(combatComponent, "hitLayerMask", ~0);

            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            dummy.name = "TrainingDummy";
            dummy.transform.position = new Vector3(0.0f, 1.0f, 10.0f);
            HealthComponent dummyHealth = dummy.AddComponent<HealthComponent>();
            SetPrivateField(dummyHealth, "maxHealth", 500.0f);
            SetPrivateField(dummyHealth, "currentHealth", 500.0f);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), Phase2ScenePath);
            AddSceneToBuildSettings(Phase2ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[PhaseValidationSceneTool] Phase 2 Validation Scene 생성 및 Build Settings 등록이 완료되었습니다. 공격: Space / 게임패드 South");
        }

        private readonly struct Phase1SceneObjects
        {
            public Phase1SceneObjects(
                GameObject player,
                InputProvider inputProvider,
                MovementComponent movementComponent,
                Camera mainCamera)
            {
                Player = player;
                InputProvider = inputProvider;
                MovementComponent = movementComponent;
                MainCamera = mainCamera;
            }

            public GameObject Player { get; }
            public InputProvider InputProvider { get; }
            public MovementComponent MovementComponent { get; }
            public Camera MainCamera { get; }
        }

        private static Phase1SceneObjects SetupPhase1MovementSceneCore(bool useMouseRaycastRotation)
        {
            GameObject lightRoot = new GameObject("Directional Light");
            Light directionalLight = lightRoot.AddComponent<Light>();
            directionalLight.type = LightType.Directional;
            directionalLight.intensity = 1.0f;
            lightRoot.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(4.0f, 1.0f, 4.0f);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0.0f, 1.0f, 0.0f);
            RemoveComponent<CapsuleCollider>(player);

            CharacterController characterController = player.AddComponent<CharacterController>();
            characterController.center = new Vector3(0.0f, 1.0f, 0.0f);
            characterController.height = 2.0f;
            characterController.radius = 0.45f;

            InputProvider inputProvider = player.AddComponent<InputProvider>();
            MovementComponent movementComponent = player.AddComponent<MovementComponent>();

            GameObject cameraRoot = new GameObject("Main Camera");
            Camera mainCamera = cameraRoot.AddComponent<Camera>();
            cameraRoot.tag = "MainCamera";
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 300.0f;
            mainCamera.fieldOfView = 60.0f;

            Phase1OrbitCameraController cameraController = cameraRoot.AddComponent<Phase1OrbitCameraController>();
            SetPrivateField(cameraController, "followTarget", player.transform);
            SetPrivateField(cameraController, "inputProvider", inputProvider);
            SetPrivateField(cameraController, "movementComponent", movementComponent);
            SetPrivateField(cameraController, "pivotOffset", new Vector3(0.0f, 1.6f, 0.0f));
            SetPrivateField(cameraController, "defaultZoomDistance", 7.0f);
            SetPrivateField(cameraController, "maxZoomDistance", 14.0f);
            SetPrivateField(cameraController, "autoFollowMode", Phase1OrbitCameraController.AutoFollowMode.MovingOnly);
            SetPrivateField(cameraController, "collisionMask", -1);
            SetPrivateField(cameraController, "waterSurfaceHeight", -1000.0f);

            // Mouse Raycast 회전이 즉시 동작하도록 MovementComponent에 카메라를 명시적으로 연결합니다.
            SetPrivateField(movementComponent, "aimCamera", mainCamera);
            SetPrivateField(movementComponent, "groundLayerMask", -1);
            SetPrivateField(movementComponent, "aimRayMaxDistance", 500.0f);
            SetPrivateField(movementComponent, "useMouseRaycastRotation", useMouseRaycastRotation);
            SetPrivateField(movementComponent, "orbitCameraController", cameraController);

            SetPrivateField(inputProvider, "normalizeDiagonalInput", true);
            SetPrivateField(inputProvider, "enableDualMouseForwardMove", true);
            SetPrivateField(inputProvider, "dualMouseForwardAmount", 1.0f);

            return new Phase1SceneObjects(player, inputProvider, movementComponent, mainCamera);
        }

        private static void TryAttachPlayerAnimatorStack(GameObject player, MovementComponent movementComponent)
        {
            Animator animator = player.GetComponent<Animator>();
            if (animator == null)
            {
                animator = player.AddComponent<Animator>();
            }

            RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                Phase1LocomotionAnimatorFactory.ControllerAssetPath);
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning(
                    "[PhaseValidationSceneTool] Animator Controller가 없습니다. " +
                    "Tools/Systemic Overload/Animation/Create Phase1 Locomotion Placeholder Controller 메뉴를 먼저 실행하세요.");
            }

            LocomotionAnimatorDriver driver = player.GetComponent<LocomotionAnimatorDriver>();
            if (driver == null)
            {
                driver = player.AddComponent<LocomotionAnimatorDriver>();
            }

            SetPrivateField(driver, "movementComponent", movementComponent);
            SetPrivateField(driver, "characterController", player.GetComponent<CharacterController>());
        }

        [MenuItem("Tools/Systemic Overload/Phase Validation/Generate Scene Policy Template")]
        public static void GenerateScenePolicyTemplate()
        {
            EnsureFolderPath(PhaseValidationFolder);

            string policyFilePath = Path.Combine(PhaseValidationFolder, "PhaseValidationSceneTemplate.txt");
            string policyTemplate = BuildPolicyTemplate();
            File.WriteAllText(policyFilePath, policyTemplate, Encoding.UTF8);

            AssetDatabase.Refresh();
            Debug.Log("[PhaseValidationSceneTool] Phase Validation Scene 템플릿 파일이 생성되었습니다.");
        }

        private static string BuildPolicyTemplate()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("Phase Validation Scene Template");
            builder.AppendLine("- Scene Path Rule: Assets/01.Scenes/PhaseValidation/Phase_0N_<Feature>Validation.unity");
            builder.AppendLine("- Build Settings: Phase Scene를 enabled 상태로 등록");
            builder.AppendLine("- Minimum Objects:");
            builder.AppendLine("  - Ground (validation floor)");
            builder.AppendLine("  - Player/Target actor with current Phase components");
            builder.AppendLine("  - Main Camera with validation camera behavior");
            builder.AppendLine("- Validation Checklist:");
            builder.AppendLine("  - Smoke: 핵심 입력/동작 1회 이상 성공");
            builder.AppendLine("  - Input Mapping: LMB FreeLook, RMB SyncRotate, LMB+RMB Forward, Wheel Zoom");
            builder.AppendLine("  - Regression: 직전 Phase 핵심 기능이 유지됨");
            builder.AppendLine("  - Performance: 프레임 드랍/GC spike 여부 확인");
            return builder.ToString();
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> buildScenes = EditorBuildSettings.scenes.ToList();
            bool alreadyExists = buildScenes.Any(scene => scene.path == scenePath);
            if (alreadyExists)
            {
                return;
            }

            buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = buildScenes.ToArray();
        }

        private static void EnsureFolderPath(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            if (segments.Length == 0 || segments[0] != "Assets")
            {
                throw new IOException("Asset path는 Assets 루트로 시작해야 합니다.");
            }

            string currentPath = "Assets";
            for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
            {
                string nextSegment = segments[segmentIndex];
                string nextPath = currentPath + "/" + nextSegment;
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, nextSegment);
                }

                currentPath = nextPath;
            }
        }

        private static void RemoveComponent<T>(GameObject targetGameObject) where T : Component
        {
            T targetComponent = targetGameObject.GetComponent<T>();
            if (targetComponent == null)
            {
                return;
            }

            Object.DestroyImmediate(targetComponent);
        }

        private static void SetPrivateField<TTarget>(TTarget targetObject, string fieldName, object value)
        {
            FieldInfo targetField = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (targetField == null)
            {
                Debug.LogWarning($"[PhaseValidationSceneTool] 필드 연결 실패: {typeof(TTarget).Name}.{fieldName}");
                return;
            }

            targetField.SetValue(targetObject, value);
        }
    }
}
