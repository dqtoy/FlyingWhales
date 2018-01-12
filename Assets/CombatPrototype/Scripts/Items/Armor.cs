using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Armor : Item {
		public ARMOR_TYPE armorType;
        public BODY_PART armorBodyType;
		public MATERIAL material;
		public QUALITY quality;
		public float baseDamageMitigation;
		public float damageNullificationChance;
		public List<ATTACK_TYPE> ineffectiveAttackTypes;
		public List<ATTACK_TYPE> effectiveAttackTypes;
		public List<IBodyPart.ATTRIBUTE> attributes;
		internal IBodyPart bodyPartAttached;
	}
}
