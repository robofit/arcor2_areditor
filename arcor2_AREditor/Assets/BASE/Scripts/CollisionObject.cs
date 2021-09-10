using System;
using System.Threading.Tasks;
using Base;
using UnityEngine;
public class CollisionObject : ActionObject3D
{
    public override string GetObjectTypeName() {
        return "Collision object";
    }

    public async override Task<bool> WriteLock(bool lockTree) {
        bool lockObject;
        lockObject = await base.WriteLock(lockTree);
        if (!lockObject)
            return false;
        try {
            await WebsocketManager.Instance.WriteLock(ActionObjectMetadata.Type, lockTree);
            return true;
        } catch (RequestFailedException) {
            await WriteUnlock();
            return false;
        }
        
    }

    public async override Task<bool> WriteUnlock() {
        bool unlockObject;
        unlockObject = await base.WriteUnlock();
        if (!unlockObject)
            return false;
        try {
            await WebsocketManager.Instance.WriteUnlock(ActionObjectMetadata.Type);
            return true;
        } catch (RequestFailedException) {
            return false;
        }

    }
}
