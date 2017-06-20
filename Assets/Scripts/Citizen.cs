using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[System.Serializable]
public class Citizen {
	public int id;
	public string name;
	public GENDER gender;
	public int age;
	public int generation;
	public int prestige;
	public int[] horoscope; 
	public Kingdom homeKingdom;
	public City homeCity;
	public City city;
	public HexTile currentLocation;
	public ROLE role;
	public Role assignedRole;
	public RACE race;
	protected TRAIT _honestyTrait;
	protected TRAIT _hostilityTrait;
	protected TRAIT _miscTrait;
	public Citizen supportedCitizen;
	public Citizen father;
	public Citizen mother;
	protected Citizen _spouse;
	public List<Citizen> children;
	public HexTile workLocation;
	public CitizenChances citizenChances;
//	public CampaignManager campaignManager;
	public List<RelationshipKings> relationshipKings;
	public List<Citizen> successionWars;
	public List<Citizen> civilWars;
	public MONTH birthMonth;
	public int birthWeek;
	public int birthYear;
	public int supportExpirationWeek;
	public int supportExpirationMonth;
	public int supportExpirationYear;
	public int monthSupportCanBeChanged;
	public int yearSupportStarted;
	public bool isIndependent;
	public bool isMarried;
	public bool isDirectDescendant;
	public bool isGovernor;
	public bool isKing;
	public bool isPretender;
	public bool isHeir;
	public bool isBusy;
//	public bool isGhost;
	public bool isImmortal;
	public bool isDead;
	public DEATH_REASONS deathReason;
	public string deathReasonText;

	[HideInInspector]public List<History> history;

	protected List<Citizen> _possiblePretenders = new List<Citizen>();
	protected Dictionary<CHARACTER_VALUE, int> _dictCharacterValues;
    protected Dictionary<CHARACTER_VALUE, int> _importantCharcterValues;

    protected CharacterValue[] _characterValues;
	protected const int MARRIAGE_CHANCE = 100; //8

	#region getters/setters
	public List<Citizen> possiblePretenders{
		get{ return this._possiblePretenders;}
	}
	public Dictionary<CHARACTER_VALUE, int> dictCharacterValues{
		get{ return this._dictCharacterValues;}
	}
	public CharacterValue[] characterValues{
		get{ return this._characterValues;}
	}
    public Dictionary<CHARACTER_VALUE, int> importantCharcterValues {
        get { return this._importantCharcterValues; }
    }
    //public Dictionary<CHARACTER_VALUE, int> importantCharcterValues {
    //    get { return this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value); }
    //}
    public List<Citizen> dependentChildren{
		get{ return this.children.Where (x => x.age < 16 && !x.isMarried).ToList ();}
	}

	public List<RelationshipKings> friends{
		get{ return this.relationshipKings.Where(x => x.lordRelationship == RELATIONSHIP_STATUS.FRIEND || x.lordRelationship == RELATIONSHIP_STATUS.ALLY).ToList();}
	}

	public TRAIT honestyTrait{
		get{ return this._honestyTrait; }
	}
	public TRAIT hostilityTrait{
		get{ return this._hostilityTrait; }
	}
	public TRAIT miscTrait{
		get{ return this._miscTrait; }
	}
    public Citizen spouse {
        get { return this._spouse; }
    }
	#endregion

	public Citizen(City city, int age, GENDER gender, int generation){
		this.id = Utilities.SetID (this);
		/*if(isGhost){
			this.id = 0;
		}else{
			this.id = Utilities.SetID (this);
		}*/
		this.race = city.kingdom.race;
		this.gender = gender;
		this.age = age;
		this.name = RandomNameGenerator.Instance.GenerateRandomName(this.race, this.gender);
		/*if(isGhost){
			this.name = "GHOST";
		}else{
			this.name = RandomNameGenerator.Instance.GenerateRandomName(this.race, this.gender);
		}*/
		this.homeCity = city;
		this.homeKingdom = city.kingdom;
		this.generation = generation;
		this.prestige = 0;
		this.city = city;
		this.role = ROLE.UNTRAINED;
		this.assignedRole = null;
//		this.behaviorTraits = new List<BEHAVIOR_TRAIT> ();
//		this.skillTraits = new List<SKILL_TRAIT> ();
//		this.miscTraits = new List<MISC_TRAIT> ();
		this.supportedCitizen = null; //initially to king
		this.father = null;
		this.mother = null;
		this._spouse = null;
		this.children = new List<Citizen> ();
		this.workLocation = null;
		this.currentLocation = this.city.hexTile;
		this.citizenChances = new CitizenChances ();
//		this.campaignManager = new CampaignManager (this);
		this.relationshipKings = new List<RelationshipKings> ();
		this.successionWars = new List<Citizen> ();
		this.civilWars = new List<Citizen> ();
		this.birthMonth = (MONTH) GameManager.Instance.month;
		this.birthWeek = GameManager.Instance.days;
		this.birthYear = GameManager.Instance.year;
		this.horoscope = new int[3];
		this.isIndependent = false;
		this.isMarried = false;
		this.isDirectDescendant = false;
		this.isGovernor = false;
		this.isKing = false;
		this.isPretender = false;
		this.isHeir = false;
		this.isBusy = false;
		this.isImmortal = false;
		this.isDead = false;
		this.history = new List<History>();
		this.supportExpirationWeek = 0;
		this.supportExpirationMonth = 0;
		this.supportExpirationYear = 0;
		this.monthSupportCanBeChanged = 0;
		this.yearSupportStarted = 0;
		this.deathReason = DEATH_REASONS.NONE;
		this.deathReasonText = string.Empty;
		this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
		this.city.citizens.Add (this);
//			this.GenerateTraits();
		this.UpdatePrestige();

		EventManager.Instance.onCitizenTurnActions.AddListener(TurnActions);
		EventManager.Instance.onUnsupportCitizen.AddListener(UnsupportCitizen);
		EventManager.Instance.onRemoveSuccessionWarCity.AddListener (RemoveSuccessionWarCity);
		/*if(!isGhost){
			this.city.citizens.Add (this);
//			this.GenerateTraits();
			this.UpdatePrestige();

			EventManager.Instance.onCitizenTurnActions.AddListener(TurnActions);
			EventManager.Instance.onUnsupportCitizen.AddListener(UnsupportCitizen);
			EventManager.Instance.onRemoveSuccessionWarCity.AddListener (RemoveSuccessionWarCity);
		}else{
			EventManager.Instance.onDeathToGhost.AddListener (DeathToGhost);
		}*/
	}

	// This function checks if the citizen has the specified trait
	public bool hasTrait(TRAIT trait) {
		if (this._honestyTrait == trait || this._hostilityTrait == trait || this._miscTrait == trait) {
			return true;
		}
		return false;
	}

	internal int[] GetHoroscope(){
		int[] newHoroscope = new int[3];
		if((int)this.birthMonth % 2 == 0){
			newHoroscope[0] = 0;
		}else{
			newHoroscope[0] = 1;
		}

		if(this.birthWeek % 2 == 0){
			newHoroscope[1] = 0;
		}else{
			newHoroscope[1] = 1;
		}
		newHoroscope[2] = UnityEngine.Random.Range(0,2);
		return newHoroscope;
	}

