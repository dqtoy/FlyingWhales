using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAttack : PlayerAbility {

    public MonsterAttack() : base(ABILITY_TYPE.STRUCTURE) {
        _name = "Monster Attack";
        _description = "Spawn a monster party on a landmark";
        _powerCost = 25;
        _threatGain = 10;
        _cooldown = 12;
    }

    #region Overrides
    public override void DoAbility(IInteractable interactable) {
        base.DoAbility(interactable);
        //MonsterAttackEvent gameEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.MONSTER_ATTACK) as MonsterAttackEvent;
        //gameEvent.Initialize(interactable as BaseLandmark);
        RecallMinion();
    }
    public override bool CanBeDone(IInteractable interactable) {
        if (base.CanBeDone(interactable)) {
            BaseLandmark landmark = interactable as BaseLandmark;
            if (landmark.specificLandmarkType != LANDMARK_TYPE.DEMONIC_PORTAL && landmark.specificLandmarkType != LANDMARK_TYPE.MONSTER_DEN 
                && landmark.specificLandmarkType != LANDMARK_TYPE.LAIR) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
