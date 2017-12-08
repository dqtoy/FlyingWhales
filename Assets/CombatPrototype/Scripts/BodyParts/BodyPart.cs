using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class BodyPart {
	public enum IMPORTANCE {
		ESSENTIAL,
		NON_ESSENTIAL,
	}

	public enum ATTRIBUTE {
		CLAWED,
		CAN_PUNCH,
		CAN_GRIP,
	}

	public enum STATUS {
		NORMAL,
		INJURED,
		DECAPITATED,
	}

//	[SerializeField]
//	[HideInInspector]
//	private string strBodyPart;

	public BODY_PART bodyPart;
	public IMPORTANCE importance;
	public STATUS status;
	public List<ATTRIBUTE> attributes;
	public List<BodyPart> bodyParts;
}
