using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ignite : PlayerJobAction {

    private List<LocationGridTile> highlightedTiles;

    public Ignite() : base(INTERVENTION_ABILITY.IGNITE) {
        description = "Targets a spot. Target will ignite and start spreading fire.";
        SetDefaultCooldownTime(24);
        targetType = JOB_ACTION_TARGET.TILE;
        lvl = 2;
        highlightedTiles = new List<LocationGridTile>();
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, LocationGridTile targetTile) {
        base.ActivateAction(assignedCharacter, targetTile);
        List<LocationGridTile> tiles = GetTargetTiles(targetTile);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.AddTrait("Burning");
        }
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
         if (lvl == 2) {
            tiles.AddRange(origin.FourNeighbours());
        } else if (lvl >= 3) {
            tiles.AddRange(origin.neighbourList);
        }
        return tiles.Where(x => x.GetNormalTrait("Burning", "Burnt", "Wet") == null && x.GetNormalTrait("Flammable") != null).ToList();
    }
}
