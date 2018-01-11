/*
 This is the Internal Quest Manager. Each Faction has one,
 it is responsible for generating new quests for each faction.
 Reference: https://trello.com/c/Wf38ZqLM/737-internal-manager-ai
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InternalQuestManager : QuestCreator {

    private Faction _owner;

    private List<Quest> _activeQuests; //TODO: Move this to quest creator interface

    #region getters/setters
    public List<Quest> activeQuests {
        get { return _activeQuests; }
    }
    #endregion

    public InternalQuestManager(Faction owner) {
        _owner = owner;
        _activeQuests = new List<Quest>();

        GameDate dueDate = new GameDate(GameManager.Instance.month, 1, GameManager.Instance.year);
        SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
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
            WeightedDictionary<QUEST_TYPE> questDictionary = GetQuestWeightedDictionary();
            if(questDictionary.GetTotalOfWeights() > 0) {
                QUEST_TYPE chosenQuestType = questDictionary.PickRandomElementGivenWeights();
                CreateNewQuest(chosenQuestType);
            }
        }

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
    }

    private WeightedDictionary<QUEST_TYPE> GetQuestWeightedDictionary() {
        WeightedDictionary<QUEST_TYPE> questDict = new WeightedDictionary<QUEST_TYPE>();
        questDict.AddElement(QUEST_TYPE.EXPLORE_REGION, GetExploreRegionWeight());
        return questDict;
    }

    private int GetExploreRegionWeight() {
        int weight = 0;
        //Loop through each Region that the Faction has a Settlement in.
        for (int i = 0; i < _owner.settlements.Count; i++) {
            Region regionOfSettlement = _owner.settlements[i].location.region;
            for (int j = 0; j < regionOfSettlement.landmarks.Count; j++) {
                BaseLandmark landmark = regionOfSettlement.landmarks[j];
                if (landmark.isHidden) {
                    weight += 20; //Add 20 Weight to Explore Region for each undiscovered Landmark
                }
            }
        }
        return weight;
    }
    private void CreateNewQuest(QUEST_TYPE questType) {
        switch (questType) {
            case QUEST_TYPE.EXPLORE_REGION:
                ExploreRegion newExploreRegionQuest = new ExploreRegion(this, 90, 3);
                AddNewQuest(newExploreRegionQuest);
                break;
            case QUEST_TYPE.OCCUPY_LANDMARK:
                break;
            case QUEST_TYPE.INVESTIGATE_LANDMARK:
                break;
            case QUEST_TYPE.OBTAIN_RESOURCE:
                break;
            case QUEST_TYPE.EXPAND:
                break;
            default:
                break;
        }
    }
    #endregion

    #region Quest Management
    private void AddNewQuest(Quest quest) {
        if (!_activeQuests.Contains(quest)) {
            _activeQuests.Add(quest);
        }
    }
    private void RemoveQuest(Quest quest) {
        _activeQuests.Remove(quest);
    }
    #endregion
}
