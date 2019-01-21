
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurrenderItemsQuest : Quest {

    private string neededItemName;

    public SurrenderItemsQuest(IQuestGiver questGiver, string neededItemName) : base(QUEST_TYPE.SURRENDER_ITEMS, questGiver) {
        this.neededItemName = neededItemName;
    }

    #region overrides
    public override Quest Clone() {
        SurrenderItemsQuest clone = new SurrenderItemsQuest(this.questGiver, neededItemName);
        SetCommonData(clone);
        return clone;
    }
    protected override string GetQuestName() {
        return "Surrender all " + neededItemName + "s to" + questGiver.name;
    }
    public override QuestAction GetQuestAction(Character character) {
        GiveItemAction giveAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.GIVE_ITEM) as GiveItemAction;
        giveAction.SetItemsToGive(character.GetItemsLike(neededItemName));

        giveAction.SetOnEndAction(() => SetQuestAsDone());

        QuestAction action = new QuestAction(giveAction, questGiver.questGiverObj, this);
        return action;

    }
    public override bool CanBeTakenBy(Character character) {
        if (!character.HasItemLike(neededItemName, 1)) { 
            return false; //if the character does not have any items like the needed item, return false
        }
        return base.CanBeTakenBy(character);
    }
    protected override void OnQuestDone() {
        base.OnQuestDone();
        Character character = owner;
        //character.currentQuest.OnQuestTurnedIn();
        //character.SetQuest(null);
    }
    //protected override void CancelQuest() {
    //    base.CancelQuest();
    //    Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    //}
    //public override void OnAcceptQuest(Character accepter) {
    //    base.OnAcceptQuest(accepter);
    //    //listen for when the owner of the quest obtains an item, if he/she already has the required items, 
    //    //the action that this quest will return when the character asks for an action will be turn in the quest
    //    Messenger.AddListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    //}
    //protected override void OnQuestDone() {
    //    base.OnQuestDone();
    //    Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    //    if (this.owner.party.actionData.questAssociatedWithCurrentAction != null && this.owner.party.actionData.questAssociatedWithCurrentAction.id == this.id) {
    //        //if the quest owner finished the quest while doing an action that this quest provided, immediately turn in the quest, otherwise, see GetQuestAction() 
    //        Debug.Log(this.owner.party.name + " obtained all needed items. Setting next action to turn in quest");
    //        this.owner.party.actionData.ForceDoAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.TURN_IN_QUEST), questGiver.questGiverObj);
    //    }
    //}
    //public override void OnQuestTurnedIn() {
    //    owner.ThrowItem(neededItemName, neededQuantity, false);
    //    for (int i = 0; i < neededQuantity; i++) { //give the items to the quest giver
    //        questGiver.AddItem(ItemManager.Instance.CreateNewItemInstance(neededItemName));
    //    }
    //    base.OnQuestTurnedIn();
    //}
    //protected override void CancelQuest() {
    //    base.CancelQuest();
    //    Messenger.RemoveListener<Item, Character>(Signals.ITEM_OBTAINED, OnItemObtained);
    //}
    #endregion
}
