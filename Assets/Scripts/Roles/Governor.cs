using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class Governor : Role {

	public City ownedCity;
	private int _loyalty;
    private const int defaultLoyalty = 0;
    private int _eventLoyaltyModifier;
    public int forTestingLoyaltyModifier;

    private string _loyaltySummary; //For UI, to display the factors that affected this governor's loyalty
    internal string _eventLoyaltySummary;

	private List<ExpirableModifier> _eventModifiers;

	private bool isInitial;

    #region getters/setters
    public int loyalty {
		get { return (_loyalty + _eventLoyaltyModifier + forTestingLoyaltyModifier); }
    }
    public string loyaltySummary {
        get { return this._loyaltySummary + _eventLoyaltySummary; }
    }
	public List<ExpirableModifier> eventModifiers {
		get { return this._eventModifiers; }
	}
    #endregion

    public Governor(Citizen citizen): base(citizen){
		this.citizen.city.governor = this.citizen;
        this._loyaltySummary = string.Empty;
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;
		this._eventModifiers = new List<ExpirableModifier> ();
		this.isInitial = true;
        this.SetOwnedCity(this.citizen.city);
	}
	internal void SetOwnedCity(City ownedCity){
		this.ownedCity = ownedCity;
	}
	internal void AdjustLoyalty(int amount){
		this._loyalty += amount;
        this._loyalty = Mathf.Clamp(this._loyalty, -100, 100);
	}

	internal void AddEventModifier(int modification, string summary, GameEvent gameEventTrigger) {
		GameDate dateToUse = new GameDate (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
		dateToUse.AddMonths(3);
		ExpirableModifier expMod = new ExpirableModifier (gameEventTrigger, summary, dateToUse, modification);
		this._eventModifiers.Add(expMod);
        _eventLoyaltyModifier += modification;
        SchedulingManager.Instance.AddEntry (expMod.dueDate.month, expMod.dueDate.day, expMod.dueDate.year, () => RemoveEventModifier(expMod));
		GovernorEvents ();
//        this._eventLoyaltyModifier += modification;
//        if(_eventLoyaltyModifier < 0) {
//            this._eventLoyaltySummary = "-" + _eventLoyaltyModifier.ToString() + "   Approval";
//        } else if (_eventLoyaltyModifier > 0) {
//            this._eventLoyaltySummary = "+" + _eventLoyaltyModifier.ToString() + "   Approval";
//        } else {
//            this._eventLoyaltySummary = _eventLoyaltyModifier.ToString() + "   Approval";
//        }
    }
//	private void UpdateEventModifiers(){
//		this._eventLoyaltyModifier = 0;
//		this._eventLoyaltySummary = string.Empty;
//		List<EVENT_TYPES> eventTypes = this._eventModifiers.Keys;
//		for (int i = 0; i < eventTypes.Count; i++) {
//			for (int j = 0; j < this._eventModifiers[eventTypes[i]].Count; j++) {
//				ExpirableModifier expMod = this._eventModifiers [eventTypes [i]] [j];
//				this._eventLoyaltyModifier += expMod.modifier;
//				if(expMod.modifier < 0){
//					this._eventLoyaltySummary += "-" + expMod.modifier.ToString() + " " + expMod.modifierReason;
//				}else{
//					this._eventLoyaltySummary += "+" + expMod.modifier.ToString() + " " + expMod.modifierReason;
//				}
//			}
//		}
//	}

	//private void CheckEventModifiers(){
	//	for (int i = 0; i < this._eventModifiers.Count; i++) {
	//		ExpirableModifier expMod = this._eventModifiers [i];
	//		if(expMod.dueDate.day == GameManager.Instance.days && expMod.dueDate.month == GameManager.Instance.month && expMod.dueDate.year == GameManager.Instance.year){
	//			RemoveEventModifierAt (i);
	//			i--;
	//		}

	//	}
	//}

	private void RemoveEventModifierAt(int index){
        ExpirableModifier modToRemove = this._eventModifiers[index];
        if (modToRemove.modifier < 0) {
            //if the modifier is negative, return the previously subtracted value
            _eventLoyaltyModifier += Mathf.Abs(modToRemove.modifier);
        } else {
            //if the modifier is positive, subtract the amount that was previously added
            _eventLoyaltyModifier -= modToRemove.modifier;
        }
        this._eventModifiers.RemoveAt (index);

	}
	private void RemoveEventModifier(ExpirableModifier expMod){
		if(!this.citizen.isDead){
			if (expMod.modifier < 0) {
				//if the modifier is negative, return the previously subtracted value
				_eventLoyaltyModifier += Mathf.Abs(expMod.modifier);
			} else {
				//if the modifier is positive, subtract the amount that was previously added
				_eventLoyaltyModifier -= expMod.modifier;
			}
			this._eventModifiers.Remove (expMod);		
		}
	}
	internal void SetLoyalty(int newLoyalty) {
        this._loyalty = newLoyalty;
    }

    internal void ResetEventModifiers() {
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;
    }

	private void GovernorEvents(){
		if(!this.isInitial){
			TriggerSecession ();
		}else{
			this.isInitial = false;
		}
	}
	private void TriggerSecession(){
		if(this.loyalty <= -50 && !this.citizen.city.kingdom.hasSecession){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 25){
				EventCreator.Instance.CreateSecessionEvent(this.citizen);
			}
		}
	}

	internal override void OnDeath (){
		base.OnDeath ();
        //Remove Family Of Governor from kingdom
        List<Citizen> familyOfGovernor = new List<Citizen>();
        familyOfGovernor.AddRange(this.citizen.GetRelatives(-1));
        familyOfGovernor.Add(this.citizen);
        for (int i = 0; i < familyOfGovernor.Count; i++) {
            Citizen currFamilyMember = familyOfGovernor[i];
            ownedCity.kingdom.RemoveCitizenFromKingdom(currFamilyMember, currFamilyMember.city);
        }

        ownedCity.kingdom.CreateNewGovernorFamily(ownedCity);
	}
}
