using UnityEngine;
using System.Collections;

public class CampaignInfo : MonoBehaviour {

	public Campaign campaign;

	public UILabel lblID;
	public UILabel lblCampaignType;
	public UILabel lblGeneralName;
	public UILabel lblTargetCity;
	public UILabel lblRallyPoint;
	public UILabel lblLeaderName;
	public UILabel lblWarType;
	public UILabel lblNeededArmy;
	public UILabel lblArmy;
	public UILabel lblExpiration;


	public void SetCampaignInfo (Campaign campaign, General general){
		this.campaign = campaign;
		this.lblID.text = "id: " + campaign.id;
		this.lblCampaignType.text = "campaign type: " + campaign.campaignType.ToString ();
		this.lblGeneralName.text = "general: " + general.citizen.name;
		this.lblTargetCity.text = "target city: " + campaign.targetCity.name;
		if(campaign.rallyPoint == null){
			this.lblRallyPoint.text = "rally point: N/A"; 
		}else{
			this.lblRallyPoint.text = "rally point: " + campaign.rallyPoint.name; 
		}

		this.lblLeaderName.text = "leader: " + campaign.leader.name;
		this.lblWarType.text = "war type: " + campaign.warType.ToString ();
		this.lblNeededArmy.text = "needed army: " + campaign.neededArmyStrength.ToString ();
		this.lblArmy.text = "army: " + campaign.GetArmyStrength ().ToString ();

		if(campaign.campaignType == CAMPAIGN.DEFENSE){
			if(campaign.expiration == -1){
				this.lblExpiration.text = "expiration: none";
			}else{
				this.lblExpiration.text = "will expire in " + campaign.expiration + " dayss";
			}
		}else{
			this.lblExpiration.text = "expiration: none";

		}

	}
}
