using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class ClippingPlane : MonoBehaviour {
    //Clipping plane script from RonjaTutorials: https://www.ronja-tutorials.com/post/021-plane-clipping/


    public List<Material> Materials;

    void Update() {
        Plane plane;
      
        plane = new Plane(transform.up, transform.position);
        
        
        Vector4 planeRepresentation = new Vector4(plane.normal.x, plane.normal.y, plane.normal.z, plane.distance);

        //pass vector to shader
        foreach (Material material in Materials) {
            material.SetVector("_Plane", planeRepresentation);
        }
        
    }
}
