using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Weapon : Item {
		public WEAPON_TYPE weaponType;
		public float skillPowerModifier;
		public List<IBodyPart.ATTRIBUTE> attributes;
		internal IBodyPart bodyPartAttached;
	}
}
