using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Minion : IUnit {

    private MinionItem _minionItem;
    private PlayerAbility _ability;
    private ICharacter _icharacter;
    private IInteractable _target;
    private DEMON_TYPE _type;
    private string _strType;
    private int _lvl;
    private int _exp;
    private int _indexDefaultSort;

    private bool _isEnabled;


    #region getters/setters
    public string name {
        get { return icharacter.name; }
    }
    public PlayerAbility ability {
        get { return _ability; }
    }
    public MinionItem minionItem {
        get { return _minionItem; }
    }
    public ICharacter icharacter {
        get { return _icharacter; }
    }
    public bool isEnabled {
        get { return _isEnabled; }
    }
    public int lvl {
        get { return _lvl; }
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

    public Minion(ICharacter icharacter, PlayerAbility ability) {
        _icharacter = icharacter;
        _ability = ability;
        _lvl = 1;
        _exp = 0;
        _type = (DEMON_TYPE) UnityEngine.Random.Range(0, System.Enum.GetValues(typeof(DEMON_TYPE)).Length);
        _strType = Utilities.NormalizeString(_type.ToString());
        //PlayerManager.Instance.player.demonicPortal.AddCharacterHomeOnLandmark(_icharacter);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(_icharacter.ownParty);
        _icharacter.SetMinion(this);
    }
    public Minion(ICharacter icharacter, PlayerAbility ability, DEMON_TYPE demonType) {
        _icharacter = icharacter;
        _ability = ability;
        _lvl = 1;
        _exp = 0;
        _type = demonType;
        _strType = Utilities.NormalizeString(_type.ToString());
        //_strType = Utilities.NormalizeString(_type.ToString());
        //PlayerManager.Instance.player.demonicPortal.AddCharacterHomeOnLandmark(_icharacter);
        //PlayerManager.Instance.player.demonicPortal.AddCharacterToLocation(_icharacter.ownParty);
        _icharacter.SetMinion(this);
    }
    public void SetDemonType(DEMON_TYPE type) {
        _type = type;
    }
    public void SendMinionToPerformAbility(IInteractable target) {
        _target = target;
        _icharacter.ownParty.GoToLocation(target.specificLocation, PATHFINDING_MODE.PASSABLE, () => DoAbility());
    }

    private void DoAbility() {
        _ability.DoAbility(_target);

        //Change activate button to recall button
    }

    public void SetEnabledState(bool state) {
        _isEnabled = state;
        _minionItem.SetEnabledState(state);
    }
    public void SetMinionItem(MinionItem minionItem) {
        _minionItem = minionItem;
    }
    public void AdjustExp(int amount) {
        _exp += amount;
        if(_exp >= 100) {
            LevelUp();
            _exp = 0;
        }else if (_exp < 0) {
            _exp = 0;
        }
        _minionItem.UpdateMinionItem();
    }
    public void SetLevel(int level) {
        _lvl = level;
    }
    public void AdjustLevel(int amount) {
        _lvl += amount;
    }
    public void LevelUp() {
        AdjustLevel(1);
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
}
