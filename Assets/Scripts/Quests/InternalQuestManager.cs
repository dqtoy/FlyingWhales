/*
 This is the Internal Quest Manager. Each Faction has one,
 it is responsible for generating new quests for each faction.
 Reference: https://trello.com/c/Wf38ZqLM/737-internal-manager-ai
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InternalQuestManager : QuestCreator {

    private Faction _owner;

    private List<Quest> _activeQuests;

    #region getters/setters
    public Faction owner {
        get { return _owner; }
    }
    public List<Quest> activeQuests {
        get { return _activeQuests; }
    }
    #endregion

    public InternalQuestManager(Faction owner) {
        _owner = owner;
        _activeQuests = new List<Quest>();
        if(owner is Tribe) {
            GameDate dueDate = new GameDate(GameManager.Instance.month, 15, GameManager.Instance.year);
            SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
        }
    }

    /*
     Get the maximum number of active quests a faction
     can have.
         */
    private int GetMaxActiveQuests() {
        if(_owner.factionType == FACTION_TYPE.MINOR) {
            return 1 + _owner.settlements.Count;
        } else {
            switch (_owner.factionSize) {
                case FACTION_SIZE.SMALL:
                    return 2 + _owner.settlements.Count;
                case FACTION_SIZE.MEDIUM:
                    return 3 + _owner.settlements.Count;
                case FACTION_SIZE.LARGE:
                    return 4 + _owner.settlements.Count;
                default:
                    return 0;
            }
        }
    }

    #region Quest Generation
    /*
     At the start of each month, if the Faction has not yet reached the cap of active Internal Quests, 
     it will attempt to create a new one.
         */
    private void GenerateMonthlyQuests() {
        if(_activeQuests.Count < GetMaxActiveQuests()) {
            WeightedDictionary<Quest> questDictionary = GetQuestWeightedDictionary();
            questDictionary.LogDictionaryValues("Quest Creation Weights: ");
            if(questDictionary.GetTotalOfWeights() > 0) {
                Quest chosenQuestToCreate = questDictionary.PickRandomElementGivenWeights();
                AddNewQuest(chosenQuestToCreate);
            }
        }

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(15);
        SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
    }

    private WeightedDictionary<Quest> GetQuestWeightedDictionary() {
        WeightedDictionary<Quest> questDict = new WeightedDictionary<Quest>();
		List<Region> checkedExpandRegions = new List<Region> ();
        //Explore region weights
        //Loop through each Region that the Faction has a Settlement in.
        for (int i = 0; i < _owner.settlements.Count; i++) {
            Region regionOfSettlement = _owner.settlements[i].location.region;
            //check if the current region already has an active quest to explore it
            if(!AlreadyHasQuestOfType(QUEST_TYPE.EXPLORE_REGION, regionOfSettlement)) {
                questDict.AddElement(new ExploreRegion(this, 20, regionOfSettlement), GetExploreRegionWeight(regionOfSettlement));
            }
            for (int j = 0; j < regionOfSettlement.connections.Count; j++) {
                if (regionOfSettlement.connections[j] is Region) {
                    Region region = (Region)regionOfSettlement.connections[j];
                    if (!region.centerOfMass.isOccupied && !checkedExpandRegions.Contains(region)) {
                        if (!AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, region.centerOfMass)) {
                            questDict.AddElement(new Expand(this, 60, region.centerOfMass), GetExpandWeight(region));
                        }
                    }
                }
            }
        }
        //End Explore Region Weights
        
        return questDict;
    }

    private int GetExploreRegionWeight(Region region) {
        int weight = 0;
        for (int i = 0; i < region.landmarks.Count; i++) {
            BaseLandmark landmark = region.landmarks[i];
            if (landmark.isHidden) {
                weight += 20; //Add 20 Weight to Explore Region for each undiscovered Landmark
            }
        }
        return weight;
    }
	private int GetExpandWeight(Region region) {
		int weight = 0;
		for (int i = 0; i < region.connections.Count; i++) {
			if(region.connections[i] is Region){
				Region adjacentRegion = (Region)region.connections[i];
				if (adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id == this._owner.id) {
					int regionWeight = (int)(adjacentRegion.centerOfMass.landmarkOnTile.civilians - 40f);
					if(regionWeight > 0){
						weight += regionWeight;
					}
				}
			}
		}
		return weight;
	}
    //private void CreateNewQuest(QUEST_TYPE questType) {
    //    switch (questType) {
    //        case QUEST_TYPE.EXPLORE_REGION:
    //            ExploreRegion newExploreRegionQuest = new ExploreRegion(this, 90, 3);
    //            break;
    //        case QUEST_TYPE.OCCUPY_LANDMARK:
    //            break;
    //        case QUEST_TYPE.INVESTIGATE_LANDMARK:
    //            break;
    //        case QUEST_TYPE.OBTAIN_RESOURCE:
    //            break;
    //        case QUEST_TYPE.EXPAND:
    //            break;
    //        default:
    //            break;
    //    }
    //}
    #endregion

    #region Quest Management
    public void AddNewQuest(Quest quest) {
        if (!_activeQuests.Contains(quest)) {
            _activeQuests.Add(quest);
            _owner.AddNewQuest(quest);
            quest.ScheduleDeadline(); //Once a quest has been added to active quest, scedule it's deadline
        }
    }
    public void RemoveQuest(Quest quest) {
        _activeQuests.Remove(quest);
        _owner.RemoveQuest(quest);
    }
    public List<Quest> GetQuestsOfType(QUEST_TYPE questType) {
        List<Quest> quests = new List<Quest>();
        for (int i = 0; i < _activeQuests.Count; i++) {
            Quest currQuest = _activeQuests[i];
            if(currQuest.questType == questType) {
                quests.Add(currQuest);
            }
        }
        return quests;
    }
	public bool AlreadyHasQuestOfType(QUEST_TYPE questType, object identifier){
		for (int i = 0; i < _activeQuests.Count; i++) {
			Quest currQuest = _activeQuests[i];
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
}
