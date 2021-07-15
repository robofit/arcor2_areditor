using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AREditorResources : Base.Singleton<AREditorResources>
{
    public Sprite Action, ActionPoint, ActionObject, Robot, Connection, InputOutput, RobotEE, Orientation, ActionInput, ActionOutput, Others, NoPose;

    public OutputTypeDialog OutputTypeDialog;
    public ConnectionSelectorDialog ConnectionSelectorDialog;

    public LeftMenuProject LeftMenuProject;

    public EditProjectParameterDialog EditProjectParameterDialog;
    public ActionParametersMenu ActionParametersMenu;

    public const float TooltipDelay = 0.5f;
}
