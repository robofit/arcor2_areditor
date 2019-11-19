using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceButton : MonoBehaviour
{
    //public string Type;
    public IO.Swagger.Model.ServiceMeta ServiceMetadata;
    public GameObject Yes, No;
    private bool state;

    public bool State {
        get => state;
        set {
            state = value;
            if (state) {
                No.SetActive(false);
                Yes.SetActive(true);
            } else {
                Yes.SetActive(false);
                No.SetActive(true);
            }
        }
    }

}
