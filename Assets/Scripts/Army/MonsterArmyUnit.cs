using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MonsterArmyUnit : Monster {

    public int armyCount { get; private set; }
    private int armyCap;

    #region getters/setters
    public override int attackPower {
        get { return _attackPower * armyCount; }
    }
    //public override int speed {
    //    get { return _speed * armyCount; }
    //}
    //public override int maxHP {
    //    get { return _maxHP * armyCount; }
    //}
    #endregion

    public MonsterArmyUnit(int armyCount) {
        this.armyCap = GetArmyCap();
        this.armyCount = armyCount;
    }

    public void AdjustArmyCount(int adjustment) {

        armyCount += adjustment;
        if (armyCap == -1) {
            armyCount = Mathf.Max(0, armyCount);
        } else {
            armyCount = Mathf.Clamp(armyCount, 0, armyCap);
        }
    }
    private int GetArmyCap() {
        return -1;
    }
    public int GetProductionCost() {
        return 20;
    }
    public new MonsterArmyUnit CreateNewCopy() {
        MonsterArmyUnit newMonster = new MonsterArmyUnit(this.armyCount);
        newMonster._name = this._name;
        newMonster._type = this._type;
        newMonster._category = this._category;
        newMonster._level = this._level;
        newMonster._experienceDrop = this._experienceDrop;
        newMonster._maxHP = this._maxHP;
        newMonster._maxSP = this._maxSP;
        newMonster._attackPower = this._attackPower;
        newMonster._speed = this._speed;
        newMonster._dodgeChance = this._dodgeChance;
        newMonster._hitChance = this._hitChance;
        newMonster._critChance = this._critChance;
        newMonster._isSleepingOnSpawn = this._isSleepingOnSpawn;
        newMonster._startingArmyCount = this._startingArmyCount;
        newMonster._portraitSettings = this._portraitSettings.CreateNewCopy();
        //#if !WORLD_CREATION_TOOL
        //        newMonster._monsterObj = ObjectManager.Instance.CreateNewObject(OBJECT_TYPE.MONSTER, "MonsterObject") as MonsterObj;
        //        newMonster._monsterObj.SetMonster(newMonster);
        //#endif
        newMonster._skills = new List<Skill>();
        for (int i = 0; i < this._skills.Count; i++) {
            newMonster._skills.Add(_skills[i].CreateNewCopy());
        }
        newMonster._elementalWeaknesses = new Dictionary<ELEMENT, float>(this._elementalWeaknesses);
        newMonster._elementalResistances = new Dictionary<ELEMENT, float>(this._elementalResistances);
        newMonster._itemDropsLookup = new Dictionary<string, float>(this._itemDropsLookup);

        return newMonster;
    }

    #region Overrides
    public override void AdjustHP(int amount, Character killer = null) {
        int previous = this._currentHP;
        this._currentHP += amount;
        this._currentHP = Mathf.Clamp(this._currentHP, 0, maxHP);
        int diff = maxHP - _currentHP;
        if (diff > 0) {
            int armyLoss = diff / _maxHP;
            AdjustArmyCount(-armyLoss);
        }
        if (previous != this._currentHP) {
            if (this._currentHP == 0) {
                FaintOrDeath(killer);
            }
        }
    }
    public override void ConstructMonsterData() {
        base.ConstructMonsterData();
        AdjustArmyCount(_startingArmyCount);
    }
    #endregion
}
