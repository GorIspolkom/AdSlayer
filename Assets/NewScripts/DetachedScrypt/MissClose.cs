using UnityEngine;

namespace Clicker.DetachedScrypts
{
    public class MissClose : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                bool isContainMouse = RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition, Camera.main);
                if (!isContainMouse)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}