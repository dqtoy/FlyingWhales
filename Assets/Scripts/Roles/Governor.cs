using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Governor : Role {

	public City ownedCity;
	internal int loyalty;

    private string _loyaltySummary; //For UI, to display the factors that affected this governor's loyalty

    private const int defaultLoyalty = 30;

    #region getters/setters
    public string loyaltySummary {
        get { return this._loyaltySummary; }
    }
    #endregion

    public Governor(Citizen citizen): base(citizen){
		this.citizen.city.governor = this.citizen;
		this.citizen.workLocation = this.citizen.city.hexTile;
		this.citizen.isGovernor = true;
		this.citizen.isKing = false;
        this._loyaltySummary = string.Empty;
		this.UpdateLoyalty ();
		this.SetOwnedCity(this.citizen.city);
	}

	internal void SetOwnedCity(City ownedCity){
		this.ownedCity = ownedCity;
	}
	internal void AdjustLoyalty(int amount){
		this.loyalty += amount;
		if(this.loyalty > 100){
			this.loyalty = 100;
		}else if(this.loyalty < -100){
			this.loyalty = -100;
		}
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


		this.loyalty = 0;
		this.AdjustLoyalty (baseLoyalty);
	}
}
