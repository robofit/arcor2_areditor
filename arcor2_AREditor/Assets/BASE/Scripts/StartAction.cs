using System;
using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

public class StartAction : StartEndAction
{
    private void Awake() {
        IO.Swagger.Model.Action projectAction = new IO.Swagger.Model.Action(
            flows: new List<IO.Swagger.Model.Flow> {
                new IO.Swagger.Model.Flow(
                    new List<string> { "output" }, IO.Swagger.Model.Flow.TypeEnum.Default) },
            id: "START",
            name: "START",
            parameters: new List<IO.Swagger.Model.ActionParameter>(),
            type: "");
        Init(projectAction, null, null, null, "START");
    }

    public override void Init(IO.Swagger.Model.Action projectAction, Base.ActionMetadata metadata, Base.ActionPoint ap, IActionProvider actionProvider, string keySuffix) {
        base.Init(projectAction, metadata, ap, actionProvider, keySuffix);
        transform.localPosition = PlayerPrefsHelper.LoadVector3(playerPrefsKey, new Vector3(0.5f, 0, 0));
    }


    public override void Enable() {
        base.Enable();
        foreach (Renderer renderer in outlineOnClick.Renderers)
            renderer.material.color = Color.green;
    }
}
