using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Expand : Quest {

	private HexTile _targetUnoccupiedTile;
	private HexTile _originTile;

	private int _civilians;

	#region getters/setters
	public HexTile targetUnoccupiedTile {
		get { return _targetUnoccupiedTile; }
	}
	public HexTile originTile {
		get { return _originTile; }
	}
	public int civilians{
		get { return _civilians; }
	}
	#endregion

	public Expand(QuestCreator createdBy, int daysBeforeDeadline, HexTile targetUnoccupiedTile, HexTile originTile = null) 
		: base(createdBy, daysBeforeDeadline, QUEST_TYPE.EXPAND) {
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction(new List<Faction>(){((InternalQuestManager)createdBy).owner}),
//			new MustBeRole(CHARACTER_ROLE.COLONIST),
		};
		_targetUnoccupiedTile = targetUnoccupiedTile;
		_originTile = originTile;
	}

	#region overrides
	public override void AcceptQuest(ECS.Character partyLeader) {
		base.AcceptQuest (partyLeader);
	}
	protected override void ConstructQuestLine() {
		base.ConstructQuestLine();
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
        base.ResetQuestValues();
		_civilians = 0;
	}
	public override bool CanAcceptQuest (ECS.Character character){
		bool canAccept = base.CanAcceptQuest (character);
		if(!canAccept){
			return false;
		}
		if(_originTile != null){
			if((int)_originTile.landmarkOnTile.civilians > 20 && character.currLocation.id == _originTile.id){
				List<HexTile> path = PathGenerator.Instance.GetPath (_originTile, this._targetUnoccupiedTile, PATHFINDING_MODE.MAJOR_ROADS);
				if(path != null){
					return true;
				}
			}else{
				return false;
			}
		}else{
			List<HexTile> path = PathGenerator.Instance.GetPath (character.currLocation, this._targetUnoccupiedTile, PATHFINDING_MODE.MAJOR_ROADS);
			if(path != null){
				return true;
			}
		}
		return false;
	}
	internal override void QuestSuccess() {
		RetaskParty (_assignedParty.JustDisbandParty);
	}

	internal override void QuestFail() {
        AddNewLog("The expansion failed!");
        RetaskParty (_assignedParty.JustDisbandParty);
	}
	#endregion

	internal void SetCivilians(int amount){
		_civilians = amount;
	}

	private void SuccessExpansion(){
		LandmarkManager.Instance.OccupyLandmark (this._targetUnoccupiedTile, this._assignedParty.partyLeader.faction);
		this._targetUnoccupiedTile.landmarkOnTile.AdjustPopulation (_civilians);
		CameraMove.Instance.UpdateMinimapTexture ();
		Settlement expandedTo = (Settlement)this._targetUnoccupiedTile.landmarkOnTile;
		ECS.Character villageHead = expandedTo.CreateNewCharacter(CHARACTER_ROLE.VILLAGE_HEAD, "Swordsman");
		villageHead.SetHome (expandedTo);
		expandedTo.SetHead(villageHead);
        AddNewLog("The expansion was successful " + villageHead.name + " is set as the head of the new settlement");
        EndQuest (QUEST_RESULT.SUCCESS);
	}
}
