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
    public override void Activate(IInteractable interactable) {
        MonsterAttackEvent gameEvent = EventManager.Instance.AddNewEvent(GAME_EVENT.MONSTER_ATTACK) as MonsterAttackEvent;
        gameEvent.Initialize(interactable as BaseLandmark);
        base.Activate(interactable);
    }
    #endregion
}
