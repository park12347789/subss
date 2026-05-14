using System.Collections.Generic;
using System.Linq;
using SystemicOverload.Weapons;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SystemicOverload.EditorTools
{
    public static class HW0514WeaponDataSceneBuilder
    {
        private const string ScenePath = "Assets/01.Scenes/HW_0514_WeaponDataScene.unity";
        private const string WeaponFolderPath = "Assets/GameData/Weapons";
        private const int StartingAmmo = 3;

        [MenuItem("Tools/Systemic Overload/Homework/Build 0514 Weapon Data Scene")]
        public static void BuildSceneFromMenu()
        {
            BuildHomeworkScene();
        }

        public static void BuildSceneFromCommandLine()
        {
            BuildHomeworkScene();
        }

        private static void BuildHomeworkScene()
        {
            EnsureFolderPath("Assets/GameData");
            EnsureFolderPath(WeaponFolderPath);
            EnsureFolderPath("Assets/01.Scenes");

            List<WeaponData> weapons = new List<WeaponData>
            {
                CreateOrUpdateWeapon(
                    "Assets/GameData/Weapons/WPN_Assassin_Dagger.asset",
                    "단검",
                    "암살자",
                    damage: 18,
                    range: 2.0f,
                    attacksPerSecond: 3.2f,
                    usesAmmo: false,
                    ammoCostPerAttack: 0,
                    isMelee: true),
                CreateOrUpdateWeapon(
                    "Assets/GameData/Weapons/WPN_Assassin_PoisonDagger.asset",
                    "독 단검",
                    "암살자",
                    damage: 26,
                    range: 2.2f,
                    attacksPerSecond: 2.4f,
                    usesAmmo: false,
                    ammoCostPerAttack: 0,
                    isMelee: true),
                CreateOrUpdateWeapon(
                    "Assets/GameData/Weapons/WPN_Assassin_ThrowingKnife.asset",
                    "투척 칼",
                    "암살자",
                    damage: 14,
                    range: 16.0f,
                    attacksPerSecond: 2.8f,
                    usesAmmo: true,
                    ammoCostPerAttack: 1,
                    isMelee: false)
            };

            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            BuildSceneObjects(weapons);

            Scene scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings(ScenePath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[HW0514WeaponDataSceneBuilder] Built 0514 WeaponData homework scene and assets.");
        }

        private static void BuildSceneObjects(IReadOnlyList<WeaponData> weapons)
        {
            GameObject root = new GameObject("HW_0514_WeaponDataRoot");

            GameObject testerObject = new GameObject("WeaponTester");
            testerObject.transform.SetParent(root.transform);
            WeaponTester tester = testerObject.AddComponent<WeaponTester>();
            SerializedObject serializedTester = new SerializedObject(tester);
            SerializedProperty weaponsProperty = serializedTester.FindProperty("weapons");
            weaponsProperty.arraySize = weapons.Count;
            for (int index = 0; index < weapons.Count; index++)
            {
                weaponsProperty.GetArrayElementAtIndex(index).objectReferenceValue = weapons[index];
            }

            serializedTester.FindProperty("ammoWeaponIndex").intValue = 2;
            serializedTester.FindProperty("startingAmmo").intValue = StartingAmmo;
            serializedTester.FindProperty("consumeAmmoKeyName").stringValue = "Space";

            GameObject displayRoot = new GameObject("WeaponDisplayRoot");
            displayRoot.transform.SetParent(root.transform);

            CreateSceneTitle(root.transform);

            CreateWeaponDisplay(displayRoot.transform, weapons[0], new Vector3(-3.0f, 0.0f, 0.0f), Color.gray, PrimitiveType.Cube);
            CreateWeaponDisplay(displayRoot.transform, weapons[1], new Vector3(0.0f, 0.0f, 0.0f), new Color(0.25f, 0.65f, 0.3f), PrimitiveType.Cube);
            CreateWeaponDisplay(displayRoot.transform, weapons[2], new Vector3(3.0f, 0.0f, 0.0f), new Color(0.4f, 0.55f, 0.85f), PrimitiveType.Cylinder);
            TextMesh ammoStatusLabel = CreateAmmoStatusDisplay(displayRoot.transform, weapons[2], new Vector3(3.0f, 0.0f, 0.0f));
            serializedTester.FindProperty("ammoStatusLabel").objectReferenceValue = ammoStatusLabel;
            serializedTester.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(tester);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(root.transform);
            ground.transform.localScale = new Vector3(0.8f, 1.0f, 0.6f);

            GameObject lightObject = new GameObject("Directional Light");
            lightObject.transform.SetParent(root.transform);
            lightObject.transform.rotation = Quaternion.Euler(50.0f, -30.0f, 0.0f);
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.SetParent(root.transform);
            cameraObject.transform.position = new Vector3(0.0f, 4.8f, -8.5f);
            cameraObject.transform.rotation = Quaternion.Euler(30.0f, 0.0f, 0.0f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.fieldOfView = 48.0f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100.0f;
        }

        private static void CreateSceneTitle(Transform parent)
        {
            GameObject titleObject = new GameObject("HomeworkTitle_Label");
            titleObject.transform.SetParent(parent);
            titleObject.transform.position = new Vector3(0.0f, 2.6f, -0.4f);
            TextMesh title = titleObject.AddComponent<TextMesh>();
            title.text = "0514 H.W - ScriptableObject Weapon Data\nAssassin Weapon Set\nPress Space: use throwing knife ammo";
            title.characterSize = 0.22f;
            title.lineSpacing = 0.85f;
            title.anchor = TextAnchor.MiddleCenter;
            title.alignment = TextAlignment.Center;
            title.color = Color.black;
        }

        private static void CreateWeaponDisplay(
            Transform parent,
            WeaponData weaponData,
            Vector3 position,
            Color color,
            PrimitiveType primitiveType)
        {
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = weaponData.WeaponName + "_Pedestal";
            pedestal.transform.SetParent(parent);
            pedestal.transform.position = position + new Vector3(0.0f, 0.1f, 0.0f);
            pedestal.transform.localScale = new Vector3(1.4f, 0.2f, 1.4f);

            GameObject prop = GameObject.CreatePrimitive(primitiveType);
            prop.name = weaponData.WeaponName + "_Display";
            prop.transform.SetParent(parent);
            prop.transform.position = position + new Vector3(0.0f, 0.85f, 0.0f);
            prop.transform.rotation = Quaternion.Euler(0.0f, 0.0f, primitiveType == PrimitiveType.Cylinder ? 90.0f : 35.0f);
            prop.transform.localScale = primitiveType == PrimitiveType.Cylinder
                ? new Vector3(0.12f, 0.9f, 0.12f)
                : new Vector3(0.18f, 1.0f, 0.18f);

            Renderer renderer = prop.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = CreateDisplayMaterial(weaponData.name + "_Preview", color);
            }

            GameObject labelObject = new GameObject(weaponData.WeaponName + "_Label");
            labelObject.transform.SetParent(parent);
            labelObject.transform.position = position + new Vector3(0.0f, 1.55f, -0.55f);
            TextMesh label = labelObject.AddComponent<TextMesh>();
            label.text = BuildDisplayLabel(weaponData);
            label.characterSize = 0.16f;
            label.lineSpacing = 0.85f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.color = Color.black;
        }

        private static string BuildDisplayLabel(WeaponData weaponData)
        {
            string ammoText = weaponData.UsesAmmo ? $"Cost {weaponData.AmmoCostPerAttack}" : "No Ammo";
            string rangeType = weaponData.IsMelee ? "Melee" : "Ranged";
            return
                $"{weaponData.WeaponName}\n" +
                $"DMG {weaponData.Damage} / RNG {weaponData.Range:0.#}\n" +
                $"SPD {weaponData.AttacksPerSecond:0.#}/s\n" +
                $"{ammoText} / {rangeType}";
        }

        private static TextMesh CreateAmmoStatusDisplay(Transform parent, WeaponData weaponData, Vector3 position)
        {
            GameObject ammoObject = new GameObject(weaponData.WeaponName + "_AmmoStatus");
            ammoObject.transform.SetParent(parent);
            ammoObject.transform.position = position + new Vector3(0.0f, 2.2f, -0.55f);
            TextMesh ammoText = ammoObject.AddComponent<TextMesh>();
            ammoText.text = $"{weaponData.WeaponName} Ammo\n{StartingAmmo}/{StartingAmmo}\nReady";
            ammoText.characterSize = 0.19f;
            ammoText.lineSpacing = 0.85f;
            ammoText.anchor = TextAnchor.MiddleCenter;
            ammoText.alignment = TextAlignment.Center;
            ammoText.color = Color.black;
            return ammoText;
        }

        private static Material CreateDisplayMaterial(string name, Color color)
        {
            EnsureFolderPath("Assets/GameData/Weapons/PreviewMaterials");

            string materialPath = $"Assets/GameData/Weapons/PreviewMaterials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                {
                    shader = Shader.Find("Standard");
                }

                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static WeaponData CreateOrUpdateWeapon(
            string assetPath,
            string weaponName,
            string jobName,
            int damage,
            float range,
            float attacksPerSecond,
            bool usesAmmo,
            int ammoCostPerAttack,
            bool isMelee)
        {
            WeaponData weaponData = AssetDatabase.LoadAssetAtPath<WeaponData>(assetPath);
            if (weaponData == null)
            {
                weaponData = ScriptableObject.CreateInstance<WeaponData>();
                AssetDatabase.CreateAsset(weaponData, assetPath);
            }

            SerializedObject serializedWeapon = new SerializedObject(weaponData);
            serializedWeapon.FindProperty("weaponName").stringValue = weaponName;
            serializedWeapon.FindProperty("jobName").stringValue = jobName;
            serializedWeapon.FindProperty("damage").intValue = damage;
            serializedWeapon.FindProperty("range").floatValue = range;
            serializedWeapon.FindProperty("attacksPerSecond").floatValue = attacksPerSecond;
            serializedWeapon.FindProperty("usesAmmo").boolValue = usesAmmo;
            serializedWeapon.FindProperty("ammoCostPerAttack").intValue = usesAmmo ? Mathf.Max(1, ammoCostPerAttack) : 0;
            serializedWeapon.FindProperty("isMelee").boolValue = isMelee;
            serializedWeapon.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(weaponData);
            return weaponData;
        }

        private static void AddSceneToBuildSettings(string scenePath)
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
            if (scenes.Any(scene => scene.path == scenePath))
            {
                return;
            }

            scenes.Add(new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolderPath(string assetPath)
        {
            string[] segments = assetPath.Split('/');
            string currentPath = segments[0];
            for (int index = 1; index < segments.Length; index++)
            {
                string nextPath = currentPath + "/" + segments[index];
                if (!AssetDatabase.IsValidFolder(nextPath))
                {
                    AssetDatabase.CreateFolder(currentPath, segments[index]);
                }

                currentPath = nextPath;
            }
        }
    }
}
