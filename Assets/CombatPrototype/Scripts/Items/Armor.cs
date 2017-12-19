using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	public class Armor : Item {
		public ARMOR_TYPE armorType;
        public BODY_PART armorBodyType;
		public int hitPoints;
        public int currHitPoints;
		public List<IBodyPart.ATTRIBUTE> attributes;
		internal IBodyPart bodyPartAttached;

        public void AdjustHitPoints(int adjustment) {
            currHitPoints += adjustment;
            currHitPoints = Mathf.Clamp(currHitPoints, 0, hitPoints);
            if(currHitPoints == 0) {
                //Armor Ineffective!
            }
        }

        public void ResetHitPoints() {
            currHitPoints = hitPoints;
        }
	}
}
