using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Plague : GameEvent {

    private delegate void OnPerformAction();
    private OnPerformAction onPerformAction;

	internal City sourceCity;
	internal Kingdom sourceKingdom;
	internal List<Kingdom> affectedKingdoms;
	internal List<City> affectedCities;
	internal List<Kingdom> otherKingdoms;
	internal int bioWeaponMeter;
	internal int vaccineMeter;
	internal int bioWeaponMeterMax;
	internal int vaccineMeterMax;
	internal int daysCount;

	public Plague(int startWeek, int startMonth, int startYear, Citizen startedBy, City sourceCity) : base (startWeek, startMonth, startYear, startedBy){
		this.sourceCity = sourceCity;
		this.sourceKingdom = sourceCity.kingdom;
		this.affectedKingdoms = new List<Kingdom>();
		this.affectedCities = new List<City> ();
		this.otherKingdoms = GetOtherKingdoms ();
		this.bioWeaponMeter = 0;
		this.vaccineMeter = 0;
		this.daysCount = 0;

		int maxMeter = 200 * NumberOfCitiesInWorld ();
		this.bioWeaponMeterMax = maxMeter;
		this.vaccineMeterMax = maxMeter;

        this.ChooseApproach();
		this.InitializePlague ();
        EventManager.Instance.AddEventToDictionary(this);
        EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}

    #region overrides
    internal override void PerformAction() {
		this.daysCount += 1;
        onPerformAction();
    }
    #endregion
	private void InitializePlague(){
		this.PlagueACity (this.sourceCity);
		this.PlagueASettlement (this.sourceCity.structures [0]);
	}
    private List<Kingdom> GetOtherKingdoms(){
		if(this.sourceCity == null){
			return null;
		}
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			if(KingdomManager.Instance.allKingdoms[i].id != this.sourceKingdom.id && KingdomManager.Instance.allKingdoms[i].isAlive()){
				kingdoms.Add (KingdomManager.Instance.allKingdoms [i]);
			}
		}
		return kingdoms;
	}
	private int NumberOfCitiesInWorld(){
		int count = 0;
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			count += KingdomManager.Instance.allKingdoms [i].cities.Count;
		}
		return count;
	}

    /*
     * Choose what approach this king will use
     * to handle this event. This function will then 
     * assign the chosen approach to onPerformAction.
     * */
    private void ChooseApproach() {
        Dictionary<CHARACTER_VALUE, int> importantCharVals = this.startedBy.importantCharcterValues;

        if (importantCharVals.ContainsKey(CHARACTER_VALUE.LIFE) || importantCharVals.ContainsKey(CHARACTER_VALUE.FAIRNESS) ||
            importantCharVals.ContainsKey(CHARACTER_VALUE.GREATER_GOOD) || importantCharVals.ContainsKey(CHARACTER_VALUE.CHAUVINISM)) {
            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = importantCharVals.First();
            if (priotiyValue.Key == CHARACTER_VALUE.CHAUVINISM) {
                onPerformAction += OpportunisticApproach;
            } else if (priotiyValue.Key == CHARACTER_VALUE.GREATER_GOOD) {
                onPerformAction += PragmaticApproach;
            } else {
                onPerformAction += HumanisticApproach;
            }
        } else {
            //a king who does not value any of the these four ethics will choose OPPORTUNISTIC APPROACH in dealing with a plague.
            onPerformAction += OpportunisticApproach;
        }
    }

    private void HumanisticApproach() {

    }

    private void PragmaticApproach() {

    }
    private void OpportunisticApproach() {

    }

	private void PlagueASettlement(HexTile hexTile){
		hexTile.isPlagued = true;

		//TODO: add poison icon on tile
	}
	private void PlagueACity(City city){
		city.plague = this;
		this.affectedCities.Add (city);
	}
	private void PlagueAKingdom(Kingdom kingdom){
		this.affectedKingdoms.Add (kingdom);
	}
	private void CureASettlement(HexTile hexTile){
		hexTile.isPlagued = false;

		//TODO: remove poison icon on tile
	}
	private void CureACity(City city){
		city.plague = null;
		this.affectedCities.Remove (city);
		List<HexTile> plaguedSettlements = city.plaguedSettlements;
		for (int i = 0; i < plaguedSettlements.Count; i++) {
			this.CureASettlement (plaguedSettlements [i]);
		}
	}
	private void CureAKingdom(Kingdom kingdom){
		this.affectedKingdoms.Remove (kingdom);
	}
	private void DestroyASettlementInCity(City city){
		List<HexTile> plaguedSettlements = city.plaguedSettlements;
		HexTile targetSettlement = plaguedSettlements [plaguedSettlements.Count - 1];
		if(targetSettlement != null){
			//TODO: destroy settlement
		}

	}
	private bool IsDaysMultipleOf (int multiple){
		if((this.daysCount % multiple) == 0){
			return true;
		}
		return false;
	}
	private void SpreadPlagueWithinCity(){
		if(this.IsDaysMultipleOf(4)){
			for (int i = 0; i < this.affectedCities.Count; i++) {
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 2 * this.affectedCities [i].plaguedSettlements.Count;
				if(chance < value){
					HexTile targetSettlement = this.affectedCities [i].structures.First (x => !x.isPlagued);
					if(targetSettlement != null){
						this.PlagueASettlement (targetSettlement);
					}
				}
			}
		}
	}
	private void SpreadPlagueWithinKingdom(){
		if (this.IsDaysMultipleOf (4)) {
			for (int i = 0; i < this.affectedKingdoms.Count; i++) {
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 1 * this.affectedKingdoms [i].cities.Sum (x => x.plaguedSettlements.Count);
				if (chance < value) {
					City targetCity = this.affectedKingdoms [i].cities.First (x => x.plague == null);
					if (targetCity != null) {
						this.PlagueACity (targetCity);
					}
				}
			}
		}
	}
	private void CureAPlagueSettlementEveryday(){
		if (this.IsDaysMultipleOf (5)) {
			for (int i = 0; i < this.affectedKingdoms.Count; i++) {
				float chance = UnityEngine.Random.Range (0f, 99f);
				float value = 1.5f * (float)this.affectedKingdoms [i].cities.Sum (x => x.plaguedSettlements.Count);
				if (chance < value) {
					City targetCity = this.affectedKingdoms [i].cities.First (x => x.plague != null);
					if (targetCity != null) {
						HexTile targetSettlement = targetCity.structures.First (x => x.isPlagued);
						if(targetSettlement != null){
							this.CureASettlement (targetSettlement);
						}
					}
				}
			}
		}
	}
	private void DestroyASettlementEveryday(){
		if (this.IsDaysMultipleOf (6)) {
			for (int i = 0; i < this.affectedKingdoms.Count; i++) {
				int chance = UnityEngine.Random.Range (0, 100);
				int value = 1 * this.affectedKingdoms [i].cities.Sum (x => x.plaguedSettlements.Count);
				if (chance < value) {
					City targetCity = this.affectedKingdoms [i].cities.First (x => x.plague != null);
					if (targetCity != null) {
						this.DestroyASettlementInCity (targetCity);
					}
				}
			}
		}
	}
}
