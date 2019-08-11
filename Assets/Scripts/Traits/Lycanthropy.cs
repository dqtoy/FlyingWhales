using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lycanthropy : Trait {

    private Character _character;
    public LycanthropyData data { get; private set; }

    public override bool isPersistent { get { return true; } }

    private int _level;
    public Lycanthropy() {
        name = "Lycanthropy";
        description = "This character can transform into a wolf.";
        thoughtText = "[Character] can transform into a wolf.";
        type = TRAIT_TYPE.SPECIAL;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        //effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
    }

    #region Overrides
    public override void OnAddTrait(ITraitable sourceCharacter) {
        if (sourceCharacter is Character) {
            _character = sourceCharacter as Character;
            data = new LycanthropyData();
            //_character.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, name);
            AlterEgoData lycanthropeAlterEgo = _character.CreateNewAlterEgo("Lycanthrope");

            //setup all alter ego data
            lycanthropeAlterEgo.SetFaction(FactionManager.Instance.neutralFaction);
            lycanthropeAlterEgo.SetRace(RACE.WOLF);
            lycanthropeAlterEgo.SetRole(CharacterRole.BEAST);
            lycanthropeAlterEgo.SetCharacterClass(CharacterManager.Instance.CreateNewCharacterClass(Utilities.GetRespectiveBeastClassNameFromByRace(RACE.WOLF)));
            lycanthropeAlterEgo.SetLevel(level);
            foreach (List<LocationStructure> structures in _character.specificLocation.structures.Values) {
                for (int i = 0; i < structures.Count; i++) {
                    for (int j = 0; j < structures[i].pointsOfInterest.Count; j++) {
                        IPointOfInterest poi = structures[i].pointsOfInterest[j];
                        if (poi is TileObject) {
                            TileObject tileObj = poi as TileObject;
                            if (tileObj.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL || tileObj.tileObjectType == TILE_OBJECT_TYPE.EDIBLE_PLANT) {
                                lycanthropeAlterEgo.AddAwareness(tileObj);
                            }
                        }
                    }
                }
            }
        }

        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
        _character.RemoveAlterEgo("Lycanthrope");
        _character = null;
        base.OnRemoveTrait(sourceCharacter, removedBy);
    }
    #endregion

    public void PlanTransformToWolf() {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.TRANSFORM_TO_WOLF, _character, _character);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
        goapPlan.ConstructAllNodes();
        _character.AddPlan(goapPlan, true);
    }
    public void PlanRevertToNormal() {
        GoapAction goapAction = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.REVERT_TO_NORMAL, _character, _character);
        GoapNode goalNode = new GoapNode(null, goapAction.cost, goapAction);
        GoapPlan goapPlan = new GoapPlan(goalNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.NONE }, GOAP_CATEGORY.REACTION);
        goapPlan.ConstructAllNodes();
        _character.AddPlan(goapPlan, true);
    }
    public void TurnToWolf() {
        ////Drop all plans except for the current action
        //_character.AdjustIsWaitingForInteraction(1);
        //_character.DropAllPlans(_character.currentAction.parentPlan);
        //_character.AdjustIsWaitingForInteraction(-1);

        ////Copy non delicate data
        //data.SetData(_character);

        //_character.SetHomeStructure(null);

        ////Reset needs
        //_character.ResetFullnessMeter();
        //_character.ResetHappinessMeter();
        //_character.ResetTirednessMeter();


        ////Remove all awareness then add all edible plants and small animals of current location to awareness
        //_character.awareness.Clear();
        //foreach (List<LocationStructure> structures in _character.specificLocation.structures.Values) {
        //    for (int i = 0; i < structures.Count; i++) {
        //        for (int j = 0; j < structures[i].pointsOfInterest.Count; j++) {
        //            IPointOfInterest poi = structures[i].pointsOfInterest[j];
        //            if(poi is TileObject) {
        //                TileObject tileObj = poi as TileObject;
        //                if(tileObj.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL || tileObj.tileObjectType == TILE_OBJECT_TYPE.EDIBLE_PLANT) {
        //                    _character.AddAwareness(tileObj);
        //                }
        //            }
        //        }
        //    }
        //}

        ////Copy relationship data then remove them
        ////data.SetRelationshipData(_character);
        ////_character.RemoveAllRelationships(false);
        //foreach (Character target in _character.relationships.Keys) {
        //    CharacterManager.Instance.SetIsDisabledRelationshipBetween(_character, target, true);
        //}

        ////Remove race and class
        ////This is done first so that when the traits are copied, it will not copy the traits from the race and class because if it is copied and the race and character is brought back, it will be doubled, which is not what we want
        //_character.RemoveRace();
        //_character.RemoveClass();

        ////Copy traits and then remove them
        //data.SetTraits(_character);
        //_character.RemoveAllNonRelationshipTraits("Lycanthropy");

        ////Change faction and race
        //_character.ChangeFactionTo(FactionManager.Instance.neutralFaction);
        //_character.SetRace(RACE.WOLF);

        ////Change class and role
        //_character.AssignRole(CharacterRole.BEAST);
        //_character.AssignClassByRole(_character.role);

        //Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, _character);

        //_character.CancelAllJobsTargettingThisCharacter("target is not found", false);
        //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, _character, "target is not found");

        _character.SwitchAlterEgo("Lycanthrope");
        //Plan idle stroll to the wilderness
        LocationStructure wilderness = _character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
        LocationGridTile targetTile = wilderness.GetRandomTile();
        _character.PlanIdleStroll(wilderness, targetTile);
    }

    public void RevertToNormal() {
        ////Drop all plans except for the current action
        //_character.AdjustIsWaitingForInteraction(1);
        //_character.DropAllPlans(_character.currentAction.parentPlan);
        //_character.AdjustIsWaitingForInteraction(-1);

        ////Revert back data including awareness
        //_character.SetFullness(data.fullness);
        //_character.SetTiredness(data.tiredness);
        //_character.SetHappiness(data.happiness);
        //_character.CopyAwareness(data.awareness);
        //_character.SetHomeStructure(data.homeStructure);
        //_character.ChangeFactionTo(data.faction);
        //_character.ChangeRace(data.race);
        //_character.AssignRole(data.role);
        //_character.AssignClass(data.characterClass);

        ////Bring back lost relationships
        //foreach (Character target in _character.relationships.Keys) {
        //    CharacterManager.Instance.SetIsDisabledRelationshipBetween(_character, target, false);
        //}

        ////Revert back the traits
        //for (int i = 0; i < data.traits.Count; i++) {
        //    _character.AddTrait(data.traits[i]);
        //}

        _character.SwitchAlterEgo(CharacterManager.Original_Alter_Ego);
    }
}

