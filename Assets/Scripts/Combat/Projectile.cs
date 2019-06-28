using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    public Transform target;
    public Rigidbody2D rigidBody;
    public float rotateSpeed = 200f;
    public float speed = 5f;

    public System.Action<Character> onHitAction;

    private Vector3 _pausedVelocity;
    private float _pausedAngularVelocity;


    [ContextMenu("TargetTest")]
    public void TargetTest() {
        SetTarget(GameObject.FindGameObjectWithTag("Player").transform);
    }


    #region Monobehaviours
    private void Awake() {
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    private void OnEnable() {
        Messenger.AddListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    private void OnDisable() {
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    private void OnDestroy() {
        Messenger.RemoveListener<bool>(Signals.PAUSED, OnGamePaused);
    }
    #endregion

    public void SetTarget(Transform target) {
        Vector3 diff = target.position - transform.position;
        diff.Normalize();
        float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, rot_z - 90);
        this.target = target;
    }

    private void FixedUpdate() {
        if (target == null) {
            return;
        }
        Vector2 direction = (Vector2)target.position - rigidBody.position;
        direction.Normalize();
        float rotateAmount = Vector3.Cross(direction, transform.up).z;
        rigidBody.angularVelocity = -rotateAmount * rotateSpeed;
        rigidBody.velocity = transform.up * speed;
    }

    public void ProjectileHit(Character character) {
        Debug.Log("Hit character " + character?.name);
        onHitAction?.Invoke(character);
        GameObject.Destroy(this.gameObject);
    }


    private void OnGamePaused(bool isPaused) {
        if (isPaused) {
            _pausedVelocity = rigidBody.velocity;
            _pausedAngularVelocity = rigidBody.angularVelocity;
            rigidBody.isKinematic = true;
        } else {
            rigidBody.isKinematic = false;
            rigidBody.velocity = _pausedVelocity;
            rigidBody.angularVelocity = _pausedAngularVelocity;
        }
    }

}
