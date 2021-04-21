using Unity;
using Base;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public abstract class InteractiveObject : Clickable {

    public bool IsLocked { get; protected set; }
    public string LockOwner { get; protected set; }

    protected string GetLockedText() {
        return "LOCKED by " + LockOwner + "\n" + GetName();
    }

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

    public virtual void Enable(bool enable) {
        Enabled = enable;
        foreach (Collider collider in Colliders) {
            collider.enabled = enable;
        }
    }

    public List<Collider> Colliders = new List<Collider>();

    public abstract void UpdateColor();

    public override void Enable(bool enable) {
        base.Enable(enable);
        UpdateColor();
    }


    public abstract Task Rename(string name);


    /// <summary>
    /// Locks object. If successful - returns true, if not - shows notification and returns false.
    /// </summary>
    /// <param name="lockTree">Lock also tree? (all levels of parents and children)</param>
    /// <returns></returns>
    public async Task<bool> LockAsync(bool lockTree) {
        if (IsLocked && LandingScreen.Instance.GetUsername() == LockOwner) //object is already locked by this user
            return true;

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

    protected virtual void OnObjectLockingEvent(object sender, ObjectLockingEventArgs args) {
        if (args.ObjectId != GetId())
            return;

        if (args.Locked) {
            OnObjectLocked(args.Owner);
        } else {
            OnObjectUnlocked();
        }
        SelectorMenu.Instance.ForceUpdateMenus();
    }

    public virtual void OnObjectUnlocked() {
        IsLocked = false;
        UpdateColor();
    }

    public virtual void OnObjectLocked(string owner) {
        IsLocked = true;
        LockOwner = owner;
        if(owner != LandingScreen.Instance.GetUsername())
            UpdateColor();
    }


}
