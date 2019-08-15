using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    private string _id, _type;
    private Vector2 _position;

    public Node(string id, string type, Vector2 position)
    {        
        _id = id;
        _type = type;
        _position = position;
    }

    private void Start()
    {
        
    }

    void Update()
    {
        
    }

}