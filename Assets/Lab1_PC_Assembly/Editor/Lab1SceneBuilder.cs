#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRARLab1.Editor
{
    public static class Lab1SceneBuilder
    {
        private const string GeneratedRoot = "Assets/Lab1_Generated";
        private const string ScenePath = GeneratedRoot + "/Scenes/Lab1_PC_Assembly.unity";
        private const string PrefabPath = GeneratedRoot + "/Prefabs/RAM_Module.prefab";

        [MenuItem("Tools/VR-AR Labs/Lab 1/Build PC Assembly Scene")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Lab 1", "Exit Play Mode before generating the scene.", "OK");
                return;
            }

            EnsureCleanGeneratedFolders();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Lab1_PC_Assembly";

            Material floorMat = CreateMaterial("Floor_Mat", new Color(0.16f, 0.19f, 0.23f));
            Material wallMat = CreateMaterial("Wall_Mat", new Color(0.72f, 0.76f, 0.80f));
            Material benchMat = CreateMaterial("Workbench_Mat", new Color(0.28f, 0.18f, 0.11f));
            Material metalMat = CreateMaterial("Metal_Mat", new Color(0.20f, 0.24f, 0.28f));
            Material pcbMat = CreateMaterial("PCB_Mat", new Color(0.04f, 0.28f, 0.18f));
            Material blueMat = CreateMaterial("AccentBlue_Mat", new Color(0.12f, 0.48f, 0.90f));
            Material orangeMat = CreateMaterial("PhysicsOrange_Mat", new Color(1.0f, 0.38f, 0.08f));
            Material yellowMat = CreateMaterial("TriggerMarker_Mat", new Color(1.0f, 0.82f, 0.12f));
            Material darkMat = CreateMaterial("Dark_Mat", new Color(0.06f, 0.07f, 0.09f));

            GameObject root = new GameObject("LAB 1 - PC ASSEMBLY TRAINER");
            GameObject environment = CreateGroup("01_Environment", root.transform);
            GameObject equipment = CreateGroup("02_Equipment", root.transform);
            GameObject prefabDemo = CreateGroup("03_Custom_Prefab", root.transform);
            GameObject physicsDemo = CreateGroup("04_Physics_Demo", root.transform);
            GameObject player = CreateGroup("05_Player", root.transform);
            GameObject lighting = CreateGroup("06_Lighting", root.transform);

            // Room / workshop environment.
            CreateCube("Floor", new Vector3(0f, -0.15f, 0f), new Vector3(12f, 0.3f, 10f), floorMat, environment.transform);
            CreateCube("BackWall", new Vector3(0f, 2.5f, 4.85f), new Vector3(12f, 5f, 0.3f), wallMat, environment.transform);
            CreateCube("LeftWall", new Vector3(-5.85f, 2.5f, 0f), new Vector3(0.3f, 5f, 10f), wallMat, environment.transform);
            CreateCube("RightWall", new Vector3(5.85f, 2.5f, 0f), new Vector3(0.3f, 5f, 10f), wallMat, environment.transform);

            // Workbench + support stand.
            CreateCube("WorkbenchTop", new Vector3(0f, 1.05f, 1.6f), new Vector3(5.0f, 0.25f, 2.2f), benchMat, equipment.transform);
            CreateCube("BenchLeg_L", new Vector3(-2.1f, 0.48f, 1.6f), new Vector3(0.35f, 1.0f, 1.7f), metalMat, equipment.transform);
            CreateCube("BenchLeg_R", new Vector3(2.1f, 0.48f, 1.6f), new Vector3(0.35f, 1.0f, 1.7f), metalMat, equipment.transform);
            CreateCube("ToolStand", new Vector3(-3.7f, 0.75f, 2.7f), new Vector3(1.1f, 1.5f, 0.9f), metalMat, equipment.transform);

            // Simple PC case, motherboard, monitor and keyboard from primitives.
            GameObject pcCase = CreateCube("PC_Case", new Vector3(1.65f, 1.85f, 1.95f), new Vector3(1.25f, 1.45f, 0.95f), darkMat, equipment.transform);
            CreateCube("Case_Window", new Vector3(1.01f, 1.86f, 1.95f), new Vector3(0.03f, 1.08f, 0.72f), blueMat, pcCase.transform);
            CreateCube("Motherboard", new Vector3(0.35f, 1.27f, 1.65f), new Vector3(1.4f, 0.08f, 1.15f), pcbMat, equipment.transform);
            CreateCube("CPU_Socket", new Vector3(0.35f, 1.34f, 1.65f), new Vector3(0.38f, 0.05f, 0.38f), metalMat, equipment.transform);
            CreateCube("Monitor", new Vector3(-1.65f, 2.0f, 2.0f), new Vector3(1.65f, 1.0f, 0.12f), darkMat, equipment.transform);
            CreateCube("MonitorScreen", new Vector3(-1.65f, 2.0f, 1.93f), new Vector3(1.45f, 0.8f, 0.03f), blueMat, equipment.transform);
            CreateCube("MonitorStand", new Vector3(-1.65f, 1.43f, 2.0f), new Vector3(0.22f, 0.25f, 0.22f), metalMat, equipment.transform);
            CreateCube("Keyboard", new Vector3(-1.5f, 1.25f, 0.95f), new Vector3(1.55f, 0.08f, 0.48f), darkMat, equipment.transform);

            // Custom prefab: RAM module assembled from primitives.
            GameObject ramPrefabSource = CreateRamModule(pcbMat, darkMat, metalMat);
            ramPrefabSource.transform.SetParent(prefabDemo.transform);
            ramPrefabSource.transform.position = new Vector3(0.35f, 1.55f, 1.0f);
            PrefabUtility.SaveAsPrefabAsset(ramPrefabSource, PrefabPath);
            GameObject.DestroyImmediate(ramPrefabSource);

            GameObject prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            GameObject ramInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefabAsset, prefabDemo.transform);
            ramInstance.name = "RAM_Module_PREFAB_Instance";
            ramInstance.transform.position = new Vector3(0.35f, 1.40f, 1.0f);
            ramInstance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            // Physics: the orange cube falls through trigger volume, then hits the table.
            GameObject triggerZone = new GameObject("TriggerZone");
            triggerZone.transform.SetParent(physicsDemo.transform);
            triggerZone.transform.position = new Vector3(-0.25f, 2.4f, 1.45f);
            BoxCollider triggerCollider = triggerZone.AddComponent<BoxCollider>();
            triggerCollider.isTrigger = true;
            triggerCollider.size = new Vector3(1.8f, 1.6f, 1.5f);
            triggerZone.AddComponent<TriggerZoneReporter>();

            GameObject triggerMarker = CreateCube("TriggerMarker_Visible", new Vector3(0f, -0.75f, 0f), new Vector3(1.85f, 0.03f, 1.55f), yellowMat, triggerZone.transform);
            Object.DestroyImmediate(triggerMarker.GetComponent<BoxCollider>());

            GameObject dropCube = CreateCube("Physics_Test_Cube", new Vector3(-0.25f, 4.3f, 1.45f), new Vector3(0.45f, 0.45f, 0.45f), orangeMat, physicsDemo.transform);
            Rigidbody rb = dropCube.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            dropCube.AddComponent<PhysicsEventReporter>();

            // Additional test item with a collider, as requested by the assignment.
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.name = "Test_Sphere";
            sphere.transform.SetParent(physicsDemo.transform);
            sphere.transform.position = new Vector3(3.2f, 1.45f, 1.2f);
            sphere.transform.localScale = Vector3.one * 0.5f;
            ApplyMaterial(sphere, blueMat);

            // Test camera controlled from keyboard/mouse.
            GameObject cameraGo = new GameObject("TestCamera_WASD");
            cameraGo.transform.SetParent(player.transform);
            cameraGo.transform.position = new Vector3(0f, 2.4f, -6.2f);
            cameraGo.transform.rotation = Quaternion.Euler(9f, 0f, 0f);
            Camera camera = cameraGo.AddComponent<Camera>();
            camera.tag = "MainCamera";
            camera.clearFlags = CameraClearFlags.Skybox;
            camera.nearClipPlane = 0.05f;
            cameraGo.AddComponent<AudioListener>();
            cameraGo.AddComponent<CameraKeyboardController>();

            // Lighting.
            GameObject sun = new GameObject("Directional Light");
            sun.transform.SetParent(lighting.transform);
            sun.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            Light sunLight = sun.AddComponent<Light>();
            sunLight.type = LightType.Directional;
            sunLight.intensity = 1.2f;
            sunLight.shadows = LightShadows.Soft;

            GameObject fill = new GameObject("Workbench Fill Light");
            fill.transform.SetParent(lighting.transform);
            fill.transform.position = new Vector3(0f, 4f, 0.5f);
            Light fillLight = fill.AddComponent<Light>();
            fillLight.type = LightType.Point;
            fillLight.range = 8f;
            fillLight.intensity = 5f;

            // Keep demo objects visible in Hierarchy and save scene.
            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Lab 1 is ready",
                "Created scene:\n" + ScenePath +
                "\n\nCreated custom prefab:\n" + PrefabPath +
                "\n\nPress Play. The orange cube will enter the trigger and then collide with the workbench. Check Console messages.\n\nCamera: WASD, Q/E, arrows or RMB + mouse.",
                "OK");
        }

        private static void EnsureCleanGeneratedFolders()
        {
            if (AssetDatabase.IsValidFolder(GeneratedRoot))
                AssetDatabase.DeleteAsset(GeneratedRoot);

            CreateFolder("Assets", "Lab1_Generated");
            CreateFolder(GeneratedRoot, "Scenes");
            CreateFolder(GeneratedRoot, "Prefabs");
            CreateFolder(GeneratedRoot, "Materials");
        }

        private static void CreateFolder(string parent, string name)
        {
            if (!AssetDatabase.IsValidFolder(parent + "/" + name))
                AssetDatabase.CreateFolder(parent, name);
        }

        private static GameObject CreateGroup(string name, Transform parent)
        {
            GameObject group = new GameObject(name);
            group.transform.SetParent(parent);
            return group;
        }

        private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            ApplyMaterial(go, material);
            return go;
        }

        private static GameObject CreateRamModule(Material pcbMaterial, Material chipMaterial, Material metalMaterial)
        {
            GameObject root = new GameObject("RAM_Module_PREFAB");

            GameObject board = CreateCube("RAM_PCB", Vector3.zero, new Vector3(1.55f, 0.42f, 0.08f), pcbMaterial, root.transform);
            board.transform.localPosition = Vector3.zero;

            for (int i = 0; i < 6; i++)
            {
                float x = -0.58f + i * 0.23f;
                GameObject chip = CreateCube("MemoryChip_" + (i + 1), Vector3.zero, new Vector3(0.16f, 0.20f, 0.05f), chipMaterial, root.transform);
                chip.transform.localPosition = new Vector3(x, 0.02f, -0.065f);
            }

            for (int i = 0; i < 10; i++)
            {
                float x = -0.67f + i * 0.15f;
                GameObject contact = CreateCube("GoldContact_" + (i + 1), Vector3.zero, new Vector3(0.08f, 0.07f, 0.03f), metalMaterial, root.transform);
                contact.transform.localPosition = new Vector3(x, -0.21f, -0.055f);
            }

            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.size = new Vector3(1.6f, 0.48f, 0.12f);
            collider.center = Vector3.zero;
            return root;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            Material material = new Material(shader) { name = name };
            if (material.HasProperty("_BaseColor"))
                material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color"))
                material.SetColor("_Color", color);

            string path = GeneratedRoot + "/Materials/" + name + ".mat";
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        private static void ApplyMaterial(GameObject go, Material material)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = material;
        }
    }
}
#endif
