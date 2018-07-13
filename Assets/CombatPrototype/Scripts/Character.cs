using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace ECS {
    public class Character : ICharacter, ILeader {
        public delegate void OnCharacterDeath();
        public OnCharacterDeath onCharacterDeath;

        public delegate void DailyAction();
        public DailyAction onDailyAction;

        private string _name;
        private string _characterColorCode;
        private int _id;
        private int _gold;
        private int _actRate;
        private float _equippedWeaponPower;
        private bool _isDead;
        private bool _isFainted;
        private bool _isInCombat;
        private GENDER _gender;
        private CharacterClass _characterClass;
        private RaceSetting _raceSetting;
        private CharacterRole _role;
        private Faction _faction;
        private CharacterParty _party;
        private Area _home;
        private BaseLandmark _homeLandmark;
        private StructureObj _homeStructure;
        private Region _currentRegion;
        //private CharacterAvatar _avatar;
        //private Combat _currentCombat;
        private Weapon _equippedWeapon;
        private CharacterBattleTracker _battleTracker;
        private CharacterBattleOnlyTracker _battleOnlyTracker;
        private PortraitSettings _portraitSettings;
        private Color _characterColor;
        //[System.NonSerialized] private List<Trait> _traits;
        //private List<TRAIT> _allTraits;
        private List<STATUS_EFFECT> _statusEffects;
        private List<BodyPart> _bodyParts;
        private List<Item> _equippedItems;
        private List<Item> _inventory;
        private List<Skill> _skills;
        private List<CharacterTag> _tags;
        private List<Log> _history;
        private List<CharacterQuestData> _questData;
        private List<BaseLandmark> _exploredLandmarks; //Currently only storing explored landmarks that were explored for the last 6 months
        private List<CharacterAction> _desperateActions;
        private List<CharacterAction> _idleActions;
        private Dictionary<Character, Relationship> _relationships;
        private Dictionary<ELEMENT, float> _elementalWeaknesses;
        private Dictionary<ELEMENT, float> _elementalResistances;
        private Dictionary<Character, List<string>> _traceInfo;
        public Dictionary<int, Combat> combatHistory;

        //Stats
        private SIDES _currentSide;
        private int _currentHP;
        private int _maxHP;
        private int _strength;
        private int _intelligence;
        private int _agility;
        private int _vitality;
        private int _currentRow;
        private int _baseMaxHP;
        private int _baseStrength;
        private int _baseIntelligence;
        private int _baseAgility;
        private int _baseVitality;
        private int _fixedMaxHP;
        private int _bonusStrength;
        private int _bonusIntelligence;
        private int _bonusAgility;
        private int _bonusVitality;
        private int _level;
        private int _experience;
        private int _maxExperience;
        private int _sp;
        private int _maxSP;
        private int _bonusPDef;
        private int _bonusMDef;
        private float _bonusPDefPercent;
        private float _bonusMDefPercent;
        private float _critChance;
        private float _critDamage;

        //private CharacterActionQueue<CharacterAction> _actionQueue;
        //private CharacterAction _currentAction;
        //private ILocation _specificLocation;

        //private bool _isDefeated;
        //private bool _isIdle; //can't do action, needs will not deplete

        //private Action _currentFunction;
        //private ActionData _actionData;

        #region getters / setters
        public string firstName {
            get { return name.Split(' ')[0]; }
        }
        public string name {
            get { return this._name; }
        }
        public string coloredName {
            get { return "<color=#" + this._characterColorCode  + ">" + this._name + "</color>"; }
        }
        public string urlName {
            get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + this._name + "</link>"; }
        }
        public string coloredUrlName {
            get { return "<link=" + '"' + this._id.ToString() + "_character" + '"' + ">" + "<color=#" + this._characterColorCode + ">" + this._name + "</color></link>"; }
        }
        public int id {
            get { return _id; }
        }
        public GENDER gender {
            get { return _gender; }
        }
        //public List<Trait> traits {
        //    get { return _traits; }
        //}
        public List<CharacterTag> tags {
            get { return _tags; }
        }
        public Dictionary<Character, Relationship> relationships {
            get { return _relationships; }
        }
        public CharacterClass characterClass {
            get { return this._characterClass; }
        }
        public RaceSetting raceSetting {
            get { return _raceSetting; }
        }
        public CharacterRole role {
            get { return _role; }
        }
        public Faction faction {
            get { return _faction; }
        }
        //public Faction attackedByFaction {
        //    get { return _party.attackedByFaction; }
        //}
        public NewParty iparty {
            get { return _party; }
        }
        public CharacterParty party {
            get { return _party; }
        }
        public List<CharacterQuestData> questData {
            get { return _questData; }
        }
        public HexTile currLocation {
            get { return (_party.specificLocation != null ? _party.specificLocation.tileLocation : null); }
        }
        public ILocation specificLocation {
            get { return (_party.specificLocation != null ? _party.specificLocation : null); }
        }
        //public Region currentRegion {
        //    get { return _party.currentRegion; }
        //}
        public List<BodyPart> bodyParts {
            get { return this._bodyParts; }
        }
        public List<Item> equippedItems {
            get { return this._equippedItems; }
        }
        public List<Item> inventory {
            get { return this._inventory; }
        }
        public List<Skill> skills {
            get { return this._skills; }
        }
        public int currentRow {
            get { return this._currentRow; }
        }
        public SIDES currentSide {
            get { return this._currentSide; }
        }
        public bool isDead {
            get { return this._isDead; }
        }
        public bool isFainted {
            get { return this._isFainted; }
        }
        public int strength {
            get { return _strength + _bonusStrength; }
        }
        public int baseStrength {
            get { return _baseStrength; }
        }
        public int intelligence {
            get { return _intelligence + _bonusIntelligence; }
        }
        public int baseIntelligence {
            get { return _baseIntelligence; }
        }
        public int agility {
            get { return _agility + _bonusAgility; }
        }
        public int baseAgility {
            get { return _baseAgility; }
        }
        public int vitality {
            get { return _vitality + _bonusVitality; }
        }
        public int baseVitality {
            get { return _baseVitality; }
        }
        public int currentHP {
            get { return this._currentHP; }
        }
        public int maxHP {
            get { return this._maxHP; }
        }
        public int baseMaxHP {
            get { return _baseMaxHP; }
        }
        public int pFinalAttack {
            get {
                float weaponAttack = 0f;
                float str = (float) strength;
                if (_equippedWeapon != null) {
                    weaponAttack = _equippedWeapon.attackPower;
                }
                return (int) (((weaponAttack + str) * (1f + ((float) str / 20f))) * (1f + ((float) level / 100f)));
            }
        }
        public int mFinalAttack {
            get {
                float weaponAttack = 0f;
                float intl = (float) intelligence;
                if (_equippedWeapon != null) {
                    weaponAttack = _equippedWeapon.attackPower;
                }
                return (int) (((weaponAttack + intl) * (1f + ((float) intl / 20f))) * (1f + ((float) level / 100f)));
            }
        }
        public int speed {
            get { return agility + level; }
        }
        public Color characterColor {
            get { return _characterColor; }
        }
        public string characterColorCode {
            get { return _characterColorCode; }
        }
        public Area home {
            get { return _home; }
        }
        public BaseLandmark homeLandmark {
            get { return _homeLandmark; }
        }
        public StructureObj homeStructure {
            get { return _homeStructure; }
        }
        public float remainingHP { //Percentage of remaining HP this character has
            get { return (float) currentHP / (float) maxHP; }
        }
        public int remainingHPPercent {
            get { return (int) (remainingHP * 100); }
        }
        public List<Log> history {
            get { return this._history; }
        }
        public float characterPower {
            get { return (float) _currentHP + (float) ((strength + intelligence + agility) * 2) + _equippedWeaponPower; }
        }
        public float equippedWeaponPower {
            get { return _equippedWeaponPower; }
        }
        public int gold {
            get { return _gold; }
        }
        public List<BaseLandmark> exploredLandmarks {
            get { return _exploredLandmarks; }
        }
        public bool isInCombat {
            get {
                return _isInCombat;
            }
        }
        public bool isFactionless {
            get { return faction == null; }
        }
        public Dictionary<Character, List<string>> traceInfo {
            get { return _traceInfo; }
        }
        //public ActionData actionData {
        //    get { return _actionData; }
        //}
        //public CharacterIcon icon {
        //    get { return _party.icon; }
        //}
        //public bool isIdle {
        //    get { return _isIdle; }
        //}
        public PortraitSettings portraitSettings {
            get { return _portraitSettings; }
        }
        public int level {
            get { return _level; }
        }
        public int currentSP {
            get { return _sp; }
        }
        public int maxSP {
            get { return _maxSP; }
        }
        public int experience {
            get { return _experience; }
        }
        public int maxExperience {
            get { return _maxExperience; }
        }
        public float critChance {
            get { return _critChance; }
        }
        public float critDamage {
            get { return _critDamage; }
        }
        public Dictionary<ELEMENT, float> elementalWeaknesses {
            get { return _elementalWeaknesses; }
        }
        public Dictionary<ELEMENT, float> elementalResistances {
            get { return _elementalResistances; }
        }
        //public Combat currentCombat {
        //    get { return party.currentCombat; }
        //    //set { _currentCombat = value; }
        //}
        public int actRate {
            get { return _actRate; }
            set { _actRate = value; }
        }
        public Weapon equippedWeapon {
            get { return _equippedWeapon; }
        }
        public CharacterBattleTracker battleTracker {
            get { return _battleTracker; }
        }
        public CharacterBattleOnlyTracker battleOnlyTracker {
            get { return _battleOnlyTracker; }
        }
        public float computedPower {
            get { return GetAttackPower() + GetDefensePower(); }
        }
        public ICHARACTER_TYPE icharacterType {
            get { return ICHARACTER_TYPE.CHARACTER; }
        }
        public List<CharacterAction> desperateActions {
            get { return _desperateActions; }
        }
        public List<CharacterAction> idleActions {
            get { return _idleActions; }
        }
        #endregion

        public Character(string className, RACE race, GENDER gender) : this() {
            _id = Utilities.SetID(this);
			_characterClass = CharacterManager.Instance.classesDictionary[className].CreateNewCopy();
			_raceSetting = RaceManager.Instance.racesDictionary[race.ToString()].CreateNewCopy();
            _gender = gender;
            _name = RandomNameGenerator.Instance.GenerateRandomName(_raceSetting.race, _gender);
            _portraitSettings = CharacterManager.Instance.GenerateRandomPortrait(race, gender);
            _skills = GetGeneralSkills();
            _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);

            GenerateRaceTags();

            AllocateStatPoints(10);
            LevelUp();

            CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(className);
            if(setup != null) {
                GenerateSetupTags(setup);
                EquipPreEquippedItems(setup);
                if(setup.optionalRole != CHARACTER_ROLE.NONE) {
                    AssignRole(setup.optionalRole);
                }
            }
        }
        public Character(CharacterSaveData data) : this(){
            _id = Utilities.SetID(this, data.id);
            _characterClass = CharacterManager.Instance.classesDictionary[data.className].CreateNewCopy();
            _raceSetting = RaceManager.Instance.racesDictionary[data.race.ToString()].CreateNewCopy();
            _gender = data.gender;
            _name = data.name;
            //LoadRelationships(data.relationshipsData);
            _portraitSettings = data.portraitSettings;
            _bodyParts = new List<BodyPart>(_raceSetting.bodyParts);
            _skills = GetGeneralSkills();
            //_skills.AddRange (GetBodyPartSkills ());

            //GenerateSetupTags(baseSetup);
            GenerateRaceTags();

            AllocateStatPoints(10);
            LevelUp();

            //EquipPreEquippedItems(baseSetup);
            CharacterSetup setup = CombatManager.Instance.GetBaseCharacterSetup(data.className);
            if (setup != null) {
                GenerateSetupTags(setup);
                EquipPreEquippedItems(setup);
                if (setup.optionalRole != CHARACTER_ROLE.NONE) {
                    AssignRole(setup.optionalRole);
                }
            }
            SubscribeToSignals();
        }
        public Character() {
            _tags = new List<CharacterTag>();
            _exploredLandmarks = new List<BaseLandmark>();
            _statusEffects = new List<STATUS_EFFECT>();
            _tags = new List<CharacterTag>();
            _equippedWeaponPower = 0f;
            _isDead = false;
            _isFainted = false;
            //_isDefeated = false;
            //_isIdle = false;
            _traceInfo = new Dictionary<Character, List<string>>();
            _history = new List<Log>();
            _questData = new List<CharacterQuestData>();
            //_actionQueue = new CharacterActionQueue<CharacterAction>();
            //previousActions = new Dictionary<CharacterTask, string>();
            _relationships = new Dictionary<Character, Relationship>();
            //_actionData = new ActionData(this);


            //RPG
            _strength = 1;
            _intelligence = 1;
            _agility = 1;
            _vitality = 1;
            _level = 0;
            _experience = 0;
            _elementalWeaknesses = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
            _elementalResistances = new Dictionary<ELEMENT, float>(CharacterManager.Instance.elementsChanceDictionary);
            _battleTracker = new CharacterBattleTracker();
            _battleOnlyTracker = new CharacterBattleOnlyTracker();
            _equippedItems = new List<Item>();
            _inventory = new List<Item>();
            combatHistory = new Dictionary<int, Combat>();

            GetRandomCharacterColor();
            ConstructDesperateActions();
            ConstructIdleActions();
            //_combatHistoryID = 0;

            Messenger.AddListener<Region>("RegionDeath", RegionDeath);
            Messenger.AddListener<List<Region>>("RegionPsytoxin", RegionPsytoxin);
            //Messenger.AddListener(Signals.HOUR_ENDED, EverydayAction);
            Messenger.AddListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
            Messenger.AddListener<ECS.Character>(Signals.CHARACTER_REMOVED, RemoveRelationshipWith);
            Messenger.AddListener<ECS.Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);
        }
        public void Initialize() { }

        #region Signals
        private void SubscribeToSignals() {
            Messenger.AddListener<ECS.Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        }
        #endregion

        private void AllocateStatPoints(int statAllocation){
            _baseStrength = 0;
            _baseIntelligence = 0;
            _baseAgility = 0;
            _baseVitality = 0;

			WeightedDictionary<string> statWeights = new WeightedDictionary<string> ();
			statWeights.AddElement ("strength", (int) _characterClass.strWeightAllocation);
			statWeights.AddElement ("intelligence", (int) _characterClass.intWeightAllocation);
			statWeights.AddElement ("agility", (int) _characterClass.agiWeightAllocation);
			statWeights.AddElement ("vitality", (int) _characterClass.vitWeightAllocation);

			if(statWeights.GetTotalOfWeights() > 0){
				string chosenStat = string.Empty;
				for (int i = 0; i < statAllocation; i++) {
					chosenStat = statWeights.PickRandomElementGivenWeights ();
					if (chosenStat == "strength") {
						_baseStrength += 1;
					}else if (chosenStat == "intelligence") {
						_baseIntelligence += 1;
					}else if (chosenStat == "agility") {
						_baseAgility += 1;
					}else if (chosenStat == "vitality") {
						_baseVitality += 1;
					}
				}
			}

			//_baseMaxHP += (int)((float)_baseMaxHP * (_characterClass.hpPercentage / 100f));
			//_baseStrength += (int)((float)_baseStrength * (_characterClass.strPercentage / 100f));
			//_baseAgility += (int)((float)_baseAgility * (_characterClass.agiPercentage / 100f));
			//_baseIntelligence += (int)((float)_baseIntelligence * (_characterClass.intPercentage / 100f));
            
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
		public void EnableDisableSkills(Combat combat){
			bool isAllAttacksInRange = true;
			bool isAttackInRange = false;

            //Body part skills / general skills
			for (int i = 0; i < this._skills.Count; i++) {
				Skill skill = this._skills [i];
				skill.isEnabled = true;

                if(skill.skillRequirements != null) {
                    for (int j = 0; j < skill.skillRequirements.Length; j++) {
                        SkillRequirement skillRequirement = skill.skillRequirements[j];
                        if (!HasAttribute(skillRequirement.attributeRequired, skillRequirement.itemQuantity)) {
                            skill.isEnabled = false;
                            break;
                        }
                    }
                    if (!skill.isEnabled) {
                        continue;
                    }
                }

                if (skill is AttackSkill){
                    AttackSkill attackSkill = skill as AttackSkill;
                    if(attackSkill.spCost > _sp) {
                        skill.isEnabled = false;
                        continue;
                    }
					isAttackInRange = combat.HasTargetInRangeForSkill (skill, this);
					if(!isAttackInRange){
						isAllAttacksInRange = false;
						skill.isEnabled = false;
						continue;
					}
				} else if (skill is FleeSkill) {
                    //					skill.isEnabled = false;
                    //					continue;
                    if (this.currentHP >= (this.maxHP / 2)) {
                        skill.isEnabled = false;
                        continue;
                    }
                }
            }

            //Character class skills
            if(_equippedWeapon != null) {
                for (int i = 0; i < _level; i++) {
                    if(i < _characterClass.skillsPerLevel.Count) {
                        if (_characterClass.skillsPerLevel[i] != null) {
                            for (int j = 0; j < _characterClass.skillsPerLevel[i].Length; j++) {
                                Skill skill = _characterClass.skillsPerLevel[i][j];
                                skill.isEnabled = true;

                                //Check for allowed weapon types
                                if (skill.allowedWeaponTypes != null) {
                                    for (int k = 0; k < skill.allowedWeaponTypes.Length; k++) {
                                        if (!skill.allowedWeaponTypes.Contains(_equippedWeapon.weaponType)) {
                                            skill.isEnabled = false;
                                            continue;
                                        }
                                    }
                                }

                                //for (int k = 0; k < skill.skillRequirements.Length; k++) {
                                //    SkillRequirement skillRequirement = skill.skillRequirements[k];
                                //    if (!HasAttribute(skillRequirement.attributeRequired, skillRequirement.itemQuantity)) {
                                //        skill.isEnabled = false;
                                //        break;
                                //    }
                                //}
                                //if (!skill.isEnabled) {
                                //    continue;
                                //}
                                if (skill is AttackSkill) {
                                    AttackSkill attackSkill = skill as AttackSkill;
                                    if (attackSkill.spCost > _sp) {
                                        skill.isEnabled = false;
                                        continue;
                                    }
                                    isAttackInRange = combat.HasTargetInRangeForSkill(skill, this);
                                    if (!isAttackInRange) {
                                        isAllAttacksInRange = false;
                                        skill.isEnabled = false;
                                        continue;
                                    }
                                }
                            }
                        }
                    } else {
                        break;
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
							if(combat.charactersSideA.Contains(this)){
								for (int j = 0; j < combat.charactersSideB.Count; j++) {
									ICharacter enemy = combat.charactersSideB [j];
									if(enemy.currentRow < this._currentRow){
										hasEnemyOnLeft = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                    ICharacter enemy = combat.charactersSideA [j];
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
                                    ICharacter enemy = combat.charactersSideB [j];
									if(enemy.currentRow > this._currentRow){
										hasEnemyOnRight = true;
										break;
									}
								}
							}else{
								for (int j = 0; j < combat.charactersSideA.Count; j++) {
                                    ICharacter enemy = combat.charactersSideA [j];
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
        public void SetRowNumber(int rowNumber){
			this._currentRow = rowNumber;
		}
		//Changes character's side
		public void SetSide(SIDES side){
			this._currentSide = side;
		}
        //Adjust current HP based on specified paramater, but HP must not go below 0
        public void AdjustHP(int amount){
            int previous = this._currentHP;
			this._currentHP += amount;
			this._currentHP = Mathf.Clamp(this._currentHP, 0, _maxHP);
            if(previous != this._currentHP) {
                if(_role != null) {
                    _role.UpdateSafety();
                }
                if (this._currentHP == 0) {
                    FaintOrDeath();
                }
            }
		}
		internal void SetHP(int amount){
			this._currentHP = amount;
		}
		private string GetFaintOrDeath(){
            return "die";
			//WeightedDictionary<string> faintDieDict = new WeightedDictionary<string> ();
			//int faintWeight = 100;
			//int dieWeight = 50;
			//if(HasTrait(TRAIT.GRITTY)){
			//	faintWeight += 50;
			//}
			//if(HasTrait(TRAIT.ROBUST)){
			//	faintWeight += 50;
			//}
			//if(HasTrait(TRAIT.FRAGILE)){
			//	dieWeight += 50;
			//}
			//faintDieDict.AddElement ("faint", 100);
			//faintDieDict.AddElement ("die", 50);

			//return faintDieDict.PickRandomElementGivenWeights ();
		}
		public void FaintOrDeath(){
			string pickedWeight = GetFaintOrDeath ();
			if(pickedWeight == "faint"){
				if(_party.currentCombat == null){
					Faint ();
				}else{
                    _party.currentCombat.CharacterFainted(this);
                }
			}else if(pickedWeight == "die"){
                _party.currentCombat.CharacterDeath(this);
                Death();
    //            if (this.currentCombat == null){
				//	Death ();
				//}else{
				//	this.currentCombat.CharacterDeath (this);
				//}
			}
		}
		//When character will faint
		internal void Faint(){
			if(!_isFainted){
				_isFainted = true;
				SetHP (1);
                ////Set Task to Fainted
                //Faint faintTask = new Faint(this);
                //faintTask.OnChooseTask(this)
;			}
		}
		internal void Unfaint(){
			if (_isFainted) {
				_isFainted = false;
				SetHP (1);
			}
		}
        public void Imprison() {
            if(_party.icharacters.Count > 1) {
                CreateNewParty();
            }
            if(_party.characterObject.currentState.stateName != "Imprisoned") {
                ObjectState imprisonedState = _party.characterObject.GetState("Imprisoned");
                _party.characterObject.ChangeState(imprisonedState);

                _party.SetIsIdle(true); //this makes the character not do any action, and needs are halted
                //Do other things when imprisoned
            }
        }
		//Character's death
		internal void Death(Character killer = null){
			if(!_isDead){
				_isDead = true;
                
                Messenger.RemoveListener<Region> ("RegionDeath", RegionDeath);
				Messenger.RemoveListener<List<Region>> ("RegionPsytoxin", RegionPsytoxin);
                Messenger.RemoveListener<StructureObj, int>("CiviliansDeath", CiviliansDiedReduceSanity);
                Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, RemoveRelationshipWith);

                CombatManager.Instance.ReturnCharacterColorToPool (_characterColor);

                if (_party.specificLocation == null) {
                    throw new Exception("Specific location of " + this.name + " is null! Please use command /l_character_location_history [Character Name/ID] in console menu to log character's location history. (Use '~' to show console menu)");
                }

				if(_party.specificLocation != null && _party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
                    Log deathLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "death");
                    deathLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                    AddHistory(deathLog);
                    (_party.specificLocation as BaseLandmark).AddHistory(deathLog);
				}
                
                //Drop all Items
                while (_equippedItems.Count > 0) {
					ThrowItem (_equippedItems [0]);
				}
				while (_inventory.Count > 0) {
					ThrowItem (_inventory [0]);
				}
                _party.RemoveCharacter(this);
                //Remove ActionData
                //_actionData.DetachActionData();

                //if(_home != null){
                //                //Remove character home on landmark
                //	_home.RemoveCharacterHomeOnLandmark (this);
                //}

                if (this._faction != null){
					if(this._faction.leader != null && this._faction.leader.id == this.id) {
						//If this character is the leader of a faction, set that factions leader as null
						this._faction.SetLeader(null);
					}
					this._faction.RemoveCharacter(this); //remove this character from it's factions list of characters
				}

                //CheckForInternationalIncident();

                //if (_specificLocation != null) {
                //    _specificLocation.RemoveCharacterFromLocation(this);
                //}
                //if (_avatar != null) {
                //    if (_avatar.mainCharacter.id == this.id) {
                //        DestroyAvatar();
                //    } else {
                //        _avatar.RemoveCharacter(this); //if the character has an avatar, remove it from the list of characters
                //    }
                //}
                //if (_isPrisoner){
                //	PrisonerDeath ();
                //}
                if (_role != null){
					_role.DeathRole ();
				}
				while(_tags.Count > 0){
					RemoveCharacterTag (_tags [0]);
				}
//				if(Messenger.eventTable.ContainsKey("CharacterDeath")){
//					Messenger.Broadcast ("CharacterDeath", this);
//				}
				if(onCharacterDeath != null){
					onCharacterDeath();
				}
                onCharacterDeath = null;
                Messenger.Broadcast(Signals.CHARACTER_DEATH, this);
                if (killer != null) {
                    Messenger.Broadcast(Signals.CHARACTER_KILLED, killer, this);
                }

                //ObjectState deadState = _characterObject.GetState("Dead");
                //_characterObject.ChangeState(deadState);

                //GameObject.Destroy(_icon.gameObject);
                //_icon = null;

                Debug.Log(this.name + " died!");
            }
		}
        internal void AddActionOnDeath(OnCharacterDeath onDeathAction) {
            onCharacterDeath += onDeathAction;
        }
        internal void RemoveActionOnDeath(OnCharacterDeath onDeathAction) {
            onCharacterDeath -= onDeathAction;
        }

		#region Body Parts
		/*
     * Add new body parts here.
     * */
		internal void AddBodyPart(BodyPart bodyPart) {
			bodyParts.Add(bodyPart);
		}
		//internal IBodyPart GetBodyPartForWeapon(Weapon weapon) {
		//	List<IBodyPart> allBodyParts = new List<IBodyPart>();
		//	for (int i = 0; i < bodyParts.Count; i++) {
		//		BodyPart currBodyPart = bodyParts[i];
		//		allBodyParts.Add(currBodyPart);
		//		for (int j = 0; j < currBodyPart.secondaryBodyParts.Count; j++) {
		//			allBodyParts.Add(currBodyPart.secondaryBodyParts[j]);
		//		}
		//	}
		//	for (int i = 0; i < allBodyParts.Count; i++) {
		//		IBodyPart currBodyPart = allBodyParts[i];
		//		bool meetsRequirements = true;
		//		//check if currBodyPart meets the weapons requirements
		//		for (int j = 0; j < ItemManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements.Count; j++) {
		//			IBodyPart.ATTRIBUTE currReq = ItemManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements[j];
		//			if (!currBodyPart.HasUnusedAttribute(currReq)) {
		//				meetsRequirements = false;
		//				break;
		//			}
		//		}
		//		if (meetsRequirements) {
		//			return currBodyPart;
		//		}
		//	}
		//	return null;
		//}
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
            for (int i = 0; i < _level; i++) {
                if (i < _characterClass.skillsPerLevel.Count) {
                    if(_characterClass.skillsPerLevel[i] != null) {
                        for (int j = 0; j < _characterClass.skillsPerLevel[i].Length; j++) {
                            Skill skill = _characterClass.skillsPerLevel[i][j];
                            if (skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.BODY_PART) {
                                return true;
                            }
                        }
                    }
                } else {
                    break;
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
            for (int i = 0; i < _level; i++) {
                if (i < _characterClass.skillsPerLevel.Count) {
                    if (_characterClass.skillsPerLevel[i] != null) {
                        for (int j = 0; j < _characterClass.skillsPerLevel[i].Length; j++) {
                            Skill skill = _characterClass.skillsPerLevel[i][j];
                            if (skill.isEnabled && skill.skillCategory == SKILL_CATEGORY.WEAPON) {
                                return true;
                            }
                        }
                    }
                } else {
                    break;
                }
            }
            return false;
		}
		#endregion

		#region Items
		//If a character picks up an item, it is automatically added to his/her inventory
		internal void PickupItem(Item item){
			Item newItem = item;
			if(item.isUnlimited){
				newItem = item.CreateNewCopy ();
				newItem.isUnlimited = false;
			}
            if (_inventory.Contains(newItem)) {
                throw new Exception(this.name + " already has an instance of " + newItem.itemName);
            }
			this._inventory.Add (newItem);
			//newItem.SetPossessor (this);
			if(newItem.owner == null){
				OwnItem (newItem);
			}
#if !WORLD_CREATION_TOOL
            Log obtainLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "obtain_item");
            obtainLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            obtainLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
            AddHistory(obtainLog);
			if (_party.specificLocation.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                (_party.specificLocation as BaseLandmark).AddHistory(obtainLog);
            }
#endif
            Messenger.Broadcast(Signals.ITEM_OBTAINED, newItem, this);
            newItem.OnItemPutInInventory(this);
        }
		internal void ThrowItem(Item item, bool addInLandmark = true){
			if(item.isEquipped){
				UnequipItem (item);
			}
			//item.SetPossessor (null);
			this._inventory.Remove (item);
			//item.exploreWeight = 15;
			if(addInLandmark){
				ILocation location = _party.specificLocation;
				if(location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
					BaseLandmark landmark = location as BaseLandmark;
					landmark.AddItemInLandmark(item);
				}
			}
            Messenger.Broadcast(Signals.ITEM_THROWN, item, this);
        }
        internal void DropItem(Item item) {
            ThrowItem(item);
            ILocation location = _party.specificLocation;
			if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
				//BaseLandmark landmark = location as BaseLandmark;
                Log dropLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "drop_item");
                dropLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                dropLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
                dropLog.AddToFillers(location, location.locationName, LOG_IDENTIFIER.LANDMARK_1);
                AddHistory(dropLog);
                (location as BaseLandmark).AddHistory(dropLog);
            }

        }
        internal void CheckForItemDrop() {
            ILocation location = _party.specificLocation;
            if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) {
                if (UnityEngine.Random.Range(0, 100) < 3) {
                    Dictionary<Item, Character> itemPool = new Dictionary<Item, Character>();
                    List<Character> charactersToCheck = new List<Character>();
                    charactersToCheck.Add(this);

                    for (int i = 0; i < charactersToCheck.Count; i++) {
                        ECS.Character currCharacter = charactersToCheck[i];
                        for (int j = 0; j < currCharacter.inventory.Count; j++) {
                            itemPool.Add(currCharacter.inventory[j], currCharacter);
                        }
                    }

                    //Drop a random item from the pool
                    if (itemPool.Count > 0) {
                        Item chosenItem = itemPool.Keys.ElementAt(UnityEngine.Random.Range(0, itemPool.Count));
                        Character owner = itemPool[chosenItem];
                        owner.DropItem(chosenItem);
                    }
                }
            }
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
			if(ItemManager.Instance.allItems.ContainsKey(itemName)){
				if (itemType == ITEM_TYPE.WEAPON) {
					Weapon weapon = ItemManager.Instance.CreateNewItemInstance(itemName) as Weapon;
					//weapon.ConstructSkillsList ();
					TryEquipWeapon(weapon);
				} else if (itemType == ITEM_TYPE.ARMOR) {
                    Armor armor = ItemManager.Instance.CreateNewItemInstance(itemName) as Armor;
					TryEquipArmor(armor);
				}
			}
		}
		public void EquipItem(string itemType, string itemName) {
            if (ItemManager.Instance.allItems.ContainsKey(itemName)) {
                if (itemType.ToLower() == "weapon") {
                    Weapon weapon = ItemManager.Instance.CreateNewItemInstance(itemName) as Weapon;
                    //weapon.ConstructSkillsList();
                    TryEquipWeapon(weapon);
                }
                else if (itemType.ToLower() == "armor") {
                    Armor armor = ItemManager.Instance.CreateNewItemInstance(itemName) as Armor;
                    TryEquipArmor(armor);
                }
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
            if (hasEquipped) {
#if !WORLD_CREATION_TOOL
                Log equipLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "equip_item");
                equipLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                equipLog.AddToFillers(null, item.itemName, LOG_IDENTIFIER.ITEM_1);
                AddHistory(equipLog);
#endif
                Messenger.Broadcast(Signals.ITEM_EQUIPPED, item, this);
            }
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
            Messenger.Broadcast(Signals.ITEM_UNEQUIPPED, item, this);
        }
		//Unown an item making the owner of it null, if successfully unowned, return true, otherwise, return false
		internal bool UnownItem(Item item){
			if(item.owner.id == this._id){
				item.SetOwner (null);
				return true;
			}
			return false;
		}
		//Own an Item
		internal void OwnItem(Item item){
			item.SetOwner (this);
		}
		//Transfer item ownership
		internal void TransferItemOwnership(Item item, Character newOwner){
			newOwner.OwnItem (item);
		}
		//Try to equip a weapon to a body part of this character and add it to the list of items this character have
		internal bool TryEquipWeapon(Weapon weapon){
            if (!_characterClass.allowedWeaponTypes.Contains(weapon.weaponType)) {
                return false;
            }
			for (int j = 0; j < ItemManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements.Count; j++) {
				IBodyPart.ATTRIBUTE currReq = ItemManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements[j];
				if(!AttachWeaponToBodyPart(weapon, currReq)){
					DetachWeaponFromBodyParts (weapon);
					return false;
				}
			}
			Weapon newWeapon = weapon;
			if(weapon.isUnlimited){
				newWeapon = (Weapon)weapon.CreateNewCopy ();
				newWeapon.isUnlimited = false;
			}
			AddEquippedItem(newWeapon);
			//newWeapon.SetPossessor (this);
			//newWeapon.ResetDurability();
//			weapon.SetOwner(this);
			if(newWeapon.owner == null){
				OwnItem (newWeapon);
			}
			_equippedWeaponPower = newWeapon.weaponPower;
            _equippedWeapon = newWeapon;
			//for (int i = 0; i < newWeapon.skills.Count; i++) {
			//	this._skills.Add (newWeapon.skills [i]);
			//}

			//          Debug.Log(this.name + " equipped " + weapon.itemName + " to " + bodyPart.bodyPart.ToString());
			//StreamingAssetsUI.Instance.UpdateCharacterSummary(this);
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
			for (int i = 0; i < ItemManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements.Count; i++) {
				IBodyPart.ATTRIBUTE req = ItemManager.Instance.weaponTypeData[weapon.weaponType].equipRequirements[i];
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
			//for (int i = 0; i < weapon.skills.Count; i++) {
			//	this._skills.Remove (weapon.skills [i]);
			//}
			_equippedWeaponPower = 0f;
		}
		//Try to equip an armor to a body part of this character and add it to the list of items this character have
		internal bool TryEquipArmor(Armor armor){
			IBodyPart bodyPartToEquip = GetBodyPartForArmor(armor);
			if(bodyPartToEquip == null){
				return false;
			}
			Armor newArmor = armor;
			if(armor.isUnlimited){
				newArmor = (Armor)armor.CreateNewCopy ();
				newArmor.isUnlimited = false;
			}
			bodyPartToEquip.AttachItem(newArmor, Utilities.GetNeededAttributeForArmor(newArmor));
			//			armor.bodyPartAttached = bodyPart;
			AddEquippedItem(newArmor);
			//newArmor.SetPossessor (this);
			//newArmor.ResetDurability();
//			armor.SetOwner(this);
			if(newArmor.owner == null){
				OwnItem (newArmor);
			}
			//Debug.Log(this.name + " equipped " + newArmor.itemName + " to " + bodyPartToEquip.name);
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
            if (_inventory.Contains(newItem)) {
                throw new Exception(this.name + " already has an instance of " + newItem.itemName);
            }
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
		//internal bool HasEquipmentOfType(EQUIPMENT_TYPE equipmentType, IBodyPart.ATTRIBUTE attribute) {
		//	for (int i = 0; i < equippedItems.Count; i++) {
		//		Item currItem = equippedItems[i];
		//		if(currItem.itemType == ITEM_TYPE.ARMOR) {
		//			Armor armor = (Armor)currItem;
		//			if ((EQUIPMENT_TYPE)armor.armorType == equipmentType && (armor.attributes.Contains(attribute) || attribute == IBodyPart.ATTRIBUTE.NONE)) {
		//				return true;
		//			}
		//		} else if (currItem.itemType == ITEM_TYPE.WEAPON) {
		//			Weapon weapon = (Weapon)currItem;
		//			if ((EQUIPMENT_TYPE)weapon.weaponType == equipmentType && (weapon.attributes.Contains(attribute) || attribute == IBodyPart.ATTRIBUTE.NONE)) {
		//				return true;
		//			}
		//		}
		//	}
		//	return false;
		//}
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
        //internal void AdjustGold(int amount) {
        //    _gold += amount;
        //    _gold = Mathf.Max(0, _gold);
        //}
        //internal void AdjustPrestige(int amount) {
        //    _prestige += amount;
        //    _prestige = Mathf.Max(0, _prestige);
        //}
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
        internal bool HasItem(string itemName) {
			for (int i = 0; i < _equippedItems.Count; i++) {
				Item currItem = _equippedItems[i];
				if (currItem.itemName.Equals(itemName)) {
					return true;
				}
			}
            for (int i = 0; i < _inventory.Count; i++) {
                Item currItem = _inventory[i];
                if (currItem.itemName.Equals(itemName)) {
                    return true;
                }
            }
            return false;
        }
        internal Item GetItemInInventory(string itemName){
			for (int i = 0; i < _inventory.Count; i++) {
				Item currItem = _inventory[i];
				if (currItem.itemName.Equals(itemName)) {
					return currItem;
				}
			}
			return null;
		}
		internal Item GetItemInEquipped(string itemName){
			for (int i = 0; i < _equippedItems.Count; i++) {
				Item currItem = _equippedItems[i];
				if (currItem.itemName.Equals(itemName)) {
					return currItem;
				}
			}
			return null;
		}
		public Item GetItemInAll(string itemName){
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
        private void AddItemBonuses(Item item) {
            if(item.itemType == ITEM_TYPE.ARMOR) {
                Armor armor = (Armor) item;
                _bonusPDef += armor.pDef;
                _bonusMDef += armor.mDef;
                _bonusPDefPercent += (armor.prefix.bonusPDefPercent + armor.suffix.bonusPDefPercent);
                _bonusMDefPercent += (armor.prefix.bonusMDefPercent + armor.suffix.bonusMDefPercent);
            }
            //AdjustMaxHP(item.bonusMaxHP);
            //AdjustBonusStrength(item.bonusStrength);
            //AdjustBonusIntelligence(item.bonusIntelligence);
            //AdjustBonusAgility(item.bonusAgility);
            //AdjustBonusVitality(item.bonusVitality);
        }
        private void RemoveItemBonuses(Item item) {
            if (item.itemType == ITEM_TYPE.ARMOR) {
                Armor armor = (Armor) item;
                _bonusPDef -= armor.pDef;
                _bonusMDef -= armor.mDef;
                _bonusPDefPercent -= (armor.prefix.bonusPDefPercent + armor.suffix.bonusPDefPercent);
                _bonusMDefPercent -= (armor.prefix.bonusMDefPercent + armor.suffix.bonusMDefPercent);
            }
            //AdjustMaxHP(-item.bonusMaxHP);
            //AdjustBonusStrength(-item.bonusStrength);
            //AdjustBonusIntelligence(-item.bonusIntelligence);
            //AdjustBonusVitality(-item.bonusVitality);
        }
        #endregion

        #region Status Effects
        internal void AddStatusEffect(STATUS_EFFECT statusEffect){
			this._statusEffects.Add (statusEffect);
		}
		internal void RemoveStatusEffect(STATUS_EFFECT statusEffect){
			this._statusEffects.Remove (statusEffect);
		}
        internal void CureStatusEffects(){
			for (int i = 0; i < _statusEffects.Count; i++) {
				STATUS_EFFECT statusEffect = _statusEffects[i];
				int chance = Utilities.rng.Next (0, 100);
				if (chance < 15) {
                    _party.currentCombat.AddCombatLog(this.name + " is cured from " + statusEffect.ToString ().ToLower () + ".", this.currentSide);
					RemoveStatusEffect (statusEffect);
					i--;
				}
			}
			for (int i = 0; i < this._bodyParts.Count; i++) {
				BodyPart bodyPart = this._bodyParts [i];
				if(bodyPart.statusEffects.Count > 0){
					for (int j = 0; j < bodyPart.statusEffects.Count; j++) {
						STATUS_EFFECT statusEffect = bodyPart.statusEffects [j];
						if(statusEffect != STATUS_EFFECT.DECAPITATED){
							int chance = Utilities.rng.Next (0, 100);
							if(chance < 15){
                                _party.currentCombat.AddCombatLog(this.name + "'s " + bodyPart.name.ToLower () + " is cured from " + statusEffect.ToString ().ToLower () + ".", this.currentSide);
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
			if (_statusEffects.Contains(statusEffect)) {
				return true;
			}
            for (int i = 0; i < this._bodyParts.Count; i++) {
                BodyPart bodyPart = this._bodyParts[i];
                if (bodyPart.statusEffects.Contains(statusEffect)) {
                    return true;
                }
            }
            return false;
        }
		#endregion

		#region Skills
		private List<Skill> GetGeneralSkills(){
            List<Skill> allGeneralSkills = new List<Skill>();
            foreach (Skill skill in SkillManager.Instance.generalSkills.Values) {
                allGeneralSkills.Add(skill.CreateNewCopy());
            }
            return allGeneralSkills;
		}
		private List<Skill> GetBodyPartSkills(){
			List<Skill> allBodyPartSkills = new List<Skill>();
			foreach (Skill skill in SkillManager.Instance.bodyPartSkills.Values) {
				bool requirementsPassed = true;
				//Skill skill	= SkillManager.Instance.bodyPartSkills [skillName];
				for (int j = 0; j < skill.skillRequirements.Length; j++) {
					if(!HasAttribute(skill.skillRequirements[j].attributeRequired, skill.skillRequirements[j].itemQuantity)){
						requirementsPassed = false;
						break;
					}
				}
				if(requirementsPassed){
					allBodyPartSkills.Add (skill.CreateNewCopy ());
				}
			}
			return allBodyPartSkills;
		}
		#endregion

		#region Roles
		public void AssignRole(CHARACTER_ROLE role) {
            bool wasRoleChanged = false;
			if(_role != null){
				_role.ChangedRole ();
#if !WORLD_CREATION_TOOL
                Log roleChangeLog = new Log(GameManager.Instance.Today(), "Character", "Generic", "change_role");
                roleChangeLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                AddHistory(roleChangeLog);
#endif
                wasRoleChanged = true;
            }
			switch (role) {
		        case CHARACTER_ROLE.HERO:
			        _role = new Hero(this);
			        break;
		        case CHARACTER_ROLE.VILLAIN:
			        _role = new Villain(this);
			        break;
                case CHARACTER_ROLE.CIVILIAN:
                    _role = new Civilian(this);
                    break;
                case CHARACTER_ROLE.KING:
                    _role = new King(this);
                    break;
                default:
		            break;
			}
            if (_role != null) {
                _role.OnAssignRole();
            }
            if (wasRoleChanged) {
                Messenger.Broadcast(Signals.ROLE_CHANGED, this);
            }
		}
		public void ChangeRole(){
			//TODO: Things to do when a character changes role

//			AssignRole(role);
			if(_raceSetting.tags.Contains(CHARACTER_TAG.SAPIENT)){
				CHARACTER_ROLE roleToCreate = CHARACTER_ROLE.HERO;
				WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary();
				if (characterRoleProductionDictionary.GetTotalOfWeights () > 0) {
					roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights ();
				}
				AssignRole(roleToCreate);
			}
   //         else{
			//	AssignRole (CHARACTER_ROLE.TAMED_BEAST);
			//}
		}
		#endregion

		#region Character Class
		public void AssignClass(CharacterClass charClass) {
			_characterClass = charClass;
		}
		#endregion

		#region Traits
  //      private void GenerateTraits() {
  //          CharacterType baseCharacterType = CharacterManager.Instance.GetRandomCharacterType();
  //          //_characterType = baseCharacterType;
  //          _allTraits = new List<TRAIT>(baseCharacterType.otherTraits);
  //          //Charisma
  //          if (baseCharacterType.charismaTrait == CHARISMA.NONE) {
  //              TRAIT charismaTrait = GenerateCharismaTrait();
  //              if(charismaTrait != TRAIT.NONE) {
  //                  _allTraits.Add(charismaTrait);
  //              }
  //          } else {
  //              _allTraits.Add((TRAIT)baseCharacterType.charismaTrait);
  //          }
  //          //Intelligence
  //          if (baseCharacterType.intelligenceTrait == INTELLIGENCE.NONE) {
  //              TRAIT intTrait = GenerateIntelligenceTrait();
  //              if(intTrait != TRAIT.NONE) {
  //                  _allTraits.Add(intTrait);
  //              }
  //          } else {
  //              _allTraits.Add((TRAIT)baseCharacterType.intelligenceTrait);
  //          }
  //          //Efficiency
  //          if (baseCharacterType.efficiencyTrait == EFFICIENCY.NONE) {
  //              TRAIT efficiencyTrait = GenerateEfficiencyTrait();
  //              if (efficiencyTrait != TRAIT.NONE) {
  //                  _allTraits.Add(efficiencyTrait);
  //              }
  //          } else {
  //              _allTraits.Add((TRAIT)baseCharacterType.efficiencyTrait);
  //          }
  //          //Military
  //          if (baseCharacterType.militaryTrait == MILITARY.NONE) {
  //              TRAIT militaryTrait = GenerateMilitaryTrait();
  //              _allTraits.Add(militaryTrait);
  //          } else {
  //              _allTraits.Add((TRAIT)baseCharacterType.militaryTrait);
  //          }
  //          //Health
  //          if (baseCharacterType.healthTrait == HEALTH.NONE) {
  //              TRAIT healthTrait = GenerateHealthTrait();
  //              if(healthTrait != TRAIT.NONE) {
  //                  _allTraits.Add(healthTrait);
  //              }
  //          } else {
  //              _allTraits.Add((TRAIT)baseCharacterType.healthTrait);
  //          }
  //          //Strength
  //          if (baseCharacterType.strengthTrait == STRENGTH.NONE) {
  //              TRAIT strengthTrait = GenerateStrengthTrait();
  //              if (strengthTrait != TRAIT.NONE) {
  //                  _allTraits.Add(strengthTrait);
  //              }
  //          } else {
  //              _allTraits.Add((TRAIT)baseCharacterType.strengthTrait);
  //          }
  //          //Agility
  //          if (baseCharacterType.agilityTrait == AGILITY.NONE) {
  //              TRAIT agilityTrait = GenerateAgilityTrait();
  //              if (agilityTrait != TRAIT.NONE) {
  //                  _allTraits.Add(agilityTrait);
  //              }
  //          } else {
  //              _allTraits.Add((TRAIT)baseCharacterType.agilityTrait);
  //          }
  //          _traits = new List<Trait>();
  //          for (int i = 0; i < _allTraits.Count; i++) {
  //              TRAIT currTrait = _allTraits[i];
  //              Trait trait = CharacterManager.Instance.CreateNewTraitForCharacter(currTrait, this);
  //              if (trait != null) {
		//			AddTrait (trait);
  //              }
  //          }
  //      }
		//public void AddTrait(Trait trait){
		//	trait.AssignCharacter(this);
		//	_traits.Add(trait);
		//}
		//public void RemoveTrait(Trait trait){
		//	trait.AssignCharacter(null);
		//	_traits.Remove(trait);
		//}
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
  //      public bool HasTrait(TRAIT trait) {
		//	for (int i = 0; i < _traits.Count; i++) {
		//		if(_traits[i].trait == trait) {
		//			return true;
		//		}
		//	}
		//	return false;
		//}
        #endregion

		#region Character Tags
		public CharacterTag AssignTag(CHARACTER_TAG tag) {
			if(HasTag(tag)){
				return null;
			}
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
            case CHARACTER_TAG.SUCCESSOR:
                charTag = new Successor(this);
                break;
			case CHARACTER_TAG.TYRANNICAL:
				charTag = new Tyrannical(this);
				break;
			case CHARACTER_TAG.WARMONGER:
				charTag = new Warmonger(this);
				break;
			case CHARACTER_TAG.MILD_PSYTOXIN:
				charTag = new MildPsytoxin(this);
				break;
			case CHARACTER_TAG.MODERATE_PSYTOXIN:
				charTag = new ModeratePsytoxin(this);
				break;
			case CHARACTER_TAG.SEVERE_PSYTOXIN:
				charTag = new SeverePsytoxin(this);
				break;
			case CHARACTER_TAG.RITUALIST:
				charTag = new Ritualist(this);
				break;
			case CHARACTER_TAG.HERBALIST:
				charTag = new Herbalist(this);
				break;
            case CHARACTER_TAG.HIBERNATES:
                charTag = new Hibernates(this);
                break;
            case CHARACTER_TAG.PREDATOR:
                charTag = new Predator(this);
                break;
			case CHARACTER_TAG.HUNTED:
				charTag = new Hunted(this);
				break;
            case CHARACTER_TAG.ALPHA:
                charTag = new Alpha(this);
                break;
            case CHARACTER_TAG.BETA:
                charTag = new Beta(this);
                break;
            case CHARACTER_TAG.HUNGRY:
                charTag = new Hungry(this);
                break;
            case CHARACTER_TAG.FAMISHED:
                charTag = new Famished(this);
                break;
            case CHARACTER_TAG.TIRED:
                charTag = new Tired(this);
                break;
            case CHARACTER_TAG.EXHAUSTED:
                charTag = new Exhausted(this);
                break;
            case CHARACTER_TAG.SAD:
                charTag = new Sad(this);
                break;
            case CHARACTER_TAG.DEPRESSED:
                charTag = new Depressed(this);
                break;
            case CHARACTER_TAG.ANXIOUS:
                charTag = new Anxious(this);
                break;
            case CHARACTER_TAG.INSECURE:
                charTag = new Insecure(this);
                break;
            case CHARACTER_TAG.DRUNK:
                charTag = new Drunk(this);
                break;
            }
			if(charTag != null){
				AddCharacterTag (charTag);
			}
            return charTag;
		}
		private void GenerateRaceTags(){
			for (int i = 0; i < _raceSetting.tags.Count; i++) {
				AssignTag (_raceSetting.tags [i]);
			}
		}
        private void GenerateSetupTags(CharacterSetup setup) {
            for (int i = 0; i < setup.tags.Count; i++) {
                AssignTag(setup.tags[i]);
            }
        }
		public void AddCharacterTag(CharacterTag tag){
			_tags.Add(tag);
			tag.Initialize ();
		}
		public bool RemoveCharacterTag(CharacterTag tag){
			if(_tags.Remove(tag)){
				tag.OnRemoveTag();
				return true;
			}
			return false;
		}
		public bool RemoveCharacterTag(CHARACTER_TAG tag) {
            for (int i = 0; i < _tags.Count; i++) {
                CharacterTag currTag = _tags[i];
                if (currTag.tagType == tag) {
                    return RemoveCharacterTag(currTag);
                }
            }
			return false;
        }
		public bool HasTags(CHARACTER_TAG[] tagsToHave, bool mustHaveAll = false){
			return DoesHaveTags (this, tagsToHave, mustHaveAll);
		}
		private bool DoesHaveTags(Character currCharacter, CHARACTER_TAG[] tagsToHave, bool mustHaveAll = false){
			if(mustHaveAll){
				int tagsCount = 0;
				for (int i = 0; i < currCharacter.tags.Count; i++) {
					for (int j = 0; j < tagsToHave.Length; j++) {
						if(tagsToHave[j] == currCharacter.tags[i].tagType){
							tagsCount++;
							break;
						}
					}
					if(tagsCount >= tagsToHave.Length){
						return true;
					}
				}
			}else{
				for (int i = 0; i < currCharacter.tags.Count; i++) {
					for (int j = 0; j < tagsToHave.Length; j++) {
						if(tagsToHave[j] == currCharacter.tags[i].tagType){
							return true;
						}
					}
				}
			}
			return false;
		}
		public bool HasTag(CHARACTER_TAG tag) {
            for (int i = 0; i < _tags.Count; i++) {
                if (_tags[i].tagType == tag) {
                    return true;
                }
            }

            return false;
		}
		public bool HasTag(string tag) {
            for (int i = 0; i < _tags.Count; i++) {
                if (_tags[i].tagName == tag) {
                    return true;
                }
            }
			return false;
		}
		public CharacterTag GetTag(CHARACTER_TAG tag){
            for (int i = 0; i < _tags.Count; i++) {
                if (_tags[i].tagType == tag) {
                    return _tags[i];
                }
            }
			return null;
		}
		public CharacterTag GetTag(string tag){
            for (int i = 0; i < _tags.Count; i++) {
                if (_tags[i].tagName == tag) {
                    return _tags[i];
                }
            }
			return null;
		}
		#endregion

        #region Faction
        public void SetFaction(Faction faction) {
			_faction = faction;
            Messenger.Broadcast<Character>(Signals.FACTION_SET, this);
		}
		#endregion

		#region Party
        /*
         Create a new Party with this character as the leader.
             */
        public NewParty CreateNewParty() {
            if(_party != null) {
                _party.RemoveCharacter(this);
            }
            CharacterParty newParty = new CharacterParty();
            newParty.AddCharacter(this);
            newParty.CreateCharacterObject();
            return newParty;
        }
		public void SetParty(NewParty party) {
			_party = party as CharacterParty;
		}
        #endregion

        #region Location
		public bool IsCharacterInAdjacentRegionOfThis(Character targetCharacter){
			for (int i = 0; i < _currentRegion.adjacentRegionsViaRoad.Count; i++) {
				if(targetCharacter.party.currentRegion.id == _currentRegion.adjacentRegionsViaRoad[i].id){
					return true;
				}
			}
			return false;
		}
		#endregion

		#region Quests
        public void AddQuestData(CharacterQuestData questData) {
            if (!_questData.Contains(questData)) {
                _questData.Add(questData);
            }
        }
        public void RemoveQuestData(CharacterQuestData questData) {
            _questData.Remove(questData);
        }

        #endregion

        #region Tags
        public void AssignInitialTags(){
			int tagChance = UnityEngine.Random.Range (0, 100);
			CHARACTER_TAG[] initialTags = (CHARACTER_TAG[])System.Enum.GetValues (typeof(CHARACTER_TAG));
			for (int j = 0; j < initialTags.Length; j++) {
				CHARACTER_TAG tag = initialTags [j];
				if(tagChance < Utilities.GetTagWorldGenChance(tag)){
					AssignTag (tag);
				}
			}
		}
        #endregion

        #region HP
        public bool IsHealthFull() {
            return _currentHP >= _maxHP;
        }
        #endregion

        #region Utilities
        public void ChangeGender(GENDER gender) {
            _gender = gender;
            Messenger.Broadcast(Signals.GENDER_CHANGED, this, gender);
        }
        public void ChangeRace(RACE race) {
            //TODO: Change data as needed
            RaceSetting raceSetting = RaceManager.Instance.racesDictionary[race.ToString()];
            _raceSetting = raceSetting.CreateNewCopy();
            //TODO: check equipped items and add log
        }
        public void ChangeClass(string className) {
            //TODO: Change data as needed
            string previousClassName = _characterClass.className;
            CharacterClass charClass = CharacterManager.Instance.classesDictionary[className];
            _characterClass = charClass.CreateNewCopy();
            OnCharacterClassChange();

#if !WORLD_CREATION_TOOL
            Log log = new Log(GameManager.Instance.Today(), "CharacterActions", "ChangeClassAction", "change_class");
            log.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, previousClassName, LOG_IDENTIFIER.STRING_1);
            log.AddToFillers(null, _characterClass.className, LOG_IDENTIFIER.STRING_2);
            AddHistory(log);
            //check equipped items
#endif

        }
        public void SetName(string newName){
			_name = newName;
		}
        //If true, character can't do daily action (onDailyAction), i.e. actions, needs
        //public void SetIsIdle(bool state) {
        //    _isIdle = state;
        //}
        public bool HasPathToParty(Party partyToJoin) {
            return PathGenerator.Instance.GetPath(currLocation, partyToJoin.currLocation, PATHFINDING_MODE.USE_ROADS, _faction) != null;
        }
        public void CenterOnCharacter() {
            if (!this.isDead) {
                CameraMove.Instance.CenterCameraOn(_party.specificLocation.tileLocation.gameObject);
            }
        }
		//Death of this character if he/she is in the region specified
		private void RegionDeath(Region region){
			if(_party.currentRegion.id == region.id){
				Death ();
			}
		}
        private void GetRandomCharacterColor() {
            _characterColor = CombatManager.Instance.UseRandomCharacterColor();
            _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
        }
        public void SetCharacterColor(Color color) {
            _characterColor = color;
            _characterColorCode = ColorUtility.ToHtmlStringRGBA(_characterColor).Substring(0, 6);
        }
        public void EverydayAction() {
            //if (!_isIdle) {
                if (onDailyAction != null) {
                    onDailyAction();
                }
            //}
        }
        //public void AdvertiseSelf(ActionThread actionThread) {
        //    if(actionThread.character.id != this.id && _currentRegion.id == actionThread.character.party.currentRegion.id) {
        //        actionThread.AddToChoices(_characterObject);
        //    }
        //}
        public bool CanObtainResource(List<RESOURCE> resources) {
            if (this.role != null) {//characters without a role cannot get actions, and therefore cannot obtain resources
                for (int i = 0; i < _party.currentRegion.landmarks.Count; i++) {
                    BaseLandmark landmark = _party.currentRegion.landmarks[i];
                    StructureObj iobject = landmark.landmarkObj;
                    if (iobject.currentState.actions != null && iobject.currentState.actions.Count > 0) {
                        for (int k = 0; k < iobject.currentState.actions.Count; k++) {
                            CharacterAction action = iobject.currentState.actions[k];
                            if (action.actionData.resourceGiven != RESOURCE.NONE && resources.Contains(action.actionData.resourceGiven)) { //does the action grant a resource, and is that a resource that is needed
                                if (action.MeetsRequirements(_party, landmark) && action.CanBeDone(iobject) && action.CanBeDoneBy(_party, iobject)) { //Filter
                                    //if the character can do an action that yields a needed resource, return true
                                    return true;
                                }
                            }

                        }
                    }
                }
            }
            return false;
        }
        public bool IsSpecialCivilian() {
            if (this.characterClass != null) {
                if (this.characterClass.className.Equals("Farmer") || this.characterClass.className.Equals("Miner") || this.characterClass.className.Equals("Retired Hero") ||
                    this.characterClass.className.Equals("Shopkeeper") || this.characterClass.className.Equals("Woodcutter")) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Relationships
        public void AddNewRelationship(Character relWith, Relationship relationship) {
            if (!_relationships.ContainsKey(relWith)) {
                _relationships.Add(relWith, relationship);
                Messenger.Broadcast<Relationship>(Signals.RELATIONSHIP_CREATED, relationship);
            } else {
                throw new Exception(this.name + " already has a relationship with " + relWith.name + ", but something is trying to create a new one!");
            }
        }
        public void RemoveRelationshipWith(Character relWith) {
            if (_relationships.ContainsKey(relWith)) {
                Relationship rel = _relationships[relWith];
                _relationships.Remove(relWith);
                Messenger.Broadcast<Relationship>(Signals.RELATIONSHIP_REMOVED, rel);
            }
        }
        public Relationship GetRelationshipWith(Character character) {
            if (_relationships.ContainsKey(character)) {
                return _relationships[character];
            }
            return null;
        }
        public bool AlreadyHasRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
            foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
                if (kvp.Value.HasStatus(relStat)) {
                    return true;
                }
            }
            return false;
        }
        public Character GetCharacterWithRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
            foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
                if (kvp.Value.HasStatus(relStat)) {
                    return kvp.Key;
                }
            }
            return null;
        }
        public List<Character> GetCharactersWithRelationshipStatus(CHARACTER_RELATIONSHIP relStat) {
            List<Character> characters = new List<Character>();
            foreach (KeyValuePair<Character, Relationship> kvp in relationships) {
                if (kvp.Value.HasStatus(relStat) && characters.Contains(kvp.Key)) {
                    characters.Add(kvp.Key);
                }
            }
            return characters;
        }
        public void LoadRelationships(List<RelationshipSaveData> data) {
            _relationships = new Dictionary<Character, Relationship>();
            for (int i = 0; i < data.Count; i++) {
                RelationshipSaveData currData = data[i];
                Character otherCharacter = CharacterManager.Instance.GetCharacterByID(currData.targetCharacterID);
                Relationship rel = new Relationship(this, otherCharacter);
                rel.AddRelationshipStatus(currData.relationshipStatuses);
                _relationships.Add(otherCharacter, rel);

            }
        }
        #endregion

        #region History
        public void AddHistory(Log log) {
            _history.Add(log);
            if (this._history.Count > 20) {
                this._history.RemoveAt(0);
            }
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
        #endregion

		#region Character
		public bool IsHostileWith(Character character){
            if (this.faction == null) {
                return true; //this character has no faction
            }
            //if (this.currentAction != null && this.currentAction.HasHostilitiesBecauseOfTask(combatInitializer)) {
            //    return true;
            //}
            //Check here if the combatInitializer is hostile with this character, if yes, return true
            Faction factionOfEnemy = character.faction;
            
            //if (combatInitializer.icharacterType == ICHARACTER_TYPE.CHARACTER) {
            //    factionOfEnemy = (combatInitializer as Character).faction;
            //}else if(combatInitializer is Party) {
            //    factionOfEnemy = (combatInitializer as Party).faction;
            //}
            if(factionOfEnemy != null) {
                if(factionOfEnemy.id == this.faction.id) {
                    return false; //characters are of same faction
                }
                FactionRelationship rel = this.faction.GetRelationshipWith(factionOfEnemy);
                if(rel.relationshipStatus == FACTION_RELATIONSHIP_STATUS.HOSTILE) {
                    return true; //factions of combatants are hostile
                }
                return false;
            } else {
                return true; //enemy has no faction
            }
			
		}
        public STANCE GetCurrentStance() {
            return STANCE.NEUTRAL;
        }
        #endregion

		#region Combat Handlers
		public void SetIsInCombat (bool state){
			_isInCombat = state;
		}
        #endregion

        #region Landmarks
        public void AddExploredLandmark(BaseLandmark landmark){
			_exploredLandmarks.Add(landmark); //did not add checking if landmark is already in list, since I want to allow duplicates
            //schedule removal of landmark after 6 months
            GameDate expiration = GameManager.Instance.Today();
            expiration.AddMonths(6);
            SchedulingManager.Instance.AddEntry(expiration, () => RemoveLandmarkAsExplored(landmark));
		}
        private void RemoveLandmarkAsExplored(BaseLandmark landmark) {
            _exploredLandmarks.Remove(landmark);
        }
		#endregion

		#region Psytoxin
		private void RegionPsytoxin(List<Region> regions){
			for (int i = 0; i < regions.Count; i++) {
				if(_party.currentRegion.id == regions[i].id){
					InfectedByPsytoxin ();
					break;
				}
			}
		}
		private void InfectedByPsytoxin(){
			if(HasTag(CHARACTER_TAG.SEVERE_PSYTOXIN)){
				return;	
			}
			ModeratePsytoxin modPsytoxin = (ModeratePsytoxin)GetTag (CHARACTER_TAG.MODERATE_PSYTOXIN);
			if(modPsytoxin != null){
				modPsytoxin.TriggerWorsenCase ();
			}else{
				MildPsytoxin mildPsytoxin = (MildPsytoxin)GetTag (CHARACTER_TAG.MILD_PSYTOXIN);
				if(mildPsytoxin != null){
					mildPsytoxin.TriggerWorsenCase ();
				}else{
					AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);
				}
			}
		}
		#endregion

		#region Traces
		public void LeaveTraceOnLandmark(){
			ILocation location = _party.specificLocation;
			if(location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK){
				BaseLandmark landmark = location as BaseLandmark;
				int chance = UnityEngine.Random.Range (0, 100);
				int value = GetLeaveTraceChance ();
				if(chance < value){
                    landmark.AddTrace(this);
                }
			}
		}
		private int GetLeaveTraceChance(){
			STANCE stance = GetCurrentStance ();
			switch(stance){
			case STANCE.COMBAT:
				return 100;
			case STANCE.NEUTRAL:
				return 50;
			case STANCE.STEALTHY:
				return 25;
			}
			return 0;
		}
		public void AddTraceInfo(Character character, string identifier, bool isUnique){
			if(isUnique){
				Character previousTrace = GetCharacterFromTraceInfo (identifier);
				if(previousTrace != null){ //If there is an existing trace of the item, replace it with this new trace
					if(previousTrace.id != character.id){
						_traceInfo [previousTrace].Remove (identifier);
						if(_traceInfo[previousTrace].Count <= 0){
							_traceInfo.Remove (previousTrace);
						}
						Debug.Log (this.name +  " REMOVED TRACE INFO OF " + previousTrace.name + " FOR " + identifier);
					}else{
						return;
					}
				}
			}
			if(_traceInfo.ContainsKey(character)){
				if(!_traceInfo [character].Contains(identifier)){
					_traceInfo [character].Add (identifier);
					Debug.Log (this.name +  " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
				}
			}else{
				_traceInfo.Add (character, new List<string> (){ identifier });
				Debug.Log (this.name +  " ADDED TRACE INFO OF " + character.name + " FOR " + identifier);
			}
		}
		public Character GetCharacterFromTraceInfo(string info){
			foreach (Character character in _traceInfo.Keys) {
				if(_traceInfo[character].Contains(info)){
					return character;
				}
			}
			return null;
		}
        #endregion

        #region Action Queue
        public bool DoesSatisfiesPrerequisite(IPrerequisite prerequisite) {
            if(prerequisite.prerequisiteType == PREREQUISITE.RESOURCE) {
                ResourcePrerequisite resourcePrerequisite = prerequisite as ResourcePrerequisite;
                if(resourcePrerequisite.resourceType != RESOURCE.NONE && _party.characterObject.resourceInventory[resourcePrerequisite.resourceType] >= resourcePrerequisite.amount) {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Needs
        private void CiviliansDiedReduceSanity(StructureObj whereCiviliansDied, int amount) {
            if(_currentRegion.id == whereCiviliansDied.objectLocation.tileLocation.region.id) {
                ILocation location = _party.specificLocation;
                if (location.tileLocation.id == whereCiviliansDied.objectLocation.tileLocation.id || whereCiviliansDied.objectLocation.tileLocation.neighbourDirections.ContainsValue(location.tileLocation)){
                    int sanityToReduce = amount * 5;
                    this.role.AdjustSanity(-sanityToReduce);
                    Debug.Log(this.name + " has reduced its sanity by " + sanityToReduce + " because " + amount + " civilians died in " + whereCiviliansDied.objectLocation.tileLocation.tileName + " (" + whereCiviliansDied.objectLocation.tileLocation.coordinates + ")");
                }
            }
        }
        #endregion

        #region Portrait Settings
        public void SetPortraitSettings(PortraitSettings settings) {
            _portraitSettings = settings;
        }
        #endregion

        #region RPG
        public void LevelUp() {
            if(_level < CharacterManager.Instance.maxLevel) {
                _level += 1;
                _experience = 0;
                _strength += _baseStrength;
                _agility += _baseAgility;
                _intelligence += _baseIntelligence;
                _vitality += _baseVitality;
                RecomputeMaxHP();
                RecomputeMaxExperience();
                RecomputeMaxSP();
                //Reset to full health and sp
                _currentHP = _maxHP;
                _sp = _maxSP;
            }
        }
        public void OnCharacterClassChange() {
            AllocateStatPoints(10);
            RecomputeMaxHP();
            RecomputeMaxSP();
            if(_currentHP > _maxHP) {
                _currentHP = _maxHP;
            }
            if (_sp > _maxSP) {
                _sp = _maxSP;
            }
        }
        public void AdjustBonusStrength(int amount) {
            _bonusStrength += amount;
        }
        public void AdjustBonusAgility(int amount) {
            _bonusAgility += amount;
        }
        public void AdjustBonusIntelligence(int amount) {
            _bonusIntelligence += amount;
        }
        public void AdjustBonusVitality(int amount) {
            _bonusVitality += amount;
        }
        public void AdjustExperience(int amount) {
            _experience += amount;
            if(_experience >= _maxExperience) {
                LevelUp();
            }
        }
        public void AdjustElementalWeakness(ELEMENT element, float amount) {
            _elementalWeaknesses[element] += amount;
        }
        public void AdjustElementalResistance(ELEMENT element, float amount) {
            _elementalResistances[element] += amount;
        }
        public void AdjustSP(int amount) {
            _sp += amount;
            _sp = Mathf.Clamp(_sp, 0, _maxSP);
        }
        public void AdjustCritChance(float amount) {
            _critChance += amount;
            _critChance = Mathf.Clamp(_critChance, 0f, 100f);
        }
        public void AdjustCritDamage(float amount) {
            _critDamage += amount;
        }
        private void RecomputeMaxHP() {
            this._fixedMaxHP = 10 + (Mathf.CeilToInt(_characterClass.hpModifier * ((Mathf.Pow((float) _level, 0.7f)) / 0.33f)));
            int previousMaxHP = this._maxHP;
            this._maxHP = this._fixedMaxHP + (int) ((float) this._fixedMaxHP * ((float) vitality / 100f));
            if (this._currentHP > this._maxHP || this._currentHP == previousMaxHP) {
                this._currentHP = this._maxHP;
            }
        }
        private void RecomputeMaxExperience() {
            _maxExperience = Mathf.CeilToInt(100f * ((Mathf.Pow((float) _level, 1.25f)) / 1.1f));
        }
        private void RecomputeMaxSP() {
            _maxSP = 10 + (Mathf.CeilToInt(_characterClass.spModifier * ((Mathf.Pow((float) _level, 0.7f)) / 0.33f)));
        }
        public int GetPDef(ICharacter enemy) {
            float levelDiff = (float) (enemy.level - level);
            return (int)((((float) (_bonusPDef + (strength + (vitality * 2)))) * (1f + (_bonusPDefPercent / 100f))) * (1f + ((levelDiff < 0 ? 0: levelDiff) / 20f)));
        }
        public int GetMDef(ICharacter enemy) {
            float levelDiff = (float) (enemy.level - level);
            return (int) ((((float) (_bonusMDef + (intelligence + (vitality * 2)))) * (1f + (_bonusMDefPercent / 100f))) * (1f + ((levelDiff < 0 ? 0 : levelDiff) / 20f)));
        }
        public int GetSelfPdef() {
            return (int) ((float) _bonusPDef * (1f + (_bonusPDefPercent / 100f)));
        }
        public int GetSelfMdef() {
            return (int) ((float) _bonusMDef * (1f + (_bonusMDefPercent / 100f)));
        }
        public void ResetToFullHP() {
            AdjustHP(_maxHP);
        }
        public void ResetToFullSP() {
            AdjustSP(_maxSP);
        }
        private float GetAttackPower() {
            float statUsed = (float)Utilities.GetStatByClass(this);
            float weaponAttack = 0f;
            if(_equippedWeapon != null) {
                weaponAttack = _equippedWeapon.attackPower;
            }
            return (((weaponAttack + statUsed) * (statUsed / 2f)) * (1f + (agility / 100f))) * (1f + (level / 100f));
        }
        private float GetDefensePower() {
            return ((float)(strength + intelligence + GetSelfPdef() + GetSelfMdef() + maxHP + (vitality * 2)) * (1f + (level / 100f))) * (1f + (agility / 100f));
            //follow up questions : 
            //sa formula merong pdef at mdef pero kelangan yun ng enemy parameter, papano yun since yung computation na ito ay para ma compute ang sariling power lang mismo?
            //ano yung '(2XVIT) multiplied or added by prefix/suffix effect', wala namang ganun ang vitality.
        }
        #endregion

        #region Player/Character Actions
        public void OnThisCharacterSnatched() {
            BaseLandmark snatcherLair = PlayerManager.Instance.player.GetAvailableSnatcherLair();
            if (snatcherLair == null) {
                throw new Exception("There is not available snatcher lair!");
            } else {
                Imprison();
                ILocation location = _party.specificLocation;
                if (location != null && location.locIdentifier == LOCATION_IDENTIFIER.LANDMARK) { //if character is at a landmark
                    location.RemoveCharacterFromLocation(_party);
                }
                snatcherLair.AddCharacterToLocation(_party);
            }
        }
        private void ConstructDesperateActions() {
            _desperateActions = new List<CharacterAction>();
            _desperateActions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.BERSERK));
            _desperateActions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.SELFMUTILATE));
            _desperateActions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.FLAIL));
        }
        private void ConstructIdleActions() {
            _idleActions = new List<CharacterAction>();
            _idleActions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.DAYDREAM));
            _idleActions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.PLAY));
            _idleActions.Add(ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.CHAT));
        }
        public CharacterAction GetRandomDesperateAction(ref IObject targetObject) {
            targetObject = _party.characterObject;
            CharacterAction chosenAction = _desperateActions[Utilities.rng.Next(0, _desperateActions.Count)];
            return chosenAction;
        }
        public CharacterAction GetRandomIdleAction(ref IObject targetObject) {
            targetObject = _party.characterObject;
            CharacterAction chosenAction = _idleActions[Utilities.rng.Next(0, _idleActions.Count)];
            if (chosenAction is ChatAction) {
                List<NewParty> partyPool = new List<NewParty>();
                //priority 1
                NewParty chosenParty = GetPriority1TargetChatAction(partyPool);
                if(chosenParty == null) {
                    chosenParty = GetPriority2TargetChatAction(partyPool);
                    if (chosenParty == null) {
                        chosenParty = GetPriority3TargetChatAction(partyPool);
                    }
                }
                targetObject = chosenParty.icharacterObject;
            }
            return chosenAction;
        }
        private NewParty GetPriority1TargetChatAction(List<NewParty> partyPool) {
            partyPool.Clear();
            for (int i = 0; i < faction.characters.Count; i++) {
                NewParty party = faction.characters[i].party;
                if (party.id != this._party.id && !partyPool.Contains(party)) {
                    partyPool.Add(party);
                }
            }
            if(partyPool.Count > 0) {
                return partyPool[Utilities.rng.Next(0, partyPool.Count)];
            }
            return null;
        }
        private NewParty GetPriority2TargetChatAction(List<NewParty> partyPool) {
            partyPool.Clear();
            List<Faction> nonHostileFactions = FactionManager.Instance.GetFactionsWithByStatus(faction, FACTION_RELATIONSHIP_STATUS.NON_HOSTILE);
            for (int i = 0; i < nonHostileFactions.Count; i++) {
                Faction faction = nonHostileFactions[i];
                for (int k = 0; k < faction.characters.Count; k++) {
                    NewParty party = faction.characters[k].party;
                    if (party.id != this._party.id && !partyPool.Contains(party)) {
                        partyPool.Add(party);
                    }
                }
            }
            if (partyPool.Count > 0) {
                return partyPool[Utilities.rng.Next(0, partyPool.Count)];
            }
            return null;
        }
        private NewParty GetPriority3TargetChatAction(List<NewParty> partyPool) {
            partyPool.Clear();
            List<Faction> hostileFactions = FactionManager.Instance.GetFactionsWithByStatus(faction, FACTION_RELATIONSHIP_STATUS.HOSTILE);
            for (int i = 0; i < hostileFactions.Count; i++) {
                Faction faction = hostileFactions[i];
                for (int k = 0; k < faction.characters.Count; k++) {
                    NewParty party = faction.characters[k].party;
                    if (party.id != this._party.id && !partyPool.Contains(party)) {
                        partyPool.Add(party);
                    }
                }
            }
            if (partyPool.Count > 0) {
                return partyPool[Utilities.rng.Next(0, partyPool.Count)];
            }
            return null;
        }
        #endregion

        #region Snatch
        /*
         When the player successfully snatches a character, other characters with relation to 
         the snatched one would all be sent signals to check whether they should react or not. 
         Other character reaction would depend on their relationship, happiness and traits.
             */
        private void OnCharacterSnatched(ECS.Character otherCharacter) {
            if (otherCharacter.id != this.id && this.party.characterObject.currentState.stateName != "Imprisoned") {
                if (relationships.ContainsKey(otherCharacter)) { //if this character has a relationship with the one that was snatched
                    Debug.Log(this.name + " will react to " + otherCharacter.name + " being snatched!");
                    //For now make all characters that have relationship with the snatched character, react.
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        //obtain release character questline
                        Debug.Log(this.name + " decided to release " + otherCharacter.name + " by himself");
                        QuestManager.Instance.TakeQuest(QUEST_TYPE.RELEASE_CHARACTER, this, otherCharacter);
                    } else {
                        //bargain with player
                        Debug.Log(this.name + " will bargain for " + otherCharacter.name + "'s freedom!");
                        TriggerBargain(otherCharacter);
                    }
                }
            }
        }
        private void TriggerBargain(ECS.Character bargainingFor) {
            List<CharacterDialogChoice> dialogChoices = new List<CharacterDialogChoice>();
            CharacterDialogChoice killYourselfChoice = new CharacterDialogChoice("Kill yourself!", () => this.Death());
            List<Character> otherCharacters = new List<Character>(CharacterManager.Instance.allCharacters.Where(x => x.party.characterObject.currentState.stateName != "Imprisoned"));
            otherCharacters.Remove(this);
            dialogChoices.Add(killYourselfChoice);
            if (otherCharacters.Count > 0) {
                ECS.Character characterToAttack = otherCharacters[UnityEngine.Random.Range(0, otherCharacters.Count)];
                CharacterDialogChoice attackCharacterChoice = new CharacterDialogChoice("Attack " + characterToAttack.name, 
                    () => party.actionData.ForceDoAction(characterToAttack.party.characterObject.currentState.GetAction(ACTION_TYPE.ATTACK)
                    , characterToAttack.party.characterObject));
                dialogChoices.Add(attackCharacterChoice);
            }

            UnityEngine.Events.UnityAction onClickAction = () => Messenger.Broadcast(Signals.SHOW_CHARACTER_DIALOG, this, "Please release " + bargainingFor.name + "!", dialogChoices);

            Messenger.Broadcast<string, int, UnityEngine.Events.UnityAction>
                (Signals.SHOW_NOTIFICATION, this.name + " wants to bargain for " + bargainingFor.name + "'s freedom!",
                144,
                onClickAction);
        }
        #endregion

        #region Home
        public void LookForNewHomeStructure() {
            //Try to get a new home structure from this character's area
            StructureObj structure = GetNewHomeStructureFromArea(_home);
            if (structure != null) {
                SetHomeStructure(structure);
            } else {
                //If there is no available structure, look for it in other areas of the faction and migrate there
                structure = GetNewHomeStructureFromFaction();
                if (structure != null) {
                    SetHomeStructure(structure);
                    SetHome(structure.objectLocation.tileLocation.areaOfTile);
                } else {
                    //TODO: For future update, migrate to another friendly faction's structure
                }
            }
        }
        private StructureObj GetNewHomeStructureFromArea(Area area) {
            StructureObj chosenStructure = null;
            for (int i = 0; i < area.landmarks.Count; i++) {
                StructureObj structure = area.landmarks[i].landmarkObj;
                if (structure != _homeStructure && structure.specificObjectType == _homeStructure.specificObjectType) {
                    if (chosenStructure == null) {
                        chosenStructure = structure;
                    } else {
                        if (structure.numOfResidentCivilians < chosenStructure.numOfResidentCivilians) {
                            chosenStructure = structure;
                        }
                    }
                }
            }
            return chosenStructure;
        }
        private StructureObj GetNewHomeStructureFromFaction() {
            StructureObj chosenStructure = null;
            if (_faction != null) {
                for (int i = 0; i < _faction.ownedAreas.Count; i++) {
                    Area area = _faction.ownedAreas[i];
                    if (area.id != _home.id) {
                        chosenStructure = GetNewHomeStructureFromArea(area);
                        if (chosenStructure != null) {
                            break;
                        }
                    }
                }
            }
            return chosenStructure;
        }
        public void SetHome(Area newHome) {
            _home = newHome;
            newHome.residents.Add(this);
        }
        public void SetHomeLandmark(BaseLandmark newHomeLandmark) {
            this._homeLandmark = newHomeLandmark;
        }
        public void SetHomeStructure(StructureObj newHomeStructure) {
            if (_homeStructure != null) {
                _homeStructure.AdjustNumOfResidents(-1);
            }
            _homeStructure = newHomeStructure;
            newHomeStructure.AdjustNumOfResidents(1);
        }
        #endregion
    }
}
