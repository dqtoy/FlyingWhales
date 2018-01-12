using UnityEngine;
using System.Collections;

public class SmoothMovement : MonoBehaviour {

    public delegate void OnMoveFinished();
    public OnMoveFinished onMoveFinished;

	internal bool isMoving = false;
	Vector3 targetPosition = Vector3.zero;
	internal DIRECTION direction;
	internal bool hasAttacked;

    [SerializeField] private float step = 0f;
    [SerializeField] private float timeStarted = 0f;
    [SerializeField] private float timeSinceStarted = 0f;

	internal GameObject avatarGO;


	void Update(){
		if(this.isMoving && !GameManager.Instance.isPaused){
			this.step = Time.deltaTime / GameManager.Instance.progressionSpeed;
			this.avatarGO.transform.position = Vector3.MoveTowards (this.avatarGO.transform.position, this.targetPosition, this.step * 2f);
			if(this.avatarGO.transform.position == this.targetPosition){
				StopMoving ();
			}
		}
	}

    internal void Reset() {
        step = 0f;
        timeStarted = 0f;
        timeSinceStarted = 0f;
        isMoving = false;
        targetPosition = Vector3.zero;
        direction = DIRECTION.LEFT;
        hasAttacked = false;
		onMoveFinished = null;
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
		this.timeStarted = Time.time;
		this.timeSinceStarted = 0f;
		this.step = 0f;
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
