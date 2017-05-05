using UnityEngine;
using System.Collections;

public class EventListItem : MonoBehaviour {

	public delegate void OnClickEvent(GameEvent gameEvent);
	public OnClickEvent onClickEvent;

	public GameEvent gameEvent;
	public UILabel eventTitleLbl;
	public UILabel eventDateLbl;

	public Kingdom ownerOfThisItem;

	public void SetEvent(GameEvent gameEvent, Kingdom ownerOfThisItem){
		this.ownerOfThisItem = ownerOfThisItem;
		this.gameEvent = gameEvent;
		eventDateLbl.text = ((MONTH)gameEvent.startMonth).ToString () + " " + gameEvent.startWeek.ToString () + ", " + gameEvent.startYear.ToString ();
		if (gameEvent.eventType == EVENT_TYPES.ADMIRATION) {
			Admiration currEvent = (Admiration)gameEvent;
			if (ownerOfThisItem.id == currEvent.kingdom1.id) {
				eventTitleLbl.text = "Admiration to " + currEvent.kingdom2.name;
			}else{
				eventTitleLbl.text = "Admiration from " + currEvent.kingdom1.name;
			}
		} else if (gameEvent.eventType == EVENT_TYPES.ASSASSINATION) {
			eventTitleLbl.text = "Assassinate " + ((Assassination)gameEvent).targetCitizen.name;
		} else if (gameEvent.eventType == EVENT_TYPES.BORDER_CONFLICT) {
			BorderConflict currEvent = (BorderConflict)gameEvent;
			if (ownerOfThisItem.id == currEvent.kingdom1.id) {
				eventTitleLbl.text = "Border Conflict with  " + currEvent.kingdom2.name;
			} else {
				eventTitleLbl.text = "Border Conflict with  " + currEvent.kingdom1.name;
			}
		} else if (gameEvent.eventType == EVENT_TYPES.DIPLOMATIC_CRISIS) {
			DiplomaticCrisis currEvent = (DiplomaticCrisis)gameEvent;
			if (ownerOfThisItem.id == currEvent.kingdom1.id) {
				eventTitleLbl.text = "Diplomatic Crisis with  " + currEvent.kingdom2.name;
			} else {
				eventTitleLbl.text = "Diplomatic Crisis with  " + currEvent.kingdom1.name;
			}
		} else if (gameEvent.eventType == EVENT_TYPES.ESPIONAGE) {
			Espionage currEvent = (Espionage)gameEvent;
			if (ownerOfThisItem.id == currEvent.sourceKingdom.id) {
				eventTitleLbl.text = "Espionage towards  " + currEvent.targetKingdom.name;
			} else {
				eventTitleLbl.text = "Espionage from" + currEvent.sourceKingdom.name;
			}
		} else if (gameEvent.eventType == EVENT_TYPES.EXHORTATION) {
			Exhortation currEvent = (Exhortation)gameEvent;
			if (ownerOfThisItem.id == currEvent.startedByKingdom.id) {
				eventTitleLbl.text = "Exhoration towards  " + currEvent.targetCitizen.name;
			} else {
				eventTitleLbl.text = "Exhoration from" + currEvent.startedByKingdom.name;
			}
		} else if (gameEvent.eventType == EVENT_TYPES.INVASION_PLAN) {
			InvasionPlan currEvent = (InvasionPlan)gameEvent;
			eventTitleLbl.text = "Invasion Plan against " + currEvent.targetKingdom.name;
		} else if (gameEvent.eventType == EVENT_TYPES.JOIN_WAR_REQUEST) {
			JoinWar currEvent = (JoinWar)gameEvent;
			eventTitleLbl.text = "Invite " + currEvent.candidateForAlliance.name + " of " + currEvent.candidateForAlliance.city.kingdom.name + " to join war";
		} else if (gameEvent.eventType == EVENT_TYPES.MILITARIZATION) {
			Militarization currEvent = (Militarization)gameEvent;
			eventTitleLbl.text = "Militarization for war against " + currEvent.invasionPlanThatTriggeredEvent.targetKingdom;
		} else if (gameEvent.eventType == EVENT_TYPES.POWER_GRAB) {
			PowerGrab currEvent = (PowerGrab)gameEvent;
			eventTitleLbl.text = "Power Grab against  " + currEvent.kingToOverthrow.name;
		} else if (gameEvent.eventType == EVENT_TYPES.RAID) {
			Raid currEvent = (Raid)gameEvent;
			if (ownerOfThisItem.id == currEvent.startedByKingdom.id) {
				eventTitleLbl.text = "Raid towards " + currEvent.raidedCity.name;
			} else {
				eventTitleLbl.text = "Raid from " + currEvent.startedByKingdom.name;
			}
		} else if (gameEvent.eventType == EVENT_TYPES.REQUEST_PEACE) {
			RequestPeace currEvent = (RequestPeace)gameEvent;
			eventTitleLbl.text = "Request Peace from  " + currEvent.targetKingdom.name;
		} else if (gameEvent.eventType == EVENT_TYPES.STATE_VISIT) {
			StateVisit currEvent = (StateVisit)gameEvent;
			if (ownerOfThisItem.id == currEvent.invitedKingdom.id) {
				eventTitleLbl.text = "State visit request from  " + currEvent.inviterKingdom.name;
			} else {
				eventTitleLbl.text = "Invited " + currEvent.invitedKingdom.name + " for State Visit";
			}
		} else if (gameEvent.eventType == EVENT_TYPES.KINGDOM_WAR) {
			War currEvent = (War)gameEvent;
			if (ownerOfThisItem.id == currEvent.kingdom1.id) {
				eventTitleLbl.text = "War against " + currEvent.kingdom2.name;
			} else {
				eventTitleLbl.text = "War against " + currEvent.kingdom1.name;
			}
		}
	}

	void OnClick(){
		if (onClickEvent != null) {
			onClickEvent(this.gameEvent);
		}
	}
}
