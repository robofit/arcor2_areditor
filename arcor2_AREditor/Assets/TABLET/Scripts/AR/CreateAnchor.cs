using Base;

public class CreateAnchor : InteractiveObject {

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

    public override string GetName() {
        return "Calibration cube";
    }

    public override string GetId() {
        return "Calibration cube";
    }

    public override void OpenMenu() {
        throw new System.NotImplementedException();
    }

    public override bool HasMenu() {
        return false;
    }

    public override bool Movable() {
        return false;
    }

    public override void StartManipulation() {
        throw new System.NotImplementedException();
    }
}
