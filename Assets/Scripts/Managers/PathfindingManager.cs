using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour {

    public static PathfindingManager Instance = null;

    [SerializeField] private AstarPath aStarPath;

    private GridGraph mainGraph;
    private List<CharacterAIPath> _allAgents;

    #region getters/setters
    public List<CharacterAIPath> allAgents {
        get { return _allAgents; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        _allAgents = new List<CharacterAIPath>();
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

        //mainGraph.isometricAngle = 90 - Mathf.Atan(1 / Mathf.Sqrt(2)) * Mathf.Rad2Deg;
        //mainGraph.aspectRatio = 1;
        //mainGraph.uniformEdgeCosts = true;
        //mainGraph.neighbours = NumNeighbours.Six;
        mainGraph.cutCorners = false;
        mainGraph.rotation = new Vector3(-90f, 0f, 0f);
        mainGraph.SetDimensions(Mathf.FloorToInt(xSize), Mathf.FloorToInt(zSize), 1f);
        mainGraph.nodeSize = 0.5f;
        //mainGraph.SetDimensions((int)GridMap.Instance.width + 35, (int)GridMap.Instance.height, 1f);
        mainGraph.collision.use2D = true;
        mainGraph.collision.type = ColliderType.Sphere;
        mainGraph.collision.diameter = 1f;

        //AddNewTag(waterTag);
        //AddNewTag(plainTag);
        //AddNewTag(mountainTag);
        mainGraph.collision.mask = LayerMask.GetMask("Unpassable");
        RescanGrid();
    }

    public void RescanGrid() {
        AstarPath.active.Scan(mainGraph);
    }

    public void AddAgent(CharacterAIPath agent) {
        _allAgents.Add(agent);
    }

    #region Monobehaviours
    private void Update() {
        for (int i = 0; i < _allAgents.Count; i++) {
            _allAgents[i].UpdateMe();
        }
    }
    #endregion
}
