using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AREditorResources : Base.Singleton<AREditorResources>
{
    public Sprite Action, ActionPoint, ActionObject, Robot, Connection, InputOutput, RobotEE, Orientation, ActionInput, ActionOutput, Others, NoPose;

    public OutputTypeDialog OutputTypeDialog;
    public ConnectionSelectorDialog ConnectionSelectorDialog;

    public LeftMenuProject LeftMenuProject;

    public EditConstantDialog EditConstantDialog;
    public ProjectConstantPicker ProjectConstantPicker;
}
