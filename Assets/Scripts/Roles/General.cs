using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class General : Role {
	internal int soldiers;
	internal bool isReturning;
	internal bool isAttacking;
	internal bool isDefending;
	internal bool willDropSoldiersAndDisappear;

	internal GeneralTask generalTask;

    public General(Citizen citizen): base(citizen){
		this.role = ROLE.GENERAL;
		this.soldiers = 0;
		this.isReturning = false;
		this.isAttacking = false;
		this.isDefending = false;
		this.willDropSoldiersAndDisappear = false;
		this.generalTask = null;
	}

	#region Override
	internal override void Initialize(GameEvent gameEvent){
		base.Initialize(gameEvent);
		this.avatar.GetComponent<GeneralAvatar>().Init(this);
	}
	internal override void CreateAvatarGO (){
		this.avatar = (GameObject)GameObject.Instantiate (ObjectPoolManager.Instance.citizenAvatarPrefabs [1], this.citizen.assignedRole.location.transform);
		this.avatar.transform.localPosition = Vector3.zero;
	}
	internal override void DestroyGO(){
		this.location.ExitCitizen(this.citizen);
		if (this.avatar != null){
			UIManager.Instance.HideSmallInfo ();
			GameObject.Destroy (this.avatar);
			this.avatar = null;
		}
		this.isDestroyed = true;
	}
	internal override void ArrivedAtTargetLocation (){
		if(this.generalTask != null){
			this.generalTask.Arrived ();
		}
	}
	#endregion

	internal void SetSoldiers(int amount){
		this.soldiers = amount;
		this.citizenAvatar.UpdateUI();
	}
	internal void AdjustSoldiers(int amount){
		this.soldiers += amount;
		if(this.soldiers < 0){
			this.soldiers = 0;
		}
		this.citizenAvatar.UpdateUI();
	}
	internal void WillDropSoldiersAndDisappear(){
		this.willDropSoldiersAndDisappear = true;
	}
	internal void DropSoldiersAndDisappear(){
		if(this.location.city != null && this.location.city.kingdom.id == this.citizen.city.kingdom.id){
			this.location.city.AdjustSoldiers (this.soldiers);
			this.gameEventInvolvedIn.DoneEvent ();
		}else{
			this.gameEventInvolvedIn.DoneEvent ();
		}
	}
	internal void DropSoldiers(){
		if(this.location.city != null && this.location.city.kingdom.id == this.citizen.city.kingdom.id){
			this.location.city.AdjustSoldiers (this.soldiers);
		}
	}

	internal void GetTask(){
		if(this.citizen.isDead){
			return;
		}
		if(this.generalTask != null){
			this.generalTask.DoneTask();
		}
		this.citizen.city.kingdom.militaryManager.AssignTaskToGeneral (this);
	}
	internal void AssignTask(GeneralTask generalTask){
		this.generalTask = generalTask;
		this.generalTask.AssignMoveDate ();
		if (this.generalTask.task == GENERAL_TASKS.ATTACK_CITY) {
			this.targetCity = ((AttackCityTask)generalTask).targetCity;
			this.targetLocation = generalTask.targetHextile;
			this.isIdle = true;
			this.citizenAvatar.SetHasArrivedState (false);
			this.citizenAvatar.CreatePath (PATHFINDING_MODE.MAJOR_ROADS_WITH_ALLIES);
			GetSoldiersFromCities ();
		} else if (this.generalTask.task == GENERAL_TASKS.DEFEND_CITY) {
			this.targetCity = ((DefendCityTask)generalTask).targetCity;
			this.targetCity.assignedDefendGeneralsCount += 1;
			this.targetLocation = generalTask.targetHextile;
			this.isIdle = true;
			this.citizenAvatar.SetHasArrivedState (false);
			this.citizenAvatar.CreatePath (PATHFINDING_MODE.MAJOR_ROADS_WITH_ALLIES);
			GetSoldiersFromCities ();
		} else {
			this.avatar.GetComponent<GeneralAvatar> ().ChangeAvatarImage (this.generalTask.task);
		}
	}
	private void GetSoldiersFromCities(){
		int neededSoldiers = NeededSoldiers ();
		for (int i = 0; i < this.citizen.city.kingdom.cities.Count; i++) {
			if(neededSoldiers > 0){
				City city = this.citizen.city.kingdom.cities [i];
				List<HexTile> path = PathGenerator.Instance.GetPath (city.hexTile, this.location, PATHFINDING_MODE.USE_ROADS_WITH_ALLIES, this.citizen.city.kingdom);
				if(path != null && path.Count > 0){
					if (path.Count + 2 <= this.generalTask.daysBeforeMoving){
						int soldiersGiven = AskForSoldiers (neededSoldiers, city, path);
						neededSoldiers -= soldiersGiven;
					}
				}
			}else{
				break;
			}
		}
	}
	private int AskForSoldiers(int neededSoldiers, City city, List<HexTile> path){
		int numOfSoldiersCanBeGiven = city.GetNumOfSoldiersCanBeGiven ();
		if(numOfSoldiersCanBeGiven > neededSoldiers){
			numOfSoldiersCanBeGiven = neededSoldiers;
		}
		if(numOfSoldiersCanBeGiven > 0){
			city.SendReinforcementsToGeneral (this, numOfSoldiersCanBeGiven, path);
		}
		return numOfSoldiersCanBeGiven;
	}
	private int NeededSoldiers(){
		if(this.generalTask.task == GENERAL_TASKS.ATTACK_CITY){
			return this.citizen.city.kingdom.soldiersCount / this.citizen.city.kingdom.militaryManager.maxGenerals / this.citizen.city.kingdom.warfareInfo.Count;
		}else if(this.generalTask.task == GENERAL_TASKS.DEFEND_CITY){
			int numOfWars = (this.citizen.city.kingdom.warfareInfo.Count > 0) ? this.citizen.city.kingdom.warfareInfo.Count : 1;
			return this.citizen.city.kingdom.soldiersCount / this.citizen.city.kingdom.militaryManager.maxGenerals / numOfWars;
		}
		return 0;
	}

	internal void Death(DEATH_REASONS reason, bool isConquered = false){
		if(!this.citizen.city.kingdom.isDead){
			this.citizen.city.kingdom.militaryManager.activeGenerals.Remove (this);
		}
		if(this.generalTask != null){
			this.generalTask.DoneTask ();
		}
		this.citizen.Death (reason, isConquered);
	}

	internal int GetPower(){
		if (this.isDefending) {
			return (int)((this.soldiers + this.location.city.soldiers) * 4) + (int)(Mathf.Sqrt((float)this.location.city.population));
		}else{
			return this.soldiers * 3;
		}
	}

	internal void ChangeBattleState(){
		if (this.location.city != null && this.location.city.kingdom.id == this.citizen.city.kingdom.id) {
			this.isDefending = true;
			this.isAttacking = false;
		}else{
			this.isDefending = false;
			this.isAttacking = true;
		}
	}
}
