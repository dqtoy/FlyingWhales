using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileReceiver : MonoBehaviour {

    protected IDamageable owner { get; private set; }
    private Collider2D _collider;

    public void Initialize(IDamageable owner) {
        this.owner = owner;
        _collider = gameObject.GetComponent<Collider2D>();
    }

    public void SetColliderState(bool state) {
        _collider.enabled = state;
    }
    
    public abstract void OnTriggerEnter2D(Collider2D collision);
}
