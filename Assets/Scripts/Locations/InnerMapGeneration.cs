using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;

public partial class LandmarkManager {
    
    [Header("Inner Structures")]
    [FormerlySerializedAs("innerStructurePrefab")] [SerializeField] private GameObject areaInnerStructurePrefab;
    [FormerlySerializedAs("areaMapsParent")] [SerializeField] private Transform innerMapsParent;
    [SerializeField] private GameObject regionInnerStructurePrefab;
    
    #region Area Maps
    public void GenerateAreaMap(Area area) {
        GameObject areaMapGO = GameObject.Instantiate(areaInnerStructurePrefab, innerMapsParent);
        AreaInnerTileMap areaMap = areaMapGO.GetComponent<AreaInnerTileMap>();
        areaMap.ClearAllTilemaps();

        string log = string.Empty;
        areaMap.Initialize(area);
        TownMapSettings generatedSettings = areaMap.GenerateTownMap(out log);
        areaMap.DrawMap(generatedSettings);
        areaMap.PlaceInitialStructures(area);

        areaMap.GenerateDetails();
        area.PlaceObjects();

        areaMap.OnMapGenerationFinished();
        area.OnMapGenerationFinished();
        InnerMapManager.Instance.OnCreateInnerMap(areaMap);
        TokenManager.Instance.LoadSpecialTokens(area);
        CharacterManager.Instance.PlaceInitialCharacters(area);
        area.OnAreaSetAsActive();
    }
    public void LoadAreaMap(SaveDataAreaInnerTileMap data) {
        GameObject areaMapGO = GameObject.Instantiate(areaInnerStructurePrefab, innerMapsParent);
        AreaInnerTileMap areaMap = areaMapGO.GetComponent<AreaInnerTileMap>();
        areaMap.ClearAllTilemaps();
        data.Load(areaMap);

        //Load other data
        Area area = areaMap.area;

        areaMap.OnMapGenerationFinished();
        area.OnMapGenerationFinished();
        InnerMapManager.Instance.OnCreateInnerMap(areaMap);

        area.OnAreaSetAsActive();
    }
    #endregion

    #region Region Maps
    private void GenerateRegionMap(Region region) {
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        innerTileMap.Initialize(region);
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    public void GenerateRegionInnerMaps() {
        for (var i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            var region = GridMap.Instance.allRegions[i];
            if (region.area == null) { //only generate inner maps for regions if they do not have settlements on them (Areas)
                region.GenerateStructures();
                GenerateRegionMap(region);    
            }
        }
    }
    #endregion

    
}