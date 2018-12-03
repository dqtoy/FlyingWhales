using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using ECS;

public class MonsterParty : Party {
    private string _name;
    private MonsterObj _monsterObj;
    private string _setupName;
    private MonsterActionData _actionData;

    #region getters/setters
    //public override string name {
    //    get { return _name; }
    //}
    public string setupName {
        get { return _setupName; }
    }
    public MonsterObj monsterObj {
        get { return _monsterObj; }
    }
    public MonsterActionData actionData {
        get { return _actionData; }
    }
    public override ICharacter owner {
        get { return mainCharacter; }
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
    #endregion

    public MonsterParty() : base(null) {
#if !WORLD_CREATION_TOOL
        _monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
        _monsterObj.SetMonster(this);
        _icharacterObject = _monsterObj;
        _actionData = new MonsterActionData(this);
        MonsterManager.Instance.allMonsterParties.Add(this);
        Messenger.AddListener(Signals.DAY_ENDED, EverydayAction);
        //ConstructResourceInventory();
#endif
    }

    #region Overrides
    /*
        Create a new icon for this character.
        Each character owns 1 icon.
            */
    public override void CreateIcon() {
        base.CreateIcon();
        GameObject characterIconGO = GameObject.Instantiate(MonsterManager.Instance.monsterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);
        _icon = characterIconGO.GetComponent<CharacterAvatar>();
        _icon.Init(this);
        //_icon = characterIconGO.GetComponent<CharacterIcon>();
        //_icon.SetCharacter(this);
        //PathfindingManager.Instance.AddAgent(_icon.aiPath);
    }
    public override void PartyDeath() {
        if (_isDead) {
            return;
        }
        base.PartyDeath();
        MonsterManager.Instance.allMonsterParties.Remove(this);
        Messenger.Broadcast(Signals.MONSTER_PARTY_DIED, this);
    }
    public override void EndAction() {
        _actionData.EndAction();
    }
    public override void RemoveListeners() {
        base.RemoveListeners();
        Messenger.RemoveListener(Signals.DAY_ENDED, EverydayAction);
    }
    public override void DetachActionData() {
        _actionData.DetachActionData();
    }
    #endregion

    #region Utilities
    public void SetName(string name) {
        _name = name;
    }
    public void SetSetupName(string setupName) {
        _setupName = setupName;
    }
    private void EverydayAction() {
        if (onDailyAction != null) {
            onDailyAction();
        }
    }
    #endregion

}
