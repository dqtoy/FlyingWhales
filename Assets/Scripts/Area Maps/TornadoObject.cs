﻿using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TornadoObject : MonoBehaviour {

    [SerializeField] private ParticleSystem[] particles;

    [Header("Movement")]
    // Movement speed in units per second.
    [SerializeField] private float baseSpeed = 1.0F;
    // Time when the movement started.
    private float startTime;
    // Total distance between the markers.
    private float journeyLength;
    private Vector3 startPosition;

    private float speed;
    private bool isSpawned;
    private int radius;

    #region getters/setters
    public LocationGridTile gridTileLocation {
        get { return GetLocationGridTileByXY(Mathf.FloorToInt(this.transform.localPosition.x), Mathf.FloorToInt(this.transform.localPosition.y)); }
    }
    public LocationGridTile destinationTile {
        get { return _destinationTile; }
        set {
            _destinationTile = value;
        }
    }
    #endregion

    private Area areaLocation;
    private LocationGridTile _destinationTile;

    public void Initialize(LocationGridTile location, float size, int durationInTicks) {
        this.transform.localPosition = location.centeredLocalLocation;
        this.radius = (int)size;
        areaLocation = location.parentAreaMap.area;
        float scale = size / 10f;
        for (int i = 0; i < particles.Length; i++) {
            particles[i].transform.localScale = new Vector3(scale, scale, scale);
        }
        isSpawned = true;
        GoToRandomTileInRadius();
        SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(durationInTicks), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
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
        UpdateSpeed();
        RecalculatePathingValues();
    }
    private void RecalculatePathingValues() {
        // Keep a note of the time the movement started.
        startTime = Time.time;
        startPosition = this.transform.position;

        // Calculate the journey length.
        journeyLength = Vector3.Distance(this.transform.position, destinationTile.centeredWorldLocation);
    }
    private void UpdateSpeed() {
        speed = baseSpeed;
        if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
            speed *= 1.5f;
        } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
            speed *= 2f;
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED prog) {
        UpdateSpeed();
        RecalculatePathingValues();
    }
    private void OnGamePaused(bool paused) {
        UpdateSpeed();
        RecalculatePathingValues();
    }
    #endregion

    private void Expire() {
        isSpawned = false;
        destinationTile = null;
        areaLocation = null;
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }

    #region Monobehaviours
    private void Update() {
        if (destinationTile == null) {
            return;
        }
        if (GameManager.Instance.isPaused) {
            RecalculatePathingValues();
            return;
        }
        // Distance moved equals elapsed time times speed..
        float distCovered = (Time.time - startTime) * speed;

        // Fraction of journey completed equals current distance divided by total distance.
        float fractionOfJourney = distCovered / journeyLength;

        // Set our position as a fraction of the distance between the markers.
        transform.position = Vector3.Lerp(startPosition, destinationTile.centeredWorldLocation, fractionOfJourney);
        if (Mathf.Approximately(transform.position.x, destinationTile.centeredWorldLocation.x) && Mathf.Approximately(transform.position.y, destinationTile.centeredWorldLocation.y)) {
            destinationTile = null;
            GoToRandomTileInRadius();
        }
    }
    #endregion

    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
        List<LocationGridTile> tiles = gridTileLocation.parentAreaMap.GetTilesInRadius(gridTileLocation, radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            List<IDamageable> damageables = tile.GetDamageablesOnTile();
            for (int j = 0; j < damageables.Count; j++) {
                IDamageable damageable = damageables[j];
                if (damageable.CanBeDamaged()) {
                    damageable.AdjustHP(-(int)(damageable.maxHP * 0.35f), true, this);
                }
            }
        }
    }

}
