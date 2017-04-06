using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MarriedCouple {

	public Citizen husband;
	public Citizen wife;

	protected bool isPregnant;
	protected int remainingWeeksUntilBirth;

	public List<Citizen> children{
		get{ return this.husband.children.Intersect(this.wife.children).ToList();}
	}

	public MarriedCouple(Citizen husband, Citizen wife){
		this.husband = husband;
		this.wife = wife;

//		if (this.wife.children.Count < 5 && this.husband.children.Count < 5) {
//			EventManager.Instance.onWeekEnd.AddListener(TurnActions);
//		}
	}

	protected void TurnActions(){
		if (this.wife.children.Count < 5 && this.husband.children.Count < 5) {
			EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
			return;
		}
		if (this.isPregnant) {
			this.WaitForBirth();
		} else {
			this.CheckForPregnancy();
		}
	}

	protected void CheckForPregnancy(){
		if (this.wife.isDead || this.husband.isDead) {
			EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
			return;
		}

		float chanceForPregnancy = 0.2f;

		if (this.husband.miscTraits.Contains(MISC_TRAIT.BARREN)) {
			return;
		}

		if (this.wife.miscTraits.Contains(MISC_TRAIT.HORNY)) {
			return;
		}

		if (this.husband.miscTraits.Contains(MISC_TRAIT.HORNY)) {
			chanceForPregnancy += 0.2f;
		}

		if (this.wife.miscTraits.Contains(MISC_TRAIT.HORNY)) {
			chanceForPregnancy += 0.2f;
		}

		if (this.wife.age < 20) {
			chanceForPregnancy += 0.2f;
		} else if (this.wife.age < 30) {
			chanceForPregnancy += 0.2f;
		}

		if (this.husband.miscTraits.Contains(MISC_TRAIT.HOMOSEXUAL)) {
			chanceForPregnancy /= 2f;
		}

		if (this.wife.miscTraits.Contains(MISC_TRAIT.HOMOSEXUAL)) {
			chanceForPregnancy /= 2f;
		}

		float pregnancyChance = Random.Range (0f, 100f);
		if (pregnancyChance < chanceForPregnancy) {
			this.isPregnant = true;
			this.remainingWeeksUntilBirth = 36;
			this.wife.history.Add(new History(GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, this.wife.name + " is pregnant.", HISTORY_IDENTIFIER.NONE));
		}
	}

	protected void WaitForBirth(){
		if (this.wife.isDead) {
			EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
			this.isPregnant = false;
			Debug.Log ("the baby died because the mother died");
			return;
		}

		if (this.remainingWeeksUntilBirth > 0) {
			this.remainingWeeksUntilBirth -= 1;
		} else {
			//Give Birth
			Citizen child = MarriageManager.Instance.MakeBaby(this.husband, this.wife);
			this.isPregnant = false;
			this.wife.history.Add(new History(GameManager.Instance.month, GameManager.Instance.week, GameManager.Instance.year, this.wife.name + " gave birth to" + child.name + ".", HISTORY_IDENTIFIER.NONE));

		}
	}
}
