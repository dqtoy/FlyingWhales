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
    public LandmarkNameplate nameplate { get; }
    public List<LANDMARK_TAG> landmarkTags { get; private set; }
    public Vector2 nameplatePos { get; private set; }
    //public int invasionTicks { get { return GetInvasionTicks(); } }
    public int invasionTicks { get; private set; }
    //private int _invasionTicks; //how many ticks until this landmark is invaded. NOTE: This is in raw ticks so if the landmark should be invaded in 1 hour, this should be set to the number of ticks in an hour.

    #region getters/setters
    public int id => _id;
    public string landmarkName => _landmarkName;
    public string urlName => $"<link=\"{this._id.ToString()}_landmark\">{_landmarkName}</link>";
    public LANDMARK_TYPE specificLandmarkType => _specificLandmarkType;
    public LandmarkVisual landmarkVisual => _landmarkVisual;
    public HexTile tileLocation => _location;
    public HexTile connectedTile => _connectedTile;
    #endregion

    private BaseLandmark() { }
    public BaseLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : this() {
        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        _id = UtilityScripts.Utilities.SetID(this);
        _location = location;
        _specificLandmarkType = specificLandmarkType;
        SetName(RandomNameGenerator.GetLandmarkName(specificLandmarkType));
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
        SetInvasionTicks(GameManager.Instance.GetTicksBasedOnHour(4));
    }
    public BaseLandmark(HexTile location, SaveDataLandmark data) : this() {
        _id = UtilityScripts.Utilities.SetID(this, data.id);
        _location = location;
        if(data.connectedTileID != -1) {
            _connectedTile = GridMap.Instance.normalHexTiles[data.connectedTileID];
        }
        _specificLandmarkType = data.landmarkType;
        SetName(data.landmarkName);
        landmarkTags = data.landmarkTags;
        SetInvasionTicks(data.invasionTicks);

        LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(specificLandmarkType);
        ConstructTags(landmarkData);
        nameplatePos = LandmarkManager.Instance.GetNameplatePosition(this.tileLocation);
        nameplate = UIManager.Instance.CreateLandmarkNameplate(this);
    }
    private void SetName(string name) {
        _landmarkName = name;
        if (_landmarkVisual != null) {
            _landmarkVisual.UpdateName();
        }
    }
    public void SetConnectedTile(HexTile connectedTile) {
        _connectedTile = connectedTile;
    }
    public void ChangeLandmarkType(LANDMARK_TYPE type) {
        if (this.specificLandmarkType.IsPlayerLandmark()) {
            //if provided landmark type is player landmark, then create a new instance instead.
            LandmarkManager.Instance.CreateNewLandmarkOnTile(tileLocation, type, false);
            return;
        }
        _specificLandmarkType = type;
        tileLocation.UpdateStructureVisuals(type);
        tileLocation.UpdateBuildSprites();
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
        if (tileLocation.region.assignedMinion != null) {
            tileLocation.region.assignedMinion.SetAssignedRegion(null);
            tileLocation.region.SetAssignedMinion(null);
        }
        for (int i = 0; i < tileLocation.featureComponent.features.Count; i++) {
            tileLocation.featureComponent.features[i].OnDemolishLandmark(tileLocation, specificLandmarkType);
        }
        if (specificLandmarkType.IsPlayerLandmark()) {
            HexTile tile = _location;
            UIManager.Instance.ShowImportantNotification(GameManager.Instance.Today(), UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(specificLandmarkType.ToString()) + " was destroyed!", () => UIManager.Instance.ShowRegionInfo(tile.region));
        }
        ObjectPoolManager.Instance.DestroyObject(nameplate.gameObject);
        _location = null;
    }
    public virtual void OnFinishedBuilding() { }
    /// <summary>
    /// What should happen when a minion is assigned to the region that this landmark is at?
    /// </summary>
    /// <param name="minion">The minion that was assigned.</param>
    public virtual void OnMinionAssigned(Minion minion) {
        if (this.specificLandmarkType.IsPlayerLandmark()) {
            Messenger.Broadcast(Signals.MINION_ASSIGNED_PLAYER_LANDMARK, minion, this);
        }
    }
    /// <summary>
    /// What should happen when a minion has been unassigned from the region that this landmark is at?
    /// </summary>
    /// <param name="minion">The minion that was unassigned.</param>
    public virtual void OnMinionUnassigned(Minion minion) {
        if (this.specificLandmarkType.IsPlayerLandmark()) {
            Messenger.Broadcast(Signals.MINION_UNASSIGNED_PLAYER_LANDMARK, minion, this);
        }
    }
    #endregion

    #region Utilities
    public void SetLandmarkObject(LandmarkVisual obj) {
        _landmarkVisual = obj;
        _landmarkVisual.SetLandmark(this);
    }
    public void CenterOnLandmark() {
        tileLocation.CenterCameraHere();
    }
    //private int GetInvasionTicks() {
    //    return invasionTicks + Mathf.RoundToInt(invasionTicks * PlayerManager.Instance.player.invasionRatePercentageModifier);
    //}
    public void SetInvasionTicks(int amount) {
        invasionTicks = amount;
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

    #region Region Features
    /// <summary>
    /// Add features to the region that this landmark is in.
    /// </summary>
    // public void AddFeaturesToRegion() {
    //     if (specificLandmarkType == LANDMARK_TYPE.NONE) {
    //         return; //do not
    //     }
    //     if (tileLocation.isCorrupted || tileLocation.region.settlement != null) {
    //         //Do not add feature is region is part of player or is a settlement region
    //         return;
    //     }
    //     //if(tileLocation.region != null && tileLocation.region.settlement != null/* && tileLocation.region.settlement.areaMap.isSettlementMap*/) {
    //     //    return; //do not
    //     //}
    //
    //     //Constant features
    //     switch (specificLandmarkType) {
    //         case LANDMARK_TYPE.BARRACKS:
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Experience_Feature));
    //             //tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(RegionFeatureDB.Fortified_Feature));
    //             break;
    //         case LANDMARK_TYPE.MAGE_TOWER:
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Experience_Feature));
    //             break;
    //         case LANDMARK_TYPE.TEMPLE:
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Warded_Feature));
    //             break;
    //         case LANDMARK_TYPE.MONSTER_LAIR:
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Summons_Feature));
    //             break;
    //         case LANDMARK_TYPE.FARM:
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Fertile_Feature));
    //             break;
    //         case LANDMARK_TYPE.MINES:
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Stony_Feature));
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Metal_Rich_Feature));
    //             break;
    //     }
    //
    //     //random features
    //     WeightedDictionary<string> randomFeatureWeights = new WeightedDictionary<string>();
    //     switch (specificLandmarkType) {
    //         case LANDMARK_TYPE.MONSTER_LAIR:
    //             randomFeatureWeights.AddElement(TileFeatureDB.Spell_Feature, 25);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Knowledge_Feature, 25);
    //             break;
    //         case LANDMARK_TYPE.BARRACKS:
    //             randomFeatureWeights.AddElement(TileFeatureDB.Spell_Feature, 25);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Knowledge_Feature, 45);
    //             //randomFeatureWeights.AddElement("Nothing", 25);
    //             break;
    //         case LANDMARK_TYPE.MAGE_TOWER:
    //             randomFeatureWeights.AddElement(TileFeatureDB.Spell_Feature, 45);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Knowledge_Feature, 25);
    //             //randomFeatureWeights.AddElement("Nothing", 25);
    //             break;
    //         case LANDMARK_TYPE.TEMPLE:
    //             randomFeatureWeights.AddElement(TileFeatureDB.Spell_Feature, 25);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Artifact_Feature, 35);
    //             //randomFeatureWeights.AddElement("Nothing", 25);
    //             break;
    //         case LANDMARK_TYPE.MINES:
    //             randomFeatureWeights.AddElement(TileFeatureDB.Summons_Feature, 25);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Artifact_Feature, 25);
    //             //randomFeatureWeights.AddElement("Nothing", 50);
    //             break;
    //         case LANDMARK_TYPE.FARM:    
    //             randomFeatureWeights.AddElement(TileFeatureDB.Knowledge_Feature, 25);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Summons_Feature, 25);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Artifact_Feature, 25);
    //             //randomFeatureWeights.AddElement("Nothing", 50);
    //             break;
    //         case LANDMARK_TYPE.WORKSHOP:
    //             randomFeatureWeights.AddElement(TileFeatureDB.Summons_Feature, 25);
    //             randomFeatureWeights.AddElement(TileFeatureDB.Artifact_Feature, 25);
    //             break;
    //     }
    //     if (randomFeatureWeights.GetTotalOfWeights() > 0) {
    //         string randomFeature = randomFeatureWeights.PickRandomElementGivenWeights();
    //         if (randomFeature != "Nothing") {
    //             tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(randomFeature));
    //         }
    //     }
    //
    //     //hallowed ground
    //     if (Random.Range(0, 100) < 20 && tileLocation.region.HasFeature(TileFeatureDB.Hallowed_Ground_Feature) == false) {
    //         tileLocation.region.AddFeature(LandmarkManager.Instance.CreateRegionFeature(TileFeatureDB.Hallowed_Ground_Feature));
    //     }
    // }
    #endregion
}
