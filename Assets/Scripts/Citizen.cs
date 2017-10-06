using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[System.Serializable]
public class Citizen {
	public int id;
    public string firstName;
    public string surName;
	public GENDER gender;
	public int age;
    private int _ageTableKey;
	public int generation;
	public City city;
	public HexTile currentLocation;
	public ROLE role;
	public Role assignedRole;
	public RACE race;
	public Citizen father;
	public Citizen mother;
	protected Citizen _spouse;
	public List<Citizen> children;
	public MONTH birthMonth;
	public int birthDay;
	public int birthYear;
	public bool isMarried;
	public bool isDirectDescendant;
	public bool isGovernor;
	public bool isKing;
	public bool isImmortal;
	public bool isDead;
	public DEATH_REASONS deathReason;
	public string deathReasonText;
    //private KINGDOM_TYPE _preferredKingdomType;

    //Traits
    private CHARISMA _charisma;
	private INTELLIGENCE _intelligence;
    private EFFICIENCY _efficiency;
	private SCIENCE _science;
	private MILITARY _military;
	private LOYALTY _loyalty;
    private PURPOSE _balanceType;

    private Dictionary<STATUS_EFFECTS, StatusEffect> _statusEffects;

    //King Opinion
    //A citizen's Opinion towards his King is a value between -100 to 100 representing how loyal he is towards his King.
    private int _loyaltyToKing;
    private string _loyaltySummary;
    internal int loyaltyModifierForTesting;


	[HideInInspector]public List<History> history;

	//protected Dictionary<CHARACTER_VALUE, int> _dictCharacterValues;
 //   protected Dictionary<CHARACTER_VALUE, int> _importantCharacterValues;

	#region getters/setters
    public string name {
        get { return firstName + " " + surName; }
    }
    public int ageTableKey {
        get { return _ageTableKey; }
    }
	//public Dictionary<CHARACTER_VALUE, int> dictCharacterValues{
	//	get{ return this._dictCharacterValues;}
	//}
 //   public Dictionary<CHARACTER_VALUE, int> importantCharacterValues {
 //       get { return this._importantCharacterValues; }
 //   }
    public Citizen spouse {
        get { return this._spouse; }
    }
    //internal KINGDOM_TYPE preferredKingdomType {
    //    get { return _preferredKingdomType; }
    //}
    internal CHARISMA charisma {
        get { return this._charisma; }
    }
    internal EFFICIENCY efficiency {
        get { return this._efficiency; }
    }
    internal INTELLIGENCE intelligence {
		get { return this._intelligence; }
    }
	internal SCIENCE science {
		get { return this._science; }
	}
	internal MILITARY military {
		get { return this._military; }
	}
	internal LOYALTY loyalty {
		get { return this._loyalty; }
	}
    internal PURPOSE balanceType {
        get { return this._balanceType; }
    }
    internal int loyaltyToKing {
        get { return Mathf.Clamp((_loyaltyToKing + loyaltyDeductionFromWar + GetLoyaltyFromStability()) + loyaltyModifierForTesting, -100, 100); }
    }
    internal int loyaltyDeductionFromWar {
        get { return city.kingdom.relationships.Values.Where(x => x.isAtWar).Count() * -10; }
    }
    internal string loyaltySummary {
        get { return _loyaltySummary; }
    }
    internal Dictionary<STATUS_EFFECTS, StatusEffect> statusEffects {
        get { return _statusEffects; }
    }
    #endregion

    public Citizen(City city, int age, GENDER gender, int generation){
		this.id = Utilities.SetID (this);
		this.race = city.kingdom.race;
		this.gender = gender;
		this.age = age;
        this._ageTableKey = -1;
        //this._preferredKingdomType = StoryTellingManager.Instance.GetRandomKingdomTypeForCitizen();
        if (this.race == RACE.HUMANS) {
            this.firstName = RandomNameGenerator.Instance.GetHumanFirstName(gender);
            this.surName = RandomNameGenerator.Instance.GetHumanSurname();
        } else {
            this.firstName = RandomNameGenerator.Instance.GenerateRandomName(this.race, this.gender);
        }
		this.generation = generation;
		this.city = city;
		this.role = ROLE.UNTRAINED;
		this.assignedRole = null;
		this.father = null;
		this.mother = null;
		this._spouse = null;
		this.children = new List<Citizen> ();
		this.currentLocation = this.city.hexTile;
		this.birthMonth = (MONTH) GameManager.Instance.month;
		this.birthDay = GameManager.Instance.days;
		this.birthYear = GameManager.Instance.year;
		this.isMarried = false;
		this.isDirectDescendant = false;
		this.isGovernor = false;
		this.isKing = false;
		this.isImmortal = false;
		this.isDead = false;
		this.history = new List<History>();
		this.deathReason = DEATH_REASONS.NONE;
		this.deathReasonText = string.Empty;
		//this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
  //      this._importantCharacterValues = new Dictionary<CHARACTER_VALUE, int>();

		GenerateTraits ();

        this._statusEffects = new Dictionary<STATUS_EFFECTS, StatusEffect>();

        this.city.citizens.Add (this);
    }

