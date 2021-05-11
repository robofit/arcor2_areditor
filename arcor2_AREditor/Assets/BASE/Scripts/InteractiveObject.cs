using Unity;
using Base;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Collections;

public abstract class InteractiveObject : Clickable {

    public bool IsLocked { get; protected set; }
    public string LockOwner { get; protected set; }

    private bool shouldUnlock = true; //used for delayed unlocking

    protected bool lockedTree = false; //when object is locked, is also locked the whole tree?

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
        UpdateColor();

        foreach (Collider collider in Colliders) {
            collider.enabled = enable;
        }
    }
    
    public List<Collider> Colliders = new List<Collider>();

    public abstract void UpdateColor();

    public abstract Task Rename(string name);

    /// <summary>
    /// Locks object. If successful - returns true, if not - shows notification and returns false.
    /// </summary>
    /// <param name="lockTree">Lock also tree? (all levels of parents and children)</param>
    /// <returns></returns>
    public virtual async Task<bool> WriteLock(bool lockTree) {
        if (IsLocked && LandingScreen.Instance.GetUsername() == LockOwner) { //object is already locked by this user
            if (lockedTree != lockTree) {
                try {
                    shouldUnlock = false;
                    await UpdateLock(lockTree ? IO.Swagger.Model.UpdateLockRequestArgs.NewTypeEnum.TREE : IO.Swagger.Model.UpdateLockRequestArgs.NewTypeEnum.OBJECT);
                    return true;
                } catch (RequestFailedException e) {
                    //try lock as usual
                }
            } else { //same type of lock
                return true;
            }
        }

        try {
            await WebsocketManager.Instance.WriteLock(GetId(), lockTree);
            lockedTree = lockTree;
            return true;
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to lock " + GetName(), ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Unlocks object. 
    /// If successful - returns true, if not - returns false.
    /// </summary>
    /// <param name="delay">if delay, object will be unlocked after 1s unless locked again</param>
    /// <returns></returns>
    public virtual async Task<bool> WriteUnlock(bool delay = true) {
        if (delay) {
            shouldUnlock = true;
            StartCoroutine(DelayedUnlock());
            return true;
        }

        try {
            await WebsocketManager.Instance.WriteUnlock(GetId());
            IsLocked = false;
            return true;
        } catch (RequestFailedException ex) {
            //Notifications.Instance.ShowNotification("Failed to unlock " + GetName(), ex.Message);
            Debug.LogError(ex.Message);
            return false;
        }
    }

    public virtual async Task<bool> UpdateLock(IO.Swagger.Model.UpdateLockRequestArgs.NewTypeEnum newType) {
        try {
            await WebsocketManager.Instance.UpdateLock(GetId(), newType);
            return true;
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to lock " + GetName(), ex.Message);
            return false;
        }
    }

    protected virtual void Start() {
        LockingEventsCache.Instance.OnObjectLockingEvent += OnObjectLockingEvent;
    }

    protected virtual void OnObjectLockingEvent(object sender, ObjectLockingEventArgs args) {
        if (!args.ObjectIds.Contains(GetId()))
            return;

        //Debug.LogError("locking event " + GetName());

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

    private IEnumerator DelayedUnlock(float time = 0.1f) {
        yield return new WaitForSeconds(time);
        if (shouldUnlock)
            WriteUnlock(false);
    }

}
