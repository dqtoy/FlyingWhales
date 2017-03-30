using UnityEngine;
using System.Collections;

[System.Serializable]
public struct IsActive {

	public bool status;

	public IsActive(bool status){
		this.status = status;
	}
}
