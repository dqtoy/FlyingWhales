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

        #region overrides
        public override Item CreateNewCopy() {
            Armor copy = new Armor();
            copy.armorType = armorType;
            copy.armorBodyType = armorBodyType;
            copy.material = material;
            copy.quality = quality;
            copy.baseDamageMitigation = baseDamageMitigation;
            copy.damageNullificationChance = damageNullificationChance;
            
            copy.ineffectiveAttackTypes = new List<ATTACK_TYPE>(ineffectiveAttackTypes);
            copy.effectiveAttackTypes = new List<ATTACK_TYPE>(effectiveAttackTypes);
            copy.attributes = new List<IBodyPart.ATTRIBUTE>(attributes);
            copy.bodyPartAttached = null;
            SetCommonData(copy);
            return copy;
        }
        #endregion

        public void SetQuality(QUALITY quality) {
            this.quality = quality;
        }
    }
}
