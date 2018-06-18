using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public class CharacterClass : EntityComponent {
        private string _className;
        private float _strWeightAllocation;
        private float _intWeightAllocation;
        private float _agiWeightAllocation;
        private float _vitWeightAllocation;
        //private int _dodgeRate;
        //private int _parryRate;
        //private int _blockRate;
        private List<WEAPON_TYPE> _allowedWeaponTypes;
        private List<Skill[]> _skillsPerLevel;

        #region getters/setters
        public string className {
            get { return _className; }
        }
        public float strWeightAllocation {
            get { return _strWeightAllocation; }
        }
        public float intWeightAllocation {
            get { return _intWeightAllocation; }
        }
        public float agiWeightAllocation {
            get { return _agiWeightAllocation; }
        }
        public float vitWeightAllocation {
            get { return _vitWeightAllocation; }
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
        }
        public List<Skill[]> skillsPerLevel {
            get { return _skillsPerLevel; }
        }
        #endregion

        public CharacterClass CreateNewCopy() {
            CharacterClass newClass = new CharacterClass();
            newClass._className = this._className;
			newClass._strWeightAllocation = this._strWeightAllocation;
			newClass._intWeightAllocation = this._intWeightAllocation;
			newClass._agiWeightAllocation = this._agiWeightAllocation;
			newClass._vitWeightAllocation = this._vitWeightAllocation;
            //newClass._dodgeRate = this._dodgeRate;
            //newClass._parryRate = this._parryRate;
            //newClass._blockRate = this._blockRate;
			newClass._allowedWeaponTypes = new List<WEAPON_TYPE>(this._allowedWeaponTypes);
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
            //this._dodgeRate = classComponent.dodgeRate;
            //this._parryRate = classComponent.parryRate;
            //this._blockRate = classComponent.blockRate;
            this._allowedWeaponTypes = new List<WEAPON_TYPE>(classComponent.allowedWeaponTypes);
            this._skillsPerLevel = new List<Skill[]>();
            for (int i = 0; i < classComponent.skillsPerLevelNames.Count; i++) {
                Skill[] skillsArray = new Skill[classComponent.skillsPerLevelNames[i].Length];
                for (int j = 0; j < classComponent.skillsPerLevelNames[i].Length; j++) {
                    skillsArray[j] = SkillManager.Instance.allSkills[classComponent.skillsPerLevelNames[i][j]];
                }
                _skillsPerLevel.Add(skillsArray);
            }
        }
    }
}

