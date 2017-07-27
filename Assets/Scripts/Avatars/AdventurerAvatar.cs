using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;
using System.Linq;

public class AdventurerAvatar : CitizenAvatar {

    #region Overrides
    internal override void Move() {
        if (this.citizenRole.targetLocation != null) {
            if (this.citizenRole.path != null) {
                if (this.citizenRole.path.Count > 0) {
                    this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
                    this.citizenRole.location = this.citizenRole.path[0];
                    this.citizenRole.citizen.currentLocation = this.citizenRole.path[0];
                    this.citizenRole.path.RemoveAt(0);
                    this.citizenRole.location.CollectEventOnTile(this.citizenRole.citizen.city.kingdom, this.citizenRole.citizen);
                    for (int i = 0; i < this.citizenRole.location.AllNeighbours.Count(); i++) {
                        HexTile currNeighbour = this.citizenRole.location.AllNeighbours.ElementAt(i);
                        currNeighbour.CollectEventOnTile(this.citizenRole.citizen.city.kingdom, this.citizenRole.citizen);
                    }
                    this.GetNextTargetTile();
                    this.UpdateFogOfWar();
                    this.CheckForKingdomDiscovery();
                }
            }
        }
    }
    #endregion

    #region Unique Functions
    //Adventurer
    private void GetNextTargetTile() {
        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;
        List<HexTile> tilesToChooseFrom = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.VISIBLE).ToList();
        if (tilesToChooseFrom.Count <= 0) {
            tilesToChooseFrom = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER).ToList();
        }
        List<HexTile> hexTilesWithEvents = tilesToChooseFrom.Where(x => x.gameEventInTile != null).ToList();
        if (hexTilesWithEvents.Count > 0) {
            tilesToChooseFrom = hexTilesWithEvents;
        }

        HexTile newTargetTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
        if (newTargetTile != null) {
            this.citizenRole.targetLocation = newTargetTile;
            this.citizenRole.path = PathGenerator.Instance.GetPath(this.citizenRole.location, this.citizenRole.targetLocation, PATHFINDING_MODE.AVATAR);
            this.citizenRole.daysBeforeMoving = this.citizenRole.path[0].movementDays;
        } else {
            Debug.LogError("Adventurer from " + this.citizenRole.citizen.city.kingdom.name + " could not find a new target tile! Current location is " + this.citizenRole.location.name);
            this.citizenRole.gameEventInvolvedIn.DoneEvent();
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
            if (!hasArrived) {
                this.citizenRole.gameEventInvolvedIn.DoneCitizenAction(this.citizenRole.citizen);
                //GetNextTargetTile();
                //SetHasArrivedState(true);
                //this.citizenRole.Attack();
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
    public void MoveToNextTile() {
        Move();
        Task.current.Succeed();
    }
    #endregion
}
