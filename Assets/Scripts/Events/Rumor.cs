using UnityEngine;
using System.Collections;

public class Rumor : GameEvent {
	internal Kingdom sourceKingdom;
	internal Kingdom rumorKingdom;
	internal Kingdom targetKingdom;


	private int daysCounter;

	public enum RUMOR_TYPE{
		POSITIVE,
		NEGATIVE,
	}

	public Rumor(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom rumorKingdom, Kingdom targetKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.RUMOR;
		this.name = "Rumor";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.sourceKingdom = startedBy.city.kingdom;
		this.rumorKingdom = rumorKingdom;
		this.targetKingdom = targetKingdom;
		this.daysCounter = 0;

		EventManager.Instance.AddEventToDictionary(this);
		Messenger.AddListener("OnDayEnd", this.PerformAction);

		//TODO: Add log - event title
		//TODO: Add log - plotting to create rumor
		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rumor", "event_title");

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rumor", "start");
		newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this.sourceKingdom, this.sourceKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this.rumorKingdom.king, this.rumorKingdom.king.name, LOG_IDENTIFIER.KING_2);
		newLog.AddToFillers (this.rumorKingdom, this.rumorKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_3);
		newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_3);
		this.EventIsCreated(this.sourceKingdom, true);
		this.EventIsCreated(this.rumorKingdom, false);
		this.EventIsCreated(this.rumorKingdom, false);
	}

	#region Overrides
	internal override void PerformAction (){
		this.daysCounter += 1;
		if(this.daysCounter % 3 == 0){
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 10){
				//StartRumor
				StartRumor();
			}
		}
	}
	internal override void DoneEvent (){
		base.DoneEvent ();
		if (this.startedBy.assignedRole != null){
			if (this.startedBy.assignedRole is King){
				((King)this.startedBy.assignedRole).isRumoring = false;
			}
		}
		((King)this.sourceKingdom.king.assignedRole).isRumoring = false;

		Messenger.RemoveListener("OnDayEnd", this.PerformAction);
	}
	#endregion

	private void StartRumor(){
		RelationshipKings relationshipSourceToTarget = this.sourceKingdom.king.GetRelationshipWithCitizen(this.targetKingdom.king);
		RelationshipKings relationshipSourceToRumor = this.sourceKingdom.king.GetRelationshipWithCitizen(this.rumorKingdom.king);
		RelationshipKings relationshipRumorToTarget = this.rumorKingdom.king.GetRelationshipWithCitizen(this.targetKingdom.king);

		if(relationshipSourceToRumor == null || relationshipRumorToTarget == null){
			//Cancel Rumor
			//TODO: Add log - cancel
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rumor", "cancel");
			newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
			this.DoneEvent();
			return;
		}
		RUMOR_TYPE rumorType = GetRumorType(relationshipSourceToTarget);
		if(rumorType == RUMOR_TYPE.NEGATIVE){
			string randomNegative = LocalizationManager.Instance.GetRandomLocalizedKey ("Others", "NegativeRumors");
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Others", "NegativeRumors", randomNegative);
			newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.rumorKingdom.king, this.rumorKingdom.king.name, LOG_IDENTIFIER.KING_2);
			newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_3);
			//TODO: Add log - told negative rumor
		}else if(rumorType == RUMOR_TYPE.POSITIVE){
			//TODO: Add log - told positive rumor
			string randomPositive = LocalizationManager.Instance.GetRandomLocalizedKey ("Others", "PositiveRumors");
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Others", "PositiveRumors", randomPositive);
			newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.rumorKingdom.king, this.rumorKingdom.king.name, LOG_IDENTIFIER.KING_2);
			newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_3);
		}
		int chance = UnityEngine.Random.Range(0,100);
		int value = 30;

		if(relationshipSourceToRumor != null && relationshipSourceToRumor.lordRelationship == RELATIONSHIP_STATUS.ALLY){
			value = 40;
		}

		if(rumorType == RUMOR_TYPE.NEGATIVE){
			if(relationshipRumorToTarget.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				value -= 15;
			}else if(relationshipRumorToTarget.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
				value -= 10;
			}else if(relationshipRumorToTarget.lordRelationship == RELATIONSHIP_STATUS.WARM){
				value -= 5;
			}
		}


		if(chance < value){
			if(relationshipSourceToTarget.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				relationshipRumorToTarget.AddEventModifier(5, this.name + " event", this);
			}else if(relationshipSourceToTarget.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
				relationshipRumorToTarget.AddEventModifier(4, this.name + " event", this);
			}else if(relationshipSourceToTarget.lordRelationship == RELATIONSHIP_STATUS.WARM){
				relationshipRumorToTarget.AddEventModifier(3, this.name + " event", this);
			}else if(relationshipSourceToTarget.lordRelationship == RELATIONSHIP_STATUS.COLD){
				relationshipRumorToTarget.AddEventModifier(-3, this.name + " event", this, ASSASSINATION_TRIGGER_REASONS.SUCCESS_RUMOR);
			}else if(relationshipSourceToTarget.lordRelationship == RELATIONSHIP_STATUS.ENEMY){
				relationshipRumorToTarget.AddEventModifier(-4, this.name + " event", this, ASSASSINATION_TRIGGER_REASONS.SUCCESS_RUMOR);
			}else if(relationshipSourceToTarget.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				relationshipRumorToTarget.AddEventModifier(-5, this.name + " event", this, ASSASSINATION_TRIGGER_REASONS.SUCCESS_RUMOR);
			}
			if(rumorType == RUMOR_TYPE.NEGATIVE){
				//TODO: Add log - successful negative rumor
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rumor", "success_negative");
				newLog.AddToFillers (this.rumorKingdom.king, this.rumorKingdom.king.name, LOG_IDENTIFIER.KING_2);
				newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_3);
			}else if(rumorType == RUMOR_TYPE.POSITIVE){
				//TODO: Add log - successful positive rumor
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rumor", "success_positive");
				newLog.AddToFillers (this.rumorKingdom.king, this.rumorKingdom.king.name, LOG_IDENTIFIER.KING_2);
				newLog.AddToFillers (this.targetKingdom.king, this.targetKingdom.king.name, LOG_IDENTIFIER.KING_3);
			}

		}else{
			//Fail Rumor
			//TODO: Add log - rumor failed
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rumor", "fail");
			newLog.AddToFillers (this.rumorKingdom.king, this.rumorKingdom.king.name, LOG_IDENTIFIER.KING_2);
			newLog.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);

			if(rumorType == RUMOR_TYPE.NEGATIVE){
				int chance2 = UnityEngine.Random.Range(0,100);
				if(chance2 < 20){
					//TODO: Add log - rumor king found out that rumor is lie
					Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "Rumor", "caught");
					newLog.AddToFillers (this.rumorKingdom.king, this.rumorKingdom.king.name, LOG_IDENTIFIER.KING_2);
					newLog2.AddToFillers (this.sourceKingdom.king, this.sourceKingdom.king.name, LOG_IDENTIFIER.KING_1);

					RelationshipKings relationshipRumorToSource = this.rumorKingdom.king.GetRelationshipWithCitizen(this.sourceKingdom.king);
					if(relationshipRumorToSource != null){
						relationshipRumorToSource.AddEventModifier(-5, this.name + " event", this, ASSASSINATION_TRIGGER_REASONS.CAUGHT_RUMOR);
					}
				}
			}
		}
		this.DoneEvent();
	}

	private RUMOR_TYPE GetRumorType(RelationshipKings relationship){
		if(relationship != null){
			if(relationship.lordRelationship == RELATIONSHIP_STATUS.COLD || relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
				return RUMOR_TYPE.NEGATIVE;
			}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.WARM || relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND || relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
				return RUMOR_TYPE.POSITIVE;
			}
		}
		return RUMOR_TYPE.POSITIVE;
	}
}
