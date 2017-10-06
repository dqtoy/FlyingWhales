using UnityEngine;
using System.Collections;

public class PathfindingManager : MonoBehaviour {

    public static PathfindingManager Instance = null;

    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private GameObject planePrefab;
    [SerializeField] private AstarPath aStarPath;

    private void Awake() {
        Instance = this;
    }

    internal void Initialize() {
        GameObject planeGO = GameObject.Instantiate(planePrefab, mapGenerator.transform) as GameObject;
        HexTile topCornerHexTile = GridMap.Instance.map[(int)GridMap.Instance.width - 1, (int)GridMap.Instance.height - 1];
        float xSize = (topCornerHexTile.transform.localPosition.x + 4f) / 10f;
        float zSize = (topCornerHexTile.transform.localPosition.y + 4f) / 10f;
        planeGO.transform.localScale = new Vector3(xSize, 1f, zSize);
        planeGO.transform.localPosition = new Vector3(-0.6f, -0.6f, 2f);

        //aStarPath.gra
    }
}
