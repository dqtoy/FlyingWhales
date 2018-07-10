using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System;

public class ActionThread : Multithread {
    private CharacterActionAdvertisement[] choices;
    private CharacterParty _party;
    private CharacterAction chosenAction;
    private IObject chosenObject;
    private List<CharacterActionAdvertisement> allChoices;
    private ChainAction chosenChainAction;
    private string actionLog;

    #region getters/setters
    public CharacterParty party {
        get { return _party; }
    }
    #endregion

    public ActionThread(CharacterParty party) {
        choices = new CharacterActionAdvertisement[3];
        allChoices = new List<CharacterActionAdvertisement>();
        _party = party;
    }

    #region Overrides
    public override void DoMultithread() {
        base.DoMultithread();
        try {
            LookForAction();
        } catch (Exception e) {
            Debug.LogError(e.Message + " ST: " + e.StackTrace);
        }
    }
    public override void FinishMultithread() {
        base.FinishMultithread();
        ReturnAction();
    }
    #endregion
    private void LookForAction() {
        if (!LookForActionFromQuests()) {
            LookForActionFromAdvertisements();
        }
    }
    private bool LookForActionFromQuests() {
        if (_party.questData.Count > 0 && _party.mainCharacter.role != null && _party.mainCharacter.role.happiness >= 100) {
            //when a character's happiness is 100 or above, he chooses randomly between his active quests and calls a function from it which should return an instruction of which next action to execute
            int randomIndex = Utilities.rng.Next(0, _party.questData.Count);
            CharacterQuestData chosenQuest = _party.questData[randomIndex];
            //if (chosenQuest is ReleaseCharacterQuestData) {
            //    ReleaseCharacterQuestData data = chosenQuest as ReleaseCharacterQuestData;
            //    data.UpdateVectorPath();
            //    while (data.isWaitingForPath) {
            //        //wait until path has completed computation
            //    }
            //}
            IObject targetObject = null;
            chosenAction = chosenQuest.GetNextQuestAction(ref targetObject);
            chosenObject = targetObject;
            if (chosenAction == null) {
                chosenObject = _party.characterObject;
                chosenAction = _party.mainCharacter.GetRandomIdleAction(); //Characters with no Quests with Happiness above 100 should perform a random Idle Action
            }
            return true;
        }
        return false;
    }
    private void LookForActionFromAdvertisements() {
        allChoices.Clear();

        actionLog = _party.name + "'s Action Advertisements: ";
        for (int i = 0; i < _party.currentRegion.landmarks.Count; i++) {
            BaseLandmark landmark = _party.currentRegion.landmarks[i];
            StructureObj iobject = landmark.landmarkObj;
            if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                    CharacterAction action = iobject.currentState.actions[k];
                    if (action.MeetsRequirements(_party, landmark) && action.CanBeDone(iobject) && action.CanBeDoneBy(_party, iobject)) { //Filter
                        float happinessIncrease = _party.TotalHappinessIncrease(action, iobject);
                        actionLog += "\n" + action.actionData.actionName + " = " + happinessIncrease + " (" + iobject.objectName + " at " + iobject.specificLocation.locationName + ")";
                        PutToChoices(action, iobject, happinessIncrease);
                    }
                }
            }
        }
        if (Messenger.eventTable.ContainsKey("LookForAction")) {
            Messenger.Broadcast<ActionThread>("LookForAction", this);
        }
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && UIManager.Instance.characterInfoUI.currentlyShowingCharacter.id == _party.id) {
            Debug.Log(actionLog);
        }
