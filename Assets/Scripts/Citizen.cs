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
	public City city;
	public ROLE role;
	public Role assignedRole;
	public HexTile assignedTile;
	public List<BEHAVIOR_TRAIT> behaviorTraits;
	public List<SKILL_TRAIT> skillTraits;
	public List<MISC_TRAIT> miscTraits;
	public List<Citizen> supportedCitizen;
	public Citizen father;
	public Citizen mother;
	public Citizen spouse;
	public List<Citizen> children;
	public HexTile workLocation;
	public CitizenChances citizenChances;
	public CampaignManager campaignManager;
	public List<RelationshipKings> relationshipKings;
	public MONTH birthMonth;
	public int birthWeek;
	public int birthYear;
	public int previousConversionMonth;
	public int previousConversionYear;
	public bool isIndependent;
	public bool isMarried;
	public bool isDirectDescendant;
	public bool isGovernor;
	public bool isKing;
	public bool isPretender;
	public bool isBusy;
	public bool isDead;

	private List<Citizen> possiblePretenders = new List<Citizen>();
	private string history;


	public Citizen(City city, int age, GENDER gender, int generation){
		this.id = Utilities.SetID (this);
		this.name = "CITIZEN" + this.id;
		this.age = age;
		this.gender = gender;
		this.generation = generation;
		this.city = city;
		this.role = ROLE.UNTRAINED;
		this.assignedRole = null;
		this.assignedTile = null;
		this.behaviorTraits = new List<BEHAVIOR_TRAIT> ();
		this.skillTraits = new List<SKILL_TRAIT> ();
		this.miscTraits = new List<MISC_TRAIT> ();
//		this.trait = GetTrait();
//		this.trait = new TRAIT[]{TRAIT.VICIOUS, TRAIT.NONE};
		this.supportedCitizen = new List<Citizen>(){this.city.kingdom.king}; //initially to king
		this.father = null;
		this.mother = null;
		this.spouse = null;
		this.children = new List<Citizen> ();
		this.workLocation = null;
		this.citizenChances = new CitizenChances ();
		this.campaignManager = new CampaignManager (this);
		this.relationshipKings = new List<RelationshipKings> ();
		this.birthMonth = (MONTH) GameManager.Instance.month;
		this.birthWeek = GameManager.Instance.week;
		this.birthYear = GameManager.Instance.year;
		this.isIndependent = false;
		this.isMarried = false;
		this.isDirectDescendant = false;
		this.isGovernor = false;
		this.isKing = false;
		this.isPretender = false;
		this.isBusy = false;
		this.isDead = false;
		this.history = string.Empty;
		this.city.citizens.Add (this);
//		if(this.kingdom.assignedLord == null){
//			this.loyalLord = this;
//		}else{
//			this.loyalLord = this.kingdom.assignedLord;
//		}

		this.GenerateTraits();

		EventManager.Instance.onCitizenTurnActions.AddListener (TurnActions);
		EventManager.Instance.onMassChangeSupportedCitizen.AddListener (MassChangeSupportedCitizen);	
	}

	internal void GenerateTraits(){
		if (this.mother == null || this.father == null) {
			return;
		}

		//Generate Behaviour trait
		int firstItem = 0;
		int secondItem = 1;
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
			}
		} else {
			skillTraits = Utilities.GetEnumValues<SKILL_TRAIT>().ToList();
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
		AttemptToAge();
		DeathReasons();
	}

	protected void AttemptToAge(){
		if((MONTH)GameManager.Instance.month == this.birthMonth && GameManager.Instance.week == this.birthWeek && GameManager.Instance.year > this.birthYear){
			this.age += 1;
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
		if(isDethroned){
			this.isPretender = true;
		}
		if(this.isPretender){
			Citizen possiblePretender = GetPossiblePretender (this);
			if(possiblePretender != null){
				possiblePretender.isPretender = true;
			}
		}
		this.city.kingdom.successionLine.Remove (this);
		this.isDead = true;
		EventManager.Instance.onCitizenTurnActions.RemoveListener (TurnActions);
		EventManager.Instance.onMassChangeSupportedCitizen.RemoveListener (MassChangeSupportedCitizen);

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


//		RoyaltyEventDelegate.onIncreaseIllnessAndAccidentChance -= IncreaseIllnessAndAccidentChance;
//		RoyaltyEventDelegate.onChangeIsDirectDescendant -= ChangeIsDirectDescendant;
//		RoyaltyEventDelegate.onMassChangeLoyalty -= MassChangeLoyalty;
//		PoliticsPrototypeManager.Instance.turnEnded -= TurnActions;

//
		if (this.id == this.city.kingdom.king.id) {
			//ASSIGN NEW LORD, SUCCESSION
			if (isDethroned) {
				if (newKing != null) {
					this.city.kingdom.AssignNewKing (newKing);
				}
			} else{ 
				if (this.city.kingdom.successionLine.Count <= 0) {
					this.city.kingdom.AssignNewKing (null);
				} else {
					this.city.kingdom.AssignNewKing (this.city.kingdom.successionLine [0]);
				}
			}
		}
	}

	internal void MassChangeSupportedCitizen(Citizen newSupported, Citizen previousSupported){
		if (this.supportedCitizen.Contains(previousSupported)) {
			this.supportedCitizen.Remove (previousSupported);
			if (this.city.kingdom.id != newSupported.city.kingdom.id) {
				this.supportedCitizen.Add(this.city.kingdom.king);
			} else {
				this.supportedCitizen.Add(newSupported);
			}
		}
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
				this.relationshipKings.Add (new RelationshipKings (otherKingdom.king, 0));
			}
		}
	}
	internal bool CheckForSpecificWar(Citizen citizen){
		for(int i = 0; i < this.relationshipKings.Count; i++){
			if(this.relationshipKings[i].king.id == citizen.id){
				if(this.relationshipKings[i].isAtWar){
					return true;
				}
			}
		}
		return false;
	}

}
