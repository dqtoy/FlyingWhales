using UnityEngine;
using System.Collections;

public class TakeQuestState : State {
    public TakeQuestState(CharacterTask parentTask) : base(parentTask, STATE.TAKE_QUEST) {
    }

    #region overrides
    public override bool PerformStateAction() {
        if (!base.PerformStateAction()) { return false; }
        if (parentTask.specificTarget == null) {
            //Get target quest based on weights
            WeightedDictionary<Quest> questWeights = GetQuestsDictionary(_assignedCharacter);
			if(questWeights.Count > 0){
				questWeights.PickRandomElementGivenWeights().AcceptQuest(_assignedCharacter);
			}else{
				parentTask.EndTaskFail();
				return false;
			}
        } else {
            (parentTask.specificTarget as Quest).AcceptQuest(_assignedCharacter);
        }
		parentTask.EndTaskSuccess();
        return true;
    }
    #endregion

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
}
