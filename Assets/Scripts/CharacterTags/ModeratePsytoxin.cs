using UnityEngine;
using System.Collections;

public class ModeratePsytoxin : CharacterTag {
	private int chanceToWorsenCase;

	public ModeratePsytoxin(ECS.Character character): base(character, CHARACTER_TAG.MODERATE_PSYTOXIN){
		chanceToWorsenCase = 5;
	}

	#region Overrides
	public override void Initialize (){
		base.Initialize ();
		if(Messenger.eventTable.ContainsKey("Psytoxinated")){
			Messenger.Broadcast ("Psytoxinated");
		}
		ScheduleAggravateCheck ();
	}
	public override void OnRemoveTag (){
		base.OnRemoveTag ();
		if (Messenger.eventTable.ContainsKey ("Unpsytoxinated")) {
			Messenger.Broadcast ("Unpsytoxinated");
		}
	}
	#endregion

	private void ScheduleAggravateCheck(){
		GameDate newSched = GameManager.Instance.FirstDayOfTheMonth();
		newSched.AddMonths (1);
		SchedulingManager.Instance.AddEntry (newSched, () => AggravateCheck ());
	}

	private void AggravateCheck(){
		if(_isRemoved){
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < chanceToWorsenCase){
			WorsenCase ();
		}else{
			chanceToWorsenCase += 1;
			ScheduleAggravateCheck ();
		}
	}
	private void WorsenCase(){
		//_character.AddHistory ("Psytoxin has worsen! It is now severe!");
		_character.AssignTag (CHARACTER_TAG.SEVERE_PSYTOXIN);
		_character.RemoveCharacterTag (this);
	}

	internal void TriggerWorsenCase(){
		int chance = Utilities.rng.Next (0, 100);
		if(chance < chanceToWorsenCase){
			WorsenCase ();
		}
	}
}
