using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class TileObjectDestroy : GoapAction {

    private LocationStructure structure;

    public TileObjectDestroy(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.TILE_OBJECT_DESTROY, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void MoveToDoAction(Character targetCharacter) {
        //base.MoveToDoAction(targetCharacter);
        //change end reached distance to the characters attack range. NOTE: Make sure to return the range to default after this action is done.
        //actor.marker.pathfindingAI.SetEndReachedDistance(actor.characterClass.attackRange);
        isPerformingActualAction = true;
        actorAlterEgo = actor.currentAlterEgo;
        AddAwareCharacter(actor);
        if (actor.marker.AddHostileInRange(poiTarget, false)) {
            structure = poiTarget.gridTileLocation.structure;
            Messenger.AddListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState); //set this to signal because adding a character as hostile, is no longer sure to return a new CharacterState
            SetState("In Progress");
        } else {
            Debug.LogWarning(GameManager.Instance.TodayLogString() + actor.name + " was unable to add target as hostile when reacting to " + poiTarget.name + " in tile object destroy action!");
            SetState("Target Missing");
        }
    }
    public override bool ShouldBeStoppedWhenSwitchingStates() {
        return false;
    }
    protected override void OnCancelActionTowardsTarget() {
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        base.OnCancelActionTowardsTarget();
    }
    public override void OnStopActionWhileTravelling() {
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        base.OnStopActionWhileTravelling();
    }
    public override void OnStopActionDuringCurrentState() {
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
    }
    public override void OnResultReturnedToActor() {
        Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
    //}
    //public override void PerformActualAction() {
    //    base.PerformActualAction();
    //    if (!isTargetMissing) {
    //        SetState("Destroy Success");
    //    } else {
    //        SetState("Target Missing");
    //    }
    //}
    protected override int GetCost() {
        return 10;
    }
    //public override void FailAction() {
    //    base.FailAction();
    //    SetState("Target Missing");
    //}
    #endregion

    #region Listeners
    private void OnCharacterStartedState(Character character, CharacterState state) {
        if (character == actor) {
            CombatState combatState = state as CombatState;
            combatState.SetActionThatTriggeredThisState(this);
            combatState.SetEndStateAction(OnFinishCombatState);
            Messenger.RemoveListener<Character, CharacterState>(Signals.CHARACTER_STARTED_STATE, OnCharacterStartedState);
        }
    }
    private void OnFinishCombatState() {
        //TODO: Add Checking if the actor of this action was the one that removed the tile object
        TileObject target = poiTarget as TileObject;
        if (target.removedBy == actor) {
            SetState("Destroy Success");
        } else {
            SetState("Target Missing");
        }
        
    }
    #endregion

    #region State Effects
    private void PreInProgress() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    private void PreDestroySuccess() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        currentState.AddLogFiller(structure.location, structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
        currentState.SetIntelReaction(SuccessReactions);
    }
    private void AfterDestroySuccess() {
        //**After Effect 1**: Destroy target tile object
        poiTarget.gridTileLocation?.structure.RemovePOI(poiTarget, actor);
    }
    private void PreTargetMissing() {
        currentState.AddLogFiller(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    }
    #endregion

    #region Requirement
    protected bool Requirement() {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
    #endregion

    #region Intel Reactions
    private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        TileObject tileObj = poiTarget as TileObject;

        RELATIONSHIP_EFFECT relWithActor = recipient.GetRelationshipEffectWith(actor);
        if (recipient == actor) {
            // - If informed: "I am embarrassed by my own actions."
            if (status == SHARE_INTEL_STATUS.INFORMED) {
                reactions.Add("I am embarrassed by my own actions.");
            }
        }
        //- Owns the item destroyed
        else if (tileObj.IsOwnedBy(recipient)) {
            if (relWithActor == RELATIONSHIP_EFFECT.NEGATIVE) {
                //-Witnesser has a negative relationship with Actor
                CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);  //-Relationship degradation with Actor
                if (status == SHARE_INTEL_STATUS.WITNESSED) {
                    //- If witnessed: Create an Assault job targeting the Actor
                    recipient.CreateKnockoutJob(actor);
                } else if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If informed: "[Actor Name] is getting even more unhinged day by day!"
                    reactions.Add(string.Format("{0} is getting even more unhinged day by day!", actor.name));
                }
            } else if (relWithActor == RELATIONSHIP_EFFECT.POSITIVE || relWithActor == RELATIONSHIP_EFFECT.NONE) {
                //- Witnesser has a positive or neutral relationship with Actor
                CharacterManager.Instance.RelationshipDegradation(actor, recipient, this);  //-Relationship degradation with Actor
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If informed: "Perhaps it's best if I avoid [Actor Name] for now."
                    reactions.Add(string.Format("Perhaps it's best if I avoid {0} for now.", actor.name));
                }
            }
        }
        //- Others
        else {
            // - If informed: "This isn't relevant to me."
            if (status == SHARE_INTEL_STATUS.INFORMED) {
                reactions.Add("This isn't relevant to me.");
            }
        }


        return reactions;
    }
    #endregion
}

public class TileObjectDestroyData : GoapActionData {
    public TileObjectDestroyData() : base(INTERACTION_TYPE.TILE_OBJECT_DESTROY) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}