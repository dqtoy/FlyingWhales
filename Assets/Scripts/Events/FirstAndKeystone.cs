using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FirstAndKeystone : GameEvent {
	private delegate void OnPerformAction();
	private OnPerformAction onPerformAction;
//	private delegate void OnPhaseAction(Kingdom kingdom);
//	private OnPhaseAction onPhaseAction;

	internal Kingdom firstOwner;
	internal Kingdom keystoneOwner;
	internal HexTile hexTileSpawnPoint;
	internal GameObject avatar;
	internal HexTile keystonePlacement;
	internal HexTile firstPlacement;

	private int daysCounter;
	private int startDayOfDecision;
	private Kingdom decidingKingdom;
	private Citizen thief;

	public FirstAndKeystone(int startWeek, int startMonth, int startYear, Citizen startedBy, HexTile hexTile) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.FIRST_AND_KEYSTONE;
		this.name = "The First and The Keystone";
		this.durationInDays = EventManager.Instance.eventDuration[this.eventType];
		this.hexTileSpawnPoint = hexTile;
		this.firstOwner = null;
		this.keystoneOwner = null;
		this.daysCounter = 0;
		WorldEventManager.Instance.AddWorlEvent(this);
		Initialize();
		EventManager.Instance.onWeekEnd.AddListener(this.PerformAction);

	}

	#region Overrides
	internal override void PerformAction (){
		this.daysCounter += 1;
		if(onPerformAction != null){
			onPerformAction ();
		}
	}
	internal override void DoneEvent (){
		base.DoneEvent ();
		onPerformAction = null;
//		onPhaseAction = null;
		WorldEventManager.Instance.RemoveWorldEvent(this);
		EventManager.Instance.onWeekEnd.RemoveListener(this.PerformAction);
		ResetFirstAndKeystoneOwnershipValues();
	}
	internal override void DoneCitizenAction (Citizen citizen){
		this.thief = null;
		base.DoneCitizenAction (citizen);
		if(citizen.assignedRole.targetCity.kingdom != this.firstOwner && citizen.assignedRole.targetCity.kingdom != this.keystoneOwner){
			return;
		}
		if(citizen.assignedRole is Envoy){
			Request(citizen.city.kingdom, citizen.assignedRole.targetCity.kingdom);
		}else if(citizen.assignedRole is Investigator){
			Investigate(citizen.city.kingdom, citizen.assignedRole.targetCity.kingdom);
		}else if(citizen.assignedRole is Thief){
			Steal(citizen.city.kingdom, citizen.assignedRole.targetCity.kingdom);
		}
	}
	#endregion
	private void Initialize(){
		this.hexTileSpawnPoint.PutEventOnTile (this);
		this.avatar = GameObject.Instantiate (Resources.Load ("GameObjects/Keystone"), this.hexTileSpawnPoint.transform) as GameObject;
		this.avatar.transform.localPosition = Vector3.zero;
//		this.avatar.GetComponent<BoonOfPowerAvatar>().Init(this);
	}
	internal void ChangeKeystoneOwnership(Kingdom kingdom){
		this.keystoneOwner = kingdom;
		this.keystoneOwner.firstAndKeystoneOwnership.approach = GetApproach(this.keystoneOwner);
		onPerformAction -= StealProcess;
		ChangeKeystonePlacement();
		CheckKeystoneOwnerPhase();
	}
	internal void ChangeFirstOwnership(Kingdom kingdom){
		this.firstOwner = kingdom;
		this.firstOwner.firstAndKeystoneOwnership.approach = GetApproach(this.firstOwner);
		ChangeFirstPlacement();
		CheckFirstOwnerPhase();
	}
	internal void ChangeKeystonePlacement(){
		City randomCity = this.keystoneOwner.cities[UnityEngine.Random.Range(0, this.keystoneOwner.cities.Count)];
		HexTile randomSettlement = randomCity.structures[UnityEngine.Random.Range(0, randomCity.structures.Count)];
		SetKeystone(randomSettlement);
	}
	private void SetKeystone(HexTile hexTile){
		if(this.keystonePlacement != null){
			this.keystonePlacement.SetKeystone(false);
		}
		hexTile.SetKeystone(true);
		this.keystonePlacement = hexTile;
	}
	internal void ChangeFirstPlacement(){
		City randomCity = this.firstOwner.cities[UnityEngine.Random.Range(0, this.firstOwner.cities.Count)];
		HexTile randomSettlement = randomCity.structures[UnityEngine.Random.Range(0, randomCity.structures.Count)];
		SetFirst(randomSettlement);
	}
	private void SetFirst(HexTile hexTile){
		if(this.firstPlacement != null){
			this.firstPlacement.SetKeystone(false);
		}
		hexTile.SetFirst(true);
		this.firstPlacement = hexTile;
	}
	internal void TransferKeystone(Kingdom kingdom, Citizen citizen){
		this.EventIsCreated ();
		this.keystoneOwner = kingdom;
		ChangeKeystonePlacement();
		GameObject.Destroy (this.avatar);

		onPerformAction += AttemptVisit;
//		if(citizen == null){
//			//Discovered by structure/tile
//			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BoonOfPower", "discovery_structure");
//			newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
//		}else{
//			//Discovered by an agent
//			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "BoonOfPower", "discovery_agent");
//			newLog.AddToFillers (citizen, citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
//			newLog.AddToFillers (kingdom, kingdom.name, LOG_IDENTIFIER.KINGDOM_1);
//		}
	}
	private void AttemptVisit(){
		if((this.daysCounter % 5) == 0){
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 3){
				TheFirstVisit();
			}
		}
	}
	private void TheFirstVisit(){
		onPerformAction -= AttemptVisit;
		//TODO: Add log: story of the first
		Kingdom kingdom = KingdomManager.Instance.allKingdoms[UnityEngine.Random.Range(0,KingdomManager.Instance.allKingdoms.Count)];
		ChangeFirstOwnership(kingdom);

	}
	private void CheckKeystoneOwnerPhase(){
		if(this.keystoneOwner.firstAndKeystoneOwnership.knowEffects){
			//Steal
			if(this.keystoneOwner.firstAndKeystoneOwnership.approach == EVENT_APPROACH.OPPORTUNISTIC){
				MILITARY_STRENGTH milStrength = this.keystoneOwner.GetMilitaryStrengthAgainst(this.firstOwner);
				if(milStrength == MILITARY_STRENGTH.COMPARABLE || milStrength == MILITARY_STRENGTH.SLIGHTLY_STRONGER || milStrength == MILITARY_STRENGTH.MUCH_STRONGER){
					StartInvasionPlanPhase();
				}else{
					StartStealPhase();
				}
			}else{
				StartStealPhase();
			}
		}else{
			//Investigate
			if(this.firstOwner.firstAndKeystoneOwnership.hasRequested){
				StartInvestigationPhase();
			}
		}
	}
	private void CheckFirstOwnerPhase(){
		if(this.firstOwner.firstAndKeystoneOwnership.hasDecided){
			if(!this.firstOwner.firstAndKeystoneOwnership.hasRequested){
				//Send request
				StartRequestPhase();
			}
		}else{
			StartDecisionMakingPhase();
		}
	}

	private void StartDecisionMakingPhase(){
		this.decidingKingdom = this.firstOwner;
		this.startDayOfDecision = this.daysCounter;
		onPerformAction += DecisionMakingProcess;
	}
	private void StartRequestPhase(){
		Citizen citizen = this.firstOwner.capitalCity.CreateAgent (ROLE.ENVOY, this.eventType, this.keystoneOwner.capitalCity.hexTile, this.remainingDays);
		if(citizen == null){
			return;
		}
		citizen.assignedRole.Initialize (this);
	}
	private void StartInvestigationPhase(){
		Citizen citizen = this.keystoneOwner.capitalCity.CreateAgent (ROLE.INVESTIGATOR, this.eventType, this.firstOwner.capitalCity.hexTile, this.remainingDays);
		if(citizen == null){
			return;
		}
		citizen.assignedRole.Initialize (this);
	}
	private void StartStealPhase(){
		onPerformAction += StealProcess;
	}
	private void StartInvasionPlanPhase(){
		//Invasion plan of keystoneOwner to firstOwner, must not have existing invasion plan, include this event in war so that we can know that this is the reason they went to war
	}
	private void DecisionMakingProcess(){
		if(this.daysCounter >= (this.startDayOfDecision + 30)){
			CreateDecision();
		}
	}
	private void CreateDecision(){
		onPerformAction -= DecisionMakingProcess;
		if(this.decidingKingdom.isAlive()){
			if(this.decidingKingdom.id != this.firstOwner.id){
				return;
			}
			this.firstOwner.firstAndKeystoneOwnership.hasDecided = true;
			CheckFirstOwnerPhase();
		}
	}

	private void Request(Kingdom kingdom1, Kingdom kingdom2){
		if(kingdom1.id != this.firstOwner.id || kingdom2.id != this.keystoneOwner.id){
			return;
		}
		this.firstOwner.firstAndKeystoneOwnership.hasRequested = true;
		if(CanBeRetrieved(this.firstOwner, this.keystoneOwner)){
			this.keystoneOwner = this.firstOwner;
			ChangeKeystonePlacement();
			CheckFirstAndKeystoneOwnership();
		}else{
			CheckKeystoneOwnerPhase();
		}

	}
	private void Investigate(Kingdom kingdom1, Kingdom kingdom2){
		if(kingdom1.id != this.keystoneOwner.id || kingdom2.id != this.firstOwner.id){
			return;
		}
		int chance = UnityEngine.Random.Range(0,100);
		if(chance < 75){
			this.keystoneOwner.firstAndKeystoneOwnership.knowEffects = true;
			CheckKeystoneOwnerPhase();
		}

	}
	private void StealProcess(){
		if(this.daysCounter % 4 == 0){
			if(this.keystoneOwner.firstAndKeystoneOwnership.approach == EVENT_APPROACH.OPPORTUNISTIC){
				MILITARY_STRENGTH milStrength = this.keystoneOwner.GetMilitaryStrengthAgainst(this.firstOwner);
				if(milStrength == MILITARY_STRENGTH.COMPARABLE || milStrength == MILITARY_STRENGTH.SLIGHTLY_STRONGER || milStrength == MILITARY_STRENGTH.MUCH_STRONGER){
					StartInvasionPlanPhase();
					return;
				}
			}
			int chance = UnityEngine.Random.Range(0,100);
			if(chance < 20){
				if(this.thief == null){
					this.thief = this.keystoneOwner.capitalCity.CreateAgent (ROLE.THIEF, this.eventType, this.firstOwner.capitalCity.hexTile, this.remainingDays);
					if(this.thief == null){
						return;
					}
					this.thief.assignedRole.Initialize (this);
				}
			}
		}
	}
	private void Steal(Kingdom kingdom1, Kingdom kingdom2){
		if(kingdom1.id != this.keystoneOwner.id || kingdom2.id != this.firstOwner.id){
			return;
		}
		int chance = UnityEngine.Random.Range(0,2);
		if(chance == 0){
			this.firstOwner = this.keystoneOwner;
			ChangeFirstPlacement();
			CheckFirstAndKeystoneOwnership();
			return;
		}else{
			RelationshipKings relationship = this.firstOwner.king.GetRelationshipWithCitizen(this.keystoneOwner.king);
			if(relationship != null){
				relationship.AdjustLikeness(-20, this);
			}
		}

	}
	internal void DestroyKeystone(){
		this.keystonePlacement.SetKeystone(false);
		this.firstPlacement.SetFirst(false);
		this.DoneEvent();
	}
	private void ResetFirstAndKeystoneOwnershipValues(){
		for(int i = 0; i < KingdomManager.Instance.allKingdoms.Count; i++){
			KingdomManager.Instance.allKingdoms[i].firstAndKeystoneOwnership.DefaultValues();
		}
	}
	private void CheckFirstAndKeystoneOwnership(){
		if(this.keystoneOwner == this.firstOwner){
			//Humanistic - destroy keystone
			if(this.keystoneOwner.firstAndKeystoneOwnership.approach == EVENT_APPROACH.HUMANISTIC){
				DestroyKeystone();
			}
			//Opporunistic - destroy other race
		}
	}

	private bool CanBeRetrieved(Kingdom retriever, Kingdom giver){
		EVENT_APPROACH retrieverApproach = GetApproach(retriever);
		EVENT_APPROACH giverApproach = GetApproach(giver);

		if(retriever.race == giver.race){
			if(retrieverApproach == giverApproach){
				return true;
			}else{
				if(!giver.firstAndKeystoneOwnership.knowEffects){
					RelationshipKings relationship = giver.king.GetRelationshipWithCitizen(retriever.king);
					if(relationship != null){
						if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND || relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
							return true;
						}
					}
				}
			}
		}else{
			if(retrieverApproach == giverApproach){
				if(retrieverApproach == EVENT_APPROACH.HUMANISTIC){
					return true;
				}
			}else{
				if(!giver.firstAndKeystoneOwnership.knowEffects){
					RelationshipKings relationship = giver.king.GetRelationshipWithCitizen(retriever.king);
					if(relationship != null){
						if(relationship.lordRelationship == RELATIONSHIP_STATUS.FRIEND || relationship.lordRelationship == RELATIONSHIP_STATUS.ALLY){
							return true;
						}
					}
				}
			}
		}
		return false;
	}
	private EVENT_APPROACH GetApproach(Kingdom kingdom){
		//Compute approach and return it if humanistic or opportunistic
		return EVENT_APPROACH.HUMANISTIC;
	}
	private Kingdom GetOwner(Kingdom kingdom){
		if(kingdom.id == this.firstOwner.id){
			return this.firstOwner;
		}else if(kingdom.id == this.keystoneOwner.id){
			return this.keystoneOwner;
		}
		return null;
	}
}
