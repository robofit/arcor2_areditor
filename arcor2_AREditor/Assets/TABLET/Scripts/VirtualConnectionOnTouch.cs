using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualConnectionOnTouch : Base.VirtualConnection {

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
                Base.GameManager.Instance.UpdateProject();
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
