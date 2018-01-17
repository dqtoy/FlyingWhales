using UnityEngine;
using System.Collections;

public class ClickHighlightHextile : MonoBehaviour {

	void OnEnable(){
		foreach (Transform child in this.gameObject.transform) {
			child.GetComponent<TweenPosition> ().PlayForward ();
		}
	}

	void OnDisable(){
		foreach (Transform child in this.gameObject.transform) {
			child.GetComponent<TweenPosition> ().ResetToBeginning ();
		}
	}
}
