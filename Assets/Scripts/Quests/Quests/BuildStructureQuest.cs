using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class BuildStructureQuest : Quest {
    private StructurePrioritySetting _setting;
    private StructureObj _buildingStructure;

    public BuildStructureQuest(StructurePrioritySetting setting, HexTile landToBuild) : base(QUEST_TYPE.BUILD_STRUCTURE) {
        _setting = setting;
        CreateLandmarkForInitialization(landToBuild);
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
    public override CharacterAction GetQuestAction(Character character, CharacterQuestData data, ref IObject targetObject) {
        BuildStructureQuestData buildQuestData = data as BuildStructureQuestData;
        List<RESOURCE> lackingResources = new List<RESOURCE>();
        for (int i = 0; i < _setting.resourceCost.Count; i++) {
            Resource resource = _setting.resourceCost[i];
            if (_buildingStructure.resourceInventory[resource.resource] < resource.amount) {
                lackingResources.Add(resource.resource);
            }
        }
        //When a character asks for action, check which resource types are still lacking from the Quest and see if the character has at least 100 of it in his inventory. If yes, give the character Deposit action.
        CharacterParty characterParty = character.party;
        targetObject = _buildingStructure;
        for (int i = 0; i < lackingResources.Count; i++) {
            if (characterParty.characterObject.resourceInventory[lackingResources[i]] >= 100) { //resource type is still lacking
                //Give deposit action
                buildQuestData.SetDepositingResource(lackingResources[i]);
                DepositAction depositAction = _buildingStructure.currentState.GetAction(ACTION_TYPE.DEPOSIT) as DepositAction;
                return depositAction;
            }
        }

        //If not, check if character's class is an Excess Class
        if (character.home.excessClasses.Contains(character.characterClass.className)) {
            //If yes, perform Change Role to a Role that can obtain needed resource type
            foreach (CharacterClass characterClass in CharacterManager.Instance.classesDictionary.Values) {
                for (int i = 0; i < lackingResources.Count; i++) {
                    if (characterClass.harvestResources.Contains(lackingResources[i])) {
                        ChangeClassAction changeClassAction = character.party.characterObject.currentState.GetAction(ACTION_TYPE.CHANGE_CLASS) as ChangeClassAction;
                        changeClassAction.SetAdvertisedClass(characterClass.className);
                        targetObject = character.party.characterObject;
                        return changeClassAction;
                    }
                }
            }
        } else {
            //If not, abandon Quest and perform an Idle Action.
            buildQuestData.AbandonQuest();
            return character.GetRandomIdleAction();
        }
        return base.GetQuestAction(character, data, ref targetObject);
    }
    #endregion
}