	internal int GetCampaignLimit(){
		return 2;
	}
	internal void AddParents(Citizen father, Citizen mother){
		this.father = father;
		this.mother = mother;

		if (this.race == RACE.HUMANS) {
			this.name = RandomNameGenerator.Instance.GetHumanFirstName (this.gender) + " " + this.father.name.Split (' ').ElementAt (1);
		} else {
			this.name = RandomNameGenerator.Instance.GenerateRandomName (this.race, this.gender);
		}
	}

	internal void AddChild(Citizen child){
		this.children.Add (child);
	}
	internal void AssignBirthday(MONTH month, int days, int year){
		this.birthMonth = month;
		this.birthWeek = days;
		this.birthYear = year;
		this.horoscope = GetHoroscope ();
		this._honestyTrait = StoryTellingManager.Instance.GenerateHonestyTrait(this);
		this._hostilityTrait = StoryTellingManager.Instance.GenerateHostilityTrait(this);
		this._miscTrait = StoryTellingManager.Instance.GenerateMiscTrait(this);
		this.history.Add(new History((int)month, days, year, this.name + " was born.", HISTORY_IDENTIFIER.NONE));
	}
	internal void TurnActions(){
		this.AttemptToAge();
		this.DeathReasons();
		this.CheckSupportExpiration ();
		if (!this.isDead) {
			this.UpdatePrestige ();
		}
	}

	protected void CheckSupportExpiration(){
		if ((GameManager.Instance.year == this.supportExpirationYear && GameManager.Instance.month == this.supportExpirationMonth && GameManager.Instance.days == this.supportExpirationWeek) ||
			this.isDead) {
			this.supportedCitizen = null;
		}
	}

	protected void AttemptToAge(){
		if((MONTH)GameManager.Instance.month == this.birthMonth && GameManager.Instance.days == this.birthWeek && GameManager.Instance.year > this.birthYear){
			this.age += 1;
            /*Every birthday starting at 16 up to 50, an unmarried King, 
             * Queen or Governor has a chance to get married. 
             * A randomly generated character will be created.
             * */
            if (this.role == ROLE.KING || this.role == ROLE.GOVERNOR) {
                int chance = Random.Range(0, 100);
                if (this.age >= 16 && this.age <= 50 && !this.isMarried && chance < MARRIAGE_CHANCE) {
                    Citizen spouse = MarriageManager.Instance.GenerateSpouseForCitizen(this);
                    MarriageManager.Instance.Marry(this, spouse);
                }
            }
        }
	}

