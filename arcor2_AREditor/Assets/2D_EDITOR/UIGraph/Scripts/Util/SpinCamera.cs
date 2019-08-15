using UnityEngine;
using System.Collections;

public class SpinCamera : MonoBehaviour {
	public Vector3 speeds;

	// Update is called once per frame
	void Update () {
		transform.Rotate(speeds * Time.deltaTime);
	}
}
