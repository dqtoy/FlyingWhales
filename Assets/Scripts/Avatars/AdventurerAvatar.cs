using UnityEngine;
using System.Collections;
using Panda;
using System.Collections.Generic;
using System.Linq;
using EZObjectPools;

public class AdventurerAvatar : CitizenAvatar {

    [SerializeField] private HexTile newTargetTile = null;

    private HashSet<EVENT_TYPES> priorityEvents = new HashSet<EVENT_TYPES>() {
        EVENT_TYPES.BOON_OF_POWER,
        EVENT_TYPES.ANCIENT_RUIN,
        EVENT_TYPES.ALTAR_OF_BLESSING,
        EVENT_TYPES.FIRST_AND_KEYSTONE
    };

    [ContextMenu("Force Change Target Tile")]
    public void ForceChangeTargetTile() {
        this.citizenRole.targetLocation = newTargetTile;
        this.citizenRole.path = PathGenerator.Instance.GetPath(this.citizenRole.citizen.currentLocation, newTargetTile, PATHFINDING_MODE.AVATAR);
        this.citizenRole.daysBeforeMoving = this.citizenRole.path[0].movementDays;
    }

    #region Unique Functions
    [Task]
    private void GetNextTargetTile() {
        if(newTargetTile != null) {
            Task.current.Succeed();
            return;
        }
        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;

        //List<HexTile> visibleTilesWithEvents = kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.VISIBLE]
        //    .Where(x => x.gameEventInTile != null && priorityEvents.Contains(x.gameEventInTile.eventType)).ToList();

        List<HexTile> visibleTilesWithEvents = new List<HexTile>(kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.VISIBLE]
            .Where(x => x.gameEventInTile != null && priorityEvents.Contains(x.gameEventInTile.eventType)));
        //		for (int i = 0; i < kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.VISIBLE].Count; i++) {
        //			
        //		}
        if (visibleTilesWithEvents.Count > 0) {
            newTargetTile = visibleTilesWithEvents[Random.Range(0, visibleTilesWithEvents.Count)];
        } else {
            //List<HexTile> seenTilesWithEvents = kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.SEEN]
            //.Where(x => x.gameEventInTile != null && priorityEvents.Contains(x.gameEventInTile.eventType)).ToList();

            List<HexTile> seenTilesWithEvents = new List<HexTile>(kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.SEEN]
            .Where(x => x.gameEventInTile != null && priorityEvents.Contains(x.gameEventInTile.eventType)));

            //Prioritize tiles with events in accordance to priorityEvents list
            if (seenTilesWithEvents.Count > 0) {
                newTargetTile = seenTilesWithEvents[Random.Range(0, seenTilesWithEvents.Count)];
            } else {
                List<HexTile> hiddenNeighbours = new List<HexTile>(this.citizenRole.location.sameTagNeighbours
                    .Where(x => kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] == FOG_OF_WAR_STATE.HIDDEN));

                if (hiddenNeighbours.Count > 0) {
                    //choose from hidden neighbours
                    newTargetTile = hiddenNeighbours[Random.Range(0, hiddenNeighbours.Count)];
                } else {
                    //get nearest elligible hidden tile as target
                    List<HexTile> elligibleHiddenTiles = new List<HexTile>(kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.HIDDEN]
                        .Where(x => x.tileTag == this.citizenRole.location.tileTag));
                    if (elligibleHiddenTiles.Count > 0) {
                        //if there is an elligible hidden tile, get the nearest
                        //order tiles by nearest from the citizens location
                        elligibleHiddenTiles = elligibleHiddenTiles.OrderBy(x => this.citizenRole.location.GetDistanceTo(x)).ToList();
                        newTargetTile = elligibleHiddenTiles.FirstOrDefault();
                    } else {
                        //if no more hidden tiles choose from seen or visible neighbours
                        List<HexTile> tilesToChooseFrom = new List<HexTile>(this.citizenRole.location.sameTagNeighbours
                            .Where(x => kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.HIDDEN));

                        List<HexTile> hexTilesWithEvents = new List<HexTile>(tilesToChooseFrom.Where(x => x.gameEventInTile != null));
                        if (hexTilesWithEvents.Count > 0) {
                            tilesToChooseFrom = hexTilesWithEvents;
                        }
                        newTargetTile = tilesToChooseFrom[Random.Range(0, tilesToChooseFrom.Count)];
                    }
                }
            }
        }

        if (newTargetTile != null) {
            this.citizenRole.targetLocation = newTargetTile;
            this.citizenRole.path = PathGenerator.Instance.GetPath(this.citizenRole.citizen.currentLocation, newTargetTile, PATHFINDING_MODE.AVATAR);
            if(this.citizenRole.path == null) {
                throw new System.Exception("An adventurer from " + kingdomOfAdventurer.name + " ran into a problem!" +
                    this.citizenRole.citizen.currentLocation.name + " -> " + newTargetTile.name);
            }
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
    public void HasArrivedAtTargetHextile() {
        if (this.citizenRole.location == this.citizenRole.targetLocation) {
                this.citizenRole.targetLocation = null;
                newTargetTile = null;
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
        //List<HexTile> hiddenNeighbours = this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER
        //    && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] == FOG_OF_WAR_STATE.HIDDEN).ToList();
        if (this.citizenRole.location.AllNeighbours.Where(x => x.elevationType != ELEVATION.WATER
            && kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] == FOG_OF_WAR_STATE.HIDDEN).Any()) {
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
    #endregion
}
