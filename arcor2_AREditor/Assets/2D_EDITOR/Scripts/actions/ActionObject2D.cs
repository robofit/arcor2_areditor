using UnityEngine;


    public class ActionObject2D : Base.ActionObject {


    private Vector3 offset;

    private void Touch() {
            MenuManager.Instance.ShowMenu(InteractiveObjectMenu, Id);
            InteractiveObjectMenu.GetComponent<InteractiveObjectMenu>().CurrentObject = gameObject;
        }

        private void OnMouseDown() => offset = gameObject.transform.position -
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f));

        private void OnMouseDrag() {
            Vector3 newPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10.0f);
            transform.position = Camera.main.ScreenToWorldPoint(newPosition) + offset;
        }

        private void OnMouseUp() => GameManager.Instance.UpdateScene();


    }


