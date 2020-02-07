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
        List<ITraitable> flammables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.parentMap.GetTilesInRadius(targetTile, radius, 0, true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            flammables.AddRange(tile.GetTraitablesOnTileWithTrait("Flammable"));
        }
        flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        BurningSource bs = new BurningSource(InnerMapManager.Instance.currentlyShowingLocation);
        for (int i = 0; i < flammables.Count; i++) {
            ITraitable flammable = flammables[i];
            if (flammable is TileObject) {
                TileObject obj = flammable as TileObject;
                GameManager.Instance.CreateExplodeEffectAt(obj.gridTileLocation);
                if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                    obj.AdjustHP(-obj.currentHP);
                    if (obj.gridTileLocation == null) {
                        continue; //object was destroyed, do not add burning trait
                    }
                }
            } else if (flammable is SpecialToken) {
                SpecialToken token = flammable as SpecialToken;
                GameManager.Instance.CreateExplodeEffectAt(token.gridTileLocation);
                token.AdjustHP(-token.currentHP);
                if (token.gridTileLocation == null) {
                    continue; //object was destroyed, do not add burning trait
                }
            } else if (flammable is Character) {
                Character character = flammable as Character;
                GameManager.Instance.CreateExplodeEffectAt(character.gridTileLocation);
                character.AdjustHP(-(int)(character.maxHP * 0.4f), true);
            }
            if (Random.Range(0, 100) < 60) {
                Burning burning = new Burning();
                burning.SetSourceOfBurning(bs, flammable);
                flammable.traitContainer.AddTrait(flammable, burning);
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
