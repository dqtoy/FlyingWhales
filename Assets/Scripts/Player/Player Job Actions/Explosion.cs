﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : PlayerJobAction, IBurningSource {

    private int radius;

    public List<Character> dousers { get; private set; }

    public Explosion() : base(INTERVENTION_ABILITY.EXPLOSION) {
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE };
        radius = 1;
        tier = 1;
        dousers = new List<Character>();
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        List<ITraitable> flammables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, radius, 0, true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            flammables.AddRange(tile.GetAllTraitablesOnTileWithTrait("Flammable"));
        }
        flammables = flammables.Where(x => x.GetNormalTrait("Burning", "Burnt", "Wet", "Fireproof") == null).ToList();
        for (int i = 0; i < flammables.Count; i++) {
            ITraitable flammable = flammables[i];
            GameManager.Instance.CreateExplodeEffectAt(flammable.gridTileLocation);
            if (flammable is TileObject) {
                TileObject obj = flammable as TileObject;
                obj.gridTileLocation.structure.RemovePOI(obj);
                continue; //go to next item
            } else if (flammable is SpecialToken) {
                SpecialToken token = flammable as SpecialToken;
                token.gridTileLocation.structure.RemovePOI(token);
                continue; //go to next item
            } else if (flammable is Character) {
                Character character = flammable as Character;
                character.AdjustHP(-(int)(character.maxHP * 0.4f), true);
            }
            if (Random.Range(0, 100) < 25) {
                Burning burning = new Burning();
                burning.SetSourceOfBurning(this);
                flammable.AddTrait(burning);
            }
        }
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        radius++;
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, radius, 0, true);
        InteriorMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, radius, 0, true);
        InteriorMapManager.Instance.UnhighlightTiles(tiles);
    }
    #endregion

    #region Burning Source
    public void AddCharactersDousingFire(Character character) {
        dousers.Add(character);
    }
    public void RemoveCharactersDousingFire(Character character) {
        dousers.Remove(character);
    }
    public Character GetNearestDouserFrom(Character otherCharacter) {
        Character nearest = null;
        float nearestDist = 9999f;
        for (int i = 0; i < dousers.Count; i++) {
            Character currDouser = dousers[i];
            float dist = Vector2.Distance(currDouser.gridTileLocation.localLocation, otherCharacter.gridTileLocation.localLocation);
            if (dist < nearestDist) {
                nearest = currDouser;
                nearestDist = dist;
            }
        }
        return nearest;
    }
    #endregion
}

public class ExplosionData : PlayerJobActionData {
    public override string name { get { return "Explosion"; } }
    public override string description { get { return "Destroy objects and structures within a huge radius and significantly damage characters within."; } }
}