	internal void DeathReasons(){
		if(this.isImmortal){
			return;
		}
		if(isDead){
			return;
		}
		float accidents = UnityEngine.Random.Range (0f, 99f);
		if(accidents <= this.citizenChances.accidentChance){
			Death (DEATH_REASONS.ACCIDENT);
//			Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.days + "/" + PoliticsPrototypeManager.Instance.year + ": " + this.name + " DIED OF ACCIDENT!");
		}else{
			if(this.age >= 60){
				float oldAge = UnityEngine.Random.Range (0f, 99f);
				if(oldAge <= this.citizenChances.oldAgeChance){
					Death (DEATH_REASONS.OLD_AGE);
//					Debug.Log(this.name + " DIES OF OLD AGE");
//					Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.days + "/" + PoliticsPrototypeManager.Instance.year + ": " + this.name + " DIED OF OLD AGE!");
				}else{
					this.citizenChances.oldAgeChance += 0.05f;
				}
			}
		}
	}
	internal void Death(DEATH_REASONS reason, bool isDethroned = false, Citizen newKing = null, bool isConquered = false){
		if(!this.isDead){
			DeathCoroutine (reason, isDethroned, newKing, isConquered);
			EventManager.Instance.onUpdateUI.Invoke();
		}
//		DeathCoroutine(reason, isDethroned, newKing, isConquered);
	}
	internal void DeathCoroutine(DEATH_REASONS reason, bool isDethroned = false, Citizen newKing = null, bool isConquered = false){
		//		this.kingdom.royaltyList.allRoyalties.Remove (this);
//		yield return null;
		Debug.LogError("DEATH: " + this.name + " of " + this.city.name);
		DeathHistory(reason);
		this.deathReason = reason;
		this.isDead = true;

		if(isDethroned){
			this.isPretender = true;
			this.city.kingdom.AddPretender (this);
		}
		if(this.isPretender){
			Citizen possiblePretender = GetPossiblePretender ();
			if(possiblePretender != null){
				possiblePretender.isPretender = true;
				this.city.kingdom.AddPretender (possiblePretender);
			}
		}
		if (this.city != null) {
			this.city.kingdom.RemoveFromSuccession(this);
		}
//		if (this.workLocation != null) {
//			this.workLocation.UnoccupyTile();
//		}
		if (this.assignedRole != null) {
			this.assignedRole.OnDeath ();
		}
			
		EventManager.Instance.onCitizenTurnActions.RemoveListener (TurnActions);
		EventManager.Instance.onUnsupportCitizen.RemoveListener (UnsupportCitizen);
		EventManager.Instance.onUnsupportCitizen.Invoke (this);
		this.UnsupportCitizen (this);
//		EventManager.Instance.onCheckCitizensSupportingMe.RemoveListener(AddPrestigeToOtherCitizen);
		EventManager.Instance.onRemoveSuccessionWarCity.RemoveListener (RemoveSuccessionWarCity);


//		if (this.role == ROLE.GENERAL && this.assignedRole != null) {
//			if (isConquered) {
//				((General)this.assignedRole).GeneralDeath ();
//			} else {
//				if (((General)this.assignedRole).army.hp <= 0) {
//					((General)this.assignedRole).GeneralDeath ();
//				}else{
//					this.city.LookForNewGeneral((General)this.assignedRole);
//					if (this.role == ROLE.GENERAL && this.assignedRole != null) {
//						this.DetachGeneralFromCitizen ();
//					}
//				}
//			}
//
//		}
		if (this.city != null) {
			this.city.citizens.Remove (this);
		}
//		if(isConquered){
//			this.city.citizens.Remove (this);
//		}else{
//			if(this.role != ROLE.GENERAL && this.city != null){
//				this.city.citizens.Remove (this);
//			}
//		}
		if(!isConquered){
			EventManager.Instance.onCitizenDiedEvent.Invoke ();
		}


		if (this.isMarried && this._spouse != null) {
            //MarriageManager.Instance.DivorceCouple(this, spouse);
            this.isMarried = false;
            this._spouse.isMarried = false;
			this._spouse.AssignSpouse(null);
            this.AssignSpouse(null);
        }

		//		RoyaltyEventDelegate.onIncreaseIllnessAndAccidentChance -= IncreaseIllnessAndAccidentChance;
		//		RoyaltyEventDelegate.onChangeIsDirectDescendant -= ChangeIsDirectDescendant;
		//		RoyaltyEventDelegate.onMassChangeLoyalty -= MassChangeLoyalty;
		//		PoliticsPrototypeManager.Instance.turnEnded -= TurnActions;

		this.isHeir = false;
		if (this.id == this.city.kingdom.king.id) {
			//ASSIGN NEW LORD, SUCCESSION
			this.city.kingdom.AdjustExhaustionToAllRelationship(10);
            //KingdomManager.Instance.RemoveRelationshipToOtherKings(this.city.kingdom.king);
            //			this.city.kingdom.PassOnInternationalWar();
            if (isDethroned) {
				if (newKing != null) {
					this.city.kingdom.AssignNewKing (newKing);
				}
				//END CIVIL WAR
			} else{ 
				if (this.city.kingdom.successionLine.Count <= 0) {
					if (!isConquered) {
						this.city.kingdom.AssignNewKing (null);
					}
				} else {
					this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
					/*if(isConquered){
						this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
//						return;
					}else{
						if(this.successionWars.Count > 0){
							if(this.successionWars.Count == 1){
								this.city.kingdom.AssignNewKing (this.successionWars [0]);
							}else{
								this.successionWars [0].isHeir = true;
								List<Citizen> claimants = new List<Citizen> ();
								if(newKing != null && newKing.id != this.successionWars[0].id){
									for(int i = 0; i < this.successionWars.Count; i++){
										if(this.successionWars[i].id != newKing.id){
											claimants.Add (this.successionWars [i]);
										}
									}
									this.city.kingdom.SuccessionWar (newKing, claimants);
								}else{
									claimants.AddRange (this.successionWars.GetRange (1, this.successionWars.Count));
									this.city.kingdom.SuccessionWar (this.successionWars [0], claimants);
								}
							}
						}else{
							List<Citizen> claimants = new List<Citizen> ();
							if(this.city.kingdom.successionLine.Count > 2){
								claimants.Clear ();
								if(this.city.kingdom.successionLine[1].prestige > this.city.kingdom.successionLine[0].prestige){
									claimants.Add (this.city.kingdom.successionLine [1]);
								}
								if(this.city.kingdom.successionLine[2].prestige > this.city.kingdom.successionLine[0].prestige){
									claimants.Add (this.city.kingdom.successionLine [2]);
								}
								claimants.AddRange (this.city.kingdom.GetPretenderClaimants (this.city.kingdom.successionLine [0]));
								claimants = claimants.Distinct ().ToList ();
								if(claimants.Count > 0){
									//START SUCCESSION WAR
									this.city.kingdom.successionLine [0].isHeir = true;
									this.city.kingdom.SuccessionWar (this.city.kingdom.successionLine [0], claimants);
								}else{
									this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
								}
							}else if(this.city.kingdom.successionLine.Count > 1){
								claimants.Clear ();
								if(this.city.kingdom.successionLine[1].prestige > this.city.kingdom.successionLine[0].prestige){
									claimants.Add (this.city.kingdom.successionLine [1]);
								}
								claimants.AddRange (this.city.kingdom.GetPretenderClaimants (this.city.kingdom.successionLine [0]));
								if(claimants.Count > 0){
									//START SUCCESSION WAR
									this.city.kingdom.successionLine [0].isHeir = true;
									this.city.kingdom.SuccessionWar (this.city.kingdom.successionLine [0], claimants);
								}else{
									this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
								}
							}else{
								this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
							}
						}
					}*/
				}
				this.RemoveSuccessionAndCivilWars ();
			}
		}else{
//			for(int i = 0; i < this.campaignManager.activeCampaigns.Count; i++){
//				this.campaignManager.CampaignDone (this.campaignManager.activeCampaigns [i], false);
//			}
			if(this.city.governor.id == this.id){
				this.city.AssignNewGovernor ();
			}
		}

		this.isKing = false;
		this.isGovernor = false;
	}
	internal void DeathHistory(DEATH_REASONS reason){
		switch (reason){
		case DEATH_REASONS.OLD_AGE:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died of natural causes.", HISTORY_IDENTIFIER.NONE));
			this.deathReasonText = "died of natural causes";
			break;
		case DEATH_REASONS.ACCIDENT:
			string selectedAccidentCause = Utilities.accidentCauses [UnityEngine.Random.Range (0, Utilities.accidentCauses.Length)];
			this.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died " + selectedAccidentCause, HISTORY_IDENTIFIER.NONE));
			this.deathReasonText = "died " + selectedAccidentCause;
			break;
		case DEATH_REASONS.BATTLE:
			this.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died in battle.", HISTORY_IDENTIFIER.NONE));
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
			this.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died at the hands of a foreign enemy.", HISTORY_IDENTIFIER.NONE));
			this.deathReasonText = "died at the hands of a foreign enemy";
			break;
		case DEATH_REASONS.STARVATION:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died of starvation.", HISTORY_IDENTIFIER.NONE));
			this.deathReasonText = "died of starvation";
			break;
		case DEATH_REASONS.DISAPPEARED_EXPANSION:
			this.history.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " disappeared during an expedition and is assumed to be dead", HISTORY_IDENTIFIER.NONE));
			this.deathReasonText = "disappeared during an expedition and is assumed to be dead";
			break;
		}
	}
	internal void UnsupportCitizen(Citizen citizen){
		if(this.isGovernor || this.isKing){
			if(this.supportedCitizen != null){
				if(citizen.id == this.supportedCitizen.id){
					this.supportedCitizen = null;
				}
			}
			/*if(this.isGovernor){
				for(int i = 0; i < this.city.citizens.Count; i++){
					if(this.city.citizens[i].assignedRole != null && this.city.citizens[i].role == ROLE.GENERAL){
						if (this.city.citizens [i].assignedRole is General) {
							if (((General)this.city.citizens [i].assignedRole).assignedCampaign != null) {
								if (((General)this.city.citizens [i].assignedRole).assignedCampaign.leader != null) {
									if (((General)this.city.citizens [i].assignedRole).assignedCampaign.leader.id == citizen.id) {
										((General)this.city.citizens [i].assignedRole).UnregisterThisGeneral ();
									}
								}
							}
						}
					}
				}
			}
			if(this.isKing){
				for(int i = 0; i < this.city.kingdom.cities.Count; i++){
					for(int j = 0; j < this.city.kingdom.cities[i].citizens.Count; j++){
						if(this.city.kingdom.cities[i].citizens[j].assignedRole != null && this.city.kingdom.cities[i].citizens[j].role == ROLE.GENERAL){
							if(this.city.kingdom.cities [i].citizens [j].assignedRole is General){
								if (((General)this.city.kingdom.cities [i].citizens [j].assignedRole).assignedCampaign != null) {
									if (((General)this.city.kingdom.cities [i].citizens [j].assignedRole).assignedCampaign.leader != null) {
										if (((General)this.city.kingdom.cities [i].citizens [j].assignedRole).assignedCampaign.leader.id == citizen.id) {
											((General)this.city.kingdom.cities [i].citizens [j].assignedRole).UnregisterThisGeneral ();
										}
									}
								}
							}
						}
					}
				}
			}*/
		}

		/*if(this.supportedHeir != null){
			if(citizen.id == this.supportedHeir.id){
				this.supportedHeir = null;
			}
		}
		if(this.isGovernor || this.isKing){
			if(this.id != citizen.id){
				this.supportedCitizen.Remove (citizen);
				if(this.isGovernor){

				}
				if(this.isKing){
					for(int i = 0; i < this.city.kingdom.cities.Count; i++){
						for(int j = 0; j < this.city.kingdom.cities[i].citizens.Count; j++){
							if(this.city.kingdom.cities[i].citizens[j].assignedRole != null && this.city.kingdom.cities[i].citizens[j].role == ROLE.GENERAL){
								if(((General)this.city.kingdom.cities[i].citizens[j].assignedRole).warLeader.id == citizen.id){
									((General)this.city.kingdom.cities[i].citizens[j].assignedRole).UnregisterThisGeneral (null);
								}
							}
						}
					}
				}
			}
		}*/
	}
	internal void RemoveSuccessionAndCivilWars(){
//		for(int i = 0; i < this.civilWars.Count; i++){
//			this.civilWars [i].civilWars.Remove (this);
//		}
		for(int i = 0; i < this.successionWars.Count; i++){
			this.successionWars [i].RemoveSuccessionWar(this);
		}
//		this.civilWars.Clear ();
		this.successionWars.Clear ();
	}
	private Citizen GetPossiblePretender(){
		this._possiblePretenders.Clear ();
		Utilities.ChangePossiblePretendersRecursively (this, this);
		this._possiblePretenders.RemoveAt (0);
		this._possiblePretenders.AddRange (GetSiblings ());

		List<Citizen> orderedMaleRoyalties = this._possiblePretenders.Where (x => x.gender == GENDER.MALE && x.generation > this.generation).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		if(orderedMaleRoyalties.Count > 0){
			return orderedMaleRoyalties [0];
		}else{
			List<Citizen> orderedFemaleRoyalties = this._possiblePretenders.Where (x => x.gender == GENDER.FEMALE && x.generation > this.generation).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
			if(orderedFemaleRoyalties.Count > 0){
				return orderedFemaleRoyalties [0];
			}else{
				List<Citizen> orderedBrotherRoyalties = this._possiblePretenders.Where (x => x.gender == GENDER.MALE && x.father.id == this.father.id && x.id != this.id).OrderByDescending(x => x.age).ToList();
				if(orderedBrotherRoyalties.Count > 0){
					return orderedBrotherRoyalties [0];
				}else{
					List<Citizen> orderedSisterRoyalties = this._possiblePretenders.Where (x => x.gender == GENDER.FEMALE && x.father.id == this.id && x.id != this.id).OrderByDescending(x => x.age).ToList();
					if(orderedSisterRoyalties.Count > 0){
						return orderedSisterRoyalties [0];
					}
				}
			}
		}
		return null;
	}
	internal List<Citizen> GetSiblings(){
		List<Citizen> siblings = new List<Citizen> ();
		if (this.mother != null) {
			if (this.mother.children != null) {
				for (int i = 0; i < this.mother.children.Count; i++) {
					if (this.mother.children [i].id != this.id) {
						if (!this.mother.children [i].isDead) {
							siblings.Add (this.mother.children [i]);
						}
					}
				}
			}
		}else{
			if (this.father != null) {
				if (this.father.children != null) {
					for (int i = 0; i < this.father.children.Count; i++) {
						if (this.father.children [i].id != this.id) {
							if (!this.father.children [i].isDead) {
								siblings.Add (this.father.children [i]);
							}
						}
					}
				}
			}
		}

		return siblings;
	}

	internal void CreateInitialRelationshipsToKings(){
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom otherKingdom = KingdomManager.Instance.allKingdoms[i];
			if (otherKingdom.id != this.city.kingdom.id) {
				this.relationshipKings.Add (new RelationshipKings (this, otherKingdom.king, 0));
			}
		}
	}
