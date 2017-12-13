using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	[System.Serializable]
	public class SecondaryBodyPart: IBodyPart {
		[SerializeField] internal BODY_PART bodyPart;
		[SerializeField] internal IMPORTANCE importance;
		[SerializeField] internal List<ATTRIBUTE> attributes;
		internal List<STATUS> status;
	}
}

