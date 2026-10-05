#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRARLab4;

namespace VRARLab4.Editor
{
    public static class Lab4SceneBuilder
    {
        private const string Lab3ScenePath = "Assets/Lab3_Generated/Scenes/Lab3_PC_Assembly_SpatialUI.unity";
        private const string GeneratedRoot = "Assets/Lab4_Generated";
        private const string ScenePath = GeneratedRoot + "/Scenes/Lab4_PC_Assembly_HandTracking.unity";

        private static readonly string[] GrabTypeNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable",
            "UnityEngine.XR.Interaction.Toolkit.XRGrabInteractable"
        };

        private static readonly string[] TrackedRaycasterNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster"
        };

        private static readonly string[] SimpleInteractableNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable",
            "UnityEngine.XR.Interaction.Toolkit.XRSimpleInteractable"
        };

        private static readonly string[] PokeFilterNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.Filtering.XRPokeFilter",
            "UnityEngine.XR.Interaction.Toolkit.Interactables.XRPokeFilter",
            "UnityEngine.XR.Interaction.Toolkit.XRPokeFilter"
        };

        [MenuItem("Tools/VR-AR Labs/Lab 4/Build Hand Tracking Scene")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Lab 4", "Exit Play Mode before generating Lab 4.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Lab3ScenePath) == null)
            {
                EditorUtility.DisplayDialog("Lab 4 - Lab 3 scene not found",
                    "First finish Lab 3 and make sure this scene exists:\n" + Lab3ScenePath, "OK");
                return;
            }

            string handsPrefabPath = FindHandsOriginPrefabPath();
            if (string.IsNullOrEmpty(handsPrefabPath))
            {
                EditorUtility.DisplayDialog("Lab 4 - Hands sample is missing",
                    "Open Window > Package Manager > XR Interaction Toolkit > Samples and import:\n" +
                    "- Hands Interaction Demo\n\n" +
                    "Also install XR Hands and import its HandVisualizer sample.\n" +
                    "Then run the builder again.", "OK");
                return;
            }

            Type grabType = FindComponentType(GrabTypeNames);
            Type trackedRaycasterType = FindComponentType(TrackedRaycasterNames);
            if (grabType == null || trackedRaycasterType == null)
            {
                EditorUtility.DisplayDialog("Lab 4 - XR components are missing",
                    "XR Interaction Toolkit components were not found. Make sure XRI is installed and Starter Assets are imported.", "OK");
                return;
            }

            EnsureGeneratedFolders();
            Scene scene = EditorSceneManager.OpenScene(Lab3ScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);

            GameObject lab3Root = FindSceneObject("LAB 3 - SPATIAL UI & WIZARD");
            if (lab3Root != null)
                lab3Root.name = "LAB 3 BASE - SPATIAL UI & WIZARD";

            GameObject root = new GameObject("LAB 4 - HAND TRACKING");
            GameObject handGroup = CreateGroup("01_Synthetic_Hands_Rig", root.transform);
            GameObject assemblyGroup = CreateGroup("02_Pinch_Grab_Assembly", root.transform);
            GameObject pokeGroup = CreateGroup("03_Poke_Controls", root.transform);
            GameObject feedbackGroup = CreateGroup("04_Visual_Feedback", root.transform);
            GameObject infoGroup = CreateGroup("05_Instructions", root.transform);

            GameObject oldOrigin = FindSceneObjectContains("XR Origin (XR Rig) - Lab2");
            Vector3 rigPosition = new Vector3(0f, 0f, -2.8f);
            Quaternion rigRotation = Quaternion.identity;
            if (oldOrigin != null)
            {
                rigPosition = oldOrigin.transform.position;
                rigRotation = oldOrigin.transform.rotation;
                oldOrigin.SetActive(false);
            }

            GameObject handRig = InstantiatePrefab(handsPrefabPath, handGroup.transform, "XR Origin Hands (XR Rig) - Lab4");
            handRig.transform.position = rigPosition;
            handRig.transform.rotation = rigRotation;

            GameObject oldSocket = FindSceneObject("RAM_Socket_Interactor");
            if (oldSocket != null)
                oldSocket.SetActive(false);

            Material waitingMat = CreateMaterial("Lab4_Target_Waiting", new Color(0.95f, 0.35f, 0.18f));
            Material successMat = CreateMaterial("Lab4_Target_Success", new Color(0.15f, 0.9f, 0.35f));
            Material panelMat = CreateMaterial("Lab4_Poke_Blue", new Color(0.1f, 0.45f, 0.8f));

            GameObject ram = FindSceneObjectContains("RAM_Module_XR_Grab");
            if (ram == null)
                ram = FindSceneObjectContains("RAM_Module");

            if (ram == null)
            {
                EditorUtility.DisplayDialog("Lab 4", "RAM object was not found in the previous lab scenes.", "OK");
                return;
            }

            ram.transform.SetParent(assemblyGroup.transform, true);
            ram.name = "RAM_Hand_PinchGrab";
            ram.SetActive(true);
            ram.transform.position = new Vector3(-0.85f, 1.48f, 0.9f);
            ram.transform.rotation = Quaternion.identity;

            Rigidbody body = ram.GetComponent<Rigidbody>();
            if (body == null)
                body = ram.AddComponent<Rigidbody>();
            body.mass = 0.2f;
            body.useGravity = true;
            body.isKinematic = false;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            Component grabInteractable = AddComponentIfMissing(ram, grabType);
            ConfigureGrabInteractable(grabInteractable, ram.GetComponent<Collider>());

            GameObject target = CreateCube("RAM_Hand_Snap_Target", new Vector3(0.55f, 1.48f, 1.05f), new Vector3(1.9f, 0.58f, 0.15f), waitingMat, assemblyGroup.transform);
            Collider targetCollider = target.GetComponent<Collider>();
            if (targetCollider != null)
                UnityEngine.Object.DestroyImmediate(targetCollider);

            GameObject snapPoint = new GameObject("RAM_Correct_Mount_Point");
            snapPoint.transform.SetParent(assemblyGroup.transform);
            snapPoint.transform.position = target.transform.position;
            snapPoint.transform.rotation = Quaternion.identity;

            GameObject lamp = CreateSphere("Assembly_Status_Lamp", new Vector3(2.0f, 1.85f, 1.1f), 0.16f, waitingMat, feedbackGroup.transform);

            Canvas canvas = CreateHandPanel(pokeGroup.transform, trackedRaycasterType, out Text statusText, out Toggle safetyToggle,
                out Slider fingerSlider, out Text sliderValue, out Button pokeButton, out Button resetButton);

            Lab4AssemblyController assembly = root.AddComponent<Lab4AssemblyController>();
            assembly.part = ram.transform;
            assembly.snapTarget = snapPoint.transform;
            assembly.targetRenderer = target.GetComponent<Renderer>();
            assembly.statusText = statusText;
            assembly.waitingColor = waitingMat.color;
            assembly.successColor = successMat.color;

            // Reliable fallback for the keyboard-driven synthetic hand poses in XR Interaction Simulator.
            // M = Pinch, K = Grab, O = Open/Release. This keeps Lab 4 testable even if
            // a package-version-specific XRI hand Select binding does not select XRGrabInteractable.
            Lab4HandGrabFallback fallbackGrab = root.AddComponent<Lab4HandGrabFallback>();
            fallbackGrab.part = ram.transform;
            fallbackGrab.handAttach = FindRightHandPinchPose(handRig.transform);
            fallbackGrab.assemblyController = assembly;
            fallbackGrab.grabDistance = 0.9f;

            Lab4HandUIController ui = canvas.gameObject.AddComponent<Lab4HandUIController>();
            ui.handSafetyToggle = safetyToggle;
            ui.fingerSlider = fingerSlider;
            ui.sliderValueText = sliderValue;
            ui.interactionStatusText = statusText;
            ui.assemblyController = assembly;
            UnityEventTools.AddPersistentListener(pokeButton.onClick, ui.PokeTestPressed);
            UnityEventTools.AddPersistentListener(resetButton.onClick, ui.ResetAssembly);

            Create3DPokeButton(pokeGroup.transform, panelMat);
            CreateInstructions(infoGroup.transform);

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Lab 4 is ready",
                "Created scene:\n" + ScenePath + "\n\nIncluded:\n" +
                "- XR Origin Hands (XR Rig) with synthetic hand support\n" +
                "- pinch/grab RAM assembly task\n" +
                "- correct mounting target with visual feedback\n" +
                "- world-space Toggle, Slider and Poke button\n" +
                "- 3D poke button object\n\n" +
                "Press Play and use XR Interaction Simulator in Hand mode. Use the on-screen simulator hotkeys for idle / poke / pinch / grab poses.", "OK");
        }


        [MenuItem("Tools/VR-AR Labs/Lab 4/Apply One-Click Grab Fix")]
        public static void ApplyOneClickGrabFix()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Lab 4 Grab Fix", "Exit Play Mode first.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            {
                EditorUtility.DisplayDialog("Lab 4 Grab Fix",
                    "Lab 4 generated scene was not found. Run Build Hand Tracking Scene first.", "OK");
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            GameObject root = FindSceneObject("LAB 4 - HAND TRACKING");
            GameObject ram = FindSceneObject("RAM_Hand_PinchGrab");
            GameObject handRig = FindSceneObjectContains("XR Origin Hands (XR Rig) - Lab4");
            GameObject snapPointGo = FindSceneObject("RAM_Correct_Mount_Point");

            if (root == null || ram == null || handRig == null || snapPointGo == null)
            {
                EditorUtility.DisplayDialog("Lab 4 Grab Fix",
                    "Required Lab 4 objects were not found. Rebuild Lab 4 once, then run this fix.", "OK");
                return;
            }

            Type grabType = FindComponentType(GrabTypeNames);
            if (grabType != null)
            {
                Component grab = AddComponentIfMissing(ram, grabType);
                ConfigureGrabInteractable(grab, ram.GetComponent<Collider>());
            }

            Lab4AssemblyController assembly = root.GetComponent<Lab4AssemblyController>();
            if (assembly == null)
                assembly = root.AddComponent<Lab4AssemblyController>();
            assembly.part = ram.transform;
            assembly.snapTarget = snapPointGo.transform;

            Lab4HandGrabFallback bridge = root.GetComponent<Lab4HandGrabFallback>();
            if (bridge == null)
                bridge = root.AddComponent<Lab4HandGrabFallback>();
            bridge.part = ram.transform;
            bridge.handAttach = FindRightHandPinchPose(handRig.transform);
            bridge.assemblyController = assembly;
            bridge.grabDistance = 0.9f;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            string attachName = bridge.handAttach != null ? bridge.handAttach.name : "not found (runtime search will be used)";
            EditorUtility.DisplayDialog("Lab 4 Grab Fix applied",
                "Done. Right-hand attach: " + attachName + "\n\n" +
                "Test: Play -> Hand mode -> Right Hand -> move near RAM -> press M (Pinch) or K (Grab) once. " +
                "Press O to release. Near the red slot RAM snaps automatically and the target turns green.", "OK");
        }

        private static void ConfigureGrabInteractable(Component grab, Collider collider)
        {
            if (grab == null)
                return;

            SerializedObject so = new SerializedObject(grab);
            SerializedProperty colliders = so.FindProperty("m_Colliders");
            if (colliders != null && colliders.isArray && collider != null)
            {
                colliders.arraySize = 1;
                colliders.GetArrayElementAtIndex(0).objectReferenceValue = collider;
            }

            SerializedProperty managerProp = so.FindProperty("m_InteractionManager");
            if (managerProp != null && managerProp.objectReferenceValue == null)
            {
                Component manager = FindInteractionManager();
                if (manager != null)
                    managerProp.objectReferenceValue = manager;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static Component FindInteractionManager()
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (!go.scene.IsValid())
                    continue;
                foreach (Component c in go.GetComponents<Component>())
                {
                    if (c == null) continue;
                    string n = c.GetType().Name;
                    if (n == "XRInteractionManager")
                        return c;
                }
            }
            return null;
        }

        private static Transform FindRightHandPinchPose(Transform handRig)
        {
            if (handRig == null)
                return null;

            Transform fallback = null;
            foreach (Transform t in handRig.GetComponentsInChildren<Transform>(true))
            {
                string n = t.name.ToLowerInvariant();
                if (!n.Contains("pinch") || !n.Contains("grab"))
                    continue;

                if (fallback == null) fallback = t;
                Transform p = t;
                while (p != null && p != handRig.parent)
                {
                    if (p.name.IndexOf("right", StringComparison.OrdinalIgnoreCase) >= 0)
                        return t;
                    p = p.parent;
                }
            }

            if (fallback != null)
                return fallback;

            foreach (Transform t in handRig.GetComponentsInChildren<Transform>(true))
                if (t.name.IndexOf("Right Hand", StringComparison.OrdinalIgnoreCase) >= 0)
                    return t;

            return handRig;
        }

        private static Canvas CreateHandPanel(Transform parent, Type trackedRaycasterType, out Text statusText,
            out Toggle safetyToggle, out Slider fingerSlider, out Text sliderValue, out Button pokeButton, out Button resetButton)
        {
            GameObject canvasGo = new GameObject("Hand_Tracking_WorldSpace_Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGo.transform.SetParent(parent, false);
            canvasGo.transform.position = new Vector3(-2.25f, 2.15f, 1.0f);
            canvasGo.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            canvasGo.transform.localScale = Vector3.one * 0.0022f;
            RectTransform canvasRt = canvasGo.GetComponent<RectTransform>();
            canvasRt.sizeDelta = new Vector2(900, 620);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            Camera main = Camera.main;
            if (main != null) canvas.worldCamera = main;
            canvasGo.AddComponent(trackedRaycasterType);

            GameObject panel = CreatePanel("Hand_Panel", canvasGo.transform, new Vector2(900, 620), new Color(0.04f, 0.07f, 0.11f, 0.96f));
            Text title = CreateText("Title", panel.transform, "LAB 4 — HAND TRACKING", 36, FontStyle.Bold, TextAnchor.MiddleCenter);
            SetRect(title.rectTransform, new Vector2(0, 250), new Vector2(820, 60));
            Text subtitle = CreateText("Subtitle", panel.transform, "Pinch/Grab • Poke • UI • Assembly", 24, FontStyle.Normal, TextAnchor.MiddleCenter);
            SetRect(subtitle.rectTransform, new Vector2(0, 205), new Vector2(820, 45));

            safetyToggle = CreateToggle(panel.transform, "Підтверджую режим Hand Tracking", new Vector2(0, 125));
            Text sliderLabel = CreateText("SliderLabel", panel.transform, "Poke / finger interaction sensitivity", 22, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(sliderLabel.rectTransform, new Vector2(-120, 55), new Vector2(580, 45));
            fingerSlider = CreateSlider(panel.transform, new Vector2(-80, 5));
            sliderValue = CreateText("SliderValue", panel.transform, "Чутливість пальця: 0.50", 21, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(sliderValue.rectTransform, new Vector2(280, 5), new Vector2(280, 45));

            pokeButton = CreateButton(panel.transform, "POKE TEST", new Vector2(-190, -95), new Vector2(260, 65));
            resetButton = CreateButton(panel.transform, "RESET RAM", new Vector2(190, -95), new Vector2(260, 65));
            statusText = CreateText("StatusText", panel.transform, "Увімкніть Hand mode у XR Interaction Simulator.", 22, FontStyle.Italic, TextAnchor.UpperLeft);
            SetRect(statusText.rectTransform, new Vector2(0, -205), new Vector2(800, 120));
            return canvas;
        }

        private static void Create3DPokeButton(Transform parent, Material material)
        {
            GameObject button = CreateCube("Poke_3D_Button", new Vector3(2.05f, 1.65f, 0.8f), new Vector3(0.42f, 0.42f, 0.16f), material, parent);
            BoxCollider c = button.GetComponent<BoxCollider>();
            c.isTrigger = true;
            Lab4PokeButtonReporter reporter = button.AddComponent<Lab4PokeButtonReporter>();
            reporter.buttonRenderer = button.GetComponent<Renderer>();

            Type simpleType = FindComponentType(SimpleInteractableNames);
            Type pokeFilterType = FindComponentType(PokeFilterNames);
            if (simpleType != null) AddComponentIfMissing(button, simpleType);
            if (pokeFilterType != null) AddComponentIfMissing(button, pokeFilterType);

            GameObject label = new GameObject("Poke_Button_Label");
            label.transform.SetParent(parent);
            label.transform.position = new Vector3(2.05f, 2.0f, 0.82f);
            label.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            TextMesh tm = label.AddComponent<TextMesh>();
            tm.text = "POKE\nBUTTON";
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.fontSize = 42;
            tm.characterSize = 0.025f;
            tm.color = Color.white;
        }

        private static void CreateInstructions(Transform parent)
        {
            GameObject go = new GameObject("HandTracking_Instructions");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(-4.7f, 0.25f, -4.3f);
            TextMesh tm = go.AddComponent<TextMesh>();
            tm.text = "LAB 4 HAND TRACKING\n1) XR Interaction Simulator -> Hand mode\n2) Use Poke pose for UI\n3) Use Pinch/Grab pose to take RAM\n4) Move RAM into the red slot; it turns green on success";
            tm.fontSize = 30;
            tm.characterSize = 0.02f;
            tm.anchor = TextAnchor.UpperLeft;
            tm.color = Color.cyan;
        }

        private static string FindHandsOriginPrefabPath()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Samples" });
            string fallback = null;
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.IndexOf("Hands Interaction Demo", StringComparison.OrdinalIgnoreCase) < 0)
                    continue;
                string name = Path.GetFileNameWithoutExtension(path);
                if (name.Equals("XR Origin Hands (XR Rig)", StringComparison.OrdinalIgnoreCase))
                    return path;
                if (name.IndexOf("XR Origin Hands", StringComparison.OrdinalIgnoreCase) >= 0)
                    fallback = path;
            }
            return fallback;
        }

        private static Type FindComponentType(params string[] fullNames)
        {
            foreach (Type type in TypeCache.GetTypesDerivedFrom<Component>())
            {
                foreach (string fullName in fullNames)
                    if (type.FullName == fullName)
                        return type;
            }
            return null;
        }

        private static GameObject InstantiatePrefab(string path, Transform parent, string name)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.name = name;
            instance.transform.SetParent(parent, true);
            return instance;
        }

        private static GameObject FindSceneObject(string exactName)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
                if (go.scene.IsValid() && go.name == exactName)
                    return go;
            return null;
        }

        private static GameObject FindSceneObjectContains(string part)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
                if (go.scene.IsValid() && go.name.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0)
                    return go;
            return null;
        }

        private static void EnsureGeneratedFolders()
        {
            if (AssetDatabase.IsValidFolder(GeneratedRoot))
                AssetDatabase.DeleteAsset(GeneratedRoot);
            CreateFolder("Assets", "Lab4_Generated");
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
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent);
            return go;
        }

        private static Material CreateMaterial(string name, Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material mat = new Material(shader) { name = name, color = color };
            string path = GeneratedRoot + "/Materials/" + name + ".mat";
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material mat, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = scale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            return go;
        }

        private static GameObject CreateSphere(string name, Vector3 position, float scale, Material mat, Transform parent)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = name;
            go.transform.SetParent(parent);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * scale;
            Renderer r = go.GetComponent<Renderer>();
            if (r != null) r.sharedMaterial = mat;
            Collider c = go.GetComponent<Collider>();
            if (c != null) UnityEngine.Object.DestroyImmediate(c);
            return go;
        }

        private static Component AddComponentIfMissing(GameObject go, Type type)
        {
            Component c = go.GetComponent(type);
            return c != null ? c : go.AddComponent(type);
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
        {
            GameObject go = CreateUIObject(name, parent);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            Image image = go.AddComponent<Image>();
            image.color = color;
            return go;
        }

        private static Font DefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        private static Text CreateText(string name, Transform parent, string value, int size, FontStyle style, TextAnchor anchor)
        {
            GameObject go = CreateUIObject(name, parent);
            Text text = go.AddComponent<Text>();
            text.font = DefaultFont();
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            GameObject go = CreatePanel("Button_" + label.Replace(" ", "_"), parent, size, new Color(0.08f, 0.32f, 0.52f, 1f));
            go.GetComponent<RectTransform>().anchoredPosition = pos;
            Button button = go.AddComponent<Button>();
            Text text = CreateText("Label", go.transform, label, 22, FontStyle.Bold, TextAnchor.MiddleCenter);
            RectTransform tr = text.rectTransform;
            tr.anchorMin = Vector2.zero;
            tr.anchorMax = Vector2.one;
            tr.offsetMin = Vector2.zero;
            tr.offsetMax = Vector2.zero;
            return button;
        }

        private static Toggle CreateToggle(Transform parent, string label, Vector2 pos)
        {
            GameObject root = CreateUIObject("HandModeToggle", parent);
            SetRect(root.GetComponent<RectTransform>(), pos, new Vector2(720, 60));
            Toggle toggle = root.AddComponent<Toggle>();
            GameObject bg = CreatePanel("Background", root.transform, new Vector2(44, 44), new Color(0.12f, 0.18f, 0.23f, 1f));
            bg.GetComponent<RectTransform>().anchoredPosition = new Vector2(-330, 0);
            GameObject check = CreatePanel("Checkmark", bg.transform, new Vector2(30, 30), new Color(0.2f, 0.85f, 0.4f, 1f));
            toggle.targetGraphic = bg.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            Text text = CreateText("Label", root.transform, label, 22, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(text.rectTransform, new Vector2(40, 0), new Vector2(620, 55));
            return toggle;
        }

        private static Slider CreateSlider(Transform parent, Vector2 pos)
        {
            GameObject root = CreateUIObject("FingerSensitivitySlider", parent);
            SetRect(root.GetComponent<RectTransform>(), pos, new Vector2(470, 52));
            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;
            GameObject bg = CreatePanel("Background", root.transform, new Vector2(450, 16), new Color(0.14f, 0.19f, 0.25f, 1f));
            GameObject fillArea = CreateUIObject("Fill Area", root.transform);
            SetRect(fillArea.GetComponent<RectTransform>(), Vector2.zero, new Vector2(430, 16));
            GameObject fill = CreatePanel("Fill", fillArea.transform, new Vector2(430, 16), new Color(0.15f, 0.65f, 0.95f, 1f));
            slider.fillRect = fill.GetComponent<RectTransform>();
            GameObject handleArea = CreateUIObject("Handle Slide Area", root.transform);
            SetRect(handleArea.GetComponent<RectTransform>(), Vector2.zero, new Vector2(430, 52));
            GameObject handle = CreatePanel("Handle", handleArea.transform, new Vector2(34, 34), Color.white);
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            return slider;
        }

        private static void SetRect(RectTransform rt, Vector2 anchoredPosition, Vector2 size)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPosition;
            rt.sizeDelta = size;
        }
    }
}
#endif
