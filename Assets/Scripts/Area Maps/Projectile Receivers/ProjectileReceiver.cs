using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileReceiver : MonoBehaviour {

    protected IDamageable owner { get; private set; }

    public void Initialize(IDamageable owner) {
        this.owner = owner;
    }

    public abstract void OnTriggerEnter2D(Collider2D collision);
}
