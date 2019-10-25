using System;
using UnityEngine;
using UnityEngine.UI;
public class SSTools : MonoBehaviour {

    public enum Time
    {
        threeSecond,
        twoSecond,
        oneSecond
    };
    public enum Position
    {
        top,
        bottom
    };
    public static void ShowMessage ( string msg, SSTools.Position position, SSTools.Time time )
    {

        //Load message prefab from resources folder
        GameObject messagePrefab = Resources.Load ( "Message" ) as GameObject;
        //Get container object of message
        GameObject containerObject = messagePrefab.gameObject.transform.GetChild ( 0 ).gameObject;
        //Get text object
        GameObject textObject = containerObject.gameObject.transform.GetChild ( 0 ).GetChild ( 0 ).gameObject;
        //Get text property
        Text msg_text = textObject.GetComponent<Text> ( );
        //Set message to text ui
        msg_text.text = msg;
        //Set position of container object of message
        SetPosition ( containerObject.GetComponent<RectTransform> ( ), position );
        //Spawn message object with all changes
        GameObject clone = Instantiate ( messagePrefab );
        // Destroy clone of message object according to the time
        RemoveClone ( clone, time );
    }

    private static void SetPosition ( RectTransform rectTransform, Position position )
    {
        if (position == Position.top)
        {
            rectTransform.anchorMin = new Vector2 ( 0.5f, 1f );
            rectTransform.anchorMax = new Vector2 ( 0.5f, 1f );
            rectTransform.anchoredPosition = new Vector3 ( 0.5f, -100f, 0 );
        }
        else
        {
            rectTransform.anchorMin = new Vector2 ( 0.5f, 0 );
            rectTransform.anchorMax = new Vector2 ( 0.5f, 0 );
            rectTransform.anchoredPosition = new Vector3 ( 0.5f, 100f, 0 );
        }
    }

    private static void RemoveClone ( GameObject clone, Time time )
    {
        if (time == Time.oneSecond)
        {
            Destroy ( clone.gameObject, 1f );
        }
        else if (time == Time.twoSecond)
        {
            Destroy ( clone.gameObject, 2f );
        }
        else
        {
            Destroy ( clone.gameObject, 3f );
        }
    }
}
