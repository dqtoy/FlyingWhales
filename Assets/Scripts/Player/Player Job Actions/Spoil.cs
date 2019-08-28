using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spoil : PlayerJobAction {

    public Spoil() : base(INTERVENTION_ABILITY.SPOIL) {
        description = "Poison the food at the target table.";
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
            targetPOI.AddTrait(poison);
            Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", this.GetType().ToString(), "activated");
            PlayerManager.Instance.player.ShowNotification(log);
        }
    }
    public override bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi is Table && poi.GetNormalTrait("Poisoned") == null) {
            return true;
        }
        return false;
    }
    protected override bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is Table && targetPOI.GetNormalTrait("Poisoned") == null) {
            return true;
        }
        return false;
    }
    #endregion
}