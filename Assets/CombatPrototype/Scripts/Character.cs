using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ECS{
	[System.Serializable]
	public class Character {
		[SerializeField] private string _name;

        [SerializeField] private int _level;
        [SerializeField] private int _maxHP;
        [SerializeField] private int _currentHP;
        [SerializeField] private int _currentRow;

        [SerializeField] private int _strength;
        [SerializeField] private int _intelligence;
        [SerializeField] private int _agility;

        [SerializeField] private int _exp;

        [SerializeField] private int _strGain;
        [SerializeField] private int _intGain;
        [SerializeField] private int _agiGain;
        [SerializeField] private int _hpGain;

        [SerializeField] private bool _isDead;
		private List<Trait>	_traits;
        [SerializeField] private List<BodyPart> _bodyParts;
		private CharacterClass _characterClass;
        protected RaceSetting _raceSetting;

		#region getters / setters
		internal string name{
			get { return this._name; }
		}
		internal int level{
			get { return this._level; }
		}
		internal int currentHP{
			get { return this._currentHP; }
		}
        internal int maxHP {
            get { return this._maxHP; }
        }
        internal CharacterClass characterClass{
			get { return this._characterClass; }
		}
        internal RaceSetting raceSetting {
            get { return _raceSetting; }
        }
		internal List<BodyPart> bodyParts{
			get { return this._bodyParts; }
		}
		internal List<Trait> traits{
			get { return this._traits; }
		}
		internal int currentRow{
			get { return this._currentRow; }
		}
		internal bool isDead{
			get { return this._isDead; }
		}
        internal int strength {
            get { return _strength; }
        }
        internal int intelligence {
            get { return _intelligence; }
        }
        internal int agility {
            get { return _agility; }
        }
        internal int exp {
            get { return _exp; }
        }
        internal int strGain {
            get { return _strGain; }
        }
        internal int intGain {
            get { return _intGain; }
        }
        internal int agiGain {
            get { return _agiGain; }
        }
        internal int hpGain {
            get { return _hpGain; }
        }
        #endregion
        public Character(CharacterSetup baseSetup) {
            GENDER gender = GENDER.MALE;
            if(Random.Range(0, 2) == 0) {
                gender = GENDER.FEMALE;
            }
            if(baseSetup.raceSetting.race == RACE.HUMANS) {
                _name = RandomNameGenerator.Instance.GenerateWholeHumanName(gender);
            } else {
                _name = RandomNameGenerator.Instance.GenerateElvenName(gender);
            }
            _level = 1;
            _maxHP = baseSetup.raceSetting.baseHP;
            _currentHP = _maxHP;
            _strength = baseSetup.raceSetting.baseStr;
            _intelligence = baseSetup.raceSetting.baseInt;
            _agility = baseSetup.raceSetting.baseAgi;
            _exp = 0;
            _strGain = baseSetup.raceSetting.strGain + baseSetup.characterClass.strGain;
            _intGain = baseSetup.raceSetting.intGain + baseSetup.characterClass.intGain;
            _agiGain = baseSetup.raceSetting.agiGain + baseSetup.characterClass.agiGain;
            _hpGain = baseSetup.raceSetting.hpGain + baseSetup.characterClass.hpGain;
            _bodyParts = new List<BodyPart>(baseSetup.raceSetting.bodyParts);
            _characterClass = baseSetup.characterClass;
            _raceSetting = baseSetup.raceSetting;
            //TODO: Generate Traits
        }

		//Check if the body parts of this character has the attribute necessary and quantity
		internal bool HasAttribute(IBodyPart.ATTRIBUTE attribute, int quantity){
			int count = 0;
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];

				if(bodyPart.status.Count <= 0){
					if(bodyPart.attributes.Contains(attribute)){
						count += 1;
						if(count >= quantity){
							return true;
						}
					}
				}

				for (int j = 0; j < bodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = bodyPart.secondaryBodyParts [j];
					if (secondaryBodyPart.status.Count <= 0) {
						if (secondaryBodyPart.attributes.Contains (attribute)) {
							count += 1;
							if (count >= quantity) {
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		//Enables or Disables skills based on skill requirements
		internal void EnableDisableSkills(){
			for (int i = 0; i < this._characterClass.skills.Count; i++) {
				Skill skill = this._characterClass.skills [i];
				skill.isEnabled = true;
				for (int j = 0; j < skill.skillRequirements.Length; j++) {
					SkillRequirement skillRequirement = skill.skillRequirements [j];
					if(!HasAttribute(skillRequirement.attributeRequired, skillRequirement.itemQuantity)){
						skill.isEnabled = false;
						break;
					}
				}

				if(skill is MoveSkill){
					if (this._currentRow == 1 && skill.skillName == "MoveLeft"){
						skill.isEnabled = false;
					} else if (this._currentRow == 5 && skill.skillName == "MoveRight"){
						skill.isEnabled = false;
					}
				}
			}
		}

		//Changes row number of this character
		internal void SetRowNumber(int rowNumber){
			this._currentRow = rowNumber;
		}

		//Adjust current HP based on specified paramater, but HP must not go below 0
		internal void AdjustHP(int amount){
			this._currentHP += amount;
			if(this._currentHP < 0){
				this._currentHP = 0;
				Death ();
			}
		}

		//Character's death
		internal void Death(){
			this._isDead = true;
			if(Messenger.eventTable.ContainsKey("CharacterDeath")){
				Messenger.Broadcast ("CharacterDeath", this);
			}
		}

        #region Body Parts
        /*
         * Add new body parts here.
         * */
        internal void AddBodyPart(BodyPart bodyPart) {
            bodyParts.Add(bodyPart);
        }
        #endregion

		internal void CureStatusEffects(){
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];
				if(bodyPart.status.Count > 0){
					for (int j = 0; j < bodyPart.status.Count; j++) {
						int chance = UnityEngine.Random.Range (0, 100);
						if(chance < 15){
							bodyPart.RemoveStatusEffectOnSecondaryBodyParts (bodyPart.status [j]);
							bodyPart.status.RemoveAt (j);
							j--;
						}
					}
				}
			}
		}
    }
}

