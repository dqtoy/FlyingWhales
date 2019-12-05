using System.Collections;
using System.Collections.Generic;
using Traits;
using UnityEngine;

public class WallObject : AreaMapObject<WallObject>, ITraitable {
    public string name { get; private set; }
    public int currentHP { get; private set; }
    public int maxHP { get; private set; }
    public ProjectileReceiver projectileReceiver { get { return visual.collisionTrigger.projectileReceiver; } }
    public RESOURCE madeOf { get; private set; }
    public ITraitContainer traitContainer { get; private set; }
    public TraitProcessor traitProcessor { get { return TraitManager.defaultTraitProcessor; } }
    public Transform worldObject { get { return visual.transform; } }
    public LocationGridTile gridTileLocation { get; private set; }

    private WallVisual visual;

    public WallObject(LocationStructure structure, WallVisual visual) {
        this.name = $"Wall of {structure.ToString()}";
        this.visual = visual;
        this.maxHP = 100;
        this.currentHP = maxHP;
        this.madeOf = RESOURCE.WOOD;
        CreateTraitContainer();
        traitContainer.AddTrait(this, "Flammable");
        visual.Initialize(this);
    }

    #region HP
    public void AdjustHP(int amount, bool triggerDeath = false, object source = null) {
        if (currentHP <= 0) {
            return; //ignore
        }
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        if (currentHP <= 0) {
            //wall has been destroyed
            visual.UpdateWallState(this);
            Messenger.Broadcast(Signals.WALL_DESTROYED, this);
        } else if (amount < 0 && currentHP < maxHP) {
            //wall has been damaged
            visual.UpdateWallAssets(this);
            Messenger.Broadcast(Signals.WALL_DAMAGED, this);
        } else if (amount > 0 && currentHP < maxHP) {
            //wall has been partially repaired
            visual.UpdateWallState(this);
            //Messenger.Broadcast(Signals.WALL_REPAIRED, this);
        } else if (currentHP == maxHP) {
            //wall has been fully repaired
            visual.UpdateWallAssets(this);
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
        visual.UpdateWallAssets(this);
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
        areaMapVisual = visual;
    }
    protected override void OnMapObjectStateChanged() { }
    #endregion

    #region Data Setting
    public void SetTileLocation(LocationGridTile tileLocation) {
        gridTileLocation = tileLocation;
    }
    #endregion
}
