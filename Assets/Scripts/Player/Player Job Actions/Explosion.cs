using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;
using Traits;

public class Explosion : PlayerJobAction {

    private int radius;

    public Explosion() : base(INTERVENTION_ABILITY.EXPLOSION) {
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE };
        radius = 1;
        tier = 1;
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        List<ITraitable> flammables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.parentMap.GetTilesInRadius(targetTile, radius, 0, true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            flammables.AddRange(tile.GetTraitablesOnTileWithTrait("Flammable"));
        }
        flammables = flammables.Where(x => x.traitContainer.GetNormalTrait<Trait>("Burning", "Burnt", "Wet", "Fireproof") == null).ToList();
        BurningSource bs = new BurningSource(InnerMapManager.Instance.currentlyShowingArea);
        for (int i = 0; i < flammables.Count; i++) {
            ITraitable flammable = flammables[i];
            if (flammable is TileObject) {
                TileObject obj = flammable as TileObject;
                GameManager.Instance.CreateExplodeEffectAt(obj.gridTileLocation);
                if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                    obj.AdjustHP(-obj.currentHP);
                    if (obj.gridTileLocation == null) {
                        continue; //object was destroyed, do not add burning trait
                    }
                }
            } else if (flammable is SpecialToken) {
                SpecialToken token = flammable as SpecialToken;
                GameManager.Instance.CreateExplodeEffectAt(token.gridTileLocation);
                token.AdjustHP(-token.currentHP);
                if (token.gridTileLocation == null) {
                    continue; //object was destroyed, do not add burning trait
                }
            } else if (flammable is Character) {
                Character character = flammable as Character;
                GameManager.Instance.CreateExplodeEffectAt(character.gridTileLocation);
                character.AdjustHP(-(int)(character.maxHP * 0.4f), true);
            }
            if (Random.Range(0, 100) < 60) {
                Burning burning = new Burning();
                burning.SetSourceOfBurning(bs, flammable);
                flammable.traitContainer.AddTrait(flammable, burning);
            }
        }
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        radius++;
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentMap.GetTilesInRadius(targetTile, radius, 0, true);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentMap.GetTilesInRadius(targetTile, radius, 0, true);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
    #endregion
}

public class ExplosionData : PlayerJobActionData {
    public override INTERVENTION_ABILITY ability => INTERVENTION_ABILITY.EXPLOSION;
    public override string name { get { return "Explosion"; } }
    public override string description { get { return "Destroy objects and structures within a huge radius and significantly damage characters within."; } }
    public override INTERVENTION_ABILITY_CATEGORY category { get { return INTERVENTION_ABILITY_CATEGORY.DEVASTATION; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;
}