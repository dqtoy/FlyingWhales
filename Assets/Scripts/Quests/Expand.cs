using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Expand : Quest {

	private HexTile _targetUnoccupiedTile;
	private int _civilians;

	#region getters/setters
	public HexTile targetUnoccupiedTile {
		get { return _targetUnoccupiedTile; }
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
		_isAccepted = true;
		//ECS.Character that accepts this quest must now create a party
		Party newParty = new Party(partyLeader, _maxPartyMembers);
		newParty.onPartyFull += OnPartyFull;
		_assignedParty = newParty;

		partyLeader.SetCurrentQuest(this);

		Collect collect = new Collect (this);
		collect.InititalizeAction (20);
		collect.onQuestDoAction += collect.Expand;
		collect.DoAction (partyLeader);
		collect.ActionDone (QUEST_ACTION_RESULT.SUCCESS);

		if(onQuestAccepted != null) {
			onQuestAccepted();
		}
	}
	protected override void ConstructQuestLine() {
		base.ConstructQuestLine();

		GoToLocation goToExpandLocationAction = new GoToLocation(this); //Go to the picked region
		goToExpandLocationAction.InititalizeAction(_targetUnoccupiedTile);
		goToExpandLocationAction.onQuestActionDone += SuccessExpansion;

//		//Enqueue all actions
//		_questLine.Enqueue(goToRegionAction); 
//		_questLine.Enqueue(roamRegionAction);
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
		if(character.currLocation.landmarkOnTile != null && character.currLocation.landmarkOnTile.owner != null && character.currLocation.landmarkOnTile.owner.id == character.faction.id && character.currLocation.landmarkOnTile.civilians > 20){
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
				List<HexTile> path = PathGenerator.Instance.GetPath (character.currLocation, this._targetUnoccupiedTile, PATHFINDING_MODE.MAJOR_ROADS);
				if(path != null){
					return true;
				}
			}
		}
		return false;
	}

	protected override void QuestFail() {
		_isDone = true;
		_questResult = QUEST_RESULT.FAIL;
		_createdBy.RemoveQuest(this);
		_currentAction.onQuestActionDone = null;
		_currentAction.ActionDone(QUEST_ACTION_RESULT.FAIL);

		for (int i = 0; i < this._assignedParty.partyMembers.Count; i++) {
			ECS.Character character = this._assignedParty.partyMembers [i];
			GoHome goHome = new GoHome (character, -1, 1);
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
		EndQuest (QUEST_RESULT.SUCCESS);
	}
}
