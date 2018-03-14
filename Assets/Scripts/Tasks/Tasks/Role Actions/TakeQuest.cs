using UnityEngine;
using System.Collections;
using ECS;

public class TakeQuest : CharacterTask {
	public TakeQuest(TaskCreator createdBy, int defaultDaysLeft = -1) : base(createdBy, TASK_TYPE.TAKE_QUEST, defaultDaysLeft) {
        _needsSpecificTarget = true;
        _specificTargetClassification = "quest";
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        if (_specificTarget == null) {
            //Get target quest based on weights
            WeightedDictionary<Quest> questWeights = GetQuestsDictionary(character);
            questWeights.PickRandomElementGivenWeights().AcceptQuest(character);
        } else {
            (_specificTarget as Quest).AcceptQuest(character);
        }
        EndTask(TASK_STATUS.SUCCESS);
    }
    public override bool CanBeDone(Character character, ILocation location) {
        //check if the character can accept any quests
        for (int i = 0; i < QuestManager.Instance.availableQuests.Count; i++) {
            Quest currQuest = QuestManager.Instance.availableQuests[i];
            if (currQuest.CanAcceptQuest(character)) {
                return true;
            }
        }
        return base.CanBeDone(character, location);
    }
    public override bool AreConditionsMet(Character character) {
        //check if the character can accept any quests
        for (int i = 0; i < QuestManager.Instance.availableQuests.Count; i++) {
            Quest currQuest = QuestManager.Instance.availableQuests[i];
            if (currQuest.CanAcceptQuest(character)) {
                return true;
            }
        }
        return base.AreConditionsMet(character);
    }
    public override int GetSelectionWeight(Character character) {
        int weight = base.GetSelectionWeight(character);
        weight += 40;
        return weight;
    }
    private WeightedDictionary<Quest> GetQuestsDictionary(ECS.Character character) {
        WeightedDictionary<Quest> questsDictionary = new WeightedDictionary<Quest>();
        for (int i = 0; i < QuestManager.Instance.availableQuests.Count; i++) {
            Quest currQuest = QuestManager.Instance.availableQuests[i];
            if (currQuest.CanAcceptQuest(character)) {
                int weight = 50;//All Quests that can be taken: 50
                bool isFromSameFaction = false;
                if (currQuest.createdBy is ECS.Character) {
                    if ((currQuest.createdBy as ECS.Character).faction == character.faction) {
                        isFromSameFaction = true;
                    }
                }
                if (isFromSameFaction) {
                    weight += 100; //Quest came from same faction: +100
                }
                questsDictionary.AddElement(currQuest, weight);
            }
        }
        return questsDictionary;
    }
    //  public override void PerformTask() {
    //      base.PerformTask();
    //_assignedCharacter.SetCurrentTask(this);
    //if (_assignedCharacter.party != null) {
    //	_assignedCharacter.party.SetCurrentTask(this);
    //      }
    //      if (!(_assignedCharacter.specificLocation is Settlement)) {
    //          //Make sure the character is at a settlement before performing this task
    //          throw new System.Exception(_assignedCharacter.name + " is not at a settlement!");
    //      }
    //Settlement currSettlement = _assignedCharacter.specificLocation as Settlement;

    //      WeightedDictionary<Quest> questWeights = new WeightedDictionary<Quest>();
    //      Quest chosenQuest = null;
    //      //for (int i = 0; i < currSettlement.questBoard.Count; i++) {
    //      //    Quest currQuest = currSettlement.questBoard[i];
    //      //    questWeights.AddElement(currQuest, 100);
    //      //}

    //      if (questWeights.GetTotalOfWeights() > 0) {
    //          chosenQuest = questWeights.PickRandomElementGivenWeights();
    //          Debug.Log(_assignedCharacter.name + " chose to take quest " + chosenQuest.questType.ToString());
    //      } 
    //      //else {
    //      //    chosenTask = new DoNothing(_assignedCharacter);
    //      //    Debug.Log(_assignedCharacter.name + " could not find any quest to take.");
    //      //}
    //      if(chosenQuest != null) {
    //          _assignedCharacter.SetCurrentQuest(chosenQuest);
    //      }

    //      EndTask(TASK_STATUS.SUCCESS);
    //  }
    //  public override int GetTaskWeight(ECS.Character character) {
    //      if (character.specificLocation is Settlement) {
    //          Settlement settlement = character.specificLocation as Settlement;
    //          //Take Quest - 400 (0 if no quest available in the current settlement)
    //          //if (settlement.questBoard.Count > 0) {
    //          //    return 400;
    //          //}
    //      }
    //      return 0;
    //  }
    #endregion
}
