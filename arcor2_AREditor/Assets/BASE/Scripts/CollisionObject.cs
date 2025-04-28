using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using Base;

public class CollisionObject : ActionObject3D {
    public override string GetObjectTypeName() {
        return "Collision object";
    }

    public async Task<bool> WriteLockObjectType() {
        try {
            return (await CommunicationManager.Instance.Client.WriteLockAsync(new WriteLockRequestArgs(ActionObjectMetadata.Type, false))).Result;
        } catch (Arcor2ConnectionException) {
            return false;
        }
    }

    public async Task<bool> WriteUnlockObjectType() {
        try {
            return (await CommunicationManager.Instance.Client.WriteUnlockAsync(new WriteUnlockRequestArgs(ActionObjectMetadata.Type))).Result;
        } catch (Arcor2ConnectionException) {
            return false;
        }
    }

}
