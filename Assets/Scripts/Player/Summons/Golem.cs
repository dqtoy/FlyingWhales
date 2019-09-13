using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Golem : Summon {

    public Golem() : base(SUMMON_TYPE.Golem, CharacterRole.SOLDIER, RACE.ABOMINATION, Utilities.GetRandomGender()) {
        SetMaxHPMod(1000);
    }
    public Golem(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override string GetClassForRole(CharacterRole role) {
        return "Barbarian"; //all golems are barbarians
    }
    public override void OnPlaceSummon(LocationGridTile tile) {
        base.OnPlaceSummon(tile);
        //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, tile.parentAreaMap.area);
        //state.SetIsUnending(true);
        GoToWorkArea();
    }
    //protected override void IdlePlans() {
    //    base.IdlePlans();
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    protected override void OnSeenBy(Character character) {
        base.OnSeenBy(character);
        if (HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return;
        }
        if (!character.IsHostileWith(this)) {
            return;
        }
        //add taunted trait to the character
        character.AddTrait(new Taunted(), this);
    }
    #endregion
}