using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Adventurer : CharacterRole {

	public Adventurer(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.ADVENTURER;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.JOIN_PARTY
        };
    }

    internal override WeightedDictionary<Quest> GetActionWeights() {
        WeightedDictionary<Quest> questWeights = base.GetActionWeights();
        Settlement currSettlement = (Settlement)_character.currLocation.landmarkOnTile;
        Region currRegionOfCharacter = _character.currLocation.region;

        //Join Party
        List<Party> partiesOnTile = currSettlement.GetPartiesInSettlement();
        for (int i = 0; i < partiesOnTile.Count; i++) {
            Party currParty = partiesOnTile[i];
            if (currParty.CanJoinParty(_character)) {
                JoinParty joinPartyTask = new JoinParty(_character, -1, currParty);
                if (joinPartyTask.CanAcceptQuest(_character)) {
                    questWeights.AddElement(joinPartyTask, GetWeightForQuest(joinPartyTask));
                }
            }
        }

        for (int i = 0; i < currRegionOfCharacter.adjacentRegionsViaMajorRoad.Count; i++) {
            Region adjRegion = currRegionOfCharacter.adjacentRegionsViaMajorRoad[i];
            Faction regionOwner = adjRegion.owner;
            if (regionOwner != null) {
                if (!regionOwner.IsHostileWith(_character.faction)) {
                    Settlement adjSettlement = (Settlement)adjRegion.centerOfMass.landmarkOnTile;
                    MoveTo moveToNonHostile = new MoveTo(_character, -1, adjSettlement.location, PATHFINDING_MODE.USE_ROADS);
                    questWeights.AddElement(moveToNonHostile, GetMoveToNonAdjacentVillageWeight(adjSettlement));
                }
            }
        }

        //Move to nearest non-hostile Village - 500 if in a hostile Settlement (0 otherwise) (NOTE: this action allows the character to move through hostile regions)
        if (currSettlement.owner.IsHostileWith(_character.faction)) {
            questWeights.AddElement(new MoveTo(_character, -1, _character.GetNearestNonHostileSettlement().location, PATHFINDING_MODE.USE_ROADS), 500);
        }
        return questWeights;
    }

    internal override int GetJoinPartyWeight(JoinParty joinParty) {
		if(_character.party != null) {
			return 0; //if already in a party
		}
		int weight = 0;
		if(joinParty.partyToJoin.partyLeader.currLocation.id == _character.currLocation.id) {
			//party leader and this character are at the same tile
			return 200;
		} else {
			List<HexTile> pathToParty = PathGenerator.Instance.GetPath(_character.currLocation, joinParty.partyToJoin.partyLeader.currLocation, PATHFINDING_MODE.USE_ROADS);
			if(pathToParty != null) {
				weight += 200 - (15 * pathToParty.Count); //200 - (15 per tile distance) if not in a party
			}
		}
		return Mathf.Max(0, weight);
	}

    internal override int GetGoHomeWeight() {
        //0 if already at Home Settlement or no path to it
        if (_character.currLocation.isHabitable && _character.currLocation.isOccupied && _character.currLocation.id == _character.home.location.id) {
            return 0;
        }
        if (PathGenerator.Instance.GetPath(_character.currLocation, _character.home.location, PATHFINDING_MODE.USE_ROADS) == null) {
            return 0;
        }
        return 20; //20 if not
    }

    private int GetMoveToNonAdjacentVillageWeight(Settlement target) {
        int weight = 0;
        //Move to an adjacent non-hostile Village - 5 + (30 x Available Quest in that Village)
        weight += 5 + (30 * target.questBoard.Count);
        return weight;
    }
}
