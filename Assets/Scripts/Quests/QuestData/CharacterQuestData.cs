﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterQuestData {

    protected Quest _parentQuest;
    protected ECS.Character _owner;
    public bool lastActionWasDesperate = false;

    public CharacterQuestData(Quest parentQuest, ECS.Character owner) {
        _parentQuest = parentQuest;
        _owner = owner;
    }

    public CharacterAction GetNextQuestAction() {
        return _parentQuest.GetQuestAction(_owner, this);
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
