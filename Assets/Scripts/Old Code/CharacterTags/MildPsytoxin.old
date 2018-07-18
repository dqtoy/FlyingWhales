using UnityEngine;
using System.Collections;

public class MildPsytoxin : CharacterTag {
	private int chanceToWorsenCase;

	public MildPsytoxin(ECS.Character character): base(character, CHARACTER_TAG.MILD_PSYTOXIN){
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
        Log worsenLog = new Log(GameManager.Instance.Today(), "CharacterTags", "Psytoxin", "worsen_psytoxin");
        worsenLog.AddToFillers(_character, _character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        worsenLog.AddToFillers(null, "moderate", LOG_IDENTIFIER.OTHER);
        _character.AddHistory(worsenLog);
        _character.AssignTag (CHARACTER_TAG.MODERATE_PSYTOXIN);
		_character.RemoveCharacterTag (this);
	}

	internal void TriggerWorsenCase(){
		int chance = Utilities.rng.Next (0, 100);
		if(chance < chanceToWorsenCase){
			WorsenCase ();
		}
	}
}
