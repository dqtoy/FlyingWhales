using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watch : GoapAction {
    public GoapAction actionBeingWatched { get; private set; }
    public CombatState combatBeingWatched { get; private set; }

    public Watch(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.WATCH, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionIconString = GoapActionStateDB.Hostile_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.MORNING,
        //    TIME_IN_WORDS.AFTERNOON,
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //};
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        base.PerformActualAction();
        Character targetCharacter = poiTarget as Character;
        if (targetCharacter.IsInOwnParty()) {
            SetState("Watch Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 10;
    }
    public override bool InitializeOtherData(object[] otherData) {
        if (otherData.Length == 1) {
            if(otherData[0] is GoapAction) {
                actionBeingWatched = otherData[0] as GoapAction;
                if (thoughtBubbleLog != null) {
                    thoughtBubbleLog.AddToFillers(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.OTHER);
                }
                if (thoughtBubbleMovingLog != null) {
                    thoughtBubbleMovingLog.AddToFillers(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.OTHER);
                }
            } else if (otherData[0] is CombatState) {
                combatBeingWatched = otherData[0] as CombatState;
                if (thoughtBubbleLog != null) {
                    thoughtBubbleLog.AddToFillers(combatBeingWatched, "Combat", LOG_IDENTIFIER.OTHER);
                }
                if (thoughtBubbleMovingLog != null) {
                    thoughtBubbleMovingLog.AddToFillers(combatBeingWatched, "Combat", LOG_IDENTIFIER.OTHER);
                }
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    #endregion

    #region State Effects
    private void PreWatchSuccess() {
        if(actionBeingWatched != null) {
            currentState.AddLogFiller(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.OTHER);
        }else if (combatBeingWatched != null) {
            currentState.AddLogFiller(combatBeingWatched, "Combat", LOG_IDENTIFIER.OTHER);
        }
        Messenger.AddListener(Signals.TICK_STARTED, PerTickWatchSuccess);
    }
    private void PerTickWatchSuccess() {
        if (actionBeingWatched != null) {
            if (actionBeingWatched.isDone) {
                Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
                if (actor.currentParty.icon.isTravelling) {
                    //Stop moving
                    actor.marker.StopMovement();
                }
                currentState.EndPerTickEffect();
                return;
            }
        } else if (combatBeingWatched != null) {
            if (combatBeingWatched.isDone) {
                Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
                if (actor.currentParty.icon.isTravelling) {
                    //Stop moving
                    actor.marker.StopMovement();
                }
                currentState.EndPerTickEffect();
                return;
            }
        }

        if(!actor.currentParty.icon.isTravelling && !actor.marker.inVisionPOIs.Contains(poiTarget)) {
            //Go to target because target is not in vision
            actor.marker.GoTo(poiTarget);
        }else if (actor.currentParty.icon.isTravelling && actor.marker.inVisionPOIs.Contains(poiTarget)) {
            //Stop moving when in vision already
            actor.marker.StopMovement();
        }
    }
    //private void AfterWatchSuccess() {
    //    if(actionBeingWatched.currentState.shareIntelReaction != null) {
    //        actionBeingWatched.currentState.shareIntelReaction.Invoke(poiTarget as Character, null, SHARE_INTEL_STATUS.INFORMED);
    //    }
    //}
    #endregion

    #region Requirements
    private bool Requirement() {
        Character target = poiTarget as Character;
        return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
    }
    #endregion
}
