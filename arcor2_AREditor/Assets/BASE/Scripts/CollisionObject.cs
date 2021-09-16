using System;
using System.Threading.Tasks;
using Base;
using UnityEngine;
public class CollisionObject : ActionObject3D
{
    public override string GetObjectTypeName() {
        return "Collision object";
    }

    public async Task<bool> WriteLockObjectType() {
        try {
            await WebsocketManager.Instance.WriteLock(ActionObjectMetadata.Type, false);
            return true;
        } catch (RequestFailedException) {
            return false;
        }
    }

    public async Task<bool> WriteUnlockObjectType() {
        try {
            await WebsocketManager.Instance.WriteUnlock(ActionObjectMetadata.Type);
            return true;
        } catch (RequestFailedException) {
            return false;
        }
    }

}
