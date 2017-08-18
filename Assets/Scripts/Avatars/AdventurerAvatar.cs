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

    #region Overrides
    //internal override void Move() {
    //    if (this.citizenRole.targetLocation != null) {
    //        if (this.citizenRole.path != null) {
    //            if (this.citizenRole.path.Count > 0) {
    //                this.MakeCitizenMove(this.citizenRole.location, this.citizenRole.path[0]);
    //                //this.transform.position = new Vector2(this.citizenRole.path[0].transform.position.x, this.citizenRole.path[0].transform.position.y);
    //                this.citizenRole.location = this.citizenRole.path[0];
    //                this.citizenRole.citizen.currentLocation = this.citizenRole.path[0];
    //                this.citizenRole.path.RemoveAt(0);
    //                this.CheckForKingdomDiscovery();
    //                //this.GetNextTargetTile();
    //            }
    //            this.UpdateFogOfWar();
    //            CollectEvents();

    //        }
    //    }
    //}
    #endregion

    #region Unique Functions
    [Task]
    private void GetNextTargetTile() {
        if(newTargetTile != null) {
            Task.current.Succeed();
            return;
        }
        Kingdom kingdomOfAdventurer = this.citizenRole.citizen.city.kingdom;

        List<HexTile> visibleTilesWithEvents = kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.VISIBLE]
            .Where(x => x.gameEventInTile != null && priorityEvents.Contains(x.gameEventInTile.eventType)).ToList();
        if(visibleTilesWithEvents.Count > 0) {
            newTargetTile = visibleTilesWithEvents[Random.Range(0, visibleTilesWithEvents.Count)];
        } else {
            List<HexTile> seenTilesWithEvents = kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.SEEN]
            .Where(x => x.gameEventInTile != null && priorityEvents.Contains(x.gameEventInTile.eventType)).ToList();

            //Prioritize tiles with events in accordance to priorityEvents list
            if (seenTilesWithEvents.Count > 0) {
                newTargetTile = seenTilesWithEvents[Random.Range(0, seenTilesWithEvents.Count)];
            } else {
                List<HexTile> hiddenNeighbours = this.citizenRole.location.sameTagNeighbours
                    .Where(x => kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] == FOG_OF_WAR_STATE.HIDDEN).ToList();

                if (hiddenNeighbours.Count > 0) {
                    //choose from hidden neighbours
                    newTargetTile = hiddenNeighbours[Random.Range(0, hiddenNeighbours.Count)];
                } else {
                    //get nearest elligible hidden tile as target
                    List<HexTile> elligibleHiddenTiles = kingdomOfAdventurer.fogOfWarDict[FOG_OF_WAR_STATE.HIDDEN]
                        .Where(x => x.tag == this.citizenRole.location.tag).ToList();
                    if (elligibleHiddenTiles.Count > 0) {
                        //if there is an elligible hidden tile, get the nearest
                        //order tiles by nearest from the citizens location
                        elligibleHiddenTiles = elligibleHiddenTiles.OrderBy(x => this.citizenRole.location.GetDistanceTo(x)).ToList();
                        newTargetTile = elligibleHiddenTiles.FirstOrDefault();
                    } else {
                        //if no more hidden tiles choose from seen or visible neighbours
                        List<HexTile> tilesToChooseFrom = this.citizenRole.location.sameTagNeighbours
                            .Where(x => kingdomOfAdventurer.fogOfWar[x.xCoordinate, x.yCoordinate] != FOG_OF_WAR_STATE.HIDDEN).ToList();

                        List<HexTile> hexTilesWithEvents = tilesToChooseFrom.Where(x => x.gameEventInTile != null).ToList();
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
                this.citizenRole.targetLocation = null;
                newTargetTile = null;
                //this.citizenRole.gameEventInvolvedIn.DoneCitizenAction(this.citizenRole.citizen);
                //GetNextTargetTile();
                //SetHasArrivedState(true);
                //this.citizenRole.Attack();
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
        CollectEvents();
        Task.current.Succeed();
    }
    #endregion
}
