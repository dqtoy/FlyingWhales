using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEParticle : MonoBehaviour {

    [SerializeField] private ParticleSystem[] particles;

    public void PlaceParticleEffect(LocationGridTile tile, int range) {
        this.transform.SetParent(tile.parentAreaMap.objectsParent);
        this.transform.localPosition = tile.centeredLocalLocation;
        for (int i = 0; i < particles.Length; i++) {
            ParticleSystem ps = particles[i];
            float side = range + 0.5f;
            ParticleSystem.ShapeModule shape = ps.shape;
            shape.scale = new Vector3(side, side, side);
            if (ps.name == "North") {
                //north
                ps.transform.localPosition = new Vector3(0f, range + 0.5f, 0f);
            } else if (ps.name == "South") {
                //south
                ps.transform.localPosition = new Vector3(0f, -(range + 0.5f), 0f);
            } else if (ps.name == "West") {
                //west
                ps.transform.localPosition = new Vector3(-(range + 0.5f), 0f, 0f);
            } else if (ps.name == "East") {
                //east
                ps.transform.localPosition = new Vector3(range + 0.5f, 0f, 0f);
            }
        }
    }
}
