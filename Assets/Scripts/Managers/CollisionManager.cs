using UnityEngine;
using System.Collections;

public class CollisionManager : MonoBehaviour {
	public static CollisionManager Instance;

	void Awake(){
		Instance = this;
	}

	internal void HasCollided(Role source, Role target){
		ROLE sourceRole = source.citizen.role;
		ROLE targetRole = target.citizen.role;

		 
		if(sourceRole == ROLE.GENERAL && targetRole == ROLE.GENERAL){
			GeneralToGeneral ((General)source.citizen.assignedRole, (General)target.citizen.assignedRole);
		}
	}

	private void GeneralToGeneral(General general1, General general2){
		if(general1.citizen.city.kingdom.id != general2.citizen.city.kingdom.id){
			Combat (general1, general2);
		}
	}

	#region Combat
	private void Combat(General general1, General general2){
		KingdomRelationship kr = general1.citizen.city.kingdom.GetRelationshipWithKingdom (general2.citizen.city.kingdom);
		if(kr.isAtWar && kr.warfare != null){
			Debug.Log ("=============== ENTERING COMBAT BETWEEN " + general1.citizen.city.name + " of " + general1.citizen.city.kingdom.name + " AND " + general2.citizen.city.name + " of " + general2.citizen.city.kingdom.name + " " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ===============");
			kr.warfare.AdjustWeariness (general1.citizen.city.kingdom, 2);

			int general1Power = general1.soldiers;
			int general2Power = general2.soldiers;

			if(general1.location.city != null && general1.location.city.kingdom.id == general1.citizen.city.kingdom.id){
				general1Power = (int)(general1Power * 1.5f);
				general1.isDefending = true;
				general1.isAttacking = false;
			}else{
				general1.isDefending = false;
				general1.isAttacking = true;
			}
			if(general2.location.city != null && general2.location.city.kingdom.id == general2.citizen.city.kingdom.id){
				general2Power = (int)(general2Power * 1.5f);
				general2.isDefending = true;
				general2.isAttacking = false;
			}
			else{
				general2.isDefending = false;
				general2.isAttacking = true;
			}

			Debug.Log ("GENERAL 1 POWER: " + general1Power);	
			Debug.Log ("GENERAL 2 POWER: " + general2Power);
			Debug.Log ("---------------------------");

			int general1MaxRoll = (int)(Mathf.Sqrt ((2000f * (float)general1Power)) * (1f + (0.05f * (float)general1.citizen.city.cityLevel)));
			int general2MaxRoll = (int)(Mathf.Sqrt ((2000f * (float)general2Power)) * (1f + (0.05f * (float)general2.citizen.city.cityLevel)));

			Debug.Log ("GENERAL 1 MAX ROLL: " + general1MaxRoll);	
			Debug.Log ("GENERAL 2 MAX ROLL: " + general2MaxRoll);
			Debug.Log ("---------------------------");

			int general1Roll = UnityEngine.Random.Range (0, general1MaxRoll);
			int general2Roll = UnityEngine.Random.Range (0, general2MaxRoll);

			Debug.Log ("GENERAL 1 ROLL: " + general1Roll);	
			Debug.Log ("GENERAL 2 ROLL: " + general2Roll);
			Debug.Log ("---------------------------");


			if(general1Roll > general2Roll){
				DamageComputation (general1, general1Roll, general2, general2Roll);
				if(kr.battle != null){
					kr.battle.BattleEnd (general1, general2);
				}
			}else if(general2Roll > general1Roll){
				DamageComputation (general2, general2Roll, general1, general1Roll);
				if(kr.battle != null){
					kr.battle.BattleEnd (general2, general1);
				}
			}else{
				if(!general1.isDefending && !general2.isDefending){
					general1.AdjustSoldiers (-general1.soldiers);
					general1.gameEventInvolvedIn.DoneEvent ();
					general2.AdjustSoldiers (-general2.soldiers);
					general2.gameEventInvolvedIn.DoneEvent ();
				}else{
					if(general1.isDefending){
						DamageComputation (general1, general1Roll, general2, general2Roll);
					}else if(general2.isDefending){
						DamageComputation (general2, general2Roll, general1, general1Roll);
					}
				}
			}
		}
	}
	private void DamageComputation(General winnerGeneral, int winnerGeneralRoll, General loserGeneral, int loserGeneralRoll){
		float winnerPowerSquared = Mathf.Pow ((float)winnerGeneralRoll, 2f);
		float loserPowerSquared = Mathf.Pow ((float)loserGeneralRoll, 2f);

		float deathPercentage = (loserPowerSquared / winnerPowerSquared) * 100f;
		Debug.Log ("DEATH PERCENTAGE: " + deathPercentage.ToString ());
		int deathCount = 0;
		if(deathPercentage >= 100f){
			deathCount = winnerGeneral.soldiers;
		}else{
			for (int i = 0; i < winnerGeneral.soldiers; i++) {
				if(UnityEngine.Random.Range(0f, 99f) < deathPercentage){
					deathCount += 1;
				}
			}
		}
		Debug.Log ("WINNER SOLDIERS: " + winnerGeneral.soldiers.ToString ());
		Debug.Log ("DEATH COUNT: " + deathCount.ToString ());
		winnerGeneral.AdjustSoldiers (-deathCount);
		Debug.Log ("CURRENT WINNER SOLDIERS: " + winnerGeneral.soldiers.ToString ());
		loserGeneral.AdjustSoldiers (-loserGeneral.soldiers);
		loserGeneral.gameEventInvolvedIn.DoneEvent ();
		if(winnerGeneral.soldiers <= 0){
			winnerGeneral.gameEventInvolvedIn.DoneEvent ();
		}
	}
	#endregion
}
