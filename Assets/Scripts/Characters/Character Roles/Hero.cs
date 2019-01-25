/*
 The hero explores the world for both monsters and treasures. 
 They form party with Warriors to perform their duties. 
 They create Minor Roads when they explore roadless tiles.
 Place functions unique to heroes here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero : CharacterRole {

	public Hero(Character character): base (character) {
        _roleType = CHARACTER_ROLE.HERO;
    }

    //#region Overrides
    //public override void OnAssignRole() {
    //    base.OnAssignRole();
    //    CharacterAction _defendAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.DEFEND) as DefendAction;
    //    _character.AddMiscAction(_defendAction);
    //    Messenger.AddListener<Party, GameEvent>(Signals.LANDMARK_UNDER_ATTACK, LandmarkUnderAttack);
    //}
    //public override void DeathRole() {
    //    base.DeathRole();
    //    _character.RemoveMiscAction(ACTION_TYPE.DEFEND);
    //    Messenger.RemoveListener<Party, GameEvent>(Signals.LANDMARK_UNDER_ATTACK, LandmarkUnderAttack);
    //}
    //public override void ChangedRole() {
    //    base.ChangedRole();
    //    Messenger.RemoveListener<Party, GameEvent>(Signals.LANDMARK_UNDER_ATTACK, LandmarkUnderAttack);
    //}
    //#endregion

    //private void LandmarkUnderAttack(Party attacker, GameEvent associatedEvent) {
    //    if(_character.currentParty.currentCombat == null && attacker.specificLocation.coreTile.landmarkOnTile != null && attacker.specificLocation.id == _character.homeLandmark.tileLocation.areaOfTile.id) {
    //        DefendAction defendAction = _character.GetMiscAction(ACTION_TYPE.DEFEND) as DefendAction;
    //        if (!_character.IsInOwnParty()) {
    //            _character.currentParty.RemoveCharacter(_character);
    //        }
    //        CharacterParty characterParty = _character.ownParty as CharacterParty;
    //        characterParty.actionData.AssignAction(defendAction, attacker.specificLocation.coreTile.landmarkOnTile.landmarkObj, null, associatedEvent);
    //    }
    //}
}
