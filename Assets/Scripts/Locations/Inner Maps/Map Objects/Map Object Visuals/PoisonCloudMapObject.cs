using System.Collections.Generic;
using DG.Tweening;
using Inner_Maps;
using Traits;
using UnityEngine;

public class PoisonCloudMapObject : MovingMapObjectVisual<TileObject> {
    
    public bool isSpawned { get; private set; }
    private string _expiryKey;
    private PoisonCloudTileObject _poisonCloudTileObject;
    private Tweener _movement;
    private List<ITraitable> _objsInRange;
    
    #region Abstract Members Implementation
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) { }
    public override bool IsMapObjectMenuVisible() {
        return true;
    }
    public override void UpdateTileObjectVisual(TileObject obj) { }
    public override void UpdateCollidersState(TileObject obj) { }
    #endregion

    private void Awake() {
        collisionTrigger = transform.GetComponentInChildren<TileObjectCollisionTrigger>();
    }
    
    #region Overrides
    public override void Initialize(TileObject obj) {
        base.Initialize(obj);
        _poisonCloudTileObject = obj as PoisonCloudTileObject;
        _objsInRange = new List<ITraitable>();
    }
    public override void PlaceObjectAt(LocationGridTile tile) {
        base.PlaceObjectAt(tile);
        MoveToRandomDirection();
        OnGamePaused(GameManager.Instance.isPaused);
        _expiryKey = SchedulingManager.Instance.AddEntry(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(2, 6))), Expire, this);
        Messenger.AddListener(Signals.TICK_ENDED, PerTick);
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.AddListener<ITraitable, Trait>(Signals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
        // Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);
        // Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemovedFromTile);
        isSpawned = true;
    }
    public override void Reset() {
        base.Reset();
        Messenger.RemoveListener(Signals.TICK_ENDED, PerTick);
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    protected override void Update() {
        base.Update();
        if (isSpawned && gridTileLocation == null) {
            Expire();
        }
    }
    #endregion

    #region Movement
    private void MoveToRandomDirection() {
        Vector3 direction = (new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f))).normalized;
        _movement = transform.DOMove(direction, 1).SetSpeedBased(true);
    }
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _movement.Pause();    
        } else {
            _movement.Play();
        }
    }
    #endregion

    #region Effects
    private void PerTick() {
        if (Random.Range(0, 100) < 15 && _objsInRange.Count > 0) {
            ITraitable traitable = UtilityScripts.CollectionUtilities.GetRandomElement(_objsInRange);
            traitable.traitContainer.AddTrait(traitable, "Poisoned");
        }
    }
    private void Explode() {
        List<ITraitable> affectedObjects =
            UtilityScripts.GameUtilities.GetTraitableDiamondTilesFromRadius(_mapLocation.innerMap, gridTileLocation.localPlace, 5);
        for (int i = 0; i < affectedObjects.Count; i++) {
            ITraitable obj = affectedObjects[i];
            obj.traitContainer.AddTrait(obj, "Poisoned");
        }
        Expire();
    }
    private void OnTraitableGainedTrait(ITraitable traitable, Trait trait) {
        if (trait is Burning && _objsInRange.Contains(traitable)) {
            Explode();
        }
    }
    #endregion
    
    #region Triggers
    public void OnTriggerEnter2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable) { 
            AddObject(traitable);   
        }
    }
    public void OnTriggerExit2D(Collider2D collision) {
        IBaseCollider collidedWith = collision.gameObject.GetComponent<IBaseCollider>();
        if (collidedWith != null && collidedWith.damageable is ITraitable traitable) { 
            AddObject(traitable);   
        }
    }
    #endregion
    
    #region POI's
    private void AddObject(ITraitable obj) {
        if (!_objsInRange.Contains(obj)) {
            _objsInRange.Add(obj);
            OnAddPOI(obj);
        }
    }
    private void RemoveObject(ITraitable obj) {
        _objsInRange.Remove(obj);
    }
    private void OnAddPOI(ITraitable obj) {
        if (obj.traitContainer.GetNormalTrait<Trait>("Burning") != null) {
            Explode();
        }
    }
    #endregion
    
    #region Expiration
    private void Expire() {
        SchedulingManager.Instance.RemoveSpecificEntry(_expiryKey);
        ObjectPoolManager.Instance.DestroyObject(this);
    }
    #endregion
    
        
}