//	internal bool CheckForSpecificWar(Citizen citizen){
//		for(int i = 0; i < this.relationshipKings.Count; i++){
//			if(this.relationshipKings[i].king.id == citizen.id){
//				if(this.relationshipKings[i].isAtWar){
//					return true;
//				}
//			}
//		}
//		return false;
//	}

	internal void AssignRole(ROLE role){
		if (this.role != ROLE.UNTRAINED) {
			if(this.assignedRole != null){
				this.assignedRole.OnDeath();
			}
		}

		this.role = role;
		if (role == ROLE.FOODIE) {
			this.assignedRole = new Foodie (this); 
		} else if (role == ROLE.GATHERER) {
			this.assignedRole = new Gatherer (this); 
		} else if (role == ROLE.MINER) {
			this.assignedRole = new Miner (this);
		} else if (role == ROLE.GENERAL) {
			this.assignedRole = new General (this);
		} else if (role == ROLE.ENVOY) {
			this.assignedRole = new Envoy (this);
		} else if (role == ROLE.GUARDIAN) {
			this.assignedRole = new Guardian (this);
		} else if (role == ROLE.SPY) {
			this.assignedRole = new Spy (this);
		} else if (role == ROLE.TRADER) {
			this.assignedRole = new Trader (this);
		} else if (role == ROLE.GOVERNOR) {
			this.assignedRole = new Governor (this);
		} else if (role == ROLE.KING) {
			this.assignedRole = new King (this);
		} else if (role == ROLE.EXPANDER) {
			this.assignedRole = new Expander (this);
		} else if (role == ROLE.RAIDER) {
			this.assignedRole = new Raider (this);
		} else if (role == ROLE.REINFORCER) {
			this.assignedRole = new Reinforcer (this);
		} else if (role == ROLE.REBEL) {
			this.assignedRole = new Rebel (this);
		}
		this.UpdatePrestige ();
	}

	internal bool IsRoyaltyCloseRelative(Citizen otherCitizen){
		if (this.id == otherCitizen.id) {
			return true;
		}

		if (otherCitizen.id == this.father.id || otherCitizen.id == this.mother.id) {
			//royalty is father or mother
			return true;
		}

		if (this.father.father != null) {
			if (otherCitizen.id == this.father.father.id) {
				return true;
			}
		}

		if (this.father.mother != null) {
			if (otherCitizen.id == this.father.mother.id) {
				return true;
			}
		}

		if (this.mother.father != null) {
			if (otherCitizen.id == this.mother.father.id) {
				return true;
			}
		}

		if (this.mother.mother != null) {
			if (otherCitizen.id == this.mother.mother.id) {
				return true;
			}
		}


		if (this.father.father != null) {
			for (int i = 0; i < this.father.father.children.Count; i++) {
				if (otherCitizen.id == this.father.father.children[i].id) {
					//royalty is uncle or aunt from fathers side
					return true;
				}
				for (int j = 0; j < this.father.father.children[i].children.Count; j++) {
					if (otherCitizen.id == this.father.father.children[i].children[j].id) {
						//citizen is cousin from father's side
						return true;
					}
				}
			}
		}

		if (this.mother.father != null) {
			for (int i = 0; i < this.mother.father.children.Count; i++) {
				if (otherCitizen.id == this.mother.father.children [i].id) {
					//royalty is uncle or aunt from mothers side
					return true;
				}
				for (int j = 0; j < this.mother.father.children[i].children.Count; j++) {
					if (otherCitizen.id == this.mother.father.children[i].children[j].id) {
						//citizen is cousin from mother's side
						return true;
					}
				}
			}
		}

		for (int i = 0; i < this.children.Count; i++) {
			if (this.children[i].id == otherCitizen.id){
				//citizen is child
				return true;
			}
			for (int j = 0; j < this.children[i].children.Count; j++) {
				if (this.children[i].children[j].id == otherCitizen.id){
					//citizen is grand child
					return true;
				}
			}
		}

		return false;
	}

	internal RelationshipKings GetRelationshipWithCitizen(Citizen citizen){
		for (int i = 0; i < this.relationshipKings.Count; i++) {
			if (relationshipKings [i].king.id == citizen.id) {
				return relationshipKings[i];
			}
		}
		return null;
	}

	internal Citizen GetKingParent(){
		if (this.father != null) {
			if (this.father.isKing) {
				return this.father;
			}
		}

		if (this.mother != null) {
			if (this.mother.isKing) {
				return this.mother;
			}
		}
		return null;
	}

	internal void UpdatePrestige(){
		if (this.city == null) {
			return;
		}

		int prestige = 0;
		this.prestige = 0;
		//compute prestige for role
		if (this.isKing) {
//			EventManager.Instance.onCheckCitizensSupportingMe.Invoke(this);
			prestige += 500;
			for (int i = 0; i < this.relationshipKings.Count; i++) {
				if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.FRIEND) {
					prestige += 10;
				} else if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.ALLY) {
					prestige += 30;
				} else if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.ENEMY) {
					prestige -= 10;
				} else if (this.relationshipKings [i].lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
					prestige -= 30;
				}
			}
			for (int i = 0; i < this.city.kingdom.cities.Count; i++) {
				prestige += 15;
			}
