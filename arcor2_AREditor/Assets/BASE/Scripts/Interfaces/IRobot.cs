using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Joint = Arcor2.ClientSdk.Communication.OpenApi.Models.Joint;

public interface IRobot
{
    string GetName();

    string GetId();

    Task<List<string>> GetEndEffectorIds(string arm_id = null);

    Task<List<string>> GetArmsIds();

    Task<RobotEE> GetEE(string ee_id, string arm_id);

    Task<List<RobotEE>> GetAllEE();

    bool HasUrdf();

    void SetJointValue(List<Joint> joints, bool angle_in_degrees = false, bool forceJointsValidCheck = false);

    void SetJointValue(string name, float angle, bool angle_in_degrees = false);

    List<Joint> GetJoints();

    void SetGrey(bool grey, bool force = false);

    Transform GetTransform();

    bool MultiArm();

    Task<bool> WriteLock(bool lockTree);

    Task<bool> WriteUnlock();

    string LockOwner();

    InteractiveObject GetInteractiveObject();

    }
