using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterQuestData {

    protected Quest _parentQuest;
    protected Character _owner;
    public bool lastActionWasDesperate = false;

    #region getters/setters
    public Quest parentQuest {
        get { return _parentQuest; }
    }
    public Character owner {
        get { return _owner; }
    }
    #endregion

    public CharacterQuestData(Quest parentQuest, Character owner) {
        _parentQuest = parentQuest;
        _owner = owner;
        //Messenger.AddListener<Quest>(Signals.QUEST_DONE, OnQuestDone);
    }

    public CharacterAction GetNextQuestAction(ref IObject targetObject) {
        return null;
            //_parentQuest.GetQuestAction(_owner, this, ref targetObject);
    }

    #region virtuals
    public virtual IEnumerator SetupValuesCoroutine() { yield return null; }
    public virtual void AbandonQuest() {
        //_owner.RemoveQuestData(this);
        //Messenger.RemoveListener<Quest>(Signals.QUEST_DONE, OnQuestDone);
    }
    protected virtual void OnQuestDone(Quest doneQuest) {
        if (_parentQuest.id == doneQuest.id) {
            //Messenger.RemoveListener<Quest>(Signals.QUEST_DONE, OnQuestDone);
            //_owner.RemoveQuestData(this); //remove this data from the character
            //if (_owner.party.actionData.questDataAssociatedWithCurrentAction != null 
            //    && _owner.party.actionData.questDataAssociatedWithCurrentAction.parentQuest.id == doneQuest.id) {
            //    //cancel the characters current action then look for another action
            //    _owner.party.actionData.EndAction();
            //}
        }
    }
    #endregion

    public void SetLastActionDesperateState(bool state) {
        lastActionWasDesperate = state;
    }
}
