using UnityEngine;
using System.Collections;

public class SmoothMovement : MonoBehaviour {
	public float speed;
	internal bool isMoving = false;
	Vector3 targetPosition = Vector3.zero;

	void Update(){
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
		if(this.GetComponent<Animator>() != null){
			this.GetComponent<Animator>().Play("Idle");
		}else{
			if (this.GetComponentInChildren<Animator> () != null) {
				this.GetComponentInChildren<Animator>().Play("Idle");
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
