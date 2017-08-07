using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Crime : PlayerEvent {

	private Kingdom kingdom;
	private CrimeData crimeData;
	private PUNISHMENT kingPunishment;

	public Crime(int startWeek, int startMonth, int startYear, Citizen startedBy, CrimeData crimeData) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = PLAYER_EVENT.CRIME;
		this.name = "Crime";
		this.kingdom = this.startedByKingdom;
		this.crimeData = crimeData;
		this.affectedKingdoms.Add (this.kingdom);

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerEvents", "Crime", "event_title");

		Initialize ();

		this.PlayerEventIsCreated ();

		this.DoneEvent ();
	}

	private void Initialize(){
		string crimeDetails = LocalizationManager.Instance.GetLocalizedValue ("Crimes", this.crimeData.fileName, "details");
		string punishmentDetails = string.Empty;

		this.kingPunishment = GetPunishment (this.startedBy);
		if(this.kingPunishment == PUNISHMENT.NO){
			punishmentDetails = LocalizationManager.Instance.GetLocalizedValue ("Crimes", this.crimeData.fileName, "no_punishment");
		}else if(this.kingPunishment == PUNISHMENT.LIGHT){
			punishmentDetails = LocalizationManager.Instance.GetLocalizedValue ("Crimes", this.crimeData.fileName, "light_punishment");
		}else{
			punishmentDetails = LocalizationManager.Instance.GetLocalizedValue ("Crimes", this.crimeData.fileName, "harsh_punishment");
		}

        KingdomReaction();
		GovernorReactions ();
		OtherKingsReactions ();

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerEvents", "Crime", "start");
		newLog.AddToFillers (null, crimeDetails, LOG_IDENTIFIER.CRIME_DETAILS);

		Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "PlayerEvents", "Crime", "punishment");
		newLog2.AddToFillers (this.startedBy, this.startedBy.name, LOG_IDENTIFIER.KING_1);
		newLog2.AddToFillers (null, punishmentDetails, LOG_IDENTIFIER.CRIME_PUNISHMENT);

	}

	private PUNISHMENT GetPunishment(object obj){
		int value = GetPunishmentValue (obj);
		if(value > 0){
			return PUNISHMENT.NO;
		}else if(value < 0){
			return PUNISHMENT.HARSH;
		}else{
			return PUNISHMENT.LIGHT;
		}
	}
	private int GetPunishmentValue(object obj){
		int value = 0;
		List<CHARACTER_VALUE> charValues = null; 
        if(obj is Citizen) {
            charValues = new List<CHARACTER_VALUE>(((Citizen)obj).importantCharacterValues.Keys);
        } else if(obj is Kingdom) {
            charValues = new List<CHARACTER_VALUE>(((Kingdom)obj).importantCharacterValues.Keys);
        }
		for (int i = 0; i < charValues.Count; i++) {
			if(this.crimeData.positiveValues.Contains(charValues[i])){
				value += 1;
			}
			if(this.crimeData.negativeValues.Contains(charValues[i])){
				value -= 1;
			}
		}
		return value;
	}

	private void GovernorReactions(){
		List<City> allCities = this.kingdom.nonRebellingCities;
		if(allCities != null && allCities.Count > 0){
			for (int i = 0; i < allCities.Count; i++) {
				PUNISHMENT governorPunishment = GetPunishment (allCities [i].governor);
				if(governorPunishment == this.kingPunishment){
					((Governor)allCities[i].governor.assignedRole).AddEventModifier(15, "Same criminal judgement", null);
				}else{
					if(this.kingPunishment == PUNISHMENT.NO){
						if(governorPunishment == PUNISHMENT.LIGHT){
							((Governor)allCities[i].governor.assignedRole).AddEventModifier(-5, "Different criminal judgement", null);
						}else if(governorPunishment == PUNISHMENT.HARSH){
							((Governor)allCities[i].governor.assignedRole).AddEventModifier(-10, "Opposite criminal judgement", null);
						}
					}else if(this.kingPunishment == PUNISHMENT.LIGHT){
						if(governorPunishment == PUNISHMENT.NO){
							((Governor)allCities[i].governor.assignedRole).AddEventModifier(-5, "Different criminal judgement", null);
						}else if(governorPunishment == PUNISHMENT.HARSH){
							((Governor)allCities[i].governor.assignedRole).AddEventModifier(-5, "Different criminal judgement", null);
						}
					}else{
						if(governorPunishment == PUNISHMENT.LIGHT){
							((Governor)allCities[i].governor.assignedRole).AddEventModifier(-5, "Different criminal judgement", null);
						}else if(governorPunishment == PUNISHMENT.NO){
							((Governor)allCities[i].governor.assignedRole).AddEventModifier(-10, "Opposite criminal judgement", null);
						}
					}
				}
			}
		}
	}

    private void KingdomReaction() {
        PUNISHMENT kingdomPunishment = GetPunishment(kingdom);
        if (kingdomPunishment == this.kingPunishment) {
            kingdom.AdjustUnrest(-10);
        } else {
            if (this.kingPunishment == PUNISHMENT.NO) {
                if (kingdomPunishment == PUNISHMENT.HARSH) {
                    kingdom.AdjustUnrest(10);
                }
            } else {
                if (kingdomPunishment == PUNISHMENT.NO) {
                    kingdom.AdjustUnrest(10);
                }
            }
        }
    }

    private void OtherKingsReactions(){
		List<Kingdom> otherKingdoms = this.kingdom.discoveredKingdoms;
		if(otherKingdoms != null && otherKingdoms.Count > 0){
			for (int i = 0; i < otherKingdoms.Count; i++) {
				RelationshipKings relationship = otherKingdoms[i].king.GetRelationshipWithCitizen(this.kingdom.king);
				if(relationship != null){
					PUNISHMENT otherKingPunishment = GetPunishment (otherKingdoms[i].king);
					if(otherKingPunishment == this.kingPunishment){
						relationship.AdjustLikeness(15, null);
					}else{
						if(this.kingPunishment == PUNISHMENT.NO){
							if(otherKingPunishment == PUNISHMENT.LIGHT){
								relationship.AdjustLikeness(-5, null, ASSASSINATION_TRIGGER_REASONS.OPPOSING_APPROACH);
							}else if(otherKingPunishment == PUNISHMENT.HARSH){
								relationship.AdjustLikeness(-10, null, ASSASSINATION_TRIGGER_REASONS.OPPOSING_APPROACH);
							}
						}else if(this.kingPunishment == PUNISHMENT.LIGHT){
							if(otherKingPunishment == PUNISHMENT.NO){
								relationship.AdjustLikeness(-5, null, ASSASSINATION_TRIGGER_REASONS.OPPOSING_APPROACH);
							}else if(otherKingPunishment == PUNISHMENT.HARSH){
								relationship.AdjustLikeness(-5, null, ASSASSINATION_TRIGGER_REASONS.OPPOSING_APPROACH);
							}
						}else{
							if(otherKingPunishment == PUNISHMENT.LIGHT){
								relationship.AdjustLikeness(-5, null, ASSASSINATION_TRIGGER_REASONS.OPPOSING_APPROACH);
							}else if(otherKingPunishment == PUNISHMENT.NO){
								relationship.AdjustLikeness(-10, null, ASSASSINATION_TRIGGER_REASONS.OPPOSING_APPROACH);
							}
						}
					}
				}
			}
		}
	}
}