//#if UNITY_EDITOR
//        _party.actionData.actionHistory.Add(Utilities.GetDateString(GameManager.Instance.Today()) + " " + actionLog);
//#endif
        CharacterActionAdvertisement chosenActionAd = PickAction();
        chosenAction = chosenActionAd.action;
        chosenObject = chosenActionAd.targetObject;
        //#if UNITY_EDITOR
        //_party.actionData.actionHistory.Add(Utilities.GetDateString(GameManager.Instance.Today()) + 
        //    " Chosen action: " + chosenAction.actionType.ToString() + " at " + chosenAction.state.obj.objectLocation.landmarkName + "(" + chosenAction.state.obj.objectLocation.tileLocation.tileName + ")");
        //#endif

        //Check Prerequisites, currently for resource prerequisites only
        //CheckPrerequisites(chosenAction);

    }
    //private void CheckPrerequisites(CharacterAction characterAction) {
    //    if (HasPrerequisite(characterAction)) {
    //        if (CanDoPrerequisite(characterAction)) {
    //            DoPrerequisite(characterAction, null, null);
    //        } else {
    //            RemoveActionFromChoices(characterAction);
    //            chosenAction = PickAction();
    //            CheckPrerequisites(chosenAction);
    //        }
    //    }
    //}
    //private bool CanDoPrerequisite(CharacterAction characterAction) {
    //    if(HasPrerequisite(characterAction)) {
    //        for (int i = 0; i < characterAction.actionData.prerequisites.Count; i++) {
    //            IPrerequisite prerequisite = characterAction.actionData.prerequisites[i];
    //            if (_party.DoesSatisfiesPrerequisite(prerequisite)) {
    //                continue;
    //            }
    //            if (prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
    //                ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
    //                CharacterAction satisfyingAction = GetActionThatMatchesResourcePrerequisite(resourcePrerequisite);
    //                if (satisfyingAction == null || (satisfyingAction != null && !CanDoPrerequisite(satisfyingAction))) {
    //                    return false;
    //                }
    //            }
    //        }
    //    }
    //    return true;
    //}
    //private void DoPrerequisite(CharacterAction characterAction, ChainAction parentAction, IPrerequisite iprerequisite) {
    //    ChainAction chainAction = new ChainAction();
    //    chainAction.parentChainAction = parentAction;
    //    chainAction.action = characterAction;
    //    chainAction.prerequisite = iprerequisite;
    //    chainAction.satisfiedPrerequisites = new List<ChainAction>();
    //    chainAction.finishedPrerequisites = new List<IPrerequisite>();
    //    if (parentAction != null) {
    //        parentAction.satisfiedPrerequisites.Add(chainAction);
    //    }
    //    chosenChainAction = chainAction;
    //    if (HasPrerequisite(characterAction)) {
    //        for (int i = 0; i < characterAction.actionData.prerequisites.Count; i++) {
    //            IPrerequisite prerequisite = characterAction.actionData.prerequisites[i];
    //            if (_party.DoesSatisfiesPrerequisite(prerequisite)) {
    //                chainAction.finishedPrerequisites.Add(prerequisite);
    //                continue;
    //            }
    //            if (prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
    //                ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
    //                CharacterAction satisfyingAction = GetActionThatMatchesResourcePrerequisite(resourcePrerequisite);
    //                if (satisfyingAction != null) {
    //                    DoPrerequisite(satisfyingAction, chainAction, prerequisite);
    //                }
    //            }
    //        }
    //    }
    //}
    //private bool HasPrerequisite(CharacterAction characterAction) {
    //    return characterAction.actionData.prerequisites != null;
    //}
    //private CharacterAction GetActionThatMatchesResourcePrerequisite(ResourcePrerequisite resourcePrerequisite) {
    //    for (int i = 0; i < allChoices.Count; i++) {
    //        CharacterAction action = allChoices[i].action;
    //        if (action.actionData.advertisedResource != RESOURCE.NONE && resourcePrerequisite.resourceType != RESOURCE.NONE && action.actionData.advertisedResource == resourcePrerequisite.resourceType && action.state.obj.resourceInventory != null) {
    //            if (action.state.obj.resourceInventory[resourcePrerequisite.resourceType] >= resourcePrerequisite.amount) {
    //                return action;
    //            }
    //        }
    //    }
    //    return null;
    //}
    private void ReturnAction() {
        _party.actionData.ReturnActionFromThread(chosenAction, chosenObject, chosenChainAction);
    }
    private void PutToChoices(CharacterAction action, IObject targetObject, float advertisement) {
        CharacterActionAdvertisement actionAdvertisement = new CharacterActionAdvertisement();
        actionAdvertisement.Set(action, targetObject, advertisement);
        allChoices.Add(actionAdvertisement);
       
    }
    private CharacterActionAdvertisement PickAction() {
        choices[0].Reset();
        choices[1].Reset();
        choices[2].Reset();
        float advertisement = 0f;
        CharacterAction action = null;
        IObject targetObject = null;
        for (int i = 0; i < allChoices.Count; i++) {
            action = allChoices[i].action;
            targetObject = allChoices[i].targetObject;
            advertisement = allChoices[i].advertisement;
            if (choices[0].action == null) {
                choices[0].Set(action, targetObject, advertisement);
            } else {
                if (advertisement > choices[0].advertisement) {
                    choices[0].Set(action, targetObject, advertisement);
                }
            }
            //if (choices[0].action == null) {
            //    choices[0].Set(action, advertisement);
            //} else if (choices[1].action == null) {
            //    choices[1].Set(action, advertisement);
            //} else if (choices[2].action == null) {
            //    choices[2].Set(action, advertisement);
            //} else {
            //    if (choices[0].advertisement <= choices[1].advertisement && choices[0].advertisement <= choices[2].advertisement) {
            //        if (advertisement > choices[0].advertisement) {
            //            choices[0].Set(action, advertisement);
            //        }
            //    } else if (choices[1].advertisement <= choices[0].advertisement && choices[1].advertisement <= choices[2].advertisement) {
            //        if (advertisement > choices[1].advertisement) {
            //            choices[1].Set(action, advertisement);
            //        }
            //    } else if (choices[2].advertisement <= choices[0].advertisement && choices[2].advertisement <= choices[1].advertisement) {
            //        if (advertisement > choices[2].advertisement) {
            //            choices[2].Set(action, advertisement);
            //        }
            //    }
            //}
        }

        //int maxChoice = 3;
        //if (choices[1].action == null) {
        //    maxChoice = 1;
        //} else if (choices[2].action == null) {
        //    maxChoice = 2;
        //}
        int chosenIndex = 0; //Utilities.rng.Next(0, maxChoice);
        CharacterActionAdvertisement chosenActionAd = choices[chosenIndex];

        if (chosenActionAd.action == null) {
            string error = _party.name + " could not find an action to do! Choices were ";
            for (int i = 0; i < choices.Length; i++) {
                CharacterActionAdvertisement currAd = choices[i];
                if (currAd.action != null) {
                    error += "\n" + currAd.action.actionType.ToString() + " " + currAd.targetObject.objectName + " at " + currAd.targetObject.objectLocation.landmarkName;
                }
            }
            throw new Exception(error);
        }
        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter != null && _party.icharacters.Contains(UIManager.Instance.characterInfoUI.currentlyShowingCharacter)) {
            Debug.Log("Chosen Action: " + chosenActionAd.action.actionData.actionName + " = " + chosenActionAd.advertisement + " (" + chosenActionAd.targetObject.objectName + " at " + chosenActionAd.targetObject.specificLocation.locationName + ")");
        }
        //Debug.Log("Chosen Action: " + chosenAction.actionData.actionName + " = " + choices[chosenIndex].advertisement + " (" + chosenAction.state.obj.objectName + " at " + chosenAction.state.obj.objectLocation.landmarkName + ")");
        return chosenActionAd;
    }

    private void RemoveActionFromChoices(CharacterAction action) {
        for (int i = 0; i < allChoices.Count; i++) {
            if(allChoices[i].action == action) {
                allChoices.RemoveAt(i);
                break;
            }
        }
    }
    public void AddToChoices(IObject iobject) {
        if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
            for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                CharacterAction action = iobject.currentState.actions[k];
                if (action.MeetsRequirements(_party, null) && action.CanBeDone(iobject) && action.CanBeDoneBy(_party, iobject)) { //Filter
                    float happinessIncrease = _party.TotalHappinessIncrease(action, iobject);
                    actionLog += "\n" + action.actionData.actionName + " = " + happinessIncrease + " (" + iobject.objectName + ")";
                    PutToChoices(action, iobject, happinessIncrease);
                }
            }
        }
    }
}
