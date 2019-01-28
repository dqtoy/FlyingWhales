using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;

public class Minion {

    //private PlayerCharacterItem _characterItem;
    //private PlayerAbility _ability;
    private Area _currentlyExploringArea;
    private Area _currentlyAttackingArea;
    private Area _currentlyCollectingArea;

    private Character _character;
    private IInteractable _target;
    //private DEMON_TYPE _type;
    //private string _strType;
    //private int _lvl;
    private int _exp;
    private int _indexDefaultSort;
    private bool _isEnabled;
    private Action _travelBackAction;

    #region getters/setters
    public string name {
        get { return character.name; }
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
        get { return character.playerCharacterItem; }
    }
    public Character character {
        get { return _character; }
    }
    public bool isEnabled {
        get { return _isEnabled; }
    }
    public int lvl {
        get { return _character.level; }
    }
    public int exp {
        get { return _exp; }
    }
    public int indexDefaultSort {
        get { return _indexDefaultSort; }
    }
    //public DEMON_TYPE type {
    //    get { return _type; }
    //}
    //public string strType {
    //    get { return _strType; }
    //}
    //public Party party {
    //    get { return icharacter.ownParty; }
    //}
    #endregion

    public Minion(Character icharacter, bool keepData) {
        _character = icharacter;
        //_ability = ability;
        //_lvl = 1;
        _exp = 0;
        //_type = DEMON_TYPE.NONE;
        //_strType = Utilities.NormalizeString(_type.ToString());
        _isEnabled = true;
        //PlayerManager.Instance.player.demonicPortal.AddCharacterHomeOnLandmark(_icharacter);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(_icharacter.ownParty);
        _character.SetMinion(this);
        _character.DisableInteractionGeneration();
        _character.characterToken.SetObtainedState(true);
        _character.ownParty.icon.SetVisualState(true);

        if (!keepData) {
            _character.SetName(RandomNameGenerator.Instance.GenerateMinionName());
        }
    }
    //public void SetDemonType(DEMON_TYPE type) {
    //    _type = type;
    //}
    //public void SendMinionToPerformAbility(IInteractable target) {
    //    _target = target;
    //    _character.ownParty.GoToLocation(target.specificLocation, PATHFINDING_MODE.PASSABLE);
    //}
    public void SetEnabledState(bool state) {
        if (character.IsInOwnParty()) {
            //also set enabled state of other party members
            for (int i = 0; i < character.ownParty.characters.Count; i++) {
                Character otherChar = character.ownParty.characters[i];
                if (otherChar.id != character.id && otherChar.minion != null) {
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
                character.currentParty.RemoveCharacter(character);
            }
        }
        _isEnabled = state;
        minionItem.SetEnabledState(state);
    }
    public void SetExploringArea(Area area) {
        _currentlyExploringArea = area;
        if (character.IsInOwnParty()) {
            for (int i = 0; i < character.currentParty.characters.Count; i++) {
                Character otherChar = character.ownParty.characters[i];
                if (otherChar.id != character.id) {
                    character.currentParty.characters[i].minion.SetExploringArea(area);
                }
            }
        }
    }
    public void SetCollectingTokenArea(Area area) {
        _currentlyCollectingArea = area;
        if (character.IsInOwnParty()) {
            for (int i = 0; i < character.currentParty.characters.Count; i++) {
                Character otherChar = character.ownParty.characters[i];
                if (otherChar.id != character.id) {
                    character.currentParty.characters[i].minion.SetCollectingTokenArea(area);
                }
            }
        }
    }
    public void SetAttackingArea(Area area) {
        _currentlyAttackingArea = area;
        if (character.IsInOwnParty()) {
            for (int i = 0; i < character.currentParty.characters.Count; i++) {
                Character otherChar = character.ownParty.characters[i];
                if (otherChar.id != character.id) {
                    character.currentParty.characters[i].minion.SetAttackingArea(area);
                }
            }
        }
    }
    public void SetPlayerCharacterItem(PlayerCharacterItem item) {
        character.SetPlayerCharacterItem(item);
    }
    public void AdjustExp(int amount) {
        _exp += amount;
        if(_exp >= 100) {
            LevelUp();
            _exp = 0;
        }else if (_exp < 0) {
            _exp = 0;
        }
        //_characterItem.UpdateMinionItem();
    }
    public void SetLevel(int level) {
        character.SetLevel(level);
    }
    public void LevelUp() {
        character.LevelUp();
    }
    public void LevelUp(int amount) {
        character.LevelUp(amount);
    }
    public void SetIndexDefaultSort(int index) {
        _indexDefaultSort = index;
    }
    //public void GoToAssignment(BaseLandmark interactable) {
    //    SetEnabledState(false);
    //    character.currentParty.specificLocation.RemoveCharacterFromLocation(character.currentParty);
    //    interactable.AddCharacterToLocation(character.currentParty);
    //}
    //public void GoBackFromAssignment() {
    //    if (character.isDead) {
    //        return;
    //    }
    //    SetEnabledState(true);
    //    character.currentParty.specificLocation.RemoveCharacterFromLocation(character.currentParty);
    //    PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(character.currentParty);
    //}
    public void TravelToAssignment(BaseLandmark target, Action action) {
        _character.currentParty.GoToLocation(target.tileLocation.areaOfTile, PATHFINDING_MODE.PASSABLE, null, () => action());
    }
    public void TravelBackFromAssignment(Action action = null) {
        _travelBackAction = action;
        if (_character.currentParty.icon.isTravelling) {
            _character.currentParty.CancelTravel(() => TravelBackFromAssignmentComplete());
        } else {
            _character.currentParty.GoToLocation(PlayerManager.Instance.player.playerArea, PATHFINDING_MODE.PASSABLE, null, () => TravelBackFromAssignmentComplete());
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
        _character.ClaimReward(reward);
    }
    #endregion
}
