using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FirstAndKeystone : GameEvent {
	internal Kingdom firstOwner;
	internal Kingdom keystoneOwner;

	public FirstAndKeystone(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom firstOwner, Kingdom keystoneOwner) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.FIRST_AND_KEYSTONE;
		this.name = "The First and The Keystone";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.firstOwner = firstOwner;
		this.keystoneOwner = keystoneOwner;
	}

    #region Overrides

    #endregion

    /*
     * Determine what approach a citizen
     * will choose. This will not add that citizen's
     * kingdom to the appropriate list.
     * */
    private EVENT_APPROACH DetermineApproach(Citizen citizen) {
        Dictionary<CHARACTER_VALUE, int> importantCharVals = citizen.importantCharacterValues;
        EVENT_APPROACH chosenApproach = EVENT_APPROACH.NONE;
        if (importantCharVals.ContainsKey(CHARACTER_VALUE.LIFE) || importantCharVals.ContainsKey(CHARACTER_VALUE.EQUALITY) ||
            importantCharVals.ContainsKey(CHARACTER_VALUE.DOMINATION)) {

            KeyValuePair<CHARACTER_VALUE, int> priotiyValue = importantCharVals
                .FirstOrDefault(x => x.Key == CHARACTER_VALUE.DOMINATION
                || x.Key == CHARACTER_VALUE.LIFE || x.Key == CHARACTER_VALUE.EQUALITY);

            if (priotiyValue.Key == CHARACTER_VALUE.LIFE || priotiyValue.Key == CHARACTER_VALUE.EQUALITY) {
                chosenApproach = EVENT_APPROACH.HUMANISTIC;
            } else {
                chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
            }
        } else {
            //a king who does not value any of the these four ethics will choose OPPORTUNISTIC APPROACH in dealing with a plague.
            chosenApproach = EVENT_APPROACH.OPPORTUNISTIC;
        }
        return chosenApproach;
    }
}
