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

        [SerializeField] private bool _isDead;
		private List<Trait>	_traits;
        [SerializeField] private List<BodyPart> _bodyParts;
		[SerializeField] private List<Item> _items;

		private CharacterClass _characterClass;
		protected RaceSetting _raceSetting;

		private Color _characterColor;
		private string _characterColorCode;

		#region getters / setters
		internal string name{
			get { return "[" + this._characterColorCode + "]" + this._name + "[-]"; }
		}
        internal int actRate {
            get { return _actRate + _items.Sum(x => x.bonusActRate); }
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
		internal RaceSetting raceSetting {
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
            get { return _strength +_items.Sum(x => x.bonusStrength); }
        }
        internal int intelligence {
            get { return _intelligence + _items.Sum(x => x.bonusIntelligence); }
        }
        internal int agility {
            get { return _agility + _items.Sum(x => x.bonusAgility); }
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
        internal int dodgeRate {
            get { return characterClass.dodgeRate + _items.Sum(x => x.bonusDodgeRate); }
        }
        internal int parryRate {
            get { return characterClass.parryRate + _items.Sum(x => x.bonusParryRate); }
        }
        internal int blockRate {
            get { return characterClass.blockRate + _items.Sum(x => x.bonusBlockRate); }
        }
		internal Color characterColor {
			get { return _characterColor; }
		}
		internal string characterColorCode {
			get { return _characterColorCode; }
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
            _characterClass = baseSetup.characterClass.CreateNewCopy();
            _raceSetting = baseSetup.raceSetting.CreateNewCopy();
			_level = level;
            _maxHP = _raceSetting.baseHP;
            _currentHP = _maxHP;
            _strength = _raceSetting.baseStr;
            _intelligence = _raceSetting.baseInt;
            _agility = _raceSetting.baseAgi;
            _exp = 0;
            _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
            _actRate = _characterClass.actRate;

			_items = new List<Item> ();

			EquipPreEquippedItems (baseSetup);
			GetRandomCharacterColor ();

            if (baseSetup.initialLevel > 1) {
				LevelUp(baseSetup.initialLevel - 1);
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
				if(this._level < skill.levelRequirement){
					skill.isEnabled = false;
					continue;
				}
				for (int j = 0; j < skill.skillRequirements.Length; j++) {
					SkillRequirement skillRequirement = skill.skillRequirements [j];
                    if(skillRequirement.equipmentType == EQUIPMENT_TYPE.NONE) {
                        if (!HasAttribute(skillRequirement.attributeRequired, skillRequirement.itemQuantity)) {
                            skill.isEnabled = false;
                            break;
                        }
                    } else {
                        //check if the character has the equipment needed to perform the skill
                        if(!HasEquipmentOfType(skillRequirement.equipmentType, skillRequirement.attributeRequired)) {
                            skill.isEnabled = false;
                            break;
                        }
                    }
				}
				if(!skill.isEnabled){
					continue;
				}
				if(skill is AttackSkill){
					if(isAllAttacksInRange){
						isAllAttacksInRange = CombatPrototype.Instance.HasTargetInRangeForSkill (skill, this);
					}
				}else if (skill is FleeSkill){
					if(this.currentHP >= (this.maxHP / 2)){
						skill.isEnabled = false;
						continue;
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
							continue;
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
								continue;
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
								continue;
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
			CombatPrototypeManager.Instance.ReturnCharacterColorToPool (_characterColor);
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
            IBodyPart.ATTRIBUTE neededAttribute = Utilities.GetNeededAttributeForArmor(armor);
			for (int i = 0; i < bodyParts.Count; i++) {
				BodyPart currBodyPart = bodyParts[i];
				//check if currBodyPart can equip the armor
				if (currBodyPart.HasUnusedAttribute(neededAttribute)) {
					return currBodyPart;
				}
				for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
					//check if currBodyPart can equip the armor
					if (currBodyPart.secondaryBodyParts[j].HasUnusedAttribute(neededAttribute)) {
						return currBodyPart.secondaryBodyParts[j];
					}
				}
			}
            return null;
        }
        #endregion

        #region Items
		//If character set up has pre equipped items, equip it here evey time a character is made
		internal void EquipPreEquippedItems(CharacterSetup charSetup){
			if(charSetup.preEquippedItems.Count > 0){
				for (int i = 0; i < charSetup.preEquippedItems.Count; i++) {
					EquipItem (charSetup.preEquippedItems [i].itemType, charSetup.preEquippedItems [i].itemName);
				}
			}

		}
		//Equip generic item, can be a weapon, armor, etc.
		public void EquipItem(ITEM_TYPE itemType, string itemName) {
			string strItemType = itemType.ToString();
			string path = "Assets/CombatPrototype/Data/Items/" + strItemType + "/" + itemName + ".json";
			string dataAsJson = System.IO.File.ReadAllText(path);
			if (strItemType.Contains("WEAPON")) {
				Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
				TryEquipWeapon(weapon);
			} else if (strItemType.Contains("ARMOR")) {
				Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
				IBodyPart bodyPartToEquip = GetBodyPartForArmor(armor);
				if (bodyPartToEquip != null) {
					EquipArmor(armor, bodyPartToEquip);
				} else {
					Debug.Log(name + " cannot equip " + armor.itemName);
				}
			}
		}
		public void EquipItem(string itemType, string itemName) {
			string path = "Assets/CombatPrototype/Data/Items/" + itemType + "/" + itemName + ".json";
			string dataAsJson = System.IO.File.ReadAllText(path);
			if (itemType.Contains("WEAPON")) {
				Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
				TryEquipWeapon(weapon);
			} else if (itemType.Contains("ARMOR")) {
				Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
				IBodyPart bodyPartToEquip = GetBodyPartForArmor(armor);
				if (bodyPartToEquip != null) {
					EquipArmor(armor, bodyPartToEquip);
				} else {
					Debug.Log(name + " cannot equip " + armor.itemName);
				}
			}
		}

        //Try to equip a weapon to a body part of this character and add it to the list of items this character have
		internal bool TryEquipWeapon(Weapon weapon){
			for (int j = 0; j < weapon.equipRequirements.Count; j++) {
				IBodyPart.ATTRIBUTE currReq = weapon.equipRequirements[j];
				if(!AttachWeaponToBodyPart(weapon, currReq)){
					DetachWeaponFromBodyParts (weapon);
					return false;
				}
			}
			AddItem(weapon);
            weapon.ResetDurability();
            weapon.SetOwner(this);
//          Debug.Log(this.name + " equipped " + weapon.itemName + " to " + bodyPart.bodyPart.ToString());
            CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
			return true;
        }
		private bool AttachWeaponToBodyPart(Weapon weapon, IBodyPart.ATTRIBUTE req){
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart currBodyPart = this._bodyParts[i];
				if(currBodyPart.AttachItem(weapon, req)){
					return true;
				}
				for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
					if(currBodyPart.secondaryBodyParts[j].AttachItem(weapon, req)){
						return true;
					}
				}
			}
			return false;
		}
		private void DetachWeaponFromBodyParts(Weapon weapon){
			for (int i = 0; i < weapon.equipRequirements.Count; i++) {
				IBodyPart.ATTRIBUTE req = weapon.equipRequirements [i];
				for (int j = 0; j < weapon.bodyPartsAttached.Count; j++) {
					if(weapon.bodyPartsAttached[j].DettachItem(weapon, req)){
						break;
					}
				}
			}
		}
        internal void UnequipWeapon(Weapon weapon) {
            RemoveItem(weapon);
			DetachWeaponFromBodyParts (weapon);
        }
		//Equip an armor to a body part of this character and add it to the list of items this character have
		internal void EquipArmor(Armor armor, IBodyPart bodyPart){
			bodyPart.AttachItem(armor, Utilities.GetNeededAttributeForArmor(armor));
//			armor.bodyPartAttached = bodyPart;
			AddItem(armor);
            armor.ResetDurability();
            armor.ResetHitPoints();
            armor.SetOwner(this);
            Debug.Log(this.name + " equipped " + armor.itemName + " to " + bodyPart.bodyPart.ToString());
            CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
        }
        internal void UnequipArmor(Armor armor) {
            RemoveItem(armor);
			armor.bodyPartAttached.DettachItem(armor, Utilities.GetNeededAttributeForArmor(armor));
        }
        internal void AddItem(Item newItem){
			this._items.Add (newItem);
			AddItemBonuses (newItem);
		}
		internal void RemoveItem(Item newItem){
			this._items.Remove (newItem);
			RemoveItemBonuses (newItem);
		}
        internal List<Weapon> GetAllAttachedWeapons() {
            List<Weapon> weapons = new List<Weapon>();
            for (int i = 0; i < items.Count; i++) {
                Item currItem = items[i];
                if(currItem.itemType == ITEM_TYPE.WEAPON) {
                    weapons.Add((Weapon)currItem);
                }
            }
            return weapons;
        }
        internal List<Armor> GetAllAttachedArmor() {
            List<Armor> weapons = new List<Armor>();
            for (int i = 0; i < items.Count; i++) {
                Item currItem = items[i];
                if (currItem.itemType == ITEM_TYPE.ARMOR) {
                    weapons.Add((Armor)currItem);
                }
            }
            return weapons;
        }
        internal bool HasEquipmentOfType(EQUIPMENT_TYPE equipmentType, IBodyPart.ATTRIBUTE attribute) {
            for (int i = 0; i < items.Count; i++) {
                Item currItem = items[i];
                if(currItem.itemType == ITEM_TYPE.ARMOR) {
                    Armor armor = (Armor)currItem;
                    if ((EQUIPMENT_TYPE)armor.armorType == equipmentType && (armor.attributes.Contains(attribute) || attribute == IBodyPart.ATTRIBUTE.NONE)) {
                        return true;
                    }
                } else if (currItem.itemType == ITEM_TYPE.WEAPON) {
                    Weapon weapon = (Weapon)currItem;
                    if ((EQUIPMENT_TYPE)weapon.weaponType == equipmentType && (weapon.attributes.Contains(attribute) || attribute == IBodyPart.ATTRIBUTE.NONE)) {
                        return true;
                    }
                }
            }
            return false;
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
        internal void IncreaseLevel() {
            _level += 1;
            _strength += strGain;
            _intelligence += intGain;
            _agility += agiGain;
			AdjustMaxHP (hpGain);
            _exp = 0;
        }
		internal void DecreaseLevel() {
			if(this._level > 1){
				_level -= 1;
				_strength -= strGain;
				_intelligence -= intGain;
				_agility -= agiGain;
				AdjustMaxHP (-hpGain);
				_exp = 0;
			}
		}
        #endregion
		private void AddItemBonuses(Item item){
			AdjustMaxHP (item.bonusMaxHP);
		}
		private void RemoveItemBonuses(Item item){
			AdjustMaxHP (-item.bonusMaxHP);
		}
		internal void AdjustMaxHP(int amount){
			if(this.currentHP == this.maxHP){
				this._maxHP += amount;
				this._currentHP = this._maxHP;
			}else{
				this._maxHP += amount;
			}
		}

		private void GetRandomCharacterColor(){
			_characterColor = CombatPrototypeManager.Instance.UseRandomCharacterColor ();
			_characterColorCode = ColorUtility.ToHtmlStringRGBA (_characterColor).Substring (0, 6);
		}
    }
}

