using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AltarOfBlessing : GameEvent {
	internal GameObject avatar;
	internal HexTile hexTileSpawnPoint;

	public AltarOfBlessing(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile hexTile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ALTAR_OF_BLESSING;
		this.name = "Altar of Blessing";
		this.isOneTime = true;
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.avatar = null;
		this.hexTileSpawnPoint = hexTile;
		Initialize ();
	}

	private void Initialize(){
		this.hexTileSpawnPoint.PutEventOnTile (this);
		//this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/AltarOfBlessing"), this.hexTileSpawnPoint.transform) as GameObject;
		//this.avatar.transform.localPosition = Vector3.zero;
		//this.avatar.GetComponent<AltarOfBlessingAvatar>().Init(this);
	}

	internal void TransferAltarOfBlessing(Kingdom kingdom, Citizen citizen){
		AltarEffect(kingdom);
		GameObject.Destroy (this.avatar);
	}
	private void AltarEffect(Kingdom kingdom){
		int chance = UnityEngine.Random.Range(0,100);
		if(chance < 10){
			IncreaseTechLevel(kingdom);
		}else if(chance >= 10 && chance < 30){
			ReduceUnrest(kingdom);
		}else{
			LevelUpCity(kingdom);
		}
		this.hexTileSpawnPoint.RemoveEventOnTile ();
		this.DoneEvent ();

	}
	private void IncreaseTechLevel(Kingdom kingdom){
		Debug.Log("ALTAR: UPGRADE TECH LEVEL");
		kingdom.UpgradeTechLevel(1);
        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "AltarOfBlessing", "increase_tech_level");
        newLog.AddToFillers(kingdom, kingdom.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if(UIManager.Instance.currentlyShowingKingdom.id == kingdom.id) {
            UIManager.Instance.ShowNotification(newLog);
        }
    }
	private void ReduceUnrest(Kingdom kingdom){
		Debug.Log("ALTAR: REDUCE UNREST");
		kingdom.AdjustUnrest(-50);
        Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "AltarOfBlessing", "reduce_unrest");
        newLog.AddToFillers(kingdom, kingdom.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        if (UIManager.Instance.currentlyShowingKingdom.id == kingdom.id) {
            UIManager.Instance.ShowNotification(newLog);
        }
    }
	private void LevelUpCity(Kingdom kingdom){
		Debug.Log("ALTAR: LEVEL UP CITY");
		List<City> cities = kingdom.nonRebellingCities;
		if(cities != null && cities.Count > 0){
			City chosenCity = cities[UnityEngine.Random.Range(0, cities.Count)];
            //Instantly purchase tile
            chosenCity.ForcePurchaseTile();
            Log newLog = this.CreateNewLogForEvent(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "AltarOfBlessing", "level_up_city");
            newLog.AddToFillers(kingdom, kingdom.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            if (UIManager.Instance.currentlyShowingKingdom.id == kingdom.id) {
                UIManager.Instance.ShowNotification(newLog);
            }
        }
	}
}
