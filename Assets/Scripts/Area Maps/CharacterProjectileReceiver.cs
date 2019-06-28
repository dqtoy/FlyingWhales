using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterProjectileReceiver : MonoBehaviour {

    [SerializeField] private CharacterMarker parentMarker;

    public void OnTriggerEnter2D(Collider2D collision) {
        Projectile collidedWith = collision.gameObject.GetComponent<Projectile>();
        if (collidedWith != null) {
            collidedWith.ProjectileHit(parentMarker.character);
            //collidedWith.ProjectileHit(null);
        }
    }
}
