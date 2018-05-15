using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class BuildAction : CharacterAction {
    private int _amountToIncrease;
    private string _structureName;
    private StructureObj _structureObject;
    private bool _isStructureInLandmark;

    public BuildAction(ObjectState state, string structureName) : base(state, ACTION_TYPE.BUILD) {
        _structureName = structureName;
        _needsSpecificTarget = true;
        _isStructureInLandmark = false;
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        _structureObject = ObjectManager.Instance.GetNewStructureObject(_structureName);
        _amountToIncrease = Mathf.RoundToInt((float) _structureObject.maxHP / (float) _actionData.duration);
    }
    public override void PerformAction(Character character) {
        base.PerformAction(character);
        if (!_isStructureInLandmark) {
            _isStructureInLandmark = _state.obj.objectLocation.AddObject(_structureObject);
        }

        ActionSuccess();
        GiveReward(NEEDS.FULLNESS, character);
        GiveReward(NEEDS.ENERGY, character);
        GiveReward(NEEDS.JOY, character);
        GiveReward(NEEDS.PRESTIGE, character);

        //TODO: Resources

        _structureObject.AdjustHP(_amountToIncrease);
        if (_structureObject.isHPFull) {
            EndAction(character);
        }
    }
    public override CharacterAction Clone(ObjectState state) {
        BuildAction buildAction = new BuildAction(state, _structureName);
        SetCommonData(buildAction);
        buildAction.Initialize();
        return buildAction;
    }
    public override bool CanBeDone() {
        if(_structureObject.currentState.stateName == "Under Construction") {
            return true;
        }
        return false;
    }
    #endregion
}
