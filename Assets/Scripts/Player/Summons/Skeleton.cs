using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A slow-walking undead that may spook civilian NPCs.
/// </summary>
public class Skeleton : Summon {

    public Skeleton() : base(SUMMON_TYPE.Skeleton, CharacterRole.BANDIT, RACE.SKELETON, Utilities.GetRandomGender()) { }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.STROLL, null, tile.parentAreaMap.area);
        state.SetIsUnending(true);
    }
    public override void ThisCharacterSaw(Character target) {
        if (GetNormalTrait("Unconscious", "Resting") != null) {
            return;
        }
        //NOTE: removed ability of skeletons to watch/witness an event
        Spooked spooked = GetNormalTrait("Spooked") as Spooked;
        if (spooked != null) {
            if (marker.AddAvoidInRange(target)) {
                spooked.AddTerrifyingCharacter(target);
            }
        }
    }
    protected override void OnSeenBy(Character character) {
        if (GetNormalTrait("Unconscious", "Resting") != null) {
            return;
        }
        if (character.role.roleType == CHARACTER_ROLE.CIVILIAN && character.GetNormalTrait("Spooked") == null) {
            character.AddTrait("Spooked", this);
        }
    }
    #endregion
}

