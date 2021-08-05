using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToastMessage : Base.Singleton<ToastMessage>
{
    [SerializeField]
    private TMPro.TMP_Text text;
    [SerializeField]
    private CanvasGroup canvasGroup;

    public void ShowMessage(string message, int duration) {
#if UNITY_ANDROID && AR_ON
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null) {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                    toastObject.Call("show");
                }));
            }
#elif UNITY_STANDALONE || !AR_ON
        text.text = message;
        StopAllCoroutines();
        StartCoroutine(ShowToast());
        StartCoroutine(HideToast(duration));
#endif
    }

    private IEnumerator ShowToast() {
        for (float f = 0; f <= 0.3f; f += Time.deltaTime) {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, f / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 1;
    }

    private IEnumerator HideToast(int duration) {
        yield return new WaitForSeconds(duration);
        for (float f = 0f; f <= 0.3f; f += Time.deltaTime) {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, f / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 0;
    }
}

