using Unity;
using Base;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

public abstract class InteractiveObject : Clickable {

    public abstract string GetName();
    public abstract string GetId();

    public abstract string GetObjectTypeName();
    public abstract void OpenMenu();
    public abstract bool HasMenu();
    public abstract Task<RequestResult> Movable();
    public abstract void StartManipulation();

    public abstract Task<RequestResult> Removable();

    public abstract void Remove();
    public virtual float GetDistance(Vector3 origin) {
        float minDist = float.MaxValue;
        foreach (Collider collider in Colliders) {
            Vector3 point = collider.ClosestPointOnBounds(origin);
           
            minDist = Math.Min(Vector3.Distance(origin, point), minDist);

        }
        return minDist;
    }

    public virtual void Enable(bool enable) {
        Enabled = enable;
        foreach (Collider collider in Colliders) {
            collider.enabled = enable;
        }
    }

    public List<Collider> Colliders = new List<Collider>();

    public abstract void Rename(string name);
}
