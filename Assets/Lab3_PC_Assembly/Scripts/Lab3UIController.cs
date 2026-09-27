using UnityEngine;
using UnityEngine.UI;

namespace VRARLab3
{
    public class Lab3UIController : MonoBehaviour
    {
        public static Lab3UIController Instance { get; private set; }

        [Header("Main tablet")]
        public GameObject tabletRoot;
        public GameObject[] pages;
        public Text stepTitle;
        public Text statusText;
        public Button backButton;
        public Button nextButton;

        [Header("Form controls")]
        public Toggle safetyToggle;
        public Slider confidenceSlider;
        public Text sliderValueText;
        public InputField operatorIdInput;

        [Header("Quiz")]
        public Text quizFeedbackText;

        [Header("Result")]
        public Text resultText;

        [Header("Object information card")]
        public GameObject infoCard;
        public Text infoTitle;
        public Text infoDescription;

        private int currentStep;
        private int errors;
        private bool formSaved;
        private bool quizAnswered;
        private bool quizCorrect;
        private float startedAt;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            startedAt = Time.time;
            if (confidenceSlider != null)
            {
                confidenceSlider.onValueChanged.AddListener(OnSliderChanged);
                OnSliderChanged(confidenceSlider.value);
            }

            if (infoCard != null)
                infoCard.SetActive(false);

            ShowStep(0);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M) && tabletRoot != null)
                tabletRoot.SetActive(!tabletRoot.activeSelf);
        }

        public void ToggleTablet()
        {
            if (tabletRoot != null)
                tabletRoot.SetActive(!tabletRoot.activeSelf);
        }

        public void NextStep()
        {
            statusText.text = string.Empty;

            if (currentStep == 0)
            {
                if (safetyToggle == null || !safetyToggle.isOn)
                {
                    errors++;
                    statusText.text = "Увімкніть підтвердження техніки безпеки перед продовженням.";
                    return;
                }
            }
            else if (currentStep == 1)
            {
                if (!formSaved)
                {
                    errors++;
                    statusText.text = "Спочатку натисніть «Зберегти дані».";
                    return;
                }
            }
            else if (currentStep == 2)
            {
                if (!quizAnswered || !quizCorrect)
                {
                    errors++;
                    statusText.text = "Оберіть правильну відповідь перед завершенням тесту.";
                    return;
                }
            }

            if (currentStep < pages.Length - 1)
            {
                currentStep++;
                ShowStep(currentStep);
            }
        }

        public void PreviousStep()
        {
            if (currentStep > 0)
            {
                currentStep--;
                ShowStep(currentStep);
            }
        }

        public void SaveForm()
        {
            string id = operatorIdInput != null ? operatorIdInput.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(id))
            {
                errors++;
                formSaved = false;
                statusText.text = "Введіть ідентифікатор оператора.";
                return;
            }

            formSaved = true;
            statusText.text = "Дані збережено: ID = " + id + ", рівень впевненості = " + confidenceSlider.value.ToString("0.00");
            Debug.Log("[Lab3][Form] Saved operator=" + id + ", confidence=" + confidenceSlider.value.ToString("0.00"));
        }

        public void ResetForm()
        {
            formSaved = false;
            if (operatorIdInput != null)
                operatorIdInput.text = string.Empty;
            if (confidenceSlider != null)
                confidenceSlider.value = 0.5f;
            statusText.text = "Поля форми скинуто.";
        }

        public void SelectCorrectAnswer()
        {
            SelectQuizAnswer(true);
        }

        public void SelectWrongAnswer()
        {
            SelectQuizAnswer(false);
        }

        public void SelectQuizAnswer(bool correct)
        {
            quizAnswered = true;
            quizCorrect = correct;
            if (correct)
            {
                quizFeedbackText.text = "Правильно. Модуль RAM встановлюється у DIMM-слот.";
                statusText.text = "Відповідь прийнято.";
                Debug.Log("[Lab3][Wizard] Correct answer selected.");
            }
            else
            {
                errors++;
                quizFeedbackText.text = "Неправильно. Спробуйте ще раз.";
                statusText.text = "Зафіксовано помилку у тесті.";
            }
        }

        public void ResetWizard()
        {
            currentStep = 0;
            errors = 0;
            formSaved = false;
            quizAnswered = false;
            quizCorrect = false;
            startedAt = Time.time;
            if (safetyToggle != null) safetyToggle.isOn = false;
            if (operatorIdInput != null) operatorIdInput.text = string.Empty;
            if (confidenceSlider != null) confidenceSlider.value = 0.5f;
            if (quizFeedbackText != null) quizFeedbackText.text = "Відповідь ще не обрано.";
            ShowStep(0);
            statusText.text = "Wizard скинуто до початку.";
        }

        private void ShowStep(int index)
        {
            for (int i = 0; i < pages.Length; i++)
                if (pages[i] != null) pages[i].SetActive(i == index);

            currentStep = Mathf.Clamp(index, 0, pages.Length - 1);
            string[] titles = { "Крок 1/4 — Інструктаж", "Крок 2/4 — Форма", "Крок 3/4 — Перевірка знань", "Крок 4/4 — Результат" };
            if (stepTitle != null)
                stepTitle.text = titles[Mathf.Clamp(currentStep, 0, titles.Length - 1)];

            if (backButton != null)
                backButton.interactable = currentStep > 0;
            if (nextButton != null)
            {
                nextButton.gameObject.SetActive(currentStep < pages.Length - 1);
                Text label = nextButton.GetComponentInChildren<Text>();
                if (label != null) label.text = currentStep == pages.Length - 2 ? "Завершити" : "Далі";
            }

            if (currentStep == pages.Length - 1)
                BuildResult();
        }

        private void BuildResult()
        {
            if (resultText == null)
                return;

            float seconds = Time.time - startedAt;
            string id = operatorIdInput != null && !string.IsNullOrWhiteSpace(operatorIdInput.text) ? operatorIdInput.text.Trim() : "не вказано";
            float confidence = confidenceSlider != null ? confidenceSlider.value : 0f;
            resultText.text =
                "Тест завершено\n\n" +
                "Оператор: " + id + "\n" +
                "Час виконання: " + seconds.ToString("0.0") + " с\n" +
                "Помилки: " + errors + "\n" +
                "Рівень впевненості: " + confidence.ToString("0.00") + "\n\n" +
                "Результат зафіксовано локально для подальшої передачі на сервер у ЛР №5.";
            Debug.Log("[Lab3][Wizard] Finished. operator=" + id + ", time=" + seconds.ToString("0.0") + ", errors=" + errors);
        }

        private void OnSliderChanged(float value)
        {
            if (sliderValueText != null)
                sliderValueText.text = "Значення: " + value.ToString("0.00");
        }

        public void ShowObjectInfo(string title, string description)
        {
            if (infoCard == null) return;
            infoCard.SetActive(true);
            if (infoTitle != null) infoTitle.text = title;
            if (infoDescription != null) infoDescription.text = description;
        }

        public void HideObjectInfo()
        {
            if (infoCard != null)
                infoCard.SetActive(false);
        }
    }
}
