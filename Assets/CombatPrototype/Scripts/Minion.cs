using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;
using System;

public class Minion : IUnit {

    private PlayerCharacterItem _characterItem;
    //private PlayerAbility _ability;
    private Area _currentlyExploringArea;
    private Area _currentlyAttackingArea;

    private ICharacter _icharacter;
    private IInteractable _target;
    private DEMON_TYPE _type;
    private string _strType;
    //private int _lvl;
    private int _exp;
    private int _indexDefaultSort;
    private bool _isEnabled;
    private Action _travelBackAction;

    #region getters/setters
    public string name {
        get { return icharacter.name; }
    }
    //public PlayerAbility ability {
    //    get { return _ability; }
    //}
    public Area currentlyExploringArea {
        get { return _currentlyExploringArea; }
    }
    public Area currentlyAttackingArea {
        get { return _currentlyAttackingArea; }
    }
    public PlayerCharacterItem minionItem {
        get { return _characterItem; }
    }
    public ICharacter icharacter {
        get { return _icharacter; }
    }
    public bool isEnabled {
        get { return _isEnabled; }
    }
    public int lvl {
        get { return _icharacter.level; }
    }
    public int exp {
        get { return _exp; }
    }
    public int indexDefaultSort {
        get { return _indexDefaultSort; }
    }
    public DEMON_TYPE type {
        get { return _type; }
    }
    public string strType {
        get { return _strType; }
    }
    public Party party {
        get { return icharacter.ownParty; }
    }
    #endregion

    public Minion(ICharacter icharacter) {
        _icharacter = icharacter;
        //_ability = ability;
        //_lvl = 1;
        _exp = 0;
        _type = DEMON_TYPE.NONE;
        _strType = Utilities.NormalizeString(_type.ToString());
        _isEnabled = true;
        //PlayerManager.Instance.player.demonicPortal.AddCharacterHomeOnLandmark(_icharacter);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(_icharacter.ownParty);
        _icharacter.SetMinion(this);
        _icharacter.SetName(RandomNameGenerator.Instance.GenerateMinionName());
        _icharacter.DisableInteractionGeneration();
        _icharacter.characterIntel.SetObtainedState(true);
        _icharacter.ownParty.icon.SetVisualState(true);
    }
    public Minion(ICharacter icharacter, DEMON_TYPE demonType) {
        _icharacter = icharacter;
        //_ability = ability;
        //_lvl = 1;
        _exp = 0;
        _type = demonType;
        _strType = Utilities.NormalizeString(_type.ToString());
        _isEnabled = true;
        //_strType = Utilities.NormalizeString(_type.ToString());
        //PlayerManager.Instance.player.demonicPortal.AddCharacterHomeOnLandmark(_icharacter);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(_icharacter.ownParty);
        _icharacter.SetMinion(this);
        _icharacter.DisableInteractionGeneration();
        _icharacter.characterIntel.SetObtainedState(true);
        _icharacter.ownParty.icon.SetVisualState(true);
    }
    public void SetDemonType(DEMON_TYPE type) {
        _type = type;
    }
    public void SendMinionToPerformAbility(IInteractable target) {
        _target = target;
        _icharacter.ownParty.GoToLocation(target.specificLocation, PATHFINDING_MODE.PASSABLE);
    }
    public void SetEnabledState(bool state) {
        if (icharacter.IsInOwnParty()) {
            //also set enabled state of other party members
            for (int i = 0; i < icharacter.ownParty.icharacters.Count; i++) {
                ICharacter otherChar = icharacter.ownParty.icharacters[i];
                if (otherChar.id != icharacter.id && otherChar.minion != null) {
                    otherChar.minion.SetEnabledState(state);
                    if (state) {
                        //Since the otherChar will be removed from the party when he is not the owner and state is true, reduce loop count so no argument exception error will be called
                        i--;
                    }
                }
            }
        } else {
            //If character is not own party and is enabled, automatically put him in his own party so he can be used again
            if (state) {
                icharacter.currentParty.RemoveCharacter(icharacter);
            }
        }
        _isEnabled = state;
        _characterItem.SetEnabledState(state);
    }
    public void SetExploringArea(Area area) {
        _currentlyExploringArea = area;
        if (icharacter.IsInOwnParty()) {
            for (int i = 0; i < icharacter.currentParty.icharacters.Count; i++) {
                ICharacter otherChar = icharacter.ownParty.icharacters[i];
                if (otherChar.id != icharacter.id) {
                    icharacter.currentParty.icharacters[i].minion.SetExploringArea(area);
                }
            }
        }
    }
    public void SetAttackingArea(Area area) {
        _currentlyAttackingArea = area;
        if (icharacter.IsInOwnParty()) {
            for (int i = 0; i < icharacter.currentParty.icharacters.Count; i++) {
                ICharacter otherChar = icharacter.ownParty.icharacters[i];
                if (otherChar.id != icharacter.id) {
                    icharacter.currentParty.icharacters[i].minion.SetAttackingArea(area);
                }
            }
        }
    }
    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        _characterItem = item;
    }
    public void AdjustExp(int amount) {
        _exp += amount;
        if(_exp >= 100) {
            LevelUp();
            _exp = 0;
        }else if (_exp < 0) {
            _exp = 0;
        }
        _characterItem.UpdateMinionItem();
    }
    public void SetLevel(int level) {
        icharacter.SetLevel(level);
    }
    public void LevelUp() {
        icharacter.LevelUp();
    }
    public void SetIndexDefaultSort(int index) {
        _indexDefaultSort = index;
    }
    public void GoToAssignment(IInteractable interactable) {
        SetEnabledState(false);
        icharacter.currentParty.specificLocation.RemoveCharacterFromLocation(icharacter.currentParty);
        interactable.specificLocation.AddCharacterToLocation(icharacter.currentParty);
    }
    public void GoBackFromAssignment() {
        if (icharacter.isDead) {
            return;
        }
        SetEnabledState(true);
        icharacter.currentParty.specificLocation.RemoveCharacterFromLocation(icharacter.currentParty);
        PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(icharacter.currentParty);
    }
    public void TravelToAssignment(BaseLandmark target, Action action) {
        _icharacter.currentParty.GoToLocation(target, PATHFINDING_MODE.PASSABLE, () => action());
    }
    public void TravelBackFromAssignment(Action action = null) {
        _travelBackAction = action;
        if (_icharacter.currentParty.icon.isTravelling) {
            _icharacter.currentParty.CancelTravel(() => TravelBackFromAssignmentComplete());
        } else {
            _icharacter.currentParty.GoToLocation(PlayerManager.Instance.player.demonicPortal, PATHFINDING_MODE.PASSABLE, () => TravelBackFromAssignmentComplete());
        }
    }
    private void TravelBackFromAssignmentComplete() {
        SetEnabledState(true);
        if(_travelBackAction != null) {
            _travelBackAction();
            _travelBackAction = null;
        }
    }

    #region Rewards
    public void ClaimReward(Reward reward) {
        switch (reward.rewardType) {
            case REWARD.EXP:
            AdjustExp(reward.amount);
            break;
            default:
            break;
        }
    }
    #endregion
}
