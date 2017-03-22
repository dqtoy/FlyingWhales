using UnityEngine;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
	public HexTile assignedTile;
	public List<BEHAVIOR_TRAIT> behaviorTraits;
	public List<SKILL_TRAIT> skillTraits;
	public List<MISC_TRAIT> miscTraits;
	public List<Citizen> supportedCitizen;
	public Citizen father;
	public Citizen mother;
	public Citizen spouse;
	public List<Citizen> children;
	public CitizenChances citizenChances;
	public CampaignManager campaignManager;
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
		this.citizenChances = new CitizenChances ();
		this.campaignManager = new CampaignManager (this);
//		this.birthMonth = (MONTH) PoliticsPrototypeManager.Instance.month;
//		this.birthWeek = PoliticsPrototypeManager.Instance.week;
//		this.birthYear = PoliticsPrototypeManager.Instance.year;
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

		EventManager.StartListening ("CitizenTurnActions", TurnActions);

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
		if(this.isPretender){
			Citizen possiblePretender = GetPossiblePretender (this);
			if(possiblePretender != null){
				possiblePretender.isPretender = true;
			}
		}
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
	internal void Campaign(CAMPAIGN type){
		if(this.assignedRole != null){
			if(this.assignedRole is General){
				
			}
		}
	}

	internal void GiveTask(Citizen citizen){
		if(citizen.assignedRole is General){
			TaskForGenerals (citizen);
		}
	}

	internal void TaskForGenerals(Citizen general){
		Campaign chosenCampaign = null;
		for(int i = 0; i < this.campaignManager.activeCampaigns.Count; i++){
			if(this.campaignManager.activeCampaigns[i] != null){
				if(!this.campaignManager.activeCampaigns[i].isFull && !this.campaignManager.activeCampaigns[i].hasStarted){
					chosenCampaign = this.campaignManager.activeCampaigns [i];
					break;
				}
			}
		}

		if(chosenCampaign != null){
			chosenCampaign.registeredGenerals.Add (general);
			AssignCampaignToGeneral (general, chosenCampaign);
			if(chosenCampaign.registeredGenerals.Count >= this.campaignManager.GetGeneralCountByPercentage (20f)){
				chosenCampaign.isFull = true;
			}
		}
	}
	internal void AssignCampaignToGeneral(Citizen general, Campaign chosenCampaign){
		if(chosenCampaign.campaignType == CAMPAIGN.OFFENSE){
			((General)general.assignedRole).targetLocation = chosenCampaign.rallyPoint;
		}else{
			((General)general.assignedRole).targetLocation = chosenCampaign.targetCity.hexTile;
		}
		((General)general.assignedRole).warLeader = chosenCampaign.leader;
		((General)general.assignedRole).campaignID = chosenCampaign.id;
		((General)general.assignedRole).assignedCampaign = chosenCampaign.campaignType;
		((General)general.assignedRole).targetCity = chosenCampaign.targetCity;
		((General)general.assignedRole).location = general.assignedTile;
	}

}
