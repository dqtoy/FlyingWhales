using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ECS{
	[System.Serializable]
	public class Character {
		[SerializeField] private string _name;

        private int _actRate;

        [SerializeField] private int _level;
        [SerializeField] private int _maxHP;
        [SerializeField] private int _currentHP;
        [SerializeField] private int _currentRow;

        [SerializeField] private int _strength;
        [SerializeField] private int _intelligence;
        [SerializeField] private int _agility;

        [SerializeField] private int _exp;

        //[SerializeField] private int _strGain;
        //[SerializeField] private int _intGain;
        //[SerializeField] private int _agiGain;
        //[SerializeField] private int _hpGain;

        [SerializeField] private bool _isDead;
		private List<Trait>	_traits;
        [SerializeField] private List<BodyPart> _bodyParts;
		[SerializeField] private List<Item> _items;

		private CharacterClass _characterClass;
		protected RaceComponent _raceSetting;

		#region getters / setters
		internal string name{
			get { return this._name; }
		}
        internal int actRate {
            get { return _actRate; }
            set { _actRate = value; }
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
		internal RaceComponent raceSetting {
            get { return _raceSetting; }
        }
		internal List<BodyPart> bodyParts{
			get { return this._bodyParts; }
		}
		internal List<Item> items{
			get { return this._items; }
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
            get { return _characterClass.strGain + _raceSetting.strGain; }
        }
        internal int intGain {
            get { return _characterClass.intGain + _raceSetting.intGain; }
        }
        internal int agiGain {
            get { return _characterClass.agiGain + _raceSetting.agiGain; }
        }
        internal int hpGain {
            get { return _characterClass.hpGain + _raceSetting.hpGain; }
        }
        #endregion
        public Character(CharacterSetup baseSetup, int level = 1) {
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
            //_strGain = baseSetup.raceSetting.strGain + baseSetup.characterClass.strGain;
            //_intGain = baseSetup.raceSetting.intGain + baseSetup.characterClass.intGain;
            //_agiGain = baseSetup.raceSetting.agiGain + baseSetup.characterClass.agiGain;
            //_hpGain = baseSetup.raceSetting.hpGain + baseSetup.characterClass.hpGain;
            _bodyParts = new List<BodyPart>(baseSetup.raceSetting.bodyParts);
            _actRate = baseSetup.characterClass.actRate;
            _characterClass = baseSetup.characterClass;
            _raceSetting = baseSetup.raceSetting;
			_items = new List<Item> ();

            if (level > 1) {
                LevelUp(level - 1);
            }
            //TODO: Generate Traits
        }

		//Check if the body parts of this character has the attribute necessary and quantity
		internal bool HasAttribute(IBodyPart.ATTRIBUTE attribute, int quantity){
			int count = 0;
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];

				if(bodyPart.statusEffects.Count <= 0){
					if(bodyPart.HasAttribute(attribute)){
						count += 1;
						if(count >= quantity){
							return true;
						}
					}
				}

				for (int j = 0; j < bodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = bodyPart.secondaryBodyParts [j];
					if (secondaryBodyPart.statusEffects.Count <= 0) {
						if (secondaryBodyPart.HasAttribute(attribute)) {
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
			bool isAllAttacksInRange = true;
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
				if(skill is AttackSkill){
					if(isAllAttacksInRange){
						isAllAttacksInRange = CombatPrototype.Instance.HasTargetInRangeForSkill (skill, this);
					}
				}else if (skill is FleeSkill){
					if(this.currentHP >= (this.maxHP / 2)){
						skill.isEnabled = false;
					}
				}
			}
			for (int i = 0; i < this._characterClass.skills.Count; i++) {
				Skill skill = this._characterClass.skills [i];
				if(skill is MoveSkill){
					skill.isEnabled = true;
					if(isAllAttacksInRange){
						skill.isEnabled = false;
						continue;
					}
					if(skill.skillName == "MoveLeft"){
						if (this._currentRow == 1) {
							skill.isEnabled = false;
						} else {
							bool hasEnemyOnLeft = false;
							if(CombatPrototype.Instance.charactersSideA.Contains(this)){
								for (int j = 0; j < CombatPrototype.Instance.charactersSideB.Count; j++) {
									Character enemy = CombatPrototype.Instance.charactersSideB [j];
									if(enemy.currentRow < this._currentRow){
										hasEnemyOnLeft = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < CombatPrototype.Instance.charactersSideA.Count; j++) {
									Character enemy = CombatPrototype.Instance.charactersSideA [j];
									if(enemy.currentRow < this._currentRow){
										hasEnemyOnLeft = true;
										break;
									}
								}
							}
							if(!hasEnemyOnLeft){
								skill.isEnabled = false;
							}
						}
					}else if(skill.skillName == "MoveRight"){
						if (this._currentRow == 5) {
							skill.isEnabled = false;
						} else {
							bool hasEnemyOnRight = false;
							if(CombatPrototype.Instance.charactersSideA.Contains(this)){
								for (int j = 0; j < CombatPrototype.Instance.charactersSideB.Count; j++) {
									Character enemy = CombatPrototype.Instance.charactersSideB [j];
									if(enemy.currentRow > this._currentRow){
										hasEnemyOnRight = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < CombatPrototype.Instance.charactersSideA.Count; j++) {
									Character enemy = CombatPrototype.Instance.charactersSideA [j];
									if(enemy.currentRow > this._currentRow){
										hasEnemyOnRight = true;
										break;
									}
								}
							}
							if(!hasEnemyOnRight){
								skill.isEnabled = false;
							}
						}
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
            this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
			if(this._currentHP == 0){
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
        internal IBodyPart GetBodyPartForWeapon(Weapon weapon) {
            List<IBodyPart> allBodyParts = new List<IBodyPart>();
            for (int i = 0; i < bodyParts.Count; i++) {
                BodyPart currBodyPart = bodyParts[i];
                allBodyParts.Add(currBodyPart);
                for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
                    allBodyParts.Add(currBodyPart.secondaryBodyParts[j]);
                }
            }
            for (int i = 0; i < allBodyParts.Count; i++) {
                IBodyPart currBodyPart = allBodyParts[i];
                bool meetsRequirements = true;
                //check if currBodyPart meets the weapons requirements
                for (int j = 0; j < weapon.equipRequirements.Count; j++) {
                    IBodyPart.ATTRIBUTE currReq = weapon.equipRequirements[j];
                    if (!currBodyPart.HasUnusedAttribute(currReq)) {
                        meetsRequirements = false;
                        break;
                    }
                }
                if (meetsRequirements) {
                    return currBodyPart;
                }
            }
            return null;
        }
        internal IBodyPart GetBodyPartForArmor(Armor armor) {
            List<IBodyPart> allBodyParts = new List<IBodyPart>();
            for (int i = 0; i < bodyParts.Count; i++) {
                BodyPart currBodyPart = bodyParts[i];
                allBodyParts.Add(currBodyPart);
                for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
                    allBodyParts.Add(currBodyPart.secondaryBodyParts[j]);
                }
            }

            IBodyPart.ATTRIBUTE neededAttribute = Utilities.GetNeededAttributeForArmor(armor);
            for (int i = 0; i < allBodyParts.Count; i++) {
                IBodyPart currBodyPart = allBodyParts[i];
                //check if currBodyPart can equip the armor
                if (currBodyPart.HasUnusedAttribute(neededAttribute)) {
                    return currBodyPart;
                }
            }
            return null;
        }
        #endregion

        #region Items
        //Equip a weapon to a body part of this character and add it to the list of items this character have
        internal void EquipWeapon(Weapon weapon, IBodyPart bodyPart){
			bodyPart.AttachItem(weapon);
			weapon.bodyPartAttached = bodyPart;
			AddItem(weapon);
            Debug.Log(this.name + " equipped " + weapon.itemName + " to " + bodyPart.bodyPart.ToString());
            CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
        }
		//Equip an armor to a body part of this character and add it to the list of items this character have
		internal void EquipArmor(Armor armor, IBodyPart bodyPart){
			bodyPart.AttachItem(armor);
			armor.bodyPartAttached = bodyPart;
			AddItem(armor);
            Debug.Log(this.name + " equipped " + armor.itemName + " to " + bodyPart.bodyPart.ToString());
            CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
        }

		internal void AddItem(Item newItem){
			this._items.Add (newItem);
		}
		internal void RemoveItem(Item newItem){
			this._items.Remove (newItem);
		}
		#endregion

		internal void CureStatusEffects(){
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];
				if(bodyPart.statusEffects.Count > 0){
					for (int j = 0; j < bodyPart.statusEffects.Count; j++) {
						STATUS_EFFECT statusEffect = bodyPart.statusEffects [j];
						if(statusEffect != STATUS_EFFECT.DECAPITATED){
							int chance = UnityEngine.Random.Range (0, 100);
							if(chance < 15){
								CombatPrototypeUI.Instance.AddCombatLog(this._name + "'s " + bodyPart.bodyPart.ToString ().ToLower () + " is cured from " + statusEffect.ToString ().ToLower () + ".");
								bodyPart.RemoveStatusEffectOnSecondaryBodyParts (statusEffect);
								bodyPart.statusEffects.RemoveAt (j);
								j--;
							}
						}
					}
				}
			}
		}

        #region Levels
        public void LevelUp(int levels) {
            for (int i = 0; i < levels; i++) {
                IncreaseLevel();
            }
        }
        private void IncreaseLevel() {
            _level += 1;
            _strength += strGain;
            _intelligence += intGain;
            _agility += agiGain;
            _maxHP += hpGain;
            _exp = 0;
        }
        #endregion
    }
}

