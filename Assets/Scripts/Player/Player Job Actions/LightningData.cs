using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class LightningData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.LIGHTNING;
    public override string name { get { return "Lightning"; } }
    public override string description { get { return "Lightning Strike"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;

    public LightningData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        List<IPointOfInterest> pois = targetTile.GetPOIsOnTile();
        for (int i = 0; i < pois.Count; i++) {
            pois[i].AdjustHP(-100, ELEMENTAL_TYPE.Electric);
        }
    }
    private void CreateLightningAt(LocationGridTile tile) {
        //TODO
        // GameObject meteorGO = InnerMapManager.Instance.mapObjectFactory.CreateNewMeteorObject();
        // meteorGO.transform.SetParent(tile.parentMap.structureParent);
        // meteorGO.transform.position = tile.centeredWorldLocation;
        // meteorGO.GetComponent<MeteorVisual>().MeteorStrike(tile, abilityRadius);
    }
}
