using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : Summon {

	public Wolf() : base(SUMMON_TYPE.Wolf, CharacterRole.BEAST, RACE.WOLF, Utilities.GetRandomGender()) { }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, tile.parentAreaMap.area);
        state.SetIsUnending(true);
    }
    #endregion
}
