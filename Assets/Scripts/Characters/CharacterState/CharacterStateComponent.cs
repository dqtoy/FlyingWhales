using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is the bridge between the character and the states, a component that stores all necessary information and process all data for the character and its states  
//Basically, this is a manager for the character states
//This is where you switch from one state to another, etc.
//Everything a character wants to do with a state must go through here
public class CharacterStateComponent {
    //public string previousMajorStateName;
    //public string previousStateName;
    //public string currentStateName;

    public Character character { get; private set; }
    //If a major state is replaced by a minor state, must be stored in order for the character to go back to this state after doing the minor state
    public CharacterState previousMajorState { get; private set; }
    //This is the character's current state
    public CharacterState currentState { get; private set; }
    //Right now this is only for Explore State so that we can store the state even when the character is still moving to the area that will be explored
    public CharacterState stateToDo { get; private set; }

    public CharacterStateComponent(Character character) {
        this.character = character;
    }

    public void SetCurrentState(CharacterState state) {
        currentState = state;
    }
    public void SetStateToDo(CharacterState state, bool unassignJob = true) {
        if(unassignJob && state == null && stateToDo != null) {
            if(stateToDo.job != null) {
                stateToDo.job.SetAssignedCharacter(null);
                stateToDo.job.SetAssignedState(null);
            }
        }
        stateToDo = state;
    }

    //This switches from one state to another
    //If the character is not in a state right now, this simply starts a new state instead of switching
    public CharacterState SwitchToState(CHARACTER_STATE state, Character targetCharacter = null, Area targetArea = null) {
        //Before switching character must end current action first because once a character is in a state in cannot make plans
        character.AdjustIsWaitingForInteraction(1);
        character.StopCurrentAction();
        character.AdjustIsWaitingForInteraction(-1);

        //Stop the movement of character because the new state probably has different movement behavior
        if(character.currentParty.icon.isTravelling) {
            if (character.currentParty.icon.travelLine == null) {
                character.marker.StopMovementOnly();
            } else {
                //TODO: What if the character travelling to an area
            }
        }

        //Create the new state
        CharacterState newState = CreateNewState(state);
        if (currentState != null) {
            if (currentState.stateCategory == CHARACTER_STATE_CATEGORY.MAJOR) {
                if(newState.stateCategory == CHARACTER_STATE_CATEGORY.MAJOR) {
                    //If the new state is a major state, automatically end current major state, do not just pause it
                    //Also, do not store the previous major state because since the new state is a major state, it must not go back to the previous major state
                    previousMajorState = null;
                    currentState.ExitState();
                } else {
                    //Store current state as previous major state so the character will go back to that state after doing the minor state
                    //Pause the previous major state so that the timer will not tick, if there's any
                    previousMajorState = currentState;
                    previousMajorState.PauseState();
                }
            } else {
                //If current state is a minor state, simply end it
                currentState.ExitState();
            }
        }else if (stateToDo != null) {
            if(stateToDo.stateCategory == CHARACTER_STATE_CATEGORY.MAJOR) {
                previousMajorState = stateToDo;
            }
            SetStateToDo(null, false);
        }

        //Assigns new state as the current state then enter that state
        //newState.SetParentMajorState(previousMajorState);
        //currentState = newState;
        newState.SetTargetCharacter(targetCharacter);
        newState.EnterState(targetArea);
        return newState;
    }

    //This ends the current state
    //This is triggered when the timer is out, or the character simply wants to end its state and go back to normal state
    public void ExitCurrentState(CharacterState state, bool stopMovement = true) {
        if (currentState == null) {
            throw new System.Exception(character.name + " is trying to exit his/her current state but it is null, Passed state is " + state?.stateName);
        }

        //Stops movement unless told otherwise
        if (stopMovement) {
            if(!(currentState != null && character.currentAction != null && character.currentAction.parentPlan == null)) {
                if (character.currentParty.icon.isTravelling) {
                    if (character.currentParty.icon.travelLine == null) {
                        character.marker.StopMovementOnly();
                    } else {
                        //TODO: What if the character travelling to an area
                    }
                }
            }
        }
        if (currentState != null) {
            //This ends the current state but I added a checker that the parameter state must be the same as the current state to avoid inconsistencies
            if(currentState != state) {
                Debug.LogError("Inconsistency! The current state " + currentState.stateName + " of " + character.name + " does not match the state " + state.stateName);
                //return;
            }
            currentState.ExitState();
        }

        if (character.isDead) {
            if(previousMajorState != null) {
                previousMajorState.ExitState();
            }
            previousMajorState = null;
            SetCurrentState(null);
            return;
        }
        //If the current state is a minor state and there is a previous major state, resume that major state
        if(currentState.stateCategory == CHARACTER_STATE_CATEGORY.MINOR && previousMajorState != null) {
            if(previousMajorState.duration > 0 && previousMajorState.currentDuration >= previousMajorState.duration) {
                //In a rare case that the previous major state has already timed out, end that state and do not resume it
                //This goes back to normal
                previousMajorState.ExitState();
                SetCurrentState(null);
            } else {
                if(character.doNotDisturb > 0) {
                    if(previousMajorState.characterState == CHARACTER_STATE.BERSERKED && character.doNotDisturb == 1 && character.GetTrait("Combat Recovery") != null) { //Quick fix only for build, MUST REDO
                        if (previousMajorState.hasStarted) {
                            //Resumes previous major state
                            SetCurrentState(previousMajorState);
                            currentState.ResumeState();
                        } else {
                            previousMajorState.EnterState(previousMajorState.targetArea);
                        }
                    } else {
                        previousMajorState.ExitState();
                        SetCurrentState(null);
                    }
                } else {
                    if (previousMajorState.hasStarted) {
                        //Resumes previous major state
                        SetCurrentState(previousMajorState);
                        currentState.ResumeState();
                    } else {
                        previousMajorState.EnterState(previousMajorState.targetArea);
                    }
                }
            }
            previousMajorState = null;
        } else {
            //This goes back to normal
            previousMajorState = null;
            SetCurrentState(null);
        }
    }

    private CharacterState CreateNewState(CHARACTER_STATE state) {
        CharacterState newState = null;
        switch (state) {
            case CHARACTER_STATE.EXPLORE:
                newState = new ExploreState(this);
                break;
            case CHARACTER_STATE.PATROL:
                newState = new PatrolState(this);
                break;
            case CHARACTER_STATE.FLEE:
                newState = new FleeState(this);
                break;
            case CHARACTER_STATE.ENGAGE:
                newState = new EngageState(this);
                break;
            case CHARACTER_STATE.HUNT:
                newState = new HuntState(this);
                break;
            case CHARACTER_STATE.STROLL:
                newState = new StrollState(this);
                break;
            case CHARACTER_STATE.STROLL_OUTSIDE:
                newState = new StrollOutsideState(this);
                break;
            case CHARACTER_STATE.BERSERKED:
                newState = new BerserkedState(this);
                break;
        }
        return newState;
    }
}
