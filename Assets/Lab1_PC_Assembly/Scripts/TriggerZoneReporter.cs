using UnityEngine;

namespace VRARLab1
{
    /// <summary>
    /// Trigger-zone side of the physics demonstration.
    /// The generator configures the BoxCollider as Is Trigger.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TriggerZoneReporter : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Lab1][TriggerZone] ENTER -> {other.name}", this);
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"[Lab1][TriggerZone] EXIT -> {other.name}", this);
        }
    }
}
