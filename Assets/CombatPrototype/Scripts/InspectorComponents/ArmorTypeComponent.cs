using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class ArmorTypeComponent : MonoBehaviour {
		public ARMOR_TYPE armorType;
		public BODY_PART armorBodyType;
		public List<MATERIAL> armorMaterials;
    }
}

