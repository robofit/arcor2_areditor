using System;
using System.Collections.Generic;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;
using Action = Arcor2.ClientSdk.Communication.OpenApi.Models.Action;
using ActionMetadata = Base.ActionMetadata;
using ActionPoint = Base.ActionPoint;

public class EndAction : StartEndAction
{
    

    public override void Init(Action projectAction, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider, string keySuffix) {
        Action prAction = new(
            flows: new List<Flow>(),
            id: "END",
            name: "END",
            parameters: new List<ActionParameter>(),
            type: "");
        base.Init(prAction, metadata, ap, actionProvider, keySuffix);
        transform.localPosition = PlayerPrefsHelper.LoadVector3(playerPrefsKey, new Vector3(0, 0.1f, 0.6f));
        //Input.SelectorItem = SelectorMenu.Instance.CreateSelectorItem(Input);
    }

    

    public override void UpdateColor()
    {
        if (Enabled) {
            foreach (Renderer renderer in outlineOnClick.Renderers)
                renderer.material.color = Color.red;
        } else {
            foreach (Renderer renderer in outlineOnClick.Renderers)
                renderer.material.color = Color.grey;
        }
    }

    public override string GetObjectTypeName() {
        return "End action";
    }

    public override void CloseMenu() {
        throw new NotImplementedException();
    }


}
