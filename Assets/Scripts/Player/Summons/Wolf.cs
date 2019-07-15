using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Summon {

	public Wolf() : base(SUMMON_TYPE.WOLF, CharacterRole.BEAST, RACE.WOLF, Utilities.GetRandomGender()) { }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, tile.parentAreaMap.area, 1000);
    }
    #endregion
}
