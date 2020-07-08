using System;
using System.Collections.Generic;

public interface IRobot
{
    string GetName();

    string GetId();

    List<string> GetEndEffectors();

    bool HasUrdf();
}
