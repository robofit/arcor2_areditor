using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;

namespace Base {
    public class NotificationsModernUI : Notifications {

        public GameObject NotificationGO;

        private UIManagerNotification Notification;

        public void Start() {
            Notification = NotificationGO.GetComponent<UIManagerNotification>();
        }
        public override void ShowNotification(string title, string text) {
            Notification.title.text = title;
            Notification.description.text = text;
            NotificationGO.GetComponent<NotificationManager>().OpenNotification();
        }
    }

}
