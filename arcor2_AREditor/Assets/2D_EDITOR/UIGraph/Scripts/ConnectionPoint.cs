using UnityEngine;

[System.Serializable]
public class ConnectionPoint {
	public enum ConnectionDirection {
		North,
		East,
		South,
		West,
		Polar
	}

	//public RectTransform transform;
	public Color color = Color.white;
	public ConnectionDirection direction = ConnectionDirection.North;
	[Range(-1f, 1f)] public float position = 0f;
	public float weight = 1f;

	public Vector3 p {get; private set;}
	public Vector3 c {get; private set;}

	public void Reset() {
		color = Color.white;
		direction = ConnectionDirection.North;
		position = 0f;
		weight = 1f;
	}

	public void CalculateVectors(RectTransform transform) {
		if (!transform) return;

		switch (direction) {
			case ConnectionDirection.North:
				p = transform.TransformPoint(
					transform.rect.width/2f * position,
					transform.rect.height/2f,
					0);
				c = p + transform.up * weight;
			break;

			case ConnectionDirection.South:
				p = transform.TransformPoint(
					transform.sizeDelta.x/2f * position,
					-transform.sizeDelta.y/2f,
					0);
				c = p - transform.up * weight;
			break;

			case ConnectionDirection.East:
				p = transform.TransformPoint(
					transform.sizeDelta.x/2f,
					transform.sizeDelta.y/2f * position,
					0);
				c = p + transform.right * weight;
			break;

			case ConnectionDirection.West:
				p = transform.TransformPoint(
					-transform.sizeDelta.x/2f,
					transform.sizeDelta.y/2f * position,
					0);
				c = p - transform.right * weight;
			break;

			default:
				float angle = Mathf.PI/2f - position*Mathf.PI;
				p = transform.TransformPoint(
					transform.sizeDelta.x/2f * Mathf.Cos(angle),
					transform.sizeDelta.y/2f * Mathf.Sin(angle),
					0);
				c = p + transform.TransformDirection(Mathf.Cos(angle), Mathf.Sin(angle), 0) * weight;
			break;
		}
	}
}
