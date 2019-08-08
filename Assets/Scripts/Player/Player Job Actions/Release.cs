using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Release : PlayerJobAction {

    public Release() : base(INTERVENTION_ABILITY.RELEASE) {
        description = "Release a prisoner from captivity.";
        tier = 3;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER };
        abilityTags.Add(ABILITY_TAG.MAGIC);
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character target = targetPOI as Character;

            target.RemoveTrait("Restrained");

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_released_character");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);

            base.ActivateAction(assignedCharacter, target);
        }
    }
    protected override bool CanPerformActionTowards(Character character, Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Restrained") == null) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character);
        }
        return false;
    }
    #endregion

    private bool CanTarget(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Restrained") == null) {
            return false;
        }
        return true;
    }
}
