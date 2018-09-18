using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Awaken : PlayerAbility {

    public Awaken() : base(ABILITY_TYPE.MONSTER) {
        _name = "Awaken";
        _description = "Awaken a monster from deep slumber";
        _powerCost = 25;
        _threatGain = 5;
        _cooldown = 12;
    }

    #region Overrides
    public override void Activate(IInteractable interactable) {
        if (!CanBeActivated(interactable)) {
            return;
        }
        Monster monster = interactable as Monster;
        if(monster.isSleeping && monster.name == "Dragon") {
            monster.SetSleeping(false);
            if(PlayerManager.Instance.player.markedCharacter != null) {
                DragonAttack dragonAttack = EventManager.Instance.AddNewEvent(GAME_EVENT.DRAGON_ATTACK) as DragonAttack;
                dragonAttack.Initialize(PlayerManager.Instance.player.markedCharacter, monster.party);
            } else {
                ScheduleSleep(monster);
            }
            base.Activate(monster);
        }
    }
    #endregion

    private void ScheduleSleep(Monster monster) {
        GameDate sleepDate = GameManager.Instance.Today();
        sleepDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(sleepDate, () => monster.SetSleeping(true));
    }
}
