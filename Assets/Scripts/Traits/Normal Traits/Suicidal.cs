using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
	public class Suicidal : Trait {
		
		public Suicidal() {
			name = "Suicidal";
			description = "This character wants to kill himself/herself.";
			type = TRAIT_TYPE.STATUS;
			effect = TRAIT_EFFECT.NEGATIVE;
			ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
		}
	}	
}

