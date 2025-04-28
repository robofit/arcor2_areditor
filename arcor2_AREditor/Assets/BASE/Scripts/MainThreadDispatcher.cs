using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour {
    private static MainThreadDispatcher instance;
    private static readonly Queue<Action> executionQueue = new();
    private static bool isInitialized = false;
    private static Thread mainThread;

    public static MainThreadDispatcher Instance {
        get {
            if (instance == null) {
                instance = FindObjectOfType<MainThreadDispatcher>();

                if (instance == null) {
                    GameObject dispatcherObject = new("MainThreadDispatcher");
                    instance = dispatcherObject.AddComponent<MainThreadDispatcher>();
                    DontDestroyOnLoad(dispatcherObject);
                }
            }

            return instance;
        }
    }

    private void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else if (instance != this) {
            Destroy(gameObject);
            return;
        }

        mainThread = Thread.CurrentThread;
        isInitialized = true;
    }

    /// <summary>
    /// Checks if the current thread is the main thread.
    /// </summary>
    public static bool IsMainThread() {
        return Thread.CurrentThread == mainThread;
    }

    /// <summary>
    /// Enqueues an action to be executed on the main thread.
    /// </summary>
    public static void Enqueue(Action action) {
        if (action == null)
            return;

        // If we're already on the main thread, just execute directly
        if (IsMainThread()) {
            action();
            return;
        }

        lock (executionQueue) {
            executionQueue.Enqueue(action);
        }
    }

    void Update() {
        // Execute all queued actions
        while (true) {
            Action action = null;
            lock (executionQueue) {
                if (executionQueue.Count > 0) {
                    action = executionQueue.Dequeue();
                } else {
                    break;
                }
            }
            action?.Invoke();
        }
    }
}
