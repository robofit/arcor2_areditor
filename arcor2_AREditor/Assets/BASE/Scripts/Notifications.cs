using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Base {
    public abstract class Notifications : Singleton<Notifications> {
        public abstract void SaveLogs(string scene, string project);
        public abstract void ShowNotification(string title, string text);
    }

}
