using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PatrolCity : AIBehaviour {

    private City _targetCity;
    private List<HexTile> _patrolTiles;
    private int _currTileIndex;
    private PATROL_DIRECTION currDirection;

    private enum PATROL_DIRECTION { CLOCKWISE, COUNTER_CLOCKWISE }

    public PatrolCity(GameAgent agentPerformingAction, City targetCity) : base(ACTION_TYPE.RANDOM, agentPerformingAction) {
        _targetCity = targetCity;
        HexTile cityTile = targetCity.hexTile;
        _patrolTiles = new List<HexTile>();
        if (cityTile.neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_WEST)) {
            _patrolTiles.Add(cityTile.neighbourDirections[HEXTILE_DIRECTION.NORTH_WEST]);
        }
        if (cityTile.neighbourDirections.ContainsKey(HEXTILE_DIRECTION.NORTH_EAST)) {
            _patrolTiles.Add(cityTile.neighbourDirections[HEXTILE_DIRECTION.NORTH_EAST]);
        }
        if (cityTile.neighbourDirections.ContainsKey(HEXTILE_DIRECTION.EAST)) {
            _patrolTiles.Add(cityTile.neighbourDirections[HEXTILE_DIRECTION.EAST]);
        }
        if (cityTile.neighbourDirections.ContainsKey(HEXTILE_DIRECTION.SOUTH_EAST)) {
            _patrolTiles.Add(cityTile.neighbourDirections[HEXTILE_DIRECTION.SOUTH_EAST]);
        }
        if (cityTile.neighbourDirections.ContainsKey(HEXTILE_DIRECTION.SOUTH_WEST)) {
            _patrolTiles.Add(cityTile.neighbourDirections[HEXTILE_DIRECTION.SOUTH_WEST]);
        }
        if (cityTile.neighbourDirections.ContainsKey(HEXTILE_DIRECTION.WEST)) {
            _patrolTiles.Add(cityTile.neighbourDirections[HEXTILE_DIRECTION.WEST]);
        }
        currDirection = PATROL_DIRECTION.CLOCKWISE;
        //for (int i = 0; i < _patrolTiles.Count; i++) {
        //    _patrolTiles[i].SetTileText(i.ToString(), 1, Color.black);
        //}
        _currTileIndex = GetStartingIndex();
        //Debug.Log("Starting index is " + _currTileIndex.ToString());
    }

    private int GetStartingIndex() {
        for (int i = 0; i < _patrolTiles.Count; i++) {
            if(_patrolTiles[i].elevationType != ELEVATION.WATER && _patrolTiles[i].elevationType != ELEVATION.MOUNTAIN) {
                return i;
            }
        }
        return -1;
    }

    #region overrides
    internal override void DoAction() {
        base.DoAction();
        agentPerformingAction.agentObj.SetTarget(_patrolTiles[_currTileIndex].transform);
    }
    internal override void OnActionDone() {
        base.OnActionDone();
        int nextTileIndex;
        if (currDirection == PATROL_DIRECTION.CLOCKWISE) {
            nextTileIndex = _currTileIndex + 1;
            if (nextTileIndex >= _patrolTiles.Count) {
                nextTileIndex = 0;
            }
        } else {
            nextTileIndex = _currTileIndex - 1;
            if (nextTileIndex < 0) {
                nextTileIndex = _patrolTiles.Count - 1;
            }
        }
        
        HexTile nextPossibleTile = _patrolTiles[nextTileIndex];
        if(nextPossibleTile.elevationType == ELEVATION.WATER || nextPossibleTile.elevationType == ELEVATION.MOUNTAIN) {
            //next tile is water, switch directions
            if(currDirection == PATROL_DIRECTION.CLOCKWISE) {
                nextTileIndex -= 2;
                currDirection = PATROL_DIRECTION.COUNTER_CLOCKWISE;
                if(nextTileIndex < 0) {
                    nextTileIndex = _patrolTiles.Count + nextTileIndex;
                }
            } else {
                nextTileIndex += 2;
                currDirection = PATROL_DIRECTION.CLOCKWISE;
                if(nextTileIndex >= _patrolTiles.Count) {
                    int excessIndex = nextTileIndex - _patrolTiles.Count;
                    nextTileIndex = (_patrolTiles.Count - 1) - excessIndex;
                }
            }
        }

        if(nextTileIndex >= _patrolTiles.Count || nextTileIndex < 0) {
            throw new System.Exception("PATROL CITY AI ERROR! " + nextTileIndex.ToString());
        }

        _currTileIndex = nextTileIndex;
    }
    #endregion
}
