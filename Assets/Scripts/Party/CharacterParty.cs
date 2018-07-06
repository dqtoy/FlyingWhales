using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using ECS;

public class CharacterParty : NewParty {
    public delegate void DailyAction();
    public DailyAction onDailyAction;

    private bool _isIdle; //can't do action, needs will not deplete
    private CharacterObj _characterObj;
    private ActionData _actionData;

    #region getters/setters
    public bool isIdle {
        get { return _isIdle; }
    }
    public CharacterObj characterObject {
        get { return _characterObj; }
    }
    public ActionData actionData {
        get { return _actionData; }
    }
    #endregion

    public CharacterParty(): base() {
        _isIdle = false;
        _actionData = new ActionData(this);
#if !WORLD_CREATION_TOOL
        _characterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.CHARACTER, "CharacterObject") as CharacterObj;
        _characterObj.SetCharacter(this);
        _icharacterObject = _characterObj;
        Messenger.AddListener(Signals.HOUR_ENDED, EverydayAction);
        //ConstructResourceInventory();
#endif
    }

    #region Utilities
    private void EverydayAction() {
        if (!_isIdle) {
            if (onDailyAction != null) {
                onDailyAction();
            }
            for (int i = 0; i < _icharacters.Count; i++) {
                _icharacters[i].EverydayAction();
            }
        }
    }
    //If true, party can't do daily action (onDailyAction), i.e. actions, needs
    public void SetIsIdle(bool state) {
        _isIdle = state;
    }
    public float TotalHappinessIncrease(CharacterAction action, IObject targetObject) {
        return _icharacters.Sum(x => x.role.GetTotalHappinessIncrease(action, targetObject));
    }
    public bool IsFull(NEEDS need) {
        for (int i = 0; i < _icharacters.Count; i++) {
            ICharacter icharacter = _icharacters[i];
            if (!icharacter.role.IsFull(need)) {
                return false;
            }
        }
        return true;
    }
    public void AssignRandomDesperateAction() {
        CharacterAction action = _icharacters[0].GetRandomDesperateAction();
        actionData.AssignAction(action, characterObject);
    }
    public void AssignRandomIdleAction() {
        CharacterAction action = _icharacters[0].GetRandomIdleAction();
        actionData.AssignAction(action, characterObject);
    }
    #endregion


    #region Overrides
    public override void PartyDeath() {
        base.PartyDeath();
        Messenger.RemoveListener(Signals.HOUR_ENDED, EverydayAction);
        actionData.DetachActionData();
    }
    /*
        Create a new icon for this character.
        Each character owns 1 icon.
            */
    public override void CreateIcon() {
        base.CreateIcon();
        GameObject characterIconGO = GameObject.Instantiate(CharacterManager.Instance.characterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);
        _icon = characterIconGO.GetComponent<CharacterIcon>();
        _icon.SetCharacter(this);
        PathfindingManager.Instance.AddAgent(_icon.aiPath);
    }
    #endregion
}
