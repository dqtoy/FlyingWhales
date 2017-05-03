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
//	public int prestigeFromSupport;
	public Kingdom homeKingdom;
	public City homeCity;
	public City city;
	public HexTile currentLocation;
	public ROLE role;
	public Role assignedRole;
	public RACE race;
	public List<BEHAVIOR_TRAIT> behaviorTraits;
	public List<SKILL_TRAIT> skillTraits;
	public List<MISC_TRAIT> miscTraits;
	public Citizen supportedCitizen;
	public Citizen father;
	public Citizen mother;
	public Citizen spouse;
	public List<Citizen> children;
	public HexTile workLocation;
	public CitizenChances citizenChances;
	public CampaignManager campaignManager;
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
	public bool isGhost;
	public bool isDead;
	[HideInInspector]public List<History> history;

	private List<Citizen> possiblePretenders = new List<Citizen>();

//	public int prestige{
//		get{ return _prestige + prestigeFromSupport;}
//	}

	public List<Citizen> dependentChildren{
		get{ return this.children.Where (x => x.age < 16 && !x.isMarried).ToList ();}
	}

	public List<RelationshipKings> friends{
		get{ return this.relationshipKings.Where(x => x.lordRelationship == RELATIONSHIP_STATUS.FRIEND || x.lordRelationship == RELATIONSHIP_STATUS.ALLY).ToList();}
	}


	public Citizen(City city, int age, GENDER gender, int generation, bool isGhost = false){
		this.isGhost = isGhost;
		if(isGhost){
			this.id = 0;
		}else{
			this.id = Utilities.SetID (this);
		}
		this.race = city.kingdom.race;
		this.gender = gender;
		this.age = age;
		if(isGhost){
			this.name = "GHOST";
		}else{
			this.name = RandomNameGenerator.Instance.GenerateRandomName(this.race, this.gender);
		}
		this.homeCity = city;
		this.homeKingdom = city.kingdom;
		this.generation = generation;
		this.prestige = 0;
//		this.prestigeFromSupport = 0;
		this.city = city;
		this.role = ROLE.UNTRAINED;
		this.assignedRole = null;
		this.behaviorTraits = new List<BEHAVIOR_TRAIT> ();
		this.skillTraits = new List<SKILL_TRAIT> ();
		this.miscTraits = new List<MISC_TRAIT> ();
//		this.trait = GetTrait();
//		this.trait = new TRAIT[]{TRAIT.VICIOUS, TRAIT.NONE};
		this.supportedCitizen = null; //initially to king
		this.father = null;
		this.mother = null;
		this.spouse = null;
		this.children = new List<Citizen> ();
		this.workLocation = null;
		this.currentLocation = this.city.hexTile;
		this.citizenChances = new CitizenChances ();
		this.campaignManager = new CampaignManager (this);
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
		this.isDead = false;
		this.history = new List<History>();
		this.supportExpirationWeek = 0;
		this.supportExpirationMonth = 0;
		this.supportExpirationYear = 0;
		this.monthSupportCanBeChanged = 0;
		this.yearSupportStarted = 0;

		if(!isGhost){
			this.city.citizens.Add (this);

			this.GenerateTraits();
			this.UpdatePrestige();

			EventManager.Instance.onCitizenTurnActions.AddListener(TurnActions);
			EventManager.Instance.onUnsupportCitizen.AddListener(UnsupportCitizen);
//			EventManager.Instance.onCheckCitizensSupportingMe.AddListener(AddPrestigeToOtherCitizen);
			EventManager.Instance.onRemoveSuccessionWarCity.AddListener (RemoveSuccessionWarCity);
		}
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
	internal void GenerateTraits(){
		this.behaviorTraits.Clear();
		this.skillTraits.Clear();
		this.miscTraits.Clear();
		//Generate Behaviour trait
		int firstItem = 1;
		int secondItem = 2;
		for (int j = 0; j < 4; j++) {
//			BEHAVIOR_TRAIT[] behaviourPair = new BEHAVIOR_TRAIT[2]{(BEHAVIOR_TRAIT)firstItem, (BEHAVIOR_TRAIT)secondItem};
			int chanceForTrait = UnityEngine.Random.Range (0, 100);
			if (chanceForTrait <= 20) {
				//the behaviour pairs are always contradicting
				BEHAVIOR_TRAIT behaviourTrait1 = (BEHAVIOR_TRAIT)firstItem;
				BEHAVIOR_TRAIT behaviourTrait2 = (BEHAVIOR_TRAIT)secondItem;
				int chanceForTrait1 = 50;
				int chanceForTrait2 = 50;
				if (this.father != null && this.mother != null) {
					if (this.mother.behaviorTraits.Contains (behaviourTrait1)) {chanceForTrait1 += 15;chanceForTrait2 -= 15;}

					if (this.father.behaviorTraits.Contains (behaviourTrait1)) {chanceForTrait1 += 15;chanceForTrait2 -= 15;}

					if (this.mother.behaviorTraits.Contains (behaviourTrait2)) {chanceForTrait2 += 15;chanceForTrait1 -= 15;}

					if (this.father.behaviorTraits.Contains (behaviourTrait2)) {chanceForTrait2 += 15;chanceForTrait1 -= 15;}
				}

				int traitChance = UnityEngine.Random.Range (0, (chanceForTrait1 + chanceForTrait2));
				if (traitChance <= chanceForTrait1) {
					this.behaviorTraits.Add (behaviourTrait1);
				} else {
					this.behaviorTraits.Add (behaviourTrait2);
				}
			}
			firstItem += 2;
			secondItem += 2;
		}

//		//Generate Skill Trait
//		int chanceForSkillTraitLength = UnityEngine.Random.Range (0, 100);
//		int numOfSkillTraits = 0;
//		if (chanceForSkillTraitLength <= 20) {
//			numOfSkillTraits = 2;
//		} else if (chanceForSkillTraitLength >= 21 && chanceForSkillTraitLength <= 40) {
//			numOfSkillTraits = 1;
//		}
//
//		List<SKILL_TRAIT> skillTraits = new List<SKILL_TRAIT>();
//		if (this.father != null && this.mother != null && (father.skillTraits.Count > 0 || mother.skillTraits.Count > 0)) {
//			int skillListChance = UnityEngine.Random.Range (0, 100);
//			if (skillListChance < 100) {
//				skillTraits.AddRange(father.skillTraits);
//				skillTraits.AddRange(mother.skillTraits);
//				skillTraits.Distinct();
//			} else {
//				skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
//				skillTraits.Remove (SKILL_TRAIT.NONE);
//			}
//		} else {
//			skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
//			skillTraits.Remove (SKILL_TRAIT.NONE);
//		}
//
//
//		for (int j = 0; j < numOfSkillTraits; j++) {
//			if (skillTraits.Count > 0) {
//				SKILL_TRAIT chosenSkillTrait = skillTraits [UnityEngine.Random.Range (0, skillTraits.Count)];
//				this.skillTraits.Add (chosenSkillTrait);
//				if (numOfSkillTraits > 1) {
//					skillTraits.Remove (chosenSkillTrait);
//					if (chosenSkillTrait == SKILL_TRAIT.EFFICIENT) {
//						skillTraits.Remove (SKILL_TRAIT.INEFFICIENT);
//					} else if (chosenSkillTrait == SKILL_TRAIT.INEFFICIENT) {
//						skillTraits.Remove (SKILL_TRAIT.EFFICIENT);
//					} else if (chosenSkillTrait == SKILL_TRAIT.LAVISH) {
//						skillTraits.Remove (SKILL_TRAIT.THRIFTY);
//					} else if (chosenSkillTrait == SKILL_TRAIT.THRIFTY) {
//						skillTraits.Remove (SKILL_TRAIT.LAVISH);
//					}
//				}
//			}
//		}
//
//		//misc traits
//		int chanceForMiscTraitLength = UnityEngine.Random.Range (0, 100);
//		int numOfMiscTraits = 0;
//		if (chanceForMiscTraitLength <= 10) {
//			numOfMiscTraits = 2;
//		} else if (chanceForMiscTraitLength >= 11 && chanceForMiscTraitLength <= 20) {
//			numOfMiscTraits = 1;
//		}
//
//		List<MISC_TRAIT> miscTraits = Utilities.GetEnumValues<MISC_TRAIT>().ToList();
//		miscTraits.Remove (MISC_TRAIT.NONE);
//		for (int j = 0; j < numOfMiscTraits; j++) {
//			MISC_TRAIT chosenMiscTrait = miscTraits[UnityEngine.Random.Range(0, miscTraits.Count)];
//			this.miscTraits.Add (chosenMiscTrait);
//			miscTraits.Remove (chosenMiscTrait);
//			if(chosenMiscTrait == MISC_TRAIT.ACCIDENT_PRONE){
////				this.citizenChances.accidentChance = 50f;
//			}
//		}
		this.behaviorTraits.Distinct().ToList();
//		this.skillTraits.Distinct().ToList();
//		this.miscTraits.Distinct().ToList();
	}

	internal int GetCampaignLimit(){
		if(this.miscTraits.Contains(MISC_TRAIT.TACTICAL)){
			return 3;
		}
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
		this.GenerateTraits();
	}

	internal void AddChild(Citizen child){
		this.children.Add (child);
	}
	internal void AssignBirthday(MONTH month, int days, int year){
		this.birthMonth = month;
		this.birthWeek = days;
		this.birthYear = year;
		this.horoscope = GetHoroscope ();
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
//			if (this.gender == GENDER.MALE) {
//				if (this.age >= 16 && !this.isMarried) {
//					this.citizenChances.marriageChance += 2;
//					if (EventManager.Instance.GetAllEventsStartedByCitizenByType (this, EVENT_TYPES.MARRIAGE_INVITATION).Count <= 0) {
//						this.AttemptToMarry ();
//					}
//				}
//			} else {
//				if (this.isKing && this.age >= 16 && !this.isMarried) {
//					this.citizenChances.marriageChance += 2;
//					if (EventManager.Instance.GetAllEventsStartedByCitizenByType (this, EVENT_TYPES.MARRIAGE_INVITATION).Count <= 0) {
//						this.AttemptToMarry ();
//					}
//				}
//			}

			if (this.miscTraits.Contains(MISC_TRAIT.AMBITIOUS)) {
				if (this.isPretender ||
				   (this.city.kingdom.successionLine.Count > 1 && this.city.kingdom.successionLine [1].id == this.id) ||
				   (this.city.kingdom.successionLine.Count > 2 && this.city.kingdom.successionLine [2].id == this.id)) {
					if (EventManager.Instance.GetAllEventsStartedByCitizenByType (this, EVENT_TYPES.POWER_GRAB).Count <= 0) {
						AttemptToGrabPower ();
					}
				}
			}

		}
	}

	protected void AttemptToMarry(){
		List<Resource> marriageInvitationCost = new List<Resource> () {
			new Resource (BASE_RESOURCE_TYPE.GOLD, 500)
		};
		if (!this.city.HasEnoughResourcesForAction (marriageInvitationCost)) {
			return;
		}

		int chanceToMarry = Random.Range (0, 100);
		this.citizenChances.marriageChance = 100;
		if (chanceToMarry < this.citizenChances.marriageChance) {
			Debug.Log (this.name + " has started a marriage invitation event!");

			MarriageInvitation marriageInvitation = new MarriageInvitation (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this);
		}
	}

	protected void AttemptToGrabPower(){
		int chanceToGrabPower = Random.Range (0, 100);
		if (chanceToGrabPower < 10) {
//		if (chanceToGrabPower < 100) {
			PowerGrab newPowerGrab = new PowerGrab(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, this.city.kingdom.king);
		}
	}

	internal void DeathReasons(){
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
					Debug.Log(this.name + " DIES OF OLD AGE");
//					Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.days + "/" + PoliticsPrototypeManager.Instance.year + ": " + this.name + " DIED OF OLD AGE!");
				}else{
					this.citizenChances.oldAgeChance += 0.05f;
				}
			}
		}
	}
	internal void DeathByStarvation(){
		Death (DEATH_REASONS.STARVATION);

		Debug.Log(this.name + " DIES OF STARVATION");
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
		this.isDead = true;

		if(isDethroned){
			this.isPretender = true;
			this.city.kingdom.AddPretender (this);
		}
		if(this.isPretender){
			Citizen possiblePretender = GetPossiblePretender (this);
			if(possiblePretender != null){
				possiblePretender.isPretender = true;
				this.city.kingdom.AddPretender (possiblePretender);
			}
		}
		if (this.city != null) {
			this.city.kingdom.RemoveFromSuccession(this);
		}
		if (this.workLocation != null) {
			this.workLocation.UnoccupyTile();
		}
		if (this.assignedRole != null) {
			this.assignedRole.OnDeath ();
		}

		for(int i = 0; i < this.campaignManager.activeCampaigns.Count; i++){
			this.campaignManager.CampaignDone (this.campaignManager.activeCampaigns [i], false);
		}
		EventManager.Instance.onCitizenTurnActions.RemoveListener (TurnActions);
		EventManager.Instance.onUnsupportCitizen.RemoveListener (UnsupportCitizen);
		EventManager.Instance.onUnsupportCitizen.Invoke (this);
		this.UnsupportCitizen (this);
//		EventManager.Instance.onCheckCitizensSupportingMe.RemoveListener(AddPrestigeToOtherCitizen);
		EventManager.Instance.onRemoveSuccessionWarCity.RemoveListener (RemoveSuccessionWarCity);


		if (this.role == ROLE.GENERAL && this.assignedRole != null) {
			if (isConquered) {
				((General)this.assignedRole).GeneralDeath ();
			} else {
				if (((General)this.assignedRole).army.hp <= 0) {
					((General)this.assignedRole).GeneralDeath ();
				}else{
					this.city.LookForNewGeneral((General)this.assignedRole);
					if (this.role == ROLE.GENERAL && this.assignedRole != null) {
						if (((General)this.assignedRole).generalAvatar != null) {
							GameObject.Destroy (((General)this.assignedRole).generalAvatar);
							((General)this.assignedRole).generalAvatar = null;
						}
						this.DetachGeneralFromCitizen ();
					}
				}
			}

		}
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

		EventManager.Instance.onCitizenDiedEvent.Invoke ();

		if (this.isMarried) {
//			MarriageManager.Instance.DivorceCouple (this, spouse);
			this.spouse.isMarried = false;
			this.spouse.spouse = null;
		}

		//		RoyaltyEventDelegate.onIncreaseIllnessAndAccidentChance -= IncreaseIllnessAndAccidentChance;
		//		RoyaltyEventDelegate.onChangeIsDirectDescendant -= ChangeIsDirectDescendant;
		//		RoyaltyEventDelegate.onMassChangeLoyalty -= MassChangeLoyalty;
		//		PoliticsPrototypeManager.Instance.turnEnded -= TurnActions;

		this.isHeir = false;
		if (this.id == this.city.kingdom.king.id) {
			//ASSIGN NEW LORD, SUCCESSION
			this.city.kingdom.AdjustExhaustionToAllRelationship(10);
			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.city.kingdom.king);
			this.city.kingdom.PassOnInternationalWar();
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
					if(isConquered){
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
					}
				}
				this.RemoveSuccessionAndCivilWars ();
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
			break;
		case DEATH_REASONS.ACCIDENT:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died " + Utilities.accidentCauses[UnityEngine.Random.Range(0, Utilities.accidentCauses.Length)], HISTORY_IDENTIFIER.NONE));
			break;
		case DEATH_REASONS.BATTLE:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died in battle.", HISTORY_IDENTIFIER.NONE));
			break;
		case DEATH_REASONS.TREACHERY:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " hanged for treason.", HISTORY_IDENTIFIER.NONE));
			break;
		case DEATH_REASONS.ASSASSINATION:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died from an assassin's arrow.", HISTORY_IDENTIFIER.NONE));
			break;
		case DEATH_REASONS.REBELLION:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " hanged by an usurper.", HISTORY_IDENTIFIER.NONE));
			break;
		case DEATH_REASONS.INTERNATIONAL_WAR:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died at the hands of a foreign enemy.", HISTORY_IDENTIFIER.NONE));
			break;
		case DEATH_REASONS.STARVATION:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " died of starvation.", HISTORY_IDENTIFIER.NONE));
			break;
		case DEATH_REASONS.DISAPPEARED_EXPANSION:
			this.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.name + " disappeared during an expedition and is assumed to be dead", HISTORY_IDENTIFIER.NONE));
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
			if(this.isGovernor){
				for(int i = 0; i < this.city.citizens.Count; i++){
					if(this.city.citizens[i].assignedRole != null && this.city.citizens[i].role == ROLE.GENERAL){
						if (((General)this.city.citizens [i].assignedRole).warLeader != null) {
							if (((General)this.city.citizens [i].assignedRole).warLeader.id == citizen.id) {
								((General)this.city.citizens [i].assignedRole).UnregisterThisGeneral (null);
							}
						}
					}
				}
			}
			if(this.isKing){
				for(int i = 0; i < this.city.kingdom.cities.Count; i++){
					for(int j = 0; j < this.city.kingdom.cities[i].citizens.Count; j++){
						if(this.city.kingdom.cities[i].citizens[j].assignedRole != null && this.city.kingdom.cities[i].citizens[j].role == ROLE.GENERAL){
							if(((General)this.city.kingdom.cities[i].citizens[j].assignedRole).warLeader != null){
								if(((General)this.city.kingdom.cities[i].citizens[j].assignedRole).warLeader.id == citizen.id){
									((General)this.city.kingdom.cities[i].citizens[j].assignedRole).UnregisterThisGeneral (null);
								}
							}
						}
					}
				}
			}
		}

