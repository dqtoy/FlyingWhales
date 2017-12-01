using UnityEngine;
using System.Collections;

public class HoverPopulation : MonoBehaviour {
	public GeneralAvatar generalAvatar;
	string summary = string.Empty;

    void OnMouseOver() {
		if (generalAvatar.citizenRole is General) {
			General general = (General)generalAvatar.citizenRole;
			if(general.generalTask != null){
				summary = "TASK: " + general.generalTask.task.ToString();
				summary += "\nTARGET CITY: " + general.generalTask.targetCity.name.ToString();
				summary += "\nMOVE DATE: " + ((MONTH)general.generalTask.moveDate.month).ToString () + " " + general.generalTask.moveDate.day.ToString() + ", " + GameManager.Instance.year.ToString ();
				UIManager.Instance.ShowSmallInfo(summary);
			}
		}
    }

	void OnMouseExit(){
		UIManager.Instance.HideSmallInfo();
	}
}
