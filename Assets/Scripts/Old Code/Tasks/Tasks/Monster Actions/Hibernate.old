using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class Hibernate : CharacterTask {

    private List<Character> _charactersToRest;
	private BaseLandmark _targetLandmark;

	#region getters/setters
	public List<Character> charactersToRest{
		get { return _charactersToRest; }
	}
	#endregion

	public Hibernate(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.HIBERNATE, stance, defaultDaysLeft) {
		_states = new Dictionary<STATE, State> {
			{ STATE.MOVE, new MoveState (this) },
			{ STATE.REST, new RestState (this) },
		};
    }

    #region overrides
    public override void OnChooseTask(ECS.Character character) {
        base.OnChooseTask(character);
		if(_assignedCharacter == null){
			return;
		}
        //Get the characters that will rest
        _charactersToRest = new List<ECS.Character>();
        if (character.party != null) {
            _charactersToRest.AddRange(character.party.partyMembers);
        } else {
            _charactersToRest.Add(character);
        }

		if(_targetLocation == null){
			_targetLandmark = GetLandmarkTarget (character);
			_targetLocation = _targetLandmark;
		}
		if(_targetLandmark != null){
			ChangeStateTo (STATE.MOVE);
			_assignedCharacter.GoToLocation (_targetLandmark, PATHFINDING_MODE.USE_ROADS, () => StartHibernation());
		}else{
			EndTask (TASK_STATUS.FAIL);
		}
    }
	public override bool CanBeDone (Character character, ILocation location){
		if(location.tileLocation.landmarkOnTile != null){
			return true;
		}
		return base.CanBeDone (character, location);
	}
	public override bool AreConditionsMet (Character character){
		return true;
	}
    //public override void PerformDailyAction() {
    //    if (_canDoDailyAction) {
    //        base.PerformDailyAction();
    //        restAction.DoAction(_assignedCharacter);
    //    }
    //}
    //public override void EndTask(TASK_STATUS taskResult) {
    //    SetCanDoDailyAction(false);
    //    base.EndTask(taskResult);
    //}

	protected override BaseLandmark GetLandmarkTarget (Character character){
		base.GetLandmarkTarget (character);
		int weight = 0;
		for (int i = 0; i < character.currentRegion.landmarks.Count; i++) {
			BaseLandmark landmark = character.currentRegion.landmarks [i];
			weight = 300;
			if(!landmark.HasHostileCharactersWith(character)){
				weight += 50;
			}
			if(landmark.charactersAtLocation.Count <= 0){
				weight += 100;
			}
			if(landmark.id == character.home.id){
				weight += 1000;
			}
			_landmarkWeights.AddElement (landmark, weight);
		}
		for (int i = 0; i < character.currentRegion.adjacentRegionsViaRoad.Count; i++) {
			Region adjacentRegion = character.currentRegion.adjacentRegionsViaRoad [i];
			for (int j = 0; j < adjacentRegion.landmarks.Count; j++) {
				BaseLandmark landmark = character.currentRegion.landmarks [i];
				weight = 50;
				if(!landmark.HasHostileCharactersWith(character)){
					weight += 50;
				}
				if(landmark.charactersAtLocation.Count <= 0){
					weight += 100;
				}
				if(landmark.id == character.home.id){
					weight += 1000;
				}
				_landmarkWeights.AddElement (landmark, weight);
			}
		}
        LogTargetWeights(_landmarkWeights);
        if (_landmarkWeights.Count > 0){
			return _landmarkWeights.PickRandomElementGivenWeights ();
		}
		return null;
	}
    #endregion
	private void StartHibernation(){
//		if(_assignedCharacter.isInCombat){
//			_assignedCharacter.SetCurrentFunction (() => StartHibernation ());
//			return;
//		}
        Log startLog = new Log(GameManager.Instance.Today(), "CharacterTasks", "Hibernate", "start");
        startLog.AddToFillers(_assignedCharacter, _assignedCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        _assignedCharacter.AddHistory(startLog);
		_targetLandmark.AddHistory(startLog);

		//_assignedCharacter.SetHome (_targetLandmark);
		ChangeStateTo (STATE.REST);
	}
    //private void GoToTargetLocation() {
    //    // The monster will move towards its Lair and then rest there indefinitely
    //    GoToLocation goToLocation = new GoToLocation(this); //Make character go to chosen settlement
    //    goToLocation.InitializeAction(_assignedCharacter.lair);
    //    goToLocation.SetPathfindingMode(PATHFINDING_MODE.NORMAL);
    //    goToLocation.onTaskActionDone += StartHibernation;
    //    goToLocation.onTaskDoAction += goToLocation.Generic;

    //    goToLocation.DoAction(_assignedCharacter);
    //}

    //private void StartHibernation() {
    //    //SetCanDoDailyAction(true);
    //    restAction = new RestAction(this);
    //    //restAction.onTaskActionDone += TaskSuccess; //rest indefinitely
    //    restAction.onTaskDoAction += restAction.RestIndefinitely;
    //    restAction.DoAction(_assignedCharacter);
    //}

}
