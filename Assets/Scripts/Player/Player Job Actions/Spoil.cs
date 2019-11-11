using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Spoil : PlayerJobAction {

    public Spoil() : base(INTERVENTION_ABILITY.SPOIL) {
        tier = 3;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            base.ActivateAction(targetPOI);
            Poisoned poison = new Poisoned();
            poison.SetLevel(level);
            targetPOI.traitContainer.AddTrait(targetPOI, poison);
            Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", this.GetType().ToString(), "activated");
            PlayerManager.Instance.player.ShowNotification(log);
        }
    }
    public override bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi is Table && !(poi.traitContainer.GetNormalTrait("Poisoned", "Robust") != null)) {
            return true;
        }
        return false;
    }
    protected override bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is Table && !(targetPOI.traitContainer.GetNormalTrait("Poisoned", "Robust") != null)) {
            return true;
        }
        return false;
    }
    #endregion
}

public class SpoilData : PlayerJobActionData {
    public override string name { get { return "Spoil"; } }
    public override string description { get { return "Poison the food at the target table."; } }
    public override INTERVENTION_ABILITY_CATEGORY category { get { return INTERVENTION_ABILITY_CATEGORY.SABOTAGE; } }
}