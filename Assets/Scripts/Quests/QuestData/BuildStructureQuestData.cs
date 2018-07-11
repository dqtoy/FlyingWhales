using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class BuildStructureQuestData : CharacterQuestData {
    private RESOURCE _currentDepositingResource;

    #region getters/setters
    public RESOURCE currentDepositingResource {
        get { return _currentDepositingResource; }
    }
    #endregion

    public BuildStructureQuestData(Quest parentQuest, Character owner) : base(parentQuest, owner) {
    }

    #region Utilities
    public void SetDepositingResource(RESOURCE resource) {
        _currentDepositingResource = resource;
    }
    #endregion
}
