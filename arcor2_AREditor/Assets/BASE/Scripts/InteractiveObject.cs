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

    protected bool lockedTree = false; //when object is locked, is also locked the whole tree?

    public bool IsLockedByMe => IsLocked && LockOwner == LandingScreen.Instance.GetUsername();
    public bool IsLockedByOtherUser => IsLocked && LockOwner != LandingScreen.Instance.GetUsername();

    protected virtual void Start() {
        LockingEventsCache.Instance.OnObjectLockingEvent += OnObjectLockingEvent;
    }

    protected virtual void OnDestroy() {
        SelectorMenu.Instance.DestroySelectorItem(this);
        LockingEventsCache.Instance.OnObjectLockingEvent -= OnObjectLockingEvent;
    }

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

    public SelectorItem SelectorItem;

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
        SelectorItem.gameObject.SetActive(enable);
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
        if (IsLockedByMe) { //object is already locked by this user
            if (lockedTree != lockTree) {
                if (await UpdateLock(lockTree ? IO.Swagger.Model.UpdateLockRequestArgs.NewTypeEnum.TREE : IO.Swagger.Model.UpdateLockRequestArgs.NewTypeEnum.OBJECT)) {
                    lockedTree = lockTree;
                    return true;
                } // if updateLock failed, try to lock normally
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
    /// <returns></returns>
    public virtual async Task<bool> WriteUnlock() {
        if (!IsLocked) {
            Debug.LogError("Trying to unlock unlocked object: " + GetId());
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
            //Notifications.Instance.ShowNotification("Failed to lock " + GetName(), ex.Message);
            Debug.LogError("failed to update lock");
            return false;
        }
    }

    protected virtual void OnObjectLockingEvent(object sender, ObjectLockingEventArgs args) {
        if (!args.ObjectIds.Contains(GetId()))
            return;

        if (args.Locked) {
            OnObjectLocked(args.Owner);
        } else {
            OnObjectUnlocked();
        }

        //SelectorMenu.Instance.ForceUpdateMenus();
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
