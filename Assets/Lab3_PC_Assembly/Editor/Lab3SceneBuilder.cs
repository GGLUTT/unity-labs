#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRARLab3;

namespace VRARLab3.Editor
{
    public static class Lab3SceneBuilder
    {
        private const string Lab2ScenePath = "Assets/Lab2_Generated/Scenes/Lab2_PC_Assembly_VR.unity";
        private const string GeneratedRoot = "Assets/Lab3_Generated";
        private const string ScenePath = GeneratedRoot + "/Scenes/Lab3_PC_Assembly_SpatialUI.unity";

        private static readonly string[] TrackedRaycasterNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster"
        };

        private static readonly string[] XRUIInputModuleNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule"
        };

        private static readonly string[] PokeInteractorNames =
        {
            "UnityEngine.XR.Interaction.Toolkit.Interactors.XRPokeInteractor",
            "UnityEngine.XR.Interaction.Toolkit.XRPokeInteractor"
        };

        [MenuItem("Tools/VR-AR Labs/Lab 3/Build Spatial UI & Wizard Scene")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Lab 3", "Exit Play Mode before generating Lab 3.", "OK");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Lab2ScenePath) == null)
            {
                EditorUtility.DisplayDialog(
                    "Lab 3 - Lab 2 scene not found",
                    "First finish Lab 2 and make sure this scene exists:\n" + Lab2ScenePath,
                    "OK");
                return;
            }

            Type trackedRaycasterType = FindType(TrackedRaycasterNames);
            Type xrUiInputModuleType = FindType(XRUIInputModuleNames);
            if (trackedRaycasterType == null || xrUiInputModuleType == null)
            {
                EditorUtility.DisplayDialog(
                    "Lab 3 - XR UI components are missing",
                    "XR Interaction Toolkit UI components were not found.\n\nOpen Package Manager > XR Interaction Toolkit and make sure the package is installed, then run the builder again.",
                    "OK");
                return;
            }

            EnsureCleanGeneratedFolders();
            Scene scene = EditorSceneManager.OpenScene(Lab2ScenePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);

            GameObject lab2Root = GameObject.Find("LAB 2 - VR INTERACTION (XRI)");
            if (lab2Root != null)
                lab2Root.name = "LAB 2 BASE - VR INTERACTION (XRI)";

            GameObject root = new GameObject("LAB 3 - SPATIAL UI & WIZARD");
            GameObject uiGroup = CreateGroup("01_WorldSpace_UI", root.transform);
            GameObject interactionGroup = CreateGroup("02_UI_Interaction_Ray_Poke", root.transform);
            GameObject infoGroup = CreateGroup("03_Object_Info_Cards", root.transform);
            GameObject wizardGroup = CreateGroup("04_Wizard_StateMachine", root.transform);

            EnsureXRUIEventSystem(xrUiInputModuleType, interactionGroup.transform);
            EnsurePokeInteractors(interactionGroup.transform);

            Canvas canvas = CreateWorldCanvas(uiGroup.transform, trackedRaycasterType);
            GameObject tablet = CreatePanel("TabletPanel", canvas.transform, new Vector2(940, 670), new Color(0.035f, 0.055f, 0.085f, 0.97f));
            RectTransform tabletRt = tablet.GetComponent<RectTransform>();
            tabletRt.anchoredPosition = Vector2.zero;

            Text header = CreateText("Header", tablet.transform, "PC ASSEMBLY — SPATIAL UI", 36, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRect(header.rectTransform, new Vector2(0, 285), new Vector2(850, 60));

            Text stepTitle = CreateText("StepTitle", tablet.transform, "Крок 1/4 — Інструктаж", 28, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRect(stepTitle.rectTransform, new Vector2(0, 230), new Vector2(850, 48));

            GameObject pageHost = CreateUIObject("Pages", tablet.transform);
            SetRect(pageHost.GetComponent<RectTransform>(), new Vector2(0, 20), new Vector2(850, 360));

            GameObject page0 = CreatePage("Page_1_Instruction", pageHost.transform);
            CreateTextBlock(page0.transform,
                "Перед початком робіт:\n• огляньте стенд та модуль RAM;\n• переконайтеся, що живлення ПК вимкнене;\n• працюйте обережно з контактами модуля.",
                new Vector2(0, 60), new Vector2(790, 210), 25);
            Toggle safety = CreateToggle(page0.transform, "Підтверджую виконання правил безпеки", new Vector2(-140, -90));

            GameObject page1 = CreatePage("Page_2_Form", pageHost.transform);
            CreateTextBlock(page1.transform, "Заповніть дані перед перевіркою знань.", new Vector2(0, 130), new Vector2(790, 55), 24);
            InputField idInput = CreateInputField(page1.transform, "Ідентифікатор оператора", new Vector2(0, 55));
            Slider slider = CreateSlider(page1.transform, new Vector2(-60, -35));
            Text sliderValue = CreateText("SliderValue", page1.transform, "Значення: 0.50", 22, FontStyle.Normal, TextAnchor.MiddleLeft);
            SetRect(sliderValue.rectTransform, new Vector2(250, -35), new Vector2(250, 45));
            Button saveButton = CreateButton(page1.transform, "Зберегти дані", new Vector2(-160, -125), new Vector2(260, 58));
            Button resetFormButton = CreateButton(page1.transform, "Скинути поля", new Vector2(160, -125), new Vector2(260, 58));

            GameObject page2 = CreatePage("Page_3_Quiz", pageHost.transform);
            CreateTextBlock(page2.transform, "До якого роз'єму материнської плати встановлюється модуль оперативної пам'яті RAM?", new Vector2(0, 105), new Vector2(800, 95), 25);
            Button answerA = CreateButton(page2.transform, "A — PCI Express", new Vector2(-180, 10), new Vector2(300, 62));
            Button answerB = CreateButton(page2.transform, "B — DIMM", new Vector2(180, 10), new Vector2(300, 62));
            Text quizFeedback = CreateText("QuizFeedback", page2.transform, "Відповідь ще не обрано.", 22, FontStyle.Italic, TextAnchor.MiddleCenter);
            SetRect(quizFeedback.rectTransform, new Vector2(0, -90), new Vector2(760, 70));

            GameObject page3 = CreatePage("Page_4_Result", pageHost.transform);
            Text result = CreateText("ResultText", page3.transform, "Результат буде сформовано після завершення Wizard.", 24, FontStyle.Normal, TextAnchor.UpperLeft);
            SetRect(result.rectTransform, new Vector2(0, 0), new Vector2(790, 320));

            GameObject infoCard = CreatePanel("ObjectInfoCard", canvas.transform, new Vector2(520, 220), new Color(0.05f, 0.12f, 0.17f, 0.97f));
            RectTransform infoRt = infoCard.GetComponent<RectTransform>();
            infoRt.anchoredPosition = new Vector2(720, 160);
            Text infoTitle = CreateText("InfoTitle", infoCard.transform, "Інформація", 27, FontStyle.Bold, TextAnchor.MiddleLeft);
            SetRect(infoTitle.rectTransform, new Vector2(0, 65), new Vector2(460, 52));
            Text infoDescription = CreateText("InfoDescription", infoCard.transform, "Наведіть курсор на об'єкт стенда.", 20, FontStyle.Normal, TextAnchor.UpperLeft);
            SetRect(infoDescription.rectTransform, new Vector2(0, -25), new Vector2(460, 120));

            Text status = CreateText("StatusText", tablet.transform, "Підказка: клавіша M відкриває/закриває планшет у режимі симулятора.", 20, FontStyle.Italic, TextAnchor.MiddleLeft);
            SetRect(status.rectTransform, new Vector2(0, -215), new Vector2(850, 55));

            Button back = CreateButton(tablet.transform, "Назад", new Vector2(-270, -280), new Vector2(220, 60));
            Button next = CreateButton(tablet.transform, "Далі", new Vector2(0, -280), new Vector2(220, 60));
            Button resetWizard = CreateButton(tablet.transform, "Скинути Wizard", new Vector2(270, -280), new Vector2(220, 60));

            GameObject runtime = new GameObject("Lab3_UI_Runtime_Controller");
            runtime.transform.SetParent(wizardGroup.transform);
            Lab3UIController controller = runtime.AddComponent<Lab3UIController>();
            controller.tabletRoot = tablet;
            controller.pages = new[] { page0, page1, page2, page3 };
            controller.stepTitle = stepTitle;
            controller.statusText = status;
            controller.backButton = back;
            controller.nextButton = next;
            controller.safetyToggle = safety;
            controller.confidenceSlider = slider;
            controller.sliderValueText = sliderValue;
            controller.operatorIdInput = idInput;
            controller.quizFeedbackText = quizFeedback;
            controller.resultText = result;
            controller.infoCard = infoCard;
            controller.infoTitle = infoTitle;
            controller.infoDescription = infoDescription;

            UnityEventTools.AddPersistentListener(back.onClick, controller.PreviousStep);
            UnityEventTools.AddPersistentListener(next.onClick, controller.NextStep);
            UnityEventTools.AddPersistentListener(resetWizard.onClick, controller.ResetWizard);
            UnityEventTools.AddPersistentListener(saveButton.onClick, controller.SaveForm);
            UnityEventTools.AddPersistentListener(resetFormButton.onClick, controller.ResetForm);
            UnityEventTools.AddPersistentListener(answerA.onClick, controller.SelectWrongAnswer);
            UnityEventTools.AddPersistentListener(answerB.onClick, controller.SelectCorrectAnswer);

            page0.SetActive(true);
            page1.SetActive(false);
            page2.SetActive(false);
            page3.SetActive(false);

            AttachInfoTargets(infoGroup.transform);

            CreateInteractionNote(interactionGroup.transform,
                "UI Interaction configured:\n• XR Ray Interactors — from XR Origin / Starter Assets\n• Direct Poke — XRPokeInteractor helper objects\n• World-Space Canvas + TrackedDeviceGraphicRaycaster\n• XRUIInputModule on EventSystem");

            Selection.activeGameObject = root;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Lab 3 is ready",
                "Created scene:\n" + ScenePath + "\n\nIncluded:\n" +
                "- World-Space Canvas / tablet\n" +
                "- Ray + Direct Poke UI setup\n" +
                "- object information cards\n" +
                "- Toggle, Slider, InputField, Save/Reset buttons\n" +
                "- 4-step Wizard / State Machine with validation and final result\n\n" +
                "Open the scene and press Play. Use XR Interaction Simulator or keyboard M to show/hide the tablet.",
                "OK");
        }

        private static void EnsureCleanGeneratedFolders()
        {
            if (AssetDatabase.IsValidFolder(GeneratedRoot))
                AssetDatabase.DeleteAsset(GeneratedRoot);
            CreateFolder("Assets", "Lab3_Generated");
            CreateFolder(GeneratedRoot, "Scenes");
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

        private static Type FindType(params string[] fullNames)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (string fullName in fullNames)
                {
                    Type t = assembly.GetType(fullName, false);
                    if (t != null) return t;
                }
            }
            return null;
        }

        private static Canvas CreateWorldCanvas(Transform parent, Type trackedRaycasterType)
        {
            GameObject go = new GameObject("Spatial_Tablet_Canvas");
            go.transform.SetParent(parent);
            go.transform.position = new Vector3(0f, 2.15f, 0.45f);
            go.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            go.transform.localScale = Vector3.one * 0.0022f;

            RectTransform rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(1900, 900);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            go.AddComponent<CanvasScaler>();
            go.AddComponent(trackedRaycasterType);
            return canvas;
        }

        private static void EnsureXRUIEventSystem(Type xrUiInputModuleType, Transform parent)
        {
            EventSystem eventSystem = UnityEngine.Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventGo = new GameObject("EventSystem_XR_UI");
                eventGo.transform.SetParent(parent);
                eventSystem = eventGo.AddComponent<EventSystem>();
            }

            if (eventSystem.GetComponent(xrUiInputModuleType) == null)
                eventSystem.gameObject.AddComponent(xrUiInputModuleType);

            var standalone = eventSystem.GetComponent<StandaloneInputModule>();
            if (standalone != null)
                standalone.enabled = false;
        }

        private static void EnsurePokeInteractors(Transform parent)
        {
            Type pokeType = FindType(PokeInteractorNames);
            GameObject noteGroup = new GameObject("Direct_Poke_Interactors");
            noteGroup.transform.SetParent(parent);
            if (pokeType == null)
            {
                noteGroup.name += "_TYPE_NOT_FOUND";
                return;
            }

            CreatePokeHelper("Left_Poke_Interactor_Lab3", pokeType, FindTransformContains("Left Controller"), noteGroup.transform);
            CreatePokeHelper("Right_Poke_Interactor_Lab3", pokeType, FindTransformContains("Right Controller"), noteGroup.transform);
        }

        private static void CreatePokeHelper(string name, Type pokeType, Transform controller, Transform fallbackParent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(controller != null ? controller : fallbackParent, false);
            go.transform.localPosition = new Vector3(0f, 0f, 0.08f);
            go.AddComponent(pokeType);
        }

        private static Transform FindTransformContains(string part)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if (t.name.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0)
                        return t;
            }
            return null;
        }

        private static void AttachInfoTargets(Transform parent)
        {
            AddInfoTarget("RAM_Module_XR_Grab", "Модуль RAM", "Оперативна пам'ять. У тренажері модуль можна захопити контролером і встановити у відповідний DIMM-слот.", parent);
            AddInfoTarget("Motherboard", "Материнська плата", "Основна плата системного блока, що містить DIMM-слоти для модулів оперативної пам'яті.", parent);
            AddInfoTarget("Monitor", "Монітор", "Пристрій виведення інформації. Використовується для візуального контролю стану системи.", parent);
        }

        private static void AddInfoTarget(string namePart, string title, string description, Transform infoParent)
        {
            GameObject target = FindObjectByNameContains(namePart);
            if (target == null) return;
            Lab3ObjectInfoTarget comp = target.GetComponent<Lab3ObjectInfoTarget>();
            if (comp == null) comp = target.AddComponent<Lab3ObjectInfoTarget>();
            comp.title = title;
            comp.description = description;

            GameObject marker = new GameObject("InfoTarget_" + title.Replace(" ", "_"));
            marker.transform.SetParent(infoParent);
            marker.transform.position = target.transform.position;
        }

        private static GameObject FindObjectByNameContains(string part)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if (t.name.IndexOf(part, StringComparison.OrdinalIgnoreCase) >= 0)
                        return t.gameObject;
            }
            return null;
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

        private static GameObject CreatePage(string name, Transform parent)
        {
            GameObject page = CreateUIObject(name, parent);
            RectTransform rt = page.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return page;
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

        private static void CreateTextBlock(Transform parent, string textValue, Vector2 pos, Vector2 size, int fontSize)
        {
            Text text = CreateText("Text", parent, textValue, fontSize, FontStyle.Normal, TextAnchor.UpperLeft);
            SetRect(text.rectTransform, pos, size);
        }

        private static Button CreateButton(Transform parent, string label, Vector2 pos, Vector2 size)
        {
            GameObject go = CreatePanel("Button_" + label.Replace(" ", "_"), parent, size, new Color(0.08f, 0.32f, 0.52f, 1f));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
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
            GameObject root = CreateUIObject("SafetyToggle", parent);
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

        private static InputField CreateInputField(Transform parent, string placeholder, Vector2 pos)
        {
            GameObject go = CreatePanel("OperatorIdInput", parent, new Vector2(620, 62), new Color(0.09f, 0.13f, 0.18f, 1f));
            go.GetComponent<RectTransform>().anchoredPosition = pos;
            InputField field = go.AddComponent<InputField>();

            Text value = CreateText("Text", go.transform, "", 23, FontStyle.Normal, TextAnchor.MiddleLeft);
            RectTransform valueRt = value.rectTransform;
            valueRt.anchorMin = Vector2.zero;
            valueRt.anchorMax = Vector2.one;
            valueRt.offsetMin = new Vector2(18, 5);
            valueRt.offsetMax = new Vector2(-18, -5);
            field.textComponent = value;

            Text ph = CreateText("Placeholder", go.transform, placeholder, 22, FontStyle.Italic, TextAnchor.MiddleLeft);
            ph.color = new Color(0.7f, 0.75f, 0.82f, 0.75f);
            RectTransform phRt = ph.rectTransform;
            phRt.anchorMin = Vector2.zero;
            phRt.anchorMax = Vector2.one;
            phRt.offsetMin = new Vector2(18, 5);
            phRt.offsetMax = new Vector2(-18, -5);
            field.placeholder = ph;
            return field;
        }

        private static Slider CreateSlider(Transform parent, Vector2 pos)
        {
            GameObject root = CreateUIObject("ConfidenceSlider", parent);
            SetRect(root.GetComponent<RectTransform>(), pos, new Vector2(470, 52));
            Slider slider = root.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0.5f;

            GameObject bg = CreatePanel("Background", root.transform, new Vector2(450, 16), new Color(0.14f, 0.19f, 0.25f, 1f));
            GameObject fillArea = CreateUIObject("Fill Area", root.transform);
            SetRect(fillArea.GetComponent<RectTransform>(), Vector2.zero, new Vector2(430, 16));
            GameObject fill = CreatePanel("Fill", fillArea.transform, new Vector2(430, 16), new Color(0.15f, 0.65f, 0.95f, 1f));
            RectTransform fillRt = fill.GetComponent<RectTransform>();
            fillRt.anchorMin = new Vector2(0, 0.5f);
            fillRt.anchorMax = new Vector2(1, 0.5f);
            slider.fillRect = fillRt;

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

        private static void CreateInteractionNote(Transform parent, string note)
        {
            GameObject go = new GameObject("Interaction_Setup_Notes");
            go.transform.SetParent(parent);
            TextMesh text = go.AddComponent<TextMesh>();
            text.text = note;
            text.fontSize = 30;
            text.characterSize = 0.02f;
            text.anchor = TextAnchor.UpperLeft;
            text.color = Color.cyan;
            go.transform.position = new Vector3(-4.8f, 0.2f, -4.5f);
        }
    }
}
#endif
