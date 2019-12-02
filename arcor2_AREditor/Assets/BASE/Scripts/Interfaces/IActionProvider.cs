using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionProvider
{
    string GetProviderName();

    Base.ActionMetadata GetActionMetadata(string action_id);
    
}
