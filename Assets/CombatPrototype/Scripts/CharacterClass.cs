using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public class CharacterClass : EntityComponent {
        [SerializeField] private string _className;
        [SerializeField] private float _strWeightAllocation;
        [SerializeField] private float _intWeightAllocation;
        [SerializeField] private float _agiWeightAllocation;
        [SerializeField] private float _vitWeightAllocation;
        [SerializeField] private float _baseHP;
        [SerializeField] private float _hpPerLevel;
        [SerializeField] private float _baseSP;
        [SerializeField] private float _spPerLevel;
        [SerializeField] private List<WEAPON_TYPE> _allowedWeaponTypes;
        [SerializeField] private List<RESOURCE> _harvestResources;
        [SerializeField] private List<StringListWrapper> _skillsPerLevelNames;
        [SerializeField] private ACTION_TYPE _workActionType;

        //private int _dodgeRate;
        //private int _parryRate;
        //private int _blockRate;
        private CharacterAction _workAction;
        private List<Skill[]> _skillsPerLevel;

        #region getters/setters
        public string className {
            get { return _className; }
            //set { _className = value; }
        }
        public float strWeightAllocation {
            get { return _strWeightAllocation; }
            //set { _strWeightAllocation = value; }
        }
        public float intWeightAllocation {
            get { return _intWeightAllocation; }
            //set { _intWeightAllocation = value; }
        }
        public float agiWeightAllocation {
            get { return _agiWeightAllocation; }
            //set { _agiWeightAllocation = value; }
        }
        public float vitWeightAllocation {
            get { return _vitWeightAllocation; }
            //set { _vitWeightAllocation = value; }
        }
        public float baseHP {
            get { return _baseHP; }
            //set { _hpModifier = value; }
        }
        public float hpPerLevel {
            get { return _hpPerLevel; }
        }
        public float baseSP {
            get { return _baseSP; }
            //set { _spModifier = value; }
        }
        public float spPerLevel {
            get { return _spPerLevel; }
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
			newClass._strWeightAllocation = this._strWeightAllocation;
			newClass._intWeightAllocation = this._intWeightAllocation;
			newClass._agiWeightAllocation = this._agiWeightAllocation;
			newClass._vitWeightAllocation = this._vitWeightAllocation;
            newClass._baseHP = this._baseHP;
            newClass._hpPerLevel = this._hpPerLevel;
            newClass._baseSP = this._baseSP;
            newClass._spPerLevel = this._spPerLevel;
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
            this._strWeightAllocation = classComponent.strWeightAllocation;
            this._intWeightAllocation = classComponent.intWeightAllocation;
            this._agiWeightAllocation = classComponent.agiWeightAllocation;
            this._vitWeightAllocation = classComponent.vitWeightAllocation;
            this._baseHP = classComponent.baseHP;
            this._hpPerLevel = classComponent.hpPerLevel;
            this._baseSP = classComponent.baseSP;
            this._spPerLevel = classComponent.spPerLevel;
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
            this._strWeightAllocation = int.Parse(ClassPanelUI.Instance.strWeightAllocInput.text);
            this._intWeightAllocation = int.Parse(ClassPanelUI.Instance.intWeightAllocInput.text);
            this._agiWeightAllocation = int.Parse(ClassPanelUI.Instance.agiWeightAllocInput.text);
            this._vitWeightAllocation = int.Parse(ClassPanelUI.Instance.vitWeightAllocInput.text);
            this._baseHP = int.Parse(ClassPanelUI.Instance.baseHPInput.text);
            this._hpPerLevel = int.Parse(ClassPanelUI.Instance.hpPerLevelInput.text);
            this._baseSP = int.Parse(ClassPanelUI.Instance.baseSPInput.text);
            this._spPerLevel = int.Parse(ClassPanelUI.Instance.spPerLevelInput.text);

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
            ConstructWorkActions();
            ConstructSchedule();
            ConstructSkills();
        }
        private void ConstructWorkActions() {
            _workAction = ObjectManager.Instance.CreateNewCharacterAction(_workActionType);
        }
        private void ConstructSchedule() {
            //TODO
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