	internal int GetCampaignLimit(){
		return 2;
	}
    internal void AssignRole(ROLE role) {
        if (this.role != ROLE.UNTRAINED) {
            if (this.assignedRole != null) {
                this.city.RemoveCitizenInImportantCitizensInCity(this);
                this.assignedRole.OnDeath();
            }
        }

        this.role = role;
        if (role == ROLE.FOODIE) {
            this.assignedRole = new Foodie(this);
        } else if (role == ROLE.GATHERER) {
            this.assignedRole = new Gatherer(this);
        } else if (role == ROLE.MINER) {
            this.assignedRole = new Miner(this);
        } else if (role == ROLE.GENERAL) {
            this.assignedRole = new General(this);
        } else if (role == ROLE.ENVOY) {
            this.assignedRole = new Envoy(this);
        } else if (role == ROLE.GUARDIAN) {
            this.assignedRole = new Guardian(this);
        } else if (role == ROLE.SPY) {
            this.assignedRole = new Spy(this);
        } else if (role == ROLE.TRADER) {
            this.assignedRole = new Trader(this);
        } else if (role == ROLE.GOVERNOR) {
            this.assignedRole = new Governor(this);
            this.city.AddCitizenToImportantCitizensInCity(this);
        } else if (role == ROLE.KING) {
            this.assignedRole = new King(this);
            this.city.AddCitizenToImportantCitizensInCity(this);
        } else if (role == ROLE.EXPANDER) {
            this.assignedRole = new Expander(this);
        } else if (role == ROLE.RAIDER) {
            this.assignedRole = new Raider(this);
        } else if (role == ROLE.REINFORCER) {
            this.assignedRole = new Reinforcer(this);
        } else if (role == ROLE.REBEL) {
            this.assignedRole = new Rebel(this);
        } else if (role == ROLE.EXTERMINATOR) {
            this.assignedRole = new Exterminator(this);
        } else if (role == ROLE.SCOURGE) {
            this.assignedRole = new Scourge(this);
        } else if (role == ROLE.HEALER) {
            this.assignedRole = new Healer(this);
        } else if (role == ROLE.PROVOKER) {
            this.assignedRole = new Provoker(this);
        } else if (role == ROLE.MISSIONARY) {
            this.assignedRole = new Missionary(this);
        } else if (role == ROLE.ABDUCTOR) {
            this.assignedRole = new Abductor(this);
        } else if (role == ROLE.LYCANTHROPE) {
            this.assignedRole = new Lycanthrope(this);
        } else if (role == ROLE.INVESTIGATOR) {
            this.assignedRole = new Investigator(this);
        } else if (role == ROLE.THIEF) {
            this.assignedRole = new Thief(this);
        } else if (role == ROLE.WITCH) {
            this.assignedRole = new Witch(this);
        } else if (role == ROLE.ADVENTURER) {
            this.assignedRole = new Adventurer(this);
        } else if (role == ROLE.RELIEVER) {
            this.assignedRole = new Reliever(this);
        } else if (role == ROLE.INTERCEPTER) {
            this.assignedRole = new Intercepter(this);
        } else if (role == ROLE.RANGER) {
            this.assignedRole = new Ranger(this);
        } else if (role == ROLE.TREATYOFFICER) {
            this.assignedRole = new TreatyOfficer(this);
        } else if (role == ROLE.TRIBUTER) {
            this.assignedRole = new Tributer(this);
        } else if (role == ROLE.MILITARY_ALLIANCE_OFFICER) {
            this.assignedRole = new MilitaryAllianceOfficer(this);
        } else if (role == ROLE.INSTIGATOR) {
            this.assignedRole = new Instigator(this);
        } else if (role == ROLE.GRAND_CHANCELLOR) {
            this.assignedRole = new GrandChancellor(this);
            this.city.AddCitizenToImportantCitizensInCity(this);
        } else if (role == ROLE.GRAND_MARSHAL) {
            this.assignedRole = new GrandMarshal(this);
            this.city.AddCitizenToImportantCitizensInCity(this);
        } else if (role == ROLE.QUEEN) {
            this.assignedRole = null;
            this.city.AddCitizenToImportantCitizensInCity(this);
        } else if (role == ROLE.QUEEN_CONSORT) {
            this.assignedRole = null;
            this.city.AddCitizenToImportantCitizensInCity(this);
        } else if (role == ROLE.CROWN_PRINCE) {
            this.assignedRole = null;
            this.city.AddCitizenToImportantCitizensInCity(this);
        } else {
            this.assignedRole = null;
            this.role = ROLE.UNTRAINED;
        }
    }
    internal void ForceWar(Kingdom targetKingdom, GameEvent gameEventTrigger, WAR_TRIGGER warTrigger = WAR_TRIGGER.NONE) {
        //		if (this.city.kingdom.HasActiveEvent(EVENT_TYPES.INVASION_PLAN)) {
        //			return;
        //		}
        //		War warEvent = KingdomManager.Instance.GetWarBetweenKingdoms (this.city.kingdom, targetKingdom);
        //		if (warEvent != null && warEvent.isAtWar) {
        //			return;
        //		}
        //		if (warEvent == null) {
        //			warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, 
        //				this.city.kingdom, targetKingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
        //		}
        //		warEvent.CreateInvasionPlan (this.city.kingdom, gameEventTrigger);

        KingdomRelationship relationship = this.city.kingdom.GetRelationshipWithKingdom(targetKingdom);
        if (relationship != null) {
            if (!relationship.isAtWar) {
                EventCreator.Instance.CreateWarEvent(this.city.kingdom, targetKingdom);
            }
        }
    }

