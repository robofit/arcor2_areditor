using System;
using UnityEngine;

public static class EditorHelper
{
    public static void EnableCanvasGroup(CanvasGroup canvasGroup, bool enable) {
        if (enable) {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        } else {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
        
    }
}
