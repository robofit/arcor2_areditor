using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class StartAction : StartEndAction
{

    public override void Init(IO.Swagger.Model.Action projectAction, Base.ActionMetadata metadata, Base.ActionPoint ap, IActionProvider actionProvider, string actionType) {
        IO.Swagger.Model.Action prAction = new IO.Swagger.Model.Action(
            flows: new List<IO.Swagger.Model.Flow> {
                new IO.Swagger.Model.Flow(
                    outputs: new List<string> { "output" }, type: IO.Swagger.Model.Flow.TypeEnum.Default) },
            id: "START",
            name: "START",
            parameters: new List<IO.Swagger.Model.ActionParameter>(),
            type: "");
        base.Init(prAction, metadata, ap, actionProvider, actionType);
        transform.localPosition = PlayerPrefsHelper.LoadVector3(playerPrefsKey, new Vector3(0, 0.15f, 0));
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
