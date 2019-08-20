using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disable : PlayerJobAction {

    public Disable() : base(INTERVENTION_ABILITY.DISABLE) {
        description = "Prevent characters from using this object for 4 hours.";
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE_OBJECT };
    }

    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject)) {
            return;
        }

        targetPOI.AddTrait(new Disabled());

        //targetPOI.SetIsDisabledByPlayer(true);
        //GameDate dueDate = GameManager.Instance.Today();
        //dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
        //SchedulingManager.Instance.AddEntry(dueDate, () => targetPOI.SetIsDisabledByPlayer(false));
        base.ActivateAction(targetPOI);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "disabled", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }

    protected override bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject) || targetPOI.gridTileLocation == null || targetPOI.isDisabledByPlayer) {
            return false;
        }
        return base.CanPerformActionTowards(targetPOI);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject) || targetPOI.gridTileLocation == null || targetPOI.isDisabledByPlayer) {
            return false;
        }
        return base.CanTarget(targetPOI);
    }
}
