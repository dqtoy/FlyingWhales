using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
	public class Hiding : Trait {

		private Character _owner;
		
		public Hiding() {
			name = "Hiding";
			description = "This character hiding from something.";
			type = TRAIT_TYPE.STATUS;
			effect = TRAIT_EFFECT.NEGATIVE;
			ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		}

		#region Overrides
		public override void OnAddTrait(ITraitable addedTo) {
			base.OnAddTrait(addedTo);
			if (addedTo is Character) {
				Character character = addedTo as Character;
				_owner = character;
				_owner.CancelAllJobs();
				LocationStructure targetStructure = _owner.homeStructure.GetLocationStructure();
				Debug.Log($"{GameManager.Instance.TodayLogString()}{_owner.name} is going to hide at {targetStructure.GetNameRelativeTo(_owner)}");
				_owner.DecreaseCanWitness();
				_owner.marker.GoTo(targetStructure.GetRandomTile(), OnArriveAtHideStructure);
			}
		}

		private void OnArriveAtHideStructure() {
			Debug.Log($"{GameManager.Instance.TodayLogString()}{_owner.name} has arrived at {_owner.currentStructure.GetNameRelativeTo(_owner)}");
			_owner.IncreaseCanWitness();
			//TODO: set limiter for structure.
			_owner.trapStructure.SetForcedStructure(_owner.currentStructure);
			_owner.DecreaseCanTakeJobs();
			StartCheckingForCowering();
		}
		
		public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
			base.OnRemoveTrait(removedFrom, removedBy);
			if (removedFrom is Character) {
				Character character = removedFrom as Character;
				StopCheckingForCowering();
				character.trapStructure.SetForcedStructure(null);
				_owner.IncreaseCanTakeJobs();
				character.needsComponent.CheckExtremeNeeds();
			}
		}
		#endregion

		#region Cowering
		private void StartCheckingForCowering() {	
			Messenger.AddListener(Signals.HOUR_STARTED, CoweringCheck);
		}
		private void CoweringCheck() {
			string summary = $"{GameManager.Instance.TodayLogString()}{_owner.name} is rolling for cower.";
			int roll = Random.Range(0, 100);
			int chance = 50;
			summary += $"\nRoll is {roll.ToString()}. Chance is {chance.ToString()}";
			if (roll < chance) {
				summary += $"\nChance met, triggering cowering interrupt";
				_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, _owner);
			}
			Debug.Log(summary);
		}
		private void StopCheckingForCowering() {
			Messenger.RemoveListener(Signals.HOUR_STARTED, CoweringCheck);
		}
		#endregion
	}
}