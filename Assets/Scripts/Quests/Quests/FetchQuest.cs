using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class FetchQuest : Quest {

    private BaseLandmark targetLandmark;
    private string neededItemName;
    private int neededQuantity;

    public FetchQuest(BaseLandmark targetLandmark, string neededItemName, int neededQuantity) : base(QUEST_TYPE.FETCH_ITEM) {
        this.targetLandmark = targetLandmark;
        this.neededItemName = neededItemName;
        this.neededQuantity = neededQuantity;
    }

    #region overrides
    protected override string GetQuestName() {
        return "Fetch " + neededItemName + " " + neededQuantity + " from " + targetLandmark.landmarkName;
    }
    public override QuestAction GetQuestAction(Character character) {
        if (isQuestDone) {
            return new QuestAction(character.workplace.landmarkObj.currentState.GetAction(ACTION_TYPE.TURN_IN_QUEST), character.workplace.landmarkObj);
        } else {
            QuestAction action = new QuestAction(targetLandmark.landmarkObj.currentState.GetAction(ACTION_TYPE.FETCH), targetLandmark.landmarkObj);
            return action;
        }
    }
    public override void OnAcceptQuest(Character accepter) {
        base.OnAcceptQuest(accepter);
        //listen for when the owner of the quest obtains an item, if he/she already has the required items, 
        //the action that this quest will return when the character asks for an action will be turn in the quest
        Messenger.AddListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    }
    protected override void OnQuestDone() {
        base.OnQuestDone();
        Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    }
    #endregion

    private void OnItemObtained(Item obtainedItem, Character characterThatObtainedItem) {
        if (characterThatObtainedItem.id == owner.id) {
            if (characterThatObtainedItem.HasItem(neededItemName, neededQuantity)) {
                this.SetQuestAsDone();
            }
        }
    }
}
