using UnityEngine;
using System.Collections;

public class CombatManager : MonoBehaviour {
	public static CombatManager Instance;

	void Awake(){
		Instance = this;
	}
	internal void Battle(ref Citizen general1, ref Citizen general2){
		Debug.Log ("BATTLE: (" + general1.city.name + ") " + general1.name + " and (" + general2.city.name + ") " + general2.name);
		Debug.Log ("enemy general army: " + ((General)general1.assignedRole).army.hp);
		Debug.Log ("friendly general army: " + ((General)general2.assignedRole).army.hp);

		float general1HPmultiplier = 1f;
		float general2HPmultiplier = 1f;

		if(((General)general1.assignedRole).assignedCampaign == CAMPAIGN.DEFENSE){
			general1HPmultiplier = 1.20f; //Utilities.defenseBuff
		}
		if(((General)general2.assignedRole).assignedCampaign == CAMPAIGN.DEFENSE){
			general2HPmultiplier = 1.20f; //Utilities.defenseBuff
		}

		int general1TotalHP = (int)(((General)general1.assignedRole).army.hp * general1HPmultiplier);
		int general2TotalHP = (int)(((General)general2.assignedRole).army.hp * general2HPmultiplier);

		if(general1TotalHP > general2TotalHP){
			((General)general1.assignedRole).army.hp -= ((General)general2.assignedRole).army.hp;
			((General)general2.assignedRole).army.hp = 0;
		}else{
			((General)general2.assignedRole).army.hp -= ((General)general1.assignedRole).army.hp;
			((General)general1.assignedRole).army.hp = 0;
		}
		Debug.Log ("RESULTS: " + general1.name + " army hp left: " + ((General)general1.assignedRole).army.hp + "\n" + general2.name + " army hp left: " + ((General)general2.assignedRole).army.hp);
	}
}
