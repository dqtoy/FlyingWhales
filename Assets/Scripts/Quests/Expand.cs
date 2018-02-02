using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Expand : Quest {

	private HexTile _targetUnoccupiedTile;
	private HexTile _originTile;


	#region getters/setters
	public HexTile targetUnoccupiedTile {
		get { return _targetUnoccupiedTile; }
	}
	public HexTile originTile {
		get { return _originTile; }
	}
	#endregion

	public Expand(TaskCreator createdBy, HexTile targetUnoccupiedTile, HexTile originTile) : base(createdBy, QUEST_TYPE.EXPAND) {
		_questFilters = new List<QuestFilter>() {
			new MustBeFaction(new List<Faction>(){((InternalQuestManager)createdBy).owner}),
//			new MustBeRole(CHARACTER_ROLE.COLONIST),
		};
		_targetUnoccupiedTile = targetUnoccupiedTile;
		_originTile = originTile;
		_originTile.landmarkOnTile.AdjustReservedPopulation (20);
		_originTile.landmarkOnTile.AdjustPopulation (-20);
	}

	#region overrides
	protected override void AcceptQuest(ECS.Character partyLeader) {
		base.AcceptQuest (partyLeader);
	}
	protected override void ConstructQuestLine() {
		base.ConstructQuestLine();
		Collect collect = new Collect(this);
		collect.InititalizeAction(20);
		collect.onTaskActionDone += this.PerformNextQuestAction;
		collect.onTaskDoAction += collect.Expand;

		GoToLocation goToExpandLocationAction = new GoToLocation(this); //Go to the picked region
		goToExpandLocationAction.InititalizeAction(_targetUnoccupiedTile);
		goToExpandLocationAction.onTaskDoAction += goToExpandLocationAction.Expand;
		goToExpandLocationAction.onTaskActionDone += SuccessExpansion;

//		//Enqueue all actions
		_questLine.Enqueue(collect);
		_questLine.Enqueue(goToExpandLocationAction);
	}

	public override bool CanAcceptQuest (ECS.Character character){
		bool canAccept = base.CanAcceptQuest (character);
		if(!canAccept){
			return false;
		}
		if(_originTile != null){
			if(_originTile.landmarkOnTile.civilians > 20 && character.currLocation.id == _originTile.id){
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
	//protected override void QuestSuccess() {
	//	RetaskParty(_assignedParty.JustDisbandParty);
 //       GiveRewards();
	//}
	//protected override void QuestFail() {
 //       AddNewLog("The expansion failed!");
 //       RetaskParty(_assignedParty.JustDisbandParty);
	//}
	#endregion

	private void SuccessExpansion(){
		LandmarkManager.Instance.OccupyLandmark (this._targetUnoccupiedTile, this._assignedParty.partyLeader.faction);
		this._targetUnoccupiedTile.landmarkOnTile.AdjustPopulation (_assignedParty.civilians);
		_assignedParty.SetCivilians (0);
		CameraMove.Instance.UpdateMinimapTexture ();
		Settlement expandedTo = (Settlement)this._targetUnoccupiedTile.landmarkOnTile;
		ECS.Character villageHead = expandedTo.CreateNewCharacter(CHARACTER_ROLE.VILLAGE_HEAD, "Swordsman");
		villageHead.SetHome (expandedTo);
		expandedTo.SetHead(villageHead);
        AddNewLog("The expansion was successful " + villageHead.name + " is set as the head of the new settlement");
        GoBackToQuestGiver(TASK_STATUS.SUCCESS);
        //EndQuest (TASK_RESULT.SUCCESS);
	}
}
