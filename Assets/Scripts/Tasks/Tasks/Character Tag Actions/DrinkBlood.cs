using UnityEngine;
using System.Collections;
using System.Linq;
using ECS;

public class DrinkBlood : CharacterTask {

	private BaseLandmark _target;

	public DrinkBlood(TaskCreator createdBy, int defaultDaysLeft = -1) 
		: base(createdBy, TASK_TYPE.DRINK_BLOOD, defaultDaysLeft) {
		SetStance (STANCE.STEALTHY);
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark ();
		}
		_target = (BaseLandmark)_targetLocation;

		_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartDrinkingBlood ());
	}

	public override void PerformTask() {
		base.PerformTask();
		Drink();
	}
	public override void TaskCancel() {
		base.TaskCancel();
		_assignedCharacter.DestroyAvatar();
	}
	public override void TaskFail() {
		base.TaskFail();
		_assignedCharacter.DestroyAvatar();
	}
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.owner != null && location.tileLocation.landmarkOnTile.civilians > 0){
			return true;
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		for (int i = 0; i < character.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = character.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(character, landmark)){
				return true;
			}
		}
		return base.AreConditionsMet (character);
	}
	#endregion

	private void StartDrinkingBlood() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartDrinkingBlood ());
			return;
		}

		_target.AddHistory(_assignedCharacter.name + " is hunting civilians for blood!");
		_assignedCharacter.AddHistory ("Hunting civilians for blood!");
	}

	private void Drink() {
		if(this.taskStatus != TASK_STATUS.IN_PROGRESS) {
			return;
		}
		string chosenAct = TaskManager.Instance.drinkBloodActions.PickRandomElementGivenWeights();
		switch (chosenAct) {
		case "drink":
			KillCivilianAndDrinkBlood ();
			End ();
			return;
		case "caught":
			Caught ();
			End ();
			return;
		case "nothing":
		default:
			break;
		}
		if(_daysLeft == 0){
			End ();
			return;
		}
		ReduceDaysLeft (1);
	}

	private void KillCivilianAndDrinkBlood() {
		if(_target.civilians > 0) {
			RACE[] races = _target.civiliansByRace.Keys.Where(x => _target.civiliansByRace[x] > 0).ToArray();
			RACE chosenRace = races [UnityEngine.Random.Range (0, races.Length)];
			_target.AdjustCivilians (chosenRace, -1);
			_target.AddHistory (_assignedCharacter.name + " hunted, killed, and drank the blood of a/an " + Utilities.GetNormalizedSingularRace(chosenRace).ToLower() + " civilian!");
			_assignedCharacter.AddHistory ("Hunted, killed, and drank the blood of a/an " + Utilities.GetNormalizedSingularRace(chosenRace).ToLower() + " civilian!");

			//          _target.ReduceCivilians(1);
			//GameDate nextDate = GameManager.Instance.Today();
			//nextDate.AddDays(1);
			//SchedulingManager.Instance.AddEntry(nextDate, () => Hunt());
		}
	}
	private void Caught(){
		_assignedCharacter.AddHistory ("Caught trying to kill and drink blood of a civilian!");
		_target.AddHistory (_assignedCharacter.name + " caught trying to kill and drink blood of a civilian!");
		if(!_assignedCharacter.HasTag(CHARACTER_TAG.CRIMINAL)){
			_assignedCharacter.AssignTag (CHARACTER_TAG.CRIMINAL);
		}
	}
	private void End(){
		//Messenger.RemoveListener("OnDayEnd", Hunt);
		//SetCanDoDailyAction(false);
		//        if (_target.location.region.centerOfMass.landmarkOnTile.isOccupied){
		//			Settlement settlement = (Settlement)_target.location.region.centerOfMass.landmarkOnTile;
		//			settlement.CancelSaveALandmark (_target);
		//		}
		EndTask(TASK_STATUS.SUCCESS);
	}

	private BaseLandmark GetTargetLandmark() {
		_landmarkWeights.Clear ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(_assignedCharacter, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
