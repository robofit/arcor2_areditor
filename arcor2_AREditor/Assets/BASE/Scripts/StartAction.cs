using System;
using System.Collections.Generic;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
using UnityEngine;
using Action = Arcor2.ClientSdk.Communication.OpenApi.Models.Action;
using ActionMetadata = Base.ActionMetadata;
using ActionPoint = Base.ActionPoint;

public class StartAction : StartEndAction {

    public override void Init(Action projectAction, ActionMetadata metadata, ActionPoint ap, IActionProvider actionProvider, string actionType) {
        Action prAction = new(
            flows: new List<Flow> {
                new(
                    outputs: new List<string> { "output" }, type: Flow.TypeEnum.Default) },
            id: "START",
            name: "START",
            parameters: new List<ActionParameter>(),
            type: "");
        base.Init(prAction, metadata, ap, actionProvider, actionType);
        transform.localPosition = PlayerPrefsHelper.LoadVector3(playerPrefsKey, new Vector3(0, 0.15f, -0.6f));
        // Output.SelectorItem = SelectorMenu.Instance.CreateSelectorItem(Output);
    }


    public override void UpdateColor() {
        Color color = Enabled ? Color.green : Color.gray;
        foreach (Renderer renderer in outlineOnClick.Renderers)
            renderer.material.color = color;
    }

    public override string GetObjectTypeName() {
        return "Start action";
    }

    public override void CloseMenu() {
        throw new NotImplementedException();
    }


}
