using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CharacterClass : EntityComponent {
    [SerializeField] private string _className;
    [SerializeField] private int _baseAttackPower;
    [SerializeField] private int _attackPowerPerLevel;
    [SerializeField] private int _baseSpeed; //movement speed
    [SerializeField] private int _speedPerLevel;
    [SerializeField] private int _hpPerLevel;
    [SerializeField] private int _baseHP;
    [SerializeField] private int _baseAttackSpeed; //The lower the amount the faster the attack rate
    [SerializeField] private float _attackRange;
    [SerializeField] private string _skillName;

    [SerializeField] private string[] _traitNames;
    [SerializeField] private string _identifier;
    [SerializeField] private bool _isNonCombatant;
    //[SerializeField] private CHARACTER_ROLE _roleType;
    [SerializeField] private JOB _jobType;
    [SerializeField] private COMBAT_POSITION _combatPosition;
    [SerializeField] private COMBAT_TARGET _combatTarget;
    [SerializeField] private ATTACK_TYPE _attackType;
    [SerializeField] private RANGE_TYPE _rangeType;
    [SerializeField] private COMBAT_OCCUPIED_TILE _occupiedTileType;
    [SerializeField] private CurrenyCost _recruitmentCost;

    //private int _dodgeRate;
    //private int _parryRate;
    //private int _blockRate;
    private Skill _skill;

    #region getters/setters
    public string className {
        get { return _className; }
        //set { _className = value; }
    }
    public string identifier {
        get { return _identifier; }
    }
    public bool isNonCombatant {
        get { return _isNonCombatant; }
    }
    public int baseAttackPower {
        get { return _baseAttackPower; }
    }
    public int attackPowerPerLevel {
        get { return _attackPowerPerLevel; }
    }
    public int baseSpeed {
        get { return _baseSpeed; }
    }
    public int speedPerLevel {
        get { return _speedPerLevel; }
    }
    public int baseHP {
        get { return _baseHP; }
    }
    public int hpPerLevel {
        get { return _hpPerLevel; }
    }
    public float attackRange {
        get { return _attackRange; }
    }
    public int baseAttackSpeed {
        get { return _baseAttackSpeed; }
    }
    //public CHARACTER_ROLE roleType {
    //    get { return _roleType; }
    //}
    public JOB jobType {
        get { return _jobType; }
    }
    public COMBAT_POSITION combatPosition {
        get { return _combatPosition; }
    }
    public COMBAT_TARGET combatTarget {
        get { return _combatTarget; }
    }
    public ATTACK_TYPE attackType {
        get { return _attackType; }
    }
    public RANGE_TYPE rangeType {
        get { return _rangeType; }
    }
    public COMBAT_OCCUPIED_TILE occupiedTileType {
        get { return _occupiedTileType; }
    }
    public string skillName {
        get { return _skillName; }
    }
    public CurrenyCost recruitmentCost {
        get { return _recruitmentCost; }
    }
    public Skill skill {
        get { return _skill; }
    }
    public string[] traitNames {
        get { return _traitNames; }
    }
    #endregion

    public CharacterClass CreateNewCopy() {
        CharacterClass newClass = new CharacterClass();
        newClass._className = this._className;
        newClass._identifier = this._identifier;
        newClass._isNonCombatant = this._isNonCombatant;
        newClass._baseAttackPower = this._baseAttackPower;
        newClass._baseSpeed = this._baseSpeed;
        newClass._baseHP = this._baseHP;
        newClass._attackPowerPerLevel = this._attackPowerPerLevel;
		newClass._speedPerLevel = this._speedPerLevel;
        newClass._hpPerLevel = this._hpPerLevel;
        //newClass._workActionType = this._workActionType;
        newClass._combatPosition = this._combatPosition;
        newClass._combatTarget = this._combatTarget;
        newClass._attackType = this._attackType;
        newClass._rangeType = this._rangeType;
        newClass._occupiedTileType = this._occupiedTileType;
        //newClass._roleType = this._roleType;
        newClass._skillName = this._skillName;
        newClass._skill = this._skill; //.CreateNewCopy()
        newClass._traitNames = this._traitNames;
        newClass._jobType = this._jobType;
        newClass._recruitmentCost = this._recruitmentCost;
        //Array.Copy(this._traitNames, newClass._traitNames, this._traitNames.Length);
        return newClass;
    }

    public void SetData(ClassComponent classComponent) {
        this._className = classComponent.className;
        //this._baseAttackPower = classComponent.baseAttackPower;
        //this._attackPowerPerLevel = classComponent.attackPowerPerLevel;
        //this._baseSpeed = classComponent.baseSpeed;
        //this._speedPerLevel = classComponent.speedPerLevel;
        //this._baseHP = classComponent.baseHP;
        //this._hpPerLevel = (float)classComponent.hpPerLevel;
        //this._workActionType = classComponent.workActionType;
        //this._dodgeRate = classComponent.dodgeRate;
        //this._parryRate = classComponent.parryRate;
        //this._blockRate = classComponent.blockRate;
        this._skillName = classComponent.skill.name;
    }

    public void SetDataFromClassPanelUI() {
        this._className = ClassPanelUI.Instance.classNameInput.text;
        this._identifier = ClassPanelUI.Instance.identifierInput.text;
        this._isNonCombatant = ClassPanelUI.Instance.nonCombatantToggle.isOn;
        this._baseAttackPower = int.Parse(ClassPanelUI.Instance.baseAttackPowerInput.text);
        this._attackPowerPerLevel = int.Parse(ClassPanelUI.Instance.attackPowerPerLevelInput.text);
        this._baseSpeed = int.Parse(ClassPanelUI.Instance.baseSpeedInput.text);
        this._speedPerLevel = int.Parse(ClassPanelUI.Instance.speedPerLevelInput.text);
        this._baseHP = int.Parse(ClassPanelUI.Instance.baseHPInput.text);
        this._hpPerLevel = int.Parse(ClassPanelUI.Instance.hpPerLevelInput.text);
        this._baseAttackSpeed = int.Parse(ClassPanelUI.Instance.baseAttackSpeedInput.text);
        this._attackRange = float.Parse(ClassPanelUI.Instance.attackRangeInput.text);
        this._combatPosition = (COMBAT_POSITION) System.Enum.Parse(typeof(COMBAT_POSITION), ClassPanelUI.Instance.combatPositionOptions.options[ClassPanelUI.Instance.combatPositionOptions.value].text);
        this._combatTarget = (COMBAT_TARGET)System.Enum.Parse(typeof(COMBAT_TARGET), ClassPanelUI.Instance.combatTargetOptions.options[ClassPanelUI.Instance.combatTargetOptions.value].text);
        this._attackType = (ATTACK_TYPE) System.Enum.Parse(typeof(ATTACK_TYPE), ClassPanelUI.Instance.attackTypeOptions.options[ClassPanelUI.Instance.attackTypeOptions.value].text);
        this._rangeType = (RANGE_TYPE) System.Enum.Parse(typeof(RANGE_TYPE), ClassPanelUI.Instance.rangeTypeOptions.options[ClassPanelUI.Instance.rangeTypeOptions.value].text);
        this._occupiedTileType = (COMBAT_OCCUPIED_TILE) System.Enum.Parse(typeof(COMBAT_OCCUPIED_TILE), ClassPanelUI.Instance.occupiedTileOptions.options[ClassPanelUI.Instance.occupiedTileOptions.value].text);
        //this._roleType = (CHARACTER_ROLE) System.Enum.Parse(typeof(CHARACTER_ROLE), ClassPanelUI.Instance.roleOptions.options[ClassPanelUI.Instance.roleOptions.value].text);
        this._skillName = ClassPanelUI.Instance.skillOptions.options[ClassPanelUI.Instance.skillOptions.value].text;
        this._traitNames = ClassPanelUI.Instance.traitNames.ToArray();
        this._jobType = (JOB) System.Enum.Parse(typeof(JOB), ClassPanelUI.Instance.jobTypeOptions.options[ClassPanelUI.Instance.jobTypeOptions.value].text);
        this._recruitmentCost  = new CurrenyCost {
            amount = int.Parse(ClassPanelUI.Instance.recruitmentCostInput.text),
            currency = (CURRENCY) System.Enum.Parse(typeof(CURRENCY), ClassPanelUI.Instance.recruitmentCostOptions.options[ClassPanelUI.Instance.recruitmentCostOptions.value].text),
        };
    }

    public void ConstructData() {
        ConstructSkills();
    }
    private void ConstructSkills() {
        _skill = SkillManager.Instance.allSkills[_skillName];
    }
}