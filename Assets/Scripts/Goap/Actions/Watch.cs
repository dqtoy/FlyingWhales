using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watch : GoapAction {
    public GoapAction actionBeingWatched { get; private set; }
    public CharacterState stateBeingWatched { get; private set; }

    private Character _targetCharacter;

    //for testing
    private int ticksInWatch;
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
        isNotificationAnIntel = false;
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
        this.otherData = otherData;
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
                stateBeingWatched = otherData[0] as CombatState;
                actionIconString = GoapActionStateDB.Hostile_Icon;
                if (thoughtBubbleLog != null) {
                    thoughtBubbleLog.AddToFillers(stateBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
                }
                if (thoughtBubbleMovingLog != null) {
                    thoughtBubbleMovingLog.AddToFillers(stateBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
                }
            } else if (otherData[0] is DouseFireState) {
                stateBeingWatched = otherData[0] as DouseFireState;
                actionIconString = GoapActionStateDB.Hostile_Icon;
                if (thoughtBubbleLog != null) {
                    thoughtBubbleLog.AddToFillers(stateBeingWatched, "Douse Fire", LOG_IDENTIFIER.STRING_1);
                }
                if (thoughtBubbleMovingLog != null) {
                    thoughtBubbleMovingLog.AddToFillers(stateBeingWatched, "Douse Fire", LOG_IDENTIFIER.STRING_1);
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
        //if (Messenger.eventTable.ContainsKey(Signals.TICK_STARTED)) {
        //    Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
        //}

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
        }else if (stateBeingWatched != null) {
            if (stateBeingWatched is CombatState) {
                currentState.AddLogFiller(stateBeingWatched, "Combat", LOG_IDENTIFIER.STRING_1);
            } else if (stateBeingWatched is DouseFireState) {
                currentState.AddLogFiller(stateBeingWatched, "Douse Fire", LOG_IDENTIFIER.STRING_1);
            }
        }
        //Messenger.AddListener(Signals.TICK_STARTED, PerTickWatchSuccess);
        ticksInWatch = 0;
    }
    private void PerTickWatchSuccess() {
        if (_targetCharacter.isDead) {
            //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
            if (actor.currentParty.icon.isTravelling) {
                //Stop moving
                actor.marker.StopMovement();
            }
            currentState.EndPerTickEffect();
            return;
        }
        if (actionBeingWatched != null) {
            if (actionBeingWatched.isDone || actionBeingWatched.actor.currentAction != actionBeingWatched) {
                //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
                if (actor.currentParty.icon.isTravelling) {
                    //Stop moving
                    actor.marker.StopMovement();
                }
                currentState.EndPerTickEffect();
                return;
            }
        } else if (stateBeingWatched != null) {
            if (stateBeingWatched.isDone || (stateBeingWatched.stateComponent.currentState != stateBeingWatched && !stateBeingWatched.isPaused)) { //only end watch state if the state is done or if the watched state is no longer active and not paused
                //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
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
            if(actor.HasTraitOf(TRAIT_EFFECT.NEGATIVE, TRAIT_TYPE.DISABLER)) {
                //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
                currentState.EndPerTickEffect();
                return;
            } else {
                actor.marker.GoTo(poiTarget);
            }
        } else if (actor.currentParty.icon.isTravelling && actor.marker.inVisionPOIs.Contains(poiTarget)) {
            //Stop moving when in vision already
            actor.marker.StopMovement();
        }

        //Always face target when not travelling
        if (!actor.currentParty.icon.isTravelling) {
            actor.FaceTarget(poiTarget);
        }
        ticksInWatch++;
    }
    private void AfterWatchSuccess() {
        if (actionBeingWatched != null) {
            AddActionDebugLog(GameManager.Instance.TodayLogString() + actor.name + " has finished watching " + actionBeingWatched.goapName + " by " + actionBeingWatched.actor.name + ". Total ticks in watch is " + ticksInWatch.ToString() + "/" + currentState.duration.ToString());
        } else if (stateBeingWatched != null) {
            AddActionDebugLog(GameManager.Instance.TodayLogString() + actor.name + " has finished watching " + stateBeingWatched.ToString() + ". Total ticks in watch is " + ticksInWatch.ToString() + "/" + currentState.duration.ToString());
        }
        //Messenger.RemoveListener(Signals.TICK_STARTED, PerTickWatchSuccess);
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        Character target = poiTarget as Character;
        return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
    }
    #endregion
}

public class WatchData : GoapActionData {
    public WatchData() : base(INTERACTION_TYPE.WATCH) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character target = poiTarget as Character;
        return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
    }
}