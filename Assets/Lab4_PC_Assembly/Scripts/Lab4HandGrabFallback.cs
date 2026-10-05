using UnityEngine;

namespace VRARLab4
{
    /// <summary>
    /// Lab-only compatibility bridge for XR Interaction Simulator synthetic hand poses.
    /// The simulator uses M=Pinch, K=Grab and O=Open in the English keyboard layout.
    /// If XRI package-version bindings do not select the XRGrabInteractable, this bridge
    /// performs the same lab interaction using the visible right-hand pinch pose.
    /// </summary>
    public class Lab4HandGrabFallback : MonoBehaviour
    {
        public Transform part;
        public Transform handAttach;
        public Lab4AssemblyController assemblyController;
        public float grabDistance = 0.9f;
        public KeyCode pinchKey = KeyCode.M;
        public KeyCode grabKey = KeyCode.K;
        public KeyCode releaseKey = KeyCode.O;

        private Rigidbody body;
        private Transform originalParent;
        private bool originalUseGravity;
        private bool holding;

        private void Start()
        {
            ResolveReferences();
            if (part != null)
            {
                body = part.GetComponent<Rigidbody>();
                originalParent = part.parent;
                if (body != null)
                    originalUseGravity = body.useGravity;
            }
        }

        private void Update()
        {
            ResolveReferences();
            if (part == null || handAttach == null)
                return;

            if (!holding)
            {
                if ((Input.GetKeyDown(pinchKey) || Input.GetKeyDown(grabKey)) &&
                    Vector3.Distance(handAttach.position, part.position) <= grabDistance)
                {
                    BeginGrab();
                }
                return;
            }

            if (assemblyController != null && assemblyController.snapTarget != null &&
                Vector3.Distance(part.position, assemblyController.snapTarget.position) <= assemblyController.snapDistance)
            {
                EndGrab(false);
                assemblyController.CompleteAssembly();
                return;
            }

            if (Input.GetKeyDown(releaseKey))
                EndGrab(true);
        }

        private void BeginGrab()
        {
            holding = true;
            if (body == null)
                body = part.GetComponent<Rigidbody>();

            if (body != null)
            {
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                body.useGravity = false;
                body.isKinematic = true;
            }

            part.SetParent(handAttach, true);
            Debug.Log("[Lab4][GrabFix] RAM grabbed by synthetic hand pose.", this);
        }

        private void EndGrab(bool restorePhysics)
        {
            holding = false;
            part.SetParent(originalParent, true);

            if (body != null && restorePhysics)
            {
                body.isKinematic = false;
                body.useGravity = originalUseGravity;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
            }
        }

        private void ResolveReferences()
        {
            if (part == null)
            {
                GameObject go = GameObject.Find("RAM_Hand_PinchGrab");
                if (go != null) part = go.transform;
            }

            if (assemblyController == null)
                assemblyController = FindFirstObjectByType<Lab4AssemblyController>();

            if (handAttach == null)
                handAttach = FindRightHandAttachAtRuntime();
        }

        private static Transform FindRightHandAttachAtRuntime()
        {
            Transform fallback = null;
            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                string n = t.name.ToLowerInvariant();
                if (!n.Contains("pinch") || !n.Contains("grab"))
                    continue;

                if (fallback == null) fallback = t;
                Transform p = t;
                while (p != null)
                {
                    if (p.name.ToLowerInvariant().Contains("right"))
                        return t;
                    p = p.parent;
                }
            }

            if (fallback != null)
                return fallback;

            foreach (Transform t in FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (t.name.ToLowerInvariant().Contains("right hand"))
                    return t;

            return null;
        }
    }
}