    #region Family Functions
    internal void AddParents(Citizen father, Citizen mother) {
        this.father = father;
        this.mother = mother;

        if (this.race == RACE.HUMANS) {
            this.firstName = RandomNameGenerator.Instance.GetHumanFirstName(this.gender);
            this.surName = this.father.surName;
        } else {
            this.firstName = RandomNameGenerator.Instance.GenerateRandomName(this.race, this.gender);
        }
    }
    internal void AddChild(Citizen child) {
        this.children.Add(child);
    }
    internal void ChangeSurname(Citizen citizenToGetSurnameFrom) {
        this.surName = citizenToGetSurnameFrom.surName;
    }
    internal void AssignSpouse(Citizen spouse) {
        this._spouse = spouse;
        this.isMarried = true;
    }
    internal void DivorceSpouse() {
        this._spouse = null;
        this.isMarried = false;
    }
    /*
     * Get relativesof this citizen.
     * If depth is set to:
     * 0 - immediate family
     * -1 - all possible relatives
     * */
    internal List<Citizen> GetRelatives(int depth = 0) {
        List<Citizen> relatives = new List<Citizen>();
        List<Citizen> immediateFamily = new List<Citizen>();
        if (this.father != null) {
            immediateFamily.Add(this.father);
        }
        if (this.mother != null) {
            immediateFamily.Add(this.mother);
        }
        if (this.spouse != null) {
            immediateFamily.Add(this.spouse);
        }
        List<Citizen> siblings = this.GetSiblings();
        immediateFamily.AddRange(siblings);
        immediateFamily.AddRange(this.children);

        relatives.AddRange(immediateFamily);

        List<Citizen> pendingCitizens = new List<Citizen>();
        pendingCitizens.AddRange(siblings);
        pendingCitizens.AddRange(this.children);
        if (depth == -1) {
            for (int i = 0; i < pendingCitizens.Count; i++) {
                Citizen currCitizen = pendingCitizens[i];
                foreach (Citizen currRelative in currCitizen.GetRelatives(0).Where(x => x.id != this.id)) {
                    if (!relatives.Contains(currRelative)) {
                        relatives.Add(currRelative);
                    }
                    if (!pendingCitizens.Contains(currRelative)) {
                        pendingCitizens.Add(currRelative);
                    }
                }
            }
        } else {
            for (int i = 0; i < depth; i++) {
                Citizen currCitizen = null;
                currCitizen = pendingCitizens.ElementAtOrDefault(i);
                if(currCitizen == null) {
                    break;
                }
                
                foreach (Citizen currRelative in currCitizen.GetRelatives(0).Where(x => x.id != this.id)) {
                    if (!relatives.Contains(currRelative)) {
                        relatives.Add(currRelative);
                    }
                    if (!pendingCitizens.Contains(currRelative)) {
                        pendingCitizens.Add(currRelative);
                    }
                }
            }
        }

        return relatives;
    }
    internal bool IsRelative(Citizen citizen) {
        if (this.mother != null && this.mother.id == citizen.id) {
            return true;
        }
        if (this.father != null && this.father.id == citizen.id) {
            return true;
        }
        List<Citizen> siblings = this.GetSiblings();
        if (siblings.Contains(citizen)) {
            return true;
        }
        if (this.children.Contains(citizen)) {
            return true;
        }
        if (this.isDirectDescendant && citizen.isDirectDescendant) {
            return true;
        }
        return false;
    }
    internal List<Citizen> GetSiblings() {
        List<Citizen> siblings = new List<Citizen>();
        if (this.mother != null) {
            if (this.mother.children != null) {
                for (int i = 0; i < this.mother.children.Count; i++) {
                    if (this.mother.children[i].id != this.id) {
                        if (!this.mother.children[i].isDead && !siblings.Contains(this.mother.children[i])) {
                            siblings.Add(this.mother.children[i]);
                        }
                    }
                }
            }
        } else {
            if (this.father != null) {
                if (this.father.children != null) {
                    for (int i = 0; i < this.father.children.Count; i++) {
                        if (this.father.children[i].id != this.id) {
                            if (!this.father.children[i].isDead && !siblings.Contains(this.father.children[i])) {
                                siblings.Add(this.father.children[i]);
                            }
                        }
                    }
                }
            }
        }

        return siblings;
    }
    #endregion

    #region Age
    internal void AssignBirthday(MONTH month, int days, int year, bool registerCitizen = true) {
        this.birthMonth = month;
        this.birthDay = days;
        this.birthYear = year;
        if (registerCitizen) {
            CitizenManager.Instance.RegisterCitizen(this);
            SchedulingManager.Instance.AddEntry((int)birthMonth, birthDay, GameManager.Instance.year, () => IncreaseAgeEveryYear());
        }
        this.history.Add(new History((int)month, days, year, this.name + " was born.", HISTORY_IDENTIFIER.NONE));
    }
    internal void IncreaseAgeEveryYear() {
        if (!isDead) {
            AdjustAge(1);
            CitizenManager.Instance.RemoveCitizenFromAgeTable(this);
            CitizenManager.Instance.AddCitizenToAgeTable(this);
            //reschedule bday
            SchedulingManager.Instance.AddEntry((int)birthMonth, birthDay, GameManager.Instance.year + 1, () => IncreaseAgeEveryYear());
        }
    }
    internal void AdjustAge(int adjustment) {
        this.age += adjustment;
        /*Every birthday starting at 16 up to 50, an unmarried King, 
         * Queen or Governor has a chance to get married. 
         * A randomly generated character will be created.
         * */
        if (this.role == ROLE.KING || this.role == ROLE.GOVERNOR) {
            if (this.age >= 16 && this.age <= 50 && !this.isMarried) {
                int chance = Random.Range(0, 100);
                if (chance < 8) {
                    Citizen spouse = MarriageManager.Instance.GenerateSpouseForCitizen(this);
                    MarriageManager.Instance.Marry(this, spouse);
                }
            }
        }
    }
    internal void SetAgeTableKey(int key) {
        _ageTableKey = key;
    }
    #endregion

