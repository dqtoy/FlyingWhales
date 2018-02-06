using UnityEngine;
using System.Collections;

[System.Serializable]
public struct Structure {
	public string name{
		get { return structureQuality.ToString () + " " + structureType.ToString (); }
	}
	public STRUCTURE_TYPE structureType;
	public STRUCTURE_QUALITY structureQuality;
}
