using UnityEngine;

namespace VRARLab2
{
    public class Lab2SnapZoneReporter : MonoBehaviour
    {
        [SerializeField] private string requiredNamePart = "RAM";
        private bool reportedInside;

        private void OnTriggerEnter(Collider other)
        {
            Transform target = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
            if (target == null || !target.name.Contains(requiredNamePart))
                return;

            if (!reportedInside)
            {
                reportedInside = true;
                Debug.Log("[Lab2][SnapZone] RAM entered the XR socket zone: " + target.name, this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Transform target = other.attachedRigidbody != null ? other.attachedRigidbody.transform : other.transform;
            if (target != null && target.name.Contains(requiredNamePart))
                reportedInside = false;
        }
    }
}
