using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScaler : MonoBehaviour {

    private RectTransform rectTransform;

    public Placement MenuPlacement = Placement.Left;

    public enum Placement {
        Left,
        Right
    }

    void Start() {
        rectTransform = GetComponent<RectTransform>();

        if (MenuPlacement == Placement.Left) {
            rectTransform.anchorMin = new Vector2(0f, 0f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0f);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0f);
        } else if (MenuPlacement == Placement.Right) {
            rectTransform.anchorMin = new Vector2(1f, 0f);
            rectTransform.anchorMax = new Vector2(1f, 1f);
            rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0f);
            rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0f);
        }
    }

}
