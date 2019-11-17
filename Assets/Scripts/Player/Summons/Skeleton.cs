using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A slow-walking undead that may spook civilian NPCs.
/// </summary>
public class Skeleton : Summon {

    public Skeleton() : base(SUMMON_TYPE.Skeleton, CharacterRole.BANDIT, RACE.SKELETON, Utilities.GetRandomGender()) { }
    public Skeleton(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.STROLL, null, tile.parentAreaMap.area);
        //state.SetIsUnending(true);
        GoToWorkArea();
    }
    //protected override void IdlePlans() {
    //    base.IdlePlans();
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    public override List<ActualGoapNode> ThisCharacterSaw(IPointOfInterest target) {
        if (traitContainer.GetNormalTrait("Unconscious", "Resting") != null) {
            return null;
        }
        for (int i = 0; i < traitContainer.allTraits.Count; i++) {
            traitContainer.allTraits[i].OnSeePOI(target, this);
        }
        return null;
    }
    protected override void OnSeenBy(Character character) {
        if (traitContainer.GetNormalTrait("Unconscious", "Resting") != null) {
            return;
        }
        //if (character.role.roleType == CHARACTER_ROLE.CIVILIAN && character.traitContainer.GetNormalTrait("Spooked") == null) {
        //    character.AddTrait("Spooked", this);
        //}
    }
    #endregion
}