public class LycanthropyData {
    public int fullness { get; private set; }
    public int tiredness { get; private set; }
    public int happiness { get; private set; }
    public Faction faction { get; private set; }
    public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; private set; }
    //public List<RelationshipLycanthropyData> relationships { get; private set; }
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
        this.awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>>(character.awareness);
        this.homeStructure = character.homeStructure;
        this.race = character.race;
        this.role = character.role;
        this.characterClass = character.characterClass;
    }

    //public void SetRelationshipData(Character character) {
    //    this.relationships = new List<RelationshipLycanthropyData>();
    //    foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in character.relationships) {
    //        this.relationships.Add(new RelationshipLycanthropyData(kvp.Key, kvp.Value, kvp.Key.GetCharacterRelationshipData(character)));
    //    }
    //}
    //public void SetTraits(Character character) {
    //    this.traits = new List<Trait>();
    //    for (int i = 0; i < character.allTraits.Count; i++) {
    //        if(character.allTraits[i].name != "Lycanthropy" && !(character.allTraits[i] is RelationshipTrait)) {
    //            this.traits.Add(character.allTraits[i]);
    //        }
    //    }
    //}
}

//public class RelationshipLycanthropyData {
//    public Character target { get; private set; }
//    public CharacterRelationshipData characterToTargetRelData { get; private set; }
//    public CharacterRelationshipData targetToCharacterRelData { get; private set; }

//    public RelationshipLycanthropyData(Character target, CharacterRelationshipData characterToTargetRelData, CharacterRelationshipData targetToCharacterRelData) {
//        this.target = target;
//        this.characterToTargetRelData = characterToTargetRelData;
//        this.targetToCharacterRelData = targetToCharacterRelData;
//    }
//}
