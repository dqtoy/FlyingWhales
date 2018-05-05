using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour {

    public static PathfindingManager Instance = null;

    [SerializeField] private AstarPath aStarPath;

    private GridGraph mainGraph;

    private void Awake() {
        Instance = this;
    }

    //internal void Initialize() {
    //    aStarPath.ClearTagNames();
    //    AddNewTag(unoccupiedTag);
    //}

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
        mainGraph.collision.diameter = 0.25f;

        //AddNewTag(waterTag);
        //AddNewTag(plainTag);
        //AddNewTag(mountainTag);
        mainGraph.collision.mask = LayerMask.GetMask("Unpassable");
        RescanGrid();
    }

    public void RescanGrid() {
        AstarPath.active.Scan(mainGraph);
    }
}
