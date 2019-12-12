using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class CreateAnchor : MonoBehaviour
{
    private ARAnchorManager arAnchorManager;
    private ARPlaneManager arPlaneManager;
    private ARAnchor worldAnchor;

    // Start is called before the first frame update
    private void Start() {
        arAnchorManager = GameObject.FindWithTag("AR_MANAGERS").GetComponent<ARAnchorManager>();
        arPlaneManager = GameObject.FindWithTag("AR_MANAGERS").GetComponent<ARPlaneManager>();
        worldAnchor = arAnchorManager.AddAnchor(new Pose(Camera.main.transform.position, Camera.main.transform.rotation));
    }

    private void OnEnable() {
        Base.GameManager.Instance.OnConnectedToServer += ConnectedToServer;
    }

    private void OnDisable() {
        Base.GameManager.Instance.OnConnectedToServer -= ConnectedToServer;
    }

    public void OnClick() {
        Debug.Log("ADDING ANCHOR");
        try {
            arAnchorManager.RemoveAnchor(worldAnchor);
        } catch(NullReferenceException e) {
            Debug.Log(e);
        }

        worldAnchor = arAnchorManager.AddAnchor(new Pose(transform.position, transform.rotation));

        if (Base.GameManager.Instance.ConnectionStatus == Base.GameManager.ConnectionStatusEnum.Connected)
            AttachAnchor();
    }

    private void AttachAnchor() {
        Base.Scene.Instance.transform.parent = worldAnchor.transform;

        Base.Scene.Instance.transform.localPosition = Vector3.zero;
        Base.Scene.Instance.transform.localScale = new Vector3(1f, 1f, 1f);
        Base.Scene.Instance.transform.localEulerAngles = Vector3.zero;
    }

    private void ConnectedToServer(object sender, Base.StringEventArgs e) {
        AttachAnchor();
    }

}
