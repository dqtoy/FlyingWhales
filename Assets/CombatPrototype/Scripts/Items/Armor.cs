using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Armor : Item {
		public ARMOR_TYPE armorType;
		public float damageMitigation;
		public List<IBodyPart.ATTRIBUTE> attributes;
		internal IBodyPart bodyPartAttached;
	}
}
