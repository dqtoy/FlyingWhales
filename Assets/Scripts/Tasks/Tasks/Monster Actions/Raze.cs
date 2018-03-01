using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Raze : CharacterTask {

//	private List<Character> _razingCharacters;

	private WeightedDictionary<string> razeResult;
	private BaseLandmark _target;

	public Raze(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.RAZE, defaultDaysLeft) {
		SetStance (STANCE.COMBAT);
		razeResult = new WeightedDictionary<string> ();
//		_razingCharacters = new List<Character> ();
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
//		_razingCharacters.Clear ();
//		if(character.party == null){
//			_razingCharacters.Add (character);
//		}else{
//			_razingCharacters.AddRange (character.party.partyMembers);
//		}
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark ();
		}
		_target = (BaseLandmark)_targetLocation;
		_assignedCharacter.GoToLocation (_target, PATHFINDING_MODE.USE_ROADS, () => StartRaze());
	}
	public override void PerformTask() {
		base.PerformTask();
		if(_daysLeft == 0){
			EndRaze ();
			return;
		}
		ReduceDaysLeft(1);
	}
	#endregion

	private void StartRaze(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartRaze ());
			return;
		}
		_target.AddHistory(_assignedCharacter.name + " has started razing " + _target.landmarkName + "!");
	}
	private void EndRaze(){
		int successWeight = 0;
		int failWeight = 0;

		successWeight += _assignedCharacter.strength;
		successWeight += (_assignedCharacter.intelligence * 2);

		failWeight += (_target.currDurability * 4);

		razeResult.ChangeElement ("success", successWeight);
		razeResult.ChangeElement ("fail", failWeight);

		string result = razeResult.PickRandomElementGivenWeights ();
		if(result == "success"){
			_target.KillAllCivilians ();
			_target.location.RuinStructureOnTile (false);
			_target.AddHistory("All civilians were killed by " + _assignedCharacter.name + "!");
			_assignedCharacter.AddHistory ("Razed " + _target.landmarkName + "!");
			//TODO: When structure in landmarks is destroyed, shall all characters in there die?
		}else{
			//TODO: Fail
			_assignedCharacter.AddHistory ("Failed to raze " + _target.landmarkName + "!");
		}
		EndTask (TASK_STATUS.SUCCESS);
	}

	private BaseLandmark GetTargetLandmark() {
		WeightedDictionary<BaseLandmark> landmarkWeights = new WeightedDictionary<BaseLandmark> ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(landmark.owner != null && landmark.civilians > 0){
				if(_assignedCharacter.faction == null){
					landmarkWeights.AddElement (landmark, 100);
				}else{
					if(_assignedCharacter.faction.id != landmark.owner.id){
						landmarkWeights.AddElement (landmark, 100);
					}
				}
			}
		}
		if(landmarkWeights.GetTotalOfWeights() > 0){
			return landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
