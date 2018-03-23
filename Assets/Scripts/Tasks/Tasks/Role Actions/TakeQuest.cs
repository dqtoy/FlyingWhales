using UnityEngine;
using System.Collections;
using ECS;

public class TakeQuest : CharacterTask {
	public TakeQuest(TaskCreator createdBy, int defaultDaysLeft = -1, STANCE stance = STANCE.NEUTRAL) : base(createdBy, TASK_TYPE.TAKE_QUEST, stance, defaultDaysLeft) {
        _needsSpecificTarget = true;
        _specificTargetClassification = "quest";

        _states = new System.Collections.Generic.Dictionary<STATE, State>() {
            { STATE.TAKE_QUEST, new TakeQuestState(this) }
        };
    }

    #region overrides
    public override void OnChooseTask(Character character) {
        base.OnChooseTask(character);
        ChangeStateTo(STATE.TAKE_QUEST);
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
		if(CanBeDone(character, character.specificLocation)){
			return true;
		}
        return base.AreConditionsMet(character);
    }
    public override int GetSelectionWeight(Character character) {
        return 40;
    }
    #endregion
}
