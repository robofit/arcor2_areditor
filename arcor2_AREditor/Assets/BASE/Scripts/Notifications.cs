
using Arcor2.ClientSdk.Communication.OpenApi.Models;

namespace Base {
    public abstract class Notifications : Singleton<Notifications> {
        public abstract void SaveLogs(Scene scene, Project project, string customNotificationTitle = "");

        public virtual void SaveLogs(string customNotificationTitle = "") {
            SaveLogs(SceneManager.Instance.GetScene(), ProjectManager.Instance.GetProject(), customNotificationTitle);
        }
        public abstract void ShowNotification(string title, string text);

        public virtual void ShowToastMessage(string message, int timeout = 3) {
            ToastMessage.Instance.ShowMessage(message, timeout);
        }
    }

}
