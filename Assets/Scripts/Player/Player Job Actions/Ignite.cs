using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ignite : PlayerJobAction {

    private List<LocationGridTile> highlightedTiles;

    public Ignite() : base(INTERVENTION_ABILITY.IGNITE) {
        description = "Targets a spot. Target will ignite and start spreading fire.";
        tier = 1;
        SetDefaultCooldownTime(24);
        targetTypes = new JOB_ACTION_TARGET[] { JOB_ACTION_TARGET.TILE };
        highlightedTiles = new List<LocationGridTile>();
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
}
