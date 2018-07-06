using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingManager : MonoBehaviour {

    public static PathfindingManager Instance = null;

    [SerializeField] private AstarPath aStarPath;

    private GridGraph mainGraph;
    private List<AIPath> _allAgents;

    #region getters/setters
    public List<AIPath> allAgents {
        get { return _allAgents; }
    }
    #endregion

    private void Awake() {
        Instance = this;
        _allAgents = new List<AIPath>();
    }

    internal void CreateGrid(HexTile[,] map, int width, int height) {
        //GameObject planeGO = GameObject.Instantiate(planePrefab, mapGenerator.transform) as GameObject;
        HexTile topCornerHexTile = map[width - 1, height - 1];
        float xSize = (topCornerHexTile.transform.localPosition.x + 4f);
        float zSize = (topCornerHexTile.transform.localPosition.y + 4f);

        mainGraph = aStarPath.data.AddGraph(typeof(GridGraph)) as GridGraph;
        mainGraph.cutCorners = false;
        mainGraph.rotation = new Vector3(-90f, 0f, 0f);
        mainGraph.SetDimensions(Mathf.FloorToInt(xSize), Mathf.FloorToInt(zSize), 1f);
        mainGraph.nodeSize = 0.5f;
        mainGraph.collision.use2D = true;
        mainGraph.collision.type = ColliderType.Sphere;
        mainGraph.collision.diameter = 0.8f;
        mainGraph.collision.mask = LayerMask.GetMask("Unpassable");
        RescanGrid();
    }
    public void LoadSettings(byte[] bytes) {
        AstarPath.active.data.DeserializeGraphs(bytes);
        if (AstarPath.active.graphs.Length > 0) {
            mainGraph = AstarPath.active.graphs[0] as GridGraph;
        }
        
        //RescanGrid();
    }
    public void ClearGraphs() {
        if (mainGraph != null) {
            aStarPath.data.RemoveGraph(mainGraph);
        }
    }
    public void RescanGrid() {
        AstarPath.active.Scan(mainGraph);
    }

    public void AddAgent(AIPath agent) {
        _allAgents.Add(agent);
    }
    public void RemoveAgent(AIPath agent) {
        _allAgents.Remove(agent);
    }

    #region Monobehaviours
#if !WORLD_CREATION_TOOL
    private void Update() {
        for (int i = 0; i < _allAgents.Count; i++) {
            _allAgents[i].UpdateMe();
        }
    }
#endif
    #endregion
}
