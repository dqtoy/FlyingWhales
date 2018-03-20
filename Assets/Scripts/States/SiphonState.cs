using UnityEngine;
using System.Collections;

public class SiphonState : State {

	public SiphonState(CharacterTask parentTask): base (parentTask, STATE.SIPHON){

	}

	#region Overrides
	public override void PerformStateAction (){
		base.PerformStateAction ();
		SiphonLife ();
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
					break;
				}
			}
		}
		if(_assignedCharacter.currentHP >= _assignedCharacter.maxHP){
			parentTask.EndTaskSuccess ();
		}
	}
}
