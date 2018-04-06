using UnityEngine;
using System.Collections;
using System.Linq;
using ECS;

public class PillageState : State {

	public PillageState(CharacterTask parentTask): base (parentTask, STATE.PILLAGE){

	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Pillage ();
		return true;
	}
	#endregion

	private void Pillage(){
		PILLAGE_ACTION chosenAct = TaskManager.Instance.pillageActions.PickRandomElementGivenWeights();
		switch (chosenAct) {
		case PILLAGE_ACTION.OBTAIN_ITEM:
			ObtainItem ();
			break;
		case PILLAGE_ACTION.END:
			_parentTask.EndTaskSuccess();
			break;
		case PILLAGE_ACTION.CIVILIAN_DIES:
			CivilianDies();
			break;
		case PILLAGE_ACTION.NOTHING:
		default:
			break;
		}
	}

	private void ObtainItem(){
		if(_targetLandmark.itemsInLandmark.Count > 0){
			Item chosenItem = _targetLandmark.itemsInLandmark [UnityEngine.Random.Range (0, _targetLandmark.itemsInLandmark.Count)];
			_targetLandmark.RemoveItemInLandmark (chosenItem);
			if(!_assignedCharacter.EquipItem(chosenItem)){
				_assignedCharacter.PickupItem (chosenItem);
			}
		}
	}
	private void CivilianDies(){
		//		int civilians = _target.civilians;
		if(_targetLandmark.civilians > 0){
			RACE[] races = _targetLandmark.civiliansByRace.Keys.Where(x => _targetLandmark.civiliansByRace[x] > 0).ToArray();
			RACE chosenRace = races [UnityEngine.Random.Range (0, races.Length)];
			_targetLandmark.AdjustCivilians (chosenRace, -1, _assignedCharacter);
			Log civilianDeathLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Pillage", "civilian_death");
			civilianDeathLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			civilianDeathLog.AddToFillers(null, Utilities.GetNormalizedSingularRace(chosenRace).ToLower(), LOG_IDENTIFIER.OTHER);
			_targetLandmark.AddHistory(civilianDeathLog);
			_assignedCharacter.AddHistory(civilianDeathLog);

		}
	}
}
