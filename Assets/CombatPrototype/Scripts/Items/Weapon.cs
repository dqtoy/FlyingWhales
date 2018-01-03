using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Weapon : Item {
		public WEAPON_TYPE weaponType;
		public float weaponPower;
        public int durabilityDamage;
		public List<IBodyPart.ATTRIBUTE> attributes;
        public List<IBodyPart.ATTRIBUTE> equipRequirements;
		internal List<IBodyPart> bodyPartsAttached = new List<IBodyPart>();
	}
}
