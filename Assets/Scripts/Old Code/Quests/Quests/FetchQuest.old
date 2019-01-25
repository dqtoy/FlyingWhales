using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class FetchQuest : Quest {

    private BaseLandmark targetLandmark;
    private string neededItemName;
    private int neededQuantity;

    public int fetchCooldown { get; private set; }

    public FetchQuest(IQuestGiver questGiver, BaseLandmark targetLandmark, string neededItemName, int neededQuantity) : base(QUEST_TYPE.FETCH_ITEM, questGiver) {
        this.targetLandmark = targetLandmark;
        this.neededItemName = neededItemName;
        this.neededQuantity = neededQuantity;
    }

    #region overrides
    public override Quest Clone() {
        FetchQuest clone = new FetchQuest(this.questGiver, targetLandmark, neededItemName, neededQuantity);
        SetCommonData(clone);
        return clone;
    }
    protected override string GetQuestName() {
        return "Fetch " + neededItemName + " " + neededQuantity + " from " + targetLandmark.landmarkName;
    }
    public override QuestAction GetQuestAction(Character character) {
        if (isQuestDone) {
            //if the quest was finished outside a quest action (eg. character obtained item from other combat), 
            //make the character turn in the quest once he/she chooses to perform a quest action (from this quest)again.
            Debug.Log(this.owner.party.name + " quest is already done. Turning in quest...");
            return new QuestAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.TURN_IN_QUEST), questGiver.questGiverObj, this);
        } else {
            QuestAction action = new QuestAction(targetLandmark.landmarkObj.currentState.GetAction(ACTION_TYPE.FETCH), targetLandmark.landmarkObj, this);
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
            this.owner.party.actionData.ForceDoAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.TURN_IN_QUEST), questGiver.questGiverObj);
        }
    }
    public override void OnQuestTurnedIn() {
        owner.ThrowItem(neededItemName, neededQuantity, false);
        //for (int i = 0; i < neededQuantity; i++) { //give the items to the quest giver
        //    questGiver.AddItem(ItemManager.Instance.CreateNewItemInstance(neededItemName));
        //}
        base.OnQuestTurnedIn();
    }
    protected override void CancelQuest() {
        base.CancelQuest();
        Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    }
    #endregion

    private void OnItemObtained(Item obtainedItem, Character characterThatObtainedItem) {
        if (characterThatObtainedItem.id == owner.id) {
            if (characterThatObtainedItem.HasItemLike(neededItemName, neededQuantity)) {
                Debug.Log(characterThatObtainedItem.name + " has obtained the needed items for the quest OUTSIDE of doing a quest action. Setting quest as done!");
                this.SetQuestAsDone();
            }
        }
    }
    public void CheckIfQuestIsCompleted() {
        if (this.owner.HasItemLike(neededItemName, neededQuantity)) {
            Debug.Log(this.owner.name + " has obtained the needed items for the quest. Setting quest as done!");
            this.SetQuestAsDone();
        }
    }

    public void AdjustFetchCooldown(int adjustment) {
        fetchCooldown += adjustment;
    }
    public void SetFetchCooldown(int cooldown) {
        fetchCooldown = cooldown;
    }
}
