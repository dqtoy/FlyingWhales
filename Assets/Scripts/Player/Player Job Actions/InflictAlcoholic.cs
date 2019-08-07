using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InflictAlcoholic : PlayerJobAction {

    public InflictAlcoholic() : base(INTERVENTION_ABILITY.INFLICT_ALCOHOLIC) {
        description = "Makes a character often want to drink.";
        tier = 3;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER, JOB_ACTION_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, IPointOfInterest poi) {
        List<Character> targets = new List<Character>();
        if (poi is Character) {
            targets.Add(poi as Character);
        } else if (poi is TileObject) {
            TileObject to = poi as TileObject;
            if (to.users != null) { targets.AddRange(to.users); }
        } else {
            return;
        }
        if (targets.Count > 0) {
            for (int i = 0; i < targets.Count; i++) {
                Character currTarget = targets[i];
                if (CanPerformActionTowards(assignedCharacter, currTarget)) {
                    currTarget.AddTrait("Alcoholic");
                    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_afflicted");
                    log.AddToFillers(currTarget, currTarget.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    log.AddToFillers(null, "Alcoholic", LOG_IDENTIFIER.STRING_1);
                    log.AddLogToInvolvedObjects();
                    PlayerManager.Instance.player.ShowNotification(log);
                }
            }
            base.ActivateAction(assignedCharacter, targets[0]);
        }        
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
    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) { //|| (!targetCharacter.isTracked && !GameManager.Instance.inspectAll)
            return false;
        }
        if (targetCharacter.GetNormalTrait("Alcoholic") == null) {
            return true;
        }
        return false;
    }
    protected override bool CanPerformActionTowards(Character character, Character targetPOI) {
        if (targetPOI.isDead) {
            return false;
        }
        if (targetPOI is Character && targetPOI.GetNormalTrait("Alcoholic") == null) {
            return true;
        }
        return false;
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
    #endregion
}