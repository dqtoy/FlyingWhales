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
    protected HexTile _location;
    protected HexTile _connectedTile;
    protected LandmarkVisual _landmarkVisual;
    protected LandmarkNameplate nameplate;
    public List<LANDMARK_TAG> landmarkTags { get; private set; }
    public int invasionTicks { get; private set; } //how many ticks until this landmark is invaded. NOTE: This is in raw ticks so if the landmark should be invaded in 1 hour, this should be set to the number of ticks in an hour.
    public Vector2 nameplatePos { get; private set; }

    #region getters/setters
    public int id {
        get { return _id; }
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
    public LandmarkVisual landmarkVisual {
        get { return _landmarkVisual; }
    }
    public HexTile tileLocation {
        get { return _location; }
    }
    public HexTile connectedTile {
        get { return _connectedTile; }
    }
    #endregion

    public BaseLandmark() {
        invasionTicks = 10;//GameManager.ticksPerDay;
    }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this() {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        SetName(RandomNameGenerator.Instance.GetLandmarkName(specificLandmarkType));
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        //nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
    }
    public BaseLandmark(HexTile location, LandmarkSaveData data) : this() {
        _id = Utilities.SetID(this, data.landmarkID);
        _location = location;
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);
        
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        //nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
    }
    public BaseLandmark(HexTile location, SaveDataLandmark data) : this() {
        _id = Utilities.SetID(this, data.id);
        _location = location;
        if(data.connectedTileID != -1) {
            _connectedTile = GridMap.Instance.hexTiles[data.connectedTileID];
        }
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);
        landmarkTags = data.landmarkTags;
        invasionTicks = GameManager.ticksPerDay;

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        //nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
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
    public void ChangeLandmarkType(LANDMARK_TYPE type) {
        _specificLandmarkType = type;
        tileLocation.UpdateLandmarkVisuals();
        //if (type == LANDMARK_TYPE.NONE) {
        //    ObjectPoolManager.Instance.DestroyObject(nameplate.gameObject);
        //}
    }
    /// <summary>
    /// Override the id of this landmark. NOTE: This is only used when a landmark is destroyed and another landmark replaces it.
    /// So that the saves won't break.
    /// </summary>
    /// <param name="id">The id to use.</param>
    public void OverrideID(int id) {
        _id = id;
    }

    #region Virtuals
    public virtual void Initialize() { }
    public virtual void DestroyLandmark() {
        _location = null;
    }
    public virtual void OnFinishedBuilding() { }
    /// <summary>
    /// What should happen when a minion is assigned to the region that this landmark is at?
    /// </summary>
    /// <param name="minion">The minion that was assigned.</param>
    public virtual void OnMinionAssigned(Minion minion) { }
    /// <summary>
    /// What should happen when a minion has been unassigned from the region that this landmark is at?
    /// </summary>
    /// <param name="minion">The minion that was unassigned.</param>
    public virtual void OnMinionUnassigned(Minion minion) { }
    #endregion

    #region Utilities
    public void SetLandmarkObject(LandmarkVisual obj) {
        _landmarkVisual = obj;
        _landmarkVisual.SetLandmark(this);
    }
    public void CenterOnLandmark() {
        tileLocation.CenterCameraHere();
    }
    public override string ToString() {
        return this.landmarkName;
    }
    #endregion

    #region Tags
    private void ConstructTags(LandmarkData landmarkData) {
        landmarkTags = new List<LANDMARK_TAG>(landmarkData.uniqueTags); //add unique tags
        ////add common tags from base landmark type
        //BaseLandmarkData baseLandmarkData = LandmarkManager.Instance.GetBaseLandmarkData(landmarkData.baseLandmarkType);
        //_landmarkTags.AddRange(baseLandmarkData.baseLandmarkTags);
    }
    #endregion       
}
