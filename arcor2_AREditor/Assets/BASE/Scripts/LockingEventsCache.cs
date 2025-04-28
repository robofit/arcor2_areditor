using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;

namespace Base {
    public class LockingEventsCache : Singleton<LockingEventsCache> {
        /// <summary>
        /// Invoked when an object is locked or unlocked
        /// </summary>
        public event EventHandler<ObjectLockingEventArgs> OnObjectLockingEvent;
        public event EventHandler<StringEventArgs> OnAfterObjectLockingEvent;
        private bool canInvoke = false;
        private bool wasAppKilled = true;
        private bool arOn = false;
        private bool tracking = false;
        private bool sceneLoaded = false;

        private List<ObjectLockingEventArgs> events = new();

        private void Start() {
            SceneManager.Instance.OnLoadScene += OnProjectOrSceneLoaded;
            ProjectManager.Instance.OnLoadProject += OnProjectOrSceneLoaded;
            CommunicationManager.Instance.Client.ConnectionClosed += CommunicationManager.SafeEventHandler<WebSocketCloseEventArgs>(OnAppDisconnected);
#if UNITY_ANDROID && AR_ON
            CalibrationManager.Instance.OnARCalibrated += OnARCalibrated;
            arOn = true;
#endif
        }


        private void OnARCalibrated(object sender, CalibrationEventArgs args) {
           // Debug.LogError("ON AR CALIBRATED");
            tracking = true;
            if (sceneLoaded)
                StartCoroutine(Wait(2));
        }

        private void OnAppDisconnected(object sender, EventArgs e) {
            canInvoke = false; //for situations when app is reconnected
            sceneLoaded = false;
            tracking = false;
           // Debug.LogError("VYNULOVANO");
        }

        private IEnumerator Wait(int time = 4) {
            yield return new WaitForSeconds(time);
            canInvoke = true;
            InvokeEvents();
        }

        private void OnProjectOrSceneLoaded(object sender, EventArgs e) {
            //Debug.LogError("PROJ LOADED");
            sceneLoaded = true;
            if (!arOn || !wasAppKilled)
                StartCoroutine(Wait());
            else if(tracking){
                StartCoroutine(Wait());
            }
        }

        public void Add(ObjectLockingEventArgs objectsLockingEvent) {
            if (IsSceneOrProject(objectsLockingEvent.ObjectIds.FirstOrDefault())) { //in the list, there should be always only objects or only projects/scenes, so taking the first id should work
                OnObjectLockingEvent?.Invoke(this, objectsLockingEvent);
            } else {
                lock (events) {
                    events.Add(objectsLockingEvent);
                }
            }
            InvokeEvents();
            OnAfterObjectLockingEvent?.Invoke(this, new StringEventArgs(""));
        }



        private void InvokeEvents() {
            if (!canInvoke) {
                if (SceneManager.Instance.Valid || ProjectManager.Instance.Valid) { //if there is no locked object when app started, class misses onprojectloaded event
                    OnProjectOrSceneLoaded(null, null);
                    return;
                }
                if (wasAppKilled) { //check for my locks, which are needed to be unlocked, because UI was reset
                    //Debug.LogError("!caninvoke + app was killer");
                    lock (events) {
                        if (!wasAppKilled) //check again
                            return;
                        List<ObjectLockingEventArgs> toRemove = new();
                        foreach (ObjectLockingEventArgs ev in events) {
                            if (ev.Locked && ev.Owner == LandingScreen.Instance.GetUsername()) {
                                foreach(var id in ev.ObjectIds)
                                    CommunicationManager.Instance.Client.WriteUnlockAsync(new WriteUnlockRequestArgs(id)); //if the app was killed, unlock my locks, because UI doesnt know what menu should be opened
                                toRemove.Add(ev);
                            }
                        }
                        events.RemoveAll(ev => toRemove.Contains(ev));
                    }
                }
                return;
            }
            wasAppKilled = false;

            lock (events) {
                foreach (ObjectLockingEventArgs ev in events) {
                    //Debug.LogError("invokuju" + ev.ObjectId);
                    OnObjectLockingEvent?.Invoke(this, ev);
                }
                events.Clear();
            }
        }

        /// <summary>
        /// if the ID belongs to a project or scene, returns true
        /// </summary>
        /// <param name="objectId"></param>
        /// <returns></returns>
        private bool IsSceneOrProject(string objectId) {
            return GameManager.Instance.Scenes.Any(s => s.Id == objectId) ||
                GameManager.Instance.Projects.Any(p => p.Id == objectId);
        }
    }
}
