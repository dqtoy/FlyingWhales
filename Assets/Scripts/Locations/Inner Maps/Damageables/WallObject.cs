using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Traits;
using UnityEngine;

public class WallObject : MapObject<WallObject>, ITraitable {
    public string name { get; private set; }
    public int currentHP { get; private set; }
    public int maxHP { get; private set; }
    public ProjectileReceiver projectileReceiver { get { return _visual.collisionTrigger.projectileReceiver; } }
    public RESOURCE madeOf { get; private set; }
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor => TraitManager.defaultTraitProcessor;
    public Transform worldObject => _visual.transform;
    public LocationGridTile gridTileLocation { get; private set; }
    public override MapObjectVisual<WallObject> mapVisual => _visual;
    public BaseMapObjectVisual mapObjectVisual => mapVisual;
    private WallVisual _visual;

    public WallObject(LocationStructure structure, WallVisual visual) {
        this.name = $"Wall of {structure.ToString()}";
        this._visual = visual;
        this.maxHP = 100;
        this.currentHP = maxHP;
        this.madeOf = RESOURCE.WOOD;
        CreateTraitContainer();
        traitContainer.AddTrait(this, "Flammable");
        visual.Initialize(this);
    }

    #region HP
    public void AdjustHP(int amount, bool triggerDeath = false, object source = null) {
        if (currentHP <= 0 && amount < 0) {
            return; //ignore
        }
        if (amount < 0) {
            GameManager.Instance.CreateHitEffectAt(this);
        }
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (currentHP <= 0) {
            //wall has been destroyed
            _visual.UpdateWallState(this);
            Messenger.Broadcast(Signals.WALL_DESTROYED, this);
            gridTileLocation.CreateSeamlessEdgesForSelfAndNeighbours();
        } else if (amount < 0 && currentHP < maxHP) {
            //wall has been damaged
            _visual.UpdateWallAssets(this);
            Messenger.Broadcast(Signals.WALL_DAMAGED, this);
        } else if (amount > 0 && currentHP < maxHP) {
            //wall has been partially repaired
            _visual.UpdateWallState(this);
            //Messenger.Broadcast(Signals.WALL_REPAIRED, this);
        } else if (currentHP == maxHP) {
            //wall has been fully repaired
            _visual.UpdateWallAssets(this);
            _visual.UpdateWallState(this);
            Messenger.Broadcast(Signals.WALL_REPAIRED, this);
        }
    }
    public void OnHitByAttackFrom(Character characterThatAttacked, CombatState state, ref string attackSummary) {
        GameManager.Instance.CreateHitEffectAt(this);
        this.AdjustHP(-characterThatAttacked.attackPower, source: characterThatAttacked);
    }
    #endregion

    #region Resource
    internal void ChangeResourceMadeOf(RESOURCE madeOf) {
        this.madeOf = madeOf;
        _visual.UpdateWallAssets(this);
        //TODO: Update HP based on new resource
        switch (madeOf) {
            case RESOURCE.WOOD:
                traitContainer.AddTrait(this, "Flammable");
                break;
            case RESOURCE.STONE:
            case RESOURCE.METAL:
                traitContainer.RemoveTrait(this, "Flammable");
                break;
        }
    }
    public void CreateTraitContainer() {
        traitContainer = new TraitContainer();
    }
    #endregion

    #region Area Map Object
    protected override void CreateAreaMapGameObject() {
        mapVisual = _visual;
    }
    protected override void OnMapObjectStateChanged() { }
    public void SetGridTileLocation(LocationGridTile tile) {
        gridTileLocation = tile;
    }
    #endregion

    public bool CanBeDamaged() {
        return mapObjectState != MAP_OBJECT_STATE.UNBUILT;
    }
}
