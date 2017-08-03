using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;
using System.Linq;

public class AdventurerAvatar : CitizenAvatar {

    public HexTile newTargetTile;

    #region Overrides
    internal override void Move() {
        if (this.citizenRole.targetLocation != null) {
            if (this.citizenRole.path != null) {
                if (this.citizenRole.path.Count > 0) {
                    this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
                    //this.transform.position = new Vector2(this.citizenRole.path[0].transform.position.x, this.citizenRole.path[0].transform.position.y);
                    this.citizenRole.location = this.citizenRole.path[0];
                    this.citizenRole.citizen.currentLocation = this.citizenRole.path[0];
                    this.citizenRole.path.RemoveAt(0);
                    this.citizenRole.location.CollectEventOnTile(this.citizenRole.citizen.city.kingdom, this.citizenRole.citizen);
                    for (int i = 0; i < this.citizenRole.location.AllNeighbours.Count(); i++) {
                        HexTile currNeighbour = this.citizenRole.location.AllNeighbours.ElementAt(i);
                        currNeighbour.CollectEventOnTile(this.citizenRole.citizen.city.kingdom, this.citizenRole.citizen);
                    }
                    //this.GetNextTargetTile();
                }
                //this.UpdateFogOfWar();
                this.CheckForKingdomDiscovery();
            }
        }
    }
    #endregion

