using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Provoke : PlayerJobAction {

    public Provoke() : base(INTERVENTION_ABILITY.PROVOKE) {
        description = "Makes a character undermine his/her enemies.";
        tier = 2;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.CHARACTER };
        //abilityTags.Add(ABILITY_TAG.MAGIC);
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return;
        }
        Character targetCharacter = targetPOI as Character;
        PlayerUI.Instance.OpenProvoke(targetCharacter);
        base.ActivateAction(targetCharacter);
    }

    protected override bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.faction.id == FactionManager.Instance.neutralFaction.id) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Unconscious") != null) {
            return false;
        }
        return base.CanPerformActionTowards(targetCharacter);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is Character)) {
            return false;
        }
        Character targetCharacter = targetPOI as Character;
        if (targetCharacter.isDead) {
            return false;
        }
        if (targetCharacter.role.roleType == CHARACTER_ROLE.BEAST || targetCharacter.faction.id == FactionManager.Instance.neutralFaction.id) {
            return false;
        }
        if (targetCharacter.GetNormalTrait("Unconscious") != null) {
            return false;
        }
        return base.CanTarget(targetCharacter);
    }
}
