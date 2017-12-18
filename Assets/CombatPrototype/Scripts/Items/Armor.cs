using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Armor : Item {
		public ARMOR_TYPE armorType;
        public BODY_PART armorBodyType;
		public int hitPoints;
		public List<IBodyPart.ATTRIBUTE> attributes;
		internal IBodyPart bodyPartAttached;
	}
}
