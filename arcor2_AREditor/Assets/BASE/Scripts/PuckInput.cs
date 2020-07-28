
namespace Base
{
    public class PuckInput : InputOutput {


        public override void OnHoverStart() {
            if (GameManager.Instance.GetEditorState() != GameManager.EditorStateEnum.SelectingActionInput)
                return;
            base.OnHoverStart();
        }
    }

}
