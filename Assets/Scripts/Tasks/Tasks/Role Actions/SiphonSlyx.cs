using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class SiphonSlyx : CharacterTask {
	private CraterBeast craterBeast;

	public SiphonSlyx(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.SIPHON_SLYX, stance, defaultDaysLeft) {
	}

	#region overrides
	public override void OnChooseTask (Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		craterBeast = (CraterBeast)_assignedCharacter.role;
	}
	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();

		if(_daysLeft == 0){
			EndSiphon();
			return;
		}
		ReduceDaysLeft (1);
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

	private void SiphonLife(){
		for (int i = 0; i < _assignedCharacter.specificLocation.charactersAtLocation.Count; i++) {
			if(_assignedCharacter.specificLocation.charactersAtLocation[i] is ECS.Character){
				ECS.Character currCharacter = (ECS.Character) _assignedCharacter.specificLocation.charactersAtLocation[i];
				if(currCharacter.role != null && currCharacter.role.roleType == CHARACTER_ROLE.SLYX){
                    Log siphonLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "SiphonSlyx", "siphon");
                    siphonLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    siphonLog.AddToFillers(currCharacter.currentHP, currCharacter.currentHP.ToString(), LOG_IDENTIFIER.OTHER);
                    siphonLog.AddToFillers(currCharacter, currCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                    _assignedCharacter.AddHistory(siphonLog);
                    currCharacter.AddHistory(siphonLog);
                    _assignedCharacter.AdjustHP (currCharacter.currentHP);
					currCharacter.Death ();
					i--;
					if(_assignedCharacter.currentHP >= _assignedCharacter.maxHP){
						break;
					}
				}
			}
		}
	}

	private void EndSiphon() {
		SiphonLife ();
		EndTask(TASK_STATUS.SUCCESS);
	}

}
