using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace ECS {
	[System.Serializable]
	public class Character : QuestCreator {
		[SerializeField] private string _name;
		private GENDER _gender;
		private List<Trait>	_traits;

		//Stats
		[SerializeField] private int _currentHP;
		[SerializeField] private int _maxHP;
		[SerializeField] private int _strength;
		[SerializeField] private int _intelligence;
		[SerializeField] private int _agility;
		[SerializeField] private int _currentRow;
		private SIDES _currentSide;
		private int _baseMaxHP;
		private int _baseStrength;
		private int _baseIntelligence;
		private int _baseAgility;

		//Skills
		private List<Skill> _skills;

		private CharacterClass _characterClass;
		private RaceSetting _raceSetting;
		private CharacterRole _role;
		private Faction _faction;
		private Party _party;
		private Quest _currentQuest;
		private HexTile _currLocation;
		private CharacterAvatar _avatar;

		[SerializeField] private List<BodyPart> _bodyParts;
		[SerializeField] private List<Item> _equippedItems;
		[SerializeField] private List<Item> _inventory;

		private Color _characterColor;
		private string _characterColorCode;
		private bool _isDead;
		private List<Quest> _activeQuests; //TODO: Move this to quest creator interface

		internal int actRate;

		#region getters / setters
		internal string name{
			get { return "[" + this._characterColorCode + "]" + this._name + "[-]"; }
		}
		internal GENDER gender{
			get { return _gender; }
		}
		internal int currentHP{
			get { return this._currentHP; }
		}
		internal int maxHP {
			get { return this._maxHP; }
		}
		internal int baseMaxHP {
			get { return _baseMaxHP; }
		}
		internal CharacterClass characterClass{
			get { return this._characterClass; }
		}
		internal RaceSetting raceSetting {
			get { return _raceSetting; }
		}
		internal CharacterRole role {
			get { return _role; }
		}
		internal Faction faction {
			get { return _faction; }
		}
		internal Party party {
			get { return _party; }
		}
		internal Quest currentQuest {
			get { return _currentQuest; }
		}
		internal HexTile currLocation{
			get { return _currLocation; }
		}
		internal CharacterAvatar avatar{
			get { return _avatar; }
		}
		internal List<BodyPart> bodyParts{
			get { return this._bodyParts; }
		}
		internal List<Item> equippedItems{
			get { return this._equippedItems; }
		}
		internal List<Item> inventory{
			get { return this._inventory; }
		}
		internal List<Trait> traits{
			get { return this._traits; }
		}
		internal List<Skill> skills{
			get { return this._skills; }
		}
		internal int currentRow{
			get { return this._currentRow; }
		}
		internal SIDES currentSide{
			get { return this._currentSide; }
		}
		internal bool isDead{
			get { return this._isDead; }
		}
		public List<Quest> activeQuests {
			get { return _activeQuests; }
		}
		internal int strength {
			get { return _strength + _equippedItems.Sum(x => x.bonusStrength); }
		}
		internal int baseStrength {
			get { return _baseStrength; }
		}
		internal int intelligence {
			get { return _intelligence + _equippedItems.Sum(x => x.bonusIntelligence); }
		}
		internal int baseIntelligence {
			get { return _baseIntelligence; }
		}
		internal int agility {
			get { return _agility + _equippedItems.Sum(x => x.bonusAgility); }
		}
		internal int baseAgility {
			get { return _baseAgility; }
		}
		internal int dodgeRate {
			get { return characterClass.dodgeRate + _equippedItems.Sum(x => x.bonusDodgeRate); }
		}
		internal int parryRate {
			get { return characterClass.parryRate + _equippedItems.Sum(x => x.bonusParryRate); }
		}
		internal int blockRate {
			get { return characterClass.blockRate + _equippedItems.Sum(x => x.bonusBlockRate); }
		}
		internal Color characterColor {
			get { return _characterColor; }
		}
		internal string characterColorCode {
			get { return _characterColorCode; }
		}
        public Settlement home {
            get { return _faction.settlements[0]; }
        }
        public float missingHP {
            get { return (float)currentHP / (float)maxHP; }
        }
        #endregion

        public Character(CharacterSetup baseSetup) {
			_characterClass = baseSetup.characterClass.CreateNewCopy();
			_raceSetting = baseSetup.raceSetting.CreateNewCopy();
			_name = RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender);
			_gender = Utilities.GetRandomGender();

			_baseMaxHP = _raceSetting.baseHP + (int)((float)_raceSetting.baseHP * (_characterClass.hpPercentage / 100f));
			_baseStrength = _raceSetting.baseStr + (int)((float)_raceSetting.baseStr * (_characterClass.strPercentage / 100f));
			_baseAgility = _raceSetting.baseAgi + (int)((float)_raceSetting.baseAgi * (_characterClass.agiPercentage / 100f));
			_baseIntelligence = _raceSetting.baseInt + (int)((float)_raceSetting.baseInt * (_characterClass.intPercentage / 100f));

			_maxHP = _baseMaxHP;
			_currentHP = _maxHP;
			_strength = _baseStrength;
			_agility = _baseAgility;
			_intelligence = _baseIntelligence;
			_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);

			_equippedItems = new List<Item> ();
			_inventory = new List<Item> ();
			_skills = GetGeneralSkills();
			_skills.AddRange (GetBodyPartSkills ());

			EquipPreEquippedItems (baseSetup);
			GetRandomCharacterColor ();

            _activeQuests = new List<Quest>();
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
			bool isAttackInRange = false;
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				skill.isEnabled = true;

				for (int j = 0; j < skill.skillRequirements.Length; j++) {
					SkillRequirement skillRequirement = skill.skillRequirements [j];
					if (!HasAttribute(skillRequirement.attributeRequired, skillRequirement.itemQuantity)) {
						skill.isEnabled = false;
						break;
					}
				}
				if(!skill.isEnabled){
					continue;
				}
				if(skill is AttackSkill){
					isAttackInRange = CombatPrototype.Instance.HasTargetInRangeForSkill (skill, this);
					if(!isAttackInRange){
						isAllAttacksInRange = false;
						skill.isEnabled = false;
						continue;
					}
				}else if (skill is FleeSkill){
					if(this.currentHP >= (this.maxHP / 2)){
						skill.isEnabled = false;
						continue;
					}
				}
			}


			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
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
									ECS.Character enemy = CombatPrototype.Instance.charactersSideB [j];
									if(enemy.currentRow < this._currentRow){
										hasEnemyOnLeft = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < CombatPrototype.Instance.charactersSideA.Count; j++) {
									ECS.Character enemy = CombatPrototype.Instance.charactersSideA [j];
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
									ECS.Character enemy = CombatPrototype.Instance.charactersSideB [j];
									if(enemy.currentRow > this._currentRow){
										hasEnemyOnRight = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < CombatPrototype.Instance.charactersSideA.Count; j++) {
									ECS.Character enemy = CombatPrototype.Instance.charactersSideA [j];
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

		//Changes character's side
		internal void SetSide(SIDES side){
			this._currentSide = side;
		}

		//Adjust current HP based on specified paramater, but HP must not go below 0
		internal void AdjustHP(int amount){
			this._currentHP += amount;
			this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
			if(this._currentHP == 0){
				Death ();
			}
		}

		//ECS.Character's death
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
		internal bool HasActivatableBodyPartSkill(){
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.BODY_PART){
					return true;
				}
			}
			return false;
		}
		internal bool HasActivatableWeaponSkill(){
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				if(skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.WEAPON){
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Items
		//If a character picks up an item, it is automatically added to his/her inventory
		internal void PickupItem(Item item){
			this._inventory.Add (item);
		}

		internal void ThrowItem(Item item){
			if(item.isEquipped){
				UnequipItem (item);
			}
			this._inventory.Remove (item);
		}

		//If character set up has pre equipped items, equip it here evey time a character is made
		internal void EquipPreEquippedItems(CharacterSetup charSetup){
			if(charSetup.preEquippedItems.Count > 0){
				for (int i = 0; i < charSetup.preEquippedItems.Count; i++) {
					EquipItem (charSetup.preEquippedItems [i].itemType, charSetup.preEquippedItems [i].itemName);
				}
			}
		}

		//For prototype only, in reality, an instance of an Item must be passed as parameter
		//Equip generic item, can be a weapon, armor, etc.
		public void EquipItem(ITEM_TYPE itemType, string itemName) {
			string strItemType = itemType.ToString();
			string path = "Assets/CombatPrototype/Data/Items/" + strItemType + "/" + itemName + ".json";
			if(System.IO.File.Exists(path)){
				string dataAsJson = System.IO.File.ReadAllText(path);
				if (strItemType.Contains("WEAPON")) {
					Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
					weapon.ConstructAllSkillsList ();
					TryEquipWeapon(weapon);
				} else if (strItemType.Contains("ARMOR")) {
					Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
					TryEquipArmor(armor);
				}
			}
		}
		public void EquipItem(string itemType, string itemName) {
			string path = "Assets/CombatPrototype/Data/Items/" + itemType + "/" + itemName + ".json";
			string dataAsJson = System.IO.File.ReadAllText(path);
			if (itemType.Contains("WEAPON")) {
				Weapon weapon = JsonUtility.FromJson<Weapon>(dataAsJson);
				weapon.ConstructAllSkillsList ();
				TryEquipWeapon(weapon);
			} else if (itemType.Contains("ARMOR")) {
				Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
				TryEquipArmor(armor);
			}
		}

		//TODO: For merge, this is the real way to equip an item
		internal void EquipItem(Item item){
			if (item is Weapon) {
				Weapon weapon = (Weapon)item;
				TryEquipWeapon(weapon);
			} else if (item is Armor) {
				Armor armor = (Armor)item;
				TryEquipArmor(armor);
			}
		}

		//Unequips an item of a character, whether it's a weapon, armor, etc.
		internal void UnequipItem(Item item){
			if(item is Weapon){
				UnequipWeapon ((Weapon)item);
			}else if(item is Armor){
				UnequipArmor ((Armor)item);
			}
			RemoveEquippedItem(item);
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
			AddEquippedItem(weapon);
			weapon.ResetDurability();
			weapon.SetOwner(this);
			for (int i = 0; i < weapon.skills.Count; i++) {
				this._skills.Add (weapon.skills [i]);
			}
			//          Debug.Log(this.name + " equipped " + weapon.itemName + " to " + bodyPart.bodyPart.ToString());
			//CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
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
		//Unequips weapon of a character
		private void UnequipWeapon(Weapon weapon) {
			DetachWeaponFromBodyParts (weapon);
			for (int i = 0; i < weapon.skills.Count; i++) {
				this._skills.Remove (weapon.skills [i]);
			}
		}

		//Try to equip an armor to a body part of this character and add it to the list of items this character have
		internal bool TryEquipArmor(Armor armor){
			IBodyPart bodyPartToEquip = GetBodyPartForArmor(armor);
			if(bodyPartToEquip == null){
				return false;
			}
			bodyPartToEquip.AttachItem(armor, Utilities.GetNeededAttributeForArmor(armor));
			//			armor.bodyPartAttached = bodyPart;
			AddEquippedItem(armor);
			armor.ResetDurability();
			armor.SetOwner(this);
			Debug.Log(this.name + " equipped " + armor.itemName + " to " + bodyPartToEquip.bodyPart.ToString());
			CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
			return true;
		}
		//Unequips armor of a character
		private void UnequipArmor(Armor armor) {
			armor.bodyPartAttached.DettachItem(armor, Utilities.GetNeededAttributeForArmor(armor));
		}
		internal void AddEquippedItem(Item newItem){
			this._inventory.Remove (newItem);
			this._equippedItems.Add (newItem);
			newItem.SetEquipped (true);
			AddItemBonuses (newItem);
		}
		internal void RemoveEquippedItem(Item newItem){
			this._inventory.Add (newItem);
			this._equippedItems.Remove (newItem);
			newItem.SetEquipped (false);
			RemoveItemBonuses (newItem);
		}
		internal bool HasWeaponEquipped(){
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if(currItem.itemType == ITEM_TYPE.WEAPON) {
					return true;
				}
			}
			return false;
		}
		internal bool HasArmorEquipped(){
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if(currItem.itemType == ITEM_TYPE.ARMOR) {
					return true;
				}
			}
			return false;
		}
		internal List<Weapon> GetAllAttachedWeapons() {
			List<Weapon> weapons = new List<Weapon>();
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if(currItem.itemType == ITEM_TYPE.WEAPON) {
					weapons.Add((Weapon)currItem);
				}
			}
			return weapons;
		}
		internal List<Armor> GetAllAttachedArmor() {
			List<Armor> weapons = new List<Armor>();
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
				if (currItem.itemType == ITEM_TYPE.ARMOR) {
					weapons.Add((Armor)currItem);
				}
			}
			return weapons;
		}
		internal bool HasEquipmentOfType(EQUIPMENT_TYPE equipmentType, IBodyPart.ATTRIBUTE attribute) {
			for (int i = 0; i < equippedItems.Count; i++) {
				Item currItem = equippedItems[i];
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
								CombatPrototypeUI.Instance.AddCombatLog(this.name + "'s " + bodyPart.bodyPart.ToString ().ToLower () + " is cured from " + statusEffect.ToString ().ToLower () + ".", this.currentSide);
								bodyPart.RemoveStatusEffectOnSecondaryBodyParts (statusEffect);
								bodyPart.statusEffects.RemoveAt (j);
								j--;
							}
						}
					}
				}
			}
		}

        internal bool HasStatusEffect(STATUS_EFFECT statusEffect) {
            for (int i = 0; i < this._bodyParts.Count; i++) {
                BodyPart bodyPart = this._bodyParts[i];
                if (bodyPart.statusEffects.Contains(statusEffect)) {
                    return true;
                }
            }
            return false;
        }
		#region Skills
		private List<Skill> GetGeneralSkills(){
			List<Skill> generalSkills = new List<Skill> ();
			string mainPath = "Assets/CombatPrototype/Data/Skills/GENERAL/";
			string[] folders = System.IO.Directory.GetDirectories (mainPath);
			for (int i = 0; i < folders.Length; i++) {
				string path = folders[i] + "/";
				DirectoryInfo di = new DirectoryInfo (path);
				if (di.Name == "ATTACK") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						AttackSkill attackSkill = JsonUtility.FromJson<AttackSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (attackSkill);
					}
				}else if (di.Name == "HEAL") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						HealSkill healSkill = JsonUtility.FromJson<HealSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (healSkill);
					}
				}else if (di.Name == "FLEE") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						FleeSkill fleeSkill = JsonUtility.FromJson<FleeSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (fleeSkill);
					}
				}else if (di.Name == "MOVE") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						MoveSkill moveSkill = JsonUtility.FromJson<MoveSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (moveSkill);
					}
				}else if (di.Name == "OBTAIN_ITEM") {
					foreach (string file in System.IO.Directory.GetFiles(path, "*.json")) {
						ObtainSkill obtainSkill = JsonUtility.FromJson<ObtainSkill> (System.IO.File.ReadAllText (file));
						generalSkills.Add (obtainSkill);
					}
				}
			}
			return generalSkills;
		}
		private List<Skill> GetBodyPartSkills(){
			List<Skill> allBodyPartSkills = new List<Skill>();
			for (int i = 0; i < CombatPrototypeManager.Instance.attributeSkills.Length; i++) {
				bool requirementsPassed = true;
				for (int j = 0; j < CombatPrototypeManager.Instance.attributeSkills[i].requirements.Length; j++) {
					if(!HasAttribute(CombatPrototypeManager.Instance.attributeSkills[i].requirements[j].attributeRequired, CombatPrototypeManager.Instance.attributeSkills[i].requirements[j].itemQuantity)){
						requirementsPassed = false;
						break;
					}
				}
				if(requirementsPassed){
					for (int j = 0; j < CombatPrototypeManager.Instance.attributeSkills[i].skills.Count; j++) {
						allBodyPartSkills.Add (CombatPrototypeManager.Instance.attributeSkills [i].skills [j].CreateNewCopy());
					}
				}
			}
			return allBodyPartSkills;
		}
		#endregion

		#region Stats
		internal void SetAgility (int amount){
			this._agility = amount;
		}
		internal void AdjustAgility (int amount){
			this._agility += amount;
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
		#region Roles
		public void AssignRole(CHARACTER_ROLE role) {
			switch (role) {
			case CHARACTER_ROLE.CHIEFTAIN:
				_role = new Chieftain();
				break;
			case CHARACTER_ROLE.VILLAGE_HEAD:
				_role = new VillageHead();
				break;
			case CHARACTER_ROLE.WARLORD:
				_role = new Warlord();
				break;
			case CHARACTER_ROLE.HERO:
				_role = new Hero();
				break;
			case CHARACTER_ROLE.TRADER:
				_role = new Trader();
				break;
			case CHARACTER_ROLE.ADVENTURER:
				_role = new Adventurer();
				break;
			case CHARACTER_ROLE.COLONIST:
				_role = new Colonist();
				break;
			case CHARACTER_ROLE.SPY:
				_role = new Spy();
				break;
			case CHARACTER_ROLE.MEDIATOR:
				_role = new Mediator();
				break;
			case CHARACTER_ROLE.NECROMANCER:
				_role = new Necromancer();
				break;
			case CHARACTER_ROLE.DRAGON_TAMER:
				_role = new DragonTamer();
				break;
			default:
				break;
			}
		}
		#endregion
		#region Character Class
		public void AssignClass(CharacterClass charClass) {
			_characterClass = charClass;
		}
		#endregion

		#region Traits
		public bool HasTrait(TRAIT trait) {
			for (int i = 0; i < _traits.Count; i++) {
				if(_traits[i].trait == trait) {
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Faction
		public void SetFaction(Faction faction) {
			_faction = faction;
		}
		#endregion

		#region Party
		public void SetParty(Party party) {
			_party = party;
		}
		#endregion

		#region Location
		public void SetLocation(HexTile location) {
			_currLocation = location;
		}
		#endregion

		#region Quests
        /*
         Determine what action the character will do, and execute that action.
             */
		internal void DetermineAction() {
			WeightedDictionary<Quest> actionWeights = GetActionWeights();
			if (actionWeights.GetTotalOfWeights() > 0) {
				Quest chosenAction = actionWeights.PickRandomElementGivenWeights();
                chosenAction.AcceptQuest(this);
			}
		}
        /*
         Get the weighted dictionary for what action the character will do next.
             */
		private WeightedDictionary<Quest> GetActionWeights() {
			WeightedDictionary<Quest> actionWeights = new WeightedDictionary<Quest>();
			for (int i = 0; i < _faction.internalQuestManager.activeQuests.Count; i++) {
				Quest currQuest = _faction.internalQuestManager.activeQuests[i];
                if (currQuest.CanAcceptQuest(this)) {
                    actionWeights.AddElement(currQuest, GetWeightForQuest(currQuest));
                }
            }

            if(this._party == null) {
                for (int i = 0; i < PartyManager.Instance.allParties.Count; i++) {
                    Party currParty = PartyManager.Instance.allParties[i];
                    if (!currParty.isFull && currParty.isOpen) {
                        JoinParty joinPartyTask = new JoinParty(this, -1, currParty);
                        if (joinPartyTask.CanAcceptQuest(this)) {
                            actionWeights.AddElement(joinPartyTask, GetWeightForQuest(joinPartyTask));
                        }
                    }
                }
            }
            
            Rest restTask = new Rest(this, -1);
			actionWeights.AddElement(restTask, GetWeightForQuest(restTask));

            GoHome goHomeTask = new GoHome(this, -1);
			actionWeights.AddElement(goHomeTask, GetWeightForQuest(goHomeTask));

            DoNothing doNothingTask = new DoNothing(this, -1);
			actionWeights.AddElement(doNothingTask, GetWeightForQuest(doNothingTask));
			return actionWeights;
		}

  //      private void StartExploreRegion() {
  //          List<Quest> exploreQuests = _faction.internalQuestManager.GetQuestsOfType(QUEST_TYPE.EXPLORE_REGION);
  //          if (exploreQuests.Count < 0) {
  //              throw new System.Exception("No explore region quests available! Explore region quest type should not have weight!");
  //          }
  //          Quest exploreQuest = exploreQuests[Random.Range(0, exploreQuests.Count)];
  //          exploreQuest.AcceptQuest(this);
  //      }
		//private void StartRestQuest() {
		//	//Rest restQuest = new Rest(this, 0, 1);
		//	//restQuest.StartQuestLine();
		//}
  //      private void StartDoNothing() {
  //          DoNothing doNothing = new DoNothing(this, -1, 1);
  //          AddNewQuest(doNothing);
  //          doNothing.AcceptQuest(this);
  //      }
  //      private void StartGoHome() {
        //    GoHome goHome = new GoHome(this, -1, 1);
        //    AddNewQuest(goHome);
        //    goHome.AcceptQuest(this);
        //}
        #endregion

        #region Quest Weights
        private int GetWeightForQuest(Quest quest) {
            int weight = 0;
            switch (quest.questType) {
                case QUEST_TYPE.EXPLORE_REGION:
                    weight += GetExploreRegionWeight((ExploreRegion)quest);
                    break;
                case QUEST_TYPE.OCCUPY_LANDMARK:
                    break;
                case QUEST_TYPE.INVESTIGATE_LANDMARK:
                    break;
                case QUEST_TYPE.OBTAIN_RESOURCE:
                    break;
                case QUEST_TYPE.EXPAND:
                    break;
                case QUEST_TYPE.REST:
                    weight += GetRestWeight();
                    break;
                case QUEST_TYPE.GO_HOME:
                    weight += GetGoHomeWeight();
                    break;
                case QUEST_TYPE.DO_NOTHING:
                    weight += GetDoNothingWeight();
                    break;
                case QUEST_TYPE.JOIN_PARTY:
                    weight += GetJoinPartyWeight((JoinParty)quest);
                    break;
                default:
                    break;
            }
            return weight;
        }

        private int GetExploreRegionWeight(ExploreRegion exploreRegionQuest) {
            int weight = 0;
            weight += 100; //Change algo if needed
            return weight;
        }
        private int GetJoinPartyWeight(JoinParty joinParty) {
            if(_party != null) {
                return 0; //if already in a party
            }
            int weight = 0;
            if(joinParty.partyToJoin.partyLeader.currLocation.id == this.currLocation.id) {
                //party leader and this character are at the same tile
                return 200;
            } else {
                List<HexTile> pathToParty = PathGenerator.Instance.GetPath(this.currLocation, joinParty.partyToJoin.partyLeader.currLocation, PATHFINDING_MODE.USE_ROADS);
                if(pathToParty != null) {
                    weight += 200 - (15 * pathToParty.Count); //200 - (15 per tile distance) if not in a party
                }
            }
            return weight;
        }
        private int GetRestWeight() {
            if (_currentHP < maxHP) {
                int percentMissing = _currentHP / maxHP;
                return 5 * percentMissing; //5 Weight per % of HP below max HP
            }
            return 0;
        }
        private int GetGoHomeWeight() {
            //0 if already at Home Settlement or no path to it
            if (currLocation.isHabitable && currLocation.isOccupied && currLocation.landmarkOnTile.owner == this._faction) {
                return 0;
            }
            if (PathGenerator.Instance.GetPath(currLocation, _faction.settlements[0].location, PATHFINDING_MODE.USE_ROADS) == null) {
                return 0;
            }
            return 5; //5 if not
        }
        private int GetDoNothingWeight() {
            return 10;
        }

        //private int GetWeightForQuestType(QUEST_TYPE questType) {
        //    int weight = 0;
        //    switch (questType) {
        //        case QUEST_TYPE.EXPLORE_REGION:
        //            weight += GetExploreRegionWeight();
        //            break;
        //        case QUEST_TYPE.OCCUPY_LANDMARK:
        //            break;
        //        case QUEST_TYPE.INVESTIGATE_LANDMARK:
        //            break;
        //        case QUEST_TYPE.OBTAIN_RESOURCE:
        //            break;
        //        case QUEST_TYPE.EXPAND:
        //            break;
        //        case QUEST_TYPE.REST:
        //            weight += GetRestWeight();
        //            break;
        //        case QUEST_TYPE.GO_HOME:
        //            weight += GetGoHomeWeight();
        //            break;
        //        case QUEST_TYPE.DO_NOTHING:
        //            weight += GetDoNothingWeight();
        //            break;
        //        default:
        //            break;
        //    }
        //    return weight;
        //}
        //private int GetExploreRegionWeight() {
        //    int weight = 0;
        //    switch (_role.roleType) {
        //        case CHARACTER_ROLE.CHIEFTAIN:
        //            weight += 100;
        //            break;
        //        default:
        //            break;
        //    }
        //    return weight;
        //}
        //private int GetRestWeight() {
        //    if (_currentHP < maxHP) {
        //        int percentMissing = _currentHP / maxHP;
        //        return 5 * percentMissing;
        //    }
        //    return 0;
        //}
        //private int GetGoHomeWeight() {
        //    //0 if already at Home Settlement or no path to it
        //    if (currLocation.isHabitable && currLocation.isOccupied && currLocation.landmarkOnTile.owner == this._faction) {
        //        return 0;
        //    }
        //    if (PathGenerator.Instance.GetPath(currLocation, _faction.settlements[0].location, PATHFINDING_MODE.USE_ROADS) == null) {
        //        return 0;
        //    }
        //    return 5; //5 if not
        //}
        #endregion

        #region HP
        int regenAmount;
		public void StartRegeneration(int amount) {
			regenAmount = amount;
			Messenger.AddListener("OnDayEnd", RegenerateHealth);
		}
		public void StopRegeneration() {
			regenAmount = 0;
			Messenger.RemoveListener("OnDayEnd", RegenerateHealth);
		}
		public void RegenerateHealth() {
			AdjustHP(regenAmount);
		}
		#endregion

		#region Avatar
		public void CreateNewAvatar() {
			//TODO: Only create one avatar per character, then enable disable it based on need, rather than destroying it then creating a new avatar when needed
			GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterAvatar", this.currLocation.transform.position, Quaternion.identity);
			CharacterAvatar avatar = avatarGO.GetComponent<CharacterAvatar>();
            avatar.Init(this);
        }
		public void SetAvatar(CharacterAvatar avatar) {
			_avatar = avatar;
		}
		public void DestroyAvatar() {
			if(_avatar != null) {
				_avatar.DestroyObject();
			}
		}
		#endregion

		#region Quest Management
		public void AddNewQuest(Quest quest) {
			if (!_activeQuests.Contains(quest)) {
				_activeQuests.Add(quest);
			}
		}
		public void RemoveQuest(Quest quest) {
			_activeQuests.Remove(quest);
		}
		public void SetCurrentQuest(Quest currentQuest) {
			this._currentQuest = currentQuest;
		}
		public List<Quest> GetQuestsOfType(QUEST_TYPE questType) {
			List<Quest> quests = new List<Quest>();
			for (int i = 0; i < _activeQuests.Count; i++) {
				Quest currQuest = _activeQuests[i];
				if(currQuest.questType == questType) {
					quests.Add(currQuest);
				}
			}
			return quests;
		}
		#endregion
	}
}
