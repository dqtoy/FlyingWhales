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
        _alignments.Add(ACTION_ALIGNMENT.HOSTILE);
        _alignments.Add(ACTION_ALIGNMENT.UNLAWFUL);
		razeResult = new WeightedDictionary<string> ();
//		_razingCharacters = new List<Character> ();
	}

	#region overrides
	public override void OnChooseTask(ECS.Character character) {
		base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
//		_razingCharacters.Clear ();
//		if(character.party == null){
//			_razingCharacters.Add (character);
//		}else{
//			_razingCharacters.AddRange (character.party.partyMembers);
//		}
		if(_targetLocation == null){
            WeightedDictionary<BaseLandmark> landmarkWeights = GetLandmarkTargetWeights(character);
            if (landmarkWeights.GetTotalOfWeights() > 0) {
                _targetLocation = landmarkWeights.PickRandomElementGivenWeights();
            } else {
                EndTask(TASK_STATUS.FAIL);
                return;
            }
			
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
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null && location.tileLocation.landmarkOnTile.owner != null && location.tileLocation.landmarkOnTile.civilians > 0){
			if(character.faction == null){
				return true;
			}else{
                if (location.HasHostilitiesWith(character.faction)) {
                    return true;
				}
			}
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
    public override int GetSelectionWeight(Character character) {
        return 20;
    }
    protected override WeightedDictionary<BaseLandmark> GetLandmarkTargetWeights(Character character) {
        WeightedDictionary<BaseLandmark> landmarkWeights = base.GetLandmarkTargetWeights(character);
        Region regionOfChar = character.specificLocation.tileLocation.region;
        for (int i = 0; i < regionOfChar.allLandmarks.Count; i++) {
            BaseLandmark currLandmark = regionOfChar.allLandmarks[i];
            int weight = 0;
            if (currLandmark.HasHostilitiesWith(character.faction)) {
                weight += 100; //Landmark is owned by a hostile faction: 100
            }
            if (currLandmark.civilians > 0) {
                weight += 30; //Landmark has civilians: +30
            }
            if (weight > 0) {
                landmarkWeights.AddElement(currLandmark, weight);
            }
        }
        return landmarkWeights;
    }
    #endregion

    private void StartRaze(){
		if(_assignedCharacter.isInCombat){
			_assignedCharacter.SetCurrentFunction (() => StartRaze ());
			return;
		}
		_target.AddHistory(_assignedCharacter.name + " has started razing " + _target.landmarkName + "!");
		_target.AddHistory("Started razing " + _target.landmarkName + "!");
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
			_target.AddHistory(_assignedCharacter.name + " razed " + _target.landmarkName + "! All civilians were killed!");
			_assignedCharacter.AddHistory ("Razed " + _target.landmarkName + "! All civilians were killed!");
			//TODO: When structure in landmarks is destroyed, shall all characters in there die?
		}else{
			//TODO: Fail
			_assignedCharacter.AddHistory ("Failed to raze " + _target.landmarkName + "!");
			_target.AddHistory(_assignedCharacter.name + " failed to raze " + _target.landmarkName + "!");
		}
		EndTask (TASK_STATUS.SUCCESS);
	}

	private BaseLandmark GetTargetLandmark() {
		_landmarkWeights.Clear ();
		for (int i = 0; i < _assignedCharacter.specificLocation.tileLocation.region.allLandmarks.Count; i++) {
			BaseLandmark landmark = _assignedCharacter.specificLocation.tileLocation.region.allLandmarks [i];
			if(CanBeDone(_assignedCharacter, landmark)){
				_landmarkWeights.AddElement (landmark, 100);
//				if(_assignedCharacter.faction == null){
//					_landmarkWeights.AddElement (landmark, 100);
//				}else{
//					if(_assignedCharacter.faction.id != landmark.owner.id){
//						_landmarkWeights.AddElement (landmark, 100);
//					}
//				}
			}
		}
		if(_landmarkWeights.GetTotalOfWeights() > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
}
