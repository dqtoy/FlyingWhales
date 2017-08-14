using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;
using System.Linq;

public class RangerAvatar : CitizenAvatar {
	public TextMesh txtDamage;
	public SpriteRenderer kingdomIndicator;

    #region Overrides
	internal override void Init (Role citizenRole){
		base.Init (citizenRole);
		this.kingdomIndicator.color = this.citizenRole.citizen.city.kingdom.kingdomColor;
	}
    internal override void Move() {
        if (this.citizenRole.targetLocation != null) {
            if (this.citizenRole.path != null) {
                if (this.citizenRole.path.Count > 0) {
                    this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
                    //this.transform.position = new Vector2(this.citizenRole.path[0].transform.position.x, this.citizenRole.path[0].transform.position.y);
					((Ranger)this.citizenRole).prevLocation = this.citizenRole.location;
                    this.citizenRole.location = this.citizenRole.path[0];
                    this.citizenRole.citizen.currentLocation = this.citizenRole.path[0];
                    this.citizenRole.path.RemoveAt(0);
                    this.CheckForKingdomDiscovery();
					((Ranger)this.citizenRole).AcquireTarget ();
					this.UpdateFogOfWar();
                    //this.GetNextTargetTile();
                }
            }
        }
    }
	public override void UpdateFogOfWar(bool forDeath = false) {
        Kingdom kingdomOfAgent = this.citizenRole.citizen.homeKingdom;
        for (int i = 0; i < visibleTiles.Count; i++) {
			HexTile currTile = visibleTiles[i];
            //this.citizenRole.citizen.homeKingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
            if (currTile.isBorder) {
                if (currTile.isBorderOfCities.Intersect(kingdomOfAgent.cities).Count() <= 0) {
                    kingdomOfAgent.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
                }
            } else if (currTile.isOuterTileOfCities.Count > 0) {
                if (currTile.isOuterTileOfCities.Intersect(kingdomOfAgent.cities).Count() <= 0) {
                    kingdomOfAgent.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
                }
            } else if (currTile.isOccupied) {
                if (currTile.ownedByCity == null || currTile.ownedByCity.kingdom.id != kingdomOfAgent.id) {
                    kingdomOfAgent.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
                }
            } else {
                kingdomOfAgent.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.SEEN);
            }
        }
		visibleTiles.Clear();
		if (!forDeath) {
			visibleTiles.Add(this.citizenRole.location);
			this.citizenRole.citizen.homeKingdom.SetFogOfWarStateForTile(this.citizenRole.location, FOG_OF_WAR_STATE.VISIBLE);

			HexTile[] neighbors = this.citizenRole.location.AllNeighbours.ToArray ();
			for (int i = 0; i < neighbors.Length; i++) {
				HexTile currTile = neighbors[i];
				if (this.citizenRole.citizen.homeKingdom.GetFogOfWarStateOfTile(currTile) != FOG_OF_WAR_STATE.HIDDEN) {
					visibleTiles.Add (currTile);
					this.citizenRole.citizen.homeKingdom.SetFogOfWarStateForTile(currTile, FOG_OF_WAR_STATE.VISIBLE);
				}

			}
		}
	}
    #endregion

    #region BehaviourTree Tasks
	[Task]
	public void IsThereCitizen() {
		if (this.citizenRole.citizen != null) {
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}
	}
	[Task]
	public void IsThereEvent() {
		if (this.citizenRole.gameEventInvolvedIn != null) {
			Task.current.Succeed();
		} else {
			Task.current.Fail();
		}
	}

	[Task]
	public void HasArrivedAtTargetHextile() {
		if (this.citizenRole.location == this.citizenRole.targetLocation) {
			if (this.citizenRole.location.lair != null) {
				if (!this.hasArrived) {
					SetHasArrivedState (true);
					this.citizenRole.Attack ();
				}
				Task.current.Succeed ();
			}else{
				if(this.citizenRole is Ranger){
					((Ranger)this.citizenRole).AcquireTarget ();
					Task.current.Fail();
				}
			}
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
	public void MoveToNextTile() {
		Move();
		Task.current.Succeed();
	}
    #endregion

	internal override void UpdateUI (){
		if(this.txtDamage.gameObject != null){
			this.txtDamage.text = this.citizenRole.damage.ToString ();
		}
	}
}
