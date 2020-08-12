using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoOutlineHandler : MonoBehaviour {

    public OutlineOnClick OutlineOnClick;

    public void ClearGizmo() {
        OutlineOnClick.GizmoUnHighlight();
    }

}
