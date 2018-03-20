using UnityEngine;
using System.Collections;

public class RazeState : State {
	private WeightedDictionary<string> razeResult;

	public RazeState(CharacterTask parentTask): base (parentTask, STATE.RAZE){
		razeResult = new WeightedDictionary<string> ();
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Raze ();
		return true;
	}
	#endregion

	private void Raze(){
		int successWeight = 0;
		int failWeight = 0;

		successWeight += _assignedCharacter.strength;
		successWeight += (_assignedCharacter.intelligence * 2);

		failWeight += (_targetLandmark.currDurability * 4);

		razeResult.ChangeElement ("success", successWeight);
		razeResult.ChangeElement ("fail", failWeight);

		string result = razeResult.PickRandomElementGivenWeights ();
		if(result == "success"){
			_targetLandmark.KillAllCivilians ();
			_targetLandmark.location.RuinStructureOnTile (false);
			Log successLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Raze", "success");
			successLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			successLog.AddToFillers(_targetLandmark, _targetLandmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);

			_targetLandmark.AddHistory(successLog);
			_assignedCharacter.AddHistory(successLog);
			//TODO: When structure in landmarks is destroyed, shall all characters in there die?
		} else{
			//TODO: Fail
			Log failLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Raze", "fail");
			failLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			failLog.AddToFillers(_targetLandmark, _targetLandmark.landmarkName, LOG_IDENTIFIER.LANDMARK_1);
			_assignedCharacter.AddHistory(failLog);
			_targetLandmark.AddHistory(failLog);
		}
	}
}
