using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaidUI : MonoBehaviour {

	public UIPopupList dropdownRaidKingdoms;
	public UIPopupList dropdownGenerals;

	public UILabel raidKingdomsLbl;
	public UILabel generalsLbl;

	void OnEnable(){
		this.dropdownRaidKingdoms.Clear ();
		this.dropdownGenerals.Clear ();

		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(UIManager.Instance.currentlyShowingCity != null){
				if(UIManager.Instance.currentlyShowingCity.kingdom.id != KingdomManager.Instance.allKingdoms[i].id){
					this.dropdownRaidKingdoms.AddItem (KingdomManager.Instance.allKingdoms [i].name, KingdomManager.Instance.allKingdoms [i]);
				}
			}else{
				this.dropdownRaidKingdoms.AddItem (KingdomManager.Instance.allKingdoms [i].name, KingdomManager.Instance.allKingdoms [i]);
			}
		}
	}

	public void OnValueChangedKingdoms(){
		this.dropdownGenerals.Clear ();
		this.raidKingdomsLbl.text = this.dropdownRaidKingdoms.value;
		Kingdom selectedKingdom = this.dropdownRaidKingdoms.data as Kingdom;
		PopulateGeneral (selectedKingdom);
	}
	public void OnValueChangeGenerals(){
		this.generalsLbl.text = this.dropdownGenerals.value;
	}
	public void OnClickCreateEvent(){
		Kingdom selectedKingdom = this.dropdownRaidKingdoms.data as Kingdom;
//		General selectedGeneral = this.dropdownGenerals.data as General;

		Raid raid = new Raid(GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, selectedKingdom.king, UIManager.Instance.currentlyShowingCity);
//		EventManager.Instance.AddEventToDictionary (raid);
	}
	private void PopulateGeneral(Kingdom kingdom){
		List<Citizen> unwantedGovernors = GetUnwantedGovernors (kingdom.king);
		for(int i = 0; i < kingdom.cities.Count; i++){
			if(!IsItThisGovernor(kingdom.cities[i].governor, unwantedGovernors)){
				for(int j = 0; j < kingdom.cities[i].citizens.Count; j++){
					if (!kingdom.cities [i].citizens [j].isDead) {
						if (kingdom.cities [i].citizens [j].assignedRole != null && kingdom.cities [i].citizens [j].role == ROLE.GENERAL) {
							if(kingdom.cities [i].citizens [j].assignedRole is General){
								if (!((General)kingdom.cities [i].citizens [j].assignedRole).inAction) {
									this.dropdownGenerals.AddItem (kingdom.cities [i].citizens [j].name, ((General)kingdom.cities [i].citizens [j].assignedRole));
								}
							}
						}
					}
				}
			}
		}
	}
	internal bool IsItThisGovernor(Citizen governor, List<Citizen> unwantedGovernors){
		for(int i = 0; i < unwantedGovernors.Count; i++){
			if(governor.id == unwantedGovernors[i].id){
				return true;
			}	
		}
		return false;
	}
	internal List<Citizen> GetUnwantedGovernors(Citizen king){
		List<Citizen> unwantedGovernors = new List<Citizen> ();
		for(int i = 0; i < king.civilWars.Count; i++){
			if(king.civilWars[i].isGovernor){
				unwantedGovernors.Add (king.civilWars [i]);
			}
		}
		for(int i = 0; i < king.successionWars.Count; i++){
			if(king.successionWars[i].isGovernor){
				unwantedGovernors.Add (king.successionWars [i]);
			}
		}
		for(int i = 0; i < king.city.kingdom.cities.Count; i++){
			if(king.city.kingdom.cities[i].governor.supportedCitizen != null){
				unwantedGovernors.Add (king.city.kingdom.cities [i].governor);
			}
		}

		return unwantedGovernors;
	}
}
