using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class MoveState : State {

	public MoveState(CharacterTask parentTask): base (parentTask, STATE.MOVE){

	}

}
