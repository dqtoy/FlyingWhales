using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class Spoil : PlayerSpell {

    public Spoil() : base(SPELL_TYPE.SPOIL) {
        tier = 3;
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE_OBJECT };
    }

    #region Overrides
    public override void ActivateAction(IPointOfInterest targetPOI) {
        if (targetPOI is TileObject) {
            base.ActivateAction(targetPOI);
            Poisoned poison = new Poisoned();
            poison.SetLevel(level);
            targetPOI.traitContainer.AddTrait(targetPOI, poison);
            Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", name, "activated");
            PlayerManager.Instance.player.ShowNotification(log);
        }
    }
    public override bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi is Table && !(poi.traitContainer.HasTrait("Poisoned", "Robust"))) {
            return true;
        }
        return false;
    }
    protected override bool CanPerformActionTowards(IPointOfInterest targetPOI) {
        if (targetPOI is Table && !(targetPOI.traitContainer.HasTrait("Poisoned", "Robust"))) {
            return true;
        }
        return false;
    }
    #endregion
}

public class SpoilData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.SPOIL;
    public override string name { get { return "Spoil"; } }
    public override string description { get { return "Poison the food at the target table."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }


    #region Overrides
    public override void ActivateAbility(IPointOfInterest targetPOI) {
        targetPOI.traitContainer.AddTrait(targetPOI, "Poisoned");
        Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", name, "activated");
        PlayerManager.Instance.player.ShowNotification(log);
    }
    public override bool CanPerformAbilityTowards(TileObject tileObject) {
        if (tileObject.gridTileLocation == null || tileObject.traitContainer.HasTrait("Poisoned", "Robust")) {
            return false;
        }
        return base.CanPerformAbilityTowards(tileObject);
    }
    // public override bool CanPerformAbilityTowards(SpecialToken item) {
    //     if (item.gridTileLocation == null || item.traitContainer.HasTrait("Poisoned", "Robust")) {
    //         return false;
    //     }
    //     return base.CanPerformAbilityTowards(item);
    // }
    #endregion
}