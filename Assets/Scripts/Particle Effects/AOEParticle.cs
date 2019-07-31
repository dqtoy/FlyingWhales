using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEParticle : MonoBehaviour {

    [SerializeField] private ParticleSystem[] particles;

    public void PlaceParticleEffect(LocationGridTile tile, int range, bool isAutoDestroy) {
        this.transform.SetParent(tile.parentAreaMap.objectsParent);
        this.transform.localPosition = tile.centeredLocalLocation;
        for (int i = 0; i < particles.Length; i++) {
            ParticleSystem ps = particles[i];
            if (!isAutoDestroy) {
                ParticleSystem.MainModule main = ps.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(range + 2, range + 4);
            }
            float size = 1 + (2 * range);
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.scale = new Vector3(size, size, size);
        }
    }
}
