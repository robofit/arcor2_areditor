using System;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;

public abstract class RightMenu<T> : Singleton<T> where T : MonoBehaviour {
    //public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    protected List<InteractiveObject> lockedObjects = new List<InteractiveObject>();

    public CanvasGroup CanvasGroup;

    public bool IsVisible => CanvasGroup.alpha > 0;

    public async Task<RequestResult> UnlockAllObjects() {
        for (int i = lockedObjects.Count - 1; i >= 0; --i) {
            if (lockedObjects[i].IsLockedByMe) {
                if (!await lockedObjects[i].WriteUnlock()) {
                    return new RequestResult(false, $"Failed to unlock {lockedObjects[i].GetName()}");
                }
                if (lockedObjects[i] is CollisionObject co) {
                    await co.WriteUnlockObjectType();
                }
                lockedObjects.RemoveAt(i);
            }
        }
        return new RequestResult(true);
    }

    public async Task<bool> LockObject(InteractiveObject interactiveObject, bool lockTree) {
        if (await interactiveObject.WriteLock(lockTree)) {
            lockedObjects.Add(interactiveObject);
            return true;
        }
        return false;
    }

    public virtual async Task<bool> Show(InteractiveObject obj, bool lockTree) {
        if (await LockObject(obj, lockTree)) {
            lockedObjects.Add(obj);
            return true;
        } else {
            return false;
        }
    }

    public virtual async Task Hide() {
        if (lockedObjects.Count > 0) {
            await UnlockAllObjects();
        }
    }

    
    
}
