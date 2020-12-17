using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletVis : InteractiveObject {
    [SerializeField]
    private GameObject name;

    public override string GetName() {
        return "Me";
    }

    public override void OnClick(Click type) {
        
    }

    public override void OnHoverEnd() {
        name.SetActive(false);
    }

    public override void OnHoverStart() {
        name.SetActive(true);
    }
}
