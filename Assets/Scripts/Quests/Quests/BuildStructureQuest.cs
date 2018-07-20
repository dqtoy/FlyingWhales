using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class BuildStructureQuest : Quest {
    private StructurePrioritySetting _setting;
    private StructureObj _buildingStructure;
    private List<RESOURCE> _lackingResources;

    private HexTile _targetTile;

    #region getters/setters
    public List<RESOURCE> lackingResources {
        get { return _lackingResources; }
    }
    public StructurePrioritySetting setting {
        get { return _setting; }
    }
    public HexTile targetTile {
        get { return _targetTile; }
    }
    public override GROUP_TYPE groupType { get { return GROUP_TYPE.SOLO; } }
    #endregion

    public BuildStructureQuest(StructurePrioritySetting setting, HexTile landToBuild) : base(QUEST_TYPE.BUILD_STRUCTURE) {
        _setting = setting;
        _lackingResources = new List<RESOURCE>();
        _targetTile = landToBuild;
        CreateLandmarkForInitialization(landToBuild);
        UpdateLackingResources();
    }

    #region Utilities
    private void CreateLandmarkForInitialization(HexTile landToBuild) {
        BaseLandmark newLandmark = LandmarkManager.Instance.CreateNewLandmarkOnTile(landToBuild, _setting.landmarkType);
        _buildingStructure = newLandmark.landmarkObj;
        ObjectState state = _buildingStructure.GetState("Under Construction");
        if(state != null) {
            _buildingStructure.ChangeState(state);
        }
    }
    #endregion

    #region overrides
    public override QuestAction GetQuestAction(Character character, CharacterQuestData data) {
        BuildStructureQuestData buildQuestData = data as BuildStructureQuestData;
        //When a character asks for action, check which resource types are still lacking from the Quest and see if the character has at least 100 of it in his inventory. If yes, give the character Deposit action.
        CharacterParty characterParty = character.party;
        IObject targetObject = _buildingStructure;
        for (int i = 0; i < _lackingResources.Count; i++) {
            if (characterParty.characterObject.resourceInventory[_lackingResources[i]] >= 100) { //character has more than 100 of the lacking resource
                //Give deposit action
                buildQuestData.SetDepositingResource(_lackingResources[i]);
                DepositAction depositAction = _buildingStructure.currentState.GetAction(ACTION_TYPE.DEPOSIT) as DepositAction;
                return new QuestAction(depositAction, targetObject);
            }
        }
        //If not, check if he has an action that can obtain that resource type
        CharacterAction actionThatCanObtainResource = ActionThatCanObtainResource(character, buildQuestData, ref targetObject);
        if (actionThatCanObtainResource != null) {
            return new QuestAction(actionThatCanObtainResource, targetObject);
        } else {
            //If not, check if character's class is an Excess Class
            if (character.home.excessClasses.Contains(character.characterClass.className)) {
                //If yes, perform Change Role to a Role that can obtain needed resource type
                if (buildQuestData.classesThatCanObtainResource.Count > 0) {
                    string chosenClassName = buildQuestData.classesThatCanObtainResource[Utilities.rng.Next(0, buildQuestData.classesThatCanObtainResource.Count)];
                    ChangeClassAction changeClassAction = character.party.characterObject.currentState.GetAction(ACTION_TYPE.CHANGE_CLASS) as ChangeClassAction;
                    changeClassAction.SetAdvertisedClass(chosenClassName);
                    targetObject = character.party.characterObject;
                    return new QuestAction(changeClassAction, targetObject);
                } else {
                    //If not, abandon Quest and perform an Idle Action.
                    buildQuestData.AbandonQuest();
                    return new QuestAction(character.GetRandomIdleAction(ref targetObject), targetObject);
                }
            } else {
                //If not, abandon Quest and perform an Idle Action.
                buildQuestData.AbandonQuest();
                return new QuestAction(character.GetRandomIdleAction(ref targetObject), targetObject);
            }
        }
        //return base.GetQuestAction(character, data, ref targetObject);
    }
    protected override string GetQuestName() {
        return "Build " + Utilities.NormalizeStringUpperCaseFirstLetters(_setting.landmarkType.ToString()) + " at " + _targetTile.locationName;
    }
    #endregion

    public void UpdateLackingResources() {
        _lackingResources.Clear();
        for (int i = 0; i < _setting.buildResourceCost.Count; i++) {
            Resource resource = _setting.buildResourceCost[i];
            if (_buildingStructure.resourceInventory[resource.resource] < resource.amount) {
                _lackingResources.Add(resource.resource);
            }
        }
        if(_lackingResources.Count <= 0) {
            //End quest
            _targetTile.landmarkOnTile.landmarkObj.ChangeState(_targetTile.landmarkOnTile.landmarkObj.states[0]);
            QuestManager.Instance.OnQuestDone(this);
        }
    }
    public List<Resource> GetNeededResources() {
        List<Resource> missingResources = new List<Resource>();
        for (int i = 0; i < _setting.buildResourceCost.Count; i++) {
            Resource neededResource = _setting.buildResourceCost[i];
            int currentBuildingAmount = _buildingStructure.resourceInventory[neededResource.resource];
            if (currentBuildingAmount < neededResource.amount) {
                missingResources.Add(new Resource(neededResource.resource, neededResource.amount - currentBuildingAmount));
            }
        }
        return missingResources;
    }

    private CharacterAction ActionThatCanObtainResource(Character character, BuildStructureQuestData buildQuestData, ref IObject targetObject) {
        CharacterParty party = character.party;
        buildQuestData.ResetAllActionsThatCanObtainResource();
        buildQuestData.ResetClassesThatCanObtainResource();
        for (int i = 0; i < party.currentRegion.landmarks.Count; i++) {
            BaseLandmark landmark = party.currentRegion.landmarks[i];
            StructureObj iobject = landmark.landmarkObj;
            if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                    CharacterAction action = iobject.currentState.actions[k];
                    if (_lackingResources.Contains(action.actionData.resourceGiven)) {
                        if (action.CanBeDone(iobject) && action.CanBeDoneBy(party, iobject)) { //Filter
                            if(action.MeetsRequirements(party, landmark)) {
                                buildQuestData.AddToAllActionsThatCanObtainResource(action, iobject);
                            }
                            for (int l = 0; l < buildQuestData.allClasses.Count; l++) {
                                if (action.MeetsRequirements(buildQuestData.allClasses[l], landmark)) {
                                    buildQuestData.AddClassThatCanObtainResource(buildQuestData.allClasses[l]);
                                    l--;
                                }
                            }
                        }
                    }
                }
            }
        }
        if (Messenger.eventTable.ContainsKey(Signals.BUILD_STRUCTURE_LOOK_ACTION)) {
            Messenger.Broadcast<BuildStructureQuestData>(Signals.BUILD_STRUCTURE_LOOK_ACTION, buildQuestData);
        }
        if(buildQuestData.allActionsThatCanObtainResource.Count > 0) {
            CharacterAction chosenAction = Utilities.GetRandomKeyFromDictionary(buildQuestData.allActionsThatCanObtainResource);
            targetObject = buildQuestData.allActionsThatCanObtainResource[chosenAction];
            return chosenAction;
        }
        return null;
    }
}
