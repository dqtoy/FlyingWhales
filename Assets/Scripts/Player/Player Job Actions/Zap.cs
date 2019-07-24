using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zap : PlayerJobAction {

    private int _zapDuration;
    public Zap() : base(INTERVENTION_ABILITY.ZAP) {
        description = "Temporarily prevents a character from moving.";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.CHARACTER };
        abilityTags.Add(ABILITY_TAG.MAGIC);
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        List<Character> targets = new List<Character>();
        if (targetPOI is Character) {
            targets.Add(targetPOI as Character);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) { targets.AddRange(to.users); }
        } else {
            return;
        }
        if (targets.Count > 0) {
            for (int i = 0; i < targets.Count; i++) {
                Character currTarget = targets[i];
                if (CanPerformActionTowards(assignedCharacter, currTarget)) {
                    Trait newTrait = new Zapped();
                    newTrait.OverrideDuration(_zapDuration);
                    currTarget.AddTrait(newTrait);
                    if (UIManager.Instance.characterInfoUI.isShowing) {
                        UIManager.Instance.characterInfoUI.UpdateThoughtBubble();
                    }
                    GameManager.Instance.CreateElectricEffectAt(currTarget);

                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, "zapped", LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotification(log);
                }
            }
            base.ActivateAction(assignedCharacter, targets[0]);
        }
    }
    protected override bool CanPerformActionTowards(Character character, IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    bool canTarget = CanPerformActionTowards(character, currUser);
                    if (canTarget) { return true; }
                }
            }
        }
        return false;
    }
    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Zapped") != null) {
            return false;
        }
        if (character.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    if (currUser != null) {
                        bool canTarget = CanTarget(currUser);
                        if (canTarget) { return true; }
                    }
                }
            }
        }
        return false;
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if(lvl == 1) {
            _zapDuration = 3;
        }else if (lvl == 2) {
            _zapDuration = 6;
        }else if (lvl == 3) {
            _zapDuration = 9;
        }
    }
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (!targetCharacter.IsInOwnParty()) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Zapped") != null) {
            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return true;
    }
}
