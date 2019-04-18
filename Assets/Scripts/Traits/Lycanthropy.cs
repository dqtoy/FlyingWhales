using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lycanthropy : Trait {

    private Character _character;
    public LycanthropyData data { get; private set; }

    public Lycanthropy() {
        name = "Lycanthropy";
        description = "This character can transform into a wolf.";
        thoughtText = "[Character] can transform into a wolf.";
        type = TRAIT_TYPE.STATUS;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        _character = sourceCharacter as Character;
        data = new LycanthropyData();
        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        _character = null;
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    public void PlanTransformToWolf() {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.TRANSFORM_TO_WOLF, _character, _character);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
        goapPlan.ConstructAllNodes();
        _character.allGoapPlans.Add(goapPlan);
    }
    public void PlanRevertToNormal() {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.REVERT_TO_NORMAL, _character, _character);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
        goapPlan.ConstructAllNodes();
        _character.allGoapPlans.Add(goapPlan);
    }
    public void TurnToWolf() {
        //Drop all plans except for the current action
        _character.AdjustIsWaitingForInteraction(1);
        _character.DropAllPlans(_character.currentAction.parentPlan);
        _character.AdjustIsWaitingForInteraction(-1);

        //Copy non delicate data
        data.SetData(_character);

        _character.SetHomeStructure(null);

        //Reset needs
        _character.ResetFullnessMeter();
        _character.ResetHappinessMeter();
        _character.ResetTirednessMeter();


        //Only retain awareness of characters, small animals, and edible plants, all other awareness must be deleted
        if (_character.awareness.ContainsKey(POINT_OF_INTEREST_TYPE.ITEM)) {
            _character.awareness.Remove(POINT_OF_INTEREST_TYPE.ITEM);
        }
        if (_character.awareness.ContainsKey(POINT_OF_INTEREST_TYPE.TILE_OBJECT)) {
            for (int i = 0; i < _character.awareness[POINT_OF_INTEREST_TYPE.TILE_OBJECT].Count; i++) {
                TileObjectAwareness toa = _character.awareness[POINT_OF_INTEREST_TYPE.TILE_OBJECT][i] as TileObjectAwareness;
                if(toa.tileObject.tileObjectType != TILE_OBJECT_TYPE.SMALL_ANIMAL && toa.tileObject.tileObjectType != TILE_OBJECT_TYPE.EDIBLE_PLANT) {
                    _character.RemoveAwareness(toa.poi);
                    i--;
                }
            }
        }
        //Copy relationship data then remove them
        data.SetRelationshipData(_character);
        _character.RemoveAllRelationships();

        //Remove race and class
        //This is done first so that when the traits are copied, it will not copy the traits from the race and class because if it is copied and the race and character is brought back, it will be doubled, which is not what we want
        _character.RemoveRace();
        _character.RemoveClass();

        //Copy traits and then remove them
        data.SetTraits(_character);
        _character.RemoveAllTraits("Lycanthropy");

        //Change faction and race
        _character.ChangeFactionTo(FactionManager.Instance.neutralFaction);
        _character.SetRace(RACE.WOLF);

        //Change class and role
        _character.AssignRole(CharacterRole.BEAST);
        _character.AssignClassByRole(_character.role);

        Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, _character);

        //Plan idle stroll to the wilderness
        _character.PlanIdleStroll(_character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
    }

    public void RevertToNormal() {
        //Drop all plans except for the current action
        _character.AdjustIsWaitingForInteraction(1);
        _character.DropAllPlans(_character.currentAction.parentPlan);
        _character.AdjustIsWaitingForInteraction(-1);

        //Revert back data including awareness
        _character.SetFullness(data.fullness);
        _character.SetTiredness(data.tiredness);
        _character.SetHappiness(data.happiness);
        _character.CopyAwareness(data.awareness);
        _character.SetHomeStructure(data.homeStructure);
        _character.ChangeFactionTo(data.faction);
        _character.ChangeRace(data.race);
        _character.AssignRole(data.role);
        _character.AssignClass(data.characterClass);

        //Bring back lost relationships
        _character.ReEstablishRelationships(data.relationships);

        //Revert back the traits
        for (int i = 0; i < data.traits.Count; i++) {
            _character.AddTrait(data.traits[i]);
        }
    }
}

public class LycanthropyData {
    public int fullness { get; private set; }
    public int tiredness { get; private set; }
    public int happiness { get; private set; }
    public Faction faction { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>> awareness { get; private set; }
    public List<RelationshipLycanthropyData> relationships { get; private set; }
    public List<Trait> traits { get; set; }
    public Dwelling homeStructure { get; private set; }
    public CharacterClass characterClass { get; private set; }
    public CharacterRole role { get; private set; }
    public RACE race { get; private set; }

    public void SetData(Character character) {
        this.fullness = character.fullness;
        this.tiredness = character.tiredness;
        this.happiness = character.happiness;
        this.faction = character.faction;
        this.awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IAwareness>>(character.awareness);
        this.homeStructure = character.homeStructure;
        this.race = character.race;
        this.role = character.role;
        this.characterClass = character.characterClass;
    }

    public void SetRelationshipData(Character character) {
        this.relationships = new List<RelationshipLycanthropyData>();
        foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in character.relationships) {
            this.relationships.Add(new RelationshipLycanthropyData(kvp.Key, kvp.Value, kvp.Key.GetCharacterRelationshipData(character)));
        }
    }
    public void SetTraits(Character character) {
        this.traits = new List<Trait>(character.traits);
    }
}

public class RelationshipLycanthropyData {
    public Character target { get; private set; }
    public CharacterRelationshipData characterToTargetRelData { get; private set; }
    public CharacterRelationshipData targetToCharacterRelData { get; private set; }

    public RelationshipLycanthropyData(Character target, CharacterRelationshipData characterToTargetRelData, CharacterRelationshipData targetToCharacterRelData) {
        this.target = target;
        this.characterToTargetRelData = characterToTargetRelData;
        this.targetToCharacterRelData = targetToCharacterRelData;
    }
}
