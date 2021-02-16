using Unity;
using Base;
using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class InteractiveObject : Clickable {

    public abstract string GetName();
    public abstract string GetId();
    public abstract void OpenMenu();
    public abstract bool HasMenu();
    public abstract bool Movable();
    public abstract void StartManipulation();
    public virtual float GetDistance(Vector3 origin) {
        float minDist = float.MaxValue;
        foreach (Collider collider in Colliders) {
            Vector3 point = collider.ClosestPointOnBounds(origin);
           
            minDist = Math.Min(Vector3.Distance(origin, point), minDist);

        }
        return minDist;
    }

    public List<Collider> Colliders = new List<Collider>();


}
