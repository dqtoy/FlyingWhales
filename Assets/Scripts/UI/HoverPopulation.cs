using UnityEngine;
using System.Collections;

public class HoverPopulation : MonoBehaviour {
	public GeneralAvatar generalAvatar;
	string summary = string.Empty;

    void OnMouseOver() {
		if (generalAvatar.citizenRole is General) {
			General general = (General)generalAvatar.citizenRole;
			if(general.generalTask != null){
				summary = "NAME: " + general.citizen.name.ToString();
				summary += "\nTASK: " + general.generalTask.task.ToString();
				summary += "\nTARGET HEXTILE: " + general.generalTask.targetHextile.name.ToString();
				if(general.generalTask.targetHextile.city != null){
					summary += "\nTARGET CITY: " + general.generalTask.targetHextile.city.name.ToString();
				}
				summary += "\nMOVE DATE: " + ((MONTH)general.generalTask.moveDate.month).ToString () + " " + general.generalTask.moveDate.day.ToString() + ", " +  general.generalTask.moveDate.year.ToString ();
				UIManager.Instance.ShowSmallInfo(summary);
			}
		}
    }

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
	}
}
