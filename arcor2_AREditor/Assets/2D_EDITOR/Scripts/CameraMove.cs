using UnityEngine;

public class CameraMove : MonoBehaviour {

    public GameObject Scene, ActionObjects;
    private bool moving;

    // Start is called before the first frame update
    private void Start() {
        moving = false;
    }

    // Update is called once per frame
    private void Update() {
        float speed = 80f;
        if (!Base.GameManager.Instance.SceneInteractable)
            return;

        Scene.transform.localScale += new Vector3(Input.mouseScrollDelta.y * 0.3f, Input.mouseScrollDelta.y * 0.3f, 0);
        //ActionObjects.transform.localScale -= new Vector3(Input.mouseScrollDelta.y * 0.1f, Input.mouseScrollDelta.y * 0.1f, 0);
        if (Input.GetMouseButtonUp(1)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (hit) {
                hit.collider.gameObject.SendMessage("OnClick", Base.Clickable.Click.MOUSE_RIGHT_BUTTON);
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
            Scene.transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * speed, Input.GetAxisRaw("Mouse Y") * Time.deltaTime * speed, 0f);
        }
    }
}
