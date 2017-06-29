using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class MarriedCouple {

	public Citizen husband;
	public Citizen wife;

	//protected bool isPregnant;
	//protected int remainingWeeksUntilBirth;

	public List<Citizen> children{
		get{ return this.husband.children.Intersect(this.wife.children).ToList();}
	}

    protected const float PREGNANCY_CHANCE = 0.5f;

	public MarriedCouple(Citizen husband, Citizen wife){
		this.husband = husband;
		this.wife = wife;
        EventManager.Instance.onWeekEnd.AddListener(TurnActions);
        
//		if (this.wife.children.Count < 5 && this.husband.children.Count < 5 && !this.wife.isDead && !this.husband.isDead) {
//			EventManager.Instance.onWeekEnd.AddListener(TurnActions);
//		}
    }

	protected void TurnActions(){
        this.CheckForPregnancy();
//		if (this.wife.children.Count >= 5 || this.husband.children.Count >= 5 || this.wife.isDead || this.husband.isDead) {
//			EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
//			return;
//		}
//		if (this.isPregnant) {
//			this.WaitForBirth();
//		} else {
//			this.CheckForPregnancy();
//		}
    }

	protected void CheckForPregnancy(){
		if (this.wife.isDead) {
			EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
			return;
		}

        if ((this.wife.spouse != null && this.wife.spouse.id != this.husband.id) || 
            (this.husband.spouse != null && this.husband.spouse.id != this.wife.id)) {
            //the couple is already divorced
            EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
            return;
        }

        if (!this.wife.isMarried || !this.husband.isMarried) {
            //the couple is already divorced
            EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
            return;
        }

        //float chanceForPregnancy = 0.2f;

        //		if (this.husband.miscTraits.Contains(MISC_TRAIT.BARREN)) {
        //			return;
        //		}
        //
        //		if (this.wife.miscTraits.Contains(MISC_TRAIT.BARREN)) {
        //			return;
        //		}
        //
        //		if (this.husband.miscTraits.Contains(MISC_TRAIT.HORNY)) {
        //			chanceForPregnancy += 0.2f;
        //		}
        //
        //		if (this.wife.miscTraits.Contains(MISC_TRAIT.HORNY)) {
        //			chanceForPregnancy += 0.2f;
        //		}

        //if (this.wife.age < 20) {
        //	chanceForPregnancy += 0.2f;
        //} else if (this.wife.age < 30) {
        //	chanceForPregnancy += 0.2f;
        //}

        //		if (this.husband.miscTraits.Contains(MISC_TRAIT.HOMOSEXUAL)) {
        //			chanceForPregnancy /= 2f;
        //		}
        //
        //		if (this.wife.miscTraits.Contains(MISC_TRAIT.HOMOSEXUAL)) {
        //			chanceForPregnancy /= 2f;
        //		}

        float pregnancyChance = Random.Range (0f, 100f);
		if (pregnancyChance < PREGNANCY_CHANCE) {
            Citizen baby = MarriageManager.Instance.MakeBaby(this.husband, this.wife);
            //Debug.Log(this.husband.name + " and " + this.wife.name + " has made a baby named: " + baby.name);
            //this.isPregnant = true;
            //this.remainingWeeksUntilBirth = 36;
            //this.wife.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.wife.name + " is pregnant.", HISTORY_IDENTIFIER.NONE));
            //			Debug.Log (this.husband.name + " and " + this.wife.name + " has made a baby and will give birth in 9 months.");
        }
	}

//	protected void WaitForBirth(){
//		if (this.wife.isDead) {
//			EventManager.Instance.onWeekEnd.RemoveListener(TurnActions);
//			this.isPregnant = false;
////			Debug.LogError (this.husband.name + " and " + this.wife.name + "'s baby died because the mother died");
//			return;
//		}

//		if (this.remainingWeeksUntilBirth > 0) {
//			this.remainingWeeksUntilBirth -= 1;
//		} else {
//			//Give Birth
//			Citizen baby = MarriageManager.Instance.MakeBaby(this.husband, this.wife);
//			this.isPregnant = false;
//			this.wife.history.Add(new History(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, this.wife.name + " gave birth to " + baby.name + ".", HISTORY_IDENTIFIER.NONE));
////			Debug.Log (this.husband.name + " and " + this.wife.name + " gave birth to " + baby.name);
//		}
//	}
}
