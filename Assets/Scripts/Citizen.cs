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
	protected int _prestige;
	public int prestigeFromSupport;
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
	public bool isIndependent;
	public bool isMarried;
	public bool isDirectDescendant;
	public bool isGovernor;
	public bool isKing;
	public bool isPretender;
	public bool isHeir;
	public bool isBusy;
	public bool isDead;

	private List<Citizen> possiblePretenders = new List<Citizen>();
	private string history;

	public int prestige{
		get{ return _prestige + prestigeFromSupport;}
	}

	public List<Citizen> dependentChildren{
		get{ return this.children.Where (x => x.age < 16 && !x.isMarried).ToList ();}
	}

	public List<RelationshipKings> friends{
		get{ return this.relationshipKings.Where(x => x.lordRelationship == RELATIONSHIP_STATUS.FRIEND || x.lordRelationship == RELATIONSHIP_STATUS.ALLY).ToList();}
	}


	public Citizen(City city, int age, GENDER gender, int generation){
		this.id = Utilities.SetID (this);
		this.name = RandomNameGenerator.GenerateRandomName();
		this.age = age;
		this.gender = gender;
		this.generation = generation;
		this._prestige = 0;
		this.prestigeFromSupport = 0;
		this.city = city;
		this.role = ROLE.UNTRAINED;
		this.assignedRole = null;
		this.race = city.kingdom.race;
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
		this.birthWeek = GameManager.Instance.week;
		this.birthYear = GameManager.Instance.year;
		this.isIndependent = false;
		this.isMarried = false;
		this.isDirectDescendant = false;
		this.isGovernor = false;
		this.isKing = false;
		this.isPretender = false;
		this.isHeir = false;
		this.isBusy = false;
		this.isDead = false;
		this.history = string.Empty;
		this.city.citizens.Add (this);


		this.GenerateTraits();
		this.UpdatePrestige();

		EventManager.Instance.onCitizenTurnActions.AddListener(TurnActions);
		EventManager.Instance.onUnsupportCitizen.AddListener(UnsupportCitizen);
		EventManager.Instance.onCheckCitizensSupportingMe.AddListener(AddPrestigeToOtherCitizen);
	}

	internal void GenerateTraits(){
		if (this.mother == null || this.father == null) {
			return;
		}

		//Generate Behaviour trait
		int firstItem = 1;
		int secondItem = 2;
		for (int j = 0; j < 4; j++) {
			BEHAVIOR_TRAIT[] behaviourPair = new BEHAVIOR_TRAIT[2]{(BEHAVIOR_TRAIT)firstItem, (BEHAVIOR_TRAIT)secondItem};
			int chanceForTrait = UnityEngine.Random.Range (0, 100);
			if (chanceForTrait <= 20) {
				//the behaviour pairs are always contradicting
				BEHAVIOR_TRAIT behaviourTrait1 = (BEHAVIOR_TRAIT)firstItem;
				BEHAVIOR_TRAIT behaviourTrait2 = (BEHAVIOR_TRAIT)secondItem;
				int chanceForTrait1 = 50;
				int chanceForTrait2 = 50;

				if (mother.behaviorTraits.Contains (behaviourTrait1)) { chanceForTrait1 += 15; chanceForTrait2 -= 15;}

				if (father.behaviorTraits.Contains (behaviourTrait1)) { chanceForTrait1 += 15; chanceForTrait2 -= 15;}

				if (mother.behaviorTraits.Contains (behaviourTrait2)) { chanceForTrait2 += 15; chanceForTrait1 -= 15;}

				if (father.behaviorTraits.Contains (behaviourTrait2)) { chanceForTrait2 += 15; chanceForTrait1 -= 15;}

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

		//Generate Skill Trait
		int chanceForSkillTraitLength = UnityEngine.Random.Range (0, 100);
		int numOfSkillTraits = 0;
		if (chanceForSkillTraitLength <= 20) {
			numOfSkillTraits = 2;
		} else if (chanceForSkillTraitLength >= 21 && chanceForSkillTraitLength <= 40) {
			numOfSkillTraits = 1;
		}

		List<SKILL_TRAIT> skillTraits = new List<SKILL_TRAIT>();
		if (father.skillTraits.Count > 0 || mother.skillTraits.Count > 0) {
			int skillListChance = UnityEngine.Random.Range (0, 100);
			if (skillListChance < 50) {
				skillTraits.AddRange(father.skillTraits);
				skillTraits.AddRange(mother.skillTraits);
			} else {
				skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
				skillTraits.Remove (SKILL_TRAIT.NONE);
			}
		} else {
			skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
			skillTraits.Remove (SKILL_TRAIT.NONE);
		}
			
		for (int j = 0; j < numOfSkillTraits; j++) {
			SKILL_TRAIT chosenSkillTrait = skillTraits[UnityEngine.Random.Range(0, skillTraits.Count)];
			this.skillTraits.Add (chosenSkillTrait);
			if (numOfSkillTraits > 1) {
				skillTraits.Remove (chosenSkillTrait);
				if (chosenSkillTrait == SKILL_TRAIT.EFFICIENT) {
					skillTraits.Remove (SKILL_TRAIT.INEFFICIENT);
				} else if (chosenSkillTrait == SKILL_TRAIT.INEFFICIENT) {
					skillTraits.Remove (SKILL_TRAIT.EFFICIENT);
				} else if (chosenSkillTrait == SKILL_TRAIT.LAVISH) {
					skillTraits.Remove (SKILL_TRAIT.THRIFTY);
				} else if (chosenSkillTrait == SKILL_TRAIT.THRIFTY) {
					skillTraits.Remove (SKILL_TRAIT.LAVISH);
				}
			}
		}

		//misc traits
		int chanceForMiscTraitLength = UnityEngine.Random.Range (0, 100);
		int numOfMiscTraits = 0;
		if (chanceForMiscTraitLength <= 10) {
			numOfMiscTraits = 2;
		} else if (chanceForMiscTraitLength >= 11 && chanceForMiscTraitLength <= 21) {
			numOfMiscTraits = 1;
		}

		List<MISC_TRAIT> miscTraits = Utilities.GetEnumValues<MISC_TRAIT>().ToList();
		miscTraits.Remove (MISC_TRAIT.NONE);
		for (int j = 0; j < numOfMiscTraits; j++) {
			MISC_TRAIT chosenMiscTrait = miscTraits[UnityEngine.Random.Range(0, miscTraits.Count)];
			this.miscTraits.Add (chosenMiscTrait);
		}

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
	}

	internal void AddChild(Citizen child){
		this.children.Add (child);
	}
	internal void AssignBirthday(MONTH month, int week, int year){
		this.birthMonth = month;
		this.birthWeek = week;
		this.birthYear = year;
	}
	internal void TurnActions(){
		this.AttemptToAge();
		this.DeathReasons();
		this.UpdatePrestige();
		this.CheckSupportExpiration();
	}

	protected void CheckSupportExpiration(){
		if (GameManager.Instance.year == this.supportExpirationYear && GameManager.Instance.month == this.supportExpirationMonth && GameManager.Instance.week == this.supportExpirationWeek) {
			this.supportedCitizen = null;
		}
	}

	protected void AttemptToAge(){
		if((MONTH)GameManager.Instance.month == this.birthMonth && GameManager.Instance.week == this.birthWeek && GameManager.Instance.year > this.birthYear){
			this.age += 1;
			if (this.age >= 16 && !this.isMarried) {
				this.citizenChances.marriageChance += 2;
				this.AttemptToMarry();
			}
			if (this.miscTraits.Contains(MISC_TRAIT.AMBITIOUS)) {
				if (this.isPretender ||
				   (this.city.kingdom.successionLine.Count > 1 && this.city.kingdom.successionLine [1].id == this.id) ||
				   (this.city.kingdom.successionLine.Count > 2 && this.city.kingdom.successionLine [2].id == this.id)) {
					AttemptToGrabPower ();
				}
			}

		}
	}

	protected void AttemptToMarry(){
		Debug.LogError ("Attempt To Marry");
		int chanceToMarry = Random.Range (0, 100);
		this.citizenChances.marriageChance = 100;
		if (chanceToMarry < this.citizenChances.marriageChance) {
			MarriageInvitation marriageInvitation = new MarriageInvitation (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this);
		}
	}

	protected void AttemptToGrabPower(){
		int chanceToGrabPower = Random.Range (0, 100);
		if (chanceToGrabPower < 10) {
			PowerGrab newPowerGrab = new PowerGrab(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this, this.city.kingdom.king);
		}
	}

	internal void DeathReasons(){
		if(isDead){
			return;
		}
		float accidents = UnityEngine.Random.Range (0f, 99f);
		if(accidents <= this.citizenChances.accidentChance){
			Death ();
			string accidentCause = Utilities.accidentCauses[UnityEngine.Random.Range(0, Utilities.accidentCauses.Length)];
			if(this.gender == GENDER.FEMALE){
				StringBuilder stringBuild = new StringBuilder (accidentCause);
				stringBuild.Replace ("He", "She");
				stringBuild.Replace ("he", "she");
				stringBuild.Replace ("him", "her");
				stringBuild.Replace ("his", "her");
			}
			this.history += accidentCause;
			Debug.Log(this.name + ": " + accidentCause);
//			Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.week + "/" + PoliticsPrototypeManager.Instance.year + ": " + this.name + " DIED OF ACCIDENT!");
		}else{
			if(this.age >= 60){
				float oldAge = UnityEngine.Random.Range (0f, 99f);
				if(oldAge <= this.citizenChances.oldAgeChance){
					Death ();
					if(this.gender == GENDER.FEMALE){
						this.history += "She died of old age";
					}else{
						this.history += "He died of old age";
					}
					Debug.Log(this.name + " DIES OF OLD AGE");
//					Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.week + "/" + PoliticsPrototypeManager.Instance.year + ": " + this.name + " DIED OF OLD AGE!");
				}else{
					this.citizenChances.oldAgeChance += 0.05f;
				}
			}
		}
	}
	internal void DeathByStarvation(){
		Death ();
		if(this.gender == GENDER.FEMALE){
			this.history += "She died of starvation";
		}else{
			this.history += "He died of starvation";
		}
		Debug.Log(this.name + " DIES OF STARVATION");
	}
	internal void Death(bool isDethroned = false, Citizen newKing = null){
//		this.kingdom.royaltyList.allRoyalties.Remove (this);
		Debug.Log("DEATH: " + this.name);
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
		this.city.kingdom.successionLine.Remove (this);
		this.isDead = true;
		if (this.assignedRole != null) {
			this.assignedRole.OnDeath ();
		}
		EventManager.Instance.onCitizenTurnActions.RemoveListener (TurnActions);
		EventManager.Instance.onUnsupportCitizen.Invoke (this);
		EventManager.Instance.onUnsupportCitizen.RemoveListener (UnsupportCitizen);
		EventManager.Instance.onCheckCitizensSupportingMe.RemoveListener(AddPrestigeToOtherCitizen);

		if(this.role == ROLE.GENERAL){
			if(((General)this.assignedRole).army.hp <= 0){
				EventManager.Instance.onCitizenMove.RemoveListener (((General)this.assignedRole).Move);
				EventManager.Instance.onRegisterOnCampaign.RemoveListener (((General)this.assignedRole).RegisterOnCampaign);
				EventManager.Instance.onDeathArmy.RemoveListener (((General)this.assignedRole).DeathArmy);
				this.city.citizens.Remove (this);
			}
		}
		if(this.role != ROLE.GENERAL){
			this.city.citizens.Remove (this);
		}
		EventManager.Instance.onCitizenDiedEvent.Invoke ();

		if (this.workLocation != null) {
			this.workLocation.UnoccupyTile();
		}

		if (this.isMarried) {
			MarriageManager.Instance.DivorceCouple (this, spouse);
		}

//		RoyaltyEventDelegate.onIncreaseIllnessAndAccidentChance -= IncreaseIllnessAndAccidentChance;
//		RoyaltyEventDelegate.onChangeIsDirectDescendant -= ChangeIsDirectDescendant;
//		RoyaltyEventDelegate.onMassChangeLoyalty -= MassChangeLoyalty;
//		PoliticsPrototypeManager.Instance.turnEnded -= TurnActions;

		this.isHeir = false;
		if (this.id == this.city.kingdom.king.id) {
			//ASSIGN NEW LORD, SUCCESSION
			KingdomManager.Instance.RemoveRelationshipToOtherKings (this.city.kingdom.king);
			if (isDethroned) {
				if (newKing != null) {
					this.city.kingdom.AssignNewKing (newKing);
				}
				//END SUCCESSION WAR
			} else{ 
				if (this.city.kingdom.successionLine.Count <= 0) {
					this.city.kingdom.AssignNewKing (null);
				} else {
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
			this.assignedRole = new Trader (this, this.city.tradeManager);
		} else if (role == ROLE.GOVERNOR) {
			this.assignedRole = new Governor (this);
			this.city.governor = this;
			this.workLocation = this.city.hexTile;
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
		if (this.father == null || this.mother == null) {
			return true;
		}

		if (otherCitizen.id == this.father.id || otherCitizen.id == this.mother.id) {
			//royalty is father or mother
			return true;
		}

		if (this.father.father != null && this.father.mother != null && this.mother.father != null && this.mother.mother != null) {
			if (otherCitizen.id == this.father.father.id || otherCitizen.id == this.father.mother.id ||
				otherCitizen.id == this.mother.father.id || otherCitizen.id == this.mother.mother.id) {
				//royalty is grand parent
				return true;
			}
		}

		for (int i = 0; i < this.father.children.Count; i++) {
			if(otherCitizen.id == this.father.children[i].id){
				//royalty is sibling
				return true;
			}
		}

		if (this.father.father != null) {
			for (int i = 0; i < this.father.father.children.Count; i++) {
				if (otherCitizen.id == this.father.father.children [i].id) {
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


	internal void UpdatePrestige(){
		int prestige = 0;
		this._prestige = 0;
		this.prestigeFromSupport = 0;
		//compute prestige for role
		if (this.isKing) {
			EventManager.Instance.onCheckCitizensSupportingMe.Invoke(this);
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

			for (int i = 0; i < ((King)this.assignedRole).ownedKingdom.cities.Count; i++) {
				prestige += 15;
			}
		}
		if (this.isGovernor) {
			EventManager.Instance.onCheckCitizensSupportingMe.Invoke(this);
			prestige += 350;

			for (int i = 0; i < ((Governor)this.assignedRole).ownedCity.ownedTiles.Count; i++) {
				prestige += 5;
			}
		}
		if (this.isHeir && this.role != ROLE.GOVERNOR) {
			prestige += 200;
			EventManager.Instance.onCheckCitizensSupportingMe.Invoke(this);
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

		//Add prestige for successors
		this._prestige = prestige;

	}
	internal void AddSuccessionWar(Citizen enemy){
		this.successionWars.Add (enemy);
		this.campaignManager.successionWarCities.Add (new CityWar (enemy.city, false, WAR_TYPE.SUCCESSION));
	}
	internal void RemoveSuccessionWar(Citizen enemy){
		List<Campaign> campaign = this.campaignManager.activeCampaigns.FindAll (x => x.targetCity.id == enemy.city.id);
		for(int i = 0; i < campaign.Count; i++){
			for(int j = 0; j < campaign[i].registeredGenerals.Count; j++){
				((General)campaign[i].registeredGenerals [j].assignedRole).UnregisterThisGeneral (campaign[i]);
			}
			this.campaignManager.activeCampaigns.Remove (campaign[i]);
		}
		this.campaignManager.successionWarCities.RemoveAll (x => x.city.id == enemy.city.id);
		this.successionWars.Remove (enemy);
	}
	internal void DeteriorateRelationship(RelationshipKings relationship){
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
				relationship.king.StateVisit(this);
			}
		}else{
			InvasionPlan (relationship);
			BorderConflict (relationship);
			Assassination (relationship);
		}
	}
	private void InvasionPlan(RelationshipKings relationship){
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 4;
		if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
			value = 8;
		}
		if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.PACIFIST)){
			value = 2;
		}else if(this.behaviorTraits.Contains(BEHAVIOR_TRAIT.WARMONGER)){
			value = 6;
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				value = 12;
			}
		}

		if(chance < value){
			//INVASION PLAN
			InvasionPlan invasionPlan = new InvasionPlan(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, 
				this, this.city.kingdom, relationship.king.city.kingdom);
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
				svValue = 2;
			}
			if(svChance < svValue){
				this.StateVisit(relationship.king);
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
									BorderConflict borderConflict = new BorderConflict (GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, null, relationship.king.city.kingdom, relationship.king.city.kingdom.relationshipsWithOtherKingdoms [i].objectInRelationship);
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
	private void Assassination(RelationshipKings relationship){
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
			Assassination assassination = new Assassination(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this, relationship.king);
			EventManager.Instance.AddEventToDictionary(assassination);
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
				List<GameEvent> invasionPlans = EventManager.Instance.GetEventsOfType(EVENT_TYPES.INVASION_PLAN).Where(x => 
					(((InvasionPlan)x).startedByKingdom.id == relationship.sourceKing.city.kingdom.id) && 
					(((InvasionPlan)x).targetKingdom.id == relationship.king.city.kingdom.id)).ToList();
				if (invasionPlans.Count > 0) {
					((InvasionPlan)invasionPlans [0]).CancelEvent();
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
			List<GameEvent> invasionPlans = EventManager.Instance.GetEventsOfType(EVENT_TYPES.INVASION_PLAN).Where(x => 
				(((InvasionPlan)x).startedByKingdom.id == relationship.sourceKing.city.kingdom.id) && 
				(((InvasionPlan)x).targetKingdom.id == relationship.king.city.kingdom.id)).ToList();
			if (invasionPlans.Count > 0) {
				((InvasionPlan)invasionPlans [0]).CancelEvent();
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
	protected void AddPrestigeToOtherCitizen(Citizen otherCitizen){
		if (this.supportedCitizen == null) {
			if (otherCitizen.city.kingdom.id == this.city.kingdom.id) {
				if (otherCitizen.isKing) {
					if (this.isGovernor) {
						otherCitizen.prestigeFromSupport += 20;
					}
				} else if (otherCitizen.isHeir) {
					if (this.isGovernor) {
						otherCitizen.prestigeFromSupport += 20;
					} else if (this.isKing) {
						otherCitizen.prestigeFromSupport += 60;
					}
				}
			}
		} else {
			if (this.supportedCitizen.id == otherCitizen.id) {
				if (this.isGovernor) {
					otherCitizen.prestigeFromSupport += 20;
				} else if (this.isKing) {
					otherCitizen.prestigeFromSupport += 60;
				}
			}
		}
	}

	internal void StateVisit(Citizen targetKing){
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
					StateVisit stateVisit = new StateVisit(GameManager.Instance.week, GameManager.Instance.month, GameManager.Instance.year, this, targetKing.city.kingdom, visitor);
					EventManager.Instance.AddEventToDictionary (stateVisit);
				}
			}else{
				Debug.Log ("STATE VISIT REJECTED RESPECTFULLY");
			}
		}else{
			Debug.Log ("STATE VISIT REJECTED RESPECTFULLY");
		}

	}

	internal void InformedAboutHiddenEvent(GameEvent hiddenEvent){
		//Reduce relationship between target and source
		if(hiddenEvent is Assassination){
			//An assassination discovered by the target kingdom decreases the target's kingdom relationship by 15
			Kingdom assassinKingdom = ((Assassination)hiddenEvent).assassinKingdom;
			Kingdom targetKingdom = ((Assassination)hiddenEvent).targetCitizen.city.kingdom;

			RelationshipKings relationship = targetKingdom.king.SearchRelationshipByID (assassinKingdom.king.id);
			relationship.AdjustLikeness (-15);
		}else if(hiddenEvent is InvasionPlan){
			//An Invasion Plan discovered by the target Kingdom decreases the target kingdom's King's relationship by 20
			Kingdom sourceKingdom = ((InvasionPlan)hiddenEvent).sourceKingdom;
			Kingdom targetKingdom = ((InvasionPlan)hiddenEvent).targetKingdom;

			RelationshipKings relationship = targetKingdom.king.SearchRelationshipByID (sourceKingdom.king.id);
			relationship.AdjustLikeness (-35);
		}

		//Perform Counteraction

	}

	internal List<Citizen> GetCitizensSupportingThisCitizen(){
		List<Citizen> citizensSupportingMe = new List<Citizen>();
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			List<Citizen> allCitizensInKingdom = KingdomManager.Instance.allKingdoms[i].GetAllCitizensInKingdom();
			for (int j = 0; j < allCitizensInKingdom.Count; j++) {
				if (allCitizensInKingdom[i].supportedCitizen == null) {
					if ((this.isKing || this.isHeir) && this.city.kingdom.id == allCitizensInKingdom [i].city.kingdom.id) {
						citizensSupportingMe.Add(allCitizensInKingdom[i]);
					}
				} else {
					if (allCitizensInKingdom [i].supportedCitizen.id == this.id) {
						citizensSupportingMe.Add(allCitizensInKingdom[i]);
					}
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
}