//			List<Citizen> supportingGovernors = this.GetCitizensSupportingThisCitizen().Where(x => x.role == ROLE.GOVERNOR).ToList ();
//			prestige += (supportingGovernors.Count * 20);
		}

		if (this.isGovernor) {
//			EventManager.Instance.onCheckCitizensSupportingMe.Invoke(this);
			prestige += 350;

			for (int i = 0; i < this.city.ownedTiles.Count; i++) {
				prestige += 5;
			}
//			List<Citizen> supportingCitizens = this.GetCitizensSupportingThisCitizen();
//			prestige += (supportingCitizens.Where (x => x.role == ROLE.GOVERNOR).Count () * 20);
//			prestige += (supportingCitizens.Where (x => x.role == ROLE.KING).Count () * 60);
		}
		if (this.city.kingdom.successionLine.Count > 0) {
			if (this.city.kingdom.successionLine [0].id == this.id && this.role != ROLE.GOVERNOR) {
				prestige += 200;
//			EventManager.Instance.onCheckCitizensSupportingMe.Invoke(this);
//			List<Citizen> supportingCitizens = this.GetCitizensSupportingThisCitizen();
//			prestige += (supportingCitizens.Where (x => x.role == ROLE.GOVERNOR).Count () * 20);
//			prestige += (supportingCitizens.Where (x => x.role == ROLE.KING).Count () * 60);
			}
		}
		if (this.isMarried && this._spouse != null) {
			if (this._spouse.isKing || this._spouse.isGovernor) {
				prestige += 150;
			}
		}
