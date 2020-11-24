using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class OnToggleClickButton : MonoBehaviour, IPointerClickHandler {

    public UnityEvent OnClick;

    public void OnPointerClick(PointerEventData eventData) {
        OnClick?.Invoke();
    }

}
