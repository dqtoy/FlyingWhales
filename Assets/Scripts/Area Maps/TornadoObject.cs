using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoObject : MonoBehaviour {

    [SerializeField] private ParticleSystem[] particles;
    [SerializeField] private Rigidbody2D rigidBody;

    #region getters/setters
    public LocationGridTile gridTileLocation {
        get { return GetLocationGridTileByXY(Mathf.FloorToInt(this.transform.localPosition.x), Mathf.FloorToInt(this.transform.localPosition.y)); }
    }
    #endregion

    private Area areaLocation;
    private LocationGridTile destinationTile;

    public void Initialize(LocationGridTile location, float size) {
        this.transform.localPosition = location.centeredLocalLocation;
        areaLocation = location.parentAreaMap.area;
        float scale = size / 10f;
        for (int i = 0; i < particles.Length; i++) {
            particles[i].transform.localScale = new Vector3(scale, scale, scale);
        }
        GoToRandomTileInRadius();
    }

    private void GoToRandomTileInRadius() {
        List<LocationGridTile> tilesInRadius = gridTileLocation.parentAreaMap.GetTilesInRadius(gridTileLocation, 8, 6, false, true);
        LocationGridTile chosen = tilesInRadius[Random.Range(0, tilesInRadius.Count)];
        GoTo(chosen);
    }

    public LocationGridTile GetLocationGridTileByXY(int x, int y, bool throwOnException = true) {
        try {
            if (throwOnException) {
                return areaLocation.areaMap.map[x, y];
            } else {
                if (Utilities.IsInRange(x, 0, areaLocation.areaMap.map.GetUpperBound(0) + 1) &&
                    Utilities.IsInRange(y, 0, areaLocation.areaMap.map.GetUpperBound(1) + 1)) {
                    return areaLocation.areaMap.map[x, y];
                }
                return null;
            }
        } catch (System.Exception e) {
            throw new System.Exception(e.Message + "\n " + this.name + "(" + x.ToString() + ", " + y.ToString() + ")");
        }

    }

    #region Pathfinding
    public void GoTo(LocationGridTile destinationTile) {
        this.destinationTile = destinationTile;
    }
    #endregion

    private void FixedUpdate() {
        if (destinationTile == null) {
            return;
        }
        if (GameManager.Instance.isPaused) {
            return;
        }
        if (Mathf.Approximately(this.transform.position.x, destinationTile.centeredWorldLocation.x) && Mathf.Approximately(this.transform.position.y, destinationTile.centeredWorldLocation.y)) {
            destinationTile = null;
            GoToRandomTileInRadius();
            return;
        }
        Vector2 direction = (Vector2)destinationTile.centeredWorldLocation - rigidBody.position;
        direction.Normalize();
        rigidBody.velocity = transform.up * 1f;
    }

}
