using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Disable : PlayerJobAction {

    public Disable() {
        name = "Disable";
        SetDefaultCooldownTime(24);
        targettableTypes = new List<JOB_ACTION_TARGET>() { JOB_ACTION_TARGET.TILE_OBJECT };
    }

    public override void ActivateAction(Character assignedCharacter, IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject)) {
            return;
        }
        targetPOI.SetIsDisabledByPlayer(true);
        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
        SchedulingManager.Instance.AddEntry(dueDate, () => targetPOI.SetIsDisabledByPlayer(false));
        base.ActivateAction(assignedCharacter, targetPOI);

        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "player_intervention");
        log.AddToFillers(targetPOI, targetPOI.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        log.AddToFillers(null, "disabled", LOG_IDENTIFIER.STRING_1);
        log.AddLogToInvolvedObjects();
        PlayerManager.Instance.player.ShowNotification(log);
    }

    protected override bool CanPerformActionTowards(Character character, IPointOfInterest targetPOI) {
        if (targetPOI.isDisabledByPlayer) {
            return false;
        }
        return base.CanPerformActionTowards(character, targetPOI);
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (!(targetPOI is TileObject)) {
            return false;
        }
        if(targetPOI.gridTileLocation == null) {
            return false;
        }
        return base.CanTarget(targetPOI);
    }
}
