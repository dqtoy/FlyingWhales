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

	[HideInInspector]public List<History> history;

	protected Dictionary<CHARACTER_VALUE, int> _dictCharacterValues;
    protected Dictionary<CHARACTER_VALUE, int> _importantCharacterValues;

	#region getters/setters
    public string name {
        get { return firstName + " " + surName; }
    }
    public int ageTableKey {
        get { return _ageTableKey; }
    }
	public Dictionary<CHARACTER_VALUE, int> dictCharacterValues{
		get{ return this._dictCharacterValues;}
	}
    public Dictionary<CHARACTER_VALUE, int> importantCharacterValues {
        get { return this._importantCharacterValues; }
    }
    public Citizen spouse {
        get { return this._spouse; }
    }
	#endregion

	public Citizen(City city, int age, GENDER gender, int generation){
		this.id = Utilities.SetID (this);
		this.race = city.kingdom.race;
		this.gender = gender;
		this.age = age;
        this._ageTableKey = -1;
        if(this.race == RACE.HUMANS) {
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
		this._dictCharacterValues = new Dictionary<CHARACTER_VALUE, int>();
        this._importantCharacterValues = new Dictionary<CHARACTER_VALUE, int>();

        this.city.citizens.Add (this);
	}

	internal int GetCampaignLimit(){
		return 2;
	}
	internal void AddParents(Citizen father, Citizen mother){
		this.father = father;
		this.mother = mother;

		if (this.race == RACE.HUMANS) {
			this.firstName = RandomNameGenerator.Instance.GetHumanFirstName (this.gender);
            this.surName = this.father.surName;
        } else {
			this.firstName = RandomNameGenerator.Instance.GenerateRandomName (this.race, this.gender);
		}
	}

	internal void AddChild(Citizen child){
		this.children.Add (child);
	}
	internal void AssignBirthday(MONTH month, int days, int year, bool registerCitizen = true){
		this.birthMonth = month;
		this.birthDay = days;
		this.birthYear = year;
        if (registerCitizen) {
            CitizenManager.Instance.RegisterCitizen(this);
            SchedulingManager.Instance.AddEntry((int)birthMonth, birthDay, GameManager.Instance.year, () => IncreaseAgeEveryYear());
        }
		this.history.Add(new History((int)month, days, year, this.name + " was born.", HISTORY_IDENTIFIER.NONE));
	}

    #region Age
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

	internal void Death(DEATH_REASONS reason, bool isDethroned = false, Citizen newKing = null, bool isConquered = false){
		if(!this.isDead){
			DeathCoroutine (reason, isDethroned, newKing, isConquered);
            Messenger.Broadcast("UpdateUI");
        }
	}
	internal void DeathCoroutine(DEATH_REASONS reason, bool isDethroned = false, Citizen newKing = null, bool isConquered = false){
		Debug.Log("DEATH: " + this.name + " of " + this.city.name + ": " + reason.ToString());
		DeathHistory(reason);
		this.deathReason = reason;
		this.isDead = true;
		if(this is Spouse){
			((Spouse)this).isAbducted = false;
		}
		if (this.city != null) {
			this.city.kingdom.RemoveFromSuccession(this);
		}
		if (this.assignedRole != null) {
			this.assignedRole.OnDeath ();
		}
        CitizenManager.Instance.UnregisterCitizen(this);

        if (this.city != null) {
			this.city.citizens.Remove (this);
		}

		if (this.isMarried && this._spouse != null) {
            //MarriageManager.Instance.DivorceCouple(this, spouse);
            this.isMarried = false;
            this._spouse.isMarried = false;
			this._spouse.AssignSpouse(null);
            this.AssignSpouse(null);
        }
		if (this.id == this.city.kingdom.king.id) {
			//ASSIGN NEW LORD, SUCCESSION
			this.city.kingdom.AdjustExhaustionToAllRelationship(10);
            if (isDethroned) {
				if (newKing != null) {
					this.city.kingdom.AssignNewKing (newKing);
				}
			} else { 
				if (this.city.kingdom.successionLine.Count <= 0) {
					if (!isConquered) {
						this.city.kingdom.AssignNewKing (null);
					}
				} else {
					this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
				}
			}
		}else{
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
		}else if (role == ROLE.ABDUCTOR) {
			this.assignedRole = new Abductor(this);
		}else if (role == ROLE.LYCANTHROPE) {
            this.assignedRole = new Lycanthrope(this);
		}else if (role == ROLE.INVESTIGATOR) {
			this.assignedRole = new Investigator(this);
		}else if (role == ROLE.THIEF) {
			this.assignedRole = new Thief(this);
		} else if (role == ROLE.WITCH) {
            this.assignedRole = new Witch(this);
        }else if (role == ROLE.ADVENTURER) {
            this.assignedRole = new Adventurer(this);
		}else if (role == ROLE.RELIEVER) {
			this.assignedRole = new Reliever(this);
		}else if (role == ROLE.INTERCEPTER) {
			this.assignedRole = new Intercepter(this);
		}else if (role == ROLE.RANGER) {
			this.assignedRole = new Ranger(this);
		}else if (role == ROLE.TREATYOFFICER) {
			this.assignedRole = new TreatyOfficer(this);
		}else if (role == ROLE.TRIBUTER) {
			this.assignedRole = new Tributer(this);
		}else if(role == ROLE.MILITARY_ALLIANCE_OFFICER) {
            this.assignedRole = new MilitaryAllianceOfficer(this);
        }
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


	internal void ForceWar(Kingdom targetKingdom, GameEvent gameEventTrigger, WAR_TRIGGER warTrigger = WAR_TRIGGER.NONE){
		if (this.city.kingdom.HasActiveEvent(EVENT_TYPES.INVASION_PLAN)) {
			return;
		}
		War warEvent = KingdomManager.Instance.GetWarBetweenKingdoms (this.city.kingdom, targetKingdom);
		if (warEvent != null && warEvent.isAtWar) {
			return;
		}
		if (warEvent == null) {
			warEvent = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, 
				this.city.kingdom, targetKingdom, WAR_TRIGGER.TARGET_GAINED_A_CITY);
		}
		warEvent.CreateInvasionPlan (this.city.kingdom, gameEventTrigger);
	}
	
	internal void ChangeSurname(Citizen citizenToGetSurnameFrom){
        this.surName = citizenToGetSurnameFrom.surName;
	}

    internal void AssignSpouse(Citizen spouse) {
        this._spouse = spouse;
        this.isMarried = true;
    }
	internal void DivorceSpouse(){
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
        if(this.father != null) {
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
        if(depth == -1) {
            for (int i = 0; i < pendingCitizens.Count; i++) {
                Citizen currCitizen = pendingCitizens[i];
                List<Citizen> relativesOfCurrCitizen = currCitizen.GetRelatives(0).Where(x => x.id != this.id).ToList();
                relatives = relatives.Union(relativesOfCurrCitizen).ToList();
                pendingCitizens = pendingCitizens.Union(relativesOfCurrCitizen).ToList();
            }
        } else {
            for (int i = 0; i < depth; i++) {
                Citizen currCitizen = null;
                try {
                    currCitizen = pendingCitizens[i];
                } catch(System.Exception e) {
                    break;
                }
                List<Citizen> relativesOfCurrCitizen = currCitizen.GetRelatives(0).Where(x => x.id != this.id).ToList();
                relatives = relatives.Union(relativesOfCurrCitizen).ToList();
                pendingCitizens = pendingCitizens.Union(relativesOfCurrCitizen).ToList();
            }
        }
        
        return relatives;
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
//        this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);
		UpdateCharacterValues();
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
        this._importantCharacterValues = this._dictCharacterValues.Where(x => x.Value >= 50).OrderByDescending(x => x.Value).Take(4).ToDictionary(x => x.Key, x => x.Value);
    }
    private void UpdateSpecificCharacterValue(CHARACTER_VALUE key, int value){
		if(this._dictCharacterValues.ContainsKey(key)){
			this._dictCharacterValues [key] += value;
//			UpdateCharacterValueByKey(key, value);
		}
	}

	//private void UpdateCharacterValueByKey(CHARACTER_VALUE key, int value){
	//	for(int i = 0; i < this._characterValues.Length; i++){
	//		if(this._characterValues[i].character == key){
	//			this._characterValues [i].value += value;
	//			break;
	//		}
	//	}
	//}

	internal int GetCharacterValueOfType(CHARACTER_VALUE characterValue){
		if(this._dictCharacterValues.ContainsKey(characterValue)){
			return this._dictCharacterValues [characterValue];
		}
		return 0;
	}

}
