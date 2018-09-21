﻿using ECS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveItemAction : CharacterAction {

    private List<Item> itemsToGive;

    public GiveItemAction() : base(ACTION_TYPE.GIVE_ITEM) {
    }

    #region overrides
    public override CharacterAction Clone() {
        GiveItemAction action = new GiveItemAction();
        SetCommonData(action);
        action.Initialize();
        return action;
    }
    public override void PerformAction(CharacterParty party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        if (targetObject is CharacterObj) {
            party.characterOwner.GiveItemsTo(itemsToGive, (targetObject as CharacterObj).party.characterOwner);
        }
        EndAction(party, targetObject);
    }
    #endregion

    public void SetItemsToGive(List<Item> items) {
        itemsToGive = items;
    }
}