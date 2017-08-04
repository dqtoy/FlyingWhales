using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct CrimeData {
	public string fileName;
	public List<CHARACTER_VALUE> positiveValues;
	public List<CHARACTER_VALUE> negativeValues;

	public void DefaultValues(){
		this.fileName = string.Empty;
		this.positiveValues.Clear ();
		this.negativeValues.Clear ();
	}
}
