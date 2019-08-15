// Simple Side-Menu
// Version: 1.0.0
// Author: Daniel Lochner

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DanielLochner.Assets.SimpleSideMenu
{
    [AddComponentMenu("UI/Simple Side-Menu")]
    public class SimpleSideMenu : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IInitializePotentialDragHandler
    {
        #region Fields
        private Vector2 closedPosition, openPosition, startPosition, previousPosition, releaseVelocity, dragVelocity;
        private GameObject overlay;
        private RectTransform rectTransform;
        private State currentState, targetState;
        private float thresholdStateChangeDistance = 10f, previousTime;
        private bool dragging, potentialDrag;

        public Placement placement = Placement.Left;
        public State defaultState = State.Closed;
        public float transitionSpeed = 10f;
        public float thresholdDragSpeed = 0f;
        public float thresholdDraggedFraction = 0.5f;
        public GameObject handle = null;
        public bool handleDraggable = true;
        public bool menuDraggable = false;
        public bool handleToggleStateOnPressed = true;
        public bool useOverlay = true;
        public Color overlayColour = new Color(0, 0, 0, 0.25f);
        public bool overlayCloseOnPressed = true;
        #endregion

        #region Properties
        public State CurrentState { get { return currentState; } }
        public State TargetState { get { return targetState; } }
        public float StateProgress { get { return ((rectTransform.anchoredPosition - closedPosition).magnitude / ((placement == Placement.Left || placement == Placement.Right) ? rectTransform.rect.width : rectTransform.rect.height)); } }
        #endregion

        #region Enumerators
        public enum Placement
        {
            Left,
            Right,
            Top,
            Bottom
        }
        public enum State
        {
            Closed,
            Open
        }
        #endregion

        #region Methods
        private void Awake()
        {
            if (Validate())
            {
                Setup();
            }
            else
            {
                throw new Exception("Invalid inspector input.");
            }
        }
        private void Update()
        {
            OnStateUpdate();
            OnOverlayUpdate();
        }

        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            potentialDrag = (handleDraggable && eventData.pointerEnter == handle) || (menuDraggable && eventData.pointerEnter == gameObject);
        }
        public void OnBeginDrag(PointerEventData eventData)
        {
            dragging = potentialDrag;
            startPosition = previousPosition = eventData.position;
        }
        public void OnEndDrag(PointerEventData eventData)
        {
            dragging = false;
            releaseVelocity = dragVelocity;
            OnTargetUpdate();
        }
        public void OnDrag(PointerEventData eventData)
        {
            if (dragging)
            {
                CanvasScaler canvasScaler = FindObjectOfType<Canvas>().GetComponent<CanvasScaler>();
                Vector2 referenceResolution;
                Vector2 displacement;

                if (canvasScaler != null)
                {
                    referenceResolution = canvasScaler.referenceResolution;
                    displacement = ((targetState == State.Closed) ? closedPosition : openPosition) + (eventData.position - startPosition) * new Vector2(referenceResolution.x / Screen.width, referenceResolution.y / Screen.height);
                }
                else
                {
                    displacement = ((targetState == State.Closed) ? closedPosition : openPosition) + (eventData.position - startPosition);
                }


                float x = (placement == Placement.Left || placement == Placement.Right) ? displacement.x : rectTransform.anchoredPosition.x;
                float y = (placement == Placement.Top || placement == Placement.Bottom) ? displacement.y : rectTransform.anchoredPosition.y;

                Vector2 min = new Vector2(Math.Min(closedPosition.x, openPosition.x), Math.Min(closedPosition.y, openPosition.y));
                Vector2 max = new Vector2(Math.Max(closedPosition.x, openPosition.x), Math.Max(closedPosition.y, openPosition.y));

                rectTransform.anchoredPosition = new Vector2(Mathf.Clamp(x, min.x, max.x), Mathf.Clamp(y, min.y, max.y));
            }
        }

        private bool Validate()
        {
            bool valid = true;
            rectTransform = GetComponent<RectTransform>();

            if (transitionSpeed <= 0)
            {
                Debug.LogError("<b>[SimpleSideMenu]</b> Transition speed cannot be less than or equal to zero.", gameObject);
                valid = false;
            }
            if (handle != null && handleDraggable && handle.transform.parent != rectTransform)
            {
                Debug.LogError("<b>[SimpleSideMenu]</b> The drag handle must be a child of the side menu in order for it to be draggable.", gameObject);
                valid = false;
            }
            if (handleToggleStateOnPressed && handle.GetComponent<Button>() == null)
            {
                Debug.LogError("<b>[SimpleSideMenu]</b> The handle must have a \"Button\" component attached to it in order for it to be able to toggle the state of the side menu when pressed.", gameObject);
                valid = false;
            }
            return valid;
        }
        private void Setup()
        {
            //Placement
            Vector2 anchorMin = Vector2.zero;
            Vector2 anchorMax = Vector2.zero;
            Vector2 pivot = Vector2.zero;

            switch (placement)
            {
                case Placement.Left:
                    anchorMin = new Vector2(0, 0.5f);
                    anchorMax = new Vector2(0, 0.5f);
                    pivot = new Vector2(1, 0.5f);
                    closedPosition = new Vector2(0, rectTransform.localPosition.y);
                    openPosition = new Vector2(rectTransform.rect.width, rectTransform.localPosition.y);
                    break;
                case Placement.Right:
                    anchorMin = new Vector2(1, 0.5f);
                    anchorMax = new Vector2(1, 0.5f);
                    pivot = new Vector2(0, 0.5f);
                    closedPosition = new Vector2(0, rectTransform.localPosition.y);
                    openPosition = new Vector2(-1 * rectTransform.rect.width, rectTransform.localPosition.y);
                    break;
                case Placement.Top:
                    anchorMin = new Vector2(0.5f, 1);
                    anchorMax = new Vector2(0.5f, 1);
                    pivot = new Vector2(0.5f, 0);
                    closedPosition = new Vector2(rectTransform.localPosition.x, 0);
                    openPosition = new Vector2(rectTransform.localPosition.x, -1 * rectTransform.rect.height);
                    break;
                case Placement.Bottom:
                    anchorMin = new Vector2(0.5f, 0);
                    anchorMax = new Vector2(0.5f, 0);
                    pivot = new Vector2(0.5f, 1);
                    closedPosition = new Vector2(rectTransform.localPosition.x, 0);
                    openPosition = new Vector2(rectTransform.localPosition.x, rectTransform.rect.height);
                    break;
            }

            rectTransform.sizeDelta = rectTransform.rect.size;
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.pivot = pivot;

            //Default State
            currentState = targetState = defaultState;
            rectTransform.anchoredPosition = (defaultState == State.Closed) ? closedPosition : openPosition;

            //Drag Handle
            if (handle != null)
            {
                //Toggle State on Pressed
                if (handleToggleStateOnPressed)
                {
                    handle.GetComponent<Button>().onClick.AddListener(delegate { ToggleState(); });
                }
                foreach (Text text in handle.GetComponentsInChildren<Text>())
                {
                    if (text.gameObject != handle) text.raycastTarget = false;
                }
            }

            //Overlay
            if (useOverlay)
            {
                overlay = Instantiate(new GameObject(), transform.parent);
                overlay.name = gameObject.name + " (Overlay)";
                overlay.transform.SetSiblingIndex(transform.GetSiblingIndex());

                RectTransform overlayRectTransform = overlay.AddComponent<RectTransform>();
                overlayRectTransform.anchorMin = Vector2.zero;
                overlayRectTransform.anchorMax = Vector2.one;
                overlayRectTransform.offsetMin = Vector2.zero;
                overlayRectTransform.offsetMax = Vector2.zero;
                Image overlayImage = overlay.AddComponent<Image>();
                overlayImage.color = (defaultState == State.Open) ? overlayColour : Color.clear;
                overlayImage.raycastTarget = overlayCloseOnPressed;
                Button overlayButton = overlay.AddComponent<Button>();
                overlayButton.transition = Selectable.Transition.None;
                overlayButton.onClick.AddListener(delegate { Close(); });
            }
        }

        private void OnTargetUpdate()
        {
            if (releaseVelocity.magnitude > thresholdDragSpeed)
            {
                if (placement == Placement.Left)
                {
                    if (releaseVelocity.x > 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else if (placement == Placement.Right)
                {
                    if (releaseVelocity.x < 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else if (placement == Placement.Top)
                {
                    if (releaseVelocity.y < 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    if (releaseVelocity.y > 0)
                    {
                        Open();
                    }
                    else
                    {
                        Close();
                    }
                }
            }
            else
            {
                float nextStateProgress = (targetState == State.Open) ? 1 - StateProgress : StateProgress;

                if (nextStateProgress > thresholdDraggedFraction)
                {
                    ToggleState();
                }
            }   
        }
        private void OnStateUpdate()
        {
            if (dragging)
            {
                Vector2 mousePosition = Input.mousePosition;
                dragVelocity = (mousePosition - previousPosition) / (Time.time - previousTime);
                previousPosition = mousePosition;
                previousTime = Time.time;
            }
            else
            {
                Vector2 targetPosition = (targetState == State.Closed) ? closedPosition : openPosition;

                rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Time.deltaTime * transitionSpeed);
                if ((rectTransform.anchoredPosition - targetPosition).magnitude <= thresholdStateChangeDistance)
                {
                    currentState = targetState;
                }
            }
        }
        private void OnOverlayUpdate()
        {
            if (useOverlay)
            {
                overlay.GetComponent<Image>().raycastTarget = overlayCloseOnPressed && (targetState == State.Open);
                overlay.GetComponent<Image>().color = new Color(overlayColour.r, overlayColour.g, overlayColour.b, overlayColour.a * StateProgress);
            }
        }

        public void ToggleState()
        {
            if (targetState == State.Closed) Open();
            else if (targetState == State.Open) Close();
        }
        public void Open()
        {
            targetState = State.Open;
        }
        public void Close()
        {
            targetState = State.Closed;
        }     
        #endregion
    }
}