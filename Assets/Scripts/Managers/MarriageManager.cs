using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MarriageManager : MonoBehaviour {

	public static MarriageManager Instance;

	public List<MarriedCouple> allMarriedCouples;

	void Awake(){
		Instance = this;
		allMarriedCouples = new List<MarriedCouple>();
	}

	internal Citizen MakeBaby(Citizen father, Citizen mother, int age = 0){
		GENDER gender = (GENDER)(UnityEngine.Random.Range (0, System.Enum.GetNames (typeof(GENDER)).Length));
		//		int age = 0;

		Citizen child = new Citizen(father.city, age, gender, father.generation + 1);
		child.AssignBirthday((MONTH)GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		if(father.isDirectDescendant || mother.isDirectDescendant){
			child.isDirectDescendant = true;
		}
		father.AddChild (child);
		mother.AddChild (child);
		child.AddParents(father, mother);
		if(child.isDirectDescendant){
			child.city.kingdom.successionLine.Add (child);
			child.city.kingdom.UpdateKingSuccession ();
		}
		//		father.kingdom.royaltyList.allRoyalties.Add (child);

		return child;
	}

	internal Citizen CreateSpouse(Citizen otherSpouse){
		GENDER gender = GENDER.FEMALE;
		int age = UnityEngine.Random.Range (20, (otherSpouse.age + 1));
		if(otherSpouse.gender == GENDER.FEMALE){
			gender = GENDER.MALE;
			int lowerLimit = otherSpouse.age - 10;
			if(lowerLimit < 20){
				lowerLimit = 20;
			}
			age = UnityEngine.Random.Range (lowerLimit, (otherSpouse.age + 11));
		}
		Citizen spouse = new Citizen(otherSpouse.city, age, gender, otherSpouse.generation);
		Citizen father = new Citizen (otherSpouse.city, UnityEngine.Random.Range (60, 81), GENDER.MALE, 1);
		Citizen mother = new Citizen (otherSpouse.city, UnityEngine.Random.Range (60, 81), GENDER.FEMALE, 1);

		father.name = RandomNameGenerator.Instance.GenerateRandomName (father.race, father.gender);
		mother.name = RandomNameGenerator.Instance.GenerateRandomName (mother.race, mother.gender);

		father.AddChild(spouse);
		mother.AddChild(spouse);
		spouse.AddParents(father, mother);

		father.isDirectDescendant = true;
		mother.isDirectDescendant = true;
		father.isDead = true;
		mother.isDead = true;

		otherSpouse.city.citizens.Remove(father);
		otherSpouse.city.citizens.Remove(mother);

		father.UnsubscribeListeners();
		mother.UnsubscribeListeners();

		MONTH monthFather = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthMother = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));
		MONTH monthSpouse = (MONTH)(UnityEngine.Random.Range (1, System.Enum.GetNames (typeof(MONTH)).Length));

		spouse.AssignBirthday (monthSpouse, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthSpouse] + 1), (GameManager.Instance.year - spouse.age));
		father.AssignBirthday (monthFather, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthFather] + 1), GameManager.Instance.year - father.age);
		mother.AssignBirthday (monthMother, UnityEngine.Random.Range (1, GameManager.daysInMonth[(int)monthMother] + 1), GameManager.Instance.year - mother.age);

		Marry (otherSpouse, spouse);
		return spouse;
	}

	internal void Marry(Citizen citizen1, Citizen citizen2){
//		Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.days + "/" + PoliticsPrototypeManager.Instance.year + ": " + husband.name + " got married to " + wife.name);
		citizen1.spouse = citizen2;
		citizen2.spouse = citizen1;
		citizen1.isMarried = true;
		citizen2.isMarried = true;
		citizen1.isIndependent = true;
		citizen2.isIndependent = true;

//		if (wife.city.kingdom.king.id == wife.id) {
			//if wife is currently queen of a kingdom, the husband will recieve the kingdom
//			wife.kingdom.AssimilateKingdom (husband.kingdom);
//		}

		//the wife will transfer to the court of the husband
//		wife.city.kingdom = husband.city.kingdom;
		//		wife.loyalLord = husband.kingdom.assignedLord;
		//		husband.kingdom.royaltyList.allRoyalties.Add(wife);
		//		wife.kingdom.royaltyList.allRoyalties.Remove(wife);
		citizen1.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, citizen1.name + " married " + citizen2.name + ".", HISTORY_IDENTIFIER.NONE));
		citizen2.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, citizen2.name + " married " + citizen1.name + ".", HISTORY_IDENTIFIER.NONE));

		if (citizen1.gender == GENDER.MALE) {
			this.allMarriedCouples.Add (new MarriedCouple (citizen1, citizen2));
			if (citizen1.race == RACE.HUMANS && citizen2.race == RACE.HUMANS) {
				citizen2.ChangeSurname (citizen1);
			}
		} else {
			this.allMarriedCouples.Add (new MarriedCouple (citizen2, citizen1));
			if (citizen1.race == RACE.HUMANS && citizen2.race == RACE.HUMANS) {
				citizen1.ChangeSurname (citizen2);
			}
		}



	}

	internal List<Citizen> GetElligibleCitizensForMarriage(Citizen citizenSearchingForLove){
		List<Citizen> elligibleCitizens = new List<Citizen>();
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			List<Citizen> elligibleCitizensInKingdom = KingdomManager.Instance.allKingdoms[i].GetAllCitizensForMarriage(citizenSearchingForLove);
			for (int j = 0; j < elligibleCitizensInKingdom.Count; j++) {
				if (elligibleCitizensInKingdom[j].age > (citizenSearchingForLove.age + 10)) {
					continue;
				}
				if (elligibleCitizensInKingdom [j].city.kingdom != citizenSearchingForLove.city.kingdom) {
					RelationshipKings relKing = citizenSearchingForLove.city.kingdom.king.GetRelationshipWithCitizen (elligibleCitizensInKingdom [j].city.kingdom.king);
//					RELATIONSHIP_STATUS rel = citizenSearchingForLove.city.kingdom.king.GetRelationshipWithCitizen (elligibleCitizensInKingdom [j].city.kingdom.king).lordRelationship;
					if (relKing == null) {
						Debug.LogError (citizenSearchingForLove.city.kingdom.king.name + " has null relationship with " + elligibleCitizensInKingdom [j].city.kingdom.king.name);
						continue;
					}
					if (relKing.lordRelationship == RELATIONSHIP_STATUS.ENEMY ||
						relKing.lordRelationship == RELATIONSHIP_STATUS.RIVAL) {
						continue;
					}
				}
				if (citizenSearchingForLove.race != elligibleCitizensInKingdom[j].race) {
					continue;
				}
				if (citizenSearchingForLove.IsRoyaltyCloseRelative (elligibleCitizensInKingdom [j])) {
					continue;
				}

				if ((citizenSearchingForLove.isKing && !elligibleCitizensInKingdom[j].isKing) || (!citizenSearchingForLove.isKing && elligibleCitizensInKingdom[j].isKing) ||
					(!citizenSearchingForLove.isKing && !elligibleCitizensInKingdom[j].isKing)) {
					if (elligibleCitizensInKingdom[j].prestige <= citizenSearchingForLove.prestige * 1.25f && 
						elligibleCitizensInKingdom[j].prestige >= citizenSearchingForLove.prestige * 0.75f) {
						elligibleCitizens.Add (elligibleCitizensInKingdom [j]);
					}
				}
			}
		}
		return elligibleCitizens.OrderBy(x => x.age).ThenByDescending(x => x.city.kingdom.cities.Count).ToList(); //younger women are prioritized and women with more cities
	}

	internal void DivorceCouple(Citizen citizen1, Citizen citizen2){
		citizen1.isMarried = false;
		citizen2.isMarried = false;

		citizen1.spouse = null;
		citizen2.spouse = null;

		for (int i = 0; i < this.allMarriedCouples.Count; i++) {
			if (citizen1.gender == GENDER.MALE) {
				if (this.allMarriedCouples [i].husband.id == citizen1.id && this.allMarriedCouples [i].wife.id == citizen2.id) {
					this.allMarriedCouples.RemoveAt(i);
					return;
				}
			} else {
				if (this.allMarriedCouples [i].wife.id == citizen1.id && this.allMarriedCouples [i].husband.id == citizen2.id) {
					this.allMarriedCouples.RemoveAt(i);
					return;
				}
			}
		}
	}

	internal List<MarriedCouple> GetCouplesCitizenInvoledIn(Citizen citizen){
		return allMarriedCouples.Where(x => x.husband.id == citizen.id || x.wife.id == citizen.id).ToList();
	}
}
