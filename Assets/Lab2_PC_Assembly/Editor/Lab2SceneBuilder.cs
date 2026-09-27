#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRARLab2.Editor
{
    public static class Lab2SceneBuilder
    {
        private const string Lab1ScenePath = "Assets/Lab1_Generated/Scenes/Lab1_PC_Assembly.unity";
        private const string GeneratedRoot = "Assets/Lab2_Generated";
        private const string ScenePath = GeneratedRoot + "/Scenes/Lab2_PC_Assembly_VR.unity";

        private static readonly string[] GrabTypeNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable",
            "UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable"
        };

        private static readonly string[] SocketTypeNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor",
            "UnityEngine.XR.Interaction.Toolkit.XRSocketInteractor"
        };

        private static readonly string[] TeleportAreaTypeNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation.TeleportationArea",
            "UnityEngine.XR.Interaction.Toolkit.TeleportationArea"
        };

        private static readonly string[] InteractionManagerTypeNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.XRInteractionManager"
        };

        [MenuItem("Tools/VR-AR Labs/Lab 2/Build VR Interaction Scene")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Lab 2", "Exit Play Mode before generating the VR scene.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Lab1ScenePath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Lab 2 - Lab 1 scene not found",
                    "First generate Lab 1 scene:\nTools > VR-AR Labs > Lab 1 > Build PC Assembly Scene",
                    "OK");
                return;
            }

            Type grabType = FindType(GrabTypeNames);
            Type socketType = FindType(SocketTypeNames);
            Type teleportType = FindType(TeleportAreaTypeNames);
            if (grabType == null || socketType == null || teleportType == null)
            {
                EditorUtility.DisplayDialog(
                    "Lab 2 - XR Interaction Toolkit is required",
                    "Install XR Interaction Toolkit in Window > Package Manager.\n\n" +
                    "Then import BOTH samples inside that package:\n" +
                    "1) Starter Assets\n" +
                    "2) XR Interaction Simulator (or XR Device Simulator / Legacy, if that is what your version shows)\n\n" +
                    "After Unity finishes importing/compiling, run this menu again.",
                    "OK");
                return;
            }

            string xrOriginPrefabPath = FindPrefabPath("XR Origin (XR Rig)", "Starter Assets");
            if (string.IsNullOrEmpty(xrOriginPrefabPath))
                xrOriginPrefabPath = FindPrefabPath("XR Origin", "Starter Assets");

            string simulatorPrefabPath = FindPrefabPath("XR Interaction Simulator", "XR Device Simulator");
            if (string.IsNullOrEmpty(simulatorPrefabPath))
                simulatorPrefabPath = FindPrefabPath("XR Device Simulator", "XR Device Simulator");

            if (string.IsNullOrEmpty(xrOriginPrefabPath) || string.IsNullOrEmpty(simulatorPrefabPath))
            {
                EditorUtility.DisplayDialog(
                    "Lab 2 - XRI samples are missing",
                    "XR Interaction Toolkit is installed, but its sample prefabs were not found.\n\n" +
                    "Open Window > Package Manager > XR Interaction Toolkit > Samples and import:\n" +
                    "- Starter Assets\n" +
                    "- XR Interaction Simulator (or XR Device Simulator / Legacy)\n\nThen run the builder again.",
                    "OK");
                return;
            }

            EnsureCleanGeneratedFolders();
            Scene scene = EditorSceneManager.OpenScene(Lab1ScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);

            GameObject lab1Root = GameObject.Find("LAB 1 - PC ASSEMBLY TRAINER");
            if (lab1Root != null)
                lab1Root.name = "LAB 1 BASE - PC ASSEMBLY TRAINER";

            DisableByName("TestCamera_WASD");
            DisableByName("04_Physics_Demo");

            GameObject root = new GameObject("LAB 2 - VR INTERACTION (XRI)");
            GameObject xrSetup = CreateGroup("01_XR_Setup", root.transform);
            GameObject interactables = CreateGroup("02_Interactables", root.transform);
            GameObject snap = CreateGroup("03_RAM_Snap_Zone", root.transform);
            GameObject locomotion = CreateGroup("04_Teleportation", root.transform);
            GameObject info = CreateGroup("05_Instructions", root.transform);

            EnsureInteractionManager(xrSetup.transform);

            GameObject xrOrigin = InstantiatePrefab(xrOriginPrefabPath, xrSetup.transform, "XR Origin (XR Rig) - Lab2");
            xrOrigin.transform.position = new Vector3(0f, 0f, -2.8f);
            xrOrigin.transform.rotation = Quaternion.identity;

            GameObject simulator = InstantiatePrefab(simulatorPrefabPath, xrSetup.transform, "XR Interaction Simulator - Lab2");
            simulator.transform.position = Vector3.zero;

            GameObject ram = FindObjectByNameContains("RAM_Module_PREFAB_Instance");
            if (ram == null)
                ram = FindObjectByNameContains("RAM_Module_PREFAB");

            if (ram == null)
            {
                EditorUtility.DisplayDialog("Lab 2", "RAM prefab instance was not found in Lab 1 scene.", "OK");
                return;
            }

            ram.transform.SetParent(interactables.transform, true);
            ram.name = "RAM_Module_XR_Grab";
            ram.transform.position = new Vector3(-0.8f, 1.45f, 0.85f);
            ram.transform.rotation = Quaternion.identity;

            Rigidbody ramBody = ram.GetComponent<Rigidbody>();
            if (ramBody == null)
                ramBody = ram.AddComponent<Rigidbody>();
            ramBody.mass = 0.25f;
            ramBody.useGravity = true;
            ramBody.isKinematic = false;
            ramBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            ramBody.interpolation = RigidbodyInterpolation.Interpolate;

            AddComponentIfMissing(ram, grabType);

            Material green = CreateMaterial("SnapZone_Green", new Color(0.12f, 0.85f, 0.35f));
            Material cyan = CreateMaterial("Teleport_Cyan", new Color(0.12f, 0.65f, 0.95f));
            Material dark = CreateMaterial("Info_Dark", new Color(0.05f, 0.07f, 0.10f));

            GameObject socketZone = new GameObject("RAM_Socket_Interactor");
            socketZone.transform.SetParent(snap.transform);
            socketZone.transform.position = new Vector3(0.35f, 1.48f, 1.43f);
            socketZone.transform.rotation = Quaternion.identity;

            BoxCollider socketCollider = socketZone.AddComponent<BoxCollider>();
            socketCollider.isTrigger = true;
            socketCollider.size = new Vector3(1.85f, 0.72f, 0.55f);
            AddComponentIfMissing(socketZone, socketType);
            socketZone.AddComponent<Lab2SnapZoneReporter>();

            GameObject attachPoint = new GameObject("AttachPoint_RAM");
            attachPoint.transform.SetParent(socketZone.transform);
            attachPoint.transform.localPosition = Vector3.zero;
            attachPoint.transform.localRotation = Quaternion.identity;
            TryAssignObjectReference(socketZone, socketType, "m_AttachTransform", attachPoint.transform);
            TryAssignObjectReference(socketZone, socketType, "attachTransform", attachPoint.transform);

            GameObject marker = CreateCube("SnapZone_Visual_Green", socketZone.transform.position, new Vector3(1.82f, 0.58f, 0.12f), green, snap.transform);
            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
                UnityEngine.Object.DestroyImmediate(markerCollider);

            GameObject floor = GameObject.Find("Floor");
            if (floor != null)
            {
                AddComponentIfMissing(floor, teleportType);
                floor.transform.SetParent(locomotion.transform, true);
            }

            CreateTeleportPad("Teleport_Pad_A", new Vector3(-2.8f, 0.03f, -1.8f), cyan, locomotion.transform);
            CreateTeleportPad("Teleport_Pad_B", new Vector3(2.8f, 0.03f, -1.8f), cyan, locomotion.transform);
            CreateTeleportPad("Teleport_Pad_C", new Vector3(0f, 0.03f, 2.8f), cyan, locomotion.transform);

            GameObject sign = CreateCube("VR_Instructions_Board", new Vector3(0f, 2.9f, 4.65f), new Vector3(4.6f, 1.15f, 0.08f), dark, info.transform);
            Collider signCollider = sign.GetComponent<Collider>();
            if (signCollider != null)
                UnityEngine.Object.DestroyImmediate(signCollider);

            GameObject textGo = new GameObject("Instructions_Text");
            textGo.transform.SetParent(info.transform);
            textGo.transform.position = new Vector3(0f, 2.9f, 4.60f);
            textGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh text = textGo.AddComponent<TextMesh>();
            text.text = "LAB 2 - VR PC ASSEMBLY\nGrab the RAM and place it in the GREEN socket\nUse XR Interaction Simulator for testing without a headset";
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
            text.fontSize = 40;
            text.characterSize = 0.055f;
            text.color = Color.white;

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Lab 2 is ready",
                "Created VR scene:\n" + ScenePath + "\n\n" +
                "Included:\n" +
                "- XR Origin (XR Rig)\n" +
                "- XR Interaction Simulator\n" +
                "- XR Grab Interactable on RAM\n" +
                "- XR Socket Interactor / Snap Zone\n" +
                "- Teleportation Area on the floor\n\n" +
                "Press Play to test without a headset.",
                "OK");
        }

        private static void EnsureCleanGeneratedFolders()
        {
            if (AssetDatabase.IsValidFolder(GeneratedRoot))
                AssetDatabase.DeleteAsset(GeneratedRoot);

            CreateFolder("Assets", "Lab2_Generated");
            CreateFolder(GeneratedRoot, "Scenes");
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

        private static void DisableByName(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null)
                go.SetActive(false);
        }

        private static GameObject FindObjectByNameContains(string part)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform[] all = root.GetComponentsInChildren<Transform>(true);
                foreach (Transform t in all)
                {
                    if (t.name.Contains(part))
                        return t.gameObject;
                }
            }
            return null;
        }

        private static Type FindType(params string[] fullNames)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (string fullName in fullNames)
                {
                    Type type = assembly.GetType(fullName, false);
                    if (type != null)
                        return type;
                }
            }
            return null;
        }

        private static Component AddComponentIfMissing(GameObject target, Type type)
        {
            Component existing = target.GetComponent(type);
            return existing != null ? existing : target.AddComponent(type);
        }

        private static void EnsureInteractionManager(Transform parent)
        {
            Type managerType = FindType(InteractionManagerTypeNames);
            if (managerType == null)
                return;

            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Component existing = root.GetComponentInChildren(managerType, true);
                if (existing != null)
                    return;
            }

            GameObject manager = new GameObject("XR Interaction Manager");
            manager.transform.SetParent(parent);
            manager.AddComponent(managerType);
        }

        private static string FindPrefabPath(string exactName, string preferredPathPart)
        {
            string[] guids = AssetDatabase.FindAssets(exactName + " t:Prefab");
            string best = null;
            int bestScore = int.MinValue;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(path);
                int score = 0;
                if (string.Equals(fileName, exactName, StringComparison.OrdinalIgnoreCase)) score += 100;
                if (!string.IsNullOrEmpty(preferredPathPart) && path.IndexOf(preferredPathPart, StringComparison.OrdinalIgnoreCase) >= 0) score += 50;
                if (path.IndexOf("Samples", StringComparison.OrdinalIgnoreCase) >= 0) score += 10;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = path;
                }
            }

            return best;
        }

        private static GameObject InstantiatePrefab(string path, Transform parent, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, true);
            return instance;
        }

        private static void TryAssignObjectReference(GameObject target, Type componentType, string propertyName, UnityEngine.Object value)
        {
            Component component = target.GetComponent(componentType);
            if (component == null)
                return;

            SerializedObject serializedObject = new SerializedObject(component);
            SerializedProperty property = serializedObject.FindProperty(propertyName);
            if (property != null && property.propertyType == SerializedPropertyType.ObjectReference)
            {
                property.objectReferenceValue = value;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            return go;
        }

        private static void CreateTeleportPad(string name, Vector3 position, Material material, Transform parent)
        {
            GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pad.name = name;
            pad.transform.SetParent(parent);
            pad.transform.position = position;
            pad.transform.localScale = new Vector3(0.75f, 0.02f, 0.75f);
            Renderer renderer = pad.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            Collider collider = pad.GetComponent<Collider>();
            if (collider != null)
                UnityEngine.Object.DestroyImmediate(collider);
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader) { name = name };
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            else if (material.HasProperty("_Color")) material.SetColor("_Color", color);
            AssetDatabase.CreateAsset(material, GeneratedRoot + "/Materials/" + name + ".mat");
            return material;
        }
    }
}
#endif
