using UnityEngine;
using UnityEngine.UI;

namespace VRARLab4
{
    public class Lab4HandUIController : MonoBehaviour
    {
        public Toggle handSafetyToggle;
        public Slider fingerSlider;
        public Text sliderValueText;
        public Text interactionStatusText;
        public Lab4AssemblyController assemblyController;

        private void Start()
        {
            if (fingerSlider != null)
            {
                fingerSlider.onValueChanged.AddListener(OnSliderChanged);
                OnSliderChanged(fingerSlider.value);
            }
            if (handSafetyToggle != null)
                handSafetyToggle.onValueChanged.AddListener(OnSafetyChanged);
        }

        public void OnSliderChanged(float value)
        {
            if (sliderValueText != null)
                sliderValueText.text = "Чутливість пальця: " + value.ToString("0.00");
        }

        public void OnSafetyChanged(bool enabled)
        {
            if (interactionStatusText != null)
                interactionStatusText.text = enabled
                    ? "Hand Tracking активний: виконайте pinch/grab або poke."
                    : "Увімкніть підтвердження перед взаємодією.";
        }

        public void PokeTestPressed()
        {
            if (interactionStatusText != null)
                interactionStatusText.text = "Poke Interaction: кнопка натиснута вказівним пальцем.";
            Debug.Log("[Lab4][Poke] UI button activated by hand interaction.", this);
        }

        public void ResetAssembly()
        {
            if (assemblyController != null)
                assemblyController.ResetAssembly();
        }
    }
}
