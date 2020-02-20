﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class Meteor : PlayerSpell {

    private int abilityRadius;

    public Meteor() : base(SPELL_TYPE.METEOR) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        abilityRadius = 1;
        tier = 1;
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        CreateMeteorStrikeAt(targetTile);
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        abilityRadius++;
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, 0, true);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, 0, true);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
    #endregion

    private void CreateMeteorStrikeAt(LocationGridTile tile) {
        GameObject meteorGO = InnerMapManager.Instance.mapObjectFactory.CreateNewMeteorObject();
        meteorGO.transform.SetParent(tile.parentMap.structureParent);
        meteorGO.transform.position = tile.centeredWorldLocation;
        meteorGO.GetComponent<MeteorVisual>().MeteorStrike(tile, abilityRadius);
    }
}

public class MeteorData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.METEOR;
    public override string name { get { return "Meteor"; } }
    public override string description { get { return "Destroy objects and structures within a huge radius and significantly damage characters within."; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;

    public MeteorData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        CreateMeteorStrikeAt(targetTile);
    }
    private void CreateMeteorStrikeAt(LocationGridTile tile) {
        GameObject meteorGO = InnerMapManager.Instance.mapObjectFactory.CreateNewMeteorObject();
        meteorGO.transform.SetParent(tile.parentMap.structureParent);
        meteorGO.transform.position = tile.centeredWorldLocation;
        meteorGO.GetComponent<MeteorVisual>().MeteorStrike(tile, abilityRadius);
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, 0, true);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(abilityRadius, 0, true);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
}