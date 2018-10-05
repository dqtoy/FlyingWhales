using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Awaken : PlayerAbility {

    private Monster monster;
    public Awaken() : base(ABILITY_TYPE.MONSTER) {
        _name = "Awaken";
        _description = "Awaken a monster from deep slumber";
        _powerCost = 25;
        _threatGain = 5;
        _cooldown = 12;
    }

    #region Overrides
    public override bool CanBeDone(IInteractable interactable) {
        if (base.CanBeDone(interactable)) {
            Monster monster = interactable as Monster;
            if (monster.isSleeping) {
                return true;
            }
        }
        return false;
    }
    public override void DoAbility(IInteractable interactable) {
        base.DoAbility(interactable);
        monster = interactable as Monster;
        Log log = new Log(GameManager.Instance.Today(), "PlayerAbilities", _name, "awaken");
        log.AddToFillers(monster, monster.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        monster.AddHistory(log);
        if (monster.name == "Dragon") {
            monster.SetSleeping(false);
            if (PlayerManager.Instance.player.markedCharacter != null) {
                TriggerDragonAttack();
            } else {
                Messenger.AddListener(Signals.CHARACTER_MARKED, ReceivedMarkedCharacterSignal);
                ScheduleSleep(monster);
                monster.currentParty.EndAction();
                (monster.currentParty as MonsterParty).actionData.AssignAction(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.IDLE), monster.currentParty.icharacterObject);
            }
        }
        RecallMinion();
    }
    #endregion

    private void ScheduleSleep(Monster monster) {
        GameDate sleepDate = GameManager.Instance.Today();
        sleepDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(sleepDate, () => MonsterWillSleep());
    }

    private void ReceivedMarkedCharacterSignal() {
        Messenger.RemoveListener(Signals.CHARACTER_MARKED, ReceivedMarkedCharacterSignal);
        TriggerDragonAttack();
    }
    private void TriggerDragonAttack() {
        DragonAttack dragonAttack = EventManager.Instance.AddNewEvent(GAME_EVENT.DRAGON_ATTACK) as DragonAttack;
        dragonAttack.Initialize(PlayerManager.Instance.player.markedCharacter, monster.party);
    }
    private void MonsterWillSleep() {
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_MARKED)) {
            Messenger.RemoveListener(Signals.CHARACTER_MARKED, ReceivedMarkedCharacterSignal);
        }
        monster.TryToSleep();
    }
}
