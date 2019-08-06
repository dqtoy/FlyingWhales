using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CorruptVampiric : PlayerJobAction {

    public CorruptVampiric() : base(INTERVENTION_ABILITY.INFLICT_VAMPIRISM) {
        description = "Makes a character have uncontrollable urge to drink blood for sustenance.";
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER, JOB_ACTION_TARGET.TILE_OBJECT };
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
                    Trait newTrait = new Vampiric();
                    newTrait.SetLevel(level);
                    currTarget.AddTrait(newTrait);
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(newTrait, newTrait.name, LOG_IDENTIFIER.STRING_1);
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
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Vampiric", "Cannibal") != null) {
            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
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
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.race == RACE.SKELETON) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Vampiric", "Cannibal") != null) {

            return false;
        }
        if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        return true;
    }
}
