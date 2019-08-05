using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Explosion : PlayerJobAction {

    private int radius;

    public Explosion() : base(INTERVENTION_ABILITY.EXPLOSION) {
        description = "Destroy objects and structures within a huge radius and significantly damage characters within.";
        SetDefaultCooldownTime(24);
        targetType = JOB_ACTION_TARGET.TILE;
        radius = 1;
    }

    #region Overrides
    public override void ActivateAction(Character assignedCharacter, LocationGridTile targetTile) {
        base.ActivateAction(assignedCharacter, targetTile);
        List<ITraitable> flammables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.parentAreaMap.GetTilesInRadius(targetTile, radius, 0, true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            flammables.AddRange(tile.GetAllTraitablesOnTileWithTrait("Flammable"));
        }
        flammables = flammables.Where(x => x.GetNormalTrait("Burning", "Burnt", "Wet") == null).ToList();
        for (int i = 0; i < flammables.Count; i++) {
            ITraitable flammable = flammables[i];
            if (flammable is Character) {
                Character character = flammable as Character;
                character.AdjustHP(-(int)(character.maxHP * 0.02f));
            }
            if (Random.Range(0, 100) < 25) {
                flammable.AddTrait("Burning");
            }
        }
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        radius++;
    }
    #endregion
}
