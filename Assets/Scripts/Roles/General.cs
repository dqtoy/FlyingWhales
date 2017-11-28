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
		this.isReturning = false;
		this.isAttacking = false;
		this.isDefending = false;
		this.willDropSoldiersAndDisappear = false;
		this.generalTask = null;
	}

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
	internal void SetSoldiers(int amount){
		this.soldiers = amount;
		this.avatar.GetComponent<GeneralAvatar>().UpdateUI();
	}
	internal void AdjustSoldiers(int amount){
		this.soldiers += amount;
		if(this.soldiers < 0){
			this.soldiers = 0;
		}
		this.avatar.GetComponent<GeneralAvatar>().UpdateUI();
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

	internal void AssignTask(GeneralTask generalTask){
		this.generalTask = generalTask;

	}
	private void GetSoldiersFromCities(){
		int neededSoldiers = NeededSoldiers ();
		for (int i = 0; i < this.citizen.city.kingdom.cities.Count; i++) {
			City city = this.citizen.city.kingdom.cities [i];
			List<HexTile> path = PathGenerator.Instance.GetPath (city.hexTile, this.location, PATHFINDING_MODE.USE_ROADS_ONLY_KINGDOM, this.citizen.city.kingdom);
			if(path != null){
				
			}
		}
	}
	private void AskForSoldiers(int neededSoldiers){
		
	}
	private int NeededSoldiers(){
		if(this.generalTask.task == GENERAL_TASKS.ATTACK_CITY){
			return this.citizen.city.kingdom.soldiersCount / this.citizen.city.kingdom.militaryManager.activeGenerals.Count / this.citizen.city.kingdom.warfareInfo.Count;
		}else if(this.generalTask.task == GENERAL_TASKS.DEFEND_CITY){
			return this.citizen.city.kingdom.soldiersCount / this.citizen.city.kingdom.militaryManager.activeGenerals.Count / this.citizen.city.kingdom.warfareInfo.Count;
		}
		return 0;
	}
}
