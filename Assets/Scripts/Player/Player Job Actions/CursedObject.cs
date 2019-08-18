using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursedObject : PlayerJobAction {
    public CursedObject() : base(INTERVENTION_ABILITY.CURSED_OBJECT) {
        description = "Put a curse on an object";
        tier = 2;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE_OBJECT };
        abilityTags.Add(ABILITY_TAG.NONE);
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            Trait newTrait = new Cursed();
            newTrait.SetLevel(level);
            targetPOI.AddTrait(newTrait);
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "cursed_object");
            log.AddToFillers(to, to.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);

            base.ActivateAction(targetPOI);
        }
    }
    protected override bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if(to.GetNormalTrait("Cursed") == null){
                return true;
            }
        }
        return false;
    }
    public override bool CanTarget(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            TileObject to = targetPOI as TileObject;
            if (to.GetNormalTrait("Cursed") == null) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
