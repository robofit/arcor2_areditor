
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



    private void OnGeneralClick(object sender, EventClickArgs e) {
        if (e.ClickType == Click.TOUCH_ENDED) {
            Deselect();
        }
    }

}
