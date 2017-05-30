using UnityEngine;
using System.Collections;

public class SmoothMovement : MonoBehaviour {
	public float speed;
	internal bool isMoving = false;
	Vector3 targetPosition = Vector3.zero;
	internal bool isDirectionUp;

	void FixedUpdate(){
		if(this.isMoving){
			if(this.targetPosition != null){
				float step = speed * Time.deltaTime;
				this.transform.position = Vector3.MoveTowards (this.transform.position, this.targetPosition, step);
				if(Vector3.Distance(this.transform.position, this.targetPosition) < 0.1f){
					StopMoving ();
				}
			}
		}
	}

	private void StopMoving(){
		string idleToPlay = "Idle";
		if(this.isDirectionUp){
			idleToPlay = "Idle_Up";
		}
		if(this.GetComponent<Animator>() != null){
			this.GetComponent<Animator>().Play(idleToPlay);
		}else{
			if (this.GetComponentInChildren<Animator> () != null) {
				this.GetComponentInChildren<Animator>().Play(idleToPlay);
			}
		}
		this.isMoving = false;
		this.targetPosition = Vector3.zero;
	}

	internal void Move(Vector3 endPos){
		this.targetPosition = endPos;
		this.isMoving = true;
	}
}
