using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ActionThread : Multithread {
    private CharacterActionAdvertisement[] choices;
    private Character _character;
    private CharacterAction chosenAction;

    public ActionThread(Character character) {
        choices = new CharacterActionAdvertisement[3];
        _character = character;
    }

    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        LookForAction();
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnAction();
    }
    #endregion
    private void LookForAction() {
        string actionLog = _character.name + "'s Action Advertisements: ";
        for (int i = 0; i < _character.currentRegion.landmarks.Count; i++) {
            BaseLandmark landmark = _character.currentRegion.landmarks[i];
            for (int j = 0; j < landmark.objects.Count; j++) {
                IObject iobject = landmark.objects[j];
                if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                    for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                        CharacterAction action = iobject.currentState.actions[k];
                        if (action.MeetsRequirements(_character, landmark)) { //Filter
                            float advertisement = action.GetTotalAdvertisementValue(_character);
                            actionLog += "\n" + action.actionData.actionName + " = " + advertisement + " (" + iobject.objectName + " at " + iobject.objectLocation.landmarkName + ")";
                            PutToChoices(action, advertisement);
                        }
                    }
                }
            }
        }
        //Debug.Log(actionLog);
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _character.id) {
            Debug.Log(actionLog);
        }
        chosenAction = PickAction();
    }
    private void ReturnAction() {
        _character.actionData.ReturnActionFromThread(chosenAction);
    }
    private void PutToChoices(CharacterAction action, float advertisement) {
        if (choices[0].action == null) {
            choices[0].Set(action, advertisement);
        } else if (choices[1].action == null) {
            choices[1].Set(action, advertisement);
        } else if (choices[2].action == null) {
            choices[2].Set(action, advertisement);
        } else {
            if (choices[0].advertisement <= choices[1].advertisement && choices[0].advertisement <= choices[2].advertisement) {
                if (advertisement > choices[0].advertisement) {
                    choices[0].Set(action, advertisement);
                }
            } else if (choices[1].advertisement <= choices[0].advertisement && choices[1].advertisement <= choices[2].advertisement) {
                if (advertisement > choices[1].advertisement) {
                    choices[1].Set(action, advertisement);
                }
            } else if (choices[2].advertisement <= choices[0].advertisement && choices[2].advertisement <= choices[1].advertisement) {
                if (advertisement > choices[2].advertisement) {
                    choices[2].Set(action, advertisement);
                }
            }
        }
    }
    private CharacterAction PickAction() {
        int maxChoice = 3;
        if (choices[1].action == null) {
            maxChoice = 1;
        } else if (choices[2].action == null) {
            maxChoice = 2;
        }
        int chosenIndex = Utilities.rng.Next(0, maxChoice);
        CharacterAction chosenAction = choices[chosenIndex].action;
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _character.id) {
            Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.objectLocation.landmarkName + ")");
        }
        //Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.objectLocation.landmarkName + ")");
        return chosenAction;
        //AssignAction(chosenAction);
        //_character.GoToLocation(chosenAction.state.obj.objectLocation, PATHFINDING_MODE.USE_ROADS);
    }
}
