using ECS;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmyIcon : MonoBehaviour {

    [SerializeField] private UILabel _armyCountLbl;
    [SerializeField] private GameObject _avatarGO;
    [SerializeField] private ArmyAIPath _aiPath;
    [SerializeField] private Animator _avatarAnimator;
    [SerializeField] private AIDestinationSetter _destinationSetter;

    private Army _army;

    private BaseLandmark _targetLandmark;
    private bool _isIdle;
    private string upOrDown = "Down";
    private string previousDir;

    #region getters/setters
    public Army army {
        get { return _army; }
    }
    public ArmyAIPath aiPath {
        get { return _aiPath; }
    }
    public BaseLandmark targetLandmark {
        get { return _targetLandmark; }
    }
    public AIDestinationSetter destinationSetter {
        get { return _destinationSetter; }
    }
    #endregion

    public void SetArmy(Army army) {
        _army = army;
        _isIdle = true;

        Messenger.AddListener<bool>(Signals.PAUSED, SetMovementState);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);

        OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
        SetMovementState(GameManager.Instance.isPaused);
    }

    public void SetTarget(BaseLandmark target) {
        _targetLandmark = target;
        if (target != null) {
            _destinationSetter.target = target.tileLocation.transform;
        } else {
            _destinationSetter.target = null;
        }
    }
    public void UpdateArmyCountLabel() {
        _armyCountLbl.text = _army.armyCount.ToString();
    }

    #region Speed
    private void SetMovementState(bool state) {
        if (state) {
            _aiPath.maxSpeed = 0f;
        }
    }
    private void OnProgressionSpeedChanged(PROGRESSION_SPEED speed) {
        switch (speed) {
            case PROGRESSION_SPEED.X1:
                _aiPath.maxSpeed = 1;
                break;
            case PROGRESSION_SPEED.X2:
                _aiPath.maxSpeed = 2;
                break;
            case PROGRESSION_SPEED.X4:
                _aiPath.maxSpeed = 4;
                break;
        }
    }
    #endregion

    public void SetPosition(Vector3 position) {
        this.transform.position = position;
    }

    public void SetActionOnTargetReached(Action action) {
        _aiPath.SetActionOnTargetReached(action);
    }
    
    #region Monobehaviours
    private void LateUpdate() {
        //Debug.Log(_aiPath.velocity);
        Vector3 newPos = _aiPath.transform.localPosition;
        newPos.y += 0.36f;
        //newPos.x += 0.02f;
        _avatarGO.transform.localPosition = newPos;
    }
    void FixedUpdate() {
        //Debug.Log(_aiPath.velocity);
        if (_aiPath.velocity != Vector3.zero) {
            if (GetLeftRight() == "Left") {
                if (_avatarAnimator.transform.localScale.x < 0f) {
                    _avatarAnimator.transform.localScale = new Vector3(_avatarAnimator.transform.localScale.x * -1f, _avatarAnimator.transform.localScale.y, _avatarAnimator.transform.localScale.z);
                }
            } else {
                if (_avatarAnimator.transform.localScale.x >= 0f) {
                    _avatarAnimator.transform.localScale = new Vector3(_avatarAnimator.transform.localScale.x * -1f, _avatarAnimator.transform.localScale.y, _avatarAnimator.transform.localScale.z);
                }
            }
            upOrDown = GetUpDown();
            Walk(upOrDown);
        } else {
            Idle(upOrDown);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.tag == "Landmark") {
            _army.CollidedWithLandmark(other.GetComponent<LandmarkVisual>().landmark);
        }
    }
    private void OnDestroy() {
        SetTarget(null);
        PathfindingManager.Instance.RemoveAgent(_aiPath);
    }
    #endregion

    #region Animation
    private void Idle(string direction) {
        if (!_isIdle) {
            _isIdle = true;
            _avatarAnimator.Play("Idle_" + direction);
        }
    }
    private void Walk(string direction) {
        if (_isIdle || previousDir != direction) {
            _isIdle = false;
            _avatarAnimator.Play("Walk_" + direction);
            previousDir = direction;
        }
    }
    private string GetLeftRight() {
        //Debug.Log(_aiPath.velocity.x);
        if(_aiPath.velocity.x <= 0f) {
            return "Left";
        } else {
            return "Right";
        }
    }
    private string GetUpDown() {
        if (_aiPath.velocity.y <= 0f) {
            return "Down";
        } else {
            return "Up";
        }
    }
    #endregion
}
