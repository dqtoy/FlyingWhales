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
    public Character characterOwner {
        get { return owner as Character; }
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
        if (!this.owner.IsInOwnParty()) {
            //if this character is not in its own party, do not perform action!
            return;
        }
        if (!_isIdle) {
            if (characterOwner.HasEventScheduled(GameManager.Instance.Today()) && !actionData.isCurrentActionFromEvent) {
                //the character has an event action scheduled for today, and it's current action is not from an event
                //queue the next action as the event action
                GameEvent scheduledEvent = characterOwner.GetScheduledEvent(GameManager.Instance.Today());
                //characterOwner.AddActionToQueue(scheduledEvent.action, scheduledEvent.targetObject);
                //actionData.EndAction(); //end current action after queueing event action
            } else {
                if (onDailyAction != null) {
                    onDailyAction();
                }
            }
            
            //Disabled everyday action for other characters in party
            //for (int i = 0; i < _icharacters.Count; i++) {
            //    _icharacters[i].EverydayAction();
            //}
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
    public void AssignRandomMiscAction() {
        IObject targetObject = null;
        string actionLog = string.Empty;
        CharacterAction action = mainCharacter.GetRandomMiscAction(ref targetObject, ref actionLog);
        actionData.AssignAction(action, targetObject);
    }
    public bool IsOwnerDead() {
        return _owner.isDead;
    }
    #endregion

    #region Overrides
    public override void PartyDeath() {
        base.PartyDeath();
        Debug.Log(this.name + " detached its action data");
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
