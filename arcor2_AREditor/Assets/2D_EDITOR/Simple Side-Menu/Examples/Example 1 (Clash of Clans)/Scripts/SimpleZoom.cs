// Simple Zoom
// Version: 1.0.0
// Author: Daniel Lochner

using UnityEngine;

namespace DanielLochner.Assets.SimpleSideMenu.SimpleZoom
{
    public class SimpleZoom : MonoBehaviour
    {
        #region Fields
        private RectTransform rectTransform;

        [SerializeField]
        float minSize = 0.5f;
        [SerializeField]
        float maxSize = 1;
        [SerializeField]
        private float zoomRate = 5;
        [SerializeField]
        private float zoom = 1f;
        [SerializeField]
        private ZoomTarget zoomTarget = ZoomTarget.Mouse;

        private Vector2 mouseLocalPosition;
        #endregion

        #region Properties
        public float MinSize { get { return minSize; } }
        public float MaxSize { get { return maxSize; } }
        public float ZoomRate { get { return zoomRate; } }
        public float Zoom { get { return zoom; } }
        #endregion

        #region Enumerators
        public enum ZoomTarget
        {
            Mouse,
            TopLeft,
            TopCentre,
            TopRight,
            MiddleLeft,
            MiddleCentre,
            MiddleRight,
            BottomLeft,
            BottomCentre,
            BottomRight
        }
        #endregion

        #region Methods
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        private void Update()
        {
            float scrollWheel = -Input.GetAxis("Mouse ScrollWheel");
            if (scrollWheel != 0)
            {
                ChangePivotAndZoom(scrollWheel);
            }
        }

        private void ChangePivotAndZoom(float scrollWheel)
        {
            //Change Pivot
            if ((scrollWheel > 0 && zoom != minSize) || (scrollWheel < 0 && zoom != maxSize))
            {
                Vector2 pivot = Vector2.zero;

                switch (zoomTarget)
                {
                    case ZoomTarget.Mouse:
                        Vector2 mouseScreenPosition = Input.mousePosition;
                        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, mouseScreenPosition, Camera.main, out mouseLocalPosition))
                        {
                            float x = rectTransform.pivot.x + (mouseLocalPosition.x / rectTransform.rect.width);
                            float y = rectTransform.pivot.y + (mouseLocalPosition.y / rectTransform.rect.height);
                            pivot = new Vector2(x, y);
                        }
                        break;
                    case ZoomTarget.TopLeft:
                        pivot = new Vector2(0, 1);
                        break;
                    case ZoomTarget.TopCentre:
                        pivot = new Vector2(0.5f, 1f);
                        break;
                    case ZoomTarget.TopRight:
                        pivot = new Vector2(1f, 1f);
                        break;
                    case ZoomTarget.MiddleLeft:
                        pivot = new Vector2(0f, 0.5f);
                        break;
                    case ZoomTarget.MiddleCentre:
                        pivot = new Vector2(0.5f, 0.5f);
                        break;
                    case ZoomTarget.MiddleRight:
                        pivot = new Vector2(1f, 0.5f);
                        break;
                    case ZoomTarget.BottomLeft:
                        pivot = new Vector2(0f, 0f);
                        break;
                    case ZoomTarget.BottomCentre:
                        pivot = new Vector2(0.5f, 0f);
                        break;
                    case ZoomTarget.BottomRight:
                        pivot = new Vector2(1f, 0f);
                        break;
                }
                SetPivot(rectTransform, pivot);
            }

            //Change Zoom
            float rate = 1 + zoomRate * Time.unscaledDeltaTime;
            if (scrollWheel > 0)
            {
                SetZoom(Mathf.Clamp(transform.localScale.x / rate, minSize, maxSize));
            }
            else
            {
                SetZoom(Mathf.Clamp(transform.localScale.x * rate, minSize, maxSize));
            }
        }
        public void SetPivot(RectTransform rectTransform, Vector2 pivot)
        {
            Vector3 deltaPosition = rectTransform.pivot - pivot;    // get change in pivot
            deltaPosition.Scale(rectTransform.rect.size);           // apply sizing
            deltaPosition.Scale(rectTransform.localScale);          // apply scaling
            deltaPosition = rectTransform.rotation * deltaPosition; // apply rotation

            rectTransform.pivot = pivot;                            // change the pivot
            rectTransform.localPosition -= deltaPosition;           // reverse the position change
        }
        private void SetZoom(float targetSize)
        {
            transform.localScale = new Vector3(targetSize, targetSize, 1);
            zoom = transform.localScale.x;
        }
        #endregion
    }
}