using UnityEngine;
using System.Collections;

public class ModeratePsytoxin : CharacterTag {
	public ModeratePsytoxin(ECS.Character character): base(character, CHARACTER_TAG.MODERATE_PSYTOXIN){
	}

	#region Overrides
	public override void Initialize (){
		base.Initialize ();
		ScheduleAggravateCheck ();
	}
	#endregion

	private void ScheduleAggravateCheck(){
		GameDate newSched = GameManager.Instance.FirstDayOfTheMonth();
		newSched.AddMonths (1);
		SchedulingManager.Instance.AddEntry (newSched, () => AggravateCheck ());
	}

	private void AggravateCheck(){
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < 5){
			WorsenCase ();
		}
	}
	private void WorsenCase(){
		_character.AssignTag (CHARACTER_TAG.SEVERE_PSYTOXIN);
		_character.RemoveCharacterTag (this);
	}
}