//		if(this.supportedHeir != null){
//			if(citizen.id == this.supportedHeir.id){
//				this.supportedHeir = null;
//			}
//		}
//		if(this.isGovernor || this.isKing){
//			if(this.id != citizen.id){
//				this.supportedCitizen.Remove (citizen);
//				if(this.isGovernor){

//				}
//				if(this.isKing){
//					for(int i = 0; i < this.city.kingdom.cities.Count; i++){
//						for(int j = 0; j < this.city.kingdom.cities[i].citizens.Count; j++){
//							if(this.city.kingdom.cities[i].citizens[j].assignedRole != null && this.city.kingdom.cities[i].citizens[j].role == ROLE.GENERAL){
//								if(((General)this.city.kingdom.cities[i].citizens[j].assignedRole).warLeader.id == citizen.id){
//									((General)this.city.kingdom.cities[i].citizens[j].assignedRole).UnregisterThisGeneral (null);
//								}
//							}
//						}
//					}
//				}
//			}
//		}
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
	private Citizen GetPossiblePretender(Citizen citizen){
		this.possiblePretenders.Clear ();
		ChangePossiblePretendersRecursively (citizen);
		this.possiblePretenders.RemoveAt (0);
		this.possiblePretenders.AddRange (GetSiblings (citizen));

		List<Citizen> orderedMaleRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.MALE && x.generation > citizen.generation).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
		if(orderedMaleRoyalties.Count > 0){
			return orderedMaleRoyalties [0];
		}else{
			List<Citizen> orderedFemaleRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.FEMALE && x.generation > citizen.generation).OrderBy(x => x.generation).ThenByDescending(x => x.age).ToList();
			if(orderedFemaleRoyalties.Count > 0){
				return orderedFemaleRoyalties [0];
			}else{
				List<Citizen> orderedBrotherRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.MALE && x.father.id == citizen.father.id && x.id != citizen.id).OrderByDescending(x => x.age).ToList();
				if(orderedBrotherRoyalties.Count > 0){
					return orderedBrotherRoyalties [0];
				}else{
					List<Citizen> orderedSisterRoyalties = this.possiblePretenders.Where (x => x.gender == GENDER.FEMALE && x.father.id == citizen.id && x.id != citizen.id).OrderByDescending(x => x.age).ToList();
					if(orderedSisterRoyalties.Count > 0){
						return orderedSisterRoyalties [0];
					}
				}
			}
		}
		return null;
	}
	internal List<Citizen> GetSiblings(Citizen citizen){
		List<Citizen> siblings = new List<Citizen> ();
		for(int i = 0; i < citizen.mother.children.Count; i++){
			if(citizen.mother.children[i].id != citizen.id){
				if(!citizen.mother.children[i].isDead){
					siblings.Add (citizen.mother.children [i]);
				}
			}
		}

		return siblings;
	}
	private void ChangePossiblePretendersRecursively(Citizen citizen){
		if(!citizen.isDead){
			this.possiblePretenders.Add (citizen);
		}

		for(int i = 0; i < citizen.children.Count; i++){
			if(citizen.children[i] != null){
				this.ChangePossiblePretendersRecursively (citizen.children [i]);
			}
		}
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
		}
		this.UpdatePrestige ();
	}

	internal void Unemploy(){
		this.spouse.role = ROLE.UNTRAINED;
		this.spouse.assignedRole = null;
		this.spouse.workLocation = null;
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
		if (this.isMarried && this.spouse != null) {
			if (this.spouse.isKing || this.spouse.isGovernor) {
				prestige += 150;
			}
		}
		if (this.role == ROLE.GENERAL) {
			prestige += 200;
		} else if (this.role == ROLE.SPY || this.role == ROLE.ENVOY || this.role == ROLE.GUARDIAN) {
			prestige += 150;
			if (this.role == ROLE.SPY) {
				for (int i = 0; i < ((Spy)this.assignedRole).successfulMissions; i++) {
					prestige += 20;
				}
				for (int i = 0; i < ((Spy)this.assignedRole).unsuccessfulMissions; i++) {
					prestige -= 5;
				}
			} else if (this.role == ROLE.ENVOY) {
				for (int i = 0; i < ((Envoy)this.assignedRole).successfulMissions; i++) {
					prestige += 20;
				}
				for (int i = 0; i < ((Envoy)this.assignedRole).unsuccessfulMissions; i++) {
					prestige -= 5;
				}
			} else if (this.role == ROLE.GUARDIAN) {
				for (int i = 0; i < ((Guardian)this.assignedRole).successfulMissions; i++) {
					prestige += 20;
				}
				for (int i = 0; i < ((Guardian)this.assignedRole).unsuccessfulMissions; i++) {
					prestige -= 5;
				}
			}
		}  else {
			if (this.role != ROLE.UNTRAINED) {
				prestige += 100;
			} else {
				prestige += 50;
			}
		} 

		if (this.isPretender) {
			prestige += 50;
		}

		if (this.city.kingdom.successionLine.Count > 1) {
			if (this.city.kingdom.successionLine [1].id == this.id) {
				prestige += 50;
			}
		}

		//For Supporting Citizens
		List<Citizen> supportingCitizens = this.GetCitizensSupportingThisCitizen();
		prestige += (supportingCitizens.Where (x => x.role == ROLE.GOVERNOR).Count () * 20);
		prestige += (supportingCitizens.Where (x => x.role == ROLE.KING).Count () * 60);

		//Add prestige for successors
		this.prestige = prestige;

	}
	internal void AddSuccessionWar(Citizen enemy){
		this.city.kingdom.AdjustExhaustionToAllRelationship (15);
		this.successionWars.Add (enemy);
		if(!this.campaignManager.SearchForSuccessionWarCities(enemy.city)){
			this.campaignManager.successionWarCities.Add (new CityWar (enemy.city, false, WAR_TYPE.SUCCESSION));
		}
		if(!this.campaignManager.SearchForDefenseWarCities(this.city, WAR_TYPE.SUCCESSION)){
			this.campaignManager.defenseWarCities.Add (new CityWar (this.city, false, WAR_TYPE.SUCCESSION));
		}
	}
	internal void RemoveSuccessionWar(Citizen enemy){
		this.city.kingdom.AdjustExhaustionToAllRelationship (-15);
		List<Campaign> campaign = this.campaignManager.activeCampaigns.FindAll (x => x.targetCity.id == enemy.city.id);
		for(int i = 0; i < campaign.Count; i++){
			for(int j = 0; j < campaign[i].registeredGenerals.Count; j++){
				campaign[i].registeredGenerals [j].UnregisterThisGeneral (campaign[i]);
			}
			this.campaignManager.activeCampaigns.Remove (campaign[i]);
		}
		CityWar cityWar = this.campaignManager.successionWarCities.Find (x => x.city.id == enemy.city.id);
		if(cityWar != null){
			this.campaignManager.successionWarCities.Remove (cityWar);
		}
		this.successionWars.Remove (enemy);
	}
	internal void DeteriorateRelationship(RelationshipKings relationship, EVENT_TYPES reason, bool isDiscovery){
		//TRIGGER OTHER EVENTS
		if(relationship.like >= -40){
			int chance = UnityEngine.Random.Range (0, 100);
			int value = 5;
			if(relationship.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.PACIFIST)){
				value = 8;
			}else if(relationship.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.WARMONGER)){
				value = 2;
			}
			if(chance < value){
				//STATE VISIT
				STATEVISIT_TRIGGER_REASONS svReason = STATEVISIT_TRIGGER_REASONS.NONE;
				if(isDiscovery){
					if(reason == EVENT_TYPES.INVASION_PLAN){
						svReason = STATEVISIT_TRIGGER_REASONS.DISCOVERING_IP;
					}else{
						svReason = STATEVISIT_TRIGGER_REASONS.DISCOVERING_A;
					}
				}else{
					if (reason == EVENT_TYPES.ASSASSINATION) {
						svReason = STATEVISIT_TRIGGER_REASONS.ASSASSINATION;
					}else if (reason == EVENT_TYPES.BORDER_CONFLICT) {
						svReason = STATEVISIT_TRIGGER_REASONS.BORDER_CONFLICT;
					}else if (reason == EVENT_TYPES.DIPLOMATIC_CRISIS) {
						svReason = STATEVISIT_TRIGGER_REASONS.DIPLOMATIC_CRISIS;
					}else if (reason == EVENT_TYPES.ESPIONAGE) {
						svReason = STATEVISIT_TRIGGER_REASONS.ESPIONAGE;
					}else if (reason == EVENT_TYPES.RAID) {
						svReason = STATEVISIT_TRIGGER_REASONS.RAID;
					}else if (reason == EVENT_TYPES.STATE_VISIT) {
						svReason = STATEVISIT_TRIGGER_REASONS.STATE_VISIT;
					}
				}
				relationship.king.StateVisit(this, svReason);
			}
		}else{
			ASSASSINATION_TRIGGER_REASONS aReason = ASSASSINATION_TRIGGER_REASONS.NONE;
			INVASION_TRIGGER_REASONS ipReason = INVASION_TRIGGER_REASONS.NONE;

			if(isDiscovery){
				if(reason == EVENT_TYPES.INVASION_PLAN){
					aReason = ASSASSINATION_TRIGGER_REASONS.DISCOVERING_IP;
					ipReason = INVASION_TRIGGER_REASONS.DISCOVERING_IP;
				}else{
					aReason = ASSASSINATION_TRIGGER_REASONS.DISCOVERING_A;
					ipReason = INVASION_TRIGGER_REASONS.DISCOVERING_A;
				}
			}else{
				if (reason == EVENT_TYPES.ASSASSINATION) {
					aReason = ASSASSINATION_TRIGGER_REASONS.ASSASSINATION;
					ipReason = INVASION_TRIGGER_REASONS.ASSASSINATION;

				}else if (reason == EVENT_TYPES.BORDER_CONFLICT) {
					aReason = ASSASSINATION_TRIGGER_REASONS.BORDER_CONFLICT;
					ipReason = INVASION_TRIGGER_REASONS.BORDER_CONFLICT;

				}else if (reason == EVENT_TYPES.DIPLOMATIC_CRISIS) {
					aReason = ASSASSINATION_TRIGGER_REASONS.DIPLOMATIC_CRISIS;
					ipReason = INVASION_TRIGGER_REASONS.DIPLOMATIC_CRISIS;

				}else if (reason == EVENT_TYPES.ESPIONAGE) {
					aReason = ASSASSINATION_TRIGGER_REASONS.ESPIONAGE;
					ipReason = INVASION_TRIGGER_REASONS.ESPIONAGE;

				}else if (reason == EVENT_TYPES.RAID) {
					aReason = ASSASSINATION_TRIGGER_REASONS.RAID;
					ipReason = INVASION_TRIGGER_REASONS.RAID;

				}else if (reason == EVENT_TYPES.STATE_VISIT) {
					aReason = ASSASSINATION_TRIGGER_REASONS.STATE_VISIT;
					ipReason = INVASION_TRIGGER_REASONS.STATE_VISIT;

				}
			}
			InvasionPlan (relationship, ipReason);
			BorderConflict (relationship);
			Assassination (relationship, aReason);
		}
	}
	private void InvasionPlan(RelationshipKings relationship, INVASION_TRIGGER_REASONS reason){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 4;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 8;
		}
		if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.PACIFIST)){
			value = 1;
		}else if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.WARMONGER)){
			value = 6;
//			value = 100;
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				value = 12;
			}
		}

		if(chance < value){
			//INVASION PLAN
			if (EventManager.Instance.GetEventsOfTypePerKingdom (this.city.kingdom, EVENT_TYPES.INVASION_PLAN).Where(x => x.isActive).Count() > 0 ||
				KingdomManager.Instance.GetWarBetweenKingdoms(this.city.kingdom, relationship.king.city.kingdom) != null) {
				return;
			}
			InvasionPlan invasionPlan = new InvasionPlan(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, 
				this, this.city.kingdom, relationship.king.city.kingdom, reason);
			
		}else{
			//STATE VISIT
			int svChance = UnityEngine.Random.Range (0, 100);
			int svValue = 4;
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				svValue = 8;
			}
			if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.PACIFIST)){
				svValue = 6;
				if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
					svValue = 12;
				}
			}else if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.WARMONGER)){
				svValue = 1;
			}
			if(svChance < svValue){
				STATEVISIT_TRIGGER_REASONS svReason = STATEVISIT_TRIGGER_REASONS.NONE;
				if(reason == INVASION_TRIGGER_REASONS.DISCOVERING_A){
					svReason = STATEVISIT_TRIGGER_REASONS.DISCOVERING_A;
				}else if(reason == INVASION_TRIGGER_REASONS.DISCOVERING_IP){
					svReason = STATEVISIT_TRIGGER_REASONS.DISCOVERING_IP;
				}else if (reason == INVASION_TRIGGER_REASONS.ASSASSINATION) {
					svReason = STATEVISIT_TRIGGER_REASONS.ASSASSINATION;
				}else if (reason == INVASION_TRIGGER_REASONS.BORDER_CONFLICT) {
					svReason = STATEVISIT_TRIGGER_REASONS.BORDER_CONFLICT;
				}else if (reason == INVASION_TRIGGER_REASONS.ESPIONAGE) {
					svReason = STATEVISIT_TRIGGER_REASONS.ESPIONAGE;
				}else if (reason == INVASION_TRIGGER_REASONS.RAID) {
					svReason = STATEVISIT_TRIGGER_REASONS.RAID;
				}else if (reason == INVASION_TRIGGER_REASONS.STATE_VISIT) {
					svReason = STATEVISIT_TRIGGER_REASONS.STATE_VISIT;
				}
				this.StateVisit(relationship.king, svReason);
			}
		}
	}
	private void BorderConflict(RelationshipKings relationship){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 4;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 8;
		}

		if (this.behaviorTraits.Contains (BEHAVIOR_TRAIT.SCHEMING)) {
			for (int i = 0; i < relationship.king.city.kingdom.relationshipsWithOtherKingdoms.Count; i++) {
				if (relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship.id != this.city.kingdom.id) {
					if (!relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].isAtWar && relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].isAdjacent) {
						if (GameManager.Instance.SearchForEligibility (relationship.king.city.kingdom, relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship, EventManager.Instance.GetEventsOfType (EVENT_TYPES.BORDER_CONFLICT))) {
							RelationshipKings relationshipToOther = this.SearchRelationshipByID (relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship.king.id);
							if (relationshipToOther.lordRelationship != RELATIONSHIP_STATUS.FRIEND && relationshipToOther.lordRelationship != RELATIONSHIP_STATUS.ALLY) {
								if (chance < value) {
									//BorderConflict
									Citizen startedBy = null;
									if (Random.Range (0, 2) == 0) {
										startedBy = relationship.king;
									} else {
										startedBy = relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship.king;
									}
									BorderConflict borderConflict = new BorderConflict (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, startedBy, relationship.king.city.kingdom, relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship);
									EventManager.Instance.AddEventToDictionary (borderConflict);
									break;
								}	
							}
						}
					}
				}
			}
		}
	}
	private void Assassination(RelationshipKings relationship, ASSASSINATION_TRIGGER_REASONS reason){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 5;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 10;
		}
		if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.SCHEMING)){
			value = 10;
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				value = 20;
			}
		}else if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.NAIVE)){
			value = 0;
		}

		if(chance < value){
			Citizen spy = GetSpy(this.city.kingdom);
			if(spy != null){
				Assassination assassination = new Assassination(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, relationship.king, spy, reason);
			}
		}
	}
	private Citizen GetSpy(Kingdom kingdom){
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
			Debug.Log (kingdom.king.name + " CAN'T SEND SPY BECAUSE THERE IS NONE!");
			return null;
		}
	}
	internal void ImproveRelationship(RelationshipKings relationship){
		//Improvement of Relationship
		if(relationship.like >= -40){
			int chance = UnityEngine.Random.Range (0, 100);
			int value = 25;
			if(relationship.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.PACIFIST)){
				value = 30;
			}else if(relationship.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.WARMONGER)){
				value = 10;
			}
			if(chance < value){
				//CANCEL INVASION PLAN
				if(EventManager.Instance.GetEventsOfType(EVENT_TYPES.INVASION_PLAN) != null){
					List<GameEvent> invasionPlans = EventManager.Instance.GetEventsOfType(EVENT_TYPES.INVASION_PLAN).Where(x => 
						(((InvasionPlan)x).startedByKingdom.id == relationship.sourceKing.city.kingdom.id) && 
						(((InvasionPlan)x).targetKingdom.id == relationship.king.city.kingdom.id)).ToList();
					if (invasionPlans.Count > 0) {
						((InvasionPlan)invasionPlans [0]).CancelEvent();
					}
				}

			}
		}else{
			CancelInvasionPlan (relationship);
		}
	}
	private void CancelInvasionPlan(RelationshipKings relationship){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 15;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 5;
		}

		if(relationship.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.PACIFIST)){
			value = 20;
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				value = 10;
			}
		}else if(relationship.king.behaviorTraits.Contains(BEHAVIOR_TRAIT.WARMONGER)){
			value = 5;
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				value = 0;
			}
		}

		if(chance < value){
			//CANCEL INVASION PLAN
			if (EventManager.Instance.GetEventsOfType (EVENT_TYPES.INVASION_PLAN) != null) {
				List<GameEvent> invasionPlans = EventManager.Instance.GetEventsOfType (EVENT_TYPES.INVASION_PLAN).Where (x => 
				(((InvasionPlan)x).startedByKingdom.id == relationship.sourceKing.city.kingdom.id) &&
				                               (((InvasionPlan)x).targetKingdom.id == relationship.king.city.kingdom.id)).ToList ();
				if (invasionPlans.Count > 0) {
					((InvasionPlan)invasionPlans [0]).CancelEvent ();
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

	internal void StateVisit(Citizen targetKing, STATEVISIT_TRIGGER_REASONS reason){
		int acceptChance = UnityEngine.Random.Range (0, 100);
		int acceptValue = 50;
		RelationshipKings relationship = targetKing.SearchRelationshipByID (this.id);
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND || relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
			acceptValue = 85;
		}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.WARM || relationship.lordRelationship == RELATIONSHIP_STATUS.NEUTRAL){
			acceptValue = 75;
		}
		if(acceptChance < acceptValue){
			if((targetKing.spouse != null && !targetKing.spouse.isDead) || targetKing.city.kingdom.successionLine.Count > 0){
				Citizen visitor = null;
				if((targetKing.spouse != null && !targetKing.spouse.isDead) && targetKing.city.kingdom.successionLine.Count > 0){
					int chance = UnityEngine.Random.Range (0, 2);
					if(chance == 0){
						visitor = targetKing.spouse;
					}else{
						visitor = targetKing.city.kingdom.successionLine [0];
					}
				}else if((targetKing.spouse != null && !targetKing.spouse.isDead) && targetKing.city.kingdom.successionLine.Count <= 0){
					visitor = targetKing.spouse;
				}else if(targetKing.spouse == null && targetKing.city.kingdom.successionLine.Count > 0){
					visitor = targetKing.city.kingdom.successionLine [0];
				}
				if(visitor != null){
					StateVisit stateVisit = new StateVisit(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, this, targetKing.city.kingdom, visitor, reason);
					EventManager.Instance.AddEventToDictionary (stateVisit);
				}
			}else{
				Debug.Log ("STATE VISIT REJECTED RESPECTFULLY");
			}
		}else{
			Debug.Log ("STATE VISIT REJECTED RESPECTFULLY");
		}

	}

	internal void InformedAboutHiddenEvent(GameEvent hiddenEvent, Citizen spy){
		//Reduce relationship between target and source
		if(hiddenEvent is Assassination){
			//An assassination discovered by the target kingdom decreases the target's kingdom relationship by 15
			Kingdom assassinKingdom = ((Assassination)hiddenEvent).assassinKingdom;
			Kingdom targetKingdom = ((Assassination)hiddenEvent).targetCitizen.city.kingdom;

			RelationshipKings relationship = targetKingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
			relationship.AdjustLikeness (-15, EVENT_TYPES.ASSASSINATION, true);
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
			relationship.AdjustLikeness (-35, EVENT_TYPES.INVASION_PLAN, true);

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
		this.campaignManager.successionWarCities.RemoveAll (x => x.city.id == city.id);
	}

	internal void UnsubscribeListeners(){
		EventManager.Instance.onCitizenTurnActions.RemoveListener(TurnActions);
		EventManager.Instance.onUnsupportCitizen.RemoveListener(UnsupportCitizen);
//		EventManager.Instance.onCheckCitizensSupportingMe.RemoveListener(AddPrestigeToOtherCitizen);
		EventManager.Instance.onRemoveSuccessionWarCity.RemoveListener (RemoveSuccessionWarCity);
	}

	internal void DetachGeneralFromCitizen(){
		Debug.Log (this.name + " HAS DETACHED HIS ARMY AND ABANDONED BEING A GENERAL");
		General general = (General)this.assignedRole;
		general.CreateGhostCitizen ();
	}
}
