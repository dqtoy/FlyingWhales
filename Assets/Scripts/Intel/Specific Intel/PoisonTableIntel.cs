using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonTableIntel : EventIntel {

    public GoapAction eatAtTableAction { get; private set; }
    public Table poisonedTable { get; private set; }
    public Dwelling targetDwelling { get; private set; }

    public PoisonTableIntel(Character actor, GoapAction action) : base(actor, action) {
        if (action.result == InteractionManager.Goap_State_Success) {
            poisonedTable = this.action.poiTarget as Table;
            targetDwelling = poisonedTable.gridTileLocation.structure as Dwelling;
            Messenger.AddListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnCharacterDidAction);
        }
    }

    private void OnCharacterDidAction(Character character, GoapAction action) {
        //once this intel is made, it should listen for when a character eats at the table that was poisoned, and store that action
        if (action.goapType == INTERACTION_TYPE.EAT_AT_TABLE && action.poiTarget == poisonedTable) {
            eatAtTableAction = action;
        }
    }

    public override void OnIntelExpire() {
        base.OnIntelExpire();
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_DID_ACTION)) {
            Messenger.RemoveListener<Character, GoapAction>(Signals.CHARACTER_DID_ACTION, OnCharacterDidAction);
        }
    }

}
