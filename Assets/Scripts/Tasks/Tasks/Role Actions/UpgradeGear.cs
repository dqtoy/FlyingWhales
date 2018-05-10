using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class UpgradeGear : CharacterTask {

    //private Settlement _settlement;

	public UpgradeGear(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.UPGRADE_GEAR, stance, defaultDaysLeft) {
		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.PURCHASE, new PurchaseState (this, "Equipment") }
		};
    }
    #region overrides
	public override void OnChooseTask (ECS.Character character){
		base.OnChooseTask (character);
		if(_assignedCharacter == null){
			return;
		}
		if(_targetLocation == null){
            _targetLocation = GetLandmarkTarget(character);
		}
		if(_targetLocation != null){
			ChangeStateTo (STATE.MOVE);
			//_settlement = (Settlement)_targetLocation;
			_assignedCharacter.GoToLocation (_targetLocation, PATHFINDING_MODE.USE_ROADS, () => StartPurchase());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
	}
    public override void PerformTask() {
		if(!CanPerformTask()){
			return;
		}
		if(_currentState != null){
			_currentState.PerformStateAction ();
		}
		EndTaskSuccess ();
    }
    public override bool CanBeDone(ECS.Character character, ILocation location) {
        if (location.tileLocation.landmarkOnTile != null && character.faction != null && location.tileLocation.landmarkOnTile is Settlement) {
            Settlement settlement = location.tileLocation.landmarkOnTile as Settlement;
            if (settlement.owner != null && settlement.owner.id == character.faction.id) {
                if (settlement.owner != null && !settlement.HasHostileCharactersWith(character)) {
                    return true;
                }
            }
        }
        return base.CanBeDone(character, location);
    }
	public override bool AreConditionsMet (ECS.Character character){
        //if(character.faction != null && character.faction.settlements.Count > 0){
        //	return true;
        //}
        List<Region> regionsToCheck = new List<Region>();
        regionsToCheck.Add(character.currentRegion);
        regionsToCheck.AddRange(character.currentRegion.adjacentRegionsViaRoad);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            //Non Hostile Settlement in current region and adjacent regions: 100
            if (currRegion.mainLandmark.HasHostileCharactersWith(character)) {
                return true;
            }
        }
        return base.AreConditionsMet (character);
	}
    public override int GetSelectionWeight(Character character) {
        return 20;
    }
    protected override BaseLandmark GetLandmarkTarget(Character character) {
        base.GetLandmarkTarget(character);
        List<Region> regionsToCheck = new List<Region>();
        regionsToCheck.Add(character.currentRegion);
        regionsToCheck.AddRange(character.currentRegion.adjacentRegionsViaRoad);
        for (int i = 0; i < regionsToCheck.Count; i++) {
            Region currRegion = regionsToCheck[i];
            //Non Hostile Settlement in current region and adjacent regions: 100
            if (currRegion.mainLandmark.HasHostileCharactersWith(character)) {
                _landmarkWeights.AddElement(currRegion.mainLandmark, 100);
            }
        }
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.GetTotalOfWeights() > 0) {
            return _landmarkWeights.PickRandomElementGivenWeights();
        }
        return null;
    }
    //public override void TaskSuccess() {
    //    if (_assignedCharacter.faction == null) {
    //        _assignedCharacter.UnalignedDetermineAction();
    //    } else {
    //        _assignedCharacter.DetermineAction();
    //    }
    //}
    #endregion

    private void StartPurchase(){
		ChangeStateTo (STATE.PURCHASE);
	}
}
