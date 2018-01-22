﻿/*
 This is the base class for character roles
 such as Chieftain, Village Head, etc.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterRole {
	protected ECS.Character _character;
    protected CHARACTER_ROLE _roleType;
    protected List<ROAD_TYPE> allowedRoadTypes; //states what roads this role can use.
    protected bool canPassHiddenRoads; //can the character use roads that haven't been discovered yet?

    #region getters/setters
    public CHARACTER_ROLE roleType {
        get { return _roleType; }
    }
	public ECS.Character character{
		get { return _character; }
	}
    #endregion

	public CharacterRole(ECS.Character character){
		_character = character;
	}

	/*
         Get the weighted dictionary for what action the character will do next.
             */
	internal WeightedDictionary<Quest> GetActionWeights() {
		WeightedDictionary<Quest> actionWeights = new WeightedDictionary<Quest>();
		for (int i = 0; i < _character.faction.activeQuests.Count; i++) {
			Quest currQuest = _character.faction.activeQuests[i];
			if (currQuest.CanAcceptQuest(_character)) {
				actionWeights.AddElement(currQuest, GetWeightForQuest(currQuest));
			}
		}

		if(_character.party == null) {
			for (int i = 0; i < PartyManager.Instance.allParties.Count; i++) {
				Party currParty = PartyManager.Instance.allParties[i];
				if (!currParty.isFull && currParty.isOpen) {
					JoinParty joinPartyTask = new JoinParty(_character, -1, currParty);
					if (joinPartyTask.CanAcceptQuest(_character)) {
						actionWeights.AddElement(joinPartyTask, GetWeightForQuest(joinPartyTask));
					}
				}
			}
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
		case QUEST_TYPE.EXPLORE_REGION:
			weight += GetExploreRegionWeight((ExploreRegion)quest);
			break;
		case QUEST_TYPE.OCCUPY_LANDMARK:
			break;
		case QUEST_TYPE.INVESTIGATE_LANDMARK:
			break;
		case QUEST_TYPE.OBTAIN_RESOURCE:
			break;
		case QUEST_TYPE.EXPAND:
			weight += GetExpandWeight ((Expand)quest);
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
	internal virtual int GetExploreRegionWeight(ExploreRegion exploreRegionQuest) {
		return 0;
	}
	internal virtual int GetJoinPartyWeight(JoinParty joinParty) {
		return 0;
	}
	internal virtual int GetRestWeight() {
		if (_character.currentHP < _character.maxHP) {
			int percentMissing = _character.currentHP / _character.maxHP;
			return 5 * percentMissing; //5 Weight per % of HP below max HP
		}
		return 0;
	}
	internal virtual int GetGoHomeWeight() {
		//0 if already at Home Settlement or no path to it
		if (_character.currLocation.isHabitable && _character.currLocation.isOccupied && _character.currLocation.landmarkOnTile.owner == _character.faction) {
			return 0;
		}
		if (PathGenerator.Instance.GetPath(_character.currLocation, _character.home.location, PATHFINDING_MODE.USE_ROADS) == null) {
			return 0;
		}
		return 5; //5 if not
	}
	internal virtual int GetDoNothingWeight() {
		return 10;
	}
	internal virtual int GetDefendWeight(Defend defendQuest) {
		return 0;
	}
	internal virtual int GetAttackWeight(Attack attackQuest) {
		return 0;
	}
}
