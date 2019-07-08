using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watch : GoapAction {
    public GoapAction actionBeingWatched { get; private set; }
    public CombatState combatBeingWatched { get; private set; }

    private Character _targetCharacter;
    public Watch(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.WATCH, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        doesNotStopTargetCharacter = true;
        //cannotCancelAction = true;
        _targetCharacter = poiTarget as Character;
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
                actionIconString = actionBeingWatched.actionIconString;
                if (thoughtBubbleLog != null) {
                    thoughtBubbleLog.AddToFillers(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.STRING_1);
                }
                if (thoughtBubbleMovingLog != null) {
                    thoughtBubbleMovingLog.AddToFillers(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.STRING_1);
                }
            } else if (otherData[0] is CombatState) {
                combatBeingWatched = otherData[0] as CombatState;
                actionIconString = GoapActionStateDB.Hostile_Icon;
                if (thoughtBubbleLog != null) {
                    thoughtBubbleLog.AddToFillers(combatBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
                }
                if (thoughtBubbleMovingLog != null) {
                    thoughtBubbleMovingLog.AddToFillers(combatBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
                }
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    public override LocationGridTile GetTargetLocationTile() {
        return InteractionManager.Instance.GetTargetLocationTile(actionLocationType, actor, null, targetStructure);
    }
    public override void OnStopActionDuringCurrentState() {
        base.OnStopActionDuringCurrentState();
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
        }

        if (shouldAddLogs && currentState.shouldAddLogs) { //only add logs if both the parent action and this state should add logs
            currentState.descriptionLog.SetDate(GameManager.Instance.Today());
            currentState.descriptionLog.AddLogToInvolvedObjects();
        }
    }
    #endregion

    #region State Effects
    private void PreWatchSuccess() {
        if(actionBeingWatched != null) {
            currentState.AddLogFiller(actionBeingWatched, actionBeingWatched.goapName, LOG_IDENTIFIER.STRING_1);
        }else if (combatBeingWatched != null) {
            currentState.AddLogFiller(combatBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
        }
        Messenger.AddListener(Signals.TICK_STARTED, PerTickWatchSuccess);
    }
    private void PerTickWatchSuccess() {
        if (_targetCharacter.isDead) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
            if (actor.currentParty.icon.isTravelling) {
                //Stop moving
                actor.marker.StopMovement();
            }
            currentState.EndPerTickEffect();
            return;
        }
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

        //Always face target when not travelling
        if (!actor.currentParty.icon.isTravelling) {
            actor.FaceTarget(poiTarget);
        }
    }
    private void AfterWatchSuccess() {
        if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
        }
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        Character target = poiTarget as Character;
        return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
    }
    #endregion
}
