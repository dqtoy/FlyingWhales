using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Governor : Role {

	public City ownedCity;
	private int _loyalty;

    private string _loyaltySummary; //For UI, to display the factors that affected this governor's loyalty

    private const int defaultLoyalty = 30;

    private int _eventLoyaltyModifier;
    private string _eventLoyaltySummary;


    #region getters/setters
    public int loyalty {
        get { return _loyalty + _eventLoyaltyModifier; }
    }
    public string loyaltySummary {
        get { return this._loyaltySummary + "\n" + _eventLoyaltySummary; }
    }
    #endregion

    public Governor(Citizen citizen): base(citizen){
		this.citizen.city.governor = this.citizen;
		this.citizen.workLocation = this.citizen.city.hexTile;
		this.citizen.isGovernor = true;
		this.citizen.isKing = false;
        this._loyaltySummary = string.Empty;
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;

        this.SetOwnedCity(this.citizen.city);
        this.citizen.GenerateCharacterValues();
        this.UpdateLoyalty ();
	}

	internal void SetOwnedCity(City ownedCity){
		this.ownedCity = ownedCity;
	}
	internal void AdjustLoyalty(int amount){
		this._loyalty += amount;
        this._loyalty = Mathf.Clamp(this._loyalty, -100, 100);
		GovernorEvents ();
	}

	internal void UpdateLoyalty(){
        this._loyaltySummary = string.Empty;
		int baseLoyalty = defaultLoyalty;
        this._loyaltySummary += "+" + defaultLoyalty.ToString() + "   Base value\n";

        Citizen king = this.citizen.city.kingdom.king;
		Kingdom kingdom = this.citizen.city.kingdom;

        List<CHARACTER_VALUE> governorValues = this.citizen.importantCharacterValues.Select(x => x.Key).ToList();
        List<CHARACTER_VALUE> kingValues = king.importantCharacterValues.Select(x => x.Key).ToList();

        List<CHARACTER_VALUE> valuesInCommon = governorValues.Intersect(kingValues).ToList();

        /*POSITIVE ADJUSTMENT OF LOYALTY
		 * */
        if (valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).Count() == 1) {
            int adjustment = 15;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   1 shared value except influence.\n";
        } else if (valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).Count() == 2) {
            int adjustment = 30;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   2 shared values except influence.\n";
        } else if(valuesInCommon.Where(x => x != CHARACTER_VALUE.INFLUENCE).Count() >= 3) {
            int adjustment = 50;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   3 shared values except influence.\n";
        }

        if (governorValues.Contains(CHARACTER_VALUE.HONOR)) {
            int adjustment = 15;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   values honor.\n";
        }

        if (king.IsRelative(this.citizen)) {
            int adjustment = 25;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   Governor is Relative of King.\n";
        }

        if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility >= 0){
            int adjustment = 15;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + "   King is husband of a relative and their compatibility is positive.\n";
        }

        /*NEGATIVE ADJUSTMENT OF LOYALTY
		 * */
        if (!governorValues.Contains(CHARACTER_VALUE.HONOR)) {
            int adjustment = -15;
            baseLoyalty += adjustment;
            this._loyaltySummary += adjustment.ToString() + "   does not value honor.\n";
        }

        if (!governorValues.Contains(CHARACTER_VALUE.INFLUENCE)) {
            int adjustment = -15;
            baseLoyalty += adjustment;
            this._loyaltySummary += adjustment.ToString() + "   does not value influence.\n";
        }

        for (int i = 0; i < kingdom.relationshipsWithOtherKingdoms.Count; i++){
			if(kingdom.relationshipsWithOtherKingdoms[i].isAtWar){
                int adjustment = 10;
                baseLoyalty -= adjustment;
                this._loyaltySummary += "-" + adjustment.ToString() + "   Kingdom is at war.\n";

                int exhaustion = (int)(kingdom.relationshipsWithOtherKingdoms [i].kingdomWar.exhaustion / 10);
				baseLoyalty -= exhaustion;
                this._loyaltySummary += "-" + exhaustion.ToString() + "   War exhaustion.\n";
            }
		}
		if(this.citizen.city.isRaided){
            int adjustment = 10;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + "   Recent Raid.\n";
        }
		if(kingdom.hasConflicted){
            int adjustment = 10;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + "   Recent Border Conflict.\n";
        }
		if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility < 0){
            int adjustment = 15;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + "   King is husband of a relative and their compatibility is negative.\n";
        }

		this._loyalty = 0;
		this.AdjustLoyalty (baseLoyalty);
	}

	internal void AddEventModifier(int modification, string summary, GameEvent gameEventTrigger) {
        this._eventLoyaltyModifier += modification;
        this._eventLoyaltySummary += summary + "\n";
    }
    internal void SetLoyalty(int newLoyalty) {
        this._loyalty = newLoyalty;
    }

    internal void ResetEventModifiers() {
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;
    }

	private void GovernorEvents(){
		TriggerSecession ();
	}
	private void TriggerSecession(){
		if(this._loyalty <= -50 && !this.citizen.city.kingdom.hasSecession){
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 25){
				EventCreator.Instance.CreateSecessionEvent(this.citizen);
			}
		}
	}
}
