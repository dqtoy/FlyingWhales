using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuryCharacter : GoapAction {

    public override LocationStructure targetStructure { get { return _targetStructure; } }

    private LocationStructure _targetStructure;
    public BuryCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.BURY_CHARACTER, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION_B;
        actionIconString = GoapActionStateDB.Work_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void SetTargetStructure() {
        //first check if the actor's current location has a cemetery
        _targetStructure = actor.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
        //if the target structure is null, check the actor's home area, if it has a cemetery and use that
        if (_targetStructure == null) {
            _targetStructure = actor.homeArea.GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
        }
        //if the target structure is still null, get a random area that has a cemetery, then target that
        if (_targetStructure == null) {
            List<Area> choices = LandmarkManager.Instance.allAreas.Where(x => x.HasStructure(STRUCTURE_TYPE.CEMETERY)).ToList();
            _targetStructure = choices[Utilities.rng.Next(0, choices.Count)].GetRandomStructureOfType(STRUCTURE_TYPE.CEMETERY);
        }
        base.SetTargetStructure();
    }
    protected override void ConstructPreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.IN_PARTY, targetPOI = poiTarget }, IsInActorParty);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        SetState("Bury Success");
    }
    protected override int GetCost() {
        return 1;
    }
    #endregion

    #region State Effects
    private void PreBurySuccess() {
       
    }
    private void AfterBurySuccess() {
        if (parentPlan.job != null) {
            parentPlan.job.SetCannotCancelJob(true);
        }
        SetCannotCancelAction(true);

        Character targetCharacter = poiTarget as Character;
        //**After Effect 1**: Remove Target from Actor's Party.
        actor.ownParty.RemoveCharacter(targetCharacter, false);
        //**After Effect 2**: Place a Tombstone tile object in adjacent unoccupied tile, link it with Target.
        List<LocationGridTile> choices = actor.gridTileLocation.UnoccupiedNeighbours.Where(x => x.structure == actor.currentStructure).ToList();
        LocationGridTile chosenLocation = choices[Random.Range(0, choices.Count)];
        Tombstone tombstone = new Tombstone(targetCharacter, actor.currentStructure);
        actor.currentStructure.AddPOI(tombstone, chosenLocation);
        List<Character> characters = targetCharacter.GetAllCharactersThatHasRelationship();
        if(characters != null) {
            for (int i = 0; i < characters.Count; i++) {
                characters[i].AddAwareness(tombstone);
            }
        }
        //targetCharacter.CancelAllJobsTargettingThisCharacter("target is already buried", false);
    }
    #endregion

    #region Preconditions
    private bool IsInActorParty() {
        Character target = poiTarget as Character;
        return target.currentParty == actor.currentParty;
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        Character targetCharacter = poiTarget as Character;
        //target character must be dead
        if (!targetCharacter.isDead) {
            return false;
        }
        //check that the charcater has been buried (has a grave)
        if (targetCharacter.grave != null) {
            return false;
        }
        if(targetCharacter.IsInOwnParty() || targetCharacter.currentParty != actor.ownParty) {
            return false;
        }
        return true;
    }
    #endregion
}
