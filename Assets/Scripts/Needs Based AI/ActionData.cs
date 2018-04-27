using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class ActionData {
    private Character _character;
    public CharacterAction currentAction;
    public int currentDay;
    public bool isDone;

    private CharacterActionAdvertisement[] choices;

    public ActionData(Character character) {
        Reset();
        _character = character;
        choices = new CharacterActionAdvertisement[3];
        Messenger.AddListener("OnDayEnd", PerformCurrentAction);
    }

    public void Reset() {
        this.currentAction = null;
        this.currentDay = 0;
        this.isDone = false;
    }

    public void AssignAction(CharacterAction action) {
        Reset();
        SetCurrentAction(action);
    }
    public void DetachActionData() {
        Reset();
        _character = null;
        Messenger.RemoveListener("OnDayEnd", PerformCurrentAction);
    }

    public void EndAction() {
        isDone = true;
    }
    public void SetCurrentAction(CharacterAction action) {
        this.currentAction = action;
    }
    public void SetCurrentDay(int day) {
        this.currentDay = day;
    }
    private void AdjustCurrentDay(int amount) {
        this.currentDay += amount;
        if(this.currentDay >= currentAction.actionData.duration) {
            EndAction();
        }
    }
    public void SetIsDone(bool state) {
        this.isDone = state;
    }

    private void PerformCurrentAction() {
        if(!isDone && currentAction != null) {
            if(_character.specificLocation != null && _character.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK && _character.specificLocation == currentAction.state.obj.objectLocation) {
                currentAction.PerformAction(_character);
                if (currentAction.actionData.duration > 0) {
                    AdjustCurrentDay(1);
                }
            }
            //else {
            //    Debug.Log(_character.name + " can't perform " + currentAction.actionData.actionName + " because he is not in the same location!");
            //}
        } else {
            LookForAction();
        }
    }

    private void LookForAction() {
        choices[0].Reset();
        choices[1].Reset();
        choices[2].Reset();

        string actionLog = "Action Advertisements:";
        for (int i = 0; i < _character.currentRegion.landmarks.Count; i++) {
            BaseLandmark landmark = _character.currentRegion.landmarks[i];
            for (int j = 0; j < landmark.objects.Count; j++) {
                IObject iobject = landmark.objects[j];
                if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                    for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                        CharacterAction action = iobject.currentState.actions[k];
                        if (action.MeetsRequirements(_character, landmark)) { //Filter
                            int advertisement = action.GetTotalAdvertisementValue(_character);
                            actionLog += "\n" + action.actionData.actionName + " = " + advertisement + " (" + iobject.objectName + ")";
                            PutToChoices(action, advertisement);
                        }
                    }
                }
            }
        }
        Debug.Log(actionLog);
        PickAction();
    }
    private void PutToChoices(CharacterAction action, int advertisement) {
        if(choices[0].action == null) {
            choices[0].Set(action, advertisement);
        } else if (choices[1].action == null) {
            choices[1].Set(action, advertisement);
        } else if (choices[2].action == null) {
            choices[2].Set(action, advertisement);
        } else {
            if(choices[0].advertisement <= choices[1].advertisement && choices[0].advertisement <= choices[2].advertisement) {
                if(advertisement > choices[0].advertisement) {
                    choices[0].Set(action, advertisement);
                }
            }else if (choices[1].advertisement <= choices[0].advertisement && choices[1].advertisement <= choices[2].advertisement) {
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
    private void PickAction() {
        int maxChoice = 3;
        if(choices[1].action == null) {
            maxChoice = 1;
        } else if (choices[2].action == null) {
            maxChoice = 2;
        }
        int chosenIndex = UnityEngine.Random.Range(0, maxChoice);
        CharacterAction chosenAction = choices[chosenIndex].action;
        AssignAction(chosenAction);
        _character.GoToLocation(chosenAction.state.obj.objectLocation, PATHFINDING_MODE.USE_ROADS);
        Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + ")");
    }
}
