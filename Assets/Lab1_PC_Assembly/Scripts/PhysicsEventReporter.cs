using UnityEngine;

namespace VRARLab1
{
    /// <summary>
    /// Demonstrates OnCollisionEnter and OnTriggerEnter for Lab 1.
    /// Attach to a Rigidbody object. It writes clear messages to the Console
    /// and changes the object color so the event is also visible in Game view.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PhysicsEventReporter : MonoBehaviour
    {
        public Color triggerColor = new Color(1f, 0.65f, 0.1f, 1f);
        public Color collisionColor = new Color(0.15f, 0.85f, 0.35f, 1f);

        private Renderer cachedRenderer;

        private void Awake()
        {
            cachedRenderer = GetComponent<Renderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Lab1][Trigger] {name} entered trigger: {other.name}", this);
            SetColor(triggerColor);
        }

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"[Lab1][Collision] {name} collided with: {collision.gameObject.name}. Relative speed: {collision.relativeVelocity.magnitude:F2}", this);
            SetColor(collisionColor);
        }

        private void SetColor(Color color)
        {
            if (cachedRenderer == null) return;

            Material materialInstance = cachedRenderer.material;
            if (materialInstance.HasProperty("_BaseColor"))
                materialInstance.SetColor("_BaseColor", color);
            else if (materialInstance.HasProperty("_Color"))
                materialInstance.SetColor("_Color", color);
        }
    }
}
