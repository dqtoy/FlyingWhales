/*
 This is the base class for all landmarks.
 eg. Settlements(Cities), Resources, Dungeons, Lairs, etc.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class BaseLandmark : ILocation {
    protected int _id;
    protected string _landmarkName;
    protected LANDMARK_TYPE _specificLandmarkType;
    protected bool _canBeOccupied; //can the landmark be occupied?
    protected bool _isOccupied;
    protected bool _isBeingInspected;
    protected bool _isRaided;
    protected bool _hasBeenInspected;
    protected bool _hasBeenCorrupted;
    protected bool _isAttackingAnotherLandmark;
    protected int _combatHistoryID;
    protected int _civilianCount;
    protected HexTile _location;
    protected HexTile _connectedTile;
    protected Faction _owner;
    protected LandmarkVisual _landmarkVisual;
    //protected List<Character> _charactersWithHomeOnLandmark;
    protected List<BaseLandmark> _connections;
    protected List<Character> _prisoners; //list of prisoners on landmark
    protected List<Log> _history;
    //protected List<Party> _charactersAtLocation;
    protected List<Party> _assaultParties;
    protected List<Item> _itemsInLandmark;
    protected List<LANDMARK_TAG> _landmarkTags;
    protected List<HexTile> _nextCorruptedTilesToCheck;
    protected List<HexTile> _wallTiles;
    //protected List<Secret> _secrets;
    protected List<Token> _tokens;
    protected List<string> _encounters;
    protected Dictionary<Character, GameDate> _characterTraces; //Lasts for 60 days
    protected Dictionary<int, Combat> _combatHistory;
    protected Dictionary<RESOURCE, int> _resourceInventory;
    
    public bool hasAdjacentCorruptedLandmark;

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
    public List<BaseLandmark> connections {
        get { return _connections; }
    }
    public bool canBeOccupied {
        get { return _canBeOccupied; }
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
    //public List<Character> charactersWithHomeOnLandmark {
    //    get { return _charactersWithHomeOnLandmark; }
    //}
    public LandmarkVisual landmarkVisual {
        get { return _landmarkVisual; }
    }
    public List<Character> prisoners {
        get { return _prisoners; }
    }
    public List<Log> history {
        get { return this._history; }
    }
    public Dictionary<int, Combat> combatHistory {
        get { return _combatHistory; }
    }
    public List<Party> charactersAtLocation {
        get { return null; }
    }
    public List<Item> itemsInLandmark {
        get { return _itemsInLandmark; }
    }
    public List<Token> tokens {
        get { return _tokens; }
    }
    public List<HexTile> wallTiles {
        get { return _wallTiles; }
    }
    public List<Party> assaultParties {
        get { return _assaultParties; }
    }
    public Dictionary<Character, GameDate> characterTraces {
        get { return _characterTraces; }
    }
    public int civilianCount {
        get { return _civilianCount; }
    }
    public bool isAttackingAnotherLandmark {
        get { return _isAttackingAnotherLandmark; }
    }
    public bool isBeingInspected {
        get { return _isBeingInspected; }
    }
    public bool hasBeenInspected {
        get { return _hasBeenInspected; }
    }
    public bool isRaided {
        get { return _isRaided; }
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
    public ILocation specificLocation {
        get { return this; }
    }
    #endregion

    public BaseLandmark() {
        _owner = null; //landmark has no owner yet
        _combatHistoryID = 0;
        _hasBeenCorrupted = false;
        hasAdjacentCorruptedLandmark = false;
        _connections = new List<BaseLandmark>();
        //_charactersWithHomeOnLandmark = new List<Character>();
        _prisoners = new List<Character>();
        _history = new List<Log>();
        //_charactersAtLocation = new List<Party>();
        _itemsInLandmark = new List<Item>();
        _nextCorruptedTilesToCheck = new List<HexTile>();
        _wallTiles = new List<HexTile>();
        //_secrets = new List<Secret>();
        _tokens = new List<Token>();
        _encounters = new List<string>();
        _combatHistory = new Dictionary<int, Combat>();
        _characterTraces = new Dictionary<Character, GameDate>();
        _assaultParties = new List<Party>();
        //SetSupplyProductionState(true);
        //SetMaxDefenderCount(4);
        //defenderBuffs = new List<Buff>();
        //defenders = new Party[LandmarkManager.MAX_DEFENDERS];
        //Messenger.AddListener(Signals.TOGGLE_CHARACTERS_VISIBILITY, OnToggleCharactersVisibility);
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
    internal void ChangeLandmarkType(LANDMARK_TYPE newLandmarkType) {
        _specificLandmarkType = newLandmarkType;
        Initialize();
    }
    public void CenterOnLandmark() {
        CameraMove.Instance.CenterCameraOn(this.tileLocation.gameObject);
    }
    public override string ToString() {
        return this.landmarkName;
    }
    public void SetIsAttackingAnotherLandmarkState(bool state) {
        _isAttackingAnotherLandmark = state;
    }
    private void OnToggleCharactersVisibility() {
        //_landmarkVisual.ToggleCharactersVisibility();
    }
    //private void RemoveListeners() {
    //Messenger.RemoveListener(Signals.DAY_START, DailySupplyProduction);
    //}
    #endregion

    #region History
    internal void AddHistory(Log log) {
        if (!_history.Contains(log)) {
            _history.Add(log);
            if (this._history.Count > 60) {
                if (this._history[0].goapAction != null) {
                    this._history[0].goapAction.AdjustReferenceCount(-1);
                }
                this._history.RemoveAt(0);
            }
            if (log.goapAction != null) {
                log.goapAction.AdjustReferenceCount(1);
            }
            //tileLocation.areaOfTile.AddHistory(log);
            Messenger.Broadcast(Signals.HISTORY_ADDED, this as object);
        }
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
                _nextCorruptedTilesToCheck.Add(tileLocation);
            }
            //_diagonalLeftBlocked = 0;
            //_diagonalRightBlocked = 0;
            //_horizontalBlocked = 0;
            //tileLocation.region.LandmarkStartedCorruption(this);
            PutWallDown();
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
        if (!hasAdjacentCorruptedLandmark && LandmarkManager.Instance.corruptedLandmarksCount > 1) {
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
        if (_nextCorruptedTilesToCheck.Count > 0) {
            int index = UnityEngine.Random.Range(0, _nextCorruptedTilesToCheck.Count);
            HexTile currentCorruptedTileToCheck = _nextCorruptedTilesToCheck[index];
            _nextCorruptedTilesToCheck.RemoveAt(index);
            SpreadCorruption(currentCorruptedTileToCheck);
        } else {
            StopSpreadCorruption();
        }
    }
    private void SpreadCorruption(HexTile originTile) {
        if (!originTile.CanThisTileBeCorrupted()) {
            return;
        }
        for (int i = 0; i < originTile.AllNeighbours.Count; i++) {
            HexTile neighbor = originTile.AllNeighbours[i];
            if (neighbor.uncorruptibleLandmarkNeighbors <= 0) {
                if (!neighbor.isCorrupted) { //neighbor.region.id == originTile.region.id && neighbor.CanThisTileBeCorrupted()
                    neighbor.SetCorruption(true, this);
                    _nextCorruptedTilesToCheck.Add(neighbor);
                }
                //if(neighbor.landmarkNeighbor != null && !neighbor.landmarkNeighbor.tileLocation.isCorrupted) {
                //    neighbor.landmarkNeighbor.CreateWall();
                //}
                if (originTile.corruptedLandmark.id != neighbor.corruptedLandmark.id) {
                    originTile.corruptedLandmark.hasAdjacentCorruptedLandmark = true;
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
        PutWallUp();
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
    public void PutWallUp() {
        //_wallDirection = direction;
        //List<HexTile> wallTiles = _horizontalTiles;
        //if(direction == "diagonalleft") {
        //    wallTiles = _diagonalLeftTiles;
        //} else if (direction == "diagonalright") {
        //    wallTiles = _diagonalRightTiles;
        //}
        //for (int i = 0; i < wallTiles.Count; i++) {
        //    wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(1);
        //}
        for (int i = 0; i < _wallTiles.Count; i++) {
            _wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(1);
        }
    }
    private void PutWallDown() {
        for (int i = 0; i < _wallTiles.Count; i++) {
            _wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        }
        //for (int i = 0; i < tileLocation.AllNeighbours.Count; i++) {
        //    tileLocation.AllNeighbours[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        //}
        //if(_wallDirection != string.Empty) {
        //    List<HexTile> wallTiles = _horizontalTiles;
        //    if (_wallDirection == "diagonalleft") {
        //        wallTiles = _diagonalLeftTiles;
        //    } else if (_wallDirection == "diagonalright") {
        //        wallTiles = _diagonalRightTiles;
        //    }
        //    for (int i = 0; i < wallTiles.Count; i++) {
        //        wallTiles[i].AdjustUncorruptibleLandmarkNeighbors(-1);
        //    }
        //    _wallDirection = string.Empty;
        //}
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

    #region IInteractable
    public void SetIsBeingInspected(bool state) {
        _isBeingInspected = state;
        //_landmarkVisual.ToggleCharactersVisibility();
        //if (_isBeingInspected) {
        //    Messenger.Broadcast(Signals.LANDMARK_INSPECTED, this);
        //}
    }
    public void SetHasBeenInspected(bool state) {
        _hasBeenInspected = state;
        //if (state) {
        //    if (owner != null && owner.id != PlayerManager.Instance.player.playerFaction.id) {
        //        PlayerManager.Instance.player.AddIntel(owner.factionIntel);
        //    }
        //    if (tileLocation.areaOfTile != null && tileLocation.areaOfTile.id != PlayerManager.Instance.player.playerArea.id) {
        //        PlayerManager.Instance.player.AddIntel(tileLocation.areaOfTile.locationIntel);
        //    }
        //}
    }
    public void EndedInspection() {
        UpdateLastInspection();
    }
    private void UpdateLastInspection() {
        
    }
    #endregion
}