    #region Death
    internal void Death(DEATH_REASONS reason, bool isDethroned = false, Citizen newKing = null, bool isConquered = false) {
        if (!this.isDead) {
            DeathCoroutine(reason, isDethroned, newKing, isConquered);
            Messenger.Broadcast("UpdateUI");
        }
    }
    internal void DeathCoroutine(DEATH_REASONS reason, bool isDethroned = false, Citizen newKing = null, bool isConquered = false) {
        try {
            Debug.Log("DEATH: " + this.role.ToString() + " " + this.name + " of " + this.city.name + ": " + reason.ToString());
        }catch(System.Exception e) {
            throw new System.Exception(this.role.ToString() + " " + this.name + " " + this.city + " " + reason + "\n" + e.Message);
        }
        
        DeathHistory(reason);
        this.deathReason = reason;
        this.isDead = true;
        if (this is Spouse) {
            ((Spouse)this).isAbducted = false;
        }
        if (this.city != null) {
            this.city.kingdom.RemoveFromSuccession(this);
        }
        ROLE previousRole = this.role;
        if (this.assignedRole != null) {
            this.assignedRole.OnDeath();
            this.assignedRole = null;
        }
        CitizenManager.Instance.UnregisterCitizen(this);

        if (this.city != null) {
            //this.city.RemoveCitizenFromCity(this);
            this.city.citizens.Remove(this);
            this.city.RemoveCitizenInImportantCitizensInCity(this);
            if (!isConquered) { //Check if citizen died of natural causes and not from conquering
                //if citizen is a grand marshal or grand chancellor, also remove family from city and generate new marshal/chancellor family
                if (previousRole == ROLE.GRAND_CHANCELLOR || previousRole == ROLE.GRAND_MARSHAL || previousRole == ROLE.GOVERNOR) {
                    List<Citizen> family = this.GetRelatives(-1);
                    for (int i = 0; i < family.Count; i++) {
                        this.city.RemoveCitizenFromCity(family[i]);
                    }
                }
                if (!this.city.isDead) {
                    if (previousRole == ROLE.GRAND_CHANCELLOR) {
                        this.city.CreateInitialChancellorFamily();
                    } else if (previousRole == ROLE.GRAND_MARSHAL) {
                        this.city.CreateInitialMarshalFamily();
                    }
                }
            }
        }

        if (this.id == this.city.kingdom.king.id && !this.city.kingdom.isDead) {
            //ASSIGN NEW LORD, SUCCESSION
            //			this.city.kingdom.AdjustExhaustionToAllRelationship(10);
            if (isDethroned) {
                if (newKing != null) {
                    this.city.kingdom.AssignNewKing(newKing);
                }
            } else {
                if (!isConquered) {
                    if (this.city.kingdom.successionLine.Count <= 0) {
                        this.city.kingdom.AssignNewKing(null);
                        //Remove family of previous king
                        List<Citizen> family = this.GetRelatives(-1);
                        for (int i = 0; i < family.Count; i++) {
                            this.city.RemoveCitizenFromCity(family[i]);
                        }
                    } else {
                        this.city.kingdom.AssignNewKing(this.city.kingdom.successionLine[0]);
                    }
                }
            }
        } else {
            if (this.city.governor.id == this.id && !this.city.isDead) {
                this.city.AssignNewGovernor();
            }
        }

        if (this.isMarried && this._spouse != null) {
            //MarriageManager.Instance.DivorceCouple(this, spouse);
            if (previousRole == ROLE.KING) {
                //Spouse of king should no longer be queen
                this.city.RemoveCitizenInImportantCitizensInCity(_spouse);
            }
            this.isMarried = false;
            this._spouse.isMarried = false;
            this._spouse.AssignSpouse(null);
            this.AssignSpouse(null);
        }

        this.isKing = false;
        this.isGovernor = false;

        if(previousRole == ROLE.KING) {
            UIManager.Instance.UpdateChooseCitizensMenuForIncurableDisease();
        }
    }
    internal void DeathHistory(DEATH_REASONS reason) {
        switch (reason) {
            case DEATH_REASONS.OLD_AGE:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died of natural causes.", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "died of natural causes";
                break;
            case DEATH_REASONS.ACCIDENT:
                string selectedAccidentCause = Utilities.accidentCauses[UnityEngine.Random.Range(0, Utilities.accidentCauses.Length)];
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died " + selectedAccidentCause, HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "died " + selectedAccidentCause;
                break;
            case DEATH_REASONS.BATTLE:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died in battle.", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "died in battle";
                break;
            case DEATH_REASONS.TREACHERY:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " hanged for treason.", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "was hanged for a treason";
                break;
            case DEATH_REASONS.ASSASSINATION:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died from an assassin's arrow.", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "died from an assassin's arrow";
                break;
            case DEATH_REASONS.REBELLION:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " hanged by an usurper.", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "was hanged by an usurper";
                break;
            case DEATH_REASONS.INTERNATIONAL_WAR:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died at the hands of a foreign enemy.", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "died at the hands of a foreign enemy";
                break;
            case DEATH_REASONS.STARVATION:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died of starvation.", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "died of starvation";
                break;
            case DEATH_REASONS.DISAPPEARED_EXPANSION:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " disappeared during an expedition and is assumed to be dead", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "disappeared during an expedition and is assumed to be dead";
                break;
            case DEATH_REASONS.PLAGUE:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " succumbed to the plague", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "succumbed to the plague";
                break;
            case DEATH_REASONS.SUICIDE:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " drank poison", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = "drank poison";
                break;
            case DEATH_REASONS.SERUM_OF_ALACRITY:
                this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died due to the serum of alacrity injection", HISTORY_IDENTIFIER.NONE));
                this.deathReasonText = " died due to the serum of alacrity injection";
                break;
        }
    }
    internal void SetImmortality(bool state) {
        this.isImmortal = state;
        if (this.assignedRole != null) {
            if (this.assignedRole.avatar != null) {
                if (this.assignedRole.avatar.GetComponent<Collider2D>() != null) {
                    this.assignedRole.avatar.GetComponent<Collider2D>().enabled = !state;
                } else {
                    this.assignedRole.avatar.GetComponentInChildren<Collider2D>().enabled = !state;
                }
            }
        }
    }
    #endregion

