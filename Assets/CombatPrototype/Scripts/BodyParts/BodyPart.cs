using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	[System.Serializable]
	public class BodyPartsData {
		public RACE race;
		public List<BodyPart> bodyParts;
	}


	[System.Serializable]
	public class BodyPart: IBodyPart {
		[SerializeField] internal BODY_PART bodyPart;
		[SerializeField] internal IMPORTANCE importance;
		[SerializeField] internal List<ATTRIBUTE> attributes;
		[SerializeField] internal List<SecondaryBodyPart> secondaryBodyParts;
		internal STATUS status;

//		internal void SetData(BODY_PART bodyPart, IMPORTANCE importance, List<ATTRIBUTE> attributes, List<SecondaryBodyPart> secondaryBodyParts, STATUS status){
//			this.bodyPart = bodyPart;
//			this.importance = importance;
//			this.attributes = new List<ATTRIBUTE> (attributes);
//			this.secondaryBodyParts = new List<SecondaryBodyPart> (secondaryBodyParts);
//			this.status = status;
//		}
	}
}