//		if (this.role == ROLE.GENERAL) {
//			prestige += 200;
//		} else if (this.role == ROLE.SPY || this.role == ROLE.ENVOY || this.role == ROLE.GUARDIAN) {
//			prestige += 150;
//			if (this.role == ROLE.SPY) {
//				for (int i = 0; i < ((Spy)this.assignedRole).successfulMissions; i++) {
//					prestige += 20;
//				}
//				for (int i = 0; i < ((Spy)this.assignedRole).unsuccessfulMissions; i++) {
//					prestige -= 5;
//				}
//			} else if (this.role == ROLE.ENVOY) {
//				for (int i = 0; i < ((Envoy)this.assignedRole).successfulMissions; i++) {
//					prestige += 20;
//				}
//				for (int i = 0; i < ((Envoy)this.assignedRole).unsuccessfulMissions; i++) {
//					prestige -= 5;
//				}
//			} else if (this.role == ROLE.GUARDIAN) {
//				for (int i = 0; i < ((Guardian)this.assignedRole).successfulMissions; i++) {
//					prestige += 20;
//				}
//				for (int i = 0; i < ((Guardian)this.assignedRole).unsuccessfulMissions; i++) {
//					prestige -= 5;
//				}
//			}
//		}  else {
//			if (this.role != ROLE.UNTRAINED) {
//				prestige += 100;
//			} else {
//				prestige += 50;
//			}
//		} 
//
//		if (this.isPretender) {
//			prestige += 50;
//		}
//
//		if (this.city.kingdom.successionLine.Count > 1) {
//			if (this.city.kingdom.successionLine [1].id == this.id) {
//				prestige += 50;
//			}
//		}

		//For Supporting Citizens
		//List<Citizen> supportingCitizens = this.GetCitizensSupportingThisCitizen();
		//prestige += (supportingCitizens.Where (x => x.role == ROLE.GOVERNOR).Count () * 20);
		//prestige += (supportingCitizens.Where (x => x.role == ROLE.KING).Count () * 60);

		//Add prestige for successors
		this.prestige = prestige;

	}
	internal void AddSuccessionWar(Citizen enemy){
		this.city.kingdom.AdjustExhaustionToAllRelationship (15);
		this.successionWars.Add (enemy);
//		if(!this.campaignManager.SearchForSuccessionWarCities(enemy.city)){
//			this.campaignManager.successionWarCities.Add (new CityWar (enemy.city, false, WAR_TYPE.SUCCESSION));
//		}
//		if(!this.campaignManager.SearchForDefenseWarCities(this.city, WAR_TYPE.SUCCESSION)){
//			this.campaignManager.defenseWarCities.Add (new CityWar (this.city, false, WAR_TYPE.SUCCESSION));
//		}
	}
	internal void RemoveSuccessionWar(Citizen enemy){
		this.city.kingdom.AdjustExhaustionToAllRelationship (-15);
//		List<Campaign> campaign = this.campaignManager.activeCampaigns.FindAll (x => x.targetCity.id == enemy.city.id);
//		for(int i = 0; i < campaign.Count; i++){
//			for(int j = 0; j < campaign[i].registeredGenerals.Count; j++){
//				campaign[i].registeredGenerals [j].UnregisterThisGeneral ();
//			}
//			this.campaignManager.activeCampaigns.Remove (campaign[i]);
//		}
//		CityWar cityWar = this.campaignManager.successionWarCities.Find (x => x.city.id == enemy.city.id);
//		if(cityWar != null){
//			this.campaignManager.successionWarCities.Remove (cityWar);
//		}
		this.successionWars.Remove (enemy);
	}

	internal void DeteriorateRelationship(RelationshipKings relationship, GameEvent gameEventTrigger, bool isDiscovery){
		//TRIGGER OTHER EVENTS
//		InvasionPlan (relationship, gameEventTrigger, this.city.kingdom.kingdomTypeData);
//		BorderConflict (relationship, gameEventTrigger);
		Assassination (relationship, gameEventTrigger);
	}
	internal void InvasionPlan(RelationshipKings relationship, GameEvent gameEventTrigger, KingdomTypeData kingdomData){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 0;

		if(relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY){
			value = 4;
			if(this.hasTrait(TRAIT.PACIFIST)){
				value = 0;
			}else if(this.hasTrait(TRAIT.WARMONGER)){
				value = 6;
			}
		}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 8;
			if(this.hasTrait(TRAIT.PACIFIST)){
				value = 0;
			}else if(this.hasTrait(TRAIT.WARMONGER)){
				value = 12;
			}
		}


		if(chance < value){
			//INVASION PLAN
			//			if (EventManager.Instance.GetEventsOfTypePerKingdom (this.city.kingdom, EVENT_TYPES.INVASION_PLAN).Where(x => x.isActive).Count() > 0 ||
			//				KingdomManager.Instance.GetWarBetweenKingdoms(this.city.kingdom, relationship.king.city.kingdom) != null) {
			War warEvent = KingdomManager.Instance.GetWarBetweenKingdoms (this.city.kingdom, relationship.king.city.kingdom);
			if (warEvent != null && warEvent.isAtWar) {
				return;
			}
			if (EventManager.Instance.GetEventsStartedByKingdom(this.city.kingdom, new EVENT_TYPES[]{EVENT_TYPES.INVASION_PLAN}).Where(x => x.isActive).Count() > 0) {
				return;
			}
			if (warEvent == null) {
				warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, 
					this.city.kingdom, relationship.king.city.kingdom);
			}
			warEvent.CreateInvasionPlan (this.city.kingdom, gameEventTrigger);
			//			InvasionPlan invasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
			//				this, this.city.kingdom, relationship.king.city.kingdom, gameEventTrigger);
		}
	}
	private void BorderConflict(RelationshipKings relationship, GameEvent gameEvent){
		if (!this.hasTrait(TRAIT.SCHEMING)) {
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 4;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 8;
		}

		for (int i = 0; i < relationship.king.city.kingdom.relationshipsWithOtherKingdoms.Count; i++) {
			if (relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].targetKingdom.id != this.city.kingdom.id) {
				if (!relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].isAtWar && relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].isAdjacent) {
					if (GameManager.Instance.SearchForEligibility (relationship.king.city.kingdom, relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].targetKingdom, EventManager.Instance.GetEventsOfType (EVENT_TYPES.BORDER_CONFLICT))) {
						RelationshipKings relationshipToOther = this.SearchRelationshipByID (relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].targetKingdom.king.id);
						if (relationshipToOther.lordRelationship != RELATIONSHIP_STATUS.FRIEND && relationshipToOther.lordRelationship != RELATIONSHIP_STATUS.ALLY) {
							if (chance < value) {
								//BorderConflict
								Citizen startedBy = null;
								if (Random.Range (0, 2) == 0) {
									startedBy = relationship.king;
								} else {
									startedBy = relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].targetKingdom.king;
								}
								BorderConflict borderConflict = new BorderConflict (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, relationship.king.city.kingdom, relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].targetKingdom);
								EventManager.Instance.AddEventToDictionary (borderConflict);
								break;
							}	
						}
					}
				}
			}
		}

	}

	private void Assassination(RelationshipKings relationship, GameEvent gameEventTrigger){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 0;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY){
			if (this.hasTrait(TRAIT.SCHEMING)) {
				value = 5;
			}
		}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 10;
			if(this.hasTrait(TRAIT.SCHEMING)){
				value = 20;
			}else if(this.hasTrait(TRAIT.HONEST)){
				value = 0;
			}
		}

		if(chance < value){
			EventCreator.Instance.CreateAssassinationEvent(this.city.kingdom, relationship.king, gameEventTrigger, EventManager.Instance.eventDuration[EVENT_TYPES.ASSASSINATION]);
		}
	}
	/*private Citizen GetSpy(Kingdom kingdom){
		List<Citizen> unwantedGovernors = Utilities.GetUnwantedGovernors (kingdom.king);
		List<Citizen> spies = new List<Citizen> ();
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!Utilities.IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.SPY) {
							if(kingdom.cities [i].citizens [j].assignedRole is Spy){
								if (!((Spy)kingdom.cities [i].citizens [j].assignedRole).inAction) {
									spies.Add (kingdom.cities [i].citizens [j]);
								}
							}

						}
					}
				}
			}
		}

		if(spies.Count > 0){
			int random = UnityEngine.Random.Range (0, spies.Count);
			((Spy)spies [random].assignedRole).inAction = true;
			return spies [random];
		}else{
//			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}*/

	internal void WarTrigger(RelationshipKings relationship, GameEvent gameEventTrigger, KingdomTypeData kingdomData, WAR_TRIGGER warTrigger = WAR_TRIGGER.NONE){
//		return;
        if(!relationship.sourceKing.city.kingdom.discoveredKingdoms.Contains(relationship.king.city.kingdom) ||
            !relationship.king.city.kingdom.discoveredKingdoms.Contains(relationship.sourceKing.city.kingdom)) {
            //At least one of the kingdoms have not discovered each other yet
            return;
        }

		if (EventManager.Instance.GetEventsStartedByKingdom(this.city.kingdom, new EVENT_TYPES[]{EVENT_TYPES.INVASION_PLAN}).Where(x => x.isActive).Count() > 0) {
			return;
		}
		War warEvent = KingdomManager.Instance.GetWarBetweenKingdoms (this.city.kingdom, relationship.king.city.kingdom);
		if (warEvent != null && warEvent.isAtWar) {
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 0;
		MILITARY_STRENGTH milStrength = relationship.king.city.kingdom.GetMilitaryStrengthAgainst (this.city.kingdom);
		if(gameEventTrigger != null){
			if(kingdomData.dictWarTriggers.ContainsKey(gameEventTrigger.warTrigger)){
				value = kingdomData.dictWarTriggers [gameEventTrigger.warTrigger];
			}
		}else{
			if(kingdomData.dictWarTriggers.ContainsKey(warTrigger)){
				value = kingdomData.dictWarTriggers [warTrigger];
			}
		}

		if(kingdomData.dictWarRateModifierMilitary.ContainsKey(milStrength)){
			float modifier = (float)value * ((float)kingdomData.dictWarRateModifierMilitary [milStrength] / 100f);
			value += Mathf.RoundToInt (modifier);
		}
		if(kingdomData.dictWarRateModifierRelationship.ContainsKey(relationship.lordRelationship)){
			float modifier = (float)value * ((float)kingdomData.dictWarRateModifierRelationship [relationship.lordRelationship] / 100f);
			value += Mathf.RoundToInt (modifier);
		}
		if(kingdomData._warRateModifierPer15HexDistance != 0){
			int distance = PathGenerator.Instance.GetDistanceBetweenTwoTiles (this.city.hexTile, relationship.king.city.hexTile);
			int multiplier = (int)(distance / kingdomData.hexDistanceModifier);
			int dividend = kingdomData._warRateModifierPer15HexDistance * multiplier;
			float modifier = (float)value * ((float)dividend / 100f);
			value += Mathf.RoundToInt (modifier);
		}
		if(kingdomData._warRateModifierPerActiveWar != 0){
			int dividend = kingdomData._warRateModifierPerActiveWar * this.city.kingdom.GetWarCount();
			float modifier = (float)value * ((float)dividend / 100f);
			value += Mathf.RoundToInt (modifier);
		}

//		Debug.LogError ("CHANCE OF WAR: " + value.ToString());

		if(chance < value){
			if (warEvent == null) {
				warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, 
					this.city.kingdom, relationship.king.city.kingdom);
			}
			warEvent.CreateInvasionPlan (this.city.kingdom, gameEventTrigger, warTrigger);
		}
	}
	internal void ForceWar(Kingdom targetKingdom, GameEvent gameEventTrigger, WAR_TRIGGER warTrigger = WAR_TRIGGER.NONE){
		if (EventManager.Instance.GetEventsStartedByKingdom(this.city.kingdom, new EVENT_TYPES[]{EVENT_TYPES.INVASION_PLAN}).Where(x => x.isActive).Count() > 0) {
			return;
		}
		War warEvent = KingdomManager.Instance.GetWarBetweenKingdoms (this.city.kingdom, targetKingdom);
		if (warEvent != null && warEvent.isAtWar) {
			return;
		}
		if (warEvent == null) {
			warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, 
				this.city.kingdom, targetKingdom);
		}
		warEvent.CreateInvasionPlan (this.city.kingdom, gameEventTrigger, warTrigger);
	}
	internal void ImproveRelationship(RelationshipKings relationship){
		//Improvement of Relationship

		int chance = UnityEngine.Random.Range (0, 100);
		int value = 0;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY){
			value = 15;
			if(this.hasTrait(TRAIT.PACIFIST)){
				value = 20;
			}else if(this.hasTrait(TRAIT.WARMONGER)){
				value = 5;
			}
		}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 5;
			if(this.hasTrait(TRAIT.PACIFIST)){
				value = 10;
			}else if(this.hasTrait(TRAIT.WARMONGER)){
				value = 0;
			}
		}else{
			value = 25;
			if(this.hasTrait(TRAIT.PACIFIST)){
				value = 30;
			}else if(this.hasTrait(TRAIT.WARMONGER)){
				value = 10;
			}
		}
		if(chance < value){
			CancelInvasionPlan (relationship);
		}

	}
	private void CancelInvasionPlan(RelationshipKings relationship){
		//CANCEL INVASION PLAN
		List<GameEvent> allInvasionPlans = EventManager.Instance.GetEventsOfType (EVENT_TYPES.INVASION_PLAN);
		if (allInvasionPlans != null && allInvasionPlans.Count > 0) {
			allInvasionPlans = allInvasionPlans.Where (x => 
				((InvasionPlan)x).startedByKingdom.id == relationship.sourceKing.city.kingdom.id &&
				((InvasionPlan)x).targetKingdom.id == relationship.king.city.kingdom.id && 
				x.isActive).ToList ();
			if (allInvasionPlans.Count > 0) {
				if(allInvasionPlans[0] is InvasionPlan){
					((InvasionPlan)allInvasionPlans [0]).CancelEvent ();
				}
			}
		}
	}
	internal RelationshipKings SearchRelationshipByID(int id){
		for(int i = 0; i < this.relationshipKings.Count; i++){
			if(this.relationshipKings[i].king.id == id){
				return this.relationshipKings [i];
			}
		}
		return null;
	}
