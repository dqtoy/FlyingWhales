using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Governor : Role {

	public City ownedCity;
	private int loyalty;
	public Governor(Citizen citizen): base(citizen){
		this.citizen.city.governor = this.citizen;
		this.citizen.workLocation = this.citizen.city.hexTile;
		this.citizen.isGovernor = true;
		this.citizen.isKing = false;
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
		int baseLoyalty = 30;
		Citizen king = this.citizen.city.kingdom.king;
		Kingdom kingdom = this.citizen.city.kingdom;

		/*POSITIVE ADJUSTMENT OF LOYALTY
		 * */
		if(this.citizen.hasTrait(TRAIT.HONEST) && king.hasTrait(TRAIT.HONEST)){
			baseLoyalty += 15;
		}
		if(this.citizen.hasTrait(TRAIT.WARMONGER) && king.hasTrait(TRAIT.WARMONGER)){
			baseLoyalty += 15;
		}else if(this.citizen.hasTrait(TRAIT.PACIFIST) && king.hasTrait(TRAIT.PACIFIST)){
			baseLoyalty += 15;
		}
		if(this.citizen.isDirectDescendant){
			baseLoyalty += 25;
		}
		if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility >= 0){
			baseLoyalty += 15;
		}

		/*NEGATIVE ADJUSTMENT OF LOYALTY
		 * */
		if(this.citizen.hasTrait(TRAIT.SCHEMING) && king.hasTrait(TRAIT.HONEST)){
			baseLoyalty -= 15;
		}else if(this.citizen.hasTrait(TRAIT.HONEST) && king.hasTrait(TRAIT.SCHEMING)){
			baseLoyalty -= 15;
		}
		if(this.citizen.hasTrait(TRAIT.WARMONGER) && king.hasTrait(TRAIT.PACIFIST)){
			baseLoyalty -= 15;
		}else if(this.citizen.hasTrait(TRAIT.PACIFIST) && king.hasTrait(TRAIT.WARMONGER)){
			baseLoyalty -= 15;
		}
		if(this.citizen.hasTrait(TRAIT.SCHEMING)){
			baseLoyalty -= 15;
		}
		if(this.citizen.hasTrait(TRAIT.AMBITIOUS)){
			baseLoyalty -= 20;
		}
		for(int i = 0; i < kingdom.relationshipsWithOtherKingdoms.Count; i++){
			if(kingdom.relationshipsWithOtherKingdoms[i].isAtWar){
				baseLoyalty -= 10;
				int exhaustion = (int)(kingdom.relationshipsWithOtherKingdoms [i].kingdomWar.exhaustion / 10);
				baseLoyalty -= exhaustion;
			}
		}
		if(this.citizen.city.isRaided){
			baseLoyalty -= 10;
		}
		if(kingdom.hasConflicted){
			baseLoyalty -= 10;
		}
		if(king.spouse != null && this.citizen.IsRelative(king.spouse) && ((Spouse)king.spouse)._marriageCompatibility < 0){
			baseLoyalty -= 15;
		}


		this.loyalty = 0;
		this.AdjustLoyalty (baseLoyalty);
	}
}
