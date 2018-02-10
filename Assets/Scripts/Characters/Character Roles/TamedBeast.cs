using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TamedBeast : CharacterRole {
	
	public TamedBeast(ECS.Character character): base (character) {
		_roleType = CHARACTER_ROLE.TAMED_BEAST;
		_canAcceptQuests = false;
		_allowedQuestTypes = new List<QUEST_TYPE>() {
			QUEST_TYPE.JOIN_PARTY
		};
	}

	internal override WeightedDictionary<CharacterTask> GetActionWeights() {
		WeightedDictionary<CharacterTask> actionWeights = base.GetActionWeights();
		Region currRegionOfCharacter = _character.currLocation.region;

		//Upgrade Gear
		UpgradeGear upgradeGearTask = new UpgradeGear(_character);
		actionWeights.AddElement(upgradeGearTask, GetWeightForTask(upgradeGearTask));

		if (_character.currLocation.landmarkOnTile != null && _character.currLocation.landmarkOnTile is Settlement) {
			Settlement currSettlement = (Settlement)_character.currLocation.landmarkOnTile;
			//Join Party
			List<Party> partiesOnTile = currSettlement.GetPartiesOnLandmark();
			for (int i = 0; i < partiesOnTile.Count; i++) {
				Party currParty = partiesOnTile[i];
				if (currParty.CanJoinParty(_character)) {
					JoinParty joinPartyTask = new JoinParty(_character, currParty);
					actionWeights.AddElement(joinPartyTask, GetWeightForTask(joinPartyTask));
				}
			}

			//Move to nearest non-hostile Village - 500 if in a hostile Settlement (0 otherwise) (NOTE: this action allows the character to move through hostile regions)
			if (currSettlement.owner.IsHostileWith(_character.faction)) {
				actionWeights.AddElement(new MoveTo(_character, _character.GetNearestNonHostileSettlement(), PATHFINDING_MODE.USE_ROADS), 500);
			}
		}

		for (int i = 0; i < currRegionOfCharacter.adjacentRegionsViaMajorRoad.Count; i++) {
			Region adjRegion = currRegionOfCharacter.adjacentRegionsViaMajorRoad[i];
			Faction regionOwner = adjRegion.owner;
			if (regionOwner != null) {
				if (!regionOwner.IsHostileWith(_character.faction)) {
					Settlement adjSettlement = (Settlement)adjRegion.centerOfMass.landmarkOnTile;
					MoveTo moveToNonHostile = new MoveTo(_character, adjSettlement, PATHFINDING_MODE.USE_ROADS);
					actionWeights.AddElement(moveToNonHostile, GetMoveToNonAdjacentVillageWeight(adjSettlement));
				}
			}
		}

		return actionWeights;
	}

	internal override int GetJoinPartyWeight(JoinParty joinParty) {
		if(_character.party != null) {
			return 0; //if already in a party
		}
		int weight = 0;
		if(joinParty.partyToJoin.currLocation.id == _character.currLocation.id) {
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

	internal override int GetRestWeight() {
		return 0;
	}
	internal override int GetGoHomeWeight() {
		return 0;
	}
	internal override int GetDoNothingWeight() {
		return 0;
	}
}
