using UnityEngine;
using System.Collections;

public class ResourceLandmark : BaseLandmark {

    private MATERIAL _materialOnLandmark;
    private Materials _materialData;

    #region getters/setters
    public MATERIAL materialOnLandmark {
        get { return _materialOnLandmark; }
    }
    public Materials materialData {
        get { return _materialData; }
    }
    #endregion

    public ResourceLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType, MATERIAL materialMadeOf) : base(location, specificLandmarkType, materialMadeOf) {
        _canBeOccupied = true;
        _materialOnLandmark = Utilities.ConvertLandmarkTypeToMaterial(specificLandmarkType);
        _materialData = MaterialManager.Instance.materialsLookup[_materialOnLandmark];
		_landmarkName = Utilities.NormalizeString(_materialOnLandmark.ToString ());
    }

    #region Ownership
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        //Create structures on location
        location.CreateStructureOnTile(faction, _materialData.structure.structureType);
        //StartResourceProduction();
    }
    //public override void UnoccupyLandmark() {
    //    base.UnoccupyLandmark();
    //    StopResourceProduction();
    //}
    #endregion

    #region Resources
    //private void StartResourceProduction() {
    //    Messenger.AddListener("OnDayEnd", ProduceResources);
    //}
    //private void StopResourceProduction() {
    //    Messenger.RemoveListener("OnDayEnd", ProduceResources);
    //}
    ///*
    //A Resource can produce between 0 to 2 amount per day per citizen 
    //(capped at 5 citizen count even if there are more than 5 citizens). 
    //A Resource Landmark can only keep up to 300 of the Resource it produces.
    //     */
    //private void ProduceResources() {
    //    int materialQuantity = _materialsInventory[_materialOnLandmark].totalCount;
    //    if(materialQuantity < 300) {
    //        //TODO: Maybe add this to a separate thread, for optimization
    //        int workingCivilians = Mathf.Min(civilians, 5);
    //        int producedResourceQuantity = 0;
    //        for (int i = 0; i < workingCivilians; i++) {
    //            producedResourceQuantity += Random.Range(0, 3);
    //        }
    //        AdjustMaterial(materialOnLandmark, producedResourceQuantity);
    //    }
    //}
    //internal override void AdjustMaterial(MATERIAL material, int amount) {
    //    _materialsInventory[material].count += amount;
    //    _materialsInventory[material].count = Mathf.Clamp(_materialsInventory[material].count, 0, _materialsInventory[material].maximumStorage);
    //}
    #endregion
}
