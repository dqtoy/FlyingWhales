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
    private bool _isGovernor;
	private bool _isKing;
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
	private WARMONGER _warmonger;

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
        get {
            if(race == RACE.HUMANS) {
                return firstName + " " + surName;
            } else {
                return firstName;
            }
        }
    }
    public int ageTableKey {
        get { return _ageTableKey; }
    }
    public Citizen spouse {
        get { return this._spouse; }
    }
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
	internal WARMONGER warmonger {
		get { return this._warmonger; }
	}
    internal int loyaltyToKing {
        get { return Mathf.Clamp((_loyaltyToKing + loyaltyDeductionFromWar + GetLoyaltyFromStability()) + loyaltyModifierForTesting, -100, 100); }
    }
    internal int loyaltyDeductionFromWar {
        get { return city.kingdom.warfareInfo.Count() * -10; }
    }
    internal string loyaltySummary {
        get { return _loyaltySummary; }
    }
    internal Dictionary<STATUS_EFFECTS, StatusEffect> statusEffects {
        get { return _statusEffects; }
    }
    internal bool isKing {
        get { return (role == ROLE.KING && assignedRole is King); }
    }
    internal bool isGovernor {
        get { return (role == ROLE.GOVERNOR && assignedRole is Governor); }
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
		this.isImmortal = false;
		this.isDead = false;
		this.history = new List<History>();
		this.deathReason = DEATH_REASONS.NONE;
		this.deathReasonText = string.Empty;
		//this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
  //      this._importantCharacterValues = new Dictionary<CHARACTER_VALUE, int>();

		GenerateTraits ();

        this._statusEffects = new Dictionary<STATUS_EFFECTS, StatusEffect>();

        //this.city.citizens.Add (this);
    }

	internal int GetCampaignLimit(){
		return 2;
	}
    internal void AssignRole(ROLE role) {
        if (this.role != ROLE.UNTRAINED && this.role != role) {
            if (this.assignedRole != null) {
                this.assignedRole.OnDeath();
            }
        }

        this.role = role;
 		if (role == ROLE.GENERAL) {
            this.assignedRole = new General(this);
        } else if (role == ROLE.GOVERNOR) {
            this.assignedRole = new Governor(this);
        } else if (role == ROLE.KING) {
            this.assignedRole = new King(this);
        } else if (role == ROLE.EXPANDER) {
            this.assignedRole = new Expander(this);
        } else if (role == ROLE.GRAND_CHANCELLOR) {
            this.assignedRole = new GrandChancellor(this);
        } else if (role == ROLE.GRAND_MARSHAL) {
            this.assignedRole = new GrandMarshal(this);
        } else {
            this.assignedRole = null;
        }
    }

    #region Family Functions
    internal void CreateFamily() {
        Citizen father = new Citizen(this.city, UnityEngine.Random.Range(60, 81), GENDER.MALE, 1);
        Citizen mother = new Citizen(this.city, UnityEngine.Random.Range(60, 81), GENDER.FEMALE, 1);

        MONTH monthFather = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthMother = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        father.AssignBirthday(monthFather, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age, false);
        mother.AssignBirthday(monthMother, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age, false);

        father.isDirectDescendant = isDirectDescendant;
        mother.isDirectDescendant = isDirectDescendant;
        father.isDead = true;
        mother.isDead = true;

        father.AddChild(this);
        mother.AddChild(this);
        this.AddParents(father, mother);
        
        MarriageManager.Instance.Marry(father, mother);

        MONTH monthSibling = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));
        MONTH monthSibling2 = (MONTH)(UnityEngine.Random.Range(1, System.Enum.GetNames(typeof(MONTH)).Length));

        int siblingsChance = UnityEngine.Random.Range(0, 100);
        if (siblingsChance < 25) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, this.age));
            Citizen sibling2 = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, this.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            sibling2.AssignBirthday(monthSibling2, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling2] + 1), (GameManager.Instance.year - sibling2.age));
            this.city.kingdom.AddCitizenToKingdom(sibling, this.city);
            this.city.kingdom.AddCitizenToKingdom(sibling2, this.city);
            sibling.UpdateKingOpinion();
            sibling2.UpdateKingOpinion();
        } else if (siblingsChance >= 25 && siblingsChance < 75) {
            Citizen sibling = MarriageManager.Instance.MakeBaby(father, mother, UnityEngine.Random.Range(0, this.age));
            sibling.AssignBirthday(monthSibling, UnityEngine.Random.Range(1, GameManager.daysInMonth[(int)monthSibling] + 1), (GameManager.Instance.year - sibling.age));
            this.city.kingdom.AddCitizenToKingdom(sibling, this.city);
            sibling.UpdateKingOpinion();
        }

        int spouseChance = UnityEngine.Random.Range(0, 100);
        if (spouseChance < 80) {
            Citizen spouse = MarriageManager.Instance.CreateSpouse(this);
            this.city.kingdom.AddCitizenToKingdom(spouse, this.city);
            spouse.UpdateKingOpinion();
        }
    }
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
    //internal void Death(DEATH_REASONS reason) {
    //    if (!this.isDead) {
    //        DeathCoroutine(reason);
    //        Messenger.Broadcast("UpdateUI");
    //    }
    //}
    internal void Death(DEATH_REASONS reason, bool isConquered = false) {
        Debug.Log("DEATH: " + this.role.ToString() + " " + this.name + " of " + this.city.name + ": " + reason.ToString());

        isDead = true;

        //Remove Citizen From City First
        City cityOfCitizen = this.city;
        Kingdom kingdomOfCitizen = this.city.kingdom;
        kingdomOfCitizen.RemoveCitizenFromKingdom(this, cityOfCitizen);

        //Manage Citizen Inheritance
        kingdomOfCitizen.RemoveFromSuccession(this);

        //Perform Role On Death Functions
        //NOTE: If the citizen was a king/governor/chancellor/marshall the replacement of political figures will occur here.
        //Do not perform succession when citizen died because of conquering, this means the kings cannot be relocated any more
        if (!isConquered) {
            if (this.assignedRole != null) {
                this.assignedRole.OnDeath(); //Refer to here, when changing succession
                this.assignedRole = null;
            }
        }

        //Manage Citizen Marriage
        if (this.isMarried && this._spouse != null) {
            MarriageManager.Instance.DivorceCouple(this, spouse);
        }


        //Unregister citizen from the manager (The manager handles random deaths)
        CitizenManager.Instance.UnregisterCitizen(this);

        Messenger.Broadcast("UpdateUI");
        //DeathHistory(reason);
        //this.deathReason = reason;
        //this.isDead = true;
        //if (this is Spouse) {
        //    ((Spouse)this).isAbducted = false;
        //}
        //if (this.city != null) {
        //    this.city.kingdom.RemoveFromSuccession(this);
        //}
        //ROLE previousRole = this.role;
        //if (this.assignedRole != null) {
        //    this.assignedRole.OnDeath();
        //    this.assignedRole = null;
        //}
        //CitizenManager.Instance.UnregisterCitizen(this);

        //if (this.city != null) {
        //    //this.city.RemoveCitizenFromCity(this);
        //    //this.city.citizens.Remove(this);
        //    //this.city.RemoveCitizenInImportantCitizensInCity(this);
        //    //if (!isConquered) { //Check if citizen died of natural causes and not from conquering
        //    //    //if citizen is a grand marshal or grand chancellor, also remove family from city and generate new marshal/chancellor family
        //    //    if (previousRole == ROLE.GRAND_CHANCELLOR || previousRole == ROLE.GRAND_MARSHAL || previousRole == ROLE.GOVERNOR) {
        //    //        List<Citizen> family = this.GetRelatives(-1);
        //    //        for (int i = 0; i < family.Count; i++) {
        //    //            this.city.RemoveCitizenFromCity(family[i]);
        //    //        }
        //    //    }
        //    //    if (!this.city.isDead) {
        //    //        if (previousRole == ROLE.GRAND_CHANCELLOR) {
        //    //            this.city.CreateInitialChancellorFamily();
        //    //        } else if (previousRole == ROLE.GRAND_MARSHAL) {
        //    //            this.city.CreateInitialMarshalFamily();
        //    //        }
        //    //    }
        //    //}
        //}

        //if (this.id == this.city.kingdom.king.id && !this.city.kingdom.isDead) {
        //    //ASSIGN NEW LORD, SUCCESSION
        //    //			this.city.kingdom.AdjustExhaustionToAllRelationship(10);
        //    if (isDethroned) {
        //        if (newKing != null) {
        //            this.city.kingdom.AssignNewKing(newKing);
        //        }
        //    } else {
        //        if (!isConquered) {
        //            if (this.city.kingdom.successionLine.Count <= 0) {
        //                this.city.kingdom.AssignNewKing(null);
        //                //Remove family of previous king
        //                List<Citizen> family = this.GetRelatives(-1);
        //                for (int i = 0; i < family.Count; i++) {
        //                    this.city.RemoveCitizenFromCity(family[i]);
        //                }
        //            } else {
        //                this.city.kingdom.AssignNewKing(this.city.kingdom.successionLine[0]);
        //            }
        //        }
        //    }
        //} else {
        //    if (this.city.governor.id == this.id && !this.city.isDead) {
        //        this.city.AssignNewGovernor();
        //    }
        //}

        //if (this.isMarried && this._spouse != null) {
        //    //MarriageManager.Instance.DivorceCouple(this, spouse);
        //    if (previousRole == ROLE.KING) {
        //        //Spouse of king should no longer be queen
        //        //this.city.RemoveCitizenInImportantCitizensInCity(_spouse);
        //    }
        //    this.isMarried = false;
        //    this._spouse.isMarried = false;
        //    this._spouse.AssignSpouse(null);
        //    this.AssignSpouse(null);
        //}

        //this.isKing = false;
        //this.isGovernor = false;

        //if(previousRole == ROLE.KING) {
        //    UIManager.Instance.UpdateChooseCitizensMenuForIncurableDisease();
        //}
    }

    //internal void DeathHistory(DEATH_REASONS reason) {
    //    switch (reason) {
    //        case DEATH_REASONS.OLD_AGE:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died of natural causes.", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "died of natural causes";
    //            break;
    //        case DEATH_REASONS.ACCIDENT:
    //            string selectedAccidentCause = Utilities.accidentCauses[UnityEngine.Random.Range(0, Utilities.accidentCauses.Length)];
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died " + selectedAccidentCause, HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "died " + selectedAccidentCause;
    //            break;
    //        case DEATH_REASONS.BATTLE:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died in battle.", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "died in battle";
    //            break;
    //        case DEATH_REASONS.TREACHERY:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " hanged for treason.", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "was hanged for a treason";
    //            break;
    //        case DEATH_REASONS.ASSASSINATION:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died from an assassin's arrow.", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "died from an assassin's arrow";
    //            break;
    //        case DEATH_REASONS.REBELLION:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " hanged by an usurper.", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "was hanged by an usurper";
    //            break;
    //        case DEATH_REASONS.INTERNATIONAL_WAR:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died at the hands of a foreign enemy.", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "died at the hands of a foreign enemy";
    //            break;
    //        case DEATH_REASONS.STARVATION:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died of starvation.", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "died of starvation";
    //            break;
    //        case DEATH_REASONS.DISAPPEARED_EXPANSION:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " disappeared during an expedition and is assumed to be dead", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "disappeared during an expedition and is assumed to be dead";
    //            break;
    //        case DEATH_REASONS.PLAGUE:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " succumbed to the plague", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "succumbed to the plague";
    //            break;
    //        case DEATH_REASONS.SUICIDE:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " drank poison", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = "drank poison";
    //            break;
    //        case DEATH_REASONS.SERUM_OF_ALACRITY:
    //            this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died due to the serum of alacrity injection", HISTORY_IDENTIFIER.NONE));
    //            this.deathReasonText = " died due to the serum of alacrity injection";
    //            break;
    //    }
    //}
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
                    return -2;
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
        if(king.intelligence == INTELLIGENCE.SMART) {
            intelligenceAdjustment = 15;
            intelligenceSummary = intelligenceAdjustment.ToString() + "  Likes " + Utilities.NormalizeString(_intelligence.ToString()) + " king\n";
        } else if (king.intelligence == INTELLIGENCE.DUMB){
            intelligenceAdjustment = -15;
            intelligenceSummary = intelligenceAdjustment.ToString() + "  Dislikes " + Utilities.NormalizeString(_intelligence.ToString()) + " king\n";
        }
        if (intelligenceAdjustment != 0) {
            _loyaltyToKing += intelligenceAdjustment;
            if (intelligenceAdjustment > 0) {
                _loyaltySummary += "+";
            }
            _loyaltySummary += intelligenceSummary;
        }

        //Efficiency
        int efficiencyAdjustment = 0;
        string efficiencySummary = string.Empty;
        if(king.efficiency == EFFICIENCY.EFFICIENT) {
            efficiencyAdjustment = 15;
            efficiencySummary = efficiencyAdjustment.ToString() + "  Likes " + Utilities.NormalizeString(_efficiency.ToString()) + " king\n";
        } else if(king.efficiency == EFFICIENCY.INEPT){
            efficiencyAdjustment = -15;
            efficiencySummary = efficiencyAdjustment.ToString() + "  Dislikes " + Utilities.NormalizeString(_efficiency.ToString()) + " king\n";
        }
        if (efficiencyAdjustment != 0) {
            _loyaltyToKing += efficiencyAdjustment;
            if (efficiencyAdjustment > 0) {
                _loyaltySummary += "+";
            }
            _loyaltySummary += efficiencySummary;
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
        return city.kingdom.stability / 10;
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

        //Compute new capital city of source kingdom given the cities that will rebel
        if (citiesForRebellion.Contains(sourceKingdom.capitalCity)) {
            //Recompute capital city
            for (int i = 0; i < sourceKingdom.cities.Count; i++) {
                City currCity = sourceKingdom.cities[i];
                if (!citiesForRebellion.Contains(currCity)) {
                    sourceKingdom.SetCapitalCity(currCity);
                    break;
                }
            }
        }

        //Remove citizen that started rebellion from kingdom
        sourceKingdom.RemoveCitizenFromKingdom(this, this.city);

        //Remove citizen that started rebellion from source kingdom succession
        sourceKingdom.RemoveFromSuccession(this);

        //Transfer Royalties That are in the cities for rebellion
        for (int i = 0; i < citiesForRebellion.Count; i++) {
            City currRebellingCity = citiesForRebellion[i];
            sourceKingdom.TransferCitizensFromCityToCapital(currRebellingCity, new HashSet<Citizen>() { this });
        }
        

        ROLE previousRole = this.role;
        City previousCity = this.city;
        Kingdom newKingdom = KingdomManager.Instance.GenerateNewKingdom(sourceKingdom.race, new List<HexTile>() { }, false, sourceKingdom, false);
        KingdomManager.Instance.TransferCitiesToOtherKingdom(sourceKingdom, newKingdom, citiesForRebellion);

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
        } else if (previousRole == ROLE.QUEEN) {
            if(_spouse != null) {
                MarriageManager.Instance.DivorceCouple(this, _spouse);
            }
            this.children.Clear();
        } else if (previousRole == ROLE.GOVERNOR) {
            citizensToTransfer.AddRange(GetRelatives(-1));
        }

        newKingdom.AssignNewKing(this);
        if (this.spouse != null) {
            this.spouse.AssignRole(ROLE.QUEEN);
        }
        

        for (int i = 0; i < citizensToTransfer.Count; i++) {
            Citizen currCitizen = citizensToTransfer[i];
            newKingdom.AddCitizenToKingdom(currCitizen, newKingdom.capitalCity);
        }

        newKingdom.CreateNewChancellorFamily();
        newKingdom.CreateNewMarshalFamily();

        newKingdom.UpdateKingSuccession();

        Messenger.Broadcast<Kingdom>("OnNewKingdomCreated", newKingdom);

        for (int i = 0; i < newKingdom.regions.Count; i++) {
            newKingdom.regions[i].CheckForDiscoveredKingdoms();
        }

        newKingdom.UpdateAllRelationshipsLikeness();
        newKingdom.UpdateAllCitizensOpinionOfKing();

        newKingdom.HighlightAllOwnedTilesInKingdom();

        //Transfer population from sourceKingdom
        int totalCities = newKingdom.cities.Count + sourceKingdom.cities.Count;
        float percentGained = ((float) newKingdom.cities.Count / (float)totalCities);
        int populationToTransfer = Mathf.FloorToInt((float)sourceKingdom.population * percentGained);
        sourceKingdom.AdjustPopulation(-populationToTransfer);
        newKingdom.SetPopulation(populationToTransfer);

        //int weaponsGained = Mathf.FloorToInt((float)sourceKingdom.baseWeapons * percentGained);
        int weaponsGained = Mathf.FloorToInt((float)sourceKingdom.baseWeapons / 3f);
        sourceKingdom.AdjustBaseWeapons(-weaponsGained);
        newKingdom.SetBaseWeapons(weaponsGained);

        //int armorGained = Mathf.FloorToInt((float)sourceKingdom.baseArmor * percentGained);
        int armorGained = Mathf.FloorToInt((float)sourceKingdom.baseArmor / 3f);
        sourceKingdom.AdjustBaseArmors(-armorGained);
        newKingdom.SetBaseArmor(armorGained);

        //Once a rebellion is declared, set source Kingdom's Stability back to 50
        sourceKingdom.ChangeStability(50);
        newKingdom.ChangeStability(50);

        CameraMove.Instance.UpdateMinimapTexture();

        Log newLog = new Log(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "General", "Kingdom", "rebellion");
        newLog.AddToFillers(this, this.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        newLog.AddToFillers(sourceKingdom, sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
        newLog.AddToFillers(newKingdom, newKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
        UIManager.Instance.ShowNotification(newLog);

        KingdomRelationship kr = newKingdom.GetRelationshipWithKingdom(sourceKingdom);
		KingdomRelationship rk = sourceKingdom.GetRelationshipWithKingdom(newKingdom);

		kr.AddRelationshipModifier (-100, "Rebellion", RELATIONSHIP_MODIFIER.REBELLION, true, false);
		rk.AddRelationshipModifier (-100, "Rebellion", RELATIONSHIP_MODIFIER.REBELLION, true, false);

        if (kr.isAdjacent) {
            Warfare warfare = new Warfare(newKingdom, sourceKingdom);
            Debug.Log(previousRole.ToString() + " " + this.name + " of " + previousCity.name + " has rebelled against " + sourceKingdom.name);
            Debug.Log("Rebelling kingdom " + newKingdom.name + " declares war on " + sourceKingdom.name);
        }
    }
    #endregion

    #region Character Traits
    private void GenerateTraits() {
        this._charisma = GenerateCharismaTrait();
        this._efficiency = GenerateEfficiencyTrait();
        this._intelligence = GenerateIntelligenceTrait();
        this._science = GenerateScienceTrait();
        this._military = GenerateMilitaryTrait();
        this._loyalty = GenerateLoyaltyTrait();
        this._balanceType = (PURPOSE)(UnityEngine.Random.Range(0, System.Enum.GetNames(typeof(PURPOSE)).Length));
		this._warmonger = GenerateWarmonger ();
    }

    private CHARISMA GenerateCharismaTrait() {
        int chance = Random.Range(0, 100);
        if(chance < 20) {
            return CHARISMA.CHARISMATIC;
        }else if(chance >= 20 && chance < 40) {
            return CHARISMA.REPULSIVE;
        } else {
            return CHARISMA.NEUTRAL;
        }
    }
    private INTELLIGENCE GenerateIntelligenceTrait() {
        int chance = Random.Range(0, 100);
        if (chance < 20) {
            return INTELLIGENCE.SMART;
        } else if (chance >= 20 && chance < 40) {
            return INTELLIGENCE.DUMB;
        } else {
            return INTELLIGENCE.NEUTRAL;
        }
    }
    private EFFICIENCY GenerateEfficiencyTrait() {
        int chance = Random.Range(0, 100);
        if (chance < 20) {
            return EFFICIENCY.EFFICIENT;
        } else if (chance >= 20 && chance < 40) {
            return EFFICIENCY.INEPT;
        } else {
            return EFFICIENCY.NEUTRAL;
        }
    }
    private SCIENCE GenerateScienceTrait() {
        int chance = Random.Range(0, 100);
        if (chance < 10) {
            return SCIENCE.ERUDITE;
        } else if (chance >= 10 && chance < 25) {
            return SCIENCE.ACADEMIC;
        } else if (chance >= 25 && chance < 40) {
            return SCIENCE.IGNORANT;
        } else {
            return SCIENCE.NEUTRAL;
        }
    }
    private MILITARY GenerateMilitaryTrait() {
        int chance = Random.Range(0, 100);
        if (chance < 10) {
            return MILITARY.HOSTILE;
        } else if (chance >= 10 && chance < 25) {
            return MILITARY.MILITANT;
        } else if (chance >= 25 && chance < 40) {
            return MILITARY.PACIFIST;
        } else {
            return MILITARY.NEUTRAL;
        }
    }
    private LOYALTY GenerateLoyaltyTrait() {
        int chance = Random.Range(0, 100);
        if (chance < 20) {
            return LOYALTY.LOYAL;
        } else if (chance >= 20 && chance < 50) {
            return LOYALTY.SCHEMING;
        } else {
            return LOYALTY.NEUTRAL;
        }
    }
	private WARMONGER GenerateWarmonger(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(this._military == MILITARY.HOSTILE){
			if(chance < 35){
				return WARMONGER.VERY_HIGH;
			}else{
				return WARMONGER.HIGH;
			}
		}else if(this._military == MILITARY.PACIFIST){
			if(chance < 35){
				return WARMONGER.VERY_LOW;
			}else{
				return WARMONGER.LOW;
			}
		}else if(this._military == MILITARY.MILITANT){
			if(chance < 15){
				return WARMONGER.VERY_HIGH;
			}else if(chance >= 15 && chance < 50){
				return WARMONGER.HIGH;
			}else{
				return WARMONGER.AVERAGE;
			}
		}else{
			if(chance < 5){
				return WARMONGER.VERY_HIGH;
			}else if(chance >= 5 && chance < 25){
				return WARMONGER.HIGH;
			}else if(chance >= 25 && chance < 75){
				return WARMONGER.AVERAGE;
			}else if(chance >= 75 && chance < 95){
				return WARMONGER.LOW;
			}else{
				return WARMONGER.VERY_LOW;
			}
		}
	}
    #endregion

	internal float GetWarmongerWarPercentage100(){
		if(this._warmonger == WARMONGER.VERY_HIGH){
			return 6f;
		}else if(this._warmonger == WARMONGER.HIGH){
			return 4f;
		}else if(this._warmonger == WARMONGER.AVERAGE){
			return 2f;
		}else if(this._warmonger == WARMONGER.LOW){
			return 1f;
		}else{
			return 0.5f;
		}
	}
	internal float GetWarmongerWarPercentage50(){
		if(this._warmonger == WARMONGER.VERY_HIGH){
			return 0.5f;
		}else if(this._warmonger == WARMONGER.HIGH){
			return 0.4f;
		}else if(this._warmonger == WARMONGER.AVERAGE){
			return 0.3f;
		}else if(this._warmonger == WARMONGER.LOW){
			return 0.2f;
		}else{
			return 0.1f;
		}
	}
}
