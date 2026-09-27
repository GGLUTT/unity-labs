using UnityEngine;

namespace VRARLab3
{
    public class Lab3ObjectInfoTarget : MonoBehaviour
    {
        public string title = "Об'єкт";
        [TextArea(2, 5)] public string description = "Інформація про об'єкт.";

        private void OnMouseEnter()
        {
            if (Lab3UIController.Instance != null)
                Lab3UIController.Instance.ShowObjectInfo(title, description);
        }

        private void OnMouseDown()
        {
            if (Lab3UIController.Instance != null)
                Lab3UIController.Instance.ShowObjectInfo(title, description);
        }

        private void OnMouseExit()
        {
            if (Lab3UIController.Instance != null)
                Lab3UIController.Instance.HideObjectInfo();
        }
    }
}
