using System.Collections;
using System.Collections.Generic;
using Base;
using UnityEngine;

/// <summary>
/// Inherited class of OutlineOnClick for displaying outline on touch began and immediately hiding the outline on touch end.
/// </summary>
public class OutlineOnClickHighlight : OutlineOnClick {
    
    private void OnEnable() {
        InputHandler.Instance.OnGeneralClick += OnGeneralClick;
    }

    private void OnDisable() {
        if (InputHandler.Instance != null) {
            InputHandler.Instance.OnGeneralClick -= OnGeneralClick;
        }
    }

    public override void OnClick(Click type) {
        if (type == Click.TOUCH) {
            Select();
        } else if (type == Click.TOUCH_ENDED || type == Click.LONG_TOUCH) {
            Deselect();
        }
    }

    private void OnGeneralClick(object sender, EventClickArgs e) {
        if (e.ClickType == Click.TOUCH_ENDED) {
            Deselect();
        }
    }

}
