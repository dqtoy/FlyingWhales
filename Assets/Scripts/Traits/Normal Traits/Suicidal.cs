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

		#region Overrides
		public override void OnAddTrait(ITraitable addedTo) {
			base.OnAddTrait(addedTo);
			if (addedTo is Character) {
				Character character = addedTo as Character;
				character.behaviourComponent.ReplaceBehaviourComponent(typeof(DefaultAtHome),
					typeof(SuicidalBehaviour));
			}
		}
		public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
			base.OnRemoveTrait(removedFrom, removedBy);
			if (removedFrom is Character) {
				Character character = removedFrom as Character;
				character.behaviourComponent.ReplaceBehaviourComponent(typeof(SuicidalBehaviour),
					typeof(DefaultAtHome));
			}
		}
		#endregion
	}	
}

