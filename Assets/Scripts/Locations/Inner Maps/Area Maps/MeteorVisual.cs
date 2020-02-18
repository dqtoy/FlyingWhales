using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;
using Inner_Maps;

public class MeteorVisual : MonoBehaviour {
    public ParticleSystem meteorParticle;
    public ParticleSystem[] meteorExplosionParticles;

    private LocationGridTile targetTile;
    private int radius;
    public void MeteorStrike(LocationGridTile targetTile, int radius) {
        this.targetTile = targetTile;
        this.radius = radius;
        meteorParticle.Play();
    }
    public void OnMeteorFell() {
        for (int i = 0; i < meteorExplosionParticles.Length; i++) {
            meteorExplosionParticles[i].Play();
        }
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.parentMap.GetTilesInRadius(targetTile, radius, 0, true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            traitables.AddRange(tile.GetTraitablesOnTile());
        }
        // flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        BurningSource bs = new BurningSource(InnerMapManager.Instance.currentlyShowingLocation);
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            if (traitable is TileObject obj) {
                GameManager.Instance.CreateExplodeEffectAt(obj.gridTileLocation);
                if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                    obj.AdjustHP(-obj.currentHP);
                    if (obj.gridTileLocation == null) {
                        continue; //object was destroyed, do not add burning trait
                    }
                }
            } else if (traitable is Character character) {
                GameManager.Instance.CreateExplodeEffectAt(character.gridTileLocation);
                character.AdjustHP(-(int)(character.maxHP * 0.4f), true);
            } else {
                traitable.AdjustHP(-traitable.currentHP);
            }
            if (traitable.currentHP > 0 && Random.Range(0, 100) < 60) {
                if (traitable.traitContainer.HasTrait("Flammable") &&
                    !traitable.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")) {
                    Burning burning = new Burning();
                    burning.SetSourceOfBurning(bs, traitable);
                    traitable.traitContainer.AddTrait(traitable, burning);
                }
            }
        }
        InnerMapCameraMove.Instance.ShakeCamera();
        GameManager.Instance.StartCoroutine(ExpireCoroutine(gameObject));
    }
    private IEnumerator ExpireCoroutine(GameObject go) {
        yield return new WaitForSeconds(2f);
        ObjectPoolManager.Instance.DestroyObject(go);
    }
}
