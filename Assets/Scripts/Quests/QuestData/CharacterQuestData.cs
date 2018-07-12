using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

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
    }

    public CharacterAction GetNextQuestAction(ref IObject targetObject) {
        return _parentQuest.GetQuestAction(_owner, this, ref targetObject);
    }

    #region virtuals
    public virtual IEnumerator SetupValuesCoroutine() { yield return null; }
    public virtual void AbandonQuest() {
        _owner.RemoveQuestData(this);
    }
    #endregion

    public void SetLastActionDesperateState(bool state) {
        lastActionWasDesperate = state;
    }
}
