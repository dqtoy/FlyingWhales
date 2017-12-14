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
		[SerializeField] internal List<SecondaryBodyPart> secondaryBodyParts;

		//This applies status effect to all secondary body part of this main body part
		//Whatever status effect added to the main body part will be added to secondary body part since they are linked
		internal void ApplyStatusEffectOnSecondaryBodyParts(STATUS_EFFECT statusEffect){
			for (int i = 0; i < this.secondaryBodyParts.Count; i++) {
				this.secondaryBodyParts [i].statusEffects.Add (statusEffect);
			}
		}

		//This removes status effect to all secondary body part of this main body part
		internal void RemoveStatusEffectOnSecondaryBodyParts(STATUS_EFFECT statusEffect){
			for (int i = 0; i < this.secondaryBodyParts.Count; i++) {
				this.secondaryBodyParts [i].statusEffects.Remove (statusEffect);
			}
		}
//		internal void SetData(BODY_PART bodyPart, IMPORTANCE importance, List<ATTRIBUTE> attributes, List<SecondaryBodyPart> secondaryBodyParts, STATUS status){
//			this.bodyPart = bodyPart;
//			this.importance = importance;
//			this.attributes = new List<ATTRIBUTE> (attributes);
//			this.secondaryBodyParts = new List<SecondaryBodyPart> (secondaryBodyParts);
//			this.status = status;
//		}
	}
}

