using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Civilian : CharacterRole {

    public override int reservedSupply { get { return 50; } }

    public Civilian() : base(CHARACTER_ROLE.CIVILIAN, "Civilian", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUPPLY, INTERACTION_CATEGORY.INVENTORY }) {
        allowedInteractions = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.MINE_ACTION,
            INTERACTION_TYPE.CHOP_WOOD,
            INTERACTION_TYPE.SCRAP,
            //INTERACTION_TYPE.ASSAULT_ACTION_NPC,
        };
    }

    //#region Overrides
    //public override void DeathRole() {
    //    base.DeathRole();
    //    Messenger.RemoveListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
    //}
    //public override void ChangedRole() {
    //    base.ChangedRole();
    //    Messenger.RemoveListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
    //}
    //public override void OnAssignRole() {
    //    base.OnAssignRole();
    //    Messenger.AddListener<StructureObj, ObjectState>(Signals.STRUCTURE_STATE_CHANGED, OnStructureChangedState);
    //}
    //#endregion

    //private void OnStructureChangedState(StructureObj structure, ObjectState state) {
    //    if(structure == _character.homeStructure && state.stateName == "Ruined") {
    //        _character.LookForNewHomeStructure();
    //    }
    //}

}
