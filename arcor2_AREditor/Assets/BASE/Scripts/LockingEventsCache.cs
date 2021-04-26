using System;
using System.Collections.Generic;
using UnityEngine;
using IO.Swagger.Model;
using System.Collections;
using System.Linq;

namespace Base {
    public class LockingEventsCache : Singleton<LockingEventsCache> {
        /// <summary>
        /// Invoked when an object is locked or unlocked
        /// </summary>
        public event AREditorEventArgs.ObjectLockingEventHandler OnObjectLockingEvent;
        private bool canInvoke = false;
        private bool wasAppKilled = true;
        private bool arOn = false;
        private bool tracking = false;
        private bool sceneLoaded = false;

        private List<ObjectLockingEventArgs> events = new List<ObjectLockingEventArgs>();

        private void Start() {
            SceneManager.Instance.OnLoadScene += OnProjectOrSceneLoaded;
            ProjectManager.Instance.OnLoadProject += OnProjectOrSceneLoaded;
            WebsocketManager.Instance.OnDisconnectEvent += OnAppDisconnected;
            CalibrationManager.Instance.OnARCalibrated += OnARCalibrated;
            //CalibrationManager.Instance.OnARRecalibrate += OnARRecalibrate;

#if UNITY_ANDROID && AR_ON
            arOn = true;
#endif
        }

        //private void OnARRecalibrate(object sender, EventArgs args) {
        //    Debug.LogError("ON AR RECALIBRATED");
        //    tracking = true;
        //    if (sceneLoaded)
        //        StartCoroutine(Wait());
        //}

        private void OnARCalibrated(object sender, GameObjectEventArgs args) {
            Debug.LogError("ON AR CALIBRATED");
            tracking = true;
            if (sceneLoaded)
                StartCoroutine(Wait(2));
        }

        private void OnAppDisconnected(object sender, EventArgs e) {
            canInvoke = false; //for situations when app is reconnected
            sceneLoaded = false;
            tracking = false;
            Debug.LogError("VYNULOVANO");
        }

        private IEnumerator Wait(int time = 5) {
            yield return new WaitForSeconds(time);
            canInvoke = true;
            InvokeEvents();
        }

        private void OnProjectOrSceneLoaded(object sender, EventArgs e) {
            Debug.LogError("PROJ LOADED");
            sceneLoaded = true;
            if (!arOn || !wasAppKilled)
                StartCoroutine(Wait());
            else if(tracking){
                StartCoroutine(Wait());
            }
        }

        public void Add(ObjectLockingEventArgs objectsLockingEvent) {
            lock (events) {
                if (IsSceneOrProject(objectsLockingEvent.ObjectId))
                    OnObjectLockingEvent?.Invoke(this, objectsLockingEvent);
                else 
                    events.Add(objectsLockingEvent);
            }
            InvokeEvents();
        }



        private void InvokeEvents() {
            if (!canInvoke) {
                if (wasAppKilled) { //check for my locks, which are needed to be unlocked, because UI was reset
                    Debug.LogError("!caninvoke + app was killer");
                    lock (events) {
                        if (!wasAppKilled) //check again
                            return;
                        List<ObjectLockingEventArgs> toRemove = new List<ObjectLockingEventArgs>();
                        foreach (ObjectLockingEventArgs ev in events) {
                            if (ev.Locked && ev.Owner == LandingScreen.Instance.GetUsername()) {
                                WebsocketManager.Instance.WriteUnlock(ev.ObjectId);
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
                    Debug.LogError("invokuju" + ev.ObjectId);
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
