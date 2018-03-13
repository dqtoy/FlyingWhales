using UnityEngine;
using System.Collections;

public class TerminallyIll : CharacterTag {

	public TerminallyIll(ECS.Character character): base(character, CHARACTER_TAG.TERMINALLY_ILL){
	}

	#region Overrides
	public override void Initialize (){
		base.Initialize ();
		ScheduleDeathCheck ();
	}
	#endregion

	private void ScheduleDeathCheck(){
		GameDate newSched = GameManager.Instance.Today ();
		newSched.AddDays (7);
		SchedulingManager.Instance.AddEntry (newSched, () => DeathCheck ());
	}

	private void DeathCheck(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 1){
			_character.Death ();
		}else{
			ScheduleDeathCheck ();
		}
	}
}
