using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IRobot
{
    string GetName();

    string GetId();

    Task<List<string>> GetEndEffectorIds();

    Task<List<string>> GetArmsIds();

    Task<RobotEE> GetEE(string ee_id);

    Task<List<RobotEE>> GetAllEE();

    bool HasUrdf();

    void SetJointValue(List<IO.Swagger.Model.Joint> joints, bool angle_in_degrees = false, bool forceJointsValidCheck = false);

    void SetJointValue(string name, float angle, bool angle_in_degrees = false);

    List<IO.Swagger.Model.Joint> GetJoints();

    void SetGrey(bool grey, bool force = false);

    Transform GetTransform();

}
