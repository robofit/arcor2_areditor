using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Updates created virtual connection between InputOutput object and VirtualPointer that is updated on mouse position.
/// On right click, the connection is destryoed.
/// </summary>
public class VirtualConnectionOnClick : Base.VirtualConnection {

    // Start is called before the first frame update
    private void Start() {
        DrawVirtualConnection = false;
    }

    // Update is called once per frame
    private void Update() {
        if (!Base.GameManager.Instance.SceneInteractable)
            return;

        if (Input.GetMouseButtonUp(1)) {
            if (DrawVirtualConnection) {

                DrawVirtualConnection = false;
                ConnectionManagerArcoro.Instance.DestroyConnectionToMouse();
                Base.GameManager.Instance.CancelSelection();
                //TODO - update connections via RPC
            }
        }

        if (DrawVirtualConnection) {
            Vector3 mouseScreenPosition, mouseWorldPosition;
            mouseScreenPosition = Input.mousePosition;
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane + 1)); //The +1 is there so you don't overlap the object and the camera, otherwise the object is drawn "inside" of the camera, and therefore you're not able to see it!

            VirtualPointer.transform.position = mouseWorldPosition;
        }
    }
}
