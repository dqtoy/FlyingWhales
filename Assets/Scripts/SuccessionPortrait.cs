using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SuccessionPortrait : MonoBehaviour {
	public GameObject supporterGO;
	public GameObject characterPortraitGO;
	public GameObject arrowGO;

	public UI2DSprite kingdomColorGO;
	public UILabel citizenName;
	public UILabel citizenTitle;

	public UIGrid supporterGrid;

	public Citizen citizen;
	public Kingdom kingdom;
	public List<Citizen> supporters;

	public void SetCitizen(Citizen citizen, Kingdom kingdom){
		this.citizen = citizen;
		this.kingdom = kingdom;
		this.kingdomColorGO.color = kingdom.kingdomColor;
		this.citizenName.text = citizen.name;
		string title = "Pretender";
		if (kingdom.successionLine.Count > 0) {
			if (kingdom.successionLine [0] != null) {
				if (kingdom.successionLine [0].id == citizen.id) {
					title = "Heir Apparent";
					if (kingdom.successionLine.Count > 1) {
						if (kingdom.successionLine [1] != null) {
							this.arrowGO.SetActive (true);
						}
					}

				}
			}
		}
		if (kingdom.successionLine.Count > 1) {
			if (kingdom.successionLine [1] != null) {
				if (kingdom.successionLine [1].id == citizen.id) {
					title = "Second in Line";
					if (kingdom.successionLine.Count > 2) {
						if (kingdom.successionLine [2] != null) {
							this.arrowGO.SetActive (true);
						}
					}
				}
			}
		}
		if (kingdom.successionLine.Count > 2) {
			if (kingdom.successionLine [2] != null) {
				if (kingdom.successionLine [2].id == citizen.id) {
					title = "Third in Line";
				}
			}
		}
		this.citizenTitle.text = title;
		this.supporters = GetSupporters (citizen);
		if(this.supporters.Count > 0){
			for (int i = 0; i < this.supporters.Count; i++) {
				GameObject citizenGO = GameObject.Instantiate (this.characterPortraitGO, this.supporterGrid.transform) as GameObject;
				citizenGO.GetComponent<CharacterPortrait> ().SetCitizen (this.supporters [i]);
				citizenGO.transform.localScale = Vector3.one;
			}
			this.supporterGrid.Reposition ();
			this.supporterGO.SetActive (true);
		}
	}
	private List<Citizen> GetSupporters(Citizen citizen){
		List<Citizen> supporterCitizens = new List<Citizen> ();

		if (this.kingdom.successionLine.Count > 0) {
			if (this.kingdom.successionLine [0].id == citizen.id) {
				if(!this.kingdom.king.isHeir || citizen.isHeir){
					for(int i = 0; i < this.kingdom.cities.Count; i++){
						if (this.kingdom.cities [i].governor.id != citizen.id) {
							if (this.kingdom.cities [i].governor.supportedCitizen == null) {
								supporterCitizens.Add (this.kingdom.cities [i].governor);
							}
						}
					}
				}
			}else{
				for(int i = 0; i < this.kingdom.cities.Count; i++){
					if (this.kingdom.cities [i].governor.supportedCitizen != null) {
						if (this.kingdom.cities [i].governor.id != citizen.id) {
							if (this.kingdom.cities [i].governor.supportedCitizen.id == citizen.id) {
								supporterCitizens.Add (this.kingdom.cities [i].governor);
							}
						}
					}
				}
			}
		}
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			if (KingdomManager.Instance.allKingdoms [i].id != this.kingdom.id) {
				if (KingdomManager.Instance.allKingdoms [i].king.id != citizen.id) {
					if (KingdomManager.Instance.allKingdoms [i].king.supportedCitizen != null) {
						if (KingdomManager.Instance.allKingdoms [i].king.supportedCitizen.id == citizen.id) {
							supporterCitizens.Add (KingdomManager.Instance.allKingdoms [i].king);
						}
					}
				}
			}
		}
		return supporterCitizens;
	}
	void OnClick(){
		UIManager.Instance.ShowCitizenInfo (this.citizen);
	}
}
