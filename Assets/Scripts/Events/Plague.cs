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

    private const float DEFAULT_CURE_CHANCE = 1.5f;
    private const float DEFAULT_SETTLEMENT_SPREAD_CHANCE = 2f;

    protected Dictionary<int, float[]> kingdomChances; //0 - cure chance, 1 - settlement spread chance

    protected List<Kingdom> pragmaticKingdoms;
    protected List<Kingdom> opportunisticKingdoms;
    protected List<Kingdom> humanisticKingdoms;

    private string[] plagueAdjectives = new string[] {
        "Red", "Green", "Yellow", "Black", "Rotting", "Silent", "Screaming", "Trembling", "Sleeping",
        "Cat", "Dog", "Pig", "Lamb", "Lizard", "Bog", "Death", "Stomach", "Eye", "Finger", "Rabid",
        "Fatal", "Blistering", "Icy", "Scaly", "Sexy", "Violent", "Necrotic", "Foul", "Vile", "Nasty",
        "Ghastly", "Malodorous", "Cave", "Phantom", "Wicked", "Strange"
    };

    private string[] plagueDieseases = new string[] {
        "Sores", "Ebola", "Anthrax", "Pox", "Face", "Sneeze", "Gangrene", "Throat", "Rash", "Warts",
        "Cholera", "Colds", "Ache", "Syndrome", "Tumor", "Chills", "Blisters", "Mouth", "Fever", "Delirium",
        "Measles", "Mutata", "Disease"
    };

    private string _plagueName;

    #region getters/setters
    public string plagueName {
        get { return this._plagueName; }
    }
    #endregion

    public Plague(int startWeek, int startMonth, int startYear, Citizen startedBy, City sourceCity) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.PLAGUE;
		this.name = "Plague";
		this._warTrigger = WAR_TRIGGER.OPPOSING_APPROACH;
		this.sourceCity = sourceCity;
        this._plagueName = GeneratePlagueName();
		this.sourceKingdom = sourceCity.kingdom;
		this.affectedKingdoms = new List<Kingdom>();
		this.affectedCities = new List<City> ();
		this.otherKingdoms = GetOtherKingdoms ();
		this.bioWeaponMeter = 0;
		this.vaccineMeter = 0;
		this.daysCount = 0;

        this.pragmaticKingdoms = new List<Kingdom>();
        this.opportunisticKingdoms = new List<Kingdom>();
        this.humanisticKingdoms = new List<Kingdom>();

        this.kingdomChances = new Dictionary<int, float[]>();

        int maxMeter = 200 * NumberOfCitiesInWorld ();
		this.bioWeaponMeterMax = maxMeter;
		this.vaccineMeterMax = maxMeter;
       
		this.InitializePlague ();

        EventManager.Instance.AddEventToDictionary(this);
        EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
        EventManager.Instance.onKingdomDiedEvent.AddListener(CureAKingdom);
	}

    private string GeneratePlagueName() {
        return plagueAdjectives[Random.Range(0, plagueAdjectives.Length)] + " " + plagueDieseases[Random.Range(0, plagueDieseases.Length)];
    }

    #region overrides
    internal override void PerformAction() {
		this.daysCount += 1;
        onPerformAction();
    }
    internal override void DoneEvent() {
        base.DoneEvent();
        EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
        onPerformAction = null;
    }
    #endregion
    private void InitializePlague(){
		this.PlagueACity (this.sourceCity);
		this.PlagueASettlement (this.sourceCity.structures [0]);
		this.PlagueAKingdom (this.sourceKingdom);
		onPerformAction += CheckEndPlague;
        onPerformAction += SpreadPlagueWithinCity;
        onPerformAction += SpreadPlagueWithinKingdom;
        onPerformAction += CureAPlagueSettlementEveryday;
        onPerformAction += DestroyASettlementEveryday;
        onPerformAction += IncreaseUnrestEveryMonth;

    }
	private void CheckEndPlague(){
		if(this.affectedCities.Count <= 0 && this.affectedKingdoms.Count <= 0){
			this.DoneEvent ();
		}
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
    private void ChooseApproach(Citizen king) {
        Dictionary<CHARACTER_VALUE, int> importantCharVals = king.importantCharcterValues;
        EVENT_APPROACH chosenApproach = EVENT_APPROACH.NONE;
        if (importantCharVals.ContainsKey(CHARACTER_VALUE.LIFE) || importantCharVals.ContainsKey(CHARACTER_VALUE.FAIRNESS) ||
            importantCharVals.ContainsKey(CHARACTER_VALUE.GREATER_GOOD) || importantCharVals.ContainsKey(CHARACTER_VALUE.CHAUVINISM)) {
            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = importantCharVals.First();
            if (priotiyValue.Key == CHARACTER_VALUE.CHAUVINISM) {
                chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
            } else if (priotiyValue.Key == CHARACTER_VALUE.GREATER_GOOD) {
                chosenApproach = EVENT_APPROACH.PRAGMATIC;
            } else {
                chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
            }
        } else {
            //a king who does not value any of the these four ethics will choose OPPORTUNISTIC APPROACH in dealing with a plague.
            chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
        }
        this.AddKingdomToApproach(chosenApproach, king.city.kingdom);
    }

    private void AddKingdomToApproach(EVENT_APPROACH approach, Kingdom kingdomToAdd) {
        if (approach == EVENT_APPROACH.HUMANISTIC) {
            if(humanisticKingdoms.Count <= 0) {
                onPerformAction += HumanisticApproach;
            }
            if (!humanisticKingdoms.Contains(kingdomToAdd)) {
                humanisticKingdoms.Add(kingdomToAdd);
            }
        } else if(approach == EVENT_APPROACH.PRAGMATIC) {
            if (pragmaticKingdoms.Count <= 0) {
                onPerformAction += PragmaticApproach;
            }
            if (!pragmaticKingdoms.Contains(kingdomToAdd)) {
                pragmaticKingdoms.Add(kingdomToAdd);
            }
        } else if (approach == EVENT_APPROACH.OPPORTUNISTIC) {
            if (opportunisticKingdoms.Count <= 0) {
                onPerformAction += OpportunisticApproach;
            }
            if (!opportunisticKingdoms.Contains(kingdomToAdd)) {
                opportunisticKingdoms.Add(kingdomToAdd);
            }
        }
    }

    private void HumanisticApproach() {

    }

    private void PragmaticApproach() {
        if (this.IsDaysMultipleOf(5)) {
            for (int i = 0; i < this.pragmaticKingdoms.Count; i++) {
                Kingdom currKingdom = this.pragmaticKingdoms[i];
                int chanceForExterminator = 3 * currKingdom.cities.Count;
                int chance = Random.Range(0, 100);
                if(chance < chanceForExterminator) {
                    //Create Exterminator
                }
            }
        }
    }
    private void OpportunisticApproach() {

    }

    internal void InfectRandomSettlement(List<HexTile> hexTilesToChooseFrom) {
        HexTile targetSettlement = hexTilesToChooseFrom.First(x => !x.isPlagued);
        if (targetSettlement != null) {
            this.PlagueASettlement(targetSettlement);
        }
    }

    private void PlagueASettlement(HexTile hexTile){
		hexTile.SetPlague(true);
	}
	internal void PlagueACity(City city){
		city.plague = this;
		this.affectedCities.Add (city);
	}
	private void PlagueAKingdom(Kingdom kingdom){
		this.affectedKingdoms.Add (kingdom);
        kingdomChances.Add(kingdom.id, new float[] {
            DEFAULT_CURE_CHANCE,
            DEFAULT_SETTLEMENT_SPREAD_CHANCE
        });
        this.ChooseApproach(kingdom.king);
	}
	private void CureASettlement(HexTile hexTile){
		hexTile.SetPlague(false);
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
		for (int i = 0; i < kingdom.cities.Count; i++) {
			this.CureACity (kingdom.cities [i]);
		}
	}
	private void DestroyASettlementInCity(City city){
		List<HexTile> plaguedSettlements = city.plaguedSettlements;
		HexTile targetSettlement = plaguedSettlements [plaguedSettlements.Count - 1];
		if(targetSettlement != null){
            /*
             * Reset tile for now.
             * TODO: When ruined settlement sprites are provided, use those instead.
             * */
            city.RemoveTileFromCity(targetSettlement);
		}

	}
	private bool IsDaysMultipleOf (int multiple){
		if((this.daysCount % multiple) == 0){
			return true;
		}
		return false;
	}

    /*
     * Every multiple of 4 day, per city, there is a base 2% chance for every 
     * plagued settlement that the plague will spread to another settlement 
     * within the city.
     * */
	private void SpreadPlagueWithinCity(){
		if(this.IsDaysMultipleOf(4)){
			for (int i = 0; i < this.affectedCities.Count; i++) {
				float chance = UnityEngine.Random.Range (0f, 99f);
                Kingdom kingdomOfCity = this.affectedCities[i].kingdom;
                float value = this.kingdomChances[kingdomOfCity.id][1] * this.affectedCities [i].plaguedSettlements.Count;
				if(chance < value){
                    InfectRandomSettlement(this.affectedCities[i].structures);
				}
			}
		}
	}

    /*
     * Every multiple of 4 day, per kingdom, there is a 1% chance for every plagued settlement
     * within the kingdom that the plague will spread to another clean city within the kingdom.
     * */
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

    /*
     * Every multiple of 5 day, per kingdom, there is a 1.5% chance for every 
     * plagued settlement that one of them will be cured.
     * */
	private void CureAPlagueSettlementEveryday(){
		if (this.IsDaysMultipleOf (5)) {
			for (int i = 0; i < this.affectedKingdoms.Count; i++) {
				float chance = UnityEngine.Random.Range (0f, 99f);
                Kingdom kingdomOfCity = this.affectedCities[i].kingdom;
                float value = this.kingdomChances[kingdomOfCity.id][0] * (float)this.affectedKingdoms [i].cities.Sum (x => x.plaguedSettlements.Count);
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

    /*
     * Every multiple of 6 day, per kingdom, there is a 1% chance for 
     * every plagued settlement that one of them will be ruined.
     * */
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

    /*
     * At the start of each month, unrest in the kingdom 
     * increases by 1 for every plagued settlement.
     * */
    private void IncreaseUnrestEveryMonth() {
        if (GameManager.Instance.days == 1) {
            for (int i = 0; i < this.affectedKingdoms.Count; i++) {
                Kingdom currKingdom = this.affectedKingdoms[i];
                int unrestIncrease = 1 * currKingdom.cities.Sum(x => x.plaguedSettlements.Count);
                currKingdom.AdjustUnrest(unrestIncrease);
            }
        }
    }
        
}
