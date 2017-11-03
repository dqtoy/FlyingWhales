using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class ExpansionAvatar : CitizenAvatar {

	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		CreatePath (PATHFINDING_MODE.AVATAR);
	}

	internal override void NewMove (){
		if (this.citizenRole.targetLocation != null) {
			if(this.citizenRole.targetLocation.isOccupied){
				HexTile hexTileToExpandTo = CityGenerator.Instance.GetExpandableTileForKingdom(this.citizenRole.citizen.city.kingdom);
				if(hexTileToExpandTo != null){
					this.citizenRole.targetLocation = hexTileToExpandTo;
					((Expansion)this.citizenRole.gameEventInvolvedIn).hexTileToExpandTo = hexTileToExpandTo;
					CreatePath (PATHFINDING_MODE.USE_ROADS);
					return;
				}else{
					CancelEventInvolvedIn ();
					return;
				}
			}
			if (this.citizenRole.path != null) {
				if (this.citizenRole.path.Count > 0) {
					this.citizenRole.location.ExitCitizen (this.citizenRole.citizen);
					this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
				}
			}
		}
	}
	internal override void HasArrivedAtTargetLocation (){
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if(this.citizenRole.targetLocation.isOccupied){
				HexTile hexTileToExpandTo = CityGenerator.Instance.GetExpandableTileForKingdom(this.citizenRole.citizen.city.kingdom);
				if(hexTileToExpandTo != null){
					this.citizenRole.targetLocation = hexTileToExpandTo;
					((Expansion)this.citizenRole.gameEventInvolvedIn).hexTileToExpandTo = hexTileToExpandTo;
					CreatePath (PATHFINDING_MODE.AVATAR);
					return;
				}else{
					CancelEventInvolvedIn ();
					return;
				}
			}
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				EndAttack ();
			}
		}
	}
	#region BehaviourTree Tasks
//	[Task]
//	public void IsThereCitizen() {
//		if (this.citizenRole.citizen != null) {
//			Task.current.Succeed();
//		} else {
//			Task.current.Fail();
//		}
//	}
//	[Task]
//	public void IsThereEvent() {
//		if (this.citizenRole.gameEventInvolvedIn != null) {
//			Task.current.Succeed();
//		} else {
//			Task.current.Fail();
//		}
//	}
	[Task]
	public void HasArrivedAtTargetHextile() {
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if (!this.hasArrived) {
				SetHasArrivedState(true);
				EndAttack ();
			}
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}

	}
	[Task]
	public void HasDiedOfOtherReasons() {
		if (this.citizenRole.citizen.isDead) {
			//Citizen has died
			this.citizenRole.gameEventInvolvedIn.DeathByOtherReasons();
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}
	}
	[Task]
	public void CheckTargetLocation() {
		if(this.citizenRole.targetLocation != null && this.citizenRole.targetLocation.isOccupied){
			HexTile hexTileToExpandTo = CityGenerator.Instance.GetExpandableTileForKingdom(this.citizenRole.citizen.city.kingdom);
			if(hexTileToExpandTo != null){
				List<HexTile> newPath = PathGenerator.Instance.GetPath (this.citizenRole.location, hexTileToExpandTo, PATHFINDING_MODE.AVATAR);
				if(newPath != null){
					this.citizenRole.path = newPath;
					this.citizenRole.targetLocation = hexTileToExpandTo;
					((Expansion)this.citizenRole.gameEventInvolvedIn).hexTileToExpandTo = hexTileToExpandTo;
					Task.current.Fail();
				}else{
					CancelEventInvolvedIn ();
					Task.current.Succeed();
				}
			}else{
				CancelEventInvolvedIn ();
				Task.current.Succeed();

			}
		}else{
			Task.current.Fail();
		}
	}
	[Task]
	public void MoveToNextTile() {
		Move();
		Task.current.Succeed();
	}
//	[Task]
//	public void HasDisappeared(){
//		if (!this.citizenRole.location.isOccupied) {
//			float chance = UnityEngine.Random.Range (0f, 99f);
//			if(chance <= 0.5f){
//				//Disappearance
//				((Expansion)this.citizenRole.gameEventInvolvedIn).Disappearance ();
//				Task.current.Succeed ();
//			}else{
//				Task.current.Fail ();
//			}
//		}else{
//			Task.current.Fail ();
//		}
//	}
	#endregion
}
