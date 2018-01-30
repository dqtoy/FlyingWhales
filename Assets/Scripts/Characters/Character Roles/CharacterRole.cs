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
    #endregion

	public CharacterRole(ECS.Character character){
		_character = character;
        _allowedQuestTypes = new List<QUEST_TYPE>();
	}

    #region Quest Weights
    /*
         Get the weighted dictionary for what action the character will do next.
             */
    internal virtual WeightedDictionary<Quest> GetActionWeights() {
        WeightedDictionary<Quest> actionWeights = new WeightedDictionary<Quest>();
        if(_character.currLocation.landmarkOnTile != null && _character.currLocation.landmarkOnTile is Settlement) {
            Settlement currSettlement = (Settlement)_character.currLocation.landmarkOnTile;
            bool canAccepQueststHere = true;
            if(_character.faction.id != currSettlement.owner.id) {
                //character is not of the same faction as the settlement
                FactionRelationship relWithFaction = FactionManager.Instance.GetRelationshipBetween(_character.faction, currSettlement.owner);
                if (relWithFaction.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                    canAccepQueststHere = false; //cannot accept quests at this settlement
                }
            }
            
            if (canAccepQueststHere) {
                for (int i = 0; i < currSettlement.questBoard.Count; i++) {
                    Quest currQuest = currSettlement.questBoard[i];
                    if (this.CanAcceptQuest(currQuest) && currQuest.CanAcceptQuest(_character)) { //Check both the quest filters and the quest types this role can accept
                        actionWeights.AddElement(currQuest, GetWeightForQuest(currQuest));
                    }
                }
            }

            //if (allowedQuestTypes.Contains(QUEST_TYPE.JOIN_PARTY)) {
            //    List<Party> partiesOnTile = currSettlement.GetPartiesInSettlement();
            //    for (int i = 0; i < partiesOnTile.Count; i++) {
            //        Party currParty = partiesOnTile[i];
            //        if (currParty.CanJoinParty(_character)) {
            //            JoinParty joinPartyTask = new JoinParty(_character, -1, currParty);
            //            if (joinPartyTask.CanAcceptQuest(_character)) {
            //                actionWeights.AddElement(joinPartyTask, GetWeightForQuest(joinPartyTask));
            //            }
            //        }
            //    }
            //}

            ////Military Quests
            //if (allowedQuestTypes.Contains(QUEST_TYPE.ATTACK) || allowedQuestTypes.Contains(QUEST_TYPE.DEFEND)) {
            //    for (int i = 0; i < _character.faction.militaryManager.activeQuests.Count; i++) {
            //        Quest currQuest = _character.faction.militaryManager.activeQuests[i];
            //        if (this.CanAcceptQuest(currQuest) && currQuest.CanAcceptQuest(_character)) { //Check both the quest filters and the quest types this role can accept
            //            actionWeights.AddElement(currQuest, GetWeightForQuest(currQuest));
            //        }
            //    }
            //}
        }

        Rest restTask = new Rest(_character, -1);
        actionWeights.AddElement(restTask, GetWeightForQuest(restTask));

        GoHome goHomeTask = new GoHome(_character, -1);
        actionWeights.AddElement(goHomeTask, GetWeightForQuest(goHomeTask));

        DoNothing doNothingTask = new DoNothing(_character, -1);
        actionWeights.AddElement(doNothingTask, GetWeightForQuest(doNothingTask));
        return actionWeights;
    }
    internal int GetWeightForQuest(Quest quest) {
        int weight = 0;
        switch (quest.questType) {
            //case QUEST_TYPE.EXPLORE_REGION:
            //	weight += GetExploreRegionWeight((ExploreRegion)quest);
            //	break;
            case QUEST_TYPE.EXPLORE_TILE:
                weight += GetExploreTileWeight((ExploreTile)quest);
                break;
            case QUEST_TYPE.EXPAND:
                weight += GetExpandWeight((Expand)quest);
                break;
            case QUEST_TYPE.REST:
                weight += GetRestWeight();
                break;
            case QUEST_TYPE.GO_HOME:
                weight += GetGoHomeWeight();
                break;
            case QUEST_TYPE.DO_NOTHING:
                weight += GetDoNothingWeight();
                break;
            case QUEST_TYPE.JOIN_PARTY:
                weight += GetJoinPartyWeight((JoinParty)quest);
                break;
            case QUEST_TYPE.ATTACK:
                weight += GetAttackWeight((Attack)quest);
                break;
            case QUEST_TYPE.DEFEND:
                weight += GetDefendWeight((Defend)quest);
                break;
            default:
                break;
        }
        return weight;
    }
    internal virtual int GetExpandWeight(Expand expandQuest) {
		return 0;
    }
    //internal virtual int GetExploreRegionWeight(ExploreRegion exploreRegionQuest) {
    //	int weight = 0;
    //	weight += 100; //Change algo if needed
    //	return weight;
    //}
    internal virtual int GetExploreTileWeight(ExploreTile exploreTileQuest) {
        return 0;
    }
    internal virtual int GetJoinPartyWeight(JoinParty joinParty) {
		return 0;
    }
    internal virtual int GetRestWeight() {
        if (_character.currentHP < _character.maxHP) {
            int percentMissing = (int)(100f - (_character.remainingHP * 100));
            if(percentMissing >= 50) {
                return 100; //+100 if HP is below 50%
            } else {
                return 5 * percentMissing; //5 Weight per % of HP below max HP, 
            }
        }
        return 0;
    }
    internal virtual int GetGoHomeWeight() {
        //0 if already at Home Settlement or no path to it
        if (_character.currLocation.isHabitable && _character.currLocation.isOccupied && _character.currLocation.id == _character.home.location.id) {
            return 0;
        }
        if (PathGenerator.Instance.GetPath(_character.currLocation, _character.home.location, PATHFINDING_MODE.USE_ROADS) == null) {
            return 0;
        }
        return 5; //5 if not
    }
    internal virtual int GetDoNothingWeight() {
        if(_character.currLocation.landmarkOnTile != null) {
            if(_character.currLocation.landmarkOnTile is Settlement) {
                Settlement currSettlement = (Settlement)_character.currLocation.landmarkOnTile;
                if(currSettlement.owner != null) {
                    if (currSettlement.owner.IsHostileWith(_character.faction)) {
                        return 200;//Do Nothing - 200 if in a non-hostile Settlement (0 otherwise)
                    }
                }
            }
        }
        return 10;
    }
    internal virtual int GetDefendWeight(Defend defendQuest) {
		return 0;
    }
    internal virtual int GetAttackWeight(Attack attackQuest) {
		return 0;
    }
    #endregion

    #region Utilities
    /*
     Check if this role can accept a quest.
         */
    public bool CanAcceptQuest(Quest quest) {
        if (_allowedQuestTypes.Contains(quest.questType)) {
            return true;
        }
        return false;
    }
    #endregion
}
