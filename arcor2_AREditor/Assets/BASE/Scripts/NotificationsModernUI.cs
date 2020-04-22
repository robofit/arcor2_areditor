using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using System.IO;
using System;

namespace Base {
    public class NotificationsModernUI : Notifications {

        public List<LogEntry> LogEntries = new List<LogEntry>();

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
            LogEntries.Add(new LogEntry("Notification", title, text));
        }

        private void OnEnable() {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable() {
            Application.logMessageReceived -= HandleLog;
        }

        private void HandleLog(string logString, string stackTrace, LogType type) {
            LogEntries.Add(new LogEntry(type.ToString(), logString, stackTrace));
        }

        public override void SaveLogs(string scene, string project) {
            string dirname = Application.persistentDataPath + "/Logs/" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            Directory.CreateDirectory(dirname);
            StreamWriter sceneFile = File.CreateText(dirname + "/scene.json");
            sceneFile.Write(scene);
            sceneFile.Close();

            StreamWriter projectFile = File.CreateText(dirname + "/project.json");
            projectFile.Write(project);
            projectFile.Close();

            StreamWriter logsFile = File.CreateText(dirname + "/logs.txt");
            foreach (LogEntry log in LogEntries) {
                logsFile.WriteLine("Timestamp: " + log.TimeStamp.ToString());
                logsFile.WriteLine("Type: " + log.LogType.ToString());
                logsFile.WriteLine("Message: " + log.LogMessage);
                if (log.LogType != LogType.Log.ToString())
                    logsFile.WriteLine("Stacktrace: " + log.StackTrace);
                logsFile.WriteLine("");
            }
            logsFile.Close();

            ShowNotification("Logs saved to directory", dirname);

        }
    }

}
