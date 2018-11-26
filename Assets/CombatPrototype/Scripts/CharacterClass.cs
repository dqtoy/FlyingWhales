using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ECS {
    public class CharacterClass : EntityComponent {
        [SerializeField] private string _className;
        [SerializeField] private int _baseSP;
        [SerializeField] private int _spPerLevel;
        [SerializeField] private int _armyCount;
        [SerializeField] private float _attackPowerPerLevel;
        [SerializeField] private float _speedPerLevel;
        [SerializeField] private float _hpPerLevel;
        [SerializeField] private string _skillName;

        [SerializeField] private List<RESOURCE> _harvestResources;
        [SerializeField] private List<string> _weaponTierNames;
        [SerializeField] private List<string> _armorTierNames;
        [SerializeField] private List<string> _accessoryTierNames;
        [SerializeField] private string[] _traitNames;
        [SerializeField] private ACTION_TYPE _workActionType;
        [SerializeField] private CHARACTER_ROLE _roleType;
        [SerializeField] private JOB _jobType;

        //private int _dodgeRate;
        //private int _parryRate;
        //private int _blockRate;
        private Skill _skill;

        #region getters/setters
        public string className {
            get { return _className; }
            //set { _className = value; }
        }
        //public float baseAttackPower {
        //    get { return _baseAttackPower; }
        //}
        public float attackPowerPerLevel {
            get { return _attackPowerPerLevel; }
        }
        //public float baseSpeed {
        //    get { return _baseSpeed; }
        //}
        public float speedPerLevel {
            get { return _speedPerLevel; }
        }
        //public int baseHP {
        //    get { return _baseHP; }
        //}
        public float hpPerLevel {
            get { return _hpPerLevel; }
        }
        public int baseSP {
            get { return _baseSP; }
            //set { _spModifier = value; }
        }
        public int armyCount {
            get { return _armyCount; }
        }
        public int spPerLevel {
            get { return _spPerLevel; }
        }
        public ACTION_TYPE workActionType {
            get { return _workActionType; }
        }
        public CHARACTER_ROLE roleType {
            get { return _roleType; }
        }
        public JOB jobType {
            get { return _jobType; }
        }
        public string skillName {
            get { return _skillName; }
        }
        //public int dodgeRate {
        //    get { return _dodgeRate; }
        //}
        //public int parryRate {
        //    get { return _parryRate; }
        //}
        //public int blockRate {
        //    get { return _blockRate; }
        //}
        //public List<WEAPON_TYPE> allowedWeaponTypes {
        //    get { return _allowedWeaponTypes; }
        //    //set { _allowedWeaponTypes = value; }
        //}
        public Skill skill {
            get { return _skill; }
        }
        //public List<StringListWrapper> skillsPerLevelNames {
        //    get { return _skillsPerLevelNames; }
        //    //set { _skillsPerLevelNames = value; }
        //}
        public List<string> weaponTierNames {
            get { return _weaponTierNames; }
        }
        public List<string> armorTierNames {
            get { return _armorTierNames; }
        }
        public List<string> accessoryTierNames {
            get { return _accessoryTierNames; }
        }
        public string[] traitNames {
            get { return _traitNames; }
        }
        public List<RESOURCE> harvestResources {
            get { return _harvestResources; }
        }
        #endregion

        public CharacterClass CreateNewCopy() {
            CharacterClass newClass = new CharacterClass();
            newClass._className = this._className;
			newClass._attackPowerPerLevel = this._attackPowerPerLevel;
			newClass._speedPerLevel = this._speedPerLevel;
            newClass._hpPerLevel = this._hpPerLevel;
            newClass._baseSP = this._baseSP;
            newClass._spPerLevel = this._spPerLevel;
            newClass._armyCount = this._armyCount;
            newClass._workActionType = this._workActionType;
            newClass._roleType = this._roleType;
            newClass._skillName = this._skillName;
            newClass._harvestResources = new List<RESOURCE>(this._harvestResources);
            newClass._skill = this._skill; //.CreateNewCopy()
            newClass._weaponTierNames = new List<string>(this._weaponTierNames);
            newClass._armorTierNames = new List<string>(this._armorTierNames);
            newClass._accessoryTierNames = new List<string>(this._accessoryTierNames);
            newClass._traitNames = this._traitNames;
            //Array.Copy(this._traitNames, newClass._traitNames, this._traitNames.Length);
            return newClass;
        }

        public void SetData(ClassComponent classComponent) {
            this._className = classComponent.className;
            //this._baseAttackPower = classComponent.baseAttackPower;
            this._attackPowerPerLevel = classComponent.attackPowerPerLevel;
            //this._baseSpeed = classComponent.baseSpeed;
            this._speedPerLevel = classComponent.speedPerLevel;
            //this._baseHP = classComponent.baseHP;
            this._hpPerLevel = (float)classComponent.hpPerLevel;
            this._baseSP = classComponent.baseSP;
            this._spPerLevel = classComponent.spPerLevel;
            this._workActionType = classComponent.workActionType;
            //this._dodgeRate = classComponent.dodgeRate;
            //this._parryRate = classComponent.parryRate;
            //this._blockRate = classComponent.blockRate;
            this._harvestResources = classComponent.harvestResources;
            this._skillName = classComponent.skill.name;

            this._weaponTierNames = new List<string>();
            for (int i = 0; i < classComponent.weaponTiers.Count; i++) {
                this._weaponTierNames.Add(classComponent.weaponTiers[i].name);
            }
            this._armorTierNames = new List<string>();
            for (int i = 0; i < classComponent.armorTiers.Count; i++) {
                this._armorTierNames.Add(classComponent.armorTiers[i].name);
            }
            this._accessoryTierNames = new List<string>();
            for (int i = 0; i < classComponent.accessoryTiers.Count; i++) {
                this._accessoryTierNames.Add(classComponent.accessoryTiers[i].name);
            }
        }

        public void SetDataFromClassPanelUI() {
            this._className = ClassPanelUI.Instance.classNameInput.text;
            //this._baseAttackPower = int.Parse(ClassPanelUI.Instance.baseAttackPowerInput.text);
            this._attackPowerPerLevel = float.Parse(ClassPanelUI.Instance.attackPowerPerLevelInput.text);
            //this._baseSpeed = int.Parse(ClassPanelUI.Instance.baseSpeedInput.text);
            this._speedPerLevel = float.Parse(ClassPanelUI.Instance.speedPerLevelInput.text);
            //this._baseHP = int.Parse(ClassPanelUI.Instance.baseHPInput.text);
            this._hpPerLevel = float.Parse(ClassPanelUI.Instance.hpPerLevelInput.text);
            this._baseSP = int.Parse(ClassPanelUI.Instance.baseSPInput.text);
            this._spPerLevel = int.Parse(ClassPanelUI.Instance.spPerLevelInput.text);
            this._armyCount = int.Parse(ClassPanelUI.Instance.armyCountInput.text);
            this._workActionType = (ACTION_TYPE)System.Enum.Parse(typeof(ACTION_TYPE), ClassPanelUI.Instance.workActionOptions.options[ClassPanelUI.Instance.workActionOptions.value].text);
            this._roleType = (CHARACTER_ROLE) System.Enum.Parse(typeof(CHARACTER_ROLE), ClassPanelUI.Instance.roleOptions.options[ClassPanelUI.Instance.roleOptions.value].text);
            this._skillName = ClassPanelUI.Instance.skillOptions.options[ClassPanelUI.Instance.skillOptions.value].text;
            this._weaponTierNames = ClassPanelUI.Instance.weaponTiers;
            this._armorTierNames = ClassPanelUI.Instance.armorTiers;
            this._accessoryTierNames = ClassPanelUI.Instance.accessoryTiers;
            this._traitNames = ClassPanelUI.Instance.traitNames.ToArray();
        }

        public void ConstructData() {
            ConstructSkills();
        }
        private void ConstructSkills() {
            _skill = SkillManager.Instance.allSkills[_skillName];
        }
    }
}