    #region Character Value Functions
    //internal void GenerateCharacterValues() {
    //    this._dictCharacterValues.Clear();
    //    this._dictCharacterValues = System.Enum.GetValues(typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE>().ToDictionary(x => x, x => UnityEngine.Random.Range(1, 101));
    //    //        this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);
    //    UpdateCharacterValues();
    //    //		CHARACTER_VALUE[] character = System.Enum.GetValues (typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE> ().ToArray ();
    //    //		this._characterValues = new CharacterValue[character.Length];
    //    //		for(int i = 0; i < this._characterValues.Length; i++){
    //    //			this._characterValues [i].character = character [i];
    //    //			this._characterValues [i].value = UnityEngine.Random.Range (1, 101);
    //    //			this._dictCharacterValues.Add(this._characterValues [i].character, this._characterValues [i].value);
    //    //		}
    //}
    //internal void UpdateCharacterValues() {
    //    for (int i = 0; i < this.city.kingdom.kingdomTypeData.characterValues.Length; i++) {
    //        this.UpdateSpecificCharacterValue(this.city.kingdom.kingdomTypeData.characterValues[i].character, this.city.kingdom.kingdomTypeData.characterValues[i].value);
    //    }
    //    this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(4).ToDictionary(x => x.Key, x => x.Value);
    //}
    //internal void ChangeCharacterValues(Dictionary<CHARACTER_VALUE, int> newCharacterValues) {
    //    this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>(newCharacterValues);
    //    this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(4).ToDictionary(x => x.Key, x => x.Value);
    //}
    //private void UpdateSpecificCharacterValue(CHARACTER_VALUE key, int value) {
    //    if (this._dictCharacterValues.ContainsKey(key)) {
    //        this._dictCharacterValues[key] += value;
    //        //			UpdateCharacterValueByKey(key, value);
    //    }
    //}
    //private void UpdateCharacterValueByKey(CHARACTER_VALUE key, int value){
    //	for(int i = 0; i < this._characterValues.Length; i++){
    //		if(this._characterValues[i].character == key){
    //			this._characterValues [i].value += value;
    //			break;
    //		}
    //	}
    //}
    //internal int GetCharacterValueOfType(CHARACTER_VALUE characterValue) {
    //    if (this._dictCharacterValues.ContainsKey(characterValue)) {
    //        return this._dictCharacterValues[characterValue];
    //    }
    //    return 0;
    //}
    #endregion

    #region Prestige
    internal int GetPrestigeContribution() {
        if(role != ROLE.GOVERNOR && role != ROLE.KING) {
            return 0;
        } else {
			switch (this._charisma) {
            case CHARISMA.CHARISMATIC:
                if (role == ROLE.KING) {
                    return 10;
                } else if (role == ROLE.GOVERNOR) {
                    return 2;
                }
                break;
            case CHARISMA.NEUTRAL:
                if (role == ROLE.KING) {
                    return 7;
                } else if (role == ROLE.GOVERNOR) {
                    return 1;
                }
                break;
            case CHARISMA.REPULSIVE:
                if (role == ROLE.KING) {
                    return 5;
                }
                break;
            }
            return 0;
        }
    }
    #endregion

    #region Expansion
    internal int GetExpansionRateContribution() {
        if (role != ROLE.GOVERNOR && role != ROLE.KING) {
            return 0;
        } else {
            switch (_charisma) {
                case CHARISMA.CHARISMATIC:
                    if (role == ROLE.KING) {
                        return 10;
                    } else if (role == ROLE.GOVERNOR) {
                        return 2;
                    }
                    break;
                case CHARISMA.NEUTRAL:
                    if (role == ROLE.KING) {
                        return 7;
                    } else if (role == ROLE.GOVERNOR) {
                        return 1;
                    }
                    break;
                case CHARISMA.REPULSIVE:
                    if (role == ROLE.KING) {
                        return 5;
                    }
                    break;
            }
            return 0;
        }
    }
    #endregion

    #region Tech
    internal int GetTechContribution() {
        if (role != ROLE.GOVERNOR && role != ROLE.KING) {
            return 0;
        } else {
            int baseValue = 3;
            if(role == ROLE.GOVERNOR) {
                baseValue = 1;
            }
            switch (_intelligence) {
                case INTELLIGENCE.SMART:
                    if (role == ROLE.KING) {
                        return baseValue + 2;
                    } else if (role == ROLE.GOVERNOR) {
                        return baseValue + 1;
                    }
                    break;
                case INTELLIGENCE.NEUTRAL:
                    return baseValue;
                case INTELLIGENCE.DUMB:
                    if (role == ROLE.KING) {
                        return baseValue - 2;
                    } else if (role == ROLE.GOVERNOR) {
                        return baseValue - 1;
                    }
                    break;
            }
            return 0;
        }
    }
    #endregion

    #region Stability
    internal int GetStabilityContribution() {
        if (role != ROLE.GOVERNOR && role != ROLE.KING) {
            return 0;
        } else {
            if(role == ROLE.GOVERNOR) {
                if(loyaltyToKing < 0) {
                    return -1;
                }
            }

            int baseValue = 3;
            if (role == ROLE.GOVERNOR) {
                baseValue = 1;
            }
            switch (_efficiency) {
                case EFFICIENCY.EFFICIENT:
                    if (role == ROLE.KING) {
                        return baseValue + 2;
                    } else if (role == ROLE.GOVERNOR) {
                        return baseValue + 1;
                    }
                    break;
                case EFFICIENCY.NEUTRAL:
                    return baseValue;
                case EFFICIENCY.INEPT:
                    if (role == ROLE.KING) {
                        return baseValue - 2;
                    } else if (role == ROLE.GOVERNOR) {
                        return baseValue - 1;
                    }
                    break;
            }
            return 0;
        }
    }
    #endregion

