using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InviteToMakeLove : GoapAction {

    public InviteToMakeLove(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.INVITE_TO_MAKE_LOVE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Entertain_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, conditionKey = null, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, conditionKey = null, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = null, targetPOI = actor });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character targetCharacter = poiTarget as Character;
        if (!isTargetMissing && targetCharacter.IsInOwnParty()) {
            if (!targetCharacter.isStarving && !targetCharacter.isExhausted
                && targetCharacter.GetTrait("Annoyed") == null && !targetCharacter.HasOtherCharacterInParty()) {
                SetState("Invite Success");
            } else {
                SetState("Invite Fail");
            }
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        TIME_IN_WORDS currentTime = GameManager.GetCurrentTimeInWordsOfTick();
        if (currentTime == TIME_IN_WORDS.EARLY_NIGHT || currentTime == TIME_IN_WORDS.LATE_NIGHT)
        {
            return Utilities.rng.Next(15, 36);
        }
        return Utilities.rng.Next(30, 56);
    }
    #endregion

    #region Effects
    private void PreInviteSuccess() {
        resumeTargetCharacterState = false;
        Character target = poiTarget as Character;
        bool isParamour = actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR);
        List<TileObject> validBeds = new List<TileObject>();
        //if the characters are paramours
        if (isParamour) {
            //check if they have lovers
            bool actorHasLover = actor.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false);
            bool targetHasLover = target.HasRelationshipTraitOf(RELATIONSHIP_TRAIT.LOVER, false);
            //if one of them doesn't have any lovers
            if (!actorHasLover || !targetHasLover) {
                Character characterWithoutLover;
                if (!actorHasLover) {
                    characterWithoutLover = actor;
                } else {
                    characterWithoutLover = target;
                }
                //pick the bed of the character that doesn't have a lover
                validBeds.AddRange(characterWithoutLover.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
            }
            //if both of them have lovers
            else {
                //check if any of their lovers are currently at the structure that their bed is at
                //if they are not, add that bed to the choices
                Character actorLover = actor.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                if (actorLover.currentStructure != actor.homeStructure) {
                    //lover is not at home structure, add bed to valid choices
                    validBeds.AddRange(actor.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
                }
                Character targetLover = target.GetCharacterWithRelationship(RELATIONSHIP_TRAIT.LOVER);
                if (targetLover.currentStructure != target.homeStructure) {
                    //lover is not at home structure, add bed to valid choices
                    validBeds.AddRange(target.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
                }
            }
        } else {
            validBeds.AddRange(actor.homeStructure.GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
        }

        //if no beds are valid from the above logic.
        if (validBeds.Count == 0) {
            //pick a random bed in a structure that is unowned (No residents)
            List<LocationStructure> unownedStructures = actor.homeArea.GetStructuresAtLocation(true).Where(x => (x is Dwelling && (x as Dwelling).residents.Count == 0)
            || x.structureType == STRUCTURE_TYPE.INN).ToList();

            for (int i = 0; i < unownedStructures.Count; i++) {
                validBeds.AddRange(unownedStructures[i].GetTileObjectsOfType(TILE_OBJECT_TYPE.BED));
            }
        }
        IPointOfInterest chosenBed = validBeds[Random.Range(0, validBeds.Count)];

        MakeLove makeLove = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.MAKE_LOVE, actor, chosenBed, false) as MakeLove;
        makeLove.SetTargetCharacter(target);
        makeLove.Initialize();
        GoapNode startNode = new GoapNode(null, makeLove.cost, makeLove);
        GoapPlan makeLovePlan = new GoapPlan(startNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY }, GOAP_CATEGORY.HAPPINESS);
        makeLovePlan.ConstructAllNodes();
        actor.AddPlan(makeLovePlan, true);
        AddTraitTo(target, "Wooed", actor);
        actor.ownParty.AddCharacter(target);
    }
    private void PreInviteFail() {
        //**After Effect 1**: Actor gains Annoyed trait.
        AddTraitTo(actor, "Annoyed", actor);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        Character target = poiTarget as Character;
        if (target == actor) {
            return false;
        }
        if (target.currentAlterEgoName != CharacterManager.Original_Alter_Ego) {
            return false;
        }
        if (target.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
            return false;
        }
        if (!actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.LOVER) && !actor.HasRelationshipOfTypeWith(target, RELATIONSHIP_TRAIT.PARAMOUR)) {
            return false; //only lovers and paramours can make love
        }
        return target.IsInOwnParty();
    }
    #endregion
}
