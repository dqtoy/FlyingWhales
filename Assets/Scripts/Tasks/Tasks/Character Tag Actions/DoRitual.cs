using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class DoRitual : CharacterTask {
	private BaseLandmark _ritualStones;

	public DoRitual(TaskCreator createdBy, int defaultDaysLeft = -1, Quest parentQuest = null) : base(createdBy, TASK_TYPE.DO_RITUAL, defaultDaysLeft, parentQuest) {
		SetStance(STANCE.STEALTHY);
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
			_targetLocation = GetTargetLandmark ();
		}
		_ritualStones = (BaseLandmark)_targetLocation;

		_assignedCharacter.GoToLocation (_ritualStones, PATHFINDING_MODE.USE_ROADS, () => StartRitual ());
	}

	public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		base.PerformTask();
		Ritual();
	}

	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.RITUAL_STONES){
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

	private void StartRitual() {
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartRitual ());
			return;
		}
		_assignedCharacter.DestroyAvatar ();
		_ritualStones.AddHistory(_assignedCharacter.name + " started to do a ritual!");
		_assignedCharacter.AddHistory ("Started to do a ritual!");
	}

	private void Ritual() {
		if(this.taskStatus != TASK_STATUS.IN_PROGRESS) {
			return;
		}
			
		if(_daysLeft == 0){
			End ();
			return;
		}
		ReduceDaysLeft (1);
	}

	private void DoMeteorStrike(){
		//Step 1 - Choose region
		List<Region> targetRegions = new List<Region> ();
		for (int i = 0; i < GridMap.Instance.allRegions.Count; i++) {
			Region currRegion = GridMap.Instance.allRegions [i];
			if(currRegion.centerOfMass.landmarkOnTile != null && currRegion.centerOfMass.landmarkOnTile.owner != null){
				targetRegions.Add (currRegion);
			}
		}
		if(targetRegions.Count <= 0){
			return;
		}
		Region chosenRegion = targetRegions[UnityEngine.Random.Range(0, targetRegions.Count)];

		//Step 2 - Destroy All Life in Region
		for (int i = 0; i < chosenRegion.allLandmarks.Count; i++) {
			BaseLandmark currLandmark = chosenRegion.allLandmarks [i];
			if(currLandmark.civilians > 0){
				currLandmark.KillAllCivilians ();
			}
			if(currLandmark.charactersAtLocation.Count > 0){
				while(currLandmark.charactersAtLocation.Count > 0) {
					if(currLandmark.charactersAtLocation[0] is Party){
						Party party = (Party)currLandmark.charactersAtLocation [0];
						while(party.partyMembers.Count > 0){
							party.partyMembers [0].Death ();
						}
					}else if(currLandmark.charactersAtLocation[0] is ECS.Character){
						ECS.Character character = (ECS.Character)currLandmark.charactersAtLocation [0];
						character.Death ();
					}
				}
			}
		}

		//Step 3 - Initialize Crater
		BaseLandmark landmark = chosenRegion.centerOfMass.landmarkOnTile;
		Settlement settlement = (Settlement)landmark;
		settlement.tileLocation.RuinStructureOnTile (false);
		settlement.ChangeLandmarkType (LANDMARK_TYPE.CRATER);

		_ritualStones.AddHistory ("A meteor crashed in " + landmark.landmarkName + " killing everything in the region!");
		landmark.AddHistory ("A meteor crashed!");
	}
	private void End(){
		//TODO: Change this to get a random ritual
//		if(_assignedCharacter.role != null && _assignedCharacter.role.roleType == CHARACTER_ROLE.VILLAIN){
//			DoMeteorStrike ();
//		}
		DoMeteorStrike ();

		EndTask(TASK_STATUS.SUCCESS);
	}

	private BaseLandmark GetTargetLandmark() {
		_landmarkWeights.Clear ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(_assignedCharacter, landmark)){
				_landmarkWeights.AddElement (landmark, 5);
			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}