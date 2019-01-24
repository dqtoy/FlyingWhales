using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuildAction : CharacterAction {
    private int _amountToIncrease;
    private string _structureName;
    private StructureObj _structureObject;
    private bool _isStructureInLandmark;

    public BuildAction(string structureName) : base(ACTION_TYPE.BUILD) {
        _structureName = structureName;
        _isStructureInLandmark = false;
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        _structureObject = ObjectManager.Instance.GetNewStructureObject(_structureName);
        _amountToIncrease = Mathf.RoundToInt((float) _structureObject.maxHP / (float) _actionData.duration);
    }
    public override void PerformAction(Party party, IObject targetObject) {
        base.PerformAction(party, targetObject);
        //if (!_isStructureInLandmark) {
        //    _isStructureInLandmark = _state.obj.objectLocation.AddObject(_structureObject);
        //}

        ActionSuccess(targetObject);
        if (party is CharacterParty) {
            GiveAllReward(party as CharacterParty);
        }

        //TODO: Resources

        _structureObject.AdjustHP(_amountToIncrease);
        if (_structureObject.isHPFull) {
            EndAction(party, targetObject);
        }
    }
    public override CharacterAction Clone() {
        BuildAction buildAction = new BuildAction(_structureName);
        SetCommonData(buildAction);
        buildAction.Initialize();
        return buildAction;
    }
    public override bool CanBeDone(IObject targetObject) {
        if(_structureObject.currentState.stateName == "Under Construction") {
            return true;
        }
        return false;
    }
    #endregion
}
