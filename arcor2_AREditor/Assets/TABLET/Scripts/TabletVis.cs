using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletVis : Base.Clickable {
    [SerializeField]
    private GameObject name;
    public override void OnClick(Click type) {
        
    }

    public override void OnHoverEnd() {
        name.SetActive(false);
    }

    public override void OnHoverStart() {
        name.SetActive(true);
    }
}