    #region King Opinion
    internal void UpdateKingOpinion() {
        _loyaltyToKing = 0;
        _loyaltySummary = string.Empty;

        Citizen king = city.kingdom.king;

        if(king.id == this.id) {
            return;
        }

        ////Per Active War
        //int disloyaltyFromWar = 0;
        //foreach(KingdomRelationship kr in city.kingdom.relationships.Values) {
        //    if (kr.isAtWar) {
        //        disloyaltyFromWar -= 10;
        //    }
        //}
        //_loyaltyToKing += disloyaltyFromWar;
        //if(disloyaltyFromWar != 0) {
        //    _loyaltySummary += disloyaltyFromWar.ToString() + "  Active Wars\n"; 
        //}

        //int numOfSharedValues = 0;
        //for (int i = 0; i < _importantCharacterValues.Keys.Count; i++) {
        //    CHARACTER_VALUE currKey = _importantCharacterValues.Keys.ElementAt(i);
        //    if (king.importantCharacterValues.ContainsKey(currKey)) {
        //        numOfSharedValues += 1;
        //    }
        //}

        ////Shared Values
        //int sharedValuesAdjustment = 0;
        //if (numOfSharedValues >= 3) {
        //    sharedValuesAdjustment = 50;
        //    _loyaltySummary += "+" + sharedValuesAdjustment.ToString() + "  Shared Values\n";
        //} else if (numOfSharedValues == 2) {
        //    sharedValuesAdjustment = 25;
        //    _loyaltySummary += "+" + sharedValuesAdjustment.ToString() + "  Shared Values\n";
        //} else if (numOfSharedValues == 1) {
        //    sharedValuesAdjustment = 15;
        //    _loyaltySummary += "+" + sharedValuesAdjustment.ToString() + "  Shared Values\n";
        //} else {
        //    //No shared values
        //    sharedValuesAdjustment = -30;
        //    _loyaltySummary += sharedValuesAdjustment.ToString() + "  No Shared Values\n";
        //}
        //_loyaltyToKing += sharedValuesAdjustment;

        ////Values
        //int valuesAdjustment = 0;
        //if (_importantCharacterValues.ContainsKey(CHARACTER_VALUE.HONOR)) {
        //    valuesAdjustment = 30;
        //    _loyaltyToKing += valuesAdjustment;
        //    _loyaltySummary += "+" + valuesAdjustment.ToString() + "  Values Honor\n";
        //}
        //if (_importantCharacterValues.ContainsKey(CHARACTER_VALUE.INFLUENCE)) {
        //    valuesAdjustment = -30;
        //    _loyaltyToKing += valuesAdjustment;
        //    _loyaltySummary += valuesAdjustment.ToString() + "  Values Influence\n";
        //}

        //Marriage
        int marriageAdjustment = 0;
        if(spouse != null && spouse.id == king.id && this is Spouse) {
            if(((Spouse)this)._marriageCompatibility > 0) {
                marriageAdjustment = 25;
            } else if(((Spouse)this)._marriageCompatibility < 0) {
                marriageAdjustment = -25;
            }
            if(marriageAdjustment != 0) {
                _loyaltyToKing += marriageAdjustment;
                if (marriageAdjustment > 0) {
                    _loyaltySummary += "+";
                }
                _loyaltySummary += marriageAdjustment.ToString() + "  Marriage\n";
            }
            
        }

        //Charisma
        int charismaAdjustment = 0;
        string charismaSummary = string.Empty;
        if(king.charisma == CHARISMA.REPULSIVE) {
            charismaAdjustment = -15;
            charismaSummary = charismaAdjustment.ToString() + "  Repulsed\n";
        } else if (king.charisma == CHARISMA.CHARISMATIC) {
            charismaAdjustment = 15;
            charismaSummary = charismaAdjustment.ToString() + "  Charmed\n";
        }
        if(charismaAdjustment != 0) {
            _loyaltyToKing += charismaAdjustment;
            if (charismaAdjustment > 0) {
                _loyaltySummary += "+";
            }
            _loyaltySummary += charismaSummary;
        }

        //Military
        int militaryTraitAdjustment = 0;
        string militarySummary = string.Empty;
        if(_military != MILITARY.NEUTRAL && king.military != MILITARY.NEUTRAL) {
            if (_military == king.military) {
                militaryTraitAdjustment = 15;
                militarySummary = militaryTraitAdjustment.ToString() + "  Both " + Utilities.NormalizeString(_military.ToString()) + "\n";
            } else if((_military == MILITARY.HOSTILE && king.military == MILITARY.PACIFIST) ||
                (_military == MILITARY.PACIFIST && king.military == MILITARY.HOSTILE)) {
                militaryTraitAdjustment = -15;
                militarySummary = militaryTraitAdjustment.ToString() + "  Dislikes " + Utilities.NormalizeString(king.military.ToString())+ "\n";
            }
            if (militaryTraitAdjustment != 0) {
                _loyaltyToKing += militaryTraitAdjustment;
                if (militaryTraitAdjustment > 0) {
                    _loyaltySummary += "+";
                }
                _loyaltySummary += militarySummary;
            }
        }

        //Science
        int scienceTraitAdjustment = 0;
        string scienceSummary = string.Empty;
        if (_science != SCIENCE.NEUTRAL && king.science != SCIENCE.NEUTRAL) {
            if (_science == king.science) {
                scienceTraitAdjustment = 15;
                scienceSummary = scienceTraitAdjustment.ToString() + "  Both " + Utilities.NormalizeString(_science.ToString()) + "\n";
            } else if ((_science == SCIENCE.ERUDITE && king.science == SCIENCE.IGNORANT) ||
                (_science == SCIENCE.IGNORANT && king.science == SCIENCE.ERUDITE)) {
                scienceTraitAdjustment = -15;
                scienceSummary = scienceTraitAdjustment.ToString() + "  Dislikes " + Utilities.NormalizeString(king.science.ToString()) + "\n";
            }
            if (scienceTraitAdjustment != 0) {
                _loyaltyToKing += scienceTraitAdjustment;
                if (scienceTraitAdjustment > 0) {
                    _loyaltySummary += "+";
                }
                _loyaltySummary += scienceSummary;
            }
        }

        //Intelligence
        int intelligenceAdjustment = 0;
        string intelligenceSummary = string.Empty;
        if(king.intelligence != INTELLIGENCE.NEUTRAL && _intelligence != INTELLIGENCE.NEUTRAL) {
            if (king.intelligence == _intelligence) {
                intelligenceAdjustment = 15;
                intelligenceSummary = intelligenceAdjustment.ToString() + "  Both " + Utilities.NormalizeString(_intelligence.ToString()) + "\n";
            } else {
                intelligenceAdjustment = -15;
                intelligenceSummary = intelligenceAdjustment.ToString() + "  Dislikes " + Utilities.NormalizeString(king.intelligence.ToString()) + "\n";
            }
            if (intelligenceAdjustment != 0) {
                _loyaltyToKing += intelligenceAdjustment;
                if (intelligenceAdjustment > 0) {
                    _loyaltySummary += "+";
                }
                _loyaltySummary += intelligenceSummary;
            }
        }
        

        //Efficiency
        int efficiencyAdjustment = 0;
        string efficiencySummary = string.Empty;
        if (king.efficiency != EFFICIENCY.NEUTRAL && _efficiency != EFFICIENCY.NEUTRAL) {
            if (king.efficiency == _efficiency) {
                efficiencyAdjustment = 15;
                efficiencySummary = efficiencyAdjustment.ToString() + "  Both " + Utilities.NormalizeString(_efficiency.ToString()) + "\n";
            } else {
                efficiencyAdjustment = -15;
                efficiencySummary = efficiencyAdjustment.ToString() + "  Dislikes " + Utilities.NormalizeString(king.efficiency.ToString()) + "\n";
            }
            if (efficiencyAdjustment != 0) {
                _loyaltyToKing += efficiencyAdjustment;
                if (efficiencyAdjustment > 0) {
                    _loyaltySummary += "+";
                }
                _loyaltySummary += efficiencySummary;
            }
        }

        //Loyalty
        int loyaltyAdjustment = 0;
        if(_loyalty == LOYALTY.SCHEMING) {
            loyaltyAdjustment = -30;
            _loyaltyToKing += loyaltyAdjustment;
            _loyaltySummary += loyaltyAdjustment.ToString() + "  Scheming\n";
        } else if (_loyalty == LOYALTY.LOYAL) {
            loyaltyAdjustment = 30;
            _loyaltyToKing += loyaltyAdjustment;
            _loyaltySummary += "+" + loyaltyAdjustment.ToString() + "  Loyal\n";
        }

        //Balance Type
        int balanceTypeAdjustment = 0;
        if(king.balanceType == _balanceType) {
            balanceTypeAdjustment = 30;
            _loyaltyToKing += balanceTypeAdjustment;
            _loyaltySummary += "+" + balanceTypeAdjustment.ToString() + "  Same International Policy\n";
        } else {
            balanceTypeAdjustment = -30;
            _loyaltyToKing += balanceTypeAdjustment;
            _loyaltySummary += balanceTypeAdjustment.ToString() + "  Different International Policy\n";
        }

        _loyaltyToKing = Mathf.Clamp(_loyaltyToKing, -100, 100);
    }
    internal int GetLoyaltyFromStability() {
        return city.kingdom.stability / 5;
    }
    #endregion

