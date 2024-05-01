using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ClippingPlane : MonoBehaviour {
    //materials we pass the value to
    public List<Material> Materials;

    public bool dir = false;

    void Update() {
        //create plane
        Plane plane;
      
        plane = new Plane(transform.up, transform.position);
        
        
        //transfer values from plane to vector4
        Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);
        //pass vector to shader
        foreach (Material material in Materials) {
            material.SetVector("_Plane", planeRepresentation);
        }
        
    }
}
