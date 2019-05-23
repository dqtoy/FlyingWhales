using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : PlayerJobAction {

    public Destroy() {
        name = "Destroy";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.TILE_OBJECT };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject)) {
            return;
        }
        targetPOI.gridTileLocation.structure.RemovePOI(targetPOI);
        base.ActivateAction(assignedCharacter, targetPOI);
    }

    //protected override bool ShouldButtonBeInteractable(Character character, IPointOfInterest targetPOI) {
    //    if (!targetPOI.IsAvailable()) {
    //        return false;
    //    }
    //    return base.ShouldButtonBeInteractable(character, targetPOI);
    //}
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject)) {
            return false;
        }
        if (targetPOI.gridTileLocation == null) {
            return false;
        }
        return base.CanTarget(targetPOI);
    }
}
