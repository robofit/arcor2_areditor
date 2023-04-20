using UnityEngine;

public class VirtualConnectionOnTouch : Base.VirtualConnection {
    private Vector3 mouseScreenPosition, mouseWorldPosition;
    // Start is called before the first frame update
    private void Start() {
        DrawVirtualConnection = false;
        mouseScreenPosition = new Vector3(Screen.width / 2, Screen.height / 2);
    }

    // Update is called once per frame
    private void Update() {
        if (!Base.GameManager.Instance.SceneInteractable)
            return;


        if (DrawVirtualConnection) {
#if UNITY_EDITOR || UNITY_STANDALONE
            mouseScreenPosition = Input.mousePosition;
#endif
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane + 1)); //The +1 is there so you don't overlap the object and the camera, otherwise the object is drawn "inside" of the camera, and therefore you're not able to see it!

            VirtualPointer.transform.position = mouseWorldPosition;
        }
    }
}
