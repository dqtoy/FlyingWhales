using UnityEngine;
using System.Collections;

public class King : Role {

	public Kingdom ownedKingdom;

	public King(Citizen citizen): base(citizen){
		this.citizen.isKing = true;
//		if(this.citizen.city.kingdom.king != null){
//			this.citizen.CopyCampaignManager (this.citizen.city.kingdom.king.campaignManager);
//		}
		this.citizen.city.kingdom.king = this.citizen;
		this.SetOwnedKingdom(this.citizen.city.kingdom);
		this.citizen.GenerateCharacterValues ();
		if(this.citizen.city.kingdom.plague != null){
			this.citizen.city.kingdom.plague.UpdateApproach (this.citizen.city.kingdom);
		}
	}

	internal void SetOwnedKingdom(Kingdom ownedKingdom){
		this.ownedKingdom = ownedKingdom;
	}
}
