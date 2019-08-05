using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Ignite : PlayerJobAction {

    public Ignite() : base(INTERVENTION_ABILITY.IGNITE) {
        description = "Targets a spot. Target will ignite and start spreading fire.";
        SetDefaultCooldownTime(24);
        targetType = JOB_ACTION_TARGET.TILE;
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, LocationGridTile targetTile) {
        base.ActivateAction(assignedCharacter, targetTile);
        List<LocationGridTile> tiles = new List<LocationGridTile>();
        if (lvl == 1) {
            tiles.Add(targetTile);
        } else if (lvl == 2) {
            tiles.AddRange(targetTile.FourNeighbours());
        } else if (lvl >= 3) {
            tiles.AddRange(targetTile.neighbourList);
        }
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            tile.AddTrait("Burning");
        }
    }
    #endregion
}
