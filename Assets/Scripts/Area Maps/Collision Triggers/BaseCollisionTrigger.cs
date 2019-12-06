using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the base class for all collision triggers (Basically anything that can accept collisions) NOTE: object that can accept collisions must implement the IDamagable interface.
/// </summary>
public abstract class BaseCollisionTrigger<T> : MonoBehaviour, IBaseCollider where T: IDamageable{

    [SerializeField] private ProjectileReceiver _projectileReciever;
    [SerializeField] protected BoxCollider2D mainCollider;
    
    public T owner { get; private set; }
    public ProjectileReceiver projectileReceiver { get { return _projectileReciever; } }
    public IDamageable damageable { get { return owner; } }

    public virtual void Initialize(T owner) {
        this.owner = owner;
        this.name = owner.ToString() + " collision trigger";
        _projectileReciever.gameObject.SetActive(true);
        _projectileReciever.Initialize(owner);
    }

    public virtual void SetMainColliderState(bool state) {
        mainCollider.enabled = state;
    }
}

