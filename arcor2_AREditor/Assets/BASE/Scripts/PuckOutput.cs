
namespace Base {
    public class PuckOutput : InputOutput
        {

        public override void OnHoverStart() {
            if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionOutput)
                return;
            base.OnHoverStart();
        }
    }

}
