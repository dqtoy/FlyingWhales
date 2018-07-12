using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class BuildStructureQuestData : CharacterQuestData {
    private RESOURCE _currentDepositingResource;
    private Dictionary<CharacterAction, IObject> _allActionsThatCanObtainResource;
    public List<string> classesThatCanObtainResource;
    public List<string> allClasses;

    #region getters/setters
    public RESOURCE currentDepositingResource {
        get { return _currentDepositingResource; }
    }
    public Dictionary<CharacterAction, IObject> allActionsThatCanObtainResource {
        get { return _allActionsThatCanObtainResource; }
    }
    #endregion

    public BuildStructureQuestData(Quest parentQuest, Character owner) : base(parentQuest, owner) {
        _allActionsThatCanObtainResource = new Dictionary<CharacterAction, IObject>();
        classesThatCanObtainResource = new List<string>();
        allClasses = new List<string>(CharacterManager.Instance.classesDictionary.Keys);
    }

    #region Utilities
    public void SetDepositingResource(RESOURCE resource) {
        _currentDepositingResource = resource;
    }
    public void ResetAllActionsThatCanObtainResource() {
        _allActionsThatCanObtainResource.Clear();
    }
    public void AddToAllActionsThatCanObtainResource(CharacterAction action, IObject targetObject) {
        _allActionsThatCanObtainResource.Add(action, targetObject);
    }
    public void AddToChoicesOfAllActionsThatCanObtainResource(IObject targetObject) {
        if (targetObject.currentState.actions != null && targetObject.currentState.actions.Count > 0) {
            BuildStructureQuest buildStructureQuest = _parentQuest as BuildStructureQuest;
            for (int k = 0; k < targetObject.currentState.actions.Count; k++) {
                CharacterAction action = targetObject.currentState.actions[k];
                if (buildStructureQuest.lackingResources.Contains(action.actionData.resourceGiven)) {
                    if (action.MeetsRequirements(_owner.party, null) && action.CanBeDone(targetObject) && action.CanBeDoneBy(_owner.party, targetObject)) { //Filter
                        AddToAllActionsThatCanObtainResource(action, targetObject);
                    }
                    for (int l = 0; l < allClasses.Count; l++) {
                        if (action.MeetsRequirements(allClasses[l], null)) {
                            AddClassThatCanObtainResource(allClasses[l]);
                            l--;
                        }
                    }
                }
            }
        }
    }
    public void AddClassThatCanObtainResource(string className) {
        allClasses.Remove(className);
        classesThatCanObtainResource.Add(className);
    }
    public void ResetClassesThatCanObtainResource() {
        if(classesThatCanObtainResource.Count > 0) {
            allClasses.AddRange(classesThatCanObtainResource);
            classesThatCanObtainResource.Clear();
        }
    }
    #endregion
}
