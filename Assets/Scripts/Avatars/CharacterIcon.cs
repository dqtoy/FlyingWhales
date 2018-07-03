using ECS;
using Pathfinding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterIcon : MonoBehaviour {

    [SerializeField] private SpriteRenderer _icon;
    [SerializeField] private GameObject _avatarGO;
    [SerializeField] private CharacterAIPath _aiPath;
    [SerializeField] private SpriteRenderer _avatarSprite;
    [SerializeField] private GameObject _characterVisualGO;
    [SerializeField] private Animator _avatarAnimator;
    [SerializeField] private AIDestinationSetter _destinationSetter;
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Seeker seeker;
    [SerializeField] private EdgeCollider2D edgeCollider;

    public CharacterPortrait characterPortrait { get; private set; }

    private IParty _iparty;

    private ILocation _targetLocation;
    private bool _isIdle;
    private string upOrDown = "Down";
    private string previousDir;

    private Vector3 normalScale;
    private bool shouldScaleUp = false;

    #region getters/setters
    public IParty iparty {
        get { return _iparty; }
    }
    public CharacterAIPath aiPath {
        get { return _aiPath; }
    }
    public ILocation targetLocation {
        get { return _targetLocation; }
    }
    public AIDestinationSetter destinationSetter {
        get { return _destinationSetter; }
    }
    #endregion

    public void SetCharacter(IParty iparty) {
        _iparty = iparty;
        normalScale = _avatarGO.transform.localScale;
        //UpdateColor();
        this.name = _iparty.icharacters[0].name + "'s Icon";
        _isIdle = true;
        //if (_character.role != null) {
        //    _avatarSprite.sprite = CharacterManager.Instance.GetSpriteByRole(_character.role.roleType);
        //}

#if !WORLD_CREATION_TOOL
        GameObject portraitGO = UIManager.Instance.InstantiateUIObject(CharacterManager.Instance.characterPortraitPrefab.name, this.transform);
        characterPortrait = portraitGO.GetComponent<CharacterPortrait>();
        characterPortrait.GeneratePortrait(_iparty.icharacters[0], IMAGE_SIZE.X64);
        portraitGO.SetActive(false);
#endif
        
        Messenger.AddListener<bool>(Signals.PAUSED, SetMovementState);
        Messenger.AddListener<PROGRESSION_SPEED>(Signals.PROGRESSION_SPEED_CHANGED, OnProgressionSpeedChanged);

#if !WORLD_CREATION_TOOL
        OnProgressionSpeedChanged(GameManager.Instance.currProgressionSpeed);
        SetMovementState(GameManager.Instance.isPaused);
#endif
    }

    public void SetTarget(ILocation target) {
        if (target != null) {
            if (_targetLocation == target) {
                return;
            }
            ILocation location = _iparty.specificLocation;
            if (location != null && location is BaseLandmark) {
                _aiPath.transform.position = location.tileLocation.transform.position;
                location.RemoveCharacterFromLocation(_iparty);
                shouldScaleUp = true;
            }

            _destinationSetter.target = target.tileLocation.transform;
            _aiPath.RecalculatePath();
        } else {
            _destinationSetter.target = null;
        }
        _targetLocation = target;
        //_aiPath.destination = _targetLocation.tileLocation.transform.position;
        //_aiPath.SetRecalculatePathState(true);
    }

    public void SetTarget(Vector3 target) {
        if (_aiPath.destination == target) {
            return;
        }
        _aiPath.destination = target;
        _aiPath.RecalculatePath();
    }
    public void SetTargetGO(GameObject obj) {
        if (obj != null) {
            //if (obj.transform == _destinationSetter.target) {
            //    return;
            //}
            _destinationSetter.target = obj.transform;
            _aiPath.RecalculatePath();
        } else {
            _destinationSetter.target = null;
        }
    }


    #region Visuals
    public void ReclaimPortrait() {
        characterPortrait.transform.SetParent(this.transform);
        characterPortrait.gameObject.SetActive(false);
    }
    #endregion

    #region Speed
    public void SetMovementState(bool state) {
        if (_iparty is CharacterParty && (_iparty as CharacterParty).isHalted) {
            return;
        }
        if (state) {
            _aiPath.maxSpeed = 0f;
        }
    }
    public void OnProgressionSpeedChanged(PROGRESSION_SPEED speed) {
        if (_iparty is CharacterParty && (_iparty as CharacterParty).isHalted) {
            return;
        }
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

    #region Utilities
    public List<HexTile> ConvertToTilePath(List<Vector3> vectorPath) {
        Vector2[] colliderPoints = new Vector2[vectorPath.Count];
        for (int i = 0; i < vectorPath.Count; i++) {
            colliderPoints[i] = vectorPath[i];
        }
        edgeCollider.offset = new Vector2(vectorPath[0].x * -1f, vectorPath[0].y * -1f);
        edgeCollider.points = colliderPoints;

        Collider2D[] collisions = new Collider2D[50];
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.SetLayerMask(LayerMask.GetMask("Hextiles"));
        int numOfCollisions = edgeCollider.OverlapCollider(contactFilter, collisions);
        List<HexTile> tilePath = new List<HexTile>();
        for (int i = 0; i < numOfCollisions; i++) {
            Debug.Log(collisions[i].gameObject.name);
            HexTile tile = collisions[i].gameObject.GetComponent<HexTile>();
            if (tile != null) {
                tilePath.Add(tile);
            }
        }
        return tilePath;
    }
    #endregion

    public void SetPosition(Vector3 position) {
        this.transform.position = position;
    }

    public void SetActionOnTargetReached(Action action) {
        _aiPath.SetActionOnTargetReached(action);
    }

    #region Monobehaviours
//    private HexTile previousTile = null;
//    private void Update() {
//#if UNITY_EDITOR && !WORLD_CREATION_TOOL
//        if (UIManager.Instance.characterInfoUI.activeCharacter != null &&
//            UIManager.Instance.characterInfoUI.activeCharacter.id == _icharacter.id && _icharacter.specificLocation is HexTile) {
//            if (previousTile != null) {
//                previousTile.UnHighlightTile();
//            }
//            (_icharacter.specificLocation as HexTile).HighlightTile(Color.gray, 128f/255f);
//            previousTile = _icharacter.specificLocation as HexTile;
//        }
//#endif
//    }
    private void LateUpdate() {
        //Debug.Log(_aiPath.velocity);
        Vector3 newPos = _aiPath.transform.localPosition;
        newPos.y += 0.5f;
        //newPos.x += 0.02f;
        _avatarGO.transform.localPosition = newPos;
        //aiPath.isStopped = true;
        //Vector3 targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //SetTarget(CameraMove.Instance.mouseObj);
        //Path path = seeker.GetCurrentPath();
        //if (path != null && path.vectorPath.Count > 0) {
        //    List<Vector3> vectorPath = path.vectorPath;
        //    lineRenderer.positionCount = vectorPath.Count;
        //    lineRenderer.SetPositions(vectorPath.ToArray());
        //}
    }
    private void FixedUpdate() {
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
        //if (targetLocation != null && _avatarGO.transform.localScale != Vector3.zero && !GameManager.Instance.isPaused) {
        //    //Debug.Log("Remaining Distance: " + _aiPath.remainingDistance);
        //    if (_aiPath.remainingDistance < 1f) {
        //        //Debug.Log("Shrink!");
        //        Vector3 newScale = _avatarGO.transform.localScale;
        //        newScale.x -= 0.02f;
        //        newScale.y -= 0.02f;
        //        newScale.x = Mathf.Max(0, newScale.x);
        //        newScale.y = Mathf.Max(0, newScale.y);
        //        iTween.ScaleUpdate(_avatarGO.gameObject, newScale, Time.deltaTime * _aiPath.maxSpeed);
        //        //_avatarGO.transform.localScale = Vector3.Lerp(_avatarGO.transform.localScale, newScale, );
        //        if (newScale.x == 0f && newScale.y == 0f) {
        //            _aiPath.transform.position = targetLocation.tileLocation.transform.position;
        //        }
        //    }
        //}
        //if (shouldScaleUp && !GameManager.Instance.isPaused) {
        //    Vector3 newScale = _avatarGO.transform.localScale;
        //    newScale.x += 0.02f;
        //    newScale.y += 0.02f;
        //    newScale.x = Mathf.Min(newScale.x, normalScale.x);
        //    newScale.y = Mathf.Min(newScale.y, normalScale.y);
        //    iTween.ScaleUpdate(_avatarGO.gameObject, newScale, Time.deltaTime * _aiPath.maxSpeed);
        //    if (newScale.x == normalScale.x && newScale.y == normalScale.y) {
        //        shouldScaleUp = false;
        //    }
        //}
    }
    //private void OnMouseDown() {
    //    MouseDown();
    //}
    //private void MouseDown() {
    //    if (UIManager.Instance.IsMouseOnUI()) {
    //        return;
    //    }
    //    if (UIManager.Instance.characterInfoUI.isWaitingForAttackTarget) {
    //        if (UIManager.Instance.characterInfoUI.currentlyShowingCharacter.faction.id != _character.faction.id) { //TODO: Change this checker to relationship status checking instead of just faction
    //            CharacterAction attackAction = _character.characterObject.currentState.GetAction(ACTION_TYPE.ATTACK);
    //            UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(attackAction);
    //            UIManager.Instance.characterInfoUI.SetAttackButtonState(false);
    //            return;
    //        }
    //    } else if (UIManager.Instance.characterInfoUI.isWaitingForJoinBattleTarget) {
    //        CharacterAction joinBattleAction = _character.characterObject.currentState.GetAction(ACTION_TYPE.JOIN_BATTLE);
    //        if (joinBattleAction.CanBeDone() && joinBattleAction.CanBeDoneBy(UIManager.Instance.characterInfoUI.currentlyShowingCharacter)) { //TODO: Change this checker to relationship status checking instead of just faction
    //            UIManager.Instance.characterInfoUI.currentlyShowingCharacter.actionData.AssignAction(joinBattleAction);
    //            UIManager.Instance.characterInfoUI.SetJoinBattleButtonState(false);
    //            return;
    //        }
    //    }
    //    UIManager.Instance.ShowCharacterInfo(_character);
    //}
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
        if (_aiPath.velocity.y <= 0.1f) {
            return "Down";
        } else {
            return "Up";
        }
    }
    #endregion
}
