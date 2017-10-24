using UnityEngine;
using System.Collections;

using Pathfinding;

public class PathfindingManager : MonoBehaviour {

    public static PathfindingManager Instance = null;

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private AstarPath aStarPath;

    private GridGraph mainGraph;
    //public static string waterTag = "Water";
    //public static string plainTag = "Plain";
    //public static string mountainTag = "Mountain";
    public static string unoccupiedTag = "Unoccupied";
    public static int unoccupiedTagIndex = 0;

    private void Awake() {
        Instance = this;
    }

    internal void Initialize() {
        aStarPath.ClearTagNames();
        AddNewTag(unoccupiedTag);
    }

    internal void CreateGrid() {
        //GameObject planeGO = GameObject.Instantiate(planePrefab, mapGenerator.transform) as GameObject;
        HexTile topCornerHexTile = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height - 1];
        float xSize = (topCornerHexTile.transform.localPosition.x + 4f);
        float zSize = (topCornerHexTile.transform.localPosition.y + 4f);
        //planeGO.transform.localScale = new Vector3(xSize, 1f, zSize);
        //planeGO.transform.localPosition = new Vector3(-0.6f, -0.6f, 2f);
        //aStarPath.gra
        mainGraph = aStarPath.data.AddGraph(typeof(GridGraph)) as GridGraph;
        mainGraph.rotation = new Vector3(-90f, 0f, 0f);
        mainGraph.SetDimensions(Mathf.FloorToInt(xSize), Mathf.FloorToInt(zSize), 1f);
        mainGraph.collision.use2D = true;
        mainGraph.collision.type = ColliderType.Sphere;
        mainGraph.collision.diameter = 1f;

        //AddNewTag(waterTag);
        //AddNewTag(plainTag);
        //AddNewTag(mountainTag);
        mainGraph.collision.mask = LayerMask.GetMask(new string[] { "Pathfinding_Mountains", "Pathfinding_Water" });
        RescanGrid();
    }

    public void RescanGrid() {
        aStarPath.Scan(mainGraph);
    }

    public void RescanSpecificPortion(GraphUpdateObject guo) {
        AstarPath.active.UpdateGraphs(guo);
    }

    internal int AddNewTag(string newTag) {
        return aStarPath.AddTagName(newTag);
    }

    internal void RemoveTag(string tagToRemove) {
        aStarPath.RemoveTagName(tagToRemove);
    }
}
