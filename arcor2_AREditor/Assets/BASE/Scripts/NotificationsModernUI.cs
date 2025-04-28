using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Michsky.UI.ModernUIPack;
using UnityEngine;

namespace Base {
    public class NotificationsModernUI : Notifications {

        public List<LogEntry> LogEntries = new();

        public NotificationManager NotificationManager;

        [SerializeField]
        private Canvas Canvas;

        public GameObject NotificationEntryPrefab, NotificationMenuContent;

        static readonly HttpClient client = new();

        public void Start() {
            //Notification = NotificationManager.gameObject.GetComponent<UIManagerNotification>();
        }
        public override void ShowNotification(string title, string text) {
            // HACK to make notifiaction in foreground
            // TODO - find better way
            Canvas.enabled = false;
            Canvas.enabled = true;
            NotificationManager.title = title;
            NotificationManager.description = text;
            NotificationManager.UpdateUI();
            NotificationManager.OpenNotification();
            LogEntries.Add(new LogEntry("Notification", title, text));
            NotificationEntry notificationEntry = Instantiate(NotificationEntryPrefab, NotificationMenuContent.transform).GetComponent<NotificationEntry>();
            notificationEntry.transform.SetAsFirstSibling();
            notificationEntry.Title.text = title;
            notificationEntry.Description.text = text;
            notificationEntry.Timestamp.text = DateTime.Now.ToString();
        }

        private void OnEnable() {
            Application.logMessageReceived += HandleLog;
        }

        private void OnDisable() {
            Application.logMessageReceived -= HandleLog;
        }


        private void HandleLog(string logString, string stackTrace, LogType type) {
            LogEntries.Add(new LogEntry(type.ToString(), logString, stackTrace));
            if (type == LogType.Exception) {
                //automatially create logs in case of exception
                SaveLogs(SceneManager.Instance.GetScene(), ProjectManager.Instance.GetProject(), "Exception occured");
            }
        }

        public async override void SaveLogs(Scene scene, Project project, string customNotificationTitle = "") {
            string sceneString = "", projectString = "";
            if (SceneManager.Instance.SceneMeta != null)
                sceneString = scene.ToJson();
            if (ProjectManager.Instance.ProjectMeta != null)
                projectString = project.ToJson();
            string dirname = Application.persistentDataPath + "/Logs/" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss");
            string zipname = Application.persistentDataPath + "/Logs/logs.zip";
            Directory.CreateDirectory(dirname);
            StreamWriter sceneFile = File.CreateText(dirname + "/scene.json");
            sceneFile.Write(sceneString);
            sceneFile.Close();

            StreamWriter projectFile = File.CreateText(dirname + "/project.json");
            projectFile.Write(projectString);
            projectFile.Close();

            StreamWriter logsFile = File.CreateText(dirname + "/logs.txt");
            logsFile.WriteLine("Editor version: " + Application.version);
            if (GameManager.Instance.SystemInfo != null) {
                logsFile.WriteLine("Server version: " + GameManager.Instance.SystemInfo.VarVersion);
            }

            logsFile.WriteLine("Editor API version: " + GameManager.ApiVersion);
            if (GameManager.Instance.SystemInfo != null) {
                logsFile.WriteLine("Server API version: " + GameManager.Instance.SystemInfo.ApiVersion);
            } else {
                logsFile.WriteLine("Not connected to server");
            }
            logsFile.WriteLine();
            foreach (LogEntry log in LogEntries) {
                logsFile.WriteLine("Timestamp: " + log.TimeStamp.ToString());
                logsFile.WriteLine("Type: " + log.LogType.ToString());
                logsFile.WriteLine("Message: " + log.LogMessage);
                if (log.LogType != LogType.Log.ToString())
                    logsFile.WriteLine("Stacktrace: " + log.StackTrace);
                logsFile.WriteLine("");
            }
            logsFile.Close();
            ShowNotification(customNotificationTitle, "Logs saved to directory " + dirname);

            // TODO why we upload logs only when the editor is connected to the server?
            string serverDomain = CommunicationManager.Instance.Client.Uri!.GetComponents(UriComponents.Host, UriFormat.Unescaped);

            if (string.IsNullOrEmpty(serverDomain))
                return;

            string uri = "http://" + serverDomain + ":6799/upload";
            try {
                if (File.Exists(zipname)) {
                    File.Delete(zipname);
                }
                ZipFile.CreateFromDirectory(dirname, zipname);
                FileStream file = new(zipname, FileMode.Open, FileAccess.Read);
                HttpContent fileStreamContent = new StreamContent(file);
                using (MultipartFormDataContent formData = new()) {
                    formData.Add(fileStreamContent, "files", Path.GetFileName(zipname));
                    HttpResponseMessage response = await client.PostAsync(uri, formData);
                    if (!response.IsSuccessStatusCode) {
                        Debug.LogError("Error:" + zipname + " not uploaded");
                    } else {
                    }

                }

            } catch (HttpRequestException ex) {
                Debug.LogError($"Failed to upload logs to {uri}: " + ex.Message);
            } catch (InvalidOperationException ex) {
                Debug.LogError($"Failed to upload logs to {uri}: " + ex.Message);
            } catch (Exception ex) when (ex is ArgumentException ||
                                          ex is PathTooLongException ||
                                          ex is DirectoryNotFoundException ||
                                          ex is IOException) {
                Debug.LogError("Failed to create zip folder with logs: " + ex.Message);
            } finally {

            }
        }
    }

}
