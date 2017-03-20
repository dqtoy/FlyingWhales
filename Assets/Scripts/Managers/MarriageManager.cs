using UnityEngine;
using System.Collections;

public class MarriageManager : MonoBehaviour {

	public static MarriageManager Instance;


	void Awake(){
		Instance = this;
	}
	internal Citizen MakeBaby(Citizen father, Citizen mother, int age = 0, bool isLord = false){
		GENDER gender = (GENDER)(UnityEngine.Random.Range (0, System.Enum.GetNames (typeof(GENDER)).Length));
		//		int age = 0;
		if(isLord){
			int randomGender = UnityEngine.Random.Range (0, 100);
			if(randomGender < 20){
				gender = GENDER.FEMALE;
			}else{
				gender = GENDER.MALE;
			}

			//			age = UnityEngine.Random.Range (16, 36);
		}
		Citizen child = new Citizen(father.kingdom, age, gender, father.generation + 1);
		if(father.isDirectDescendant || mother.isDirectDescendant){
			child.isDirectDescendant = true;
		}
		father.AddChild (child);
		mother.AddChild (child);
		child.AddParents(father, mother);
		if(child.isDirectDescendant){
			child.kingdom.UpdateKingSuccession ();
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
		Citizen spouse = new Citizen(otherSpouse.kingdom, age, gender, otherSpouse.generation);

		Marry (otherSpouse, spouse);
		return spouse;
	}

	internal void Marry(Citizen husband, Citizen wife){
//		Debug.Log (PoliticsPrototypeManager.Instance.month + "/" + PoliticsPrototypeManager.Instance.week + "/" + PoliticsPrototypeManager.Instance.year + ": " + husband.name + " got married to " + wife.name);
		husband.spouse = wife;
		wife.spouse = husband;
		husband.isMarried = true;
		wife.isMarried = true;
		husband.isIndependent = true;
		wife.isIndependent = true;

		if (wife.kingdom.king.id == wife.id) {
			//if wife is currently queen of a kingdom, the husband will recieve the kingdom
//			wife.kingdom.AssimilateKingdom (husband.kingdom);
		}

		//the wife will transfer to the court of the husband
		wife.kingdom = husband.kingdom;
		//		wife.loyalLord = husband.kingdom.assignedLord;
		//		husband.kingdom.royaltyList.allRoyalties.Add(wife);
		//		wife.kingdom.royaltyList.allRoyalties.Remove(wife);

//		husband.kingdom.marriedCouples.Add(new MarriedCouple (husband, wife));
	}
}
