using Base;

public class CreateAnchor : Base.Clickable {

    public override void OnClick(Click type) {
        if (GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.Normal ||
            GameManager.Instance.GetEditorState() == GameManager.EditorStateEnum.InteractionDisabled) {
            CalibrationManager.Instance.CreateAnchor(transform);
        }
    }

    public override void OnHoverStart() {

    }

    public override void OnHoverEnd() {

    }

}
