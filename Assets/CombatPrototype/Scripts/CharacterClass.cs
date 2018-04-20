using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    [System.Serializable]
    public class CharacterClass : EntityComponent {
        public string className;
		public float strPercentage;
		public float intPercentage;
		public float agiPercentage;
		public float hpPercentage;
        public int dodgeRate;
        public int parryRate;
        public int blockRate;
		public List<WEAPON_TYPE> allowedWeaponTypes;

        public CharacterClass CreateNewCopy() {
            CharacterClass newClass = new CharacterClass();
            newClass.className = this.className;
			newClass.strPercentage = this.strPercentage;
			newClass.intPercentage = this.intPercentage;
			newClass.agiPercentage = this.agiPercentage;
			newClass.hpPercentage = this.hpPercentage;
            newClass.dodgeRate = this.dodgeRate;
            newClass.parryRate = this.parryRate;
            newClass.blockRate = this.blockRate;
			newClass.allowedWeaponTypes = this.allowedWeaponTypes;
            return newClass;
        }
    }
}