//	protected void AddPrestigeToOtherCitizen(Citizen otherCitizen){
//		if (this.city == null) {
//			return;
//		}
//		if (this.supportedCitizen == null) {
//			if (otherCitizen.city.kingdom.id == this.city.kingdom.id) {
//				if (otherCitizen.isKing) {
//					if (this.isGovernor) {
//						otherCitizen.prestigeFromSupport += 20;
//					}
//				} else if (otherCitizen.isHeir) {
//					if (this.isGovernor) {
//						otherCitizen.prestigeFromSupport += 20;
//					} else if (this.isKing) {
//						otherCitizen.prestigeFromSupport += 60;
//					}
//				}
//			}
//		} else {
//			if (this.supportedCitizen.id == otherCitizen.id) {
//				if (this.isGovernor) {
//					otherCitizen.prestigeFromSupport += 20;
//				} else if (this.isKing) {
//					otherCitizen.prestigeFromSupport += 60;
//				}
//			}
//		}
//	}

//	internal void StateVisit(Citizen targetKing, GameEvent gameEventTrigger){
//		int acceptChance = UnityEngine.Random.Range (0, 100);
//		int acceptValue = 50;
//		RelationshipKings relationship = targetKing.SearchRelationshipByID (this.id);
//		if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND || relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
//			acceptValue = 85;
//		}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.WARM || relationship.lordRelationship == RELATIONSHIP_STATUS.NEUTRAL){
//			acceptValue = 75;
//		}
//		if(acceptChance < acceptValue){
//			if((targetKing.spouse != null && !targetKing.spouse.isDead) || targetKing.city.kingdom.successionLine.Count > 0){
//				Citizen visitor = null;
//				if((targetKing.spouse != null && !targetKing.spouse.isDead) && targetKing.city.kingdom.successionLine.Count > 0){
//					int chance = UnityEngine.Random.Range (0, 2);
//					if(chance == 0){
//						visitor = targetKing.spouse;
//					}else{
//						visitor = targetKing.city.kingdom.successionLine [0];
//					}
//				}else if((targetKing.spouse != null && !targetKing.spouse.isDead) && targetKing.city.kingdom.successionLine.Count <= 0){
//					visitor = targetKing.spouse;
//				}else if(targetKing.spouse == null && targetKing.city.kingdom.successionLine.Count > 0){
//					visitor = targetKing.city.kingdom.successionLine [0];
//				}
//				if(visitor != null){
//					StateVisit stateVisit = new StateVisit(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, targetKing.city.kingdom, visitor, gameEventTrigger);
//					EventManager.Instance.AddEventToDictionary (stateVisit);
//				}
//			}else{
//				Debug.Log ("STATE VISIT REJECTED RESPECTFULLY");
//			}
//		}else{
//			Debug.Log ("STATE VISIT REJECTED RESPECTFULLY");
//		}
//
//	}
//
	internal void InformedAboutHiddenEvent(GameEvent hiddenEvent, Citizen spy){
		//Reduce relationship between target and source
		if(hiddenEvent is Assassination){
			//An assassination discovered by the target kingdom decreases the target's kingdom relationship by 15
			Kingdom assassinKingdom = ((Assassination)hiddenEvent).assassinKingdom;
			Kingdom targetKingdom = ((Assassination)hiddenEvent).targetCitizen.city.kingdom;

			RelationshipKings relationship = targetKingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
			relationship.AdjustLikeness (-15, hiddenEvent, true);
			relationship.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				targetKingdom.name +  " discovered an assassination plot against it, launched by " + assassinKingdom.name,
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));

		}else if(hiddenEvent is InvasionPlan){
			//An Invasion Plan discovered by the target Kingdom decreases the target kingdom's King's relationship by 20
			Kingdom sourceKingdom = ((InvasionPlan)hiddenEvent).sourceKingdom;
			Kingdom targetKingdom = ((InvasionPlan)hiddenEvent).targetKingdom;

			RelationshipKings relationship = targetKingdom.king.SearchRelationshipByID (sourceKingdom.king.id);
			relationship.AdjustLikeness (-35, hiddenEvent, true);

			relationship.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				targetKingdom.name +  " discovered an invasion plot against it, launched by " + sourceKingdom.name,
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
		}
		spy.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, spy.name + " informed " + this.name + " about " + hiddenEvent.eventType.ToString(), HISTORY_IDENTIFIER.NONE));

		//Perform Counteraction

	}

	internal List<Citizen> GetCitizensSupportingThisCitizen(){
		List<Citizen> citizensSupportingMe = new List<Citizen>();

		//Check governors of this kingdom
		List<Citizen> allGovernorsInThisKingdom = this.city.kingdom.GetAllCitizensOfType(ROLE.GOVERNOR);
		for (int i = 0; i < allGovernorsInThisKingdom.Count; i++) {
			Citizen currentGovernor = allGovernorsInThisKingdom[i];
			if (currentGovernor.id != this.id) {
				if (currentGovernor.supportedCitizen == null) {
					if (this.isKing || (this.city.kingdom.successionLine.Count > 0 && this.city.kingdom.successionLine [0].id == this.id)) {
						citizensSupportingMe.Add (currentGovernor);
					}
				} else {
					if (currentGovernor.supportedCitizen.id == this.id) {
						citizensSupportingMe.Add (currentGovernor);
					}
				}
			}
		}

		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[i];
			if (currentKingdom.id != this.city.kingdom.id) {
				if (currentKingdom.king.supportedCitizen != null && currentKingdom.king.supportedCitizen.id == this.id) {
					citizensSupportingMe.Add(currentKingdom.king);
				}
			}
		}
		return citizensSupportingMe;
	}

	internal bool SearchForSuccessionWar(Citizen citizen){
		for(int i = 0; i < this.successionWars.Count; i++){
			if(this.successionWars[i].id == citizen.id){
				return true;
			}
		}
		return false;
	}
	internal Citizen GetTargetSuccessionWar(City city){
		for(int i = 0; i < this.successionWars.Count; i++){
			if(this.successionWars[i].city.id == city.id){
				return this.successionWars [i];
			}
		}
		return null;
	}
	internal void ChangeSurname(Citizen citizenToGetSurnameFrom){
		string newSurname = citizenToGetSurnameFrom.name.Split(' ').ElementAt(1);
		this.name = this.name.Split (' ').ElementAt (0) + " " + newSurname;
	}

	internal void RemoveSuccessionWarCity (City city){
//		this.campaignManager.successionWarCities.RemoveAll (x => x.city.id == city.id);
	}

	internal void UnsubscribeListeners(){
		EventManager.Instance.onCitizenTurnActions.RemoveListener(TurnActions);
		EventManager.Instance.onUnsupportCitizen.RemoveListener(UnsupportCitizen);
//		EventManager.Instance.onCheckCitizensSupportingMe.RemoveListener(AddPrestigeToOtherCitizen);
		EventManager.Instance.onRemoveSuccessionWarCity.RemoveListener (RemoveSuccessionWarCity);
	}

	/*internal void DetachGeneralFromCitizen(){
//		Debug.Log (this.name + " of " + this.city.name + " HAS DETACHED HIS ARMY AND ABANDONED BEING A GENERAL");
		if(this.assignedRole is General){
			General general = (General)this.assignedRole;
//			Debug.Log ("CREATED GHOST CITIZEN FOR " + this.name);
			general.CreateGhostCitizen ();
		}
	
	}
	internal void DeathToGhost(City city){
		if(this.city.id == city.id){
			EventManager.Instance.onDeathToGhost.RemoveListener (DeathToGhost);
			if (this.assignedRole is General) {
				General general = (General)this.assignedRole;
				general.GeneralDeath ();
			}
		}
	}
	internal void CopyCampaignManager(CampaignManager source){
		Debug.Log (this.name + " of " + this.city.kingdom.name + " IS COPYING THE CAMPAIGN MANAGER OF " + source.leader.name + " of " + source.leader.city.kingdom.name);
		for(int i = 0; i < source.intlWarCities.Count; i++){
			if(!this.campaignManager.SearchForInternationalWarCities(source.intlWarCities[i].city)){
				this.campaignManager.intlWarCities.Add(source.intlWarCities[i]);
			}
		}
		for(int i = 0; i < source.civilWarCities.Count; i++){
			if(!this.campaignManager.SearchForCivilWarCities(source.civilWarCities[i].city)){
				this.campaignManager.civilWarCities.Add(source.civilWarCities[i]);
			}
		}
		for(int i = 0; i < source.successionWarCities.Count; i++){
			if(!this.campaignManager.SearchForSuccessionWarCities(source.successionWarCities[i].city)){
				this.campaignManager.successionWarCities.Add(source.successionWarCities[i]);
			}
		}
		for(int i = 0; i < source.defenseWarCities.Count; i++){
			if(source.defenseWarCities[i].warType == WAR_TYPE.INTERNATIONAL){
				if(!this.campaignManager.SearchForDefenseWarCities(source.defenseWarCities[i].city, source.defenseWarCities[i].warType)){
					this.campaignManager.defenseWarCities.Add(source.defenseWarCities[i]);
				}
			}
		}

		for (int i = 0; i < source.activeCampaigns.Count; i++) {
			source.activeCampaigns [i].leader = this;
		}
		source.leader = this;
		this.campaignManager.activeCampaigns.AddRange(source.activeCampaigns);
	}*/

	internal void SetHonestyTrait(TRAIT trait){
		this._honestyTrait = trait;
	}

	internal void SetHostilityTrait(TRAIT trait){
		this._hostilityTrait = trait;
	}

	internal void SetMiscTrait(TRAIT trait){
		this._miscTrait = trait;
	}

    internal void AssignSpouse(Citizen spouse) {
        this._spouse = spouse;
        this.isMarried = true;
    }

	internal bool IsRelative(Citizen citizen){
		if(this.mother != null && this.mother.id == citizen.id){
			return true;
		}
		if(this.father != null && this.father.id == citizen.id){
			return true;
		}
		List<Citizen> siblings = this.GetSiblings ();
		if(siblings.Contains(citizen)){
			return true;
		}
		if(this.children.Contains(citizen)){
			return true;
		}
		if(this.isDirectDescendant && citizen.isDirectDescendant){
			return true;
		}
		return false;
	}

	internal void SetImmortality(bool state){
		this.isImmortal = state;
		if(this.assignedRole != null){
			if(this.assignedRole.avatar != null){
				if(this.assignedRole.avatar.GetComponent<Collider2D> () != null){
					this.assignedRole.avatar.GetComponent<Collider2D> ().enabled = !state;
				}else{
					this.assignedRole.avatar.GetComponentInChildren<Collider2D> ().enabled = !state;
				}
			}
		}
	}

	internal void GenerateCharacterValues(){
		this._dictCharacterValues.Clear ();
		this._dictCharacterValues = System.Enum.GetValues (typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE> ().ToDictionary (x => x, x => UnityEngine.Random.Range (1, 101));
//		CHARACTER_VALUE[] character = System.Enum.GetValues (typeof(CHARACTER_VALUE)).Cast<CHARACTER_VALUE> ().ToArray ();
//		this._characterValues = new CharacterValue[character.Length];
//		for(int i = 0; i < this._characterValues.Length; i++){
//			this._characterValues [i].character = character [i];
//			this._characterValues [i].value = UnityEngine.Random.Range (1, 101);
//			this._dictCharacterValues.Add(this._characterValues [i].character, this._characterValues [i].value);
//		}
	}
	internal void UpdateCharacterValues(){
		for(int i = 0; i < this.city.kingdom.kingdomTypeData.characterValues.Length; i++){
			this.UpdateSpecificCharacterValue (this.city.kingdom.kingdomTypeData.characterValues [i].character, this.city.kingdom.kingdomTypeData.characterValues [i].value);
		}
        this._importantCharcterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

    }
	private void UpdateSpecificCharacterValue(CHARACTER_VALUE key, int value){
		if(this._dictCharacterValues.ContainsKey(key)){
			this._dictCharacterValues [key] += value;
//			UpdateCharacterValueByKey(key, value);
		}
	}

	private void UpdateCharacterValueByKey(CHARACTER_VALUE key, int value){
		for(int i = 0; i < this._characterValues.Length; i++){
			if(this._characterValues[i].character == key){
				this._characterValues [i].value += value;
				break;
			}
		}
	}

}