    #region Status Effects
    private StatusEffect CreateNewStatusEffectForCitizen(STATUS_EFFECTS statusEffectType) {
        if(statusEffectType == STATUS_EFFECTS.INCURABLE_DISEASE) {
            return new IncurableDisease(this);
        }
        return null;
    }
    internal void AddStatusEffect(STATUS_EFFECTS statusEffect) {
        if (!_statusEffects.ContainsKey(statusEffect)) {
            _statusEffects.Add(statusEffect, CreateNewStatusEffectForCitizen(statusEffect));
            Debug.Log(this.role.ToString() + " " + this.name + " is now afflicted with " + statusEffect.ToString());
        }
    }
    internal void RemoveStatusEffect(STATUS_EFFECTS statusEffect) {
        _statusEffects.Remove(statusEffect);
    }
    #endregion

    #region Rebellion
    internal void StartRebellion() {
        Kingdom sourceKingdom = city.kingdom;
        List<City> citiesForRebellion = new List<City>();
        List<City> citiesLeftInSourceKingdom = new List<City>(sourceKingdom.cities);

        if (this.role == ROLE.GOVERNOR) {
            //If Citizen to rebel is a governor, automatically add his/her owned city to cities that will rebel
            citiesForRebellion.Add(this.city);
        } else {
            //Get a random origin city that is not the capital city, where the rebellion will originate from
            List<City> citiesToChooseFrom = new List<City>(sourceKingdom.cities);
            citiesToChooseFrom.Remove(sourceKingdom.capitalCity);
            City chosenOriginCity = citiesToChooseFrom[Random.Range(0, citiesToChooseFrom.Count)];
            citiesLeftInSourceKingdom.Remove(chosenOriginCity);
            citiesForRebellion.Add(chosenOriginCity);
        }

        int maxNumOfCitiesForRebellion = Mathf.Min(sourceKingdom.cities.Count, 5);
        int numOfCitiesForRebellion = Random.Range(2, maxNumOfCitiesForRebellion);
        while(citiesForRebellion.Count < numOfCitiesForRebellion) {
            //Make sure that the new kingdom has 2 to 5 cities
            for (int i = 0; i < citiesForRebellion.Count; i++) {
                City currCity = citiesForRebellion[i];
                for (int j = 0; j < currCity.region.adjacentRegions.Count; j++) {
                    //loop through the occupied adjacent regions of cities for rebellion
                    City adjacentRegionOccupant = currCity.region.adjacentRegions[j].occupant;
                    if(adjacentRegionOccupant != null && adjacentRegionOccupant.kingdom.id == sourceKingdom.id) {
                        if (!citiesForRebellion.Contains(adjacentRegionOccupant)) {
                            citiesLeftInSourceKingdom.Remove(adjacentRegionOccupant);
                            citiesForRebellion.Add(adjacentRegionOccupant);
                            if(citiesForRebellion.Count >= numOfCitiesForRebellion) {
                                break;
                            }
                        }
                    }
                }
                if (citiesForRebellion.Count >= numOfCitiesForRebellion) {
                    break;
                }
            }
            if(citiesForRebellion.Count < numOfCitiesForRebellion) {
                if(citiesLeftInSourceKingdom.Count <= 1) {
                    //There are no more cities that the source kingdom can give
                    break;
                }
                City closestCity = null;
                float nearestDistance = 99999f;
                for (int i = 0; i < citiesLeftInSourceKingdom.Count; i++) {
                    City currCity = citiesLeftInSourceKingdom[i];
                    for (int j = 0; j < citiesForRebellion.Count; j++) {
                        City cityInRebellion = citiesForRebellion[j];
                        float distance = cityInRebellion.region.centerOfMass.GetDistanceTo(currCity.region.centerOfMass);
                        if(distance < nearestDistance) {
                            nearestDistance = distance;
                            closestCity = currCity;
                        }
                    }
                }
                if(closestCity != null) {
                    citiesForRebellion.Add(closestCity);
                }
            }
        }

        ROLE previousRole = this.role;
        City previousCity = this.city;
        Kingdom newKingdom = KingdomManager.Instance.GenerateNewKingdom(sourceKingdom.race, new List<HexTile>() { }, false, sourceKingdom, true, this);
        KingdomManager.Instance.TransferCitiesToOtherKingdom(sourceKingdom, newKingdom, citiesForRebellion);
        newKingdom.HighlightAllOwnedTilesInKingdom();

        //Transfer family of citizen to the capital city of the new kingdom
        //if this citizen was a crown prince, bring his/her spouse and children,
        //if this citizen was a queen, do not bring any family,
        //if this citizen was a governor, bring all family members
        List<Citizen> citizensToTransfer = new List<Citizen>();
        citizensToTransfer.Add(this);
        if (previousRole == ROLE.CROWN_PRINCE) {
            if(spouse != null) {
                citizensToTransfer.Add(spouse);
            }
            if(children != null) {
                citizensToTransfer.AddRange(children);
            }
        } else if (previousRole == ROLE.QUEEN || previousRole == ROLE.QUEEN_CONSORT) {
            if(_spouse != null) {
                MarriageManager.Instance.DivorceCouple(this, _spouse);
            }
        } else if (previousRole == ROLE.GOVERNOR) {
            citizensToTransfer.AddRange(GetRelatives(-1));
        }


        for (int i = 0; i < citizensToTransfer.Count; i++) {
            Citizen currCitizen = citizensToTransfer[i];
            previousCity.RemoveCitizenInImportantCitizensInCity(currCitizen);
            previousCity.citizens.Remove(currCitizen);
            //previousCity.RemoveCitizenFromCity(currCitizen);
            newKingdom.capitalCity.AddCitizenToCity(currCitizen);
        }

        newKingdom.capitalCity.CreateInitialChancellorFamily();
        newKingdom.capitalCity.CreateInitialMarshalFamily();

        newKingdom.UpdateKingSuccession();

        newKingdom.UpdateAllRelationshipsLikeness();
        newKingdom.UpdateAllCitizensOpinionOfKing();

        //Transfer population from sourceKingdom
        int totalCities = newKingdom.cities.Count + sourceKingdom.cities.Count;
        float percentGained = ((float) newKingdom.cities.Count / (float)totalCities);
        int populationToTransfer = Mathf.FloorToInt((float)sourceKingdom.population * percentGained);
        sourceKingdom.AdjustPopulation(-populationToTransfer);
        newKingdom.SetPopulation(populationToTransfer);

        int weaponsGained = Mathf.FloorToInt((float)sourceKingdom.baseWeapons * percentGained);
        sourceKingdom.AdjustBaseWeapons(-weaponsGained);
        newKingdom.SetBaseWeapons(weaponsGained);

        int armorGained = Mathf.FloorToInt((float)sourceKingdom.baseArmor * percentGained);
        sourceKingdom.AdjustBaseArmors(-armorGained);
        newKingdom.SetBaseArmor(armorGained);

        //Once a rebellion is declared, set source Kingdom's Stability back to 50
        sourceKingdom.ChangeStability(50);

        CameraMove.Instance.UpdateMinimapTexture();

        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "rebellion");
        newLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        newLog.AddToFillers(sourceKingdom, sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(newKingdom, newKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        UIManager.Instance.ShowNotification(newLog);

        Warfare warfare = new Warfare(newKingdom, sourceKingdom);
        Debug.Log("Rebelling kingdom " + newKingdom.name + " declares war on " + sourceKingdom.name);
    }
    #endregion

	private void GenerateTraits(){
		this._charisma = (CHARISMA)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(CHARISMA)).Length));
		this._efficiency = (EFFICIENCY)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(EFFICIENCY)).Length));
		this._intelligence = (INTELLIGENCE)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(INTELLIGENCE)).Length));
		this._science = (SCIENCE)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(SCIENCE)).Length));
		this._military = (MILITARY)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(MILITARY)).Length));
		this._loyalty = (LOYALTY)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(LOYALTY)).Length));
        this._balanceType = (PURPOSE)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(PURPOSE)).Length));
    }
}
