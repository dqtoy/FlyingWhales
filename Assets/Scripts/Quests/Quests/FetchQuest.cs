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
            //if the quest was finished outside a quest action (eg. character obtained item from other combat), 
            //make the character turn in the quest once he/she chooses to perform a quest action (from this quest)again.
            //character.party.actionData.SetQuestAssociatedWithAction(null);
            Debug.Log(this.owner.party.name + " quest is already done. Turning in quest...");
            if (Messenger.eventTable.ContainsKey(Signals.ITEM_OBTAINED)) {
                Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
            }
            return new QuestAction(character.workplace.landmarkObj.currentState.GetAction(ACTION_TYPE.TURN_IN_QUEST), character.workplace.landmarkObj, this);
        } else {
            QuestAction action = new QuestAction(targetLandmark.landmarkObj.currentState.GetAction(ACTION_TYPE.FETCH), targetLandmark.landmarkObj, this);
            //character.party.actionData.SetQuestAssociatedWithAction(this);
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
        if (this.owner.party.actionData.questAssociatedWithCurrentAction != null && this.owner.party.actionData.questAssociatedWithCurrentAction.id == this.id) {
            //if the quest owner finished the quest while doing an action that this quest provided, immediately turn in the quest, otherwise, see GetQuestAction() 
            Debug.Log(this.owner.party.name + " obtained all needed items. Setting next action to turn in quest");
            this.owner.party.actionData.ForceDoAction(owner.workplace.landmarkObj.currentState.GetAction(ACTION_TYPE.TURN_IN_QUEST), owner.workplace.landmarkObj);
        }
    }
    #endregion

    private void OnItemObtained(Item obtainedItem, Character characterThatObtainedItem) {
        if (characterThatObtainedItem.id == owner.id) {
            if (characterThatObtainedItem.HasItem(neededItemName, neededQuantity)) {
                Debug.Log(characterThatObtainedItem.name + " has obtained the needed items for the quest OUTSIDE of doing a quest action. Setting quest as done!");
                this.SetQuestAsDone();
            }
        }
    }

    public void CheckIfQuestIsCompleted() {
        if (this.owner.HasItem(neededItemName, neededQuantity)) {
            Debug.Log(this.owner.name + " has obtained the needed items for the quest. Setting quest as done!");
            this.SetQuestAsDone();
        }
    }
}
