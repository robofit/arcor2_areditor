using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

namespace Base {
    public class NotificationsModernUI : Notifications {

        public NotificationManager NotificationManager;

        private UIManagerNotification Notification;
        [SerializeField]
        private Canvas Canvas;

        public void Start() {
            Notification = NotificationManager.gameObject.GetComponent<UIManagerNotification>();
        }
        public override void ShowNotification(string title, string text) {
            // HACK to make notifiaction in foreground
            // TODO - find better way
            Canvas.enabled = false;
            Canvas.enabled = true;
            Notification.title.text = title;
            Notification.description.text = text;
            NotificationManager.OpenNotification();
        }
    }

}
