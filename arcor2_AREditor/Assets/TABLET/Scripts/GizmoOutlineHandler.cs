using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The script has to be placed on GameObject manipulated by TransformGizmo.
/// Clear gizmo will be called when the GameObject is deselected.
/// </summary>
public class GizmoOutlineHandler : MonoBehaviour {

    public OutlineOnClick OutlineOnClick;

    /// <summary>
    /// Called via SendMessage("ClearGizmo") from TransformGizmo.cs
    /// </summary>
    public void ClearGizmo() {
        OutlineOnClick.GizmoUnHighlight();
    }

}
