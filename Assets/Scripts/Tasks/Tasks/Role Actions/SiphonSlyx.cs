using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class SiphonSlyx : CharacterTask {
	private CraterBeast craterBeast;

	public SiphonSlyx(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.SIPHON_SLYX, stance, defaultDaysLeft) {
		_states = new Dictionary<STATE, State> {
			{ STATE.SIPHON, new SiphonState (this) }
		};
	}

	#region overrides
	public override void OnChooseTask (Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		craterBeast = (CraterBeast)_assignedCharacter.role;
		StartSiphon ();
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(character.currentHP < character.maxHP && location == character.specificLocation){
			for (int i = 0; i < location.charactersAtLocation.Count; i++) {
				if(location.charactersAtLocation[i] is ECS.Character){
					ECS.Character currCharacter = (ECS.Character) location.charactersAtLocation[i];
					if(currCharacter.role != null && currCharacter.role.roleType == CHARACTER_ROLE.SLYX){
						return true;
					}
				}
			}
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		if(CanBeDone(character, character.specificLocation)){
			return true;
		}
		return base.AreConditionsMet (character);
	}
	public override int GetSelectionWeight (Character character){
		int weight = (100 - ((100 * character.currentHP) / character.maxHP)) * 50;
		return weight;
	}
	#endregion

	private void StartSiphon(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartSiphon ());
//			return;
//		}

		ChangeStateTo (STATE.SIPHON);
//		_assignedCharacter.AddHistory ("Started siphoning life from slyxes!");
//		LandmarkManager.Instance.craterLandmark.AddHistory (_assignedCharacter.name + " started siphoning life from slyxes!");
	}
}
