/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class BaseLandmark {
    protected int _id;
    protected string _landmarkName;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected bool _isOccupied;
    protected bool _hasBeenCorrupted;
    protected int _civilianCount;
    protected HexTile _location;
    protected HexTile _connectedTile;
    protected Faction _owner;
    protected LandmarkVisual _landmarkVisual;
    protected List<Item> _itemsInLandmark;
    protected List<LANDMARK_TAG> _landmarkTags;
    public LANDMARK_YIELD_TYPE yieldType;
    public List<BaseLandmark> inGoingConnections { get; private set; }
    public List<BaseLandmark> outGoingConnections { get; private set; }

    #region getters/setters
    public int id {
        get { return _id; }
    }
    public string name {
        get { return landmarkName; }
    }
    public string thisName {
        get { return landmarkName; }
    }
    public string locationName {
        get { return landmarkName + " " + tileLocation.locationName; }
    }
    public string landmarkName {
        get { return _landmarkName; }
    }
    public string urlName {
        get { return "<link=" + '"' + this._id.ToString() + "_landmark" + '"' + ">" + _landmarkName + "</link>"; }
    }
    public LANDMARK_TYPE specificLandmarkType {
        get { return _specificLandmarkType; }
    }
    public bool isOccupied {
        get { return _isOccupied; }
    }
    public Faction owner {
        get { return _owner; }
    }
    public Faction faction {
        get { return _owner; }
    }
    public LandmarkVisual landmarkVisual {
        get { return _landmarkVisual; }
    }
    public LOCATION_IDENTIFIER locIdentifier {
        get { return LOCATION_IDENTIFIER.LANDMARK; }
    }
    public HexTile tileLocation {
        get { return _location; }
    }
    public HexTile connectedTile {
        get { return _connectedTile; }
    }
    #endregion

    public BaseLandmark() {
        _owner = null; //landmark has no owner yet
        _hasBeenCorrupted = false;
        _itemsInLandmark = new List<Item>();
        inGoingConnections = new List<BaseLandmark>();
        outGoingConnections = new List<BaseLandmark>();
    }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this() {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        SetName(RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType));
        ConstructTags(landmarkData);
    }
    public BaseLandmark(HexTile location, LandmarkSaveData data) : this() {
        _id = Utilities.SetID(this, data.landmarkID);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);
        
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
    }
    public BaseLandmark(HexTile location, SaveDataLandmark data) : this() {
        _id = Utilities.SetID(this, data.id);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
    }

    public void SetName(string name) {
        _landmarkName = name;
        if (_landmarkVisual != null) {
            _landmarkVisual.UpdateName();
        }
    }
    public void SetConnectedTile(HexTile connectedTile) {
        _connectedTile = connectedTile;
    }

    #region Virtuals
    public virtual void Initialize() { }
    public virtual void DestroyLandmark() {
        //if (!_landmarkObj.isRuined) {
            //ObjectState ruined = landmarkObj.GetState("Ruined");
            //landmarkObj.ChangeState(ruined);
            //tileLocation.areaOfTile.CheckDeath();
        //}
        //RemoveListeners();
    }
    /*
     What should happen when a character searches this landmark
         */
    public virtual void SearchLandmark(Character character) { }
    #endregion

    #region Ownership
    public virtual void OccupyLandmark(Faction faction) {
        _owner = faction;
        _isOccupied = true;
        _location.Occupy();
        _owner.OwnLandmark(this);
    }
    public virtual void UnoccupyLandmark() {
        if (_owner == null) {
            //throw new System.Exception("Landmark doesn't have an owner but something is trying to unoccupy it!");
        }
        _isOccupied = false;
        _location.Unoccupy();
        _owner = null;
    }
    public void ChangeOwner(Faction newOwner) {
        _owner = newOwner;
        _isOccupied = true;
        _location.Occupy();
    }
    #endregion

    #region Location
    public void AddCharacterToLocation(Party iparty) {
//        //if(iparty.owner.homeLandmark == this) {
//        if (!_charactersAtLocation.Contains(iparty)) {
//            //if (!IsDefenderOfLandmark(iparty)) {
//            _charactersAtLocation.Add(iparty); //only add to characters list if the party is not a defender of the landmark
//            //}
//            //this.tileLocation.RemoveCharacterFromLocation(iparty);
//            //if (iparty.specificLocation != null) {
//            //    iparty.specificLocation.RemoveCharacterFromLocation(iparty);
//            //}
//            iparty.SetSpecificLocation(this.tileLocation.areaOfTile);
//            tileLocation.areaOfTile.AddCharacterToLocation(iparty.owner);
//#if !WORLD_CREATION_TOOL
//            //_landmarkVisual.OnCharacterEnteredLandmark(iparty);
//            Messenger.Broadcast<Party, BaseLandmark>(Signals.PARTY_ENTERED_LANDMARK, iparty, this);
//#endif
//        }
//        //}
    }
    public void RemoveCharacterFromLocation(Party iparty, bool addToTile = false) {
//        if (_charactersAtLocation.Remove(iparty)) {
//            if (addToTile) {
//                this.tileLocation.AddCharacterToLocation(iparty);
//            }
//            tileLocation.areaOfTile.RemoveCharacterFromLocation(iparty.owner);
//#if !WORLD_CREATION_TOOL
//            Messenger.Broadcast<Party, BaseLandmark>(Signals.PARTY_EXITED_LANDMARK, iparty, this);
//#endif
//        } else {
//            Debug.LogWarning("Cannot remove character from " + this.name + " because he/she is not here");
//        }
    }
    public void ReplaceCharacterAtLocation(Party ipartyToReplace, Party ipartyToAdd) {
        //if (_charactersAtLocation.Contains(ipartyToReplace)) {
        //    int indexOfCharacterToReplace = _charactersAtLocation.IndexOf(ipartyToReplace);
        //    _charactersAtLocation.Insert(indexOfCharacterToReplace, ipartyToAdd);
        //    _charactersAtLocation.Remove(ipartyToReplace);
        //    ipartyToAdd.SetSpecificLocation(this.tileLocation.areaOfTile);
        //}
    }
    #endregion

    #region Utilities
    public void SetLandmarkObject(LandmarkVisual obj) {
        _landmarkVisual = obj;
        _landmarkVisual.SetLandmark(this);
    }
    public void CenterOnLandmark() {
        CameraMove.Instance.CenterCameraOn(this.tileLocation.gameObject);
    }
    public override string ToString() {
        return this.landmarkName;
    }
    #endregion

    #region Tags
    private void ConstructTags(LandmarkData landmarkData) {
        _landmarkTags = new List<LANDMARK_TAG>(landmarkData.uniqueTags); //add unique tags
        ////add common tags from base landmark type
        //BaseLandmarkData baseLandmarkData = LandmarkManager.Instance.GetBaseLandmarkData(landmarkData.baseLandmarkType);
        //_landmarkTags.AddRange(baseLandmarkData.baseLandmarkTags);
    }
    #endregion

    #region Corruption
    public void ToggleCorruption(bool state) {
        if (state) {
            LandmarkManager.Instance.corruptedLandmarksCount++;
            if (!_hasBeenCorrupted) {
                _hasBeenCorrupted = true;
            }
            //_diagonalLeftBlocked = 0;
            //_diagonalRightBlocked = 0;
            //_horizontalBlocked = 0;
            //tileLocation.region.LandmarkStartedCorruption(this);
            Messenger.AddListener(Signals.TICK_ENDED, DoCorruption);
            //if (Messenger.eventTable.ContainsKey("StartCorruption")) {
            //    Messenger.RemoveListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);
            //    Messenger.Broadcast<BaseLandmark>("StartCorruption", this);
            //}
        } else {
            LandmarkManager.Instance.corruptedLandmarksCount--;
            StopSpreadCorruption();
        }
    }
    private void StopSpreadCorruption() {
        if (LandmarkManager.Instance.corruptedLandmarksCount > 1) {
            HexTile chosenTile = null;
            int range = 3;
            while (chosenTile == null) {
                List<HexTile> tilesToCheck = tileLocation.GetTilesInRange(range, true);
                for (int i = 0; i < tilesToCheck.Count; i++) {
                    if (tilesToCheck[i].corruptedLandmark != null && tilesToCheck[i].corruptedLandmark.id != this.id) {
                        chosenTile = tilesToCheck[i];
                        break;
                    }
                }
                range++;
            }
            PathGenerator.Instance.CreatePath(this, this.tileLocation, chosenTile, PATHFINDING_MODE.UNRESTRICTED);
        }
        //tileLocation.region.LandmarkStoppedCorruption(this);
        Messenger.RemoveListener(Signals.TICK_ENDED, DoCorruption);
        //Messenger.Broadcast<BaseLandmark>("StopCorruption", this);
    }
    private void DoCorruption() {
        //if (_nextCorruptedTilesToCheck.Count > 0) {
        //    int index = UnityEngine.Random.Range(0, _nextCorruptedTilesToCheck.Count);
        //    HexTile currentCorruptedTileToCheck = _nextCorruptedTilesToCheck[index];
        //    _nextCorruptedTilesToCheck.RemoveAt(index);
        //    SpreadCorruption(currentCorruptedTileToCheck);
        //} else {
        //    StopSpreadCorruption();
        //}
    }
    private void SpreadCorruption(HexTile originTile) {
        //if (!originTile.CanThisTileBeCorrupted()) {
        //    return;
        //}
        for (int i = 0; i < originTile.AllNeighbours.Count; i++) {
            HexTile neighbor = originTile.AllNeighbours[i];
            if (neighbor.uncorruptibleLandmarkNeighbors <= 0) {
                if (!neighbor.isCorrupted) { //neighbor.region.id == originTile.region.id && neighbor.CanThisTileBeCorrupted()
                    neighbor.SetCorruption(true, this);
                    //_nextCorruptedTilesToCheck.Add(neighbor);
                }
                //if(neighbor.landmarkNeighbor != null && !neighbor.landmarkNeighbor.tileLocation.isCorrupted) {
                //    neighbor.landmarkNeighbor.CreateWall();
                //}
                if (originTile.corruptedLandmark.id != neighbor.corruptedLandmark.id) {
                    //originTile.corruptedLandmark.hasAdjacentCorruptedLandmark = true;
                }
            }
            //else {
            //if cannot be corrupted it means that it has a landmark still owned by a kingdom
            //neighbor.landmarkOnTile.CreateWall();
            //}
        }
    }
    public void ALandmarkHasStartedCorruption(BaseLandmark corruptedLandmark) {
        //Messenger.RemoveListener<BaseLandmark>("StartCorruption", ALandmarkHasStartedCorruption);

        //int corruptedX = corruptedLandmark.tileLocation.xCoordinate;
        //int corruptedY = corruptedLandmark.tileLocation.yCoordinate;

        //string direction = "horizontal";
        ////if same column, the wall is automatically horizontal, if not, enter here
        //if (tileLocation.xCoordinate != corruptedX) {
        //    if (tileLocation.yCoordinate == corruptedY) {
        //        int chance = UnityEngine.Random.Range(0, 2);
        //        if (chance == 0) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    } else if (tileLocation.yCoordinate < corruptedY) {
        //        if (tileLocation.xCoordinate < corruptedX) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    } else {
        //        if (tileLocation.xCoordinate < corruptedX) {
        //            direction = "diagonalright";
        //        } else {
        //            direction = "diagonalleft";
        //        }
        //    }
        //}
        //int chance = UnityEngine.Random.Range(0, 3);
        //if (chance == 0) {
        //    direction = "diagonalleft";
        //} else {
        //    direction = "diagonalright";
        //}
        // if (tileLocation.xCoordinate != corruptedX) {
        //    if (tileLocation.xCoordinate < corruptedX) {
        //        if(tileLocation.yCoordinate == corruptedY) {
        //            if(_diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //                direction = "diagonalright";
        //            }else if (_diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //                direction = "diagonalleft";
        //            } else {
        //                if (chance == 0) {
        //                    direction = "diagonalleft";
        //                } else {
        //                    direction = "diagonalright";
        //                }
        //            }
        //        } else {
        //            if(tileLocation.yCoordinate < corruptedY) {
        //                if (_diagonalLeftBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalleft";
        //                } else if (_diagonalLeftBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalleft";
        //                    }
        //                }
        //            } else {
        //                if (_diagonalRightBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalright";
        //                } else if (_diagonalRightBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalright";
        //                    }
        //                }
        //            }
        //        }
        //    } else {
        //        if (tileLocation.yCoordinate == corruptedY) {
        //            if (_diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //                direction = "diagonalright";
        //            } else if (_diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //                direction = "diagonalleft";
        //            } else {
        //                if (chance == 0) {
        //                    direction = "diagonalleft";
        //                } else {
        //                    direction = "diagonalright";
        //                }
        //            }
        //        } else {
        //            if (tileLocation.yCoordinate < corruptedY) {
        //                if (_diagonalRightBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalright";
        //                } else if (_diagonalRightBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalright";
        //                    }
        //                }
        //            } else {
        //                if (_diagonalLeftBlocked <= 0 && _horizontalBlocked > 0) {
        //                    direction = "diagonalleft";
        //                } else if (_diagonalLeftBlocked > 0 && _horizontalBlocked <= 0) {
        //                    direction = "horizontal";
        //                } else {
        //                    if (chance == 0) {
        //                        direction = "diagonalleft";
        //                    }
        //                }
        //            }
        //        }
        //    }
        //} else {
        //    if (_horizontalBlocked > 0 && _diagonalLeftBlocked > 0 && _diagonalRightBlocked <= 0) {
        //        direction = "diagonalright";
        //    } else if (_horizontalBlocked > 0 && _diagonalLeftBlocked <= 0 && _diagonalRightBlocked > 0) {
        //        direction = "diagonalleft";
        //    } else if (_horizontalBlocked <= 0 && _diagonalLeftBlocked > 0 && _diagonalRightBlocked > 0) {
        //        direction = "horizontal";
        //    } else {
        //        if (chance == 0) {
        //            direction = "diagonalleft";
        //        } else {
        //            direction = "diagonalright";
        //        }
        //    }
        //}
        //AdjustDirectionBlocked(direction, 1);
        //_blockedLandmarkDirection.Add(corruptedLandmark, direction);
    }
    public void ReceivePath(List<HexTile> pathTiles) {
        if (pathTiles != null) {
            ConnectCorruption(pathTiles);
        }
    }
    private void ConnectCorruption(List<HexTile> pathTiles) {
        for (int i = 0; i < pathTiles.Count; i++) {
            pathTiles[i].SetUncorruptibleLandmarkNeighbors(0);
            pathTiles[i].SetCorruption(true, this);
        }
    }
    #endregion

    #region Civilians
    public void SetCivilianCount(int count) {
        _civilianCount = count;
    }
    public void AdjustCivilianCount(int amount) {
        _civilianCount += amount;
    }
    #endregion

    #region Connections
    public void AddInGoingConnection(BaseLandmark newConnection) {
        inGoingConnections.Add(newConnection);
    }
    public void AddOutGoingConnection(BaseLandmark newConnection) {
        outGoingConnections.Add(newConnection);
    }
    public bool IsConnectedWith(BaseLandmark otherLandmark) {
        return inGoingConnections.Contains(otherLandmark) || outGoingConnections.Contains(otherLandmark);
    }
    #endregion

    #region Yield Types
    public void SetYieldType(LANDMARK_YIELD_TYPE type) {
        yieldType = type;
    }
    #endregion
}
