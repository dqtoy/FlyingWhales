using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileReceiver : MonoBehaviour {

    private IPointOfInterest owner;

    public void Initialize(IPointOfInterest owner) {
        this.owner = owner;
    }

    public void OnTriggerEnter2D(Collider2D collision) {
        Projectile projectileThatHit = collision.gameObject.GetComponent<Projectile>();
        if (projectileThatHit != null && projectileThatHit.targetPOI == owner) { //added checker to only register hit if the object that triggered this is the actual target of the projectile.
            projectileThatHit.OnProjectileHit(owner);
            //collidedWith.ProjectileHit(null);
        }
    }
}
