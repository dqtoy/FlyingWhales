using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class DragonAttack : GameEvent {

    private Character _targetCharacter;
    private MonsterParty _dragonParty;

    public DragonAttack() : base(GAME_EVENT.DRAGON_ATTACK) {
    }

    public void Initialize(Character target, MonsterParty dragonParty) {
        _targetCharacter = target;
        _dragonParty = dragonParty;
        _dragonParty.GoToLocation(_targetCharacter.specificLocation, PATHFINDING_MODE.PASSABLE, CheckIfTargetIsStillInLocation);
    }

    private void CheckIfTargetIsStillInLocation() {
        if(_targetCharacter.specificLocation.tileLocation.id == _dragonParty.specificLocation.tileLocation.id) {
            CombatWithTarget();
        } else {
            _dragonParty.GoToLocation(_targetCharacter.specificLocation, PATHFINDING_MODE.PASSABLE, CheckIfTargetIsStillInLocation);
        }
    }

    private void CombatWithTarget() {
        //Make sure that the target character will not leave
        _dragonParty.SetIsAttacking(true);
        Combat combat = _dragonParty.StartCombatWith(_targetCharacter.currentParty);
        combat.AddAfterCombatAction(() => CheckCombatResults(combat));
        if(_dragonParty.specificLocation.tileLocation.landmarkOnTile != null) {
            Messenger.Broadcast(Signals.LANDMARK_UNDER_ATTACK, _dragonParty, this);
        }
    }
    private void CheckCombatResults(Combat combat) {
        if(_dragonParty.mainCharacter.currentSide == combat.winningSide && !_dragonParty.isDead) {
            _dragonParty.SetIsAttacking(false);
            if(_dragonParty.specificLocation.tileLocation.landmarkOnTile != null) {
                _dragonParty.specificLocation.tileLocation.landmarkOnTile.DestroyLandmark();
            }
            _dragonParty.GoHome(() => LayEggAndGoToSleep());
        }
        EndEvent();
    }

    private void LayEggAndGoToSleep() {
        Item dragonEgg = ItemManager.Instance.CreateNewItemInstance("Dragon Egg");
        _dragonParty.homeLandmark.AddItem(dragonEgg);
        (_dragonParty.mainCharacter as Monster).SetSleeping(true);
    }
}
