using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DragonAttack : GameEvent {

    private Character _targetCharacter;
    private MonsterParty _dragonParty;

    public DragonAttack() : base(GAME_EVENT.DRAGON_ATTACK) {
    }

    public void Initialize(Character target, MonsterParty dragonParty) {
        _targetCharacter = target;
        _dragonParty = dragonParty;
        _dragonParty.EndAction();
        _dragonParty.actionData.AssignAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.ATTACK), null);
        _dragonParty.GoToLocation(_targetCharacter.specificLocation, PATHFINDING_MODE.PASSABLE, CheckIfTargetIsStillInLocation, _targetCharacter);
    }

    private void CheckIfTargetIsStillInLocation() {
        if (_targetCharacter.specificLocation.tileLocation.id == _dragonParty.specificLocation.tileLocation.id && _dragonParty.specificLocation.tileLocation.landmarkOnTile != null) {
            CombatWithTarget();
        } else {
            _dragonParty.GoToLocation(_targetCharacter.specificLocation, PATHFINDING_MODE.PASSABLE, CheckIfTargetIsStillInLocation, _targetCharacter);
        }
    }

    private void CombatWithTarget() {
        //Make sure that the target character will not leave
        _dragonParty.SetIsAttacking(true);
        Combat combat = _dragonParty.CreateCombatWith(_targetCharacter.currentParty);
        combat.Fight(() => CheckCombatResults(combat));
        //combat.AddAfterCombatAction(() => CheckCombatResults(combat));
        if(_dragonParty.specificLocation.tileLocation.landmarkOnTile != null) {
            Messenger.Broadcast(Signals.LANDMARK_UNDER_ATTACK, _dragonParty.GetBase(), this.GetBase());
        }
    }
    private void CheckCombatResults(Combat combat) {
        if(_dragonParty.mainCharacter.currentSide == combat.winningSide && !_dragonParty.isDead) {
            _dragonParty.SetIsAttacking(false);
            _dragonParty.GoHome(() => LayEggAndGoToSleep(), () => DestroyLandmarkOnLeaveOfDragon(_dragonParty.specificLocation.tileLocation.landmarkOnTile));
        }
        EndEvent();
    }
    private void DestroyLandmarkOnLeaveOfDragon(BaseLandmark landmark) {
        if(landmark != null) {
            landmark.DestroyLandmark();
        }
    }
    private void LayEggAndGoToSleep() {
        Item dragonEgg = ItemManager.Instance.CreateNewItemInstance("Dragon Egg");
        _dragonParty.homeLandmark.AddItem(dragonEgg);
        Log log = new Log(GameManager.Instance.Today(), "Events", "DragonAttack", "lay_egg");
        log.AddToFillers(_dragonParty.mainCharacter, _dragonParty.mainCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(_dragonParty.homeLandmark.tileLocation.areaOfTile, _dragonParty.homeLandmark.tileLocation.areaOfTile.name, LOG_IDENTIFIER.LANDMARK_1);
        _dragonParty.mainCharacter.AddHistory(log);
        _dragonParty.homeLandmark.tileLocation.areaOfTile.AddHistory(log);

        _dragonParty.EndAction();
        Hibernate();
    }
    private void Hibernate() {
        CharacterAction hibernateAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.HIBERNATE);
        _dragonParty.actionData.AssignAction(hibernateAction, _dragonParty.icharacterObject);
    }
}
