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
	internal List<Kingdom> otherKingdoms;
	internal int bioWeaponMeter;
	internal int vaccineMeter;
	internal int bioWeaponMeterMax;
	internal int vaccineMeterMax;

	public Plague(int startWeek, int startMonth, int startYear, Citizen startedBy, City sourceCity) : base (startWeek, startMonth, startYear, startedBy){
		this.sourceCity = sourceCity;
		this.sourceKingdom = sourceCity.kingdom;
		this.affectedKingdoms = new List<Kingdom>();
		this.otherKingdoms = GetOtherKingdoms ();
		this.bioWeaponMeter = 0;
		this.vaccineMeter = 0;

		int maxMeter = 200 * NumberOfCitiesInWorld ();
		this.bioWeaponMeterMax = maxMeter;
		this.vaccineMeterMax = maxMeter;

        this.ChooseApproach();

        EventManager.Instance.AddEventToDictionary(this);
        EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
	}

    #region overrides
    internal override void PerformAction() {
        onPerformAction();
    }
    #endregion

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
}
