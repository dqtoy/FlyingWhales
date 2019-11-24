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
    void Start() {
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
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
    public void RescanGrid(GridGraph graph) {
        AstarPath.active.Scan(graph);
    }
    public void AddAgent(CharacterAIPath agent) {
        _allAgents.Add(agent);
    }
    public void RemoveAgent(CharacterAIPath agent) {
        _allAgents.Remove(agent);
    }

    private void OnGamePaused(bool state) {
        if (state) {
            for (int i = 0; i < _allAgents.Count; i++) {
                CharacterAIPath currentAI = _allAgents[i];
                currentAI.marker.PauseAnimation();
            }
        } else {
            for (int i = 0; i < _allAgents.Count; i++) {
                CharacterAIPath currentAI = _allAgents[i];
                currentAI.marker.UnpauseAnimation();
            }
        }
    }
    #region Monobehaviours
#if !WORLD_CREATION_TOOL
    private void Update() {
        for (int i = 0; i < _allAgents.Count; i++) {
            CharacterAIPath currentAI = _allAgents[i];
            currentAI.UpdateMe();
            //if (currentAI is CharacterAIPath && (currentAI as CharacterAIPath).icon.pathfinder.isWaitingForPathCalculation) {
            //    (currentAI as CharacterAIPath).icon.pathfinder.UpdateMe();
            //}
        }
    }
#endif
    #endregion
}
