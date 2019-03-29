using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lycanthropy : Trait {

    private Character _character;
    private LycanthropyData _data;

    public Lycanthropy() {
        name = "Lycanthropy";
        description = "This character can transform into a wolf.";
        thoughtText = "[Character] can transform into a wolf.";
        type = TRAIT_TYPE.ABILITY;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_SEVERITY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        _character = sourceCharacter as Character;
        _data = new LycanthropyData();
        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        _character = null;
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    public void TurnToWolf() {
        //Refill all needs meters to full and immediately end sleep, store previous needs values somewhere
        _character.AdjustIsWaitingForInteraction(1);
        _character.DropAllPlans();
        _character.AdjustIsWaitingForInteraction(-1);

        _data.SetData(_character);

        _character.ResetFullnessMeter();
        _character.ResetHappinessMeter();
        _character.ResetTirednessMeter();
        _character.ChangeRace(RACE.WOLF);

        _data.SetRelationshipData(_character);
        _character.RemoveAllRelationships();

        _data.SetTraits(_character);

        _character.PlanIdleStroll(_character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));


        //Change race to Wolf
    }
}

public class LycanthropyData {
    public int fullness { get; private set; }
    public int tiredness { get; private set; }
    public int happiness { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness { get; private set; }
    public Dictionary<Character, CharacterRelationshipData> relationships { get; private set; }
    public List<Trait> traits { get; set; }
    public LocationStructure homeStructure { get; private set; }
    public RACE race { get; private set; }

    public void SetData(Character character) {
        this.fullness = character.fullness;
        this.tiredness = character.tiredness;
        this.happiness = character.happiness;
        this.awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>>(character.awareness);
        this.homeStructure = character.homeStructure;
        this.race = character.race;
    }
    public void SetRelationshipData(Character character) {
        this.relationships = new Dictionary<Character, CharacterRelationshipData>(character.relationships);
    }
    public void SetTraits(Character character) {
        this.traits = new List<Trait>(character.traits);
    }
}
