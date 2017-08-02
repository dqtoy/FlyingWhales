using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GreatStorm : GameEvent {
	private Kingdom _affectedKingdom;
	private int damagePercentage;
	private int afterEffectsDuration;
	private bool isStorming;

	private int daysCounter;
	private int destroyedStructures;
	private int totalStructures;
	public GreatStorm(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom affectedKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.GREAT_STORM;
		this.name = "Great Storm";
		this.durationInDays = UnityEngine.Random.Range(15,31);
		this.afterEffectsDuration = this.durationInDays;
		this._affectedKingdom = affectedKingdom;
		this._affectedKingdom.SetLockDown(true);
		this._affectedKingdom.SetTechProduction(false);
		this._affectedKingdom.SetGrowthState(false);
		this.isStorming = true;
		this.totalStructures = GetTotalStructuresOfKingdom ();
		this.damagePercentage = 0;
		this.daysCounter = 0;
		this.destroyedStructures = 0;
		this._warTrigger = WAR_TRIGGER.GREAT_STORM;

		EventManager.Instance.AddEventToDictionary(this);
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

		//TODO: Add log - event title
		//TODO: Add log - start
		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "event_title");

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "start");
		newLog.AddToFillers (this._affectedKingdom, this._affectedKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

		this.EventIsCreated();
	}

	#region Overrides
	internal override void PerformAction (){
		if(this.isStorming){
			this.durationInDays -= 1;
			if(this.durationInDays <= 0){
				this.durationInDays = 0;
				StartStormAfterEffects();
			}else{
				StormEffects();
			}
		}else{
			this.daysCounter += 1;
			if(this.daysCounter >= this.afterEffectsDuration){
				this.daysCounter = 0;
				EndStormAfterEffects();
			}else{
				StormAfterEffects();
			}
		}
	}
	internal override void DoneEvent (){
		base.DoneEvent ();
		this._affectedKingdom.SetTechProductionPercentage(1);
		this._affectedKingdom.SetProductionGrowthPercentage(1);
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion
	private int GetTotalStructuresOfKingdom(){
		int count = 0;
		for (int i = 0; i < this._affectedKingdom.cities.Count; i++) {
			count += this._affectedKingdom.cities [i].structures.Count;
		}
		return count;
	}
	private void SetDamagePercentageAndAfterEffectsDuration(){
		if(this.durationInDays >= 15 && this.durationInDays < 18){
			this.damagePercentage = UnityEngine.Random.Range(1,20);
			this.afterEffectsDuration = 14;
		}else if(this.durationInDays >= 18 && this.durationInDays < 21){
			this.damagePercentage = UnityEngine.Random.Range(20,40);
			this.afterEffectsDuration = 28;
		}else if(this.durationInDays >= 21 && this.durationInDays < 24){
			this.damagePercentage = UnityEngine.Random.Range(40,60);
			this.afterEffectsDuration = 42;
		}else if(this.durationInDays >= 24 && this.durationInDays < 27){
			this.damagePercentage = UnityEngine.Random.Range(60,80);
			this.afterEffectsDuration = 56;
		}else{
			this.damagePercentage = UnityEngine.Random.Range(80,90);
			this.afterEffectsDuration = 70;
		}
	}
	private void StormEffects(){
		int chance = UnityEngine.Random.Range(0, 100);
		if(chance < 15){
			City chosenCity = this._affectedKingdom.cities[UnityEngine.Random.Range(0,this._affectedKingdom.cities.Count)];
			//Destroy Random Structure in City
			chosenCity.RemoveTileFromCity(chosenCity.structures[chosenCity.structures.Count - 1]);
			this.destroyedStructures += 1;
		}
	}
	private void StartStormAfterEffects(){
		//TODO: Add log - after effects
		this.damagePercentage = (int)(this.destroyedStructures / this.totalStructures);

		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "after_effects");
		newLog.AddToFillers (this._affectedKingdom, this._affectedKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, this.damagePercentage.ToString(), LOG_IDENTIFIER.OTHER);

		this.isStorming = false;
		this._affectedKingdom.SetLockDown(false);
		this._affectedKingdom.SetTechProduction(true);
		this._affectedKingdom.SetGrowthState(true);
	}
	private void StormAfterEffects(){
		//Send relief goods
		//Intercept relied goods
		//Wage war
		SendReliefGoods();
		WageWar();
	}
	private void EndStormAfterEffects(){
		//TODO: Add log - storm and effects has passed
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "end_effects");
		newLog.AddToFillers (this._affectedKingdom, this._affectedKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

		this.DoneEvent();
	}

	private void SendReliefGoods(){
		if(this.daysCounter % 3 == 0){
			List<Kingdom> otherFriendlyKingdoms = new List<Kingdom>();
			for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
				int chance = UnityEngine.Random.Range(0,100);
				if(chance < 50){
					if(KingdomManager.Instance.allKingdoms[i].id != this._affectedKingdom.id){
						RelationshipKings relationship = KingdomManager.Instance.allKingdoms[i].king.GetRelationshipWithCitizen(this._affectedKingdom.king);
						if(relationship != null){
							//Send Reliever
							City chosenCity = KingdomManager.Instance.allKingdoms[i].cities.FirstOrDefault(x => x.currentGrowth == KingdomManager.Instance.allKingdoms[i].cities.Max(y => y.currentGrowth));
							int reliefGoods = chosenCity.currentGrowth;
							chosenCity.AdjustDailyGrowth(-reliefGoods);
							ReliefGoods (reliefGoods, KingdomManager.Instance.allKingdoms [i]);
//							EventCreator.Instance.CreateSendReliefGoodsEvent(KingdomManager.Instance.allKingdoms[i], this._affectedKingdom, this, reliefGoods);
						}
					}
				}
			}
		}
	}
	private void ReliefGoods(int reliefGoods, Kingdom senderKingdom){
		int chance = UnityEngine.Random.Range(0,100);
		if(chance < 40){
			Intercept (reliefGoods, senderKingdom);
		}else{
			ReceiveReliefGoods (reliefGoods, senderKingdom);
		}
	}
	private void Intercept(int reliefGoods, Kingdom senderKingdom){
		List<Kingdom> otherKingdoms = new List<Kingdom>();
		for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
			Kingdom currentKingdom = KingdomManager.Instance.allKingdoms[i];
			if(currentKingdom.id != senderKingdom.id && currentKingdom.id != this._affectedKingdom.id){
				RelationshipKingdom relationshipKingdomToReceiver = currentKingdom.GetRelationshipWithOtherKingdom(this._affectedKingdom);
				if(relationshipKingdomToReceiver != null && !relationshipKingdomToReceiver.isAtWar){
					RelationshipKings relationshipToReceiver = currentKingdom.king.GetRelationshipWithCitizen(this._affectedKingdom.king);
					RelationshipKings relationshipToSender = currentKingdom.king.GetRelationshipWithCitizen(senderKingdom.king);
					if(relationshipToReceiver != null && relationshipToSender != null){
						if((relationshipToReceiver.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationshipToReceiver.lordRelationship == RELATIONSHIP_STATUS.RIVAL)
							&& (relationshipToSender.lordRelationship != RELATIONSHIP_STATUS.FRIEND && relationshipToSender.lordRelationship != RELATIONSHIP_STATUS.ALLY)){
							otherKingdoms.Add(currentKingdom);
						}
					}
				}
			}
		}
		if (otherKingdoms.Count > 0) {
			Kingdom intercepterKingdom = otherKingdoms [UnityEngine.Random.Range (0, otherKingdoms.Count)];
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 60){
				InterceptReliefGoods(reliefGoods, senderKingdom, intercepterKingdom);
			}else{
				ReceiveReliefGoods(reliefGoods, senderKingdom);
				DiscoveredIntercepter(reliefGoods, senderKingdom, intercepterKingdom);
			}
		}else{
			ReceiveReliefGoods(reliefGoods, senderKingdom);
		}
	}
	private void InterceptReliefGoods(int reliefGoods, Kingdom senderKingdom, Kingdom intercepterKingdom){
		City chosenCity = intercepterKingdom.cities.FirstOrDefault(x => x.currentGrowth == this._affectedKingdom.cities.Min(y => y.currentGrowth));
		chosenCity.AdjustDailyGrowth(reliefGoods);

		//TODO: Add log - intercept relief goods
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "intercept_relief");
		newLog.AddToFillers (intercepterKingdom, intercepterKingdom.name, LOG_IDENTIFIER.KINGDOM_3);
	}
	private void ReceiveReliefGoods(int reliefGoods, Kingdom senderKingdom){
		if(!this._affectedKingdom.isDead){
			City chosenCity = this._affectedKingdom.cities.FirstOrDefault(x => x.currentGrowth == this._affectedKingdom.cities.Min(y => y.currentGrowth));
			chosenCity.AdjustDailyGrowth(reliefGoods);

			RelationshipKings relationship = this._affectedKingdom.king.GetRelationshipWithCitizen(senderKingdom.king);
			if(relationship != null){
				relationship.AdjustLikeness(20, this);
			}
			//TODO: Add log - received relief goods
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "receive_relief");
			newLog.AddToFillers (this._affectedKingdom.king, this._affectedKingdom.king.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this._affectedKingdom, this._affectedKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (senderKingdom.king, senderKingdom.king.name, LOG_IDENTIFIER.KING_2);
			newLog.AddToFillers (senderKingdom, senderKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

		}
	}
	private void DiscoveredIntercepter(int reliefGoods, Kingdom senderKingdom, Kingdom intercepterKingdom){
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "discovered_intercepter");
		newLog.AddToFillers (senderKingdom.king, senderKingdom.king.name, LOG_IDENTIFIER.KING_2);
		newLog.AddToFillers (this._affectedKingdom.king, this._affectedKingdom.king.name, LOG_IDENTIFIER.KING_1);

		RelationshipKings relationshipFromSender = senderKingdom.king.GetRelationshipWithCitizen(intercepterKingdom.king);
		RelationshipKings relationshipFromReceiver = this._affectedKingdom.king.GetRelationshipWithCitizen(intercepterKingdom.king);
		if(relationshipFromSender != null){
			relationshipFromSender.AdjustLikeness(-20, this);
		}
		if(relationshipFromReceiver != null){
			relationshipFromReceiver.AdjustLikeness(-20, this);
		}
	}
	private void WageWar(){
		if(this.daysCounter % 7 == 0){
			for (int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++) {
				int chance = UnityEngine.Random.Range(0,100);
				if(chance < 40){
					if(KingdomManager.Instance.allKingdoms[i].id != this._affectedKingdom.id){
						RelationshipKings relationship = KingdomManager.Instance.allKingdoms[i].king.GetRelationshipWithCitizen(this._affectedKingdom.king);
						if(relationship != null){
							int percentToGive = 0;
							if(relationship.lordRelationship == RELATIONSHIP_STATUS.ENEMY || relationship.lordRelationship == RELATIONSHIP_STATUS.RIVAL){
								//TODO: Add log - wage war
								War war = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, KingdomManager.Instance.allKingdoms[i].king, KingdomManager.Instance.allKingdoms[i], this._affectedKingdom, this.warTrigger);
								war.CreateInvasionPlan (KingdomManager.Instance.allKingdoms[i], this);
							}
						}
					}
				}
			}
		}
	}
}
