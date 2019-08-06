using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spoil : PlayerJobAction {

    public Spoil() : base(INTERVENTION_ABILITY.SPOIL) {
        description = "Poison the food at the target table.";
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            base.ActivateAction(assignedCharacter, targetPOI);
            Poisoned poison = new Poisoned();
            poison.SetLevel(lvl);
            targetPOI.AddTrait(poison);
        }
        
    }
    public override bool CanTarget(IPointOfInterest poi) {
        if (poi is Table && poi.GetNormalTrait("Poisoned") == null) {
            return true;
        }
        return false;
    }
    protected override bool CanPerformActionTowards(Character character, IPointOfInterest targetPOI) {
        if (targetPOI is Table && targetPOI.GetNormalTrait("Poisoned") == null) {
            return true;
        }
        return false;
    }
    #endregion
}