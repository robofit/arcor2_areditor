using Unity;
using Base;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public abstract class InteractiveObject : Clickable {

    public abstract string GetName();
    public abstract string GetId();

    public abstract string GetObjectTypeName();
    public abstract void OpenMenu();
    public abstract bool HasMenu();
    public abstract Task<RequestResult> Movable();
    public abstract void StartManipulation();

    public abstract Task<RequestResult> Removable();

    public abstract void Remove();
    public virtual float GetDistance(Vector3 origin) {
        float minDist = float.MaxValue;
        foreach (Collider collider in Colliders) {
            Vector3 point = collider.ClosestPointOnBounds(origin);
           
            minDist = Math.Min(Vector3.Distance(origin, point), minDist);

        }
        return minDist;
    }

    public List<Collider> Colliders = new List<Collider>();

    public abstract Task Rename(string name);


    /// <summary>
    /// Locks object. If successful - returns true, if not - shows notification and returns false.
    /// </summary>
    /// <param name="lockTree">Lock also all children? (including children of children etc.)</param>
    /// <returns></returns>
    public async Task<bool> LockAsync(bool lockTree) {
        try {
            await WebsocketManager.Instance.WriteLock(GetId(), lockTree);
            return true;
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to lock " + GetName(), ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Unlocks object. If successful - returns true, if not - shows notification and returns false.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> UnlockAsync() {
        try {
            await WebsocketManager.Instance.WriteUnlock(GetId());
            return true;
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to unlock " + GetName(), ex.Message);
            return false;
        }
    }

    protected virtual void Start() {
        WebsocketManager.Instance.OnObjectLockingEvent += OnObjectLockingEvent;
    }

    private void OnObjectLockingEvent(object sender, ObjectLockingEventArgs args) {
        if (args.ObjectId != GetId())
            return;
        Enable(args.Locked);
    }
}
