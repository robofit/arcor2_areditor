using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    public GameObject Cam, _MenuManager, _ConnectionManager;
    bool moving;
    public bool DrawVirtualConnection;
    public GameObject VirtualPointer;
    GameManager GameManager;
    // Start is called before the first frame update
    void Start()
    {
        DrawVirtualConnection = false;
        moving = false;
        GameManager = GameObject.Find("_GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 10f;
        if (Input.GetMouseButtonUp(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (hit)
            {
                hit.collider.gameObject.SendMessage("Touch");
                Debug.Log("Touch");
                Debug.Log(hit.collider.gameObject);
            }
            if (DrawVirtualConnection)
            {
                DrawVirtualConnection = false;
                _ConnectionManager.GetComponent<ConnectionManagerArcoro>().DestroyConnectionToMouse();
                GameManager.UpdateProject();
            }
        }

        else if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (!hit)
            {
                moving = true;
                //_MenuManager.GetComponent<MenuManager>().HideMenu();
            }
        }

        else if (Input.GetMouseButtonUp(0))
        {
            moving = false;
        }

        else if (moving && Input.GetMouseButton(0))
        { 
            Cam.transform.position += new Vector3(Input.GetAxisRaw("Mouse X") * Time.deltaTime * speed, Input.GetAxisRaw("Mouse Y") * Time.deltaTime * speed, 0f);                       
        }

        //Camera.main.orthographicSize -= Input.mouseScrollDelta.y/2;

        if (DrawVirtualConnection)
        {
            Vector3 mouseScreenPosition, mouseWorldPosition;
            mouseScreenPosition = Input.mousePosition;
            mouseWorldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, Camera.main.nearClipPlane + 1)); //The +1 is there so you don't overlap the object and the camera, otherwise the object is drawn "inside" of the camera, and therefore you're not able to see it!

            VirtualPointer.transform.position = mouseWorldPosition;
        }
            
    }
}
