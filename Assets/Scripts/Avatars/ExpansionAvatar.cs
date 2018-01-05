using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Panda;

public class ExpansionAvatar : CitizenAvatar {

	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
	}

	internal override void NewMove (){
		if (this.citizenRole.targetLocation != null) {
			if(this.citizenRole.targetLocation.isOccupied){
				HexTile hexTileToExpandTo = CityGenerator.Instance.GetExpandableTileForKingdom(this.citizenRole.citizen.city.kingdom, false);
				if(hexTileToExpandTo != null){
					this.citizenRole.targetLocation = hexTileToExpandTo;
					((Expansion)this.citizenRole.gameEventInvolvedIn).hexTileToExpandTo = hexTileToExpandTo;
					CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
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
//					Debug.LogError (this.citizenRole.role.ToString() + " " + this.citizenName + " START DAY: " + GameManager.Instance.days);
				}
			}
		}
	}
	internal override void HasArrivedAtTargetLocation (){
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if(this.citizenRole.targetLocation.isOccupied){
				HexTile hexTileToExpandTo = CityGenerator.Instance.GetExpandableTileForKingdom(this.citizenRole.citizen.city.kingdom, false);
				if(hexTileToExpandTo != null){
					this.citizenRole.targetLocation = hexTileToExpandTo;
					((Expansion)this.citizenRole.gameEventInvolvedIn).hexTileToExpandTo = hexTileToExpandTo;
					CreatePath (PATHFINDING_MODE.MAJOR_ROADS_ONLY_KINGDOM);
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
}
