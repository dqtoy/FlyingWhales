/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterRole {
	protected ECS.Character _character;
    protected CHARACTER_ROLE _roleType;
    protected List<ROAD_TYPE> _allowedRoadTypes; //states what roads this role can use.
    protected bool _canPassHiddenRoads; //can the character use roads that haven't been discovered yet?
    protected List<QUEST_TYPE> _allowedQuestTypes;
	protected List<CharacterTask> _roleTasks;
	protected CharacterTask _defaultRoleTask;

    #region getters/setters
    public CHARACTER_ROLE roleType {
        get { return _roleType; }
    }
	public ECS.Character character{
		get { return _character; }
	}
    public List<QUEST_TYPE> allowedQuestTypes {
        get { return _allowedQuestTypes; }
    }
	public List<CharacterTask> roleTasks {
		get { return _roleTasks; }
	}
	public CharacterTask defaultRoleTask {
		get { return _defaultRoleTask; }
	}
    #endregion

	public CharacterRole(ECS.Character character){
		_character = character;
        _allowedQuestTypes = new List<QUEST_TYPE>();
		_roleTasks = new List<CharacterTask> ();
		_roleTasks.Add (new RecruitFollowers (this._character, 5));
	}
		
    #region Action Weights
    public virtual void AddTaskWeightsFromRole(WeightedDictionary<CharacterTask> tasks) {
		for (int i = 0; i < _roleTasks.Count; i++) {
			CharacterTask currTask = _roleTasks[i];
			if(currTask.forPlayerOnly || !currTask.AreConditionsMet(_character)){
				continue;
			}
			tasks.AddElement (currTask, currTask.GetTaskWeight(_character));
		}
    }
    /*
         Get the weighted dictionary for what action the character will do next.
             */
//    internal virtual WeightedDictionary<CharacterTask> GetActionWeights() {
//        WeightedDictionary<CharacterTask> actionWeights = new WeightedDictionary<CharacterTask>();
//        if (_canAcceptQuests) {
//            if (_character.currLocation.landmarkOnTile != null && _character.currLocation.landmarkOnTile is Settlement) {
//                TakeQuest takeQuestTask = new TakeQuest(_character);
//                actionWeights.AddElement(takeQuestTask, GetWeightForTask(takeQuestTask));
//            }
//        }
//        
//        Rest restTask = new Rest(_character);
//        actionWeights.AddElement(restTask, GetWeightForTask(restTask));
//
//        UpgradeGear upgradeGearTask = new UpgradeGear(_character);
//        actionWeights.AddElement(upgradeGearTask, GetWeightForTask(upgradeGearTask));
//
//        GoHome goHomeTask = new GoHome(_character);
//        actionWeights.AddElement(goHomeTask, GetWeightForTask(goHomeTask));
//
//        DoNothing doNothingTask = new DoNothing(_character);
//        actionWeights.AddElement(doNothingTask, GetWeightForTask(doNothingTask));
//
//		int dropPrisonersWeight = GetDropPrisonersWeight ();
//		if(dropPrisonersWeight > 0){
//			DropPrisoners dropPrisonersTask = new DropPrisoners(_character);
//			actionWeights.AddElement(dropPrisonersTask, dropPrisonersWeight);
//		}
//
//        return actionWeights;
//    }
//    internal WeightedDictionary<CharacterTask> GetQuestWeights() {
//        WeightedDictionary<CharacterTask> questWeights = new WeightedDictionary<CharacterTask>();
//        if (_character.currLocation.landmarkOnTile != null && _character.currLocation.landmarkOnTile is Settlement) {
//            Settlement currSettlement = (Settlement)_character.currLocation.landmarkOnTile;
//            bool canAccepQueststHere = true;
//            if (_character.faction.id != currSettlement.owner.id) {
//                //character is not of the same faction as the settlement
//                FactionRelationship relWithFaction = FactionManager.Instance.GetRelationshipBetween(_character.faction, currSettlement.owner);
//                if (relWithFaction.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
//                    canAccepQueststHere = false; //cannot accept quests at this settlement
//                }
//            }
//
//            if (canAccepQueststHere) {
//                for (int i = 0; i < currSettlement.questBoard.Count; i++) {
//                    OldQuest.Quest currQuest = currSettlement.questBoard[i];
//                    if (this.CanAcceptQuest(currQuest) && currQuest.CanAcceptQuest(_character)) { //Check both the quest filters and the quest types this role can accept
//                        questWeights.AddElement(currQuest, GetWeightForTask(currQuest));
//                    }
//                }
//            }
//        }
//        return questWeights;
//    }
//    internal int GetWeightForTask(CharacterTask task) {
//        int weight = 0;
//        if(task.taskType == TASK_TYPE.QUEST) {
//            OldQuest.Quest quest = (OldQuest.Quest)task;
//            switch (quest.questType) {
//                case QUEST_TYPE.EXPLORE_TILE:
//                    weight += GetExploreTileWeight((ExploreTile)task);
//                    break;
//                case QUEST_TYPE.EXPAND:
//                    weight += GetExpandWeight((Expand)task);
//                    break;
//                case QUEST_TYPE.ATTACK:
//                    weight += GetAttackWeight((Attack)task);
//                    break;
//                case QUEST_TYPE.DEFEND:
//                    weight += GetDefendWeight((Defend)task);
//                    break;
//                case QUEST_TYPE.BUILD_STRUCTURE:
//                    weight += GetBuildStructureWeight((BuildStructure)task);
//                    break;
//				case QUEST_TYPE.OBTAIN_MATERIAL:
//					weight += GetObtainMaterialWeight((ObtainMaterial)task);
//					break;
//                case QUEST_TYPE.EXPEDITION:
//                    weight += GetExpeditionWeight((Expedition)task);
//                    break;
//                default:
//                    break;
//            }
//        } else {
//            switch (task.taskType) {
//                case TASK_TYPE.REST:
//                    weight += GetRestWeight();
//                    break;
//                case TASK_TYPE.GO_HOME:
//                    weight += GetGoHomeWeight();
//                    break;
//                case TASK_TYPE.DO_NOTHING:
//                    weight += GetDoNothingWeight();
//                    break;
//                case TASK_TYPE.JOIN_PARTY:
//                    weight += GetJoinPartyWeight((JoinParty)task);
//                    break;
//                case TASK_TYPE.TAKE_QUEST:
//                    weight += GetTakeQuestWeight();
//                    break;
//                case TASK_TYPE.UPGRADE_GEAR:
//                    weight += GetUpgradeGearWeight();
//                    break;
//                default:
//                    break;
//            }
//        }
//        
//        return weight;
//    }
//    internal virtual int GetExpandWeight(Expand expandQuest) {
//		return 0;
//    }
//    //internal virtual int GetExploreRegionWeight(ExploreRegion exploreRegionQuest) {
//    //	int weight = 0;
//    //	weight += 100; //Change algo if needed
//    //	return weight;
//    //}
//    internal virtual int GetExploreTileWeight(ExploreTile exploreTileQuest) {
//        return 0;
//    }
//    internal virtual int GetJoinPartyWeight(JoinParty joinParty) {
//		return 0;
//    }
//    internal virtual int GetTakeQuestWeight() {
//        Settlement settlement = (Settlement)_character.currLocation.landmarkOnTile;
//        //Take OldQuest.Quest - 400 (0 if no quest available in the current settlement)
//        if (settlement.questBoard.Count > 0) {
//            return 400;
//        }
//        return 0;
//    }
//    internal virtual int GetRestWeight() {
//        if (_character.currentHP < _character.maxHP) {
//            int percentMissing = (int)(100f - (_character.remainingHP * 100));
//            if(percentMissing >= 50) {
//                return 100; //+100 if HP is below 50%
//            } else {
//                return 5 * percentMissing; //5 Weight per % of HP below max HP, 
//            }
//        }
//        return 0;
//    }
//    internal virtual int GetGoHomeWeight() {
//        //0 if already at Home Settlement or no path to it
//        if (_character.currLocation.isHabitable && _character.currLocation.isOccupied && _character.currLocation.id == _character.home.location.id) {
//            return 0;
//        }
//        if (PathGenerator.Instance.GetPath(_character.currLocation, _character.home.location, PATHFINDING_MODE.USE_ROADS) == null) {
//            return 0;
//        }
//        return 5;
//    }
//    internal virtual int GetDoNothingWeight() {
//        if(_character.currLocation.landmarkOnTile != null) {
//            if(_character.currLocation.landmarkOnTile is Settlement) {
//                Settlement currSettlement = (Settlement)_character.currLocation.landmarkOnTile;
//                if(currSettlement.owner != null) {
//                    if (!currSettlement.owner.IsHostileWith(_character.faction)) {
//                        return 200;//Do Nothing - 200 if in a non-hostile Settlement (0 otherwise)
//                    }
//                }
//            }
//        }
//        return 10;
//    }
//    internal virtual int GetDefendWeight(Defend defendQuest) {
//		return 0;
//    }
//    internal virtual int GetAttackWeight(Attack attackQuest) {
//		return 0;
//    }
//    internal virtual int GetBuildStructureWeight(BuildStructure buildStructure) {
//        return 0;
//    }
//	internal virtual int GetObtainMaterialWeight(ObtainMaterial obtainMaterial) {
//		return 0;
//	}
//    internal virtual int GetMoveToNonAdjacentVillageWeight(Settlement target) {
//        int weight = 0;
//        //Move to an adjacent non-hostile Village - 5 + (30 x Available OldQuest.Quest in that Village)
//        weight += 5 + (30 * target.questBoard.Count);
//        return weight;
//    }
//	internal virtual int GetDropPrisonersWeight(){
//		int dropPrisonersWeight = 0;
//		List<ECS.Character> prisoners = ((_character.party != null) ? _character.party.prisoners : character.prisoners);
//		if (prisoners != null && prisoners.Count > 0) {
//			for (int i = 0; i < prisoners.Count; i++) {
//				if(_character.faction == null || prisoners[i].faction == null || _character.faction.id != prisoners[i].faction.id){
//					dropPrisonersWeight += 40;
//				}
//			}
//		}
//		return dropPrisonersWeight;
//	}
//    internal virtual int GetUpgradeGearWeight() {
//        int weight = 0;
//        if (_character.GetNeededEquipmentTypes().Count > 0) { //check if character needs any equipment
//            if (_character.gold >= 30) { // 0 if Gold is less than 30g
//                if (!_character.HasWeaponEquipped()) {
//                    weight += 200; //+200 if missing Weapon
//                }
//                List<EQUIPMENT_TYPE> missingArmor = _character.GetMissingArmorTypes();
//                weight += 50 * missingArmor.Count; //+50 for each missing armor part
//
//                //+20 for every 10g above 30g
//                int goldAbove = _character.gold - 30;
//                goldAbove /= 10;
//                weight += 20 * goldAbove;
//            }
//        }
//        return weight;
//    }
//    internal virtual int GetExpeditionWeight(Expedition expedition) {
//        return 0;
//    }
//	internal virtual int GetSaveLandmarkWeight(ObtainMaterial obtainMaterial) {
//		return 0;
//	}
    #endregion

    #region Utilities
    /*
     Check if this role can accept a quest.
         */
    public bool CanAcceptQuest(OldQuest.Quest quest) {
        if (_allowedQuestTypes.Contains(quest.questType)) {
            return true;
        }
        return false;
    }
    #endregion

	#region Role Tasks
	public CharacterTask GetRoleTask(TASK_TYPE taskType){
		for (int i = 0; i < _roleTasks.Count; i++) {
			CharacterTask task = _roleTasks [i];
			if(task.taskType == taskType){
				return task;
			}
		}
		return null;
	}
	#endregion
}
