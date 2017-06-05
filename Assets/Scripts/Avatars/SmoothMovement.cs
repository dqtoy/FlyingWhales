using UnityEngine;
using System.Collections;

public class SmoothMovement : MonoBehaviour {
	public float speed;
	internal bool isMoving = false;
	Vector3 targetPosition = Vector3.zero;
	internal DIRECTION direction;
	internal bool hasAttacked;

	private float step = 0f;
	private float timeStarted = 0f;
	private float timeSinceStarted = 0f;

	void FixedUpdate(){
		if(this.isMoving){
			if(this.targetPosition != null){
				this.timeSinceStarted = Time.time - this.timeStarted;
				this.step = this.timeSinceStarted / GameManager.Instance.progressionSpeed;
				this.transform.position = Vector3.Lerp (this.transform.position, this.targetPosition, this.step);
				if(this.step >= 1.0f){
					StopMoving ();
				}
			}
		}
	}

	private void StopMoving(){
		if (!this.hasAttacked) {
			string idleToPlay = GetIdleDirection();

			if(this.GetComponent<Animator>() != null){
				this.GetComponent<Animator>().Play(idleToPlay);
			}else{
				if (this.GetComponentInChildren<Animator> () != null) {
					this.GetComponentInChildren<Animator>().Play(idleToPlay);
				}
			}
		}
		this.isMoving = false;
		this.targetPosition = Vector3.zero;
	}

	internal void Move(Vector3 endPos){
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
