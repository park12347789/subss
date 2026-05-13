using System;
using SystemicOverload.Combat;
using SystemicOverload.Interaction;
using SystemicOverload.Phase1;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// 강의용 TPS Physics Query 컴포넌트와 Layer 설정을 자동으로 구성합니다.
    /// </summary>
    public static class TpsPhysicsQuerySceneSetupTool
    {
        private const string MainInputActionAssetPath = "Assets/00.Preset/MainInputActionData.inputactions";
        private const string EnemyLayerName = "Enemy";
        private const string InteractableLayerName = "Interactable";
        private const string ObstacleLayerName = "Obstacle";
        private const string GroundLayerName = "Ground";

        [MenuItem("Tools/Systemic Overload/TPS Physics Query/Setup Current Scene")]
        public static void SetupCurrentScene()
        {
            EnsureLayerExists(EnemyLayerName);
            EnsureLayerExists(InteractableLayerName);
            EnsureLayerExists(ObstacleLayerName);
            EnsureLayerExists(GroundLayerName);

            InputProvider inputProvider = UnityEngine.Object.FindFirstObjectByType<InputProvider>();
            if (inputProvider == null)
            {
                PlayerInput playerInput = UnityEngine.Object.FindFirstObjectByType<PlayerInput>();
                if (playerInput != null)
                {
                    inputProvider = playerInput.GetComponent<InputProvider>();
                    if (inputProvider == null)
                    {
                        inputProvider = playerInput.gameObject.AddComponent<InputProvider>();
                    }
                }
            }

            if (inputProvider == null)
            {
                GameObject taggedPlayer = GameObject.FindGameObjectWithTag("Player");
                if (taggedPlayer != null)
                {
                    inputProvider = taggedPlayer.GetComponent<InputProvider>();
                    if (inputProvider == null)
                    {
                        inputProvider = taggedPlayer.AddComponent<InputProvider>();
                    }
                }
            }

            if (inputProvider == null)
            {
                Debug.LogError("Player를 찾지 못했습니다. Player tag 또는 PlayerInput 컴포넌트를 확인하세요.");
                return;
            }

            GameObject playerObject = inputProvider.gameObject;
            Camera mainCamera = Camera.main;
            AssignMainInputActionAsset(inputProvider);

            TpsRayInteractor rayInteractor = EnsureComponent<TpsRayInteractor>(playerObject);
            TpsMeleeAttackComponent meleeComponent = EnsureComponent<TpsMeleeAttackComponent>(playerObject);
            TpsMagicSphereCastComponent magicComponent = EnsureComponent<TpsMagicSphereCastComponent>(playerObject);
            TpsGroundAoeSkillComponent aoeComponent = EnsureComponent<TpsGroundAoeSkillComponent>(playerObject);
            TpsDashCollisionComponent dashComponent = EnsureComponent<TpsDashCollisionComponent>(playerObject);

            Transform attackPoint = EnsureAttackPoint(playerObject.transform);

            SetObjectReference(rayInteractor, "aimCamera", mainCamera);
            SetFloat(rayInteractor, "interactDistance", 4.0f);
            SetLayerMask(rayInteractor, "interactMask", LayerMask.GetMask(InteractableLayerName));

            SetObjectReference(meleeComponent, "attackPoint", attackPoint);
            SetFloat(meleeComponent, "radius", 1.6f);
            SetLayerMask(meleeComponent, "enemyMask", LayerMask.GetMask(EnemyLayerName));

            SetObjectReference(magicComponent, "aimCamera", mainCamera);
            SetFloat(magicComponent, "range", 30.0f);
            SetFloat(magicComponent, "radius", 0.35f);
            SetLayerMask(magicComponent, "hitMask", LayerMask.GetMask(EnemyLayerName, ObstacleLayerName));

            SetObjectReference(aoeComponent, "aimCamera", mainCamera);
            SetLayerMask(aoeComponent, "groundMask", LayerMask.GetMask(GroundLayerName));
            SetLayerMask(aoeComponent, "enemyMask", LayerMask.GetMask(EnemyLayerName));
            SetInt(aoeComponent, "bufferSize", 32);

            SetObjectReference(dashComponent, "aimCamera", mainCamera);
            SetLayerMask(dashComponent, "obstacleMask", LayerMask.GetMask(ObstacleLayerName));

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("TPS Physics Query Scene 구성이 완료되었습니다.");
        }

        private static void AssignMainInputActionAsset(InputProvider inputProvider)
        {
            InputActionAsset mainInputActionAsset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(MainInputActionAssetPath);
            if (mainInputActionAsset == null)
            {
                Debug.LogError($"InputActionAsset을 찾을 수 없습니다: {MainInputActionAssetPath}");
                return;
            }

            PlayerInput playerInput = inputProvider.GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                SetObjectReference(playerInput, "m_Actions", mainInputActionAsset);
                EditorUtility.SetDirty(playerInput);
            }

            inputProvider.SetInputActionsAsset(mainInputActionAsset);
            SetObjectReference(inputProvider, "inputActionsAsset", mainInputActionAsset);
            SetString(inputProvider, "resourcesFallbackPath", string.Empty);
        }

        private static T EnsureComponent<T>(GameObject owner) where T : Component
        {
            T component = owner.GetComponent<T>();
            if (component != null)
            {
                return component;
            }

            return owner.AddComponent<T>();
        }

        private static Transform EnsureAttackPoint(Transform playerTransform)
        {
            Transform attackPoint = playerTransform.Find("AttackPoint");
            if (attackPoint != null)
            {
                return attackPoint;
            }

            GameObject attackPointObject = new GameObject("AttackPoint");
            attackPointObject.transform.SetParent(playerTransform);
            attackPointObject.transform.localPosition = new Vector3(0.0f, 1.0f, 1.2f);
            attackPointObject.transform.localRotation = Quaternion.identity;
            return attackPointObject.transform;
        }

        private static void SetObjectReference(UnityEngine.Object targetObject, string propertyName, UnityEngine.Object value)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"{targetObject.GetType().Name}.{propertyName} 프로퍼티를 찾을 수 없습니다.");
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetObject);
        }

        private static void SetLayerMask(UnityEngine.Object targetObject, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"{targetObject.GetType().Name}.{propertyName} 프로퍼티를 찾을 수 없습니다.");
                return;
            }

            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetObject);
        }

        private static void SetFloat(UnityEngine.Object targetObject, string propertyName, float value)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"{targetObject.GetType().Name}.{propertyName} 프로퍼티를 찾을 수 없습니다.");
                return;
            }

            property.floatValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetObject);
        }

        private static void SetInt(UnityEngine.Object targetObject, string propertyName, int value)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"{targetObject.GetType().Name}.{propertyName} 프로퍼티를 찾을 수 없습니다.");
                return;
            }

            property.intValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetObject);
        }

        private static void SetString(UnityEngine.Object targetObject, string propertyName, string value)
        {
            SerializedObject serializedObject = new SerializedObject(targetObject);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property == null)
            {
                Debug.LogWarning($"{targetObject.GetType().Name}.{propertyName} 프로퍼티를 찾을 수 없습니다.");
                return;
            }

            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(targetObject);
        }

        private static void EnsureLayerExists(string layerName)
        {
            if (LayerMask.NameToLayer(layerName) >= 0)
            {
                return;
            }

            SerializedObject tagManagerObject = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProperty = tagManagerObject.FindProperty("layers");

            for (int index = 8; index < layersProperty.arraySize; index++)
            {
                SerializedProperty layerProperty = layersProperty.GetArrayElementAtIndex(index);
                if (!string.IsNullOrEmpty(layerProperty.stringValue))
                {
                    continue;
                }

                layerProperty.stringValue = layerName;
                tagManagerObject.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log($"Layer 생성: {layerName} (Index: {index})");
                return;
            }

            throw new InvalidOperationException($"사용 가능한 Layer 슬롯이 없습니다: {layerName}");
        }
    }
}
