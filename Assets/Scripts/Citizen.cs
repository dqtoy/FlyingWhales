using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Citizen {

	public int id;
	public string name;
	public GENDER gender;
	public int age;
	public int generation;
	public City city;
	public ROLE role;
	public Role assignedRole;
	public List<BEHAVIOR_TRAIT> behaviorTraits;
	public List<SKILL_TRAIT> skillTraits;
	public List<MISC_TRAIT> miscTraits;
	public Citizen supportedCitizen;
	public Citizen possiblePretender;
	public Citizen father;
	public Citizen mother;
	public Citizen spouse;
	public List<Citizen> children;
	public CitizenChances citizenChances;
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
	public bool isDead;

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
//		this.trait = GetTrait();
//		this.trait = new TRAIT[]{TRAIT.VICIOUS, TRAIT.NONE};
		this.supportedCitizen = null; //initially to king
		this.possiblePretender = null;
		this.father = null;
		this.mother = null;
		this.spouse = null;
		this.children = new List<Citizen> ();
		this.citizenChances = new CitizenChances ();
//		this.birthMonth = (MONTH) PoliticsPrototypeManager.Instance.month;
//		this.birthWeek = PoliticsPrototypeManager.Instance.week;
//		this.birthYear = PoliticsPrototypeManager.Instance.year;
		this.isIndependent = false;
		this.isMarried = false;
		this.isDirectDescendant = false;
		this.isGovernor = false;
		this.isKing = false;
		this.isPretender = false;
		this.isDead = false;
		this.history = string.Empty;
		this.city.citizens.Add (this);
//		if(this.kingdom.assignedLord == null){
//			this.loyalLord = this;
//		}else{
//			this.loyalLord = this.kingdom.assignedLord;
//		}

		EventManager.StartListening ("CitizenTurnActions", TurnActions);

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
//		CheckAge ();
		DeathReasons ();
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
					this.citizenChances.oldAgeChance+= 0.05f;
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
	internal void Death(){
//		this.kingdom.royaltyList.allRoyalties.Remove (this);
		this.city.kingdom.successionLine.Remove (this);
		this.isDead = true;
		EventManager.StopListening ("CitizenTurnActions", TurnActions);
//		RoyaltyEventDelegate.onIncreaseIllnessAndAccidentChance -= IncreaseIllnessAndAccidentChance;
//		RoyaltyEventDelegate.onChangeIsDirectDescendant -= ChangeIsDirectDescendant;
//		RoyaltyEventDelegate.onMassChangeLoyalty -= MassChangeLoyalty;
//		PoliticsPrototypeManager.Instance.turnEnded -= TurnActions;
//
//		if(this.id == this.kingdom.assignedLord.id){
//			//ASSIGN NEW LORD, SUCCESSION
//			if (this.kingdom.royaltyList.successionRoyalties.Count <= 0) {
//				this.kingdom.AssignNewLord (null);
//			} else {
//				this.kingdom.AssignNewLord (this.kingdom.royaltyList.successionRoyalties [0]);
//			}
//		}
	}

	internal void Campaign(CAMPAIGN type){
		if(this.assignedRole != null){
			if(this.assignedRole is General){
				
			}
		}
	}
}
