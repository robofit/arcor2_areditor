using UnityEngine;

[RequireComponent(typeof(LineRenderer)), ExecuteInEditMode]
public class Connection : MonoBehaviour {
	const int minResolution = 2;
	const int maxResolution = 20;
	const int avgResolution = 20;

	//TODO: rename to "targets", plural
	public RectTransform[] target = new RectTransform[2];
	public ConnectionPoint[] points = new ConnectionPoint[2]{
		new ConnectionPoint(),
		new ConnectionPoint()
	};

	[SerializeField, Range(minResolution, maxResolution)] int resolution = avgResolution;

	[SerializeField] LineRenderer _line;
	public LineRenderer line {
		get {
			if (!_line) _line = GetComponent<LineRenderer>();
			return _line;
		}
	}

	public bool isValid {
		get {return target[0] && target[1];}
	}

	public bool Match(RectTransform start, RectTransform end) {
		if (!start || !end) return false;

		return
			start.Equals(target[0]) && end.Equals(target[1]) ||
			end.Equals(target[0]) && start.Equals(target[1]);
	}

	public bool Contains(RectTransform transform) {
		if (!transform) return false;

		return transform.Equals(target[0]) || transform.Equals(target[1]);
	}

	public int GetIndex(RectTransform transform) {
		if (transform) {
			if (transform.Equals(target[0])) return 0;
			if (transform.Equals(target[1])) return 1;
		}
		return -1;
	}

	public void OnValidate() {
		OrganizeTransforms();
		UpdateName();
		UpdateCurve();
	}

	void Awake() {
		ConnectionManager.AddConnection(this);
	}

	void OnDestroy() {
		ConnectionManager.RemoveConnection(this);
	}

	void Update() {
		if (isValid) {
			if (target[0].hasChanged || target[1].hasChanged) {
				UpdateCurve();
			}
		}
	}

	void OrganizeTransforms() {
		string n1 = target[0] ? target[0].name : null;
		string n2 = target[1] ? target[1].name : null;

		if (string.Compare(n1, n2) > 0) {
			RectTransform swapT = target[1];
			target[1] = target[0];
			target[0] = swapT;

			ConnectionPoint swapP = points[1];
			points[1] = points[0];
			points[0] = swapP;
		}
	}

	void UpdateName() {
		string n1 = target[0] ? target[0].name : "None";
		string n2 = target[1] ? target[1].name : "None";
		gameObject.name = string.Format("{0} <> {1}", n1, n2);
	}

	void UpdateCurve() {
		if (!line) return;
		if (!isValid) {
			line.enabled = false;
			return;
		}

		bool sActive = target[0].gameObject.activeInHierarchy;
		bool eActive = target[1].gameObject.activeInHierarchy;

		if (!sActive && !eActive) {
			line.enabled = false;
		} else {
			line.enabled = true;
			if (sActive && !eActive) {
				line.SetColors(points[0].color, Color.clear);
			} else if (!sActive && eActive) {
				line.SetColors(Color.clear, points[1].color);
			} else {
				line.SetColors(points[0].color, points[1].color);
			}
		}

		points[0].CalculateVectors(target[0]);
		points[1].CalculateVectors(target[1]);

		line.SetVertexCount(resolution);
		for (int i = 0; i < resolution; i++) {
			line.SetPosition(i, GetBezierPoint((float)i/(float)(resolution-1)));
		}

		//handle icons here

		transform.position = GetBezierPoint(.5f);
	}

	public Vector3 GetBezierPoint(float t, int derivative = 0) {
		derivative = Mathf.Clamp(derivative, 0, 2);
		float u = (1f-t);
		Vector3 p1 = points[0].p, p2 = points[1].p, c1 = points[0].c, c2 = points[1].c;

		if (derivative == 0) {
			return u*u*u*p1 + 3f*u*u*t*c1 + 3f*u*t*t*c2 + t*t*t*p2;

		} else if (derivative == 1) {
			return 3f*u*u*(c1-p1) + 6f*u*t*(c2-c1) + 3f*t*t*(p2-c2);

		} else if (derivative == 2) {
			return 6f*u*(c2-2f*c1+p1) + 6f*t*(p2-2f*c2+c1);

		} else {
			return Vector3.zero;
		}
	}

	public void SetTargets(RectTransform t1, RectTransform t2) {
		target[0] = t1;
		target[1] = t2;
	}
}