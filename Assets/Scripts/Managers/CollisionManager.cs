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
		if(general1 != null && general2 != null){
			if(general1.citizen.city.kingdom.id != general2.citizen.city.kingdom.id){
				Combat (general1, general2);
			}else{
				if(general1.generalTask != null && general1.generalTask.task == GENERAL_TASKS.REINFORCE_CITY){
					Reinforcement (general1, general2);
				}else if(general2.generalTask != null && general2.generalTask.task == GENERAL_TASKS.REINFORCE_CITY){
					Reinforcement (general2, general1);
				}
			}
		}
	}

	#region Combat
	private void Combat(General general1, General general2){
		KingdomRelationship kr = general1.citizen.city.kingdom.GetRelationshipWithKingdom (general2.citizen.city.kingdom);
		if(kr.sharedRelationship.isAtWar && kr.sharedRelationship.warfare != null){
			Debug.Log ("=============== ENTERING COMBAT BETWEEN " + general1.citizen.city.name + " of " + general1.citizen.city.kingdom.name + " AND " + general2.citizen.city.name + " of " + general2.citizen.city.kingdom.name + " " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ===============");
			kr.sharedRelationship.warfare.AdjustWeariness (general1.citizen.city.kingdom, 2);

			int general1Power = general1.GetPower();
			int general2Power = general2.GetPower();

			general1.ChangeBattleState ();
			general2.ChangeBattleState ();

			Debug.Log ("GENERAL 1 POWER: " + general1Power);	
			Debug.Log ("GENERAL 2 POWER: " + general2Power);
			Debug.Log ("---------------------------");

			int general1MaxRoll = (int)(Mathf.Sqrt ((2000f * (float)general1Power)));
			int general2MaxRoll = (int)(Mathf.Sqrt ((2000f * (float)general2Power)));

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
				if(!general1.citizen.city.kingdom.isDead && !general2.citizen.city.kingdom.isDead){
					if(general1.isDefending){
						kr.sharedRelationship.warfare.AdjustWeariness (general1.citizen.city.kingdom, 1);
					}
					kr.sharedRelationship.warfare.AdjustWeariness (general2.citizen.city.kingdom, 5);
					//kr.sharedRelationship.warfare.AttemptToPeace (general1.citizen.city.kingdom);
				}
			}else if(general2Roll > general1Roll){
				DamageComputation (general2, general2Roll, general1, general1Roll);
				if(!general1.citizen.city.kingdom.isDead && !general2.citizen.city.kingdom.isDead){
					if(general2.isDefending){
						kr.sharedRelationship.warfare.AdjustWeariness (general2.citizen.city.kingdom, 1);
					}
					kr.sharedRelationship.warfare.AdjustWeariness (general1.citizen.city.kingdom, 5);
					//kr.sharedRelationship.warfare.AttemptToPeace (general2.citizen.city.kingdom);
				}
			}else{
				if(!general1.isDefending && !general2.isDefending){
					general1.AdjustSoldiers (-general1.soldiers);
					if(general1.gameEventInvolvedIn != null){
						general1.gameEventInvolvedIn.DoneEvent ();
					}
					general2.AdjustSoldiers (-general2.soldiers);
					if (general2.gameEventInvolvedIn != null) {
						general2.gameEventInvolvedIn.DoneEvent ();
					}
					if(!general1.citizen.city.kingdom.isDead && !general2.citizen.city.kingdom.isDead){
						kr.sharedRelationship.warfare.AdjustWeariness (general1.citizen.city.kingdom, 5);
						kr.sharedRelationship.warfare.AdjustWeariness (general2.citizen.city.kingdom, 5);
					}
				}else{
					if(general1.isDefending){
						DamageComputation (general1, general1Roll, general2, general2Roll);
						if(!general1.citizen.city.kingdom.isDead && !general2.citizen.city.kingdom.isDead){
							kr.sharedRelationship.warfare.AdjustWeariness (general2.citizen.city.kingdom, 5);
							//kr.sharedRelationship.warfare.AttemptToPeace (general1.citizen.city.kingdom);
						}
//						if(kr.sharedRelationship.battle != null){
//							kr.sharedRelationship.battle.BattleEnd (general1, general2);
//						}
					}else if(general2.isDefending){
						DamageComputation (general2, general2Roll, general1, general1Roll);
						if(!general1.citizen.city.kingdom.isDead && !general2.citizen.city.kingdom.isDead){
							kr.sharedRelationship.warfare.AdjustWeariness (general1.citizen.city.kingdom, 5);
							//kr.sharedRelationship.warfare.AttemptToPeace (general2.citizen.city.kingdom);
						}
//						if(kr.sharedRelationship.battle != null){
//							kr.sharedRelationship.battle.BattleEnd (general2, general1);
//						}
					}
				}
			}
		}
	}
	internal void Combat(General general1, City city){
		KingdomRelationship kr = general1.citizen.city.kingdom.GetRelationshipWithKingdom (city.kingdom);
		if(kr.sharedRelationship.isAtWar && kr.sharedRelationship.warfare != null){
			Debug.Log ("=============== ENTERING COMBAT BETWEEN " + general1.citizen.city.name + " of " + general1.citizen.city.kingdom.name + " AND " + city.name + " of " + city.kingdom.name + " " + GameManager.Instance.month.ToString() + "/" + GameManager.Instance.days.ToString() + "/" + GameManager.Instance.year.ToString() + " ===============");
			kr.sharedRelationship.warfare.AdjustWeariness (general1.citizen.city.kingdom, 2);

			int general1Power = general1.soldiers * 3;
			int cityPower = (int)((city.soldiers * 4) + (int)(city.population / 2));

			if(general1.location.city != null && general1.location.city.kingdom.id == general1.citizen.city.kingdom.id){
				general1Power = (int)((general1.soldiers + general1.location.city.soldiers) * 4) + (int)(general1.location.city.population / 2);
				general1.isDefending = true;
				general1.isAttacking = false;
			}else{
				general1.isDefending = false;
				general1.isAttacking = true;
			}


			Debug.Log ("GENERAL 1 POWER: " + general1Power);	
			Debug.Log ("CITY POWER: " + cityPower);
			Debug.Log ("---------------------------");

			int general1MaxRoll = (int)(Mathf.Sqrt ((2000f * (float)general1Power)) * (1f + (0.05f * (float)general1.citizen.city.cityLevel)));
			int cityMaxRoll = (int)(Mathf.Sqrt ((2000f * (float)cityPower)) * (1f + (0.05f * (float)city.cityLevel)));

			Debug.Log ("GENERAL 1 MAX ROLL: " + general1MaxRoll);	
			Debug.Log ("CITY MAX ROLL: " + cityMaxRoll);
			Debug.Log ("---------------------------");

			int general1Roll = UnityEngine.Random.Range (0, general1MaxRoll);
			int cityRoll = UnityEngine.Random.Range (0, cityMaxRoll);

			Debug.Log ("GENERAL 1 ROLL: " + general1Roll);	
			Debug.Log ("CITY ROLL: " + cityRoll);
			Debug.Log ("---------------------------");


			if(general1Roll > cityRoll){
				DamageComputationLoserCity (general1, general1Roll, city, cityRoll);
				//if(!general1.citizen.city.kingdom.isDead && !city.kingdom.isDead){
				//	kr.sharedRelationship.warfare.AttemptToPeace (general1.citizen.city.kingdom);
				//}
			}else{
				DamageComputationWinnerCity (general1, general1Roll, city, cityRoll);
				//if(!general1.citizen.city.kingdom.isDead && !city.kingdom.isDead){
				//	kr.sharedRelationship.warfare.AttemptToPeace (city.kingdom);
				//}
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
		loserGeneral.Death (DEATH_REASONS.BATTLE);
		if(winnerGeneral.soldiers <= 0){
			winnerGeneral.Death (DEATH_REASONS.BATTLE);
		}
		if(loserGeneral.isDefending){
			winnerGeneral.citizen.city.kingdom.ConquerCity (loserGeneral.location.city);
		}
	}
	private void DamageComputationLoserCity(General general, int generalRoll, City city, int cityRoll){
		float winnerPowerSquared = Mathf.Pow ((float)generalRoll, 2f);
		float loserPowerSquared = Mathf.Pow ((float)cityRoll, 2f);

		float deathPercentage = (loserPowerSquared / winnerPowerSquared) * 100f;
		Debug.Log ("DEATH PERCENTAGE: " + deathPercentage.ToString ());
		int deathCount = 0;
		if(deathPercentage >= 100f){
			deathCount = general.soldiers;
		}else{
			for (int i = 0; i < general.soldiers; i++) {
				if(UnityEngine.Random.Range(0f, 99f) < deathPercentage){
					deathCount += 1;
				}
			}
		}
		Debug.Log ("WINNER SOLDIERS: " + general.soldiers.ToString ());
		Debug.Log ("DEATH COUNT: " + deathCount.ToString ());
		general.AdjustSoldiers (-deathCount);
		Debug.Log ("CURRENT WINNER SOLDIERS: " + general.soldiers.ToString ());
		city.AdjustSoldiers (-city.soldiers);
		if(general.soldiers <= 0){
			general.Death (DEATH_REASONS.BATTLE);
		}
		general.citizen.city.kingdom.ConquerCity (city);
	}
	private void DamageComputationWinnerCity(General general, int generalRoll, City city, int cityRoll){
		float winnerPowerSquared = Mathf.Pow ((float)cityRoll, 2f);
		float loserPowerSquared = Mathf.Pow ((float)generalRoll, 2f);

		float deathPercentage = (loserPowerSquared / winnerPowerSquared) * 100f;
		Debug.Log ("DEATH PERCENTAGE: " + deathPercentage.ToString ());
		int deathCount = 0;
		if(deathPercentage >= 100f){
			deathCount = city.soldiers;
		}else{
			for (int i = 0; i < city.soldiers; i++) {
				if(UnityEngine.Random.Range(0f, 99f) < deathPercentage){
					deathCount += 1;
				}
			}
		}
		Debug.Log ("WINNER SOLDIERS: " + city.soldiers.ToString ());
		Debug.Log ("DEATH COUNT: " + deathCount.ToString ());
		city.AdjustSoldiers (-deathCount);
		Debug.Log ("CURRENT WINNER SOLDIERS: " + city.soldiers.ToString ());
		general.AdjustSoldiers (-general.soldiers);
		general.Death (DEATH_REASONS.BATTLE);
	}
	#endregion

	#region Reinforcement
	private void Reinforcement(General reinforceGeneral, General general2){
		if(general2.generalTask != null && (general2.generalTask.task == GENERAL_TASKS.ATTACK_CITY || general2.generalTask.task == GENERAL_TASKS.DEFEND_CITY)){
			if(reinforceGeneral.generalTask is ReinforceCityTask){
				ReinforceCityTask task = (ReinforceCityTask)reinforceGeneral.generalTask;
				if(task.mainGeneral.citizen.id == general2.citizen.id){
					general2.AdjustSoldiers (reinforceGeneral.soldiers);
					reinforceGeneral.citizen.Death (DEATH_REASONS.ACCIDENT);
				}
			}
		}
	}
	#endregion
}
