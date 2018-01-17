using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Expand : Quest {

	private HexTile _targetUnoccupiedTile;
	private HexTile _targetFactionSettlementTile;
	private int _civilians;

	#region getters/setters
	public HexTile targetUnoccupiedTile {
		get { return _targetUnoccupiedTile; }
	}
	public HexTile targetFactionSettlementTile {
		get { return _targetFactionSettlementTile; }
	}
	public int civilians{
		get { return _civilians; }
	}
	#endregion

	public Expand(QuestCreator createdBy, int daysBeforeDeadline, HexTile targetUnoccupiedTile) 
		: base(createdBy, daysBeforeDeadline, QUEST_TYPE.EXPAND) {
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction(new List<Faction>(){((InternalQuestManager)createdBy).owner}),
			new MustBeRole(CHARACTER_ROLE.COLONIST),
		};
		_targetUnoccupiedTile = targetUnoccupiedTile;
	}

	#region overrides
	public override void AcceptQuest(ECS.Character partyLeader) {
		base.AcceptQuest (partyLeader);
	}
	protected override void ConstructQuestLine() {
		base.ConstructQuestLine();
		if (_assignedParty.partyLeader.currLocation.id != this._targetFactionSettlementTile.id) {
			GoToLocation goToSettlementLocationAction = new GoToLocation(this); //Go to the picked region
			goToSettlementLocationAction.InititalizeAction(this._targetFactionSettlementTile);
			goToSettlementLocationAction.onQuestDoAction += goToSettlementLocationAction.Expand;
			goToSettlementLocationAction.onQuestActionDone += this.PerformNextQuestAction;
			_questLine.Enqueue(goToSettlementLocationAction);
		}
		Collect collect = new Collect(this);
		collect.InititalizeAction(20);
		collect.onQuestActionDone += this.PerformNextQuestAction;
		collect.onQuestDoAction += collect.Expand;

		GoToLocation goToExpandLocationAction = new GoToLocation(this); //Go to the picked region
		goToExpandLocationAction.InititalizeAction(_targetUnoccupiedTile);
		goToExpandLocationAction.onQuestDoAction += goToExpandLocationAction.Expand;
		goToExpandLocationAction.onQuestActionDone += SuccessExpansion;



//		//Enqueue all actions
		_questLine.Enqueue(collect);
		_questLine.Enqueue(goToExpandLocationAction);
	}
	internal override void EndQuest(QUEST_RESULT result) {
		base.EndQuest(result);
	}
	protected override void ResetQuestValues(){
		_civilians = 0;
	}
	public override bool CanAcceptQuest (ECS.Character character){
		bool canAccept = base.CanAcceptQuest (character);
		if(!canAccept){
			return false;
		}
		this._targetFactionSettlementTile = character.faction.GetSettlementWithHighestPopulation ().location;
		if(character.currLocation.id != this._targetFactionSettlementTile.id){
			List<HexTile> path = PathGenerator.Instance.GetPath (character.currLocation, this._targetFactionSettlementTile, PATHFINDING_MODE.MAJOR_ROADS);
			if(path == null){
				return false;
			}
		}
		bool isAdjacentToTribe = false;
		for (int j = 0; j < this._targetUnoccupiedTile.region.connections.Count; j++) {
			if(this._targetUnoccupiedTile.region.connections[j] is Region){
				Region region = (Region)this._targetUnoccupiedTile.region.connections [j];
				if(region.centerOfMass.landmarkOnTile.owner != null && region.centerOfMass.landmarkOnTile.owner.id == character.faction.id){
					isAdjacentToTribe = true;
					break;
				}
			}
		}
		if(isAdjacentToTribe){
			List<HexTile> path = PathGenerator.Instance.GetPath (this._targetFactionSettlementTile, this._targetUnoccupiedTile, PATHFINDING_MODE.MAJOR_ROADS);
			if(path != null){
				return true;
			}
		}
		return false;
	}
	internal override void QuestSuccess() {
		_isDone = true;
		_questResult = QUEST_RESULT.SUCCESS;
		_createdBy.RemoveQuest(this);
		ECS.Character partyLeader = this._assignedParty.partyLeader;
		partyLeader.ChangeRole (CHARACTER_ROLE.VILLAGE_HEAD);
		partyLeader.SetHome ((Settlement)this._targetUnoccupiedTile.landmarkOnTile);
		this._assignedParty.DisbandParty ();
		this._assignedParty.partyLeader.DestroyAvatar ();
	}

	internal override void QuestFail() {
		_isDone = true;
		_questResult = QUEST_RESULT.FAIL;
		_createdBy.RemoveQuest(this);
//		if(_currentAction != null){
//			_currentAction.onQuestActionDone = null;
//			_currentAction.ActionDone(QUEST_ACTION_RESULT.FAIL);
//		}
		while(_questLine.Count > 0){
			QuestAction questAction = _questLine.Dequeue ();
			questAction.onQuestActionDone = null;
			questAction.ActionDone(QUEST_ACTION_RESULT.FAIL);
		}

		for (int i = 0; i < this._assignedParty.partyMembers.Count; i++) {
			ECS.Character character = this._assignedParty.partyMembers [i];
			GoHome goHome = new GoHome (character, -1);
			goHome.AcceptQuest (character);
		}
		this._assignedParty.DisbandParty ();
	}
	#endregion

	internal void SetCivilians(int amount){
		_civilians = amount;
	}

	private void SuccessExpansion(){
		LandmarkManager.Instance.OccupyLandmark (this._targetUnoccupiedTile, this._assignedParty.partyLeader.faction);
		this._targetUnoccupiedTile.landmarkOnTile.AdjustPopulation (_civilians);
		CameraMove.Instance.UpdateMinimapTexture ();
		EndQuest (QUEST_RESULT.SUCCESS);
	}
}
