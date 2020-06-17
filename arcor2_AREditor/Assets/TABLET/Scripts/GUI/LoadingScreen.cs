using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class LoadingScreen : MonoBehaviour
{
    public TMPro.TMP_Text Text;

    private bool ForceToHide = false;

    private CanvasGroup CanvasGroup;

    private void Awake() {
        CanvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show(string text, bool forceToHide = false) {
        Text.text = text;
        CanvasGroup.alpha = 1;
        CanvasGroup.blocksRaycasts = true;
        if (!ForceToHide)
            ForceToHide = forceToHide;
    }

    public void Hide(bool force = false) {
        if (ForceToHide && !force)
            return;
        CanvasGroup.alpha = 0;
        CanvasGroup.blocksRaycasts = false;
        ForceToHide = false;
    }
}
