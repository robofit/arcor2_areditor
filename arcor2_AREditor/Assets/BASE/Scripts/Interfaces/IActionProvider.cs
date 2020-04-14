using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionProvider
{
    string GetProviderName();

    string GetProviderId();

    string GetProviderType();

    Base.ActionMetadata GetActionMetadata(string action_id);


    bool IsRobot();



}
