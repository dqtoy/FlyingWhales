using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Admiration : GameEvent {

	public Kingdom kingdom1;
	public Kingdom kingdom2;

	public bool isResolvedPeacefully;

	public Admiration(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom kingdom1, Kingdom kingdom2) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.ADMIRATION;
		if(startedBy != null){
			this.description = startedBy.name + " has created admiration between " + kingdom1.name + " and " + kingdom2.name + ".";
			this.startedBy.city.cityHistory.Add (new History (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, 
				startedBy.name + " has created admiration between " + kingdom1.name + " and " + kingdom2.name + "." , HISTORY_IDENTIFIER.NONE));
		}else{
			this.description = "Admiration has began between " + kingdom1.name + " and " + kingdom2.name + ".";
		}
		this.durationInWeeks = 15;
		this.remainingWeeks = this.durationInWeeks;
		this.kingdom1 = kingdom1;
		this.kingdom2 = kingdom2;
		this.kingdom1.cities[0].hexTile.AddEventOnTile(this);
		this.kingdom2.cities[0].hexTile.AddEventOnTile(this);

		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);
		Debug.LogError (this.description);
	}

	internal override void PerformAction(){
		this.remainingWeeks -= 1;
		if (this.remainingWeeks <= 0) {
			this.remainingWeeks = 0;
			this.isResolvedPeacefully = true;
			DoneEvent ();
		}else{
			CheckIfAlreadyAtWar ();
		}
	}
	private void CheckIfAlreadyAtWar(){
		if(this.kingdom1.GetRelationshipWithOtherKingdom(this.kingdom2).isAtWar){
			this.isResolvedPeacefully = false;
			DoneEvent ();
		}
	}

	internal override void DoneEvent(){
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
		this.isActive = false;
		EventManager.Instance.onGameEventEnded.Invoke(this);
		this.endWeek = GameManager.Instance.days;
		this.endMonth = GameManager.Instance.month;
		this.endYear = GameManager.Instance.year;

		RelationshipKings relationship1 = this.kingdom1.king.SearchRelationshipByID (this.kingdom2.king.id);
		RelationshipKings relationship2 = this.kingdom2.king.SearchRelationshipByID (this.kingdom1.king.id);

		if(this.isResolvedPeacefully){
			Debug.Log("ADMIRATION BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED PEACEFULLY!");

			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endWeek + ", " + this.endYear + ". Admiration ended great.";

			relationship1.AdjustLikeness (35, this);

			relationship1.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				this.kingdom1.king.name +  " admired " + this.kingdom2.king.name + ".",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
			relationship2.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				this.kingdom1.king.name +  " admired " + this.kingdom2.king.name + ".",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
		}else{
			Debug.Log("ADMIRATION BETWEEN " + this.kingdom1.name + " AND " + this.kingdom2.name + " ENDED HORRIBLY! RELATIONSHIP DETERIORATED!");

			this.resolution = "Ended on " + ((MONTH)this.endMonth).ToString() + " " + this.endWeek + ", " + this.endYear + ". Admiration ended horribly.";

			relationship1.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				this.kingdom1.king.name +  " did not admire " + this.kingdom2.king.name + ".",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
			relationship2.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.days,
				GameManager.Instance.year,
				this.kingdom1.king.name +  " did not admire " + this.kingdom2.king.name + ".",
				HISTORY_IDENTIFIER.KING_RELATIONS,
				false
			));
		}
		//		EventManager.Instance.allEvents [EVENT_TYPES.BORDER_CONFLICT].Remove (this);

		//Remove UI Icon
	}
}
