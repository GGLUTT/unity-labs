using System.Collections;
using UnityEngine;

namespace VRARLab4
{
    public class Lab4PokeButtonReporter : MonoBehaviour
    {
        public Renderer buttonRenderer;
        public Color idleColor = new Color(0.1f, 0.45f, 0.8f, 1f);
        public Color pressedColor = new Color(0.2f, 0.95f, 0.4f, 1f);
        private bool busy;

        private void Start()
        {
            if (buttonRenderer == null)
                buttonRenderer = GetComponent<Renderer>();
            SetColor(idleColor);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (busy || other == null)
                return;

            string n = other.name.ToLowerInvariant();
            if (!n.Contains("hand") && !n.Contains("finger") && !n.Contains("poke") && !n.Contains("index"))
                return;

            StartCoroutine(PressRoutine());
        }

        private IEnumerator PressRoutine()
        {
            busy = true;
            SetColor(pressedColor);
            Debug.Log("[Lab4][Poke3D] 3D button pressed by hand/finger.", this);
            yield return new WaitForSeconds(0.35f);
            SetColor(idleColor);
            busy = false;
        }

        private void SetColor(Color color)
        {
            if (buttonRenderer != null && buttonRenderer.material != null)
                buttonRenderer.material.color = color;
        }
    }
}
