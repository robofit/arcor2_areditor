using UnityEngine;

public class CameraMove : MonoBehaviour {

    public GameObject Cam, MenuManager, ConnectionManager;
    private bool moving;
    public bool DrawVirtualConnection;
    public GameObject VirtualPointer;

    // Start is called before the first frame update
    private void Start() {
        DrawVirtualConnection = false;
        moving = false;
    }

    // Update is called once per frame
    private void Update() {
        float speed = 10f;
        if (Input.GetMouseButtonUp(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (hit) {
                hit.collider.gameObject.SendMessage("Touch");
            }
            if (DrawVirtualConnection) {
                DrawVirtualConnection = false;
                ConnectionManager.GetComponent<ConnectionManagerArcoro>().DestroyConnectionToMouse();
                GameManager.Instance.UpdateProject();
            }
        } else if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (!hit) {
                moving = true;
            }
        } else if (Input.GetMouseButtonUp(0)) {
            moving = false;
        } else if (moving && Input.GetMouseButton(0)) {
            Cam.transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * speed, Input.GetAxisRaw("Mouse Y") * Time.deltaTime * speed, 0f);
        }


        if (DrawVirtualConnection) {
            Vector3 mouseScreenPosition, mouseWorldPosition;
            mouseScreenPosition = Input.mousePosition;
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane + 1)); //The +1 is there so you don't overlap the object and the camera, otherwise the object is drawn "inside" of the camera, and therefore you're not able to see it!

            VirtualPointer.transform.position = mouseWorldPosition;
        }

    }
}
