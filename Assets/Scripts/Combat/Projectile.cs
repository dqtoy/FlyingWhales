using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public Transform target;
    public Rigidbody2D rigidBody;
    public float rotateSpeed = 200f;
    public float speed = 5f;

    public IPointOfInterest targetPOI { get; private set; }

    public System.Action<IPointOfInterest, CombatState> onHitAction;

    private Vector3 _pausedVelocity;
    private float _pausedAngularVelocity;
    private CombatState createdBy;

    #region Monobehaviours
    private void OnDestroy() {
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
        Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.RemoveListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        Messenger.RemoveListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
    }
    #endregion

    public void SetTarget(Transform target, IPointOfInterest targetPOI, CombatState createdBy) {
        Vector3 diff = target.position - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        this.target = target;
        this.targetPOI = targetPOI;
        this.createdBy = createdBy;
        if (targetPOI is Character) {
            Messenger.AddListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnCharacterAreaTravelling);
            Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        } else if (targetPOI is TileObject) {
            Messenger.AddListener<TileObject, Character, LocationGridTile>(Signals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        } else if (targetPOI is SpecialToken) {
            Messenger.AddListener<SpecialToken, LocationGridTile>(Signals.ITEM_REMOVED_FROM_TILE, OnItemRemovedFromTile);
        }

        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
    }

    private void FixedUpdate() {
        if (target == null) {
            return;
        }
        if (GameManager.Instance.isPaused) {
            return;
        }
        Vector2 direction = (Vector2)target.position - rigidBody.position;
        direction.Normalize();
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        rigidBody.angularVelocity = -rotateAmount * rotateSpeed;
        rigidBody.velocity = transform.up * speed;
    }

    public void OnProjectileHit(IPointOfInterest poi) {
        //Debug.Log("Hit character " + character?.name);
        onHitAction?.Invoke(poi, createdBy);
        DestroyProjectile();
    }

    private void DestroyProjectile() {
        GameObject.Destroy(this.gameObject);
    }

    #region Listeners
    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _pausedVelocity = rigidBody.velocity;
            _pausedAngularVelocity = rigidBody.angularVelocity;
            rigidBody.velocity = Vector2.zero;
            rigidBody.angularVelocity = 0f;
            rigidBody.isKinematic = true;
        } else {
            rigidBody.isKinematic = false;
            rigidBody.velocity = _pausedVelocity;
            rigidBody.angularVelocity = _pausedAngularVelocity;
        }
    }
    private void OnCharacterAreaTravelling(Party party) {
        if (targetPOI is Character) {
            if (party.owner == targetPOI || party.carriedPOI == targetPOI) { //party.characters.Contains(targetPOI as Character)
                DestroyProjectile();
            }
        }
    }
    private void OnCharacterDied(Character character) {
        if (character == targetPOI) {
            DestroyProjectile();
        }
    }
    private void OnTileObjectRemoved(TileObject obj, Character removedBy, LocationGridTile removedFrom) {
        if (obj == targetPOI) {
            DestroyProjectile();
        }
    }
    private void OnItemRemovedFromTile(SpecialToken item, LocationGridTile removedFrom) {
        if (item == targetPOI) {
            DestroyProjectile();
        }
    }
    #endregion


}
