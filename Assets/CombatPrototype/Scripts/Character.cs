using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace ECS {
	[System.Serializable]
	public class Character : TaskCreator, ICombatInitializer {
		[SerializeField] private string _name;
        private int _id;
		private GENDER _gender;
        [System.NonSerialized] private CharacterType _characterType; //Base Character Type(For Traits)
        [System.NonSerialized] private List<Trait>	_traits;
        private List<TRAIT> _allTraits;
		private List<CharacterTag>	_tags;
        private Dictionary<Character, Relationship> _relationships;
		private const int MAX_FOLLOWERS = 4;

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
		private int _fixedMaxHP;
		private StatsModifierPercentage _statsModifierPercentage;

		//Skills
		private List<Skill> _skills;

		private CharacterClass _characterClass;
		private RaceSetting _raceSetting;
		private CharacterRole _role;
		private Faction _faction;
		private Party _party;
        private QuestData _questData;
		private CharacterTask _currentTask;
        private ILocation _specificLocation;
		private CharacterAvatar _avatar;

		[SerializeField] private List<BodyPart> _bodyParts;
		[SerializeField] private List<Item> _equippedItems;
		[SerializeField] private List<Item> _inventory;

		private Color _characterColor;
		private string _characterColorCode;
		private bool _isDead;
		private bool _isFainted;
		private bool _isPrisoner;
        private bool _isFollower;
		private bool _isDefeated;
		private object _isPrisonerOf;
		private List<OldQuest.Quest> _activeQuests;
		private BaseLandmark _home;
        private BaseLandmark _lair;
		private List<string> _history;
		private int _combatHistoryID;
		private List<ECS.Character> _prisoners;
		private List<ECS.Character> _followers;
		private ECS.Character _isFollowerOf;

        private Dictionary<RACE, int> _civiliansByRace;

		internal int actRate;
		internal CombatPrototype currentCombat;
		internal Dictionary<int, CombatPrototype> combatHistory;

		private float _equippedWeaponPower;
        private int _gold;
        private int _prestige;

		private Action _currentFunction;
		private bool _isInCombat;

        //When the character should have a next action it should do after it's current one.
        private CharacterTask nextTaskToDo;

//        private Dictionary<MATERIAL, int> _materialInventory;
		private Dictionary<int, BaseLandmark> _exploredLandmarks;

		#region getters / setters
        internal string firstName {
            get { return name.Split(' ')[0]; }
        }
		internal string name{
			get { return this._name; }
		}
		internal string coloredName{
			get { return "[" + this._characterColorCode + "]" + this._name + "[-]"; }
		}
		internal string urlName{
			get { return "[url=" + this._id.ToString() + "_character]" + this._name + "[/url]"; }
		}
		internal string coloredUrlName{
			get { return "[url=" + this._id.ToString() + "_character]" + "[" + this._characterColorCode + "]" + this._name + "[-]" + "[/url]"; }
		}
		internal string prisonerName{
			get { return "[url=" + this._id.ToString() + "_prisoner]" + this._name + "[/url]"; }
		}
        internal int id {
            get { return _id; }
        }
		internal GENDER gender{
			get { return _gender; }
		}
        internal List<Trait> traits {
            get { return _traits; }
        }
		internal List<CharacterTag> tags {
			get { return _tags; }
		}
        internal Dictionary<Character, Relationship> relationships {
            get { return _relationships; }
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
		public Faction faction {
			get { return _faction; }
		}
		internal Party party {
			get { return _party; }
		}
		public CharacterTask currentTask {
			get { return _currentTask; }
		}
        public ILocation specificLocation {
            get {
                ILocation loc = null;
				loc = (party == null ? (_isFollowerOf == null ? _specificLocation : _isFollowerOf.specificLocation) : party.specificLocation);
                return loc;
            }
        }
		internal HexTile currLocation{
			get { return (this.specificLocation != null ? this.specificLocation.tileLocation : null); }
		}
		public CharacterAvatar avatar{
			get { return _avatar; }
		}
		internal List<BodyPart> bodyParts{
			get { return this._bodyParts; }
		}
		internal List<Item> equippedItems {
			get { return this._equippedItems; }
		}
		internal List<Item> inventory{
			get { return this._inventory; }
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
		internal bool isFainted{
			get { return this._isFainted; }
		}
		internal bool isPrisoner{
			get { return this._isPrisoner; }
		}
        internal bool isFollower {
            get { return _isFollower; }
        }
		public List<OldQuest.Quest> activeQuests {
			get { return _activeQuests; }
		}
		internal int strength {
			get { return _strength + (int)((float)_strength * _statsModifierPercentage.strPercentage); }
		}
		internal int baseStrength {
			get { return _baseStrength; }
		}
		internal int intelligence {
			get { return _intelligence + (int)((float)_intelligence * _statsModifierPercentage.intPercentage); }
		}
		internal int baseIntelligence {
			get { return _baseIntelligence; }
		}
		internal int agility {
			get { return _agility + (int)((float)_agility * _statsModifierPercentage.agiPercentage); }
		}
		internal int baseAgility {
			get { return _baseAgility; }
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
		internal StatsModifierPercentage statsModifierPercentage {
			get { return _statsModifierPercentage; }
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
        internal BaseLandmark home {
            get { return _home; }
        }
        internal BaseLandmark lair {
            get { return _lair; }
        }
		internal float remainingHP { //Percentage of remaining HP this character has
            get { return (float)currentHP / (float)maxHP; }
        }
		internal List<string> history{
			get { return this._history; }
		}
		internal float characterPower{
			get { return (float)_currentHP + (float)((_strength + _intelligence + _agility) * 2) + _equippedWeaponPower;}
		}
		internal float equippedWeaponPower{
			get { return _equippedWeaponPower; }
		}
		internal List<ECS.Character> prisoners{
			get { return this._prisoners; }
		}
		internal List<ECS.Character> followers{
			get { return this._followers; }
		}
		internal object isPrisonerOf{
			get { return this._isPrisonerOf; }
		}
        internal int gold {
            get { return _gold; }
        }
        internal int prestige {
            get { return _prestige; }
		}
		public bool isDefeated {
			get {
//				if(_party != null){
//					return _party.isDefeated;
//				}
				return _isDefeated; 
			}
		}
        public int civilians {
            get { 
				if(_civiliansByRace == null){
					return 0;
				}
				return _civiliansByRace.Sum(x => x.Value); 
			}
        }
		public Dictionary<RACE, int> civiliansByRace{
			get { return _civiliansByRace; }
		}
//      public Dictionary<MATERIAL, int> materialInventory {
//          get { return _materialInventory; }
//      }
		public Dictionary<int, BaseLandmark> exploredLandmarks {
			get { return _exploredLandmarks; }
		}
		public bool isInCombat{
			get {
				if(_party != null){
					return _party.isInCombat;
				}
				return _isInCombat; 
			}
		}
		public Action currentFunction{
			get { 
//				if(_party != null){
//					return _party.currentFunction;
//				}
				return _currentFunction; 
			}
		}
        public bool isFactionless {
            get { return faction == null; }
        }
		internal bool isFollowersFull{
			get { return followers.Count >= MAX_FOLLOWERS; }
		}
		internal ECS.Character isFollowerOf{
			get { return _isFollowerOf; }
		}
		public ECS.Character mainCharacter{
			get { return this; }
		}
        #endregion

        public Character(CharacterSetup baseSetup, int statAllocationBonus = 0) {
            _id = Utilities.SetID(this);
			_characterClass = baseSetup.characterClass.CreateNewCopy();
			_raceSetting = baseSetup.raceSetting.CreateNewCopy();
			_gender = Utilities.GetRandomGender();
            _name = RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender);
            _traits = new List<Trait> ();
			_tags = new List<CharacterTag> ();
            _relationships = new Dictionary<Character, Relationship>();
			_exploredLandmarks = new Dictionary<int, BaseLandmark> ();
			_isDead = false;
			_isFainted = false;
			_isPrisoner = false;
			_isDefeated = false;
			_isPrisonerOf = null;
			_prisoners = new List<ECS.Character> ();
			_history = new List<string> ();
			_followers = new List<ECS.Character> ();
			_isFollowerOf = null;
			_statsModifierPercentage = new StatsModifierPercentage ();
            _questData = new QuestData();

			GenerateRaceTags ();

			AllocateStatPoints (statAllocationBonus);

            GenerateTraits();

			_strength = _baseStrength;
			_agility = _baseAgility;
			_intelligence = _baseIntelligence;
			_bodyParts = new List<BodyPart>(_raceSetting.bodyParts);

			_equippedItems = new List<Item> ();
			_inventory = new List<Item> ();
			_skills = GetGeneralSkills();
			_skills.AddRange (GetBodyPartSkills ());
            _history = new List<string>();

			AdjustMaxHP (_baseMaxHP);

            EquipPreEquippedItems (baseSetup);
			GetRandomCharacterColor ();

//          _maxHP = _baseMaxHP;
//          _currentHP = maxHP;

            _activeQuests = new List<OldQuest.Quest>();
			currentCombat = null;
			combatHistory = new Dictionary<int, CombatPrototype> ();
			_combatHistoryID = 0;
            //ConstructMaterialInventory();
		}

		private void AllocateStatPoints(int statAllocationBonus){
			_baseMaxHP = _raceSetting.baseHP;
			_baseStrength = _raceSetting.baseStr;
			_baseAgility = _raceSetting.baseAgi;
			_baseIntelligence = _raceSetting.baseInt;

			WeightedDictionary<string> statWeights = new WeightedDictionary<string> ();
			statWeights.AddElement ("strength", _raceSetting.strWeightAllocation);
			statWeights.AddElement ("intelligence", _raceSetting.intWeightAllocation);
			statWeights.AddElement ("agility", _raceSetting.agiWeightAllocation);
			statWeights.AddElement ("hp", _raceSetting.hpWeightAllocation);

			if(statWeights.GetTotalOfWeights() > 0){
				string chosenStat = string.Empty;
				int totalStatAllocation = _raceSetting.statAllocationPoints + statAllocationBonus;
				for (int i = 0; i < totalStatAllocation; i++) {
					chosenStat = statWeights.PickRandomElementGivenWeights ();
					if (chosenStat == "strength") {
						_baseStrength += 5;
					}else if (chosenStat == "intelligence") {
						_baseIntelligence += 5;
					}else if (chosenStat == "agility") {
						_baseAgility += 5;
					}else if (chosenStat == "hp") {
						_baseMaxHP += 50;
					}
				}
			}

			_baseMaxHP += (int)((float)_baseMaxHP * (_characterClass.hpPercentage / 100f));
			_baseStrength += (int)((float)_baseStrength * (_characterClass.strPercentage / 100f));
			_baseAgility += (int)((float)_baseAgility * (_characterClass.agiPercentage / 100f));
			_baseIntelligence += (int)((float)_baseIntelligence * (_characterClass.intPercentage / 100f));
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

		internal bool HasBodyPart(string bodyPartType){
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];

				if(bodyPart.name == bodyPartType){
					return true;
				}

				for (int j = 0; j < bodyPart.secondaryBodyParts.Count; j++) {
					SecondaryBodyPart secondaryBodyPart = bodyPart.secondaryBodyParts [j];
					if(secondaryBodyPart.name == bodyPartType){
						return true;
					}
				}
			}
			return false;
		}


		//Enables or Disables skills based on skill requirements
		internal void EnableDisableSkills(CombatPrototype combat){
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
					isAttackInRange = combat.HasTargetInRangeForSkill (skill, this);
					if(!isAttackInRange){
						isAllAttacksInRange = false;
						skill.isEnabled = false;
						continue;
					}
				}else if (skill is FleeSkill){
					skill.isEnabled = false;
					continue;
//					if(this.currentHP >= (this.maxHP / 2)){
//						skill.isEnabled = false;
//						continue;
//					}
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
							if(combat.charactersSideA.Contains(this)){
								for (int j = 0; j < combat.charactersSideB.Count; j++) {
									ECS.Character enemy = combat.charactersSideB [j];
									if(enemy.currentRow < this._currentRow){
										hasEnemyOnLeft = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < combat.charactersSideA.Count; j++) {
									ECS.Character enemy = combat.charactersSideA [j];
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
							if(combat.charactersSideA.Contains(this)){
								for (int j = 0; j < combat.charactersSideB.Count; j++) {
									ECS.Character enemy = combat.charactersSideB [j];
									if(enemy.currentRow > this._currentRow){
										hasEnemyOnRight = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < combat.charactersSideA.Count; j++) {
									ECS.Character enemy = combat.charactersSideA [j];
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
				FaintOrDeath ();
			}
		}
		internal void SetHP(int amount){
			this._currentHP = amount;
		}
		private string GetFaintOrDeath(){
			WeightedDictionary<string> faintDieDict = new WeightedDictionary<string> ();
			int faintWeight = 100;
			int dieWeight = 50;
			if(HasTrait(TRAIT.GRITTY)){
				faintWeight += 50;
			}
			if(HasTrait(TRAIT.ROBUST)){
				faintWeight += 50;
			}
			if(HasTrait(TRAIT.FRAGILE)){
				dieWeight += 50;
			}
			faintDieDict.AddElement ("faint", 100);
			faintDieDict.AddElement ("die", 50);

			return faintDieDict.PickRandomElementGivenWeights ();
		}
		internal void FaintOrDeath(){
			string pickedWeight = GetFaintOrDeath ();
			if(pickedWeight == "faint"){
				if(this.currentCombat == null){
					Faint ();
				}else{
                    this.currentCombat.CharacterFainted(this);
                }
			}else if(pickedWeight == "die"){
				if(this.currentCombat == null){
					Death ();
				}else{
					this.currentCombat.CharacterDeath (this);
				}
			}
		}

		//When character will faint
		internal void Faint(){
			if(!_isFainted){
				_isFainted = true;
				SetHP (1);
				if (this._party != null) {
					this._party.RemovePartyMember(this);
				}
				if (this._isFollowerOf != null) {
					this._isFollowerOf.RemoveFollower(this);
				}
			}
		}

		internal void Unfaint(){
			if (_isFainted) {
				_isFainted = false;
				SetHP (1);
			}
		}

		//ECS.Character's death
		internal void Death(){
			if(!_isDead){
				_isDead = true;
				CombatPrototypeManager.Instance.ReturnCharacterColorToPool (_characterColor);

				if(specificLocation is BaseLandmark){
					(specificLocation as BaseLandmark).AddHistory (this.name + " died.");
				}
				if(_home != null){
                    //Remove character home on landmark
					_home.RemoveCharacterHomeOnLandmark (this);
				}

				if(this._faction != null){
					if(this._faction.leader != null && this._faction.leader.id == this.id) {
						//If this character is the leader of a faction, set that factions leader as null
						this._faction.SetLeader(null);
					}
					this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
				}

                CheckForInternationalIncident();

                if (this._party != null) {
                    this._party.RemovePartyMember(this, true);
				}else{
					this.specificLocation.RemoveCharacterFromLocation(this);
				}

				if(this._isFollowerOf != null){
					this._isFollowerOf.RemoveFollower (this);
					if(this.specificLocation != null){
						this.specificLocation.RemoveCharacterFromLocation(this);
					}
				}

                if (_avatar != null) {
                    _avatar.RemoveCharacter(this); //if the character has an avatar, remove it from the list of characters
                }

                if (_isPrisoner){
					PrisonerDeath ();
				}
//				if(Messenger.eventTable.ContainsKey("CharacterDeath")){
//					Messenger.Broadcast ("CharacterDeath", this);
//				}
            }
		}
        private void CheckForInternationalIncident() {
            //a non-Adventurer character from a tribe dies while in a region owned by another tribe
			if(this._role == null){
				return;
			}
            if (this.faction == null) {
                return;
            }
            Faction ownerOfCurrLocation = this.currLocation.region.owner;
            if (ownerOfCurrLocation.id != this.faction.id) {
                if(currentTask != null && currentTask.taskType == TASK_TYPE.QUEST) { //if this character is in a quest when he/she died
                    OldQuest.Quest currentQuest = (OldQuest.Quest)currentTask;
                    if (FactionManager.Instance.IsQuestHarmful(currentQuest.questType)) { //check if the quest is meant to negatively impact a faction
                        //if it is, check if this character died on a region owned by the faction he/she means to negatively impact
                        //if he/she is, do not count this character's death as an international incident
                        if (currentQuest.targetFaction.id != ownerOfCurrLocation.id) {
                            //otherwise, this is an international incident
                            FactionManager.Instance.InternationalIncidentOccured(this.faction, this.currLocation.region.owner, INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH, this);
                        }
                    } else {
                        FactionManager.Instance.InternationalIncidentOccured(this.faction, this.currLocation.region.owner, INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH, this);
                    }
                } else {
                    FactionManager.Instance.InternationalIncidentOccured(this.faction, this.currLocation.region.owner, INTERNATIONAL_INCIDENT_TYPE.CHARACTER_DEATH, this);
                }
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
			if(item.owner == null){
				OwnItem (item);
			}
			AddHistory ("Obtained " + item.itemName + ".");
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
					weapon.ConstructSkillsList ();
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
				weapon.ConstructSkillsList ();
				TryEquipWeapon(weapon);
			} else if (itemType.Contains("ARMOR")) {
				Armor armor = JsonUtility.FromJson<Armor>(dataAsJson);
				TryEquipArmor(armor);
			}
		}

        /*
         this is the real way to equip an item
         this will return a boolean whether the character successfully equipped
         the item or not.
             */
        internal bool EquipItem(Item item){
			bool hasEquipped = false;
			if (item is Weapon) {
				Weapon weapon = (Weapon)item;
				hasEquipped = TryEquipWeapon(weapon);
			} else if (item is Armor) {
				Armor armor = (Armor)item;
				hasEquipped = TryEquipArmor(armor);
			}
//			if(hasEquipped){
//				AddHistory ("Equipped " + item.itemName + ".");
//			}
			return hasEquipped;
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

		//Unown an item making the owner of it null, if successfully unowned, return true, otherwise, return false
		internal bool UnownItem(Item item){
			if(item.owner.id == this._id){
				AddHistory ("Unowned " + item.nameWithQuality + ".");
				item.SetOwner (null);
				return true;
			}
			return false;
		}

		//Own an Item
		internal void OwnItem(Item item){
			AddHistory ("Owned " + item.nameWithQuality + ".");
			item.SetOwner (this);
		}

		//Transfer item ownership
		internal void TransferItemOwnership(Item item, Character newOwner){
			AddHistory ("Transfered " + item.nameWithQuality + " ownership to " + newOwner.name + ".");
			newOwner.OwnItem (item);
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
//			weapon.SetOwner(this);
			if(weapon.owner == null){
				OwnItem (weapon);
			}
			_equippedWeaponPower += weapon.weaponPower;

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
			_equippedWeaponPower -= weapon.weaponPower;
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
//			armor.SetOwner(this);
			if(armor.owner == null){
				OwnItem (armor);
			}
			Debug.Log(this.name + " equipped " + armor.itemName + " to " + bodyPartToEquip.name);
            if(CombatPrototypeUI.Instance != null) {
                CombatPrototypeUI.Instance.UpdateCharacterSummary(this);
            }
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
        internal bool HasEquipmentOfType(EQUIPMENT_TYPE equipmentType) {
            for (int i = 0; i < equippedItems.Count; i++) {
                Item currItem = equippedItems[i];
                if (currItem.itemType == ITEM_TYPE.ARMOR) {
                    Armor armor = (Armor)currItem;
                    if ((EQUIPMENT_TYPE)armor.armorType == equipmentType) {
                        return true;
                    }
                } else if (currItem.itemType == ITEM_TYPE.WEAPON) {
                    Weapon weapon = (Weapon)currItem;
                    if ((EQUIPMENT_TYPE)weapon.weaponType == equipmentType) {
                        return true;
                    }
                }
            }
            return false;
        }
        /*
         Get the most needed armor type of this character,
         if this returns NONE. That means that this character already
         has all types of armor equipped.
             */
        internal ARMOR_TYPE GetNeededArmorType() {
            for (int i = 0; i < Utilities.orderedArmorTypes.Count; i++) {
                ARMOR_TYPE currArmorType = Utilities.orderedArmorTypes[i];
                //Get the armor type that the character doesn't currently have
                if (!this.HasEquipmentOfType((EQUIPMENT_TYPE)currArmorType)) {
                    return currArmorType; //character has not yet equipped an armor of this type
                }
            }
            return ARMOR_TYPE.NONE;
        }
        /*
         Get armor types that this character has not equipped yet.
             */
        internal List<EQUIPMENT_TYPE> GetMissingArmorTypes() {
            List<EQUIPMENT_TYPE> missingArmor = new List<EQUIPMENT_TYPE>();
            for (int i = 0; i < Utilities.orderedArmorTypes.Count; i++) {
                EQUIPMENT_TYPE currArmorType = (EQUIPMENT_TYPE)Utilities.orderedArmorTypes[i];
                //Get the armor type that the character doesn't currently have
                if (!this.HasEquipmentOfType((EQUIPMENT_TYPE)currArmorType)) {
                    missingArmor.Add(currArmorType); //character has not yet equipped an armor of this type
                }
            }
            return missingArmor;
        }
        /*
        Get a random weapon type for a character
        given its allowed weapon types.
             */
        internal WEAPON_TYPE GetWeaponTypeForCharacter() {
            if (_characterClass.className.Equals("Classless")) {
                return WEAPON_TYPE.NONE;
            }
            return this.characterClass.allowedWeaponTypes[UnityEngine.Random.Range(0, this.characterClass.allowedWeaponTypes.Count)];
        }
        internal void AdjustGold(int amount) {
            _gold += amount;
            _gold = Mathf.Max(0, _gold);
        }
        internal void AdjustPrestige(int amount) {
            _prestige += amount;
            _prestige = Mathf.Max(0, _prestige);
        }
        /*
         Get the Item type that this character needs,
         this is mainly used for upgrading gear at a settlement.
             */
        internal ITEM_TYPE GetNeededItemType() {
            if (!HasWeaponEquipped()) {
                return ITEM_TYPE.WEAPON;
            }
            return ITEM_TYPE.ARMOR;
        }
        /*
         Get the list of equipment that
         this character needs
             */
        internal List<EQUIPMENT_TYPE> GetNeededEquipmentTypes() {
            List<EQUIPMENT_TYPE> neededEquipment = new List<EQUIPMENT_TYPE>();
            if (!_characterClass.className.Equals("Classless") && !HasWeaponEquipped()) {
                neededEquipment.Add((EQUIPMENT_TYPE)GetWeaponTypeForCharacter());
            }
            neededEquipment.AddRange(GetMissingArmorTypes());
            return neededEquipment;
        }
        #endregion

        internal void CureStatusEffects(){
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];
				if(bodyPart.statusEffects.Count > 0){
					for (int j = 0; j < bodyPart.statusEffects.Count; j++) {
						STATUS_EFFECT statusEffect = bodyPart.statusEffects [j];
						if(statusEffect != STATUS_EFFECT.DECAPITATED){
							int chance = Utilities.rng.Next (0, 100);
							if(chance < 15){
								CombatPrototypeManager.Instance.combat.AddCombatLog(this.name + "'s " + bodyPart.name.ToLower () + " is cured from " + statusEffect.ToString ().ToLower () + ".", this.currentSide);
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
			this._strength += item.bonusStrength;
			this._intelligence += item.bonusIntelligence;
			this._agility += item.bonusAgility;
		}
		private void RemoveItemBonuses(Item item){
			AdjustMaxHP (-item.bonusMaxHP);
			this._strength -= item.bonusStrength;
			this._intelligence -= item.bonusIntelligence;
			this._agility -= item.bonusAgility;
		}
		internal void AdjustMaxHP(int amount){
			this._fixedMaxHP += amount;
//			this._maxHP = this._maxHP + (int)((float)this._maxHP * _statsModifierPercentage.hpPercentage);
			RecomputeMaxHP();
		}
		private void RecomputeMaxHP(){
			int previousMaxHP = this._maxHP;
			this._maxHP = this._fixedMaxHP + (int)((float)this._fixedMaxHP * _statsModifierPercentage.hpPercentage);
			if(this._currentHP > this._maxHP || this._currentHP == previousMaxHP){
				this._currentHP = this._maxHP;
			}
		}

		private void GetRandomCharacterColor(){
			_characterColor = CombatPrototypeManager.Instance.UseRandomCharacterColor ();
			_characterColorCode = ColorUtility.ToHtmlStringRGBA (_characterColor).Substring (0, 6);
		}
		public void SetCharacterColor(Color color){
			_characterColor = color;
			_characterColorCode = ColorUtility.ToHtmlStringRGBA (_characterColor).Substring (0, 6);
		}

		#region Roles
		public void AssignRole(CHARACTER_ROLE role) {
			switch (role) {
			case CHARACTER_ROLE.CHIEFTAIN:
				_role = new Chieftain(this);
				break;
			case CHARACTER_ROLE.VILLAGE_HEAD:
				_role = new VillageHead(this);
				break;
			case CHARACTER_ROLE.WARLORD:
				_role = new Warlord(this);
				break;
			case CHARACTER_ROLE.HERO:
				_role = new Hero(this);
				break;
			//case CHARACTER_ROLE.ADVENTURER:
			//	_role = new Adventurer(this);
			//	break;
			case CHARACTER_ROLE.COLONIST:
				_role = new Colonist(this);
				break;
            case CHARACTER_ROLE.WORKER:
                _role = new Worker(this);
                break;
			case CHARACTER_ROLE.TAMED_BEAST:
				_role = new TamedBeast(this);
				break;
			case CHARACTER_ROLE.FLYING_BEAST:
				_role = new FlyingBeast(this);
				break;
            case CHARACTER_ROLE.ANCIENT_VAMPIRE:
                _role = new AncientVampire(this);
                break;
            default:
			    break;
			}
		}
		public void ChangeRole(){
			//TODO: Things to do when a character changes role

//			AssignRole(role);
			if(_raceSetting.tags.Contains(CHARACTER_TAG.SAPIENT)){
				CHARACTER_ROLE roleToCreate = CHARACTER_ROLE.WORKER;
				WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionaryNoRestrictions(this._faction, (Settlement)this._home);
				if (characterRoleProductionDictionary.GetTotalOfWeights () > 0) {
					roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights ();
				}
				AssignRole(roleToCreate);
			}else{
				AssignRole (CHARACTER_ROLE.TAMED_BEAST);
			}
		}
		#endregion

		#region Character Class
		public void AssignClass(CharacterClass charClass) {
			_characterClass = charClass;
		}
		#endregion

		#region Traits
        private void GenerateTraits() {
            CharacterType baseCharacterType = CharacterManager.Instance.GetRandomCharacterType();
            _characterType = baseCharacterType;
            _allTraits = new List<TRAIT>(baseCharacterType.otherTraits);
            //Charisma
            if (baseCharacterType.charismaTrait == CHARISMA.NONE) {
                TRAIT charismaTrait = GenerateCharismaTrait();
                if(charismaTrait != TRAIT.NONE) {
                    _allTraits.Add(charismaTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.charismaTrait);
            }
            //Intelligence
            if (baseCharacterType.intelligenceTrait == INTELLIGENCE.NONE) {
                TRAIT intTrait = GenerateIntelligenceTrait();
                if(intTrait != TRAIT.NONE) {
                    _allTraits.Add(intTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.intelligenceTrait);
            }
            //Efficiency
            if (baseCharacterType.efficiencyTrait == EFFICIENCY.NONE) {
                TRAIT efficiencyTrait = GenerateEfficiencyTrait();
                if (efficiencyTrait != TRAIT.NONE) {
                    _allTraits.Add(efficiencyTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.efficiencyTrait);
            }
            //Military
            if (baseCharacterType.militaryTrait == MILITARY.NONE) {
                TRAIT militaryTrait = GenerateMilitaryTrait();
                _allTraits.Add(militaryTrait);
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.militaryTrait);
            }
            //Health
            if (baseCharacterType.healthTrait == HEALTH.NONE) {
                TRAIT healthTrait = GenerateHealthTrait();
                if(healthTrait != TRAIT.NONE) {
                    _allTraits.Add(healthTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.healthTrait);
            }
            //Strength
            if (baseCharacterType.strengthTrait == STRENGTH.NONE) {
                TRAIT strengthTrait = GenerateStrengthTrait();
                if (strengthTrait != TRAIT.NONE) {
                    _allTraits.Add(strengthTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.strengthTrait);
            }
            //Agility
            if (baseCharacterType.agilityTrait == AGILITY.NONE) {
                TRAIT agilityTrait = GenerateAgilityTrait();
                if (agilityTrait != TRAIT.NONE) {
                    _allTraits.Add(agilityTrait);
                }
            } else {
                _allTraits.Add((TRAIT)baseCharacterType.agilityTrait);
            }
            _traits = new List<Trait>();
            for (int i = 0; i < _allTraits.Count; i++) {
                TRAIT currTrait = _allTraits[i];
                Trait trait = CharacterManager.Instance.CreateNewTraitForCharacter(currTrait, this);
                if (trait != null) {
					AddTrait (trait);
                }
            }
        }
		public void AddTrait(Trait trait){
			trait.AssignCharacter(this);
			_traits.Add(trait);
			AddTraitBonuses (trait);
		}
		public void RemoveTrait(Trait trait){
			trait.AssignCharacter(null);
			_traits.Remove(trait);
			RemoveTraitBonuses (trait);
		}
        private TRAIT GenerateCharismaTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.CHARISMATIC; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.REPULSIVE; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateIntelligenceTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.SMART; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.DUMB; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateEfficiencyTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.EFFICIENT; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.INEPT; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateMilitaryTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.HOSTILE; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.PACIFIST; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateHealthTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.ROBUST; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.FRAGILE; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateStrengthTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.STRONG; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.WEAK; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        private TRAIT GenerateAgilityTrait() {
            int chance = UnityEngine.Random.Range(0, 100);
            if (chance < 20) {
                return TRAIT.AGILE; //20%
            } else if (chance >= 20 && chance < 40) {
                return TRAIT.CLUMSY; //20%
            } else {
                return TRAIT.NONE;
            }
        }
        public bool HasTrait(TRAIT trait) {
			for (int i = 0; i < _traits.Count; i++) {
				if(_traits[i].trait == trait) {
					return true;
				}
			}
			return false;
		}
		private void AddTraitBonuses(Trait trait){
			_statsModifierPercentage.intPercentage += trait.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage += trait.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage += trait.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage += trait.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
		private void RemoveTraitBonuses(Trait trait){
			_statsModifierPercentage.intPercentage -= trait.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage -= trait.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage -= trait.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage -= trait.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
        #endregion

		#region Character Tags
		public void AssignTag(CHARACTER_TAG tag) {
			CharacterTag charTag = null;
			switch (tag) {
			case CHARACTER_TAG.ANCIENT_KNOWLEDGE:
				charTag = new AncientKnowledge (this);
				break;
			case CHARACTER_TAG.CRIMINAL:
				charTag = new Criminal (this);
				break;
			case CHARACTER_TAG.HYPNOTIZED:
				charTag = new Hypnotized (this);
				break;
			case CHARACTER_TAG.SAPIENT:
				charTag = new Sapient (this);
				break;
			case CHARACTER_TAG.TERMINALLY_ILL:
				charTag = new TerminallyIll (this);
				break;
			case CHARACTER_TAG.VAMPIRE:
				charTag = new Vampire (this);
				break;
			case CHARACTER_TAG.LOST_HEIR:
				charTag = new LostHeir (this);
				break;
			}
			if(charTag != null){
				AddCharacterTag (charTag);
			}
		}
		private void GenerateRaceTags(){
			for (int i = 0; i < _raceSetting.tags.Count; i++) {
				AssignTag (_raceSetting.tags [i]);
			}
		}
		public void AddCharacterTag(CharacterTag tag){
			_tags.Add(tag);
			AddCharacterTagBonuses (tag);
			tag.Initialize ();
		}
		public void RemoveCharacterTag(CharacterTag tag){
			_tags.Remove(tag);
			RemoveCharacterTagBonuses (tag);
		}
		private void AddCharacterTagBonuses(CharacterTag tag){
			_statsModifierPercentage.intPercentage += tag.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage += tag.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage += tag.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage += tag.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
		private void RemoveCharacterTagBonuses(CharacterTag tag){
			_statsModifierPercentage.intPercentage -= tag.statsModifierPercentage.intPercentage;
			_statsModifierPercentage.strPercentage -= tag.statsModifierPercentage.strPercentage;
			_statsModifierPercentage.agiPercentage -= tag.statsModifierPercentage.agiPercentage;
			_statsModifierPercentage.hpPercentage -= tag.statsModifierPercentage.hpPercentage;
			RecomputeMaxHP ();
		}
		public bool HasTag(CHARACTER_TAG tag) {
			for (int i = 0; i < _tags.Count; i++) {
				if(_tags[i].tagType == tag) {
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
        /*
         Create a new Party with this character as the leader.
             */
        public Party CreateNewParty() {
            Party newParty = new Party(this);
            return newParty;
        }
		public void SetParty(Party party) {
			_party = party;
		}
		#endregion

		#region Location
        public void SetSpecificLocation(ILocation specificLocation) {
            _specificLocation = specificLocation;
        }
		#endregion

		#region Quests
        /*
         Determine what action the character will do, and execute that action.
             */
		internal void DetermineAction() {
            //Set Task of character to do nothing for now
			if(_role != null){
				if(_role.defaultRoleTask != null){
					_role.defaultRoleTask.OnChooseTask (this);
				}
			}
            return;

			if(isInCombat){
				SetCurrentFunction (() => DetermineAction ());
				return;
			}
			if(_isFainted || _isPrisoner || _isDead || _isFollower){
				return;
			}
            if(_party != null) {
                //if the character is in a party, and is not the leader, do not decide any action
                if (!_party.IsCharacterLeaderOfParty(this)) {
                    return;
                }
            }
			if(_currentTask != null && !_currentTask.isDone){
				_currentTask.SetIsHalted (true);
			}
            if(nextTaskToDo != null) {
                //Force accept quest, if any
				nextTaskToDo.OnChooseTask(this);
//                nextTaskToDo.PerformTask();
                nextTaskToDo = null;
                return;
            }
			WeightedDictionary<CharacterTask> actionWeights = new WeightedDictionary<CharacterTask> ();
			if(_role != null){
				_role.AddTaskWeightsFromRole (actionWeights);
			}
			for (int i = 0; i < _tags.Count; i++) {
				_tags [i].AddTaskWeightsFromTags (actionWeights);
			}
			//TODO: Quest Tasks

			CharacterTask chosenTask = actionWeights.PickRandomElementGivenWeights ();
			chosenTask.ResetTask ();
			chosenTask.OnChooseTask (this);
//			chosenTask.PerformTask ();

//			WeightedDictionary<CharacterTask> actionWeights = _role.GetActionWeights();
//            AddActionWeightsFromTags(actionWeights); //Add weights from tags
//            if (actionWeights.GetTotalOfWeights () > 0) {
//				CharacterTask chosenAction = actionWeights.PickRandomElementGivenWeights();
//                if (chosenAction.taskType == TASK_TYPE.QUEST) {
//                    Debug.Log(this.name + " decides to " + ((OldQuest.Quest)chosenAction).questType.ToString() + " on " + Utilities.GetDateString(GameManager.Instance.Today()));
//                } else {
//                    Debug.Log(this.name + " decides to " + chosenAction.taskType.ToString() + " on " + Utilities.GetDateString(GameManager.Instance.Today()));
//                }
//                chosenAction.PerformTask(this);
//            } else {
//                throw new Exception(this.name + " could not decide action because weights are zero!");
//            }

		}
        /*
         Set a task that this character will accept next
             */
        internal void SetTaskToDoNext(CharacterTask taskToDo) {
            nextTaskToDo = taskToDo;
        }
//        private WeightedDictionary<CharacterTask> GetUnalignedActionWeights() {
//            WeightedDictionary<CharacterTask> actionWeights = new WeightedDictionary<CharacterTask>();
//
//            Rest restTask = new Rest(this);
//            actionWeights.AddElement(restTask, GetUnalignedRestWeight());
//
//            if(_lair != null) {
//                GoHome goHomeTask = new GoHome(this);
//                actionWeights.AddElement(goHomeTask, GetUnalignedGoHomeWeight());
//            }
//
//            DoNothing doNothingTask = new DoNothing(this);
//            actionWeights.AddElement(doNothingTask, GetUnalignedDoNothingWeight());
//
//            return actionWeights;
//        }
//        
//        private int GetUnalignedRestWeight() {
//            if (currentHP < maxHP) {
//                int percentMissing = (int)(100f - (remainingHP * 100));
//                if (percentMissing >= 50) {
//                    return 100; //+100 if HP is below 50%
//                } else {
//                    return 5 * percentMissing; //5 Weight per % of HP below max HP, 
//                }
//            }
//            return 0;
//        }
//        private int GetUnalignedDoNothingWeight() {
//            return 300;
//        }
//        private int GetUnalignedGoHomeWeight() {
//            return 50;
//        }
//
//        private void AddTaskWeightsFromQuest(WeightedDictionary<CharacterTask> tasks) {
//            if (_currentQuest != null) {
//                CharacterTask currentTaskForQuest = _currentQuest.GetCurrentTaskOfQuest();
//                tasks.AddElement(currentTaskForQuest, currentTaskForQuest.GetTaskWeight(this));
//            }
//        }
//
//        private WeightedDictionary<CharacterTask> GetTaskWeights() {
//            WeightedDictionary<CharacterTask> taskWeights = new WeightedDictionary<CharacterTask>();
//            if (_role != null) {
//                _role.AddTaskWeightsFromRole(taskWeights);
//            }
//            AddTaskWeightsFromTags(taskWeights);
//            AddTaskWeightsFromQuest(taskWeights);
//            return taskWeights;
//        }
        #endregion

        #region Tags
        /*
         Add tag specific actions to action weights
             */
//        private void AddTaskWeightsFromTags(WeightedDictionary<CharacterTask> actionWeights) {
//            for (int i = 0; i < _raceSetting.tags.Count; i++) {
//                CHARACTER_TAG currTag = _raceSetting.tags[i];
//                switch (currTag) {
//                    case CHARACTER_TAG.PREDATOR:
//                        AddHuntPreyWeights(actionWeights);
//                        break;
//                    case CHARACTER_TAG.HIBERNATES:
//                        AddHibernateWeights(actionWeights);
//                        break;
//                    case CHARACTER_TAG.PILLAGER:
//                        AddPillageWeights(actionWeights);
//                        break;
//                    default:
//                        break;
//                }
//            }
//        }
//        private void AddHuntPreyWeights(WeightedDictionary<CharacterTask> actionWeights) {
//            Region mainRegion = currLocation.region;
//            List<Region> elligibleRegions = new List<Region>(mainRegion.adjacentRegionsViaMajorRoad);
//            elligibleRegions.Add(mainRegion);
//            for (int i = 0; i < elligibleRegions.Count; i++) {
//                Region currRegion = elligibleRegions[i];
//                List<BaseLandmark> allLandmarksInRegion = new List<BaseLandmark>(currRegion.landmarks);
//                allLandmarksInRegion.Add(currRegion.mainLandmark);
//                for (int j = 0; j < allLandmarksInRegion.Count; j++) {
//                    BaseLandmark currLandmark = allLandmarksInRegion[j];
//                    int weight = 0;
//                    if (currLandmark.civilians > 0) {
//                        weight += 5 * currLandmark.civilians; //+5 Weight per Civilian in that landmark
//                        weight -= 40 * currLandmark.charactersAtLocation.Count;//-40 Weight per character in that landmark.
//                        if (weight > 0) {
//                            actionWeights.AddElement(new HuntPrey(this, currLandmark), weight);
//                        }
//                    }
//                }
//            }
//        }
//        private void AddHibernateWeights(WeightedDictionary<CharacterTask> actionWeights) {
//            if (lair != null) {
//                actionWeights.AddElement(new Hibernate(this), 5); //Hibernate - 5, 0 if the monster does not have a Lair
//            }
//        }
//        private void AddPillageWeights(WeightedDictionary<CharacterTask> actionWeights) {
//            Region mainRegion = currLocation.region;
//            List<Region> elligibleRegions = new List<Region>(mainRegion.adjacentRegionsViaMajorRoad);
//            elligibleRegions.Add(mainRegion);
//            for (int i = 0; i < elligibleRegions.Count; i++) {
//                Region currRegion = elligibleRegions[i];
//                List<BaseLandmark> allLandmarksInRegion = new List<BaseLandmark>(currRegion.landmarks);
//                allLandmarksInRegion.Add(currRegion.mainLandmark);
//                for (int j = 0; j < allLandmarksInRegion.Count; j++) {
//                    BaseLandmark currLandmark = allLandmarksInRegion[j];
//                    int weight = 0;
//                    //int totalMaterials = currLandmark.materialsInventory.Sum(x => x.Value.count);
//                    //weight += totalMaterials / 20; //+1 Weight per 20 resource in the landmark (regardless of value).
//                    weight -= 40 * currLandmark.charactersAtLocation.Count;//-40 Weight per character in that landmark.
//                    if (weight > 0) {
//                        actionWeights.AddElement(new Pillage(this, currLandmark), weight);
//                    }
//                }
//
//            }
//        }
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
        public bool IsHealthFull() {
            return _currentHP >= _maxHP;
        }
		#endregion

		#region Avatar
		public void CreateNewAvatar() {
			//TODO: Only create one avatar per character, then enable disable it based on need, rather than destroying it then creating a new avatar when needed
            if(this._role != null) {
                if (this._role.roleType == CHARACTER_ROLE.COLONIST) {
                    //				GameObject avatarGO = (GameObject)GameObject.Instantiate (ObjectPoolManager.Instance.otherPrefabs [2], this.currLocation.transform.position, Quaternion.identity);
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("ColonistAvatar", this.currLocation.transform.position, Quaternion.identity);
                    ColonistAvatar avatar = avatarGO.GetComponent<ColonistAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                } else if (this._role.roleType == CHARACTER_ROLE.WARLORD) {
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("WarlordAvatar", this.currLocation.transform.position, Quaternion.identity);
                    WarlordAvatar avatar = avatarGO.GetComponent<WarlordAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                } else if (this._role.roleType == CHARACTER_ROLE.HERO) {
                    //				GameObject avatarGO = (GameObject)GameObject.Instantiate (ObjectPoolManager.Instance.otherPrefabs [2], this.currLocation.transform.position, Quaternion.identity);
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("HeroAvatar", this.currLocation.transform.position, Quaternion.identity);
                    HeroAvatar avatar = avatarGO.GetComponent<HeroAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                } else {
                    GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterAvatar", this.currLocation.transform.position, Quaternion.identity);
                    CharacterAvatar avatar = avatarGO.GetComponent<CharacterAvatar>();
                    if (party != null) {
                        avatar.Init(party);
                    } else {
                        avatar.Init(this);
                    }
                }
            } else {
                GameObject avatarGO = ObjectPoolManager.Instance.InstantiateObjectFromPool("CharacterAvatar", this.currLocation.transform.position, Quaternion.identity);
                CharacterAvatar avatar = avatarGO.GetComponent<CharacterAvatar>();
                if (party != null) {
                    avatar.Init(party);
                } else {
                    avatar.Init(this);
                }
            }
			
        }
		public void SetAvatar(CharacterAvatar avatar) {
			_avatar = avatar;
		}
		public void DestroyAvatar() {
			if(_avatar != null) {
				_avatar.DestroyObject();
                SetAvatar(null);
            }
        }
        public void GoToNearestNonHostileSettlement(Action onReachSettlement) {
			if(isInCombat){
				SetCurrentFunction (() => GoToNearestNonHostileSettlement (() => onReachSettlement()));
				return;
			}
            //check first if the character is already at a non hostile settlement
            if(this.currLocation.landmarkOnTile != null && this.currLocation.landmarkOnTile.specificLandmarkType == LANDMARK_TYPE.CITY
                && this.currLocation.landmarkOnTile.owner != null) {
                if(this.faction.id != this.currLocation.landmarkOnTile.owner.id) {
                    if(FactionManager.Instance.GetRelationshipBetween(this.faction, this.currLocation.landmarkOnTile.owner).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                        onReachSettlement();
                        return;
                    }
                } else {
                    onReachSettlement();
                    return;
                }
            }
            //character is not on a non hostile settlement
            List<Settlement> allSettlements = new List<Settlement>();
            for (int i = 0; i < FactionManager.Instance.allTribes.Count; i++) { //Get all the occupied settlements
                Tribe currTribe = FactionManager.Instance.allTribes[i];
                if(this.faction.id == currTribe.id ||
                    FactionManager.Instance.GetRelationshipBetween(this.faction, currTribe).relationshipStatus != RELATIONSHIP_STATUS.HOSTILE) {
                    allSettlements.AddRange(currTribe.settlements);
                }
            }
            allSettlements = allSettlements.OrderBy(x => Vector2.Distance(this.currLocation.transform.position, x.location.transform.position)).ToList();
            if(_avatar == null) {
                CreateNewAvatar();
            }
            _avatar.SetTarget(allSettlements[0].location);
            _avatar.StartPath(PATHFINDING_MODE.USE_ROADS, () => onReachSettlement());
        }
        /*
         This is the default action to be done when a 
         character returns to a non hostile settlement after a quest.
             */
        internal void OnReachNonHostileSettlementAfterQuest() {
            //if (_party != null) {
            //    _party.CheckLeavePartyAfterQuest();
            //}
            //_currLocation.AddCharacterOnTile(this);
            DestroyAvatar();
            DetermineAction();
        }
		internal void GoToLocation(ILocation targetLocation, PATHFINDING_MODE pathfindingMode, Action doneAction = null){
			if (currLocation.id == targetLocation.tileLocation.id) {
				//action doer is already at the target location
				if(doneAction != null){
					doneAction ();
				}
			} else {
				if (_avatar == null) {
					//Instantiate a new character avatar
					CreateNewAvatar();
				}
				_avatar.SetTarget(targetLocation, true);
				if(doneAction == null){
					_avatar.StartPath(pathfindingMode);
				}else{
					_avatar.StartPath(pathfindingMode, () => doneAction());
				}
			}
		}
		#endregion

		#region Task Management
        public void SetCurrentQuest(Quest currentQuest) {
            _questData.SetActiveQuest(currentQuest);
            _questData.SetQuestPhase(0);
        }
		public void AddNewQuest(OldQuest.Quest quest) {
			if (!_activeQuests.Contains(quest)) {
				_activeQuests.Add(quest);
			}
		}
		public void RemoveQuest(OldQuest.Quest quest) {
			_activeQuests.Remove(quest);
		}
		public void SetCurrentTask(CharacterTask currentTask) {
			_currentTask = currentTask;
		}
		public List<OldQuest.Quest> GetQuestsOfType(QUEST_TYPE questType) {
			List<OldQuest.Quest> quests = new List<OldQuest.Quest>();
			for (int i = 0; i < _activeQuests.Count; i++) {
				OldQuest.Quest currQuest = _activeQuests[i];
				if(currQuest.questType == questType) {
					quests.Add(currQuest);
				}
			}
			return quests;
		}
		public List<CharacterTask> GetAllPossibleTasks(ILocation location){
			List<CharacterTask> possibleTasks = new List<CharacterTask> ();
			if(_role != null){
				for (int i = 0; i < _role.roleTasks.Count; i++) {
					CharacterTask currentTask = _role.roleTasks [i];
					if(currentTask.CanBeDone(this, location)){
						possibleTasks.Add (currentTask);
					}
				}
			}
			for (int i = 0; i < _tags.Count; i++) {
				for (int j = 0; j < _tags[i].tagTasks.Count; j++) {
					CharacterTask currentTask = _tags[i].tagTasks[j];
					if(currentTask.CanBeDone(this, location)){
						possibleTasks.Add (currentTask);
					}
				}
			}
			//TODO: Tag and Quest Tasks

			return possibleTasks;
		}
        #endregion

        #region Utilities
        public Character GetFollowerByID(int id) {
            if (party != null) {
                for (int i = 0; i < party.partyMembers.Count; i++) {
                    Character currChar = party.partyMembers[i];
                    if (currChar.id == id) {
                        return currChar;
                    }
                }
            }
            return null;
        }
        public void SetFollowerState(bool state) {
            _isFollower = state;
        }
		public void SetHome(BaseLandmark newHome) {
            this._home = newHome;
        }
        public void SetLair(BaseLandmark newLair) {
            this._lair = newLair;
        }
        public bool HasPathToParty(Party partyToJoin) {
            return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.USE_ROADS_FACTION_RELATIONSHIP, _faction) != null;
        }
        public Settlement GetNearestNonHostileSettlement() {
			if(faction != null){
				List<Faction> nonHostileFactions = faction.GetMajorFactionsWithRelationshipStatus
					(new List<RELATIONSHIP_STATUS>() { RELATIONSHIP_STATUS.FRIENDLY, RELATIONSHIP_STATUS.NEUTRAL });

				List<Settlement> settlements = new List<Settlement>();
				nonHostileFactions.ForEach(x => settlements.AddRange(x.settlements));
				settlements.AddRange(_faction.settlements); //Add the settlements of the faction that this character belongs to

				settlements.OrderByDescending(x => currLocation.GetDistanceTo(x.location));

				return settlements.First();
			}
			return null;
        }
        public Settlement GetNearestSettlementFromFaction() {
            if(this.faction != null) {
                List<Settlement> factionSettlements = new List<Settlement>(faction.settlements);
                if (factionSettlements.Count > 0) {
                    factionSettlements.OrderBy(x => this.currLocation.GetDistanceTo(x.location)).ToList();
                    return factionSettlements[0];
                }
            }
            return null;
        }
        public BaseLandmark GetNearestLandmarkWithoutHostiles() {
            Region currRegionLocation = specificLocation.tileLocation.region;
            List<BaseLandmark> elligibleLandmarks = new List<BaseLandmark>();
            elligibleLandmarks.Add(currRegionLocation.mainLandmark);
            elligibleLandmarks.AddRange(currRegionLocation.landmarks);
            if (specificLocation is BaseLandmark) {
                elligibleLandmarks.Remove(specificLocation as BaseLandmark);
            }
            Dictionary<BaseLandmark, List<HexTile>> landmarksWithoutHostiles = new Dictionary<BaseLandmark, List<HexTile>>();
            Dictionary<BaseLandmark, List<HexTile>> landmarksWithHostiles = new Dictionary<BaseLandmark, List<HexTile>>();
            for (int i = 0; i < elligibleLandmarks.Count; i++) {
                BaseLandmark currLandmark = elligibleLandmarks[i];
                List<HexTile> path = PathGenerator.Instance.GetPath(specificLocation.tileLocation, currLandmark.location, PATHFINDING_MODE.USE_ROADS);
                if(path != null) {
                    //check for hostiles
                    if (!currLandmark.HasHostilitiesWith(this.faction)) {
                        if (landmarksWithoutHostiles.ContainsKey(currLandmark)) {
                            landmarksWithoutHostiles.Add(currLandmark, path);
                        }
                    } else {
                        if (landmarksWithHostiles.ContainsKey(currLandmark)) {
                            landmarksWithHostiles.Add(currLandmark, path);
                        }
                    }
                }
            }

            if (landmarksWithoutHostiles.Count > 0) {
                landmarksWithoutHostiles.OrderBy(x => x.Value.Count);
                return landmarksWithoutHostiles.Keys.First();
            } else {
                if (landmarksWithHostiles.Count > 0) {
                    landmarksWithHostiles.OrderBy(x => x.Value.Count);
                    return landmarksWithHostiles.Keys.First();
                }
            }

            return null;
        }
        #endregion

        #region Relationships
        public void AddNewRelationship(Character relWith, Relationship relationship) {
            if (!_relationships.ContainsKey(relWith)) {
                _relationships.Add(relWith, relationship);
            } else {
                throw new Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
            }
        }
        public void RemoveRelationshipWith(Character relWith) {
            if (_relationships.ContainsKey(relWith)) {
                _relationships.Remove(relWith);
            }
        }
        public Relationship GetRelationshipWith(ECS.Character character) {
            if (_relationships.ContainsKey(character)) {
                return _relationships[character];
            }
            return null;
        }
        #endregion

		#region History
		internal void AddHistory(string text, object obj = null){
			GameDate today = GameManager.Instance.Today ();
			string date = "[" + ((MONTH)today.month).ToString() + " " + today.day + ", " + today.year + "]";
			if(obj != null){
				if(obj is CombatPrototype){
					CombatPrototype combat = (CombatPrototype)obj;
					if(this.combatHistory.Count > 20){
						this.combatHistory.Remove (0);
					}
					_combatHistoryID += 1;
					combatHistory.Add (_combatHistoryID, combat);
					string combatText = "[url=" + _combatHistoryID.ToString() + "_combat]" + text + "[/url]";
					text = combatText;
				}
			}
			this._history.Insert (0, date + " " + text);
			if(this._history.Count > 20){
				this._history.RemoveAt (this._history.Count - 1);
			}
		}
		#endregion

		#region Prisoner
		internal void SetPrisoner(bool state, object prisonerOf){
			_isPrisoner = state;
			_isPrisonerOf = prisonerOf;
			if(state){
				if(this.specificLocation != null){
					this.specificLocation.RemoveCharacterFromLocation (this);
				}
				string wardenName = string.Empty;
				if(_isPrisonerOf is Party){
					wardenName = ((Party)_isPrisonerOf).name;
				}else if(_isPrisonerOf is ECS.Character){
					wardenName = ((ECS.Character)_isPrisonerOf).name;
				}else if(_isPrisonerOf is BaseLandmark){
					wardenName = ((BaseLandmark)_isPrisonerOf).landmarkName;
				}
				AddHistory ("Became a prisoner of " + wardenName + ".");
				Unfaint ();
			}
		}
		internal void AddPrisoner(ECS.Character character){
			if (this._party != null) {
				this._party.AddPrisoner(character);
			}else{
				character.SetPrisoner (true, this);
				_prisoners.Add (character);
			}
		}
		internal void RemovePrisoner(ECS.Character character){
			_prisoners.Remove (character);
		}
		internal void ReleasePrisoner(){
			string wardenName = string.Empty;
            ILocation location = null;
            if (_isPrisonerOf is Party) {
                Party prisonerOf = _isPrisonerOf as Party;
                wardenName = prisonerOf.name;
                prisonerOf.RemovePrisoner(this);
                location = prisonerOf.specificLocation;
            } else if (_isPrisonerOf is ECS.Character) {
                ECS.Character prisonerOf = _isPrisonerOf as ECS.Character;
                wardenName = prisonerOf.name;
                prisonerOf.RemovePrisoner(this);
                location = prisonerOf.specificLocation;
            } else if (_isPrisonerOf is BaseLandmark) {
                BaseLandmark prisonerOf = _isPrisonerOf as BaseLandmark;
                wardenName = prisonerOf.landmarkName;
                prisonerOf.RemovePrisoner(this);
                location = prisonerOf;
            }

            SetPrisoner(false, null);

            AddHistory("Released from the prison of " + wardenName + ".");
            if (this.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                BaseLandmark landmark = (BaseLandmark)this.specificLocation;
                landmark.AddHistory("Prisoner " + this.name + " is released.");
            }

            //When this character is released from imprisonment
            //Check if this character is a follower
            if (this.isFollower) {
                //if this character is
                if (this.faction != null) {
                    //Set this character as a civilian of the nearest settlement of his/her faction
                    Settlement settlement = GetNearestSettlementFromFaction();
                    settlement.AdjustCivilians(this.raceSetting.race, 1);
                }
            } else {
                //if not, make this character decide for itself again
                location.AddCharacterToLocation(this);
                DetermineAction();
            }
		}
		internal void TransferPrisoner(object newPrisonerOf){
			//Remove from previous prison
			if(_isPrisonerOf is Party){
				((Party)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is ECS.Character){
				((ECS.Character)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is BaseLandmark){
				((BaseLandmark)_isPrisonerOf).RemovePrisoner (this);
			}

			//Add prisoner to new prison
			if(newPrisonerOf is Party){
				((Party)newPrisonerOf).AddPrisoner (this);
			}else if(newPrisonerOf is ECS.Character){
				((ECS.Character)newPrisonerOf).AddPrisoner (this);
			}else if(newPrisonerOf is BaseLandmark){
				((BaseLandmark)newPrisonerOf).AddPrisoner (this);
			}
		}
		private void PrisonerDeath(){
			if(_isPrisonerOf is Party){
				((Party)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is ECS.Character){
				((ECS.Character)_isPrisonerOf).RemovePrisoner (this);
			}else if(_isPrisonerOf is BaseLandmark){
				((BaseLandmark)_isPrisonerOf).RemovePrisoner (this);
			}
			SetPrisoner (false, null);
		}
		public void ConvertToFaction(){
			BaseLandmark prison = (BaseLandmark)_isPrisonerOf;
			Faction faction = prison.owner;
			SetFaction (faction);
			SetHome (prison);
			prison.AddCharacterToLocation(this, false);
			prison.AddCharacterHomeOnLandmark(this);
			ChangeRole ();
			prison.owner.AddNewCharacter(this);
			AddHistory (this.name + " joined " + faction.name + " as " + this.role.roleType.ToString () + ".");
			prison.AddHistory (this.name + " joined " + faction.name + " as " + this.role.roleType.ToString () + ".");
		}
		#endregion

		#region ICombatInitializer
		//public bool InitializeCombat(){
		//	if(_role != null && _role.roleType == CHARACTER_ROLE.WARLORD){
		//		//Start Combat with hostile or unaligned
		//		ICombatInitializer enemy = this.specificLocation.GetCombatEnemy (this);
		//		if(enemy != null){
		//			ECS.CombatPrototype combat = new ECS.CombatPrototype (this, enemy, this.specificLocation);
		//			combat.AddCharacters (ECS.SIDES.A, new List<ECS.Character>(){this});
		//			if(enemy is Party){
		//				combat.AddCharacters (ECS.SIDES.B, ((Party)enemy).partyMembers);
		//			}else{
		//				combat.AddCharacters (ECS.SIDES.B, new List<ECS.Character>(){((ECS.Character)enemy)});
		//			}
		//			this.specificLocation.SetCurrentCombat (combat);
		//			CombatThreadPool.Instance.AddToThreadPool (combat);
		//			return true;
		//		}
		//		return false;
		//	}else{
		//		return false;
		//	}
		//}
		public bool IsHostileWith(ICombatInitializer combatInitializer){
            if (this.faction == null) {
                return true; //this character has no faction
            }
            //Check here if the combatInitializer is hostile with this character, if yes, return true
            Faction factionOfEnemy = null;
            if(combatInitializer is ECS.Character) {
                factionOfEnemy = (combatInitializer as ECS.Character).faction;
            }else if(combatInitializer is Party) {
                factionOfEnemy = (combatInitializer as Party).faction;
            }
            if(factionOfEnemy != null) {
                if(factionOfEnemy.id == this.faction.id) {
                    return false; //characters are of same faction
                }
                FactionRelationship rel = this.faction.GetRelationshipWith(factionOfEnemy);
                if(rel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE) {
                    return true; //factions of combatants are hostile
                }
                return false;
            } else {
                return true; //enemy has no faction
            }
			
		}
		public void ReturnCombatResults(ECS.CombatPrototype combat){
            this.SetIsInCombat(false);
			if (this.isDefeated) {
				//this character was defeated
				if(_currentTask != null && faction != null) {
					_currentTask.EndTask(TASK_STATUS.CANCEL);
				}
                if (!this.isDead) {
                    BaseLandmark targetLocation = GetNearestLandmarkWithoutHostiles();
                    if (targetLocation == null) {
                        throw new Exception(this.name + " could not find a non hostile location to run to!");
                    } else {
                        GoToLocation(targetLocation, PATHFINDING_MODE.USE_ROADS, () => DetermineAction());
                    }
                }
            } else{
                //this character won the combat, continue his/her current action if any
                if (currentFunction != null) {
                    currentFunction();
                    SetCurrentFunction(null);
                }
     //           else {
					//DetermineAction();
     //           }
			}
        }
		public void SetIsDefeated(bool state){
			_isDefeated = state;
		}
        public void AdjustCivilians(Dictionary<RACE, int> civilians) {
            foreach (KeyValuePair<RACE, int> kvp in civilians) {
                AdjustCivilians(kvp.Key, kvp.Value);
            }
        }
        public void ReduceCivilians(Dictionary<RACE, int> civilians) {
            foreach (KeyValuePair<RACE, int> kvp in civilians) {
                AdjustCivilians(kvp.Key, -kvp.Value);
            }
        }
        public void AdjustCivilians(RACE race, int amount) {
            if (!_civiliansByRace.ContainsKey(race)) {
                _civiliansByRace.Add(race, 0);
            }
            _civiliansByRace[race] += amount;
            _civiliansByRace[race] = Mathf.Max(0, _civiliansByRace[race]);
        }
        public void TransferCivilians(BaseLandmark to, Dictionary<RACE, int> civilians) {
            ReduceCivilians(civilians);
            to.AdjustCivilians(civilians);
        }
        public STANCE GetCurrentStance() {
            if (currentTask != null) {
                return currentTask.stance;
                //if (avatar != null && avatar.isTravelling) {
                //    if (currentTask is Attack || currentTask is Defend || currentTask is Pillage || currentTask is HuntPrey) {
                //        return STANCE.COMBAT;
                //    }
                //    return STANCE.NEUTRAL;
                //}
                //if (currentTask is Attack || currentTask is Defend || currentTask is Pillage || currentTask is HuntPrey) {
                //    return STANCE.COMBAT;
                //} else if (currentTask is Rest || currentTask is Hibernate || (currentTask is OldQuest.Quest && !(currentTask as OldQuest.Quest).isExpired) /*Forming Party*/ || currentTask is DoNothing) {
                //    return STANCE.NEUTRAL;
                //} else if (currentTask is ExploreTile) {
                //    return STANCE.STEALTHY;
                //}
            }
            return STANCE.NEUTRAL;
        }
        public void ContinueDailyAction() {
            if (!isInCombat) {
                if (currentTask != null) {
                    if (avatar != null && avatar.isTravelling) {
                        return;
                    }
                    currentTask.PerformTask();
                }
            }
        }
        #endregion

        #region Materials
        //private void ConstructMaterialInventory() {
        //    _materialInventory = new Dictionary<MATERIAL, int>();
        //    MATERIAL[] allMaterials = Utilities.GetEnumValues<MATERIAL>();
        //    for (int i = 0; i < allMaterials.Length; i++) {
        //        MATERIAL currMat = allMaterials[i];
        //        if(currMat != MATERIAL.NONE) {
        //            _materialInventory.Add(currMat, 0);
        //        }
        //    }
        //}
        //public void AdjustMaterial(MATERIAL material, int amount) {
        //    int newAmount = _materialInventory[material] + amount;
        //    newAmount = Mathf.Max(0, newAmount);
        //    _materialInventory[material] = newAmount;
        //}
        ///*
        // Transfer materials from this character to
        // another character.
        //     */
        //public void TransferMaterials(ECS.Character transferTo, MATERIAL material, int amount) {
        //    AdjustMaterial(material, -amount);
        //    transferTo.AdjustMaterial(material, amount);
        //}
        ///*
        // Transfer materials from this character
        // to a party
        //     */
        //public void TransferMaterials(Party party, MATERIAL material, int amount) {
        //    AdjustMaterial(material, -amount);
        //    party.AdjustMaterial(material, amount);
        //}
        ///*
        // Transfer ALL materials from this character to
        // another character.
        //     */
        //public void TransferMaterials(ECS.Character transferTo) {
        //    foreach (KeyValuePair<MATERIAL, int> kvp in _materialInventory) {
        //        MATERIAL currMat = kvp.Key;
        //        int amount = kvp.Value;
        //        AdjustMaterial(currMat, -amount);
        //        transferTo.AdjustMaterial(currMat, amount);
        //    }
        //}
        ///*
        // Transfer ALL materials from this character
        // to a party
        //     */
        //public void TransferMaterials(Party party) {
        //    foreach (KeyValuePair<MATERIAL, int> kvp in _materialInventory) {
        //        MATERIAL currMat = kvp.Key;
        //        int amount = kvp.Value;
        //        AdjustMaterial(currMat, -amount);
        //        party.AdjustMaterial(currMat, amount);
        //    }
        //}

		public ECS.Item GetItemByName(string itemName){
			if(_equippedItems.Count > 0){
				for (int i = 0; i < _equippedItems.Count; i++) {
					if(_equippedItems[i].itemName == itemName){
						return _equippedItems [i];
					}
				}
			}
			if(_inventory.Count > 0){
				for (int i = 0; i < _inventory.Count; i++) {
					if(_inventory[i].itemName == itemName){
						return _inventory [i];
					}
				}
			}
			return null;
		}
		public ECS.Item GetEquippedItemByName(string itemName){
			if(_equippedItems.Count > 0){
				for (int i = 0; i < _equippedItems.Count; i++) {
					if(_equippedItems[i].itemName == itemName){
						return _equippedItems [i];
					}
				}
			}
			return null;
		}
        #endregion

		#region Combat Handlers
		public void SetIsInCombat (bool state){
			_isInCombat = state;
		}
		public void SetCurrentFunction (Action function){
			if(_party != null){
				_party.SetCurrentFunction (() => function ());
			}else{
				_currentFunction = function;
			}
		}
		#endregion

		#region Landmarks
		public void AddExploredLandmark(BaseLandmark landmark){
			if(!_exploredLandmarks.ContainsKey(landmark.id)){
				_exploredLandmarks.Add (landmark.id, landmark);
			}
		}
		#endregion

		#region Followers
		public void AddFollower(ECS.Character character){
			if(!_followers.Contains(character)){
				_followers.Add (character);
				character._isFollowerOf = this;
				character.SetFollowerState (true);
			}
		}
		public void RemoveFollower(ECS.Character character){
			if(_followers.Remove(character)){
				character._isFollowerOf = null;
				character.SetFollowerState (false);
			}
		}
		#endregion
    }
}
