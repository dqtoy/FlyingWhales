using UnityEngine;
using System.Collections;

public class NoScaleGO : MonoBehaviour {

	private float orthoOrg;
	private float orthoCurr;
	private Vector3 scaleOrg;
	//private Vector3 posOrg;

	void Start() {
		orthoOrg = Camera.main.orthographicSize;
		orthoCurr = orthoOrg;
		scaleOrg = transform.localScale;
		//posOrg = Camera.main.WorldToViewportPoint(transform.position);
	}
	void Update() {
		var osize = Camera.main.orthographicSize;
		if (orthoCurr != osize) {
			float minus = 0.04f * (orthoOrg - osize);
			Vector3 size = scaleOrg * osize / orthoOrg;
//			transform.localScale = size;
			transform.localScale = new Vector3(size.x + minus, size.y + minus, size.z + minus);
			orthoCurr = osize;
//			transform.position = Camera.main.ViewportToWorldPoint(posOrg);
		}

	}
}
