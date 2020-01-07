using System.Collections;
using Inner_Maps;
using UnityEngine;
using UnityEngine.Serialization;

public partial class LandmarkManager {
    
    [Header("Inner Structures")]
    [FormerlySerializedAs("innerStructurePrefab")] [SerializeField] private GameObject areaInnerStructurePrefab;
    [FormerlySerializedAs("areaMapsParent")] [SerializeField] private Transform innerMapsParent;
    [SerializeField] private GameObject regionInnerStructurePrefab;
    
    #region Area Maps
    public IEnumerator GenerateAreaMap(Area area) {
        GameObject areaMapGO = GameObject.Instantiate(areaInnerStructurePrefab, innerMapsParent);
        AreaInnerTileMap areaMap = areaMapGO.GetComponent<AreaInnerTileMap>();
        areaMap.ClearAllTilemaps();

        string log = string.Empty;
        areaMap.Initialize(area);
        TownMapSettings generatedSettings = areaMap.GenerateTownMap(out log);
        yield return StartCoroutine(areaMap.DrawMap(generatedSettings));
        yield return StartCoroutine(areaMap.PlaceInitialStructures(area));

        yield return StartCoroutine(areaMap.GenerateDetails());
        yield return StartCoroutine(area.PlaceObjects());

        areaMap.OnMapGenerationFinished();
        //area.OnMapGenerationFinished();
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
        //area.OnMapGenerationFinished();
        InnerMapManager.Instance.OnCreateInnerMap(areaMap);

        area.OnAreaSetAsActive();
    }
    #endregion

    #region Region Maps
    public IEnumerator GenerateRegionMap(Region region, int mapWidth, int mapHeight) {
        GameObject regionMapGo = Instantiate(regionInnerStructurePrefab, innerMapsParent);
        RegionInnerTileMap innerTileMap = regionMapGo.GetComponent<RegionInnerTileMap>();
        innerTileMap.Initialize(region);
        region.GenerateStructures();
        yield return StartCoroutine(innerTileMap.GenerateMap(region, mapWidth, mapHeight));
        InnerMapManager.Instance.OnCreateInnerMap(innerTileMap);
    }
    #endregion

    public void MakeAllRegionsAwareOfEachOther() {
        for (var i = 0; i < GridMap.Instance.allRegions.Length; i++) {
            var currRegion = GridMap.Instance.allRegions[i];
            for (var j = 0; j < GridMap.Instance.allRegions.Length; j++) {
                var otherRegion = GridMap.Instance.allRegions[j];
                if (currRegion != otherRegion && otherRegion.regionTileObject != null) {
                    currRegion.AddAwareness(otherRegion.regionTileObject);
                }
            }
        }
    }
}