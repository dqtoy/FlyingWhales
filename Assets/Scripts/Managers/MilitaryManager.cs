/*
 This is the Internal OldQuest.Quest Manager. Each Faction has one,
 it is responsible for generating new quests for each faction.
 Reference: https://trello.com/c/Wf38ZqLM/737-internal-manager-ai
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MilitaryManager : TaskCreator {

    private Faction _owner;

    private List<OldQuest.Quest> _activeQuests;

    #region getters/setters
    public Faction owner {
        get { return _owner; }
    }
    public List<OldQuest.Quest> activeQuests {
        get { return _activeQuests; }
    }
    #endregion

	public MilitaryManager(Faction owner) {
        _owner = owner;
        _activeQuests = new List<OldQuest.Quest>();
        if(owner is Tribe) {
//            GameDate dueDate = new GameDate(GameManager.Instance.month, 1, GameManager.Instance.year);
//            SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
        }
    }

    /*
     Get the maximum number of active quests a faction
     can have.
         */
    private int GetMaxActiveQuests() {
        if(_owner.factionType == FACTION_TYPE.MINOR) {
            return 2;
        } else {
            switch (_owner.factionSize) {
                case FACTION_SIZE.SMALL:
                    return 3;
                case FACTION_SIZE.MEDIUM:
                    return 5;
                case FACTION_SIZE.LARGE:
                    return 7;
                default:
                    return 0;
            }
        }
    }

    #region OldQuest.Quest Generation
    /*
     At the start of each month, if the Faction has not yet reached the cap of active Internal Quests, 
     it will attempt to create a new one.
         */
    private void GenerateMonthlyQuests() {
        if(_activeQuests.Count < GetMaxActiveQuests()) {
            WeightedDictionary<OldQuest.Quest> questDictionary = GetQuestWeightedDictionary();
            questDictionary.LogDictionaryValues("OldQuest.Quest Creation Weights: ");
			if(questDictionary.Count > 0){
				OldQuest.Quest chosenQuestToCreate = questDictionary.PickRandomElementGivenWeights();
				AddNewQuest(chosenQuestToCreate);
			}
        }

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
    }

    private WeightedDictionary<OldQuest.Quest> GetQuestWeightedDictionary() {
        WeightedDictionary<OldQuest.Quest> questDict = new WeightedDictionary<OldQuest.Quest>();

        //Loop through each owned landmarks without active defend quest.
        //Defend weights
//        List<BaseLandmark> allOwnedLandmarks = _owner.GetAllOwnedLandmarks();
//        for (int i = 0; i < allOwnedLandmarks.Count; i++) {
//			BaseLandmark landmark = allOwnedLandmarks[i];
//			if(!IsAlreadyBeingDefended(landmark)){
//				int defendWeight = GetDefendWeight (landmark);
//				if(defendWeight > 0){
//					questDict.AddElement(new Defend(this, landmark), defendWeight);
//				}
//			}
//        }
        
        //Attack weights
//		if(_owner.IsAtWar()){
//			List<BaseLandmark> landmarksToBeAttacked = _owner.GetAllPossibleLandmarksToAttack ();
//			for (int i = 0; i < landmarksToBeAttacked.Count; i++) {
//				int attackWeight = GetAttackWeight (landmarksToBeAttacked[i]);
//				if(attackWeight > 0){
//					questDict.AddElement(new Attack(this, landmarksToBeAttacked[i]), attackWeight);
//				}
//			}
//		}
        return questDict;
    }

    private int GetDefendWeight(BaseLandmark landmark) {
        int weight = 0;
		if(landmark is Settlement){
			if(landmark.specificLandmarkType == LANDMARK_TYPE.CITY){
				if(landmark.IsBorder()){
					if(landmark.IsAdjacentToEnemyTribe()){
						Settlement village = (Settlement)landmark;
						weight += (4 * village.civilians);
						//weight += village.materialsInventory.Sum (x => x.Value.totalCount);
						weight += (15 * village.GetTechnologyCount());
						if(landmark.HasWarlordOnAdjacentVillage()){
							weight += 150;
						}
						if(_owner.leader != null && _owner.leader.HasTrait(TRAIT.SMART) && IsLandmarkTargeted(landmark)){
							weight += 500;
						}
						if(_owner.leader != null && _owner.leader.HasTrait(TRAIT.DEFENSIVE)){
							weight += 50;
						}
					}else{
						weight += 50;
					}

					foreach (FactionRelationship factionRel in _owner.relationships.Values) {
						if(factionRel.isAdjacent){
							if(factionRel.faction1.leader != null && factionRel.faction2.leader != null){
								Relationship rel = factionRel.faction1.leader.GetRelationshipWith (factionRel.faction2.leader);
								if(rel != null){
									int relModifier = 1;
									if(rel.totalValue < 0){
										relModifier = 2;
									}
									weight -= (relModifier * rel.totalValue);
								}
							}
							weight += (4 * factionRel.factionLookup[factionRel.factionLookup[this._owner.id].targetFaction.id].threat);
						}
					}
				}else{
					weight += 20;
					if(HasDiscoveredMinorFaction(landmark.tileLocation.region)){
						weight += 30;
					}
					if(_owner.leader != null && _owner.leader.HasTrait(TRAIT.SMART) && IsLandmarkTargeted(landmark)){
						weight += 300;
					}
				}
			}else{
				weight += 10;
				if(HasDiscoveredMinorFaction(landmark.tileLocation.region)){
					weight += 20;
				}
				if(_owner.leader != null && owner.leader.HasTrait(TRAIT.SMART) && IsLandmarkTargeted(landmark)){
					weight += 300;
				}
			}
		}else{
			weight += 5;
			if(HasDiscoveredMinorFaction(landmark.tileLocation.region)){
				weight += 10;
			}
			if(_owner.leader != null && _owner.leader.HasTrait(TRAIT.SMART) && IsLandmarkTargeted(landmark)){
				weight += 300;
			}
		}
        return weight;
    }
	private int GetAttackWeight(BaseLandmark landmark) {
		int weight = 0;
		weight += (15 * landmark.GetTechnologyCount ());
		weight += (4 * ((Settlement)landmark).civilians);

		/*TODO:	- add 100 to Weight to Attack if the city produces a Deficit resource
				- add 4 to Weight to Attack for each point of Relative Strength I have over the Faction
				- subtract 4 to Weight to Attack for each point of Relative Strength they have over my Faction
		*/
		if(_owner.leader != null && _owner.leader.HasTrait(TRAIT.IMPERIALIST)){
			weight += 50;
		}

		if (this._owner.leader != null && landmark.owner.leader != null) {
			Relationship rel = this._owner.leader.GetRelationshipWith (landmark.owner.leader);
			if (rel != null) {
				int relModifier = 3;
				if (rel.totalValue < 0) {
					relModifier = 5;
				}
				weight -= (relModifier * rel.totalValue);
			}
		}
		return weight;
	}
	private bool HasDiscoveredMinorFaction(Region region){
		for (int i = 0; i < region.landmarks.Count; i++) {
			if(region.landmarks[i].owner != null && region.landmarks[i].owner.factionType == FACTION_TYPE.MINOR && region.landmarks[i].isExplored){
				FactionRelationship factionRel = this._owner.GetRelationshipWith(region.landmarks[i].owner);
				if(factionRel != null && factionRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
					return true;
				}
			}
		}
		return false;
	}
	private bool IsLandmarkTargeted(BaseLandmark landmark){
		for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
			if(FactionManager.Instance.allFactions[i].id != this._owner.id && FactionManager.Instance.allFactions[i].militaryManager.IsAlreadyBeingAttacked(landmark)){
				return true;
			}
		}
		return false;
	}
    #endregion

    #region OldQuest.Quest Management
    public void AddNewQuest(OldQuest.Quest quest) {
        if (!_activeQuests.Contains(quest)) {
            _activeQuests.Add(quest);
			_owner.AddNewQuest(quest);
            //quest.ScheduleDeadline(); //Once a quest has been added to active quest, scedule it's deadline
        }
    }
    public void RemoveQuest(OldQuest.Quest quest) {
        _activeQuests.Remove(quest);
		_owner.RemoveQuest(quest);
    }
    public List<OldQuest.Quest> GetQuestsOfType(QUEST_TYPE questType) {
        List<OldQuest.Quest> quests = new List<OldQuest.Quest>();
        for (int i = 0; i < _activeQuests.Count; i++) {
            OldQuest.Quest currQuest = _activeQuests[i];
            if(currQuest.questType == questType) {
                quests.Add(currQuest);
            }
        }
        return quests;
    }
	public bool AlreadyHasQuestOfType(QUEST_TYPE questType, object identifier){
		for (int i = 0; i < _activeQuests.Count; i++) {
			OldQuest.Quest currQuest = _activeQuests[i];
			if(currQuest.questType == questType) {
				if(questType == QUEST_TYPE.EXPLORE_REGION){
					Region region = (Region)identifier;
					if(((ExploreRegion)currQuest).regionToExplore.id == region.id){
						return true;
					}
				}else if(questType == QUEST_TYPE.EXPAND){
					HexTile hexTile = (HexTile)identifier;
					if(((Expand)currQuest).targetUnoccupiedTile.id == hexTile.id){
						return true;
					}
				}
			}
		}
		return false;
	}
    #endregion

	private bool IsAlreadyBeingDefended(BaseLandmark landmark){
//		for (int i = 0; i < _activeQuests.Count; i++) {
//			if(_activeQuests[i].questType == QUEST_TYPE.DEFEND){
//				Defend defend = (Defend)_activeQuests [i];
//				if(defend.landmarkToDefend.id == landmark.id){
//					return true;
//				}
//			}
//		}
		return false;
	}
	internal bool IsAlreadyBeingAttacked(BaseLandmark landmark){
//		for (int i = 0; i < _activeQuests.Count; i++) {
//			if(_activeQuests[i].questType == QUEST_TYPE.ATTACK){
//				Attack attack = (Attack)_activeQuests [i];
//				if(attack.landmarkToAttack.id == landmark.id){
//					return true;
//				}
//			}
//		}
		return false;
	}
}
