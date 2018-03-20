using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HuntState : State {

	public HuntState(CharacterTask parentTask): base (parentTask, STATE.HUNT){

	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Hunt ();
		return true;
	}
	#endregion

	private void Hunt(){
		HUNT_ACTION chosenAct = TaskManager.Instance.huntActions.PickRandomElementGivenWeights();
		switch (chosenAct) {
		case HUNT_ACTION.EAT:
			EatCivilian();
			break;
		case HUNT_ACTION.END:
			parentTask.EndTaskSuccess();
			break;
		case HUNT_ACTION.NOTHING:
			break;
		default:
			break;
		}
	}
	private void EatCivilian() {
		if(_targetLandmark.civilians > 0) {
			RACE[] races = _targetLandmark.civiliansByRace.Keys.Where(x => _targetLandmark.civiliansByRace[x] > 0).ToArray();
			RACE chosenRace = races [UnityEngine.Random.Range (0, races.Length)];
			_targetLandmark.AdjustCivilians (chosenRace, -1);
			Log eatLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "HuntPrey", "eat_civilian");
			eatLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			eatLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(chosenRace).ToLower(), LOG_IDENTIFIER.OTHER);
			_targetLandmark.AddHistory(eatLog);
			_assignedCharacter.AddHistory(eatLog);

			//          _target.ReduceCivilians(1);
			//GameDate nextDate = GameManager.Instance.Today();
			//nextDate.AddDays(1);
			//SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
		}
	}
}
