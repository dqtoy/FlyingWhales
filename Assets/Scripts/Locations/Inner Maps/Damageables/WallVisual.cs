using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

public class WallVisual : MapObjectVisual<StructureWallObject> {

    private SpriteRenderer[] spriteRenderers;

    void Awake() {
        spriteRenderers = this.transform.GetComponentsInChildren<SpriteRenderer>();
        collisionTrigger = transform.GetComponentInChildren<WallObjectCollisionTrigger>();
        collisionTrigger.gameObject.SetActive(false);
    }

    public override void Initialize(StructureWallObject obj) {
        collisionTrigger.Initialize(obj);
        collisionTrigger.gameObject.SetActive(true);
        UpdateWallAssets(obj);
    }
    public void UpdateWallAssets(StructureWallObject structureWallObject) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            //update the sprite given the wall objects material, and if it is damaged or not.
            WallAsset wallAsset = InnerMapManager.Instance.GetWallAsset(structureWallObject.madeOf, spriteRenderer.sprite.name);
            if (structureWallObject.currentHP == structureWallObject.maxHP) {
                spriteRenderer.sprite = wallAsset.undamaged;
            } else {
                spriteRenderer.sprite = wallAsset.damaged;
            }
            
        }
    }
    public void UpdateSortingOrders(int sortingOrder) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            if (spriteRenderer.sprite.name.Contains("corner")) {
                //all corners should be 1 layer above walls
                spriteRenderer.sortingOrder = sortingOrder + 1;
            } else {
                spriteRenderer.sortingOrder = sortingOrder;
            }
            
        }
    }
    public void UpdateWallState(StructureWallObject structureWallObject) {
        if (structureWallObject.currentHP == 0) {
            //wall is destroyed disable gameobject
            this.gameObject.SetActive(false);
        } else {
            //wall is not destroyed enable game object
            this.gameObject.SetActive(true);
        }
    }
    public void SetWallColor(Color color) {
        for (int i = 0; i < spriteRenderers.Length; i++) {
            SpriteRenderer spriteRenderer = spriteRenderers[i];
            spriteRenderer.color = color;
        }
    }

    public override void Reset() {
        base.Reset();
        collisionTrigger.gameObject.SetActive(false);
    }

    public override void UpdateTileObjectVisual(StructureWallObject obj) {
        throw new System.NotImplementedException();
    }
    public override void ApplyFurnitureSettings(FurnitureSetting furnitureSetting) {
        throw new System.NotImplementedException();
    }
    public override bool IsMapObjectMenuVisible() {
        return true; //always true so that this is skipped
    }
    public override void UpdateCollidersState(StructureWallObject obj) { }
}
