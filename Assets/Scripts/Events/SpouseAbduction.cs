using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpouseAbduction : GameEvent {
	private delegate void OnPerformAction();
	private OnPerformAction onPerformAction;

	internal Citizen abductorKing;
	internal Citizen targetKing;
	internal Spouse abductee;
	internal Abductor abductor;
	internal Kingdom abductorKingdom;
	internal Kingdom targetKingdom;
	internal List<Kingdom> otherKingdoms;

	internal bool isSecretlyAllowed;
	internal bool isCompatibleWithTargetKing;
	internal bool isCompatibleWithAbductorKing;
	internal bool isSpouseDead;

	internal int marriageCompatibilityWithTargetKing;
	internal int marriageCompatibilityWithAbductorKing;

	private RelationshipKings relationshipKing;
	private int daysCounter;

	public SpouseAbduction(int startWeek, int startMonth, int startYear, Citizen startedBy, Citizen targetKing) : base (startWeek, startMonth, startYear, startedBy){
		this.eventType = EVENT_TYPES.SPOUSE_ABDUCTION;
		this.name = "Spouse Abduction";
		this.durationInDays = EventManager.Instance.eventDuration [this.eventType];
		this.abductorKing = startedBy;
		this.targetKing = targetKing;
		this.abductee = (Spouse)targetKing.spouse;
		this.abductor = null;
		this.abductorKingdom = startedBy.city.kingdom;
		this.targetKingdom = targetKing.city.kingdom;
		this.otherKingdoms = GetOtherKingdoms();

		this.isSecretlyAllowed = false;
		this.isCompatibleWithTargetKing = IsMarriageCompatible (targetKing);
		this.isCompatibleWithAbductorKing = false;
		this.isSpouseDead = false;

		this.marriageCompatibilityWithTargetKing = ((Spouse)targetKing.spouse)._marriageCompatibility;

		this._warTrigger = WAR_TRIGGER.SPOUSE_ABDUCTION;

		relationshipKing = this.targetKing.GetRelationshipWithCitizen (this.abductorKing);
		this.daysCounter = 0;

		Log newLogTitle = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "event_title");
		newLogTitle.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

		//Add log - King is admiring the beauty of spouse*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "admire_spouse");
		newLog.AddToFillers (this.abductorKing, this.abductorKing.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this.abductorKingdom, this.abductorKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		newLog.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);
		newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

		EventManager.Instance.AddEventToDictionary (this);
		EventManager.Instance.onWeekEnd.AddListener (this.PerformAction);
		onPerformAction += AdmireSpouse;
		this.EventIsCreated ();
	}
	#region Overrides
	internal override void PerformAction (){
		if(onPerformAction != null){
			onPerformAction ();
		}
	}
	internal override void DoneCitizenAction (Citizen citizen){
		base.DoneCitizenAction (citizen);
		AbductorArrived ();
	}
	internal override void DoneEvent (){
		base.DoneEvent ();
		onPerformAction = null;
		EventManager.Instance.onWeekEnd.RemoveListener (this.PerformAction);
	}
	internal override void DeathByOtherReasons(){
		this.DoneEvent();
	}
	internal override void DeathByAgent(Citizen citizen, Citizen deadCitizen){
		base.DeathByAgent(citizen, deadCitizen);
		this.DoneEvent();
	}
	#endregion
	private bool IsMarriageCompatible(Citizen targetCitizen){
		if(((Spouse)targetCitizen.spouse)._marriageCompatibility >= 0){
			return true;
		}
		return false;
	}
	private List<Kingdom> GetOtherKingdoms(){
		List<Kingdom> kingdoms = new List<Kingdom> ();
		for(int i = 0; i < this.abductorKingdom.discoveredKingdoms.Count; i++){
			if(this.abductorKingdom.discoveredKingdoms[i].id != this.targetKingdom.id && this.abductorKingdom.discoveredKingdoms[i].isAlive()){
				kingdoms.Add (this.abductorKingdom.discoveredKingdoms[i]);
			}
		}
		return kingdoms;
	}
	private void AdmireSpouse(){
		this.daysCounter += 1;
		if(this.daysCounter >= 15){
			this.daysCounter = 0;
			SendAbductor ();
		}
	}

	private void SendAbductor(){
		//Add log - Send abductor*
		onPerformAction -= AdmireSpouse;
		Citizen citizen = this.abductorKingdom.capitalCity.CreateAgent (ROLE.ABDUCTOR, this.eventType, this.targetKingdom.capitalCity.hexTile, this.durationInDays);
		if(citizen != null){
			this.abductor = (Abductor)citizen.assignedRole;
			this.abductor.Initialize (this);
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "send_abductor");
			newLog.AddToFillers (this.abductorKing, this.abductorKing.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.abductorKingdom, this.abductorKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			newLog.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

		}
	}

	private void AbductorArrived(){
		//Add log - Abductor arrived at city and will begin abduction*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "arrive_abductor");
		newLog.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);

		if(this.abductee.isAbducted){
			Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "already_abducted");
			newLog2.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			newLog2.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

			this.DoneEvent ();
		}else{
			this.daysCounter = 0;
			onPerformAction += BeginAbduction;
		}

	}

	private void BeginAbduction(){
		this.daysCounter += 1;

		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 8){
			AbductorCaught ();
		}

		if(this.daysCounter >= 5){
			this.daysCounter = 0;
			AbductorNotCaught ();
		}
	}

	private void AbductorCaught(){
		//Add log - abductor caught and dies or abductor caught and secretly allowed*
		if(this.isCompatibleWithTargetKing){
			onPerformAction -= BeginAbduction;
			this.daysCounter = 0;
			AbductorDies ();
		}else{
			int chance = UnityEngine.Random.Range (0, 100);
			if(chance < 35){
				this.isSecretlyAllowed = true;
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "abductor_secretly_allowed");
				newLog.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
				newLog.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);
			}else{
				onPerformAction -= BeginAbduction;
				this.daysCounter = 0;
				AbductorDies ();
			}
		}
	}

	private void AbductorNotCaught(){
		onPerformAction -= BeginAbduction;
		int chance = UnityEngine.Random.Range (0, 100);
		int value = 70;
		if(!this.isCompatibleWithTargetKing){
			value += 20;
		}

		if(chance < value){
			SuccessAbduction ();	
		}else{
			FailAbduction ();
		}
	}

	private void SuccessAbduction(){
		//Add log - success abduction*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "success_abduction");
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		newLog.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

		TransferSpouse ();
		if(this.isSecretlyAllowed){
			SecretlyAllowedSuccessAbduction ();
		}else{
			NotSecretlyAllowedSuccessAbduction ();
		}

	}
	private void AssassinateKingBySpouse(bool isDoneEvent = false){
		this.isCompatibleWithAbductorKing = IsMarriageCompatible (this.abductorKing);
		if(!isCompatibleWithAbductorKing){
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				AttemptAssassinateKingBySpouse (this.abductorKing);
				//return;
			}
		}
		if(isDoneEvent){
			this.DoneEvent ();
		}
	}
	private void SecretlyAllowedSuccessAbduction(){
		HonorableOtherKingsAdjustRelationshipToBothKings();

		this.daysCounter = 0;
		onPerformAction += SecretlyAllowedAttemptAssassination;
	}
	private void SecretlyAllowedAttemptAssassination(){
		this.daysCounter += 1;
		if(this.daysCounter >= 5){
			this.daysCounter = 0;
			onPerformAction -= SecretlyAllowedAttemptAssassination;
			AssassinateKingBySpouse (true);
		}
	}
	private void NotSecretlyAllowedSuccessAbduction(){
		//Add log - target king starts searching for spouse*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "success_not_secretly_allowed");
		newLog.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

		this.daysCounter = 0;
		onPerformAction += BeginSearchForSpouse;
	}
	private void HonorableOtherKingsAdjustRelationshipToBothKings(){
		//Add log - target king will do nothing*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "do_nothing");
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		newLog.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);
		 
		AdjustRelationshipFromOtherKingdoms(this.targetKing, -10, CHARACTER_VALUE.HONOR);
		AdjustRelationshipFromOtherKingdoms(this.abductorKing, -10, CHARACTER_VALUE.HONOR);
	}
	private void BeginSearchForSpouse(){
		this.daysCounter += 1;
		if(this.daysCounter == 5){
			AssassinateKingBySpouse ();
		}
		if(this.daysCounter >= 15){
			this.daysCounter = 0;
			FoundOutAboutAbduction ();
		}

	}
	private void FoundOutAboutAbduction(){
		onPerformAction -= BeginSearchForSpouse;
		//Add log - target king has found out the true cuplrit
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "found_out");
		newLog.AddToFillers (this.abductorKing, this.abductorKing.name, LOG_IDENTIFIER.KING_1);
		newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);

		if(this.isCompatibleWithTargetKing){
			InstantWar();
			if(this.isSpouseDead){
//				if(this.relationshipKing != null){
//					this.relationshipKing.AdjustLikeness (-60, this);
//				}
				AdjustRelationshipFromOtherKingdoms (this.abductorKing, -20, CHARACTER_VALUE.LIFE);

				//Add log - instant war because of abduction and death
				Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "compatible_instant_war_dead");
				newLog2.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);
				newLog2.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLog2.AddToFillers (this.abductorKingdom, this.abductorKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

				this.DoneEvent ();
			}else{
//				if(this.relationshipKing != null){
//					this.relationshipKing.AdjustLikeness (-30, this);
//				}
				AdjustRelationshipFromOtherKingdoms (this.abductorKing, -20, CHARACTER_VALUE.HONOR);

				Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "compatible_instant_war_alive");
				newLog2.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);
				newLog2.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLog2.AddToFillers (this.abductorKingdom, this.abductorKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
			}
		}else{
			if(this.isSpouseDead){
				if(this.targetKing.importantCharacterValues.ContainsKey(CHARACTER_VALUE.LIFE)){
					InstantWar();
//					if(this.relationshipKing != null){
//						this.relationshipKing.AdjustLikeness (-25, this);
//					}
					Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "instant_war_life");
					newLog2.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);
					newLog2.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
					newLog2.AddToFillers (this.abductorKingdom, this.abductorKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
				}else{
					HonorableOtherKingsAdjustRelationshipToBothKings();
				}
				AdjustRelationshipFromOtherKingdoms(this.abductorKing, -20, CHARACTER_VALUE.LIFE);
			}else{
				HonorableOtherKingsAdjustRelationshipToBothKings();
			}
			this.DoneEvent ();
		}
	}
	private void FailAbduction(){
		//Add log - fail abduction*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "fail_abduction");
		newLog.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

		if(this.isCompatibleWithTargetKing){
			AbductorDies ();
		}else{
			Log newLog2 = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "abductor_live");
			newLog2.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);

			this.DoneEvent ();
		}
	}
	private void AbductorDies(){
		//Add log - abductor has been executed*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "abductor_dies");
		newLog.AddToFillers (this.abductor.citizen, this.abductor.citizen.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

		this.abductor.citizen.Death (DEATH_REASONS.TREACHERY);

		if(this.targetKing.GetCharacterValueOfType(CHARACTER_VALUE.PEACE) > this.targetKing.GetCharacterValueOfType(CHARACTER_VALUE.HONOR)){
			if(this.relationshipKing != null){
				this.relationshipKing.AdjustLikeness (-20, this);
			}
		}else if(this.targetKing.GetCharacterValueOfType(CHARACTER_VALUE.HONOR) > this.targetKing.GetCharacterValueOfType(CHARACTER_VALUE.PEACE)){
			InstantWar();
//			if (this.relationshipKing != null) {
//				this.relationshipKing.AdjustLikeness (-15, this);
//			}
		}else{
			int chance = UnityEngine.Random.Range (0, 2);
			if(chance == 0){
				if (this.relationshipKing != null) {
					this.relationshipKing.AdjustLikeness (-20, this);
				}
			}else{
				InstantWar();
//				if (this.relationshipKing != null) {
//					this.relationshipKing.AdjustLikeness (-15, this);
//				}
			}
		}
		AdjustRelationshipFromOtherKingdoms (this.abductorKing, -20, CHARACTER_VALUE.HONOR);

		this.DoneEvent ();
	}

	private void TransferSpouse(){
//		Add log - spouse and the abductor king will get married?
		this.targetKing.DivorceSpouse ();
		this.abductee.DivorceSpouse ();

		MarriageManager.Instance.Marry (this.abductorKing, this.abductee);
		this.abductee.GenerateMarriageCompatibility ();
		this.marriageCompatibilityWithAbductorKing = this.abductee._marriageCompatibility;
		this.abductee.isAbducted = true;
	}
	private void ReturnSpouse(){
		//Add log - spouse will return because the target kingdom wins the war*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "return_spouse");
		newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

		this.abductorKing.DivorceSpouse ();
		this.abductee.DivorceSpouse ();

		MarriageManager.Instance.Marry (this.targetKing, this.abductee);
		this.abductee._marriageCompatibility = this.marriageCompatibilityWithTargetKing;
		this.abductee.isAbducted = false;
	}
	private void AttemptAssassinateKingBySpouse(Citizen king){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 30){
			//Add log - spouse successfully assassinated king*
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "abductor_king_assassination_success");
			newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			newLog.AddToFillers (this.abductorKing, this.abductorKing.name, LOG_IDENTIFIER.KING_1);
			newLog.AddToFillers (this.abductorKingdom, this.abductorKingdom.name, LOG_IDENTIFIER.KINGDOM_1);

			SpouseReplaceKing(king);
		}else{
			//Add log - spouse has been executed for attempting to assassinate king*
			Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "abductor_king_assassination_fail");
			newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			newLog.AddToFillers (this.abductorKing, this.abductorKing.name, LOG_IDENTIFIER.KING_1);

			SpouseDies();
		}
	}
	private void SpouseDies(){
		if(!this.abductee.isDead){
			this.abductee.Death (DEATH_REASONS.TREACHERY);
		}
		this.isSpouseDead = true;
	}
	private void SpouseReplaceKing(Citizen king){
		king.Death (DEATH_REASONS.ASSASSINATION, true, this.abductee);
		SpouseReplacedKingResults (king);
	}

	private void SpouseReplacedKingResults(Citizen king){
		if(king.id == this.abductorKing.id){
			if(!this.isCompatibleWithTargetKing){
				//Declare war against target kingdom
				//Add log - declared war against target kingdom because of marriage incompatibility*
				Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "spouse_replace_war");
				newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
				newLog.AddToFillers (this.targetKing, this.targetKing.name, LOG_IDENTIFIER.KING_2);

				KingdomManager.Instance.InstantWarBetweenKingdoms(this.abductorKingdom, this.targetKingdom, WAR_TRIGGER.MARRIAGE_INCOMPATIBILITY);
			}else{
				//Assimilate Kingdom?
			}
		}else if(king.id == this.targetKing.id){
			if (!this.isCompatibleWithAbductorKing) {
				//Declare war against abductor kingdom
				//Add log - declared war against abductor kingdom because of marriage incompatibility
				KingdomManager.Instance.InstantWarBetweenKingdoms(this.targetKingdom, this.abductorKingdom, WAR_TRIGGER.MARRIAGE_INCOMPATIBILITY);
			}else{
				//Assimilate Kingdom?
			}
		}
		this.DoneEvent ();
	}

	private void AdjustRelationshipFromOtherKingdoms(Citizen targetKing, int amount, CHARACTER_VALUE requiredValue){
		for(int i = 0; i < this.otherKingdoms.Count; i++){
			if(this.otherKingdoms[i].king.importantCharacterValues.ContainsKey(requiredValue)){
				RelationshipKings relationship = this.otherKingdoms [i].king.GetRelationshipWithCitizen (targetKing);
				if(relationship != null){
					relationship.AdjustLikeness (amount, this, ASSASSINATION_TRIGGER_REASONS.OPPOSING_APPROACH);
				}
			}
		}
	}

	internal void ReturnSpouseToOriginal(){
		if(this.isCompatibleWithTargetKing){
			ReturnSpouse ();
		}else{
			SpouseSuicide ();
		}
	}

	private void SpouseSuicide(){
		//Add log - spouse committed suicide because of marriage incompatibility*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "spouse_suicide");
		newLog.AddToFillers (this.targetKingdom, this.targetKingdom.name, LOG_IDENTIFIER.KINGDOM_2);
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);

		if(!this.abductee.isDead){
			this.abductee.Death (DEATH_REASONS.SUICIDE);
		}
		this.isSpouseDead = true;
	}

	internal void WarWinner (Kingdom winnerKingdom){
		if(winnerKingdom.id == this.abductorKingdom.id){
			AbductorKingdomWins ();
		}else if(winnerKingdom.id == this.targetKingdom.id){
			TargetKingdomWins ();
		}
	}

	private void AbductorKingdomWins(){
		//Add log - abductor wins, the spouse will not be returned*
		Log newLog = this.CreateNewLogForEvent (GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year, "Events", "SpouseAbduction", "abductor_wins");
		newLog.AddToFillers (this.abductorKingdom, this.abductorKingdom.name, LOG_IDENTIFIER.KINGDOM_1);
		newLog.AddToFillers (this.abductee, this.abductee.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		newLog.AddToFillers (this.abductorKing, this.abductorKing.name, LOG_IDENTIFIER.KING_1);

		this.DoneEvent();
	}

	private void TargetKingdomWins(){
		if(!this.isSpouseDead){
			ReturnSpouseToOriginal ();
		}
		this.DoneEvent ();
	}

	private void InstantWar(){
		KingdomManager.Instance.InstantWarBetweenKingdoms (this.targetKingdom, this.abductorKingdom, this._warTrigger, this);
	}
}
