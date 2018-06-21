using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Monster {
    private string _name;
    private MONSTER_TYPE _type;
    private MONSTER_CATEGORY _category;
    private int _level;
    private int _experienceDrop;
    private int _currentHP;
    private int _maxHP;
    private int _currentSP;
    private int _maxSP;
    private int _attackPower;
    private int _speed;
    private int _pDef;
    private int _mDef;
    private float _dodgeChance;
    private float _hitChance;
    private float _critChance;
    private List<Skill> _skills;
    private Dictionary<ELEMENT, float> _elementalWeaknesses;
    private Dictionary<ELEMENT, float> _elementalResistance;

    //To add item drops and their chances

    #region getters/setters
    public string name {
        get { return _name; }
    }
    public List<Skill> skills {
        get { return _skills; }
    }
    #endregion

    public Monster CreateNewCopy() {
        Monster newMonster = new Monster();
        newMonster._name = this._name;
        newMonster._type = this._type;
        newMonster._category = this._category;
        newMonster._level = this._level;
        newMonster._experienceDrop = this._experienceDrop;
        newMonster._maxHP = this._maxHP;
        newMonster._maxSP = this._maxSP;
        newMonster._attackPower = this._attackPower;
        newMonster._speed = this._speed;
        newMonster._pDef = this._pDef;
        newMonster._mDef = this._mDef;
        newMonster._dodgeChance = this._dodgeChance;
        newMonster._hitChance = this._hitChance;
        newMonster._critChance = this._critChance;

        newMonster._skills = new List<Skill>();
        for (int i = 0; i < this._skills.Count; i++) {
            newMonster._skills.Add(_skills[i].CreateNewCopy());
        }
        return newMonster;
    }

    public void SetData(MonsterComponent monsterComponent) {
        this._name = monsterComponent.name;
        this._type = monsterComponent.type;
        this._category = monsterComponent.category;
        this._experienceDrop = monsterComponent.experienceDrop;
        this._level = monsterComponent.level;
        this._maxHP = monsterComponent.maxHP;
        this._maxSP = monsterComponent.maxSP;
        this._attackPower = monsterComponent.attackPower;
        this._speed = monsterComponent.speed;
        this._pDef = monsterComponent.pDef;
        this._mDef = monsterComponent.mDef;
        this._dodgeChance = monsterComponent.dodgeChance;
        this._hitChance = monsterComponent.hitChance;
        this._critChance = monsterComponent.critChance;

        this._skills = new List<Skill>();
        for (int i = 0; i < monsterComponent.skillNames.Count; i++) {
            _skills.Add(SkillManager.Instance.allSkills[monsterComponent.skillNames[i]]);
        }
    }
}
