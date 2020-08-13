using System;
using System.Collections.Generic;

public interface IRobot
{
    string GetName();

    string GetId();

    List<string> GetEndEffectorIds();

    RobotEE GetEE(string ee_id);

    bool HasUrdf();

    void SetJointValue(string name, float angle);
}
