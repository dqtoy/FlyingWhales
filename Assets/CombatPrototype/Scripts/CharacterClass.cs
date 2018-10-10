using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public class CharacterClass : EntityComponent {
        [SerializeField] private string _className;
        [SerializeField] private int _baseHP;
        [SerializeField] private int _hpPerLevel;
        [SerializeField] private int _baseSP;
        [SerializeField] private int _spPerLevel;
        [SerializeField] private float _baseAttackPower;
        [SerializeField] private float _attackPowerPerLevel;
        [SerializeField] private float _baseSpeed;
        [SerializeField] private float _speedPerLevel;

        [SerializeField] private List<WEAPON_TYPE> _allowedWeaponTypes;
        [SerializeField] private List<RESOURCE> _harvestResources;
        [SerializeField] private List<StringListWrapper> _skillsPerLevelNames;
        [SerializeField] private ACTION_TYPE _workActionType;

        //private int _dodgeRate;
        //private int _parryRate;
        //private int _blockRate;
        private List<Skill[]> _skillsPerLevel;

        #region getters/setters
        public string className {
            get { return _className; }
            //set { _className = value; }
        }
        public float baseAttackPower {
            get { return _baseAttackPower; }
        }
        public float attackPowerPerLevel {
            get { return _attackPowerPerLevel; }
        }
        public float baseSpeed {
            get { return _baseSpeed; }
        }
        public float speedPerLevel {
            get { return _speedPerLevel; }
        }
        public int baseHP {
            get { return _baseHP; }
            //set { _hpModifier = value; }
        }
        public int hpPerLevel {
            get { return _hpPerLevel; }
        }
        public int baseSP {
            get { return _baseSP; }
            //set { _spModifier = value; }
        }
        public int spPerLevel {
            get { return _spPerLevel; }
        }
        public ACTION_TYPE workActionType {
            get { return _workActionType; }
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
        public List<WEAPON_TYPE> allowedWeaponTypes {
            get { return _allowedWeaponTypes; }
            //set { _allowedWeaponTypes = value; }
        }
        public List<Skill[]> skillsPerLevel {
            get { return _skillsPerLevel; }
        }
        public List<StringListWrapper> skillsPerLevelNames {
            get { return _skillsPerLevelNames; }
            //set { _skillsPerLevelNames = value; }
        }
        public List<RESOURCE> harvestResources {
            get { return _harvestResources; }
        }
        #endregion

        public CharacterClass CreateNewCopy() {
            CharacterClass newClass = new CharacterClass();
            newClass._className = this._className;
			newClass._baseAttackPower = this._baseAttackPower;
			newClass._attackPowerPerLevel = this._attackPowerPerLevel;
			newClass._baseSpeed = this._baseSpeed;
			newClass._speedPerLevel = this._speedPerLevel;
            newClass._baseHP = this._baseHP;
            newClass._hpPerLevel = this._hpPerLevel;
            newClass._baseSP = this._baseSP;
            newClass._spPerLevel = this._spPerLevel;
            newClass._workActionType = this._workActionType;
            //newClass._dodgeRate = this._dodgeRate;
            //newClass._parryRate = this._parryRate;                        
            //newClass._blockRate = this._blockRate;
            newClass._allowedWeaponTypes = new List<WEAPON_TYPE>(this._allowedWeaponTypes);
            newClass._harvestResources = new List<RESOURCE>(this._harvestResources);
            newClass._skillsPerLevel = new List<Skill[]>();
            for (int i = 0; i < this._skillsPerLevel.Count; i++) {
                Skill[] skillsArray = new Skill[this._skillsPerLevel[i].Length];
                for (int j = 0; j < this._skillsPerLevel[i].Length; j++) {
                    skillsArray[j] = this._skillsPerLevel[i][j].CreateNewCopy();
                }
                newClass._skillsPerLevel.Add(skillsArray);
            }
            return newClass;
        }

        public void SetData(ClassComponent classComponent) {
            this._className = classComponent.className;
            this._baseAttackPower = classComponent.baseAttackPower;
            this._attackPowerPerLevel = classComponent.attackPowerPerLevel;
            this._baseSpeed = classComponent.baseSpeed;
            this._speedPerLevel = classComponent.speedPerLevel;
            this._baseHP = classComponent.baseHP;
            this._hpPerLevel = classComponent.hpPerLevel;
            this._baseSP = classComponent.baseSP;
            this._spPerLevel = classComponent.spPerLevel;
            this._workActionType = classComponent.workActionType;
            //this._dodgeRate = classComponent.dodgeRate;
            //this._parryRate = classComponent.parryRate;
            //this._blockRate = classComponent.blockRate;
            this._allowedWeaponTypes = classComponent.allowedWeaponTypes;
            this._harvestResources = classComponent.harvestResources;
            this._skillsPerLevelNames = new List<StringListWrapper>();
            for (int i = 0; i < classComponent.skillsPerLevel.Count; i++) {
                StringListWrapper skillNames = new StringListWrapper();
                skillNames.list = new List<string>();
                for (int j = 0; j < classComponent.skillsPerLevel[i].list.Count; j++) {
                    skillNames.list.Add(classComponent.skillsPerLevel[i].list[j].name);
                }
                _skillsPerLevelNames.Add(skillNames);
            }
        }

        public void SetDataFromClassPanelUI() {
            this._className = ClassPanelUI.Instance.classNameInput.text;
            this._baseAttackPower = int.Parse(ClassPanelUI.Instance.baseAttackPowerInput.text);
            this._attackPowerPerLevel = int.Parse(ClassPanelUI.Instance.attackPowerPerLevelInput.text);
            this._baseSpeed = int.Parse(ClassPanelUI.Instance.baseSpeedInput.text);
            this._speedPerLevel = int.Parse(ClassPanelUI.Instance.speedPerLevelInput.text);
            this._baseHP = int.Parse(ClassPanelUI.Instance.baseHPInput.text);
            this._hpPerLevel = int.Parse(ClassPanelUI.Instance.hpPerLevelInput.text);
            this._baseSP = int.Parse(ClassPanelUI.Instance.baseSPInput.text);
            this._spPerLevel = int.Parse(ClassPanelUI.Instance.spPerLevelInput.text);
            this._workActionType = (ACTION_TYPE)System.Enum.Parse(typeof(ACTION_TYPE), ClassPanelUI.Instance.workActionOptions.options[ClassPanelUI.Instance.workActionOptions.value].text);

            this._allowedWeaponTypes = new List<WEAPON_TYPE>();
            for (int i = 0; i < ClassPanelUI.Instance.allowedWeaponTypes.Count; i++) {
                this._allowedWeaponTypes.Add((WEAPON_TYPE) System.Enum.Parse(typeof(WEAPON_TYPE), ClassPanelUI.Instance.allowedWeaponTypes[i]));
            }

            this._skillsPerLevelNames = new List<StringListWrapper>();
            foreach (Transform child in ClassPanelUI.Instance.skillsContentTransform) {
                LevelCollapseUI collapseUI = child.GetComponent<LevelCollapseUI>();
                StringListWrapper skillNames = new StringListWrapper();
                skillNames.list = collapseUI.skills;
                this._skillsPerLevelNames.Add(skillNames);
            }
        }

        public void ConstructData() {
            ConstructSkills();
        }
        private void ConstructSkills() {
            _skillsPerLevel = new List<Skill[]>();
            for (int i = 0; i < _skillsPerLevelNames.Count; i++) {
                Skill[] skillArray = new Skill[_skillsPerLevelNames[i].list.Count];
                for (int j = 0; j < _skillsPerLevelNames[i].list.Count; j++) {
                    skillArray[j] = SkillManager.Instance.allSkills[_skillsPerLevelNames[i].list[j]];
                }
                _skillsPerLevel.Add(skillArray);
            }
        }
    }
}

