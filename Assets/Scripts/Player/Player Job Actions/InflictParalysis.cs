using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictParalysis : PlayerJobAction {
    public InflictParalysis() : base(INTERVENTION_ABILITY.INFLICT_PARALYSIS) {
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER, JOB_ACTION_TARGET.TILE_OBJECT };
        //abilityTags.Add(ABILITY_TAG.PHYSICAL);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
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
                if (CanPerformActionTowards(currTarget)) {
                    Trait newTrait = new Paralyzed();
                    newTrait.SetLevel(level);
                    currTarget.AddTrait(newTrait);
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(newTrait, newTrait.name, LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotification(log);
                }
            }
            base.ActivateAction(targets[0]);
        }
    }
    protected override bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    bool canTarget = CanPerformActionTowards(currUser);
                    if (canTarget) { return true; }
                }
            }
        }
        return false;
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Paralyzed") != null) {
            return false;
        }
        //if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
        //    return false;
        //}
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character, ref hoverText);
        } else if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.users != null) {
                for (int i = 0; i < to.users.Length; i++) {
                    Character currUser = to.users[i];
                    if (currUser != null) {
                        bool canTarget = CanTarget(currUser, ref hoverText);
                        if (canTarget) { return true; }
                    }
                }
            }
        }
        return false;
    }
    #endregion

    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.GetNormalTrait("Paralyzed") != null) {
            return false;
        }
        //if (targetCharacter.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
        //    return false;
        //}
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}

public class InflictParalysisData : PlayerJobActionData {
    public override string name { get { return "Inflict Paralysis"; } }
    public override string description { get { return "Makes a character unable to move permanently."; } }
}
