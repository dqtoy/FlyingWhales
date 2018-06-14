using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS {
    public class CharacterClass : EntityComponent {
        private string _className;
        private float _strPercentage;
        private float _intPercentage;
        private float _agiPercentage;
        private float _hpPercentage;
        private int _dodgeRate;
        private int _parryRate;
        private int _blockRate;
        private List<WEAPON_TYPE> _allowedWeaponTypes;
        private List<Skill[]> _skillsPerLevel;

        #region getters/setters
        public string className {
            get { return _className; }
        }
        public float strPercentage {
            get { return _strPercentage; }
        }
        public float intPercentage {
            get { return _intPercentage; }
        }
        public float agiPercentage {
            get { return _agiPercentage; }
        }
        public float hpPercentage {
            get { return _hpPercentage; }
        }
        public int dodgeRate {
            get { return _dodgeRate; }
        }
        public int parryRate {
            get { return _parryRate; }
        }
        public int blockRate {
            get { return _blockRate; }
        }
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
			newClass._strPercentage = this._strPercentage;
			newClass._intPercentage = this._intPercentage;
			newClass._agiPercentage = this._agiPercentage;
			newClass._hpPercentage = this._hpPercentage;
            newClass._dodgeRate = this._dodgeRate;
            newClass._parryRate = this._parryRate;
            newClass._blockRate = this._blockRate;
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
            this._strPercentage = classComponent.strPercentage;
            this._intPercentage = classComponent.intPercentage;
            this._agiPercentage = classComponent.agiPercentage;
            this._hpPercentage = classComponent.hpPercentage;
            this._dodgeRate = classComponent.dodgeRate;
            this._parryRate = classComponent.parryRate;
            this._blockRate = classComponent.blockRate;
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

