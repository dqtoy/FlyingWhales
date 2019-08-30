using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Release : PlayerJobAction {

    public Release() : base(INTERVENTION_ABILITY.RELEASE) {
        tier = 3;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER };
        //abilityTags.Add(ABILITY_TAG.MAGIC);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is Character) {
            Character target = targetPOI as Character;

            target.RemoveTrait("Restrained");

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_released_character");
            log.AddToFillers(target, target.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);

            base.ActivateAction(target);
        }
    }
    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Restrained") == null) {
            return false;
        }
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (targetPOI is Character) {
            return CanTarget(targetPOI as Character, ref hoverText);
        }
        return false;
    }
    #endregion

    private bool CanTarget(Character targetCharacter, ref string hoverText) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Restrained") == null) {
            return false;
        }
        return base.CanTarget(targetCharacter, ref hoverText);
    }
}

public class ReleaseData : PlayerJobActionData {
    public override string name { get { return "Release"; } }
    public override string description { get { return "Release a prisoner from captivity."; } }
}
