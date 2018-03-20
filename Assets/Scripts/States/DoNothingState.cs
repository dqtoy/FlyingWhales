using UnityEngine;
using System.Collections;

public class DoNothingState : State {
	public DoNothingState(CharacterTask parentTask): base (parentTask, STATE.DO_NOTHING){

	}
}
