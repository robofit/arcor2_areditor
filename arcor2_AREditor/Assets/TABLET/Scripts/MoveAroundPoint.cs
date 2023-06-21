using UnityEngine;

public class MoveAroundPoint : MonoBehaviour {
    public Transform Center;
    public Vector3 RotateAroundAxis;
    public float Speed;

    // Update is called once per frame
    void Update() {
        transform.RotateAround(Center.transform.position, RotateAroundAxis, Speed);
    }
}
