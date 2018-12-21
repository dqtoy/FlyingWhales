using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class CharacterParty : Party {
    private bool _isIdle; //can't do action, needs will not deplete
    private CharacterObj _characterObj;
    private ActionData _actionData;

    #region getters/setters
    public override string name {
        get {
            return GetPartyName();
        }
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
    public Character characterOwner {
        get { return owner; }
    }
    public override CharacterAction currentAction {
        get { return _actionData.currentAction; }
    }
    public override IActionData iactionData {
        get { return _actionData; }
    }
    public override int currentDay {
        get { return _actionData.currentDay; }
    }
    public bool isBusy { //if the party's current action is not null and their action is not rest, they are busy
        get { return IsBusy(); }
    }
    #endregion

    public CharacterParty() : base (null){

    }

    public CharacterParty(Character owner): base(owner) {
        _isIdle = false;
        _actionData = new ActionData(this);
#if !WORLD_CREATION_TOOL
        //Messenger.AddListener(Signals.DAY_ENDED, EverydayAction);
        //Messenger.AddListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //ConstructResourceInventory();
#endif
    }

    public void CreateCharacterObject() {
#if !WORLD_CREATION_TOOL
        //if (mainCharacter.characterClass != null && mainCharacter.characterClass.className == "Retired Hero") {
        //    _characterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.CHARACTER, "RetiredHeroObject") as CharacterObj;
        //} else {
            _characterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.CHARACTER, "CharacterObject") as CharacterObj;
        //}
        _characterObj.SetCharacter(this);
        _icharacterObject = _characterObj;
#endif
    }

    #region Utilities
    private string GetPartyName() {
        if (owner is CharacterArmyUnit) {
            if (characters.Count > 1) {
                string name = "Army of:";
                for (int i = 0; i < characters.Count; i++) {
                    name += "\n" + characters[i].name;
                }
                return name;
            } else {
                return owner.name;
            }
        } else {
            return base.name;
        }
    }
    private void EverydayAction() {
        if (!_isIdle) {
            if (!this.owner.IsInOwnParty()) {
                //if this character is not in its own party, do not perform action!
                if (this.characterOwner.onDailyAction != null) {
                    this.characterOwner.onDailyAction();
                }
            } else {
                if (onDailyAction != null) {
                    onDailyAction();
                }
                if (!actionData.isCurrentActionFromEvent) {
                    //the character's current action is not from an event
                    if (this.characterOwner.onDailyAction != null) {
                        this.characterOwner.onDailyAction();
                    }
                } 
                //else {
                //    if (onDailyAction != null) {
                //        onDailyAction();
                //    }
                //    if (this.characterOwner.onDailyAction != null) {
                //        this.characterOwner.onDailyAction();
                //    }
                //}
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
        return _characters.Sum(x => x.role.GetTotalHappinessIncrease(action, targetObject));
    }
    public bool IsFull(NEEDS need) {
        for (int i = 0; i < _characters.Count; i++) {
            Character icharacter = _characters[i];
            if (!icharacter.role.IsFull(need)) {
                return false;
            }
        }
        return true;
    }
    public void AssignRandomMiscAction() {
        IObject targetObject = null;
        CharacterAction action = mainCharacter.GetRandomMiscAction(ref targetObject);
        actionData.AssignAction(action, targetObject);
    }
    public bool IsOwnerDead() {
        return _owner.isDead;
    }
    private bool IsBusy() {
        if (owner.minion != null) {
            //if the owner of the party is a minion, just check if it is enabled
            //if it is not enabled, means that the minion currently has an action
            return !owner.minion.isEnabled;
        }
        if (this.icon.isTravelling || (_actionData.currentAction != null && _actionData.currentAction.actionData.actionType != ACTION_TYPE.REST)) {
            return true;
        }
        return false;
    }
    #endregion

    #region Overrides
    public void DisbandPartyKeepOwner() {
        while (characters.Count != 1) {
            for (int i = 0; i < characters.Count; i++) {
                Character currCharacter = characters[i];
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

        _icon = characterIconGO.GetComponent<CharacterAvatar>();
        _icon.Init(this);
        //_icon = characterIconGO.GetComponent<CharacterIcon>();
        //_icon.SetCharacter(this);
        //_icon.SetAnimator(CharacterManager.Instance.GetAnimatorByRole(mainCharacter.role.roleType));
        //PathfindingManager.Instance.AddAgent(_icon.aiPath);
        //PathfindingManager.Instance.AddAgent(_icon.pathfinder);

    }
    public override void RemoveListeners() {
        base.RemoveListeners();
        //Messenger.RemoveListener(Signals.DAY_ENDED, EverydayAction);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    public override void EndAction() {
        _actionData.EndAction();
    }
    public override void DetachActionData() {
        _actionData.DetachActionData();
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
        if (diedCharacter.id == _owner.id) {
            //character that died was the main character of this party, disband it
            DisbandPartyKeepOwner();
        }
    }
    #endregion
}