    #region Unique Functions
    [Task]
    private void GetNearestHiddenTile() {
        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;

        List<HexTile> allHiddenTiles = GridMap.Instance.listHexes
                .Where(x => x.GetComponent<HexTile>().elevationType != ELEVATION.WATER
                && kingdomOfAdventurer.fogOfWar[x.GetComponent<HexTile>().xCoordinate, x.GetComponent<HexTile>().yCoordinate] == FOG_OF_WAR_STATE.HIDDEN)
                .Select(x => x.GetComponent<HexTile>()).ToList();

        allHiddenTiles = allHiddenTiles.OrderBy(x => Vector2.Distance(this.citizenRole.location.transform.position, x.transform.position)).ToList();
        if (allHiddenTiles.Count > 0) {
            HexTile nearestHiddenTile = allHiddenTiles.FirstOrDefault();
            newTargetTile = nearestHiddenTile.AllNeighbours.Where(x => kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.HIDDEN)
                .OrderBy(x => Vector2.Distance(this.citizenRole.location.transform.position, x.transform.position)).FirstOrDefault();
            Task.current.Succeed();
            if (newTargetTile != null) {
                this.citizenRole.targetLocation = newTargetTile;
                this.citizenRole.path = PathGenerator.Instance.GetPath(this.citizenRole.location, this.citizenRole.targetLocation, PATHFINDING_MODE.AVATAR);
                this.citizenRole.daysBeforeMoving = this.citizenRole.path[0].movementDays;
                Task.current.Succeed();
            } else {
                Task.current.Fail();
                Debug.LogError("Adventurer from " + this.citizenRole.citizen.city.kingdom.name + " could not find a new target tile! Current location is " + this.citizenRole.location.name);
                this.citizenRole.gameEventInvolvedIn.DoneEvent();
            }
        } else {
            Task.current.Fail();
        }
    }
    [Task]
    private void GetTargetTileFromNeighbours() {
        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;
        List<HexTile> hiddenNeighbours = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER
            && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] == FOG_OF_WAR_STATE.HIDDEN).ToList();

        if (hiddenNeighbours.Count > 0) {
            //choose from hidden neighbours
            newTargetTile = hiddenNeighbours.ElementAtOrDefault(Random.Range(0, hiddenNeighbours.Count));
            if (newTargetTile != null) {
                this.citizenRole.targetLocation = newTargetTile;
                this.citizenRole.path = PathGenerator.Instance.GetPath(this.citizenRole.location, this.citizenRole.targetLocation, PATHFINDING_MODE.AVATAR);
                this.citizenRole.daysBeforeMoving = this.citizenRole.path[0].movementDays;
                Task.current.Succeed();
            } else {
                Task.current.Fail();
                Debug.LogError("Adventurer from " + this.citizenRole.citizen.city.kingdom.name + " could not find a new target tile! Current location is " + this.citizenRole.location.name);
                this.citizenRole.gameEventInvolvedIn.DoneEvent();
            }
        } else {
            Task.current.Fail();
        }
    }
    [Task]
    private void GetNextTargetTile() {
        if(newTargetTile != null) {
            Task.current.Succeed();
            return;
        }

        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;

        List<HexTile> hiddenNeighbours = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER
            && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] == FOG_OF_WAR_STATE.HIDDEN).ToList();

        if (hiddenNeighbours.Count > 0) {
            //choose from hidden neighbours
            newTargetTile = hiddenNeighbours[Random.Range(0, hiddenNeighbours.Count)];
        } else {
            //get nearest hidden tile as target
            List<HexTile> allHiddenTiles = GridMap.Instance.listHexes
                .Where(x => x.GetComponent<HexTile>().elevationType != ELEVATION.WATER 
                && kingdomOfAdventurer.fogOfWar[x.GetComponent<HexTile>().xCoordinate, x.GetComponent<HexTile>().yCoordinate] == FOG_OF_WAR_STATE.HIDDEN)
                .Select(x => x.GetComponent<HexTile>()).ToList();

            allHiddenTiles = allHiddenTiles.OrderBy(x => Vector2.Distance(this.citizenRole.location.transform.position, x.transform.position)).ToList();
            if(allHiddenTiles.Count > 0) {
                HexTile nearestHiddenTile = allHiddenTiles.FirstOrDefault();
                newTargetTile = nearestHiddenTile.AllNeighbours.Where(x => kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.HIDDEN)
                    .OrderBy(x => Vector2.Distance(this.citizenRole.location.transform.position, x.transform.position)).FirstOrDefault();
            } else {
                //if no more hidden tiles choose from seen or visible neighbours
                List<HexTile> tilesToChooseFrom = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER
                    && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.HIDDEN).ToList();
                List<HexTile> hexTilesWithEvents = tilesToChooseFrom.Where(x => x.gameEventInTile != null).ToList();
                if (hexTilesWithEvents.Count > 0) {
                    tilesToChooseFrom = hexTilesWithEvents;
                }
                newTargetTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
            }
        }

        //List<HexTile> tilesToChooseFrom = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER 
        //    && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.VISIBLE).ToList();

        //if (tilesToChooseFrom.Count <= 0) {
        //    tilesToChooseFrom = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER).ToList();
        //}
        //List<HexTile> hexTilesWithEvents = tilesToChooseFrom.Where(x => x.gameEventInTile != null).ToList();
        //if (hexTilesWithEvents.Count > 0) {
        //    tilesToChooseFrom = hexTilesWithEvents;
        //}

        if (newTargetTile != null) {
            this.citizenRole.targetLocation = newTargetTile;
            this.citizenRole.path = PathGenerator.Instance.GetPath(this.citizenRole.location, this.citizenRole.targetLocation, PATHFINDING_MODE.AVATAR);
            this.citizenRole.daysBeforeMoving = this.citizenRole.path[0].movementDays;
            Task.current.Succeed();
        } else {
            Task.current.Fail();
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
                newTargetTile = null;
                //this.citizenRole.gameEventInvolvedIn.DoneCitizenAction(this.citizenRole.citizen);
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

    [Task]
    public bool IsNextToHiddenTile() {
        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;
        List<HexTile> hiddenNeighbours = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER
            && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] == FOG_OF_WAR_STATE.HIDDEN).ToList();
        if (hiddenNeighbours.Count > 0) {
            return true;
        }
        return false;
    }

    [Task]
    public bool HasTargetTile() {
        if(newTargetTile != null) {
            return true;
        }
        return false;
    }

    [Task]
    private void ForceUpdateFogOfWar() {
        this.UpdateFogOfWar();
        Task.current.Succeed();
    }
    #endregion
}
