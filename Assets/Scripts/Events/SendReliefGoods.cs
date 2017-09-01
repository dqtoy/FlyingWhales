using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SendReliefGoods : GameEvent {
	internal Kingdom senderKingdom;
	internal Kingdom receiverKingdom;
	internal GameEvent gameEvent;
	internal int reliefGoods;

	internal Reliever reliever;
	internal Intercepter intercepter;

	public SendReliefGoods(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom receiverKingdom, Reliever reliever, GameEvent gameEvent, int reliefGoods) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SEND_RELIEF_GOODS;
		this.name = "Send Relief Goods";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.senderKingdom = startedBy.city.kingdom;
		this.receiverKingdom = receiverKingdom;
		this.reliever = reliever;
		this.gameEvent = gameEvent;
		this.reliefGoods = reliefGoods;
		this.intercepter = null;

		//TODO: Add log - sent reliever
		Log newLog = gameEvent.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "send_reliever");
		newLog.AddToFillers (this.senderKingdom.king, this.senderKingdom.king.name, LOG_IDENTIFIER.KING_2);
		newLog.AddToFillers (this.senderKingdom, this.senderKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (this.reliever.citizen, this.reliever.citizen.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		newLog.AddToFillers (this.receiverKingdom, this.receiverKingdom.name, LOG_IDENTIFIER.KINGDOM_1);


		SendIntercepterOtherKingdoms();
	}

	#region Overrides
	internal override void DoneCitizenAction (Citizen citizen){
		base.DoneCitizenAction (citizen);
		if(citizen.assignedRole is Reliever){
			RelieverArrival();
		}else if(citizen.assignedRole is Intercepter){
			//Intercepter arrives at the city
		}
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
	internal override void DoneEvent(){
		base.DoneEvent();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

	private void SendIntercepterOtherKingdoms(){
		List<Kingdom> otherKingdoms = new List<Kingdom>();
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[i];
			if(currentKingdom.id != this.senderKingdom.id && currentKingdom.id != this.receiverKingdom.id){
				KingdomRelationship relationshipKingdomToReceiver = currentKingdom.GetRelationshipWithKingdom(this.receiverKingdom);
				if(relationshipKingdomToReceiver != null && !relationshipKingdomToReceiver.isAtWar){
					KingdomRelationship relationshipToReceiver = currentKingdom.GetRelationshipWithKingdom(this.receiverKingdom);
					KingdomRelationship relationshipToSender = currentKingdom.GetRelationshipWithKingdom(this.senderKingdom);
					if(relationshipToReceiver != null && relationshipToSender != null){
						if((relationshipToReceiver.relationshipStatus == RELATIONSHIP_STATUS.ENEMY || relationshipToReceiver.relationshipStatus == RELATIONSHIP_STATUS.RIVAL)
							&& (relationshipToSender.relationshipStatus != RELATIONSHIP_STATUS.FRIEND && relationshipToSender.relationshipStatus != RELATIONSHIP_STATUS.ALLY)){
							otherKingdoms.Add(currentKingdom);
						}
					}
				}
			}
		}

		if(otherKingdoms.Count > 0){
			otherKingdoms = Utilities.Shuffle(otherKingdoms);
			for (int i = 0; i < otherKingdoms.Count; i++) {
				Citizen citizen = otherKingdoms[i].capitalCity.CreateAgent(ROLE.INTERCEPTER, this.eventType, this.receiverKingdom.capitalCity.hexTile, this.reliever.path.Count);
				if(citizen != null){
					this.intercepter = (Intercepter)citizen.assignedRole;
					this.intercepter.Initialize(this);

					//TODO: Add log - send intercepter
					Log newLog = this.gameEvent.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "send_intercepter");
					newLog.AddToFillers (otherKingdoms[i].king, otherKingdoms[i].king.name, LOG_IDENTIFIER.KING_3);
					newLog.AddToFillers (otherKingdoms[i], otherKingdoms[i].name, LOG_IDENTIFIER.KINGDOM_3);
					newLog.AddToFillers (this.intercepter.citizen, this.intercepter.citizen.name, LOG_IDENTIFIER.CHARACTER_3);
					newLog.AddToFillers (this.receiverKingdom, this.receiverKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
					break;
				}
			}
		}
	}

	private void RelieverArrival(){
		if(this.intercepter == null){
			ReceiveReliefGoods();
		}else{
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 80){
				InterceptReliefGoods();
			}else{
				ReceiveReliefGoods();
				DiscoveredIntercepter();
			}
		}
		this.DoneEvent();
	}

	private void ReceiveReliefGoods(){
		if(!this.receiverKingdom.isDead){
			City chosenCity = this.receiverKingdom.cities.FirstOrDefault(x => x.currentGrowth == this.receiverKingdom.cities.Min(y => y.currentGrowth));
			chosenCity.AdjustDailyGrowth(this.reliefGoods);

			KingdomRelationship relationship = this.receiverKingdom.GetRelationshipWithKingdom(this.senderKingdom);
			if(relationship != null){
				relationship.AddEventModifier(5, this.name + " event", this);
			}
			//TODO: Add log - received relief goods
			Log newLog = this.gameEvent.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "receive_relief");
			newLog.AddToFillers (this.receiverKingdom.king, this.receiverKingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.receiverKingdom, this.receiverKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (this.senderKingdom.king, this.senderKingdom.king.name, LOG_IDENTIFIER.KING_2);
			newLog.AddToFillers (this.senderKingdom, this.senderKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

		}
	}
	private void InterceptReliefGoods(){
		if(!this.intercepter.citizen.city.kingdom.isDead){
			City chosenCity = this.intercepter.citizen.city.kingdom.cities.FirstOrDefault(x => x.currentGrowth == this.receiverKingdom.cities.Min(y => y.currentGrowth));
			chosenCity.AdjustDailyGrowth(this.reliefGoods);

			//TODO: Add log - intercept relief goods
			Log newLog = this.gameEvent.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "intercept_relief");
			newLog.AddToFillers (this.intercepter.citizen, this.intercepter.citizen.name, LOG_IDENTIFIER.CHARACTER_3);
			newLog.AddToFillers (this.intercepter.citizen.city.kingdom, this.intercepter.citizen.city.kingdom.name, LOG_IDENTIFIER.KINGDOM_3);
		}

	}
	private void DiscoveredIntercepter(){
		//TODO: Add log - discovered intercepter
		Log newLog = this.gameEvent.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "discovered_intercepter");
		newLog.AddToFillers (this.intercepter.citizen, this.intercepter.citizen.name, LOG_IDENTIFIER.CHARACTER_3);
		newLog.AddToFillers (this.intercepter.citizen.city.kingdom, this.intercepter.citizen.city.kingdom.name, LOG_IDENTIFIER.KINGDOM_3);
		newLog.AddToFillers (this.senderKingdom.king, this.senderKingdom.king.name, LOG_IDENTIFIER.KING_2);
		newLog.AddToFillers (this.receiverKingdom.king, this.receiverKingdom.king.name, LOG_IDENTIFIER.KING_1);

		this.intercepter.citizen.Death(DEATH_REASONS.TREACHERY);
		KingdomRelationship relationshipFromSender = this.senderKingdom.GetRelationshipWithKingdom(this.intercepter.citizen.city.kingdom);
		KingdomRelationship relationshipFromReceiver = this.receiverKingdom.GetRelationshipWithKingdom(this.intercepter.citizen.city.kingdom);
		if(relationshipFromSender != null){
			relationshipFromSender.AddEventModifier(-5, this.name + " event", this);
		}
		if(relationshipFromReceiver != null){
			relationshipFromReceiver.AddEventModifier(-5, this.name + " event", this);
		}
	}
}
