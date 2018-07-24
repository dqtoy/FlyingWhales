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
    public override string name {
        get { return _owner.name + "'s Party"; }
    }
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

    public CharacterParty(ICharacter owner): base(owner) {
        _isIdle = false;
        _actionData = new ActionData(this);
#if !WORLD_CREATION_TOOL
        Messenger.AddListener(Signals.HOUR_ENDED, EverydayAction);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //ConstructResourceInventory();
#endif
    }

    public void CreateCharacterObject() {
#if !WORLD_CREATION_TOOL
        if (mainCharacter.characterClass != null && mainCharacter.characterClass.className == "Retired Hero") {
            _characterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.CHARACTER, "RetiredHeroObject") as CharacterObj;
        } else {
            _characterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.CHARACTER, "CharacterObject") as CharacterObj;
        }
        _characterObj.SetCharacter(this);
        _icharacterObject = _characterObj;
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
        if (_isIdle) {
            _actionData.Reset();
        }
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
        IObject targetObject = null;
        CharacterAction action = mainCharacter.GetRandomDesperateAction(ref targetObject);
        actionData.AssignAction(action, targetObject);
    }
    public void AssignRandomIdleAction() {
        IObject targetObject = null;
        CharacterAction action = mainCharacter.GetRandomIdleAction(ref targetObject);
        actionData.AssignAction(action, targetObject);
    }
    public bool IsOwnerDead() {
        return _owner.isDead;
    }
    #endregion

    #region Overrides
    public override void PartyDeath() {
        base.PartyDeath();
        actionData.DetachActionData();
    }
    public void DisbandPartyKeepOwner() {
        while (icharacters.Count != 1) {
            for (int i = 0; i < icharacters.Count; i++) {
                ICharacter currCharacter = icharacters[i];
                if (currCharacter.id != owner.id) {
                    RemoveCharacter(currCharacter);
                    break;
                }
            }
        }
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
        PathfindingManager.Instance.AddAgent(_icon.pathfinder);

    }
    protected override void RemoveListeners() {
        base.RemoveListeners();
        Messenger.RemoveListener(Signals.HOUR_ENDED, EverydayAction);
        Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.RemoveListener<ECS.Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    #endregion

    #region Outside Handlers
    public void OnCharacterSnatched(Character snatchedCharacter) {
        if (snatchedCharacter.id == _owner.id) {
            //snatched character was the main character of this party, disband it
            DisbandPartyKeepOwner();
        }
    }
    public void OnCharacterDied(Character diedCharacter) {
        if (diedCharacter.id == _owner.id && this.currentCombat == null) {
            //character that died was the main character of this party, disband it
            DisbandParty();
        }
    }
    #endregion
}
