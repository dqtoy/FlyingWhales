using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : PlayerJobAction {

    public Destroy() : base(INTERVENTION_ABILITY.DESTROY) {
        //description = "Remove this object from the world.";
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE_OBJECT };
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject)) {
            return;
        }
        targetPOI.gridTileLocation.structure.RemovePOI(targetPOI);
        base.ActivateAction(targetPOI);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "destroyed", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }

    //protected override bool ShouldButtonBeInteractable(Character character, IPointOfInterest targetPOI) {
    //    if (!targetPOI.IsAvailable()) {
    //        return false;
    //    }
    //    return base.ShouldButtonBeInteractable(character, targetPOI);
    //}
    public override bool CanTarget(IPointOfInterest targetPOI, ref string hoverText) {
        if (!(targetPOI is TileObject)) {
            return false;
        }
        if (targetPOI.gridTileLocation == null) {
            return false;
        }
        return base.CanTarget(targetPOI, ref hoverText);
    }
}
