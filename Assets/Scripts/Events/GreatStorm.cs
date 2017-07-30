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

	public GreatStorm(int startWeek, int startMonth, int startYear, Citizen startedBy, Kingdom affectedKingdom) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.GREAT_STORM;
		this.name = "Great Storm";
		this.durationInDays = UnityEngine.Random.Range(15,31);
		SetDamagePercentageAndAfterEffectsDuration();
		this._affectedKingdom = affectedKingdom;
		this._affectedKingdom.SetLockDown(true);
		this._affectedKingdom.SetTechProduction(false);
		this._affectedKingdom.SetGrowthState(false);
		this.isStorming = true;
		this.daysCounter = 0;
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
		this._affectedKingdom.SetTechProductionPercentage(100);
		this._affectedKingdom.SetProductionGrowthPercentage(100);
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		citizen.Death (DEATH_REASONS.BATTLE);
		this.DoneEvent();
	}
	internal override void CancelEvent (){
		base.CancelEvent ();
		this.DoneEvent ();
	}
	#endregion

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
		if(chance < 10){
			City chosenCity = this._affectedKingdom.cities[UnityEngine.Random.Range(0,this._affectedKingdom.cities.Count)];
			//Destroy Random Structure in City
		}
	}
	private void StartStormAfterEffects(){
		//TODO: Add log - after effects
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "GreatStorm", "after_effects");
		newLog.AddToFillers (this._affectedKingdom, this._affectedKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (null, this.damagePercentage.ToString(), LOG_IDENTIFIER.OTHER);


		this.isStorming = false;
		this._affectedKingdom.SetLockDown(false);
		this._affectedKingdom.SetTechProduction(true);
		this._affectedKingdom.SetGrowthState(true);

		//Production and Tech growth will reduce by the damage percentage taken by the kingdom
		int growthValue = 100 - this.damagePercentage;
		float growthPercentage = (float)growthValue / 100f;
		this._affectedKingdom.SetTechProductionPercentage(growthPercentage);
		this._affectedKingdom.SetProductionGrowthPercentage(growthPercentage);

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
				if(chance < 40){
					if(KingdomManager.Instance.allKingdoms[i].id != this._affectedKingdom.id){
						RelationshipKings relationship = KingdomManager.Instance.allKingdoms[i].king.GetRelationshipWithCitizen(this._affectedKingdom.king);
						if(relationship != null){
							int percentToGive = 0;
							if(relationship.lordRelationship == RELATIONSHIP_STATUS.WARM){
								percentToGive = 10;
							}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND){
								percentToGive = 20;
							}else if(relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
								percentToGive = 30;
							}
							if(percentToGive != 0){
								//Send Reliever
								City chosenCity = KingdomManager.Instance.allKingdoms[i].cities.FirstOrDefault(x => x.currentGrowth == KingdomManager.Instance.allKingdoms[i].cities.Max(y => y.currentGrowth));
								int reliefGoods = (int)(chosenCity.currentGrowth * (percentToGive / 100f));
								chosenCity.AdjustDailyGrowth(-reliefGoods);
								EventCreator.Instance.CreateSendReliefGoodsEvent(KingdomManager.Instance.allKingdoms[i], this._affectedKingdom, this, reliefGoods);
							}
						}
					}
				}
			}
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
								this._warTrigger = WAR_TRIGGER.GREAT_STORM;
								War war = new War (GameManager.Instance.days, GameManager.Instance.month, GameManager.Instance.year, KingdomManager.Instance.allKingdoms[i].king, KingdomManager.Instance.allKingdoms[i], this._affectedKingdom);
								war.CreateInvasionPlan (KingdomManager.Instance.allKingdoms[i], this, this.warTrigger);
							}
						}
					}
				}
			}
		}
	}
}
