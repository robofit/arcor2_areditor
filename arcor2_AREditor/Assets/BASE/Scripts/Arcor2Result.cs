using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arcor2Result : MonoBehaviour
{
    public bool Result;
    public string Message;

    public Arcor2Result(bool result, string message) {
        Result = result;
        Message = message;
    }

    public Arcor2Result(bool result, List<string> messages) {
        Result = result;
        Message = messages.Count > 0 ? messages[0] : "";
    }


}
