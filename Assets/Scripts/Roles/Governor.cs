using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Governor : Role {

	public City ownedCity;
	private int _loyalty;

    private string _loyaltySummary; //For UI, to display the factors that affected this governor's loyalty

    private const int defaultLoyalty = 30;

    private int _eventLoyaltyModifier;
    private string _eventLoyaltySummary;

	internal List<int> plagueHandling;

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
		this.plagueHandling = new List<int> ();
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;

		this.UpdateLoyalty ();
		this.SetOwnedCity(this.citizen.city);
		this.citizen.GenerateCharacterValues ();
	}

	internal void SetOwnedCity(City ownedCity){
		this.ownedCity = ownedCity;
	}
	internal void AdjustLoyalty(int amount){
		this._loyalty += amount;
        this._loyalty = Mathf.Clamp(this._loyalty, -100, 100);
	}
	internal void AdjustPlagueHandling(int amount){
		this.plagueHandling.Add (amount);
	}
	internal void UpdateLoyalty(){
        this._loyaltySummary = string.Empty;
		int baseLoyalty = defaultLoyalty;
        this._loyaltySummary += "+" + defaultLoyalty.ToString() + " Base value\n";

        Citizen king = this.citizen.city.kingdom.king;
		Kingdom kingdom = this.citizen.city.kingdom;

		/*POSITIVE ADJUSTMENT OF LOYALTY
		 * */
		if(this.citizen.hasTrait(TRAIT.HONEST) && king.hasTrait(TRAIT.HONEST)){
            int adjustment = 15;
			baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + " King is also honest.\n";
        }
		if(this.citizen.hasTrait(TRAIT.WARMONGER) && king.hasTrait(TRAIT.WARMONGER)){
            int adjustment = 15;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + " King is also a warmonger.\n";
        } else if(this.citizen.hasTrait(TRAIT.PACIFIST) && king.hasTrait(TRAIT.PACIFIST)){
            int adjustment = 15;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + " King is also a pacifist.\n";
        }
		if(this.citizen.isDirectDescendant){
            int adjustment = 25;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + " King is a relative.\n";
        }
		if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility >= 0){
            int adjustment = 15;
            baseLoyalty += adjustment;
            this._loyaltySummary += "+" + adjustment.ToString() + " King is husband of a relative and their compatibility is positive.\n";
        }

		/*NEGATIVE ADJUSTMENT OF LOYALTY
		 * */
		if(this.citizen.hasTrait(TRAIT.SCHEMING) && king.hasTrait(TRAIT.HONEST)){
            int adjustment = 15;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " King has opposing trait.\n";
        } else if(this.citizen.hasTrait(TRAIT.HONEST) && king.hasTrait(TRAIT.SCHEMING)){
            int adjustment = 15;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " King has opposing trait.\n";
        }
		if(this.citizen.hasTrait(TRAIT.WARMONGER) && king.hasTrait(TRAIT.PACIFIST)){
            int adjustment = 15;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " King has opposing trait.\n";
        } else if(this.citizen.hasTrait(TRAIT.PACIFIST) && king.hasTrait(TRAIT.WARMONGER)){
            int adjustment = 15;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " King has opposing trait.\n";
        }
		if(this.citizen.hasTrait(TRAIT.SCHEMING)){
            int adjustment = 15;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " Governor is scheming.\n";
        }
		if(this.citizen.hasTrait(TRAIT.AMBITIOUS)){
            int adjustment = 20;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " Governor is ambitious.\n";
        }
		for(int i = 0; i < kingdom.relationshipsWithOtherKingdoms.Count; i++){
			if(kingdom.relationshipsWithOtherKingdoms[i].isAtWar){
                int adjustment = 10;
                baseLoyalty -= adjustment;
                this._loyaltySummary += "-" + adjustment.ToString() + " Kingdom is at war.\n";

                int exhaustion = (int)(kingdom.relationshipsWithOtherKingdoms [i].kingdomWar.exhaustion / 10);
				baseLoyalty -= exhaustion;
                this._loyaltySummary += "-" + exhaustion.ToString() + " War exhaustion.\n";
            }
		}
		if(this.citizen.city.isRaided){
            int adjustment = 10;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " Recent Raid.\n";
        }
		if(kingdom.hasConflicted){
            int adjustment = 10;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " Recent Border Conflict.\n";
        }
		if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility < 0){
            int adjustment = 15;
            baseLoyalty -= adjustment;
            this._loyaltySummary += "-" + adjustment.ToString() + " King is husband of a relative and their compatibility is negative.\n";
        }

		/*NEUTRAL ADJUSTMENT OF LOYALTY
		 * */
		for (int i = 0; i < this.plagueHandling.Count; i++) {
			baseLoyalty += this.plagueHandling [i];
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

    private void ResetEventModifiers() {
        this._eventLoyaltyModifier = 0;
        this._eventLoyaltySummary = string.Empty;
    }
}
