using UnityEngine;
using System.Collections;

public class HoverAvatar : MonoBehaviour {
	public CitizenAvatar citizenAvatar;
	string summary = string.Empty;

    void OnMouseOver() {
		if (citizenAvatar.citizenRole is General) {
			General general = (General)citizenAvatar.citizenRole;
			summary = "NAME: " + general.citizen.name.ToString();
			if(general.generalTask != null){
				summary += "\nTASK: " + general.generalTask.task.ToString();
				summary += "\nTARGET HEXTILE: " + general.generalTask.targetHextile.name.ToString();
				if(general.generalTask.targetHextile.city != null){
					summary += "\nTARGET CITY: " + general.generalTask.targetHextile.city.name.ToString();
				}
				summary += "\nMOVE DATE: " + ((MONTH)general.generalTask.moveDate.month).ToString () + " " + general.generalTask.moveDate.day.ToString() + ", " +  general.generalTask.moveDate.year.ToString ();
				for (int i = 0; i < general.citizen.city.kingdom.militaryManager.activeGenerals.Count; i++) {
					General otherGeneral = general.citizen.city.kingdom.militaryManager.activeGenerals [i];
					if(otherGeneral.location.id == general.location.id && general.citizen.id != otherGeneral.citizen.id){
						summary += "\n\nNAME: " + otherGeneral.citizen.name.ToString();
						if(otherGeneral.generalTask != null){
							summary += "\nTASK: " + otherGeneral.generalTask.task.ToString();
							summary += "\nTARGET HEXTILE: " + otherGeneral.generalTask.targetHextile.name.ToString();
							if(otherGeneral.generalTask.targetHextile.city != null){
								summary += "\nTARGET CITY: " + otherGeneral.generalTask.targetHextile.city.name.ToString();
							}
							summary += "\nMOVE DATE: " + ((MONTH)otherGeneral.generalTask.moveDate.month).ToString () + " " + otherGeneral.generalTask.moveDate.day.ToString() + ", " +  otherGeneral.generalTask.moveDate.year.ToString ();
						}
					}
				}
			}else{
				for (int i = 0; i < general.citizen.city.kingdom.militaryManager.activeGenerals.Count; i++) {
					General otherGeneral = general.citizen.city.kingdom.militaryManager.activeGenerals [i];
					if(otherGeneral.location.id == general.location.id){
						summary += "\n\nNAME: " + otherGeneral.citizen.name.ToString();
						if(otherGeneral.generalTask != null){
							summary += "\nTASK: " + otherGeneral.generalTask.task.ToString();
							summary += "\nTARGET HEXTILE: " + otherGeneral.generalTask.targetHextile.name.ToString();
							if(otherGeneral.generalTask.targetHextile.city != null){
								summary += "\nTARGET CITY: " + otherGeneral.generalTask.targetHextile.city.name.ToString();
							}
							summary += "\nMOVE DATE: " + ((MONTH)otherGeneral.generalTask.moveDate.month).ToString () + " " + otherGeneral.generalTask.moveDate.day.ToString() + ", " +  otherGeneral.generalTask.moveDate.year.ToString ();
						}
					}
				}
			}
			UIManager.Instance.ShowSmallInfo(summary);

		}else if (citizenAvatar.citizenRole is Caravan) {
			Caravan caravan = (Caravan)citizenAvatar.citizenRole;
			if(caravan.gameEventInvolvedIn != null){
				if(caravan.gameEventInvolvedIn is Caravaneer){
					Caravaneer caravaneer = (Caravaneer)caravan.gameEventInvolvedIn;
					summary = "NAME: " + caravan.citizen.name.ToString();
					if(caravaneer.isReturning){
						summary += "\n GOING BACK TO: " + caravaneer.sourceCity.name;
					}else{
						summary += "\n TASK: GET " + caravaneer.neededResource.ToString();
						summary += "\n RESERVED AMOUNT: " + caravaneer.reserveAmount.ToString();
						summary += "\n TARGET CITY: " + caravaneer.targetCity.name;
					}
					UIManager.Instance.ShowSmallInfo(summary);
				}
			}
		}
    }

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
	}
}
