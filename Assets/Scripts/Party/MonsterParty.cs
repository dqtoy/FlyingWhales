using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using ECS;

public class MonsterParty : NewParty {
    private string _name;
    private MonsterObj _monsterObj;
    private string _setupName;

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
    public override ICharacter owner {
        get { return mainCharacter; }
    }
    #endregion

    public MonsterParty() : base(null) {
#if !WORLD_CREATION_TOOL
        _monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
        _monsterObj.SetMonster(this);
        _icharacterObject = _monsterObj;
        MonsterManager.Instance.allMonsterParties.Add(this);
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
        _icon = characterIconGO.GetComponent<CharacterIcon>();
        _icon.SetCharacter(this);
        PathfindingManager.Instance.AddAgent(_icon.aiPath);
    }
    public override void PartyDeath() {
        base.PartyDeath();
        MonsterManager.Instance.allMonsterParties.Remove(this);
        Messenger.Broadcast(Signals.MONSTER_PARTY_DIED, this);
    }
    #endregion

    #region Utilities
    public void SetName(string name) {
        _name = name;
    }
    public void SetSetupName(string setupName) {
        _setupName = setupName;
    }
    #endregion
}
