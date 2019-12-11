using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class TornadoVisual : MapObjectVisual<TileObject> {

    [SerializeField] private ParticleSystem[] particles;

    [Header("Movement")]
    [SerializeField] private float baseSpeed = 1.0F; // Movement speed in units per second.
    private float startTime;  // Time when the movement started.
    private float journeyLength; // Total distance between the markers.
    private Vector3 startPosition;

    public bool isSpawned { get; private set; }
    private float speed;
    private int radius;
    private List<IDamageable> damagablesInTornado;
    private Area areaLocation;
    private LocationGridTile _destinationTile;
    private TornadoTileObject tornado;

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

    public override void Initialize(TileObject poi) {
        this.tornado = poi as TornadoTileObject;
        this.transform.localPosition = poi.gridTileLocation.centeredLocalLocation;
        this.radius = tornado.radius;
        areaLocation = poi.gridTileLocation.structure.areaLocation;
        float scale = tornado.radius / 5f;
        for (int i = 0; i < particles.Length; i++) {
            particles[i].transform.localScale = new Vector3(scale, scale, scale);
        }
        damagablesInTornado = new List<IDamageable>();
        collisionTrigger = transform.GetComponentInChildren<TileObjectCollisionTrigger>();
    }
    public override void UpdateTileObjectVisual(TileObject obj) { }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    private void GoToRandomTileInRadius() {
        List<LocationGridTile> tilesInRadius = gridTileLocation.parentMap.GetTilesInRadius(gridTileLocation, 8, 6, false, true);
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

    public override void PlaceObjectAt(LocationGridTile tile) {
        this.transform.SetParent(tile.parentMap.objectsParent);
        Vector3 worldPos = tile.centeredWorldLocation;
        this.transform.position = worldPos;

        GoToRandomTileInRadius();
        SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(tornado.durationInTicks), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        isSpawned = true;
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
        tornado.OnExpire();
        ObjectPoolManager.Instance.DestroyObject(this.gameObject);
    }

    public override void Reset() {
        base.Reset();
        isSpawned = false;
        destinationTile = null;
        areaLocation = null;
        journeyLength = 0f;
        startPosition = Vector3.zero;
        startTime = 0f;
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
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
    void FixedUpdate() {
        for (int i = 0; i < damagablesInTornado.Count; i++) {
            IDamageable damageable = damagablesInTornado[i];
            if (damageable.mapObjectVisual != null && damageable.CanBeDamaged()) {
                iTween.ShakeRotation(damageable.mapObjectVisual.gameObjectVisual, new Vector3(5f, 5f, 5f), 0.5f);
            }
        }
    }
    #endregion

    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null) {
            if (collidedWith.damageable == null) {
                throw new System.Exception("Tornado collided with " + collidedWith.ToString() + " but damagable was null!");
            }
            Debug.Log("Tornado collision enter with " + collidedWith.damageable.name);
            AddDamageable(collidedWith.damageable);
        }

    }
    public void OnTriggerExit2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null) {
            Debug.Log("Tornado collision exit with " + collidedWith.damageable.name);
            RemoveDamageable(collidedWith.damageable);
        }
    }
    #endregion

    #region POI's
    private void AddDamageable(IDamageable poi) {
        if (!damagablesInTornado.Contains(poi)) {
            damagablesInTornado.Add(poi);
            OnAddPOIActions(poi);
        }
    }
    private void RemoveDamageable(IDamageable poi) {
        if (damagablesInTornado.Remove(poi)) {

        }
    }
    private void OnAddPOIActions(IDamageable poi) {
        //DealDamage(poi);
    }
    #endregion

    private void PerTick() {
        if (isSpawned == false) {
            return;
        }
        List<LocationGridTile> tiles = gridTileLocation.parentMap.GetTilesInRadius(gridTileLocation, radius, includeCenterTile: true, includeTilesInDifferentStructure: true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            if (tile.genericTileObject.CanBeDamaged()) {
                tile.genericTileObject.AdjustHP(-(int)(tile.genericTileObject.maxHP * 0.35f), true, this);
            }
        }
        for (int i = 0; i < damagablesInTornado.Count; i++) {
            IDamageable damageable = damagablesInTornado[i];
            if (damageable.mapObjectVisual != null) {
                Vector3 distance = transform.position - damageable.mapObjectVisual.gameObjectVisual.transform.position;
                if (distance.magnitude < 2f) {
                    DealDamage(damageable);
                } else {
                    //check for suck in
                    TrySuckIn(damageable);
                }
            }
        }
    }
    private void DealDamage(IDamageable damageable) {
        if (damageable.CanBeDamaged()) {
            damageable.AdjustHP(-(int)(damageable.maxHP * 0.35f), true, this);
        }
    }
    private void TrySuckIn(IDamageable damageable) {
        if (CanBeSuckedIn(damageable) && Random.Range(0, 100) < 25) {
            FollowerComponent fc = damageable.mapObjectVisual.gameObjectVisual.AddComponent<FollowerComponent>();
            fc.SetTarget(this.transform, () => OnDamagableReachedThis(damageable));
            if (damageable is IPointOfInterest) {
                IPointOfInterest poi = damageable as IPointOfInterest;
                poi.SetPOIState(POI_STATE.INACTIVE);
            }
        }
    }
    private void OnDamagableReachedThis(IDamageable damageable) {
        damageable.AdjustHP(-damageable.maxHP);
    }

    private bool CanBeSuckedIn(IDamageable damageable) {
        return damageable.CanBeDamaged() && (damageable is GenericTileObject) == false && (damageable is Character) == false;
    }
}
