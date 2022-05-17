using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnboardingHandler : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] public GameObject OnboardingOverlay;


    public void Fade ()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        StartCoroutine(HandleFading());
    }

    public void ShowPanel() {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        canvasGroup.blocksRaycasts = true;
    }

    public void HidePanel() {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }
    public IEnumerator HandleFading()
    {
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup.alpha > 0.5) {
            for (float i = 1; i >= 0f; i -= 0.1f) {
                if (i < 0.1f)
                    i = 0;

                canvasGroup.alpha = i;
                yield return new WaitForSeconds(0.001f);
            }
        } else {
            for (float i = 0; i <= 1; i += 0.1f) {
                canvasGroup.alpha = i;
                yield return new WaitForSeconds(0.001f);
            }
        }

        if (canvasGroup.alpha == 0) {
            canvasGroup.blocksRaycasts = false;
        } else {
            canvasGroup.blocksRaycasts = true;
        }
    }
}
