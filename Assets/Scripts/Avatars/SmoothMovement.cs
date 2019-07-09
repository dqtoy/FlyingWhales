using UnityEngine;
using System.Collections;
using System;

public class SmoothMovement : MonoBehaviour {

    public delegate void OnMoveFinished();
    public OnMoveFinished onMoveFinished;

    public float moveTicksPerTile;

    [NonSerialized] public DIRECTION direction;
    [NonSerialized] public bool isMoving = false;
    [NonSerialized] public bool hasAttacked;
    [NonSerialized] public bool isHalted;
    [NonSerialized] public GameObject avatarGO;

    //[SerializeField] private float step = 0f;
    //[SerializeField] private float timeStarted = 0f;
    //[SerializeField] private float timeSinceStarted = 0f;

    private Vector3 targetPosition = Vector3.zero;

 //   void Update(){
	//	if(this.isMoving && !GameManager.Instance.isPaused && !isHalted){
	//		this.step = Time.deltaTime / GameManager.Instance.progressionSpeed;
	//		this.avatarGO.transform.position = Vector3.Lerp (this.avatarGO.transform.position, this.targetPosition, this.step * 2f);
	//		if(this.avatarGO.transform.position == this.targetPosition){
	//			StopMoving ();
	//		}
	//	}
	//}
    private IEnumerator MoveToPosition(Vector3 from, Vector3 to) {
        float t = 0f;
        while (t < 1) {
            if (this.isMoving && !GameManager.Instance.isPaused && !isHalted) {
                t += Time.deltaTime / (moveTicksPerTile * GameManager.Instance.progressionSpeed);
                transform.position = Vector3.Lerp(from, to, t);
            }
            yield return null;
        }
        StopMoving();
    }
    internal void Reset() {
        //step = 0f;
        //timeStarted = 0f;
        //timeSinceStarted = 0f;
        isMoving = false;
        targetPosition = Vector3.zero;
        direction = DIRECTION.LEFT;
        hasAttacked = false;
		//onMoveFinished = null;
    }
    public void ForceStopMovement() {
        this.isMoving = false;
        this.targetPosition = Vector3.zero;
    }
    private void StopMoving(){
		this.isMoving = false;
		this.targetPosition = Vector3.zero;
		if(onMoveFinished != null) {
			onMoveFinished();
        }
    }
	internal void Move(Vector3 endPos, DIRECTION direction){
		this.direction = direction;
		this.targetPosition = endPos;
		this.isMoving = true;
		//this.timeStarted = Time.time;
		//this.timeSinceStarted = 0f;
		//this.step = 0f;
        StartCoroutine(MoveToPosition(this.avatarGO.transform.position, this.targetPosition));
	}

	internal string GetIdleDirection(){
		if(this.direction == DIRECTION.UP){
			return "Idle_Up";
		}else if(this.direction == DIRECTION.DOWN){
			return "Idle_Down";
		}else if(this.direction == DIRECTION.LEFT){
			return "Idle_Left";
		}else if(this.direction == DIRECTION.RIGHT){
			return "Idle_Right";
		}
		return "Idle_Left";
	}
}
