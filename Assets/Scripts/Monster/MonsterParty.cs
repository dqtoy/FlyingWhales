using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using ECS;

public class MonsterParty : NewParty {
    private MonsterObj _monsterObj;

    #region getters/setters
    public MonsterObj monsterObj {
        get { return _monsterObj; }
    }
    #endregion

    public MonsterParty() :base() {
#if !WORLD_CREATION_TOOL
        _monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
        _monsterObj.SetMonster(this);
        _icharacterObject = _monsterObj;
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
    #endregion
}
