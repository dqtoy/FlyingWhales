using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ignite : PlayerJobAction, IBurningSource {

    private List<LocationGridTile> highlightedTiles;

    public List<Character> dousers { get; private set; }

    public Ignite() : base(INTERVENTION_ABILITY.IGNITE) {
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE };
        highlightedTiles = new List<LocationGridTile>();
        dousers = new List<Character>();
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        List<LocationGridTile> tiles = GetTargetTiles(targetTile);
        if (tiles.Count > 0) {
            for (int i = 0; i < tiles.Count; i++) {
                LocationGridTile tile = tiles[i];
                Burning burning = new Burning();
                burning.SetSourceOfBurning(this);
                tile.AddTrait(burning);
            }
            Log log = new Log(GameManager.Instance.Today(), "InterventionAbility", this.GetType().ToString(), "activated");
            PlayerManager.Instance.player.ShowNotification(log);
        }
    }
    public override bool CanTarget(LocationGridTile tile) {
        return GetTargetTiles(tile).Count > 0;
    }
    protected override bool CanPerformActionTowards(LocationGridTile tile) {
        return GetTargetTiles(tile).Count > 0;
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        highlightedTiles = GetTargetTiles(targetTile);
        InteriorMapManager.Instance.HighlightTiles(highlightedTiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        InteriorMapManager.Instance.UnhighlightTiles(highlightedTiles);
        highlightedTiles.Clear();
    }
    #endregion

    private List<LocationGridTile> GetTargetTiles(LocationGridTile origin) {
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        tiles.Add(origin);
         if (level == 2) {
            tiles.AddRange(origin.FourNeighbours());
        } else if (level >= 3) {
            tiles.AddRange(origin.neighbourList);
        }
        return tiles.Where(x => x.GetNormalTrait("Burning", "Burnt", "Wet", "Fireproof") == null && x.GetNormalTrait("Flammable") != null).ToList();
    }

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

public class IgniteData : PlayerJobActionData {
    public override string name { get { return "Ignite"; } }
    public override string description { get { return "Targets a spot. Target will ignite and start spreading fire."; } }
}