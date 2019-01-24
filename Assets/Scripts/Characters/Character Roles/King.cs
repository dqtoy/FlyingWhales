using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class King : CharacterRole {

    public King(Character character) : base(character) {
        _roleType = CHARACTER_ROLE.KING;
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
    //    if (structure == _character.homeStructure && state.stateName == "Ruined") {
    //        _character.LookForNewHomeStructure();
    //    }
    //}
}
