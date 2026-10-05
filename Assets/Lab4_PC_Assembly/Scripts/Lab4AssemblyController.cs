using UnityEngine;
using UnityEngine.UI;

namespace VRARLab4
{
    public class Lab4AssemblyController : MonoBehaviour
    {
        [Header("Assembly")]
        public Transform part;
        public Transform snapTarget;
        public Renderer targetRenderer;
        public Text statusText;
        public float snapDistance = 0.22f;

        [Header("Feedback")]
        public Color waitingColor = new Color(0.95f, 0.35f, 0.18f, 1f);
        public Color successColor = new Color(0.15f, 0.9f, 0.35f, 1f);

        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool complete;

        private void Start()
        {
            if (part != null)
            {
                startPosition = part.position;
                startRotation = part.rotation;
            }
            SetTargetColor(waitingColor);
            SetStatus("Візьміть RAM жестом pinch/grab і встановіть у монтажний паз.");
        }

        private void Update()
        {
            if (complete || part == null || snapTarget == null)
                return;

            if (Vector3.Distance(part.position, snapTarget.position) <= snapDistance)
                CompleteAssembly();
        }

        public void CompleteAssembly()
        {
            if (complete || part == null || snapTarget == null)
                return;

            complete = true;
            part.position = snapTarget.position;
            part.rotation = snapTarget.rotation;

            Rigidbody body = part.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.isKinematic = true;
            }

            SetTargetColor(successColor);
            SetStatus("УСПІХ: RAM правильно зафіксована у монтажному пазі.");
            Debug.Log("[Lab4][HandAssembly] RAM fixed in the correct slot.", this);
        }

        public void ResetAssembly()
        {
            if (part == null)
                return;

            complete = false;
            part.position = startPosition;
            part.rotation = startRotation;
            Rigidbody body = part.GetComponent<Rigidbody>();
            if (body != null)
            {
                body.isKinematic = false;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
            SetTargetColor(waitingColor);
            SetStatus("Скинуто. Візьміть RAM жестом pinch/grab і встановіть у паз.");
            Debug.Log("[Lab4][HandAssembly] Assembly reset.", this);
        }

        private void SetTargetColor(Color color)
        {
            if (targetRenderer != null && targetRenderer.material != null)
                targetRenderer.material.color = color;
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
                statusText.text = message;
        }
    }
}
