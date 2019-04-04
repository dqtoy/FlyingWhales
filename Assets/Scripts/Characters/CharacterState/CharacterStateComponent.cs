using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStateComponent {
    public string previousMajorStateName;
    public string previousStateName;
    public string currentStateName;

    public Character character { get; private set; }
    public CharacterState previousMajorState { get; private set; }
    public CharacterState currentState { get; private set; }

    public CharacterStateComponent(Character character) {
        this.character = character;
    }
    public void SwitchToState(CHARACTER_STATE state, Character targetCharacter = null) {
        character.AdjustIsWaitingForInteraction(1);
        if (character.currentAction != null && !character.currentAction.isDone) {
            if (!character.currentAction.isPerformingActualAction) {
                character.SetCurrentAction(null);
            } else {
                character.currentAction.currentState.EndPerTickEffect();
            }
        }
        character.AdjustIsWaitingForInteraction(-1);

        if(character.currentParty.icon.isTravelling) {
            if (character.currentParty.icon.travelLine == null) {
                character.marker.StopMovementOnly();
            } else {
                //TODO: What if the character travelling to an area
            }
        }
        CharacterState newState = CreateNewState(state);

        if (currentState != null) {
            previousStateName = currentState.stateName;
            if (currentState.stateCategory == CHARACTER_STATE_CATEGORY.MAJOR) {
                previousMajorStateName = currentState.stateName;
                if(newState.stateCategory == CHARACTER_STATE_CATEGORY.MAJOR) {
                    //If the new state is a major state, automatically end current major state, do not just pause it
                    previousMajorState = null;
                    currentState.ExitState();
                } else {
                    previousMajorState = currentState;
                    previousMajorState.PauseState();
                }
            } else {
                currentState.ExitState();
            }

        }

        currentState = newState;
        currentState.SetTargetCharacter(targetCharacter);
        currentStateName = currentState.stateName;
        currentState.EnterState();
    }
    public void ExitCurrentState(CharacterState state, bool stopMovement = true) {
        if (stopMovement) {
            if (character.currentParty.icon.isTravelling) {
                if (character.currentParty.icon.travelLine == null) {
                    character.marker.StopMovementOnly();
                } else {
                    //TODO: What if the character travelling to an area
                }
            }
        }
        if (currentState != null) {
            if(currentState != state) {
                Debug.LogError("Inconsistency! The current state " + currentState.stateName + " of " + character.name + " does not match the state " + state.stateName);
                return;
            }
            previousStateName = currentState.stateName;
            if (currentState.stateCategory == CHARACTER_STATE_CATEGORY.MAJOR) {
                //previousMajorState = currentState;
                previousMajorStateName = currentState.stateName; 
            }
            currentState.ExitState();

        }
        if(currentState.stateCategory == CHARACTER_STATE_CATEGORY.MINOR && previousMajorState != null) {
            currentState = previousMajorState;
            currentStateName = currentState.stateName;
            currentState.ResumeState();
        } else {
            currentState = null;
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
        }
        return newState;
    }
}
