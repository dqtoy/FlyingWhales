using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System;

public class ActionThread : Multithread {
    private CharacterActionAdvertisement[] choices;
    private Character _character;
    private CharacterAction chosenAction;
    private List<CharacterActionAdvertisement> allChoices;
    private ChainAction chosenChainAction;

    public ActionThread(Character character) {
        choices = new CharacterActionAdvertisement[3];
        allChoices = new List<CharacterActionAdvertisement>();
        _character = character;
    }

    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            LookForAction();
        } catch (Exception e) {
            Debug.LogError(e.Message);
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnAction();
    }
    #endregion
    private void LookForAction() {
        allChoices.Clear();

        string actionLog = _character.name + "'s Action Advertisements: ";
        for (int i = 0; i < _character.currentRegion.landmarks.Count; i++) {
            BaseLandmark landmark = _character.currentRegion.landmarks[i];
            StructureObj iobject = landmark.landmarkObj;
            if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                    CharacterAction action = iobject.currentState.actions[k];
                    if (action.MeetsRequirements(_character, landmark) && action.CanBeDone()) { //Filter
                        float happinessIncrease = _character.role.GetTotalHappinessIncrease(action);
                        actionLog += "\n" + action.actionData.actionName + " = " + happinessIncrease + " (" + iobject.objectName + " at " + iobject.specificLocation.locationName + ")";
                        PutToChoices(action, happinessIncrease);
                    }
                }
            }
            //for (int j = 0; j < landmark.objects.Count; j++) {
            //    IObject iobject = landmark.objects[j];
            //    if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
            //        for (int k = 0; k < iobject.currentState.actions.Count; k++) {
            //            CharacterAction action = iobject.currentState.actions[k];
            //            if (action.MeetsRequirements(_character, landmark)) { //Filter
            //                float advertisement = action.GetTotalAdvertisementValue(_character);
            //                actionLog += "\n" + action.actionData.actionName + " = " + advertisement + " (" + iobject.objectName + " at " + iobject.objectLocation.landmarkName + ")";
            //                PutToChoices(action, advertisement);
            //            }
            //        }
            //    }
            //}
        }
        //Debug.Log(actionLog);
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _character.id) {
            Debug.Log(actionLog);
        }
        chosenAction = PickAction();

        //Check Prerequisites, currently for resource prerequisites only
        CheckPrerequisites(chosenAction);

    }
    private void CheckPrerequisites(CharacterAction characterAction) {
        if (HasPrerequisite(characterAction)) {
            if (CanDoPrerequisite(characterAction)) {
                DoPrerequisite(characterAction, null, null);
            } else {
                RemoveActionFromChoices(characterAction);
                chosenAction = PickAction();
                CheckPrerequisites(chosenAction);
            }
        }
    }
    private bool CanDoPrerequisite(CharacterAction characterAction) {
        if(HasPrerequisite(characterAction)) {
            for (int i = 0; i < characterAction.actionData.prerequisites.Count; i++) {
                IPrerequisite prerequisite = characterAction.actionData.prerequisites[i];
                if (_character.DoesSatisfiesPrerequisite(prerequisite)) {
                    continue;
                }
                if (prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
                    ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
                    CharacterAction satisfyingAction = GetActionThatMatchesResourcePrerequisite(resourcePrerequisite);
                    if (satisfyingAction == null || (satisfyingAction != null && !CanDoPrerequisite(satisfyingAction))) {
                        return false;
                    }
                }
            }
        }
        return true;
    }
    private void DoPrerequisite(CharacterAction characterAction, ChainAction parentAction, IPrerequisite iprerequisite) {
        ChainAction chainAction = new ChainAction();
        chainAction.parentChainAction = parentAction;
        chainAction.action = characterAction;
        chainAction.prerequisite = iprerequisite;
        chainAction.satisfiedPrerequisites = new List<ChainAction>();
        chainAction.finishedPrerequisites = new List<IPrerequisite>();
        if (parentAction != null) {
            parentAction.satisfiedPrerequisites.Add(chainAction);
        }
        chosenChainAction = chainAction;
        if (HasPrerequisite(characterAction)) {
            for (int i = 0; i < characterAction.actionData.prerequisites.Count; i++) {
                IPrerequisite prerequisite = characterAction.actionData.prerequisites[i];
                if (_character.DoesSatisfiesPrerequisite(prerequisite)) {
                    chainAction.finishedPrerequisites.Add(prerequisite);
                    continue;
                }
                if (prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
                    ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
                    CharacterAction satisfyingAction = GetActionThatMatchesResourcePrerequisite(resourcePrerequisite);
                    if (satisfyingAction != null) {
                        DoPrerequisite(satisfyingAction, chainAction, prerequisite);
                    }
                }
            }
        }
    }
    private bool HasPrerequisite(CharacterAction characterAction) {
        return characterAction.actionData.prerequisites != null;
    }
    private CharacterAction GetActionThatMatchesResourcePrerequisite(ResourcePrerequisite resourcePrerequisite) {
        for (int i = 0; i < allChoices.Count; i++) {
            CharacterAction action = allChoices[i].action;
            if (action.actionData.advertisedResource != RESOURCE.NONE && resourcePrerequisite.resourceType != RESOURCE.NONE && action.actionData.advertisedResource == resourcePrerequisite.resourceType && action.state.obj.resourceInventory != null) {
                if (action.state.obj.resourceInventory[resourcePrerequisite.resourceType] >= resourcePrerequisite.amount) {
                    return action;
                }
            }
        }
        return null;
    }
    private void ReturnAction() {
        _character.actionData.ReturnActionFromThread(chosenAction, chosenChainAction);
    }
    private void PutToChoices(CharacterAction action, float advertisement) {
        CharacterActionAdvertisement actionAdvertisement = new CharacterActionAdvertisement();
        actionAdvertisement.Set(action, advertisement);
        allChoices.Add(actionAdvertisement);
       
    }
    private CharacterAction PickAction() {
        choices[0].Reset();
        choices[1].Reset();
        choices[2].Reset();
        float advertisement = 0f;
        CharacterAction action = null;
        for (int i = 0; i < allChoices.Count; i++) {
            action = allChoices[i].action;
            advertisement = allChoices[i].advertisement;
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
       
        int maxChoice = 3;
        if (choices[1].action == null) {
            maxChoice = 1;
        } else if (choices[2].action == null) {
            maxChoice = 2;
        }
        int chosenIndex = Utilities.rng.Next(0, maxChoice);
        CharacterAction chosenAction = choices[chosenIndex].action;
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _character.id) {
            Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.specificLocation.locationName + ")");
        }
        //Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.objectLocation.landmarkName + ")");
        return chosenAction;
        //AssignAction(chosenAction);
        //_character.GoToLocation(chosenAction.state.obj.objectLocation, PATHFINDING_MODE.USE_ROADS);
    }

    private void RemoveActionFromChoices(CharacterAction action) {
        for (int i = 0; i < allChoices.Count; i++) {
            if(allChoices[i].action == action) {
                allChoices.RemoveAt(i);
                break;
            }
        }
    }
}
