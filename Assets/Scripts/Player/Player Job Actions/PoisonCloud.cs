using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class PoisonCloud : PlayerSpell {
    

    public PoisonCloud() : base(SPELL_TYPE.POISON_CLOUD) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        PoisonCloudTileObject tornadoTileObject = new PoisonCloudTileObject();
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
    }
    
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 3);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 3);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
    public override bool CanTarget(LocationGridTile tile) {
        return tile.structure != null;
    }
    protected override bool CanPerformActionTowards(LocationGridTile tile) {
        return tile.structure != null;
    }
    #endregion
}

public class PoisonCloudData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.POISON_CLOUD;
    public override string name { get { return "Poison Cloud"; } }
    public override string description { get { return "Spawn a poison cloud that randomly moves around."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;

    public PoisonCloudData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        PoisonCloudTileObject tornadoTileObject = new PoisonCloudTileObject();
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null;
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 3);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = UtilityScripts.GameUtilities.GetDiamondTilesFromRadius(targetTile.parentMap, targetTile.localPlace, 3);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
}
