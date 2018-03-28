using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class StealState : State {
	private Character _targetCharacter;

	public StealState(CharacterTask parentTask): base (parentTask, STATE.STEAL){
	}

	#region Overrides
	public override void OnChooseState (Character character){
		base.OnChooseState (character);
		if(_parentTask.specificTarget != null){
			_targetCharacter = _parentTask.specificTarget as Character;
		}
	}
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		StealStuff ();
		return true;
	}
	protected override void ResetState (){
		base.ResetState ();
		_targetCharacter = null;
	}
	#endregion


	private void StealStuff(){
		if(_targetCharacter.specificLocation != _assignedCharacter.specificLocation){
			_parentTask.EndTaskFail ();
			return;
		}
		string stealAction = TaskManager.Instance.stealActions.PickRandomElementGivenWeights ();
		if(stealAction == "steal"){
			StealAttempt ();
		}
	}

	private void StealAttempt(){
		string attemptAction = TaskManager.Instance.stealAttemptActions.PickRandomElementGivenWeights ();
		if (attemptAction == "success not caught"){
			Item itemToSteal = GetItemToSteal ();
			if(itemToSteal != null){
				itemToSteal.possessor.ThrowItem (itemToSteal, false);
				_assignedCharacter.PickupItem (itemToSteal);
			}
		}else if (attemptAction == "success caught"){
			Item itemToSteal = GetItemToSteal ();
			if(itemToSteal != null){
				itemToSteal.possessor.ThrowItem (itemToSteal, false);
				_assignedCharacter.PickupItem (itemToSteal);
			}
			CharacterCaught ();
		}else if (attemptAction == "failed not caught"){
			//Nothing happens, just put log
		}else if (attemptAction == "failed caught"){
			CharacterCaught ();
		}
		_parentTask.EndTaskSuccess ();
	}

	private Item GetItemToSteal(){
		if(_targetCharacter.party == null){
			if(_targetCharacter.inventory.Count > 0){
				return _targetCharacter.inventory [UnityEngine.Random.Range (0, _targetCharacter.inventory.Count)];
			}
		}else{
			List<Item> itemsInParty = new List<Item> ();
			for (int i = 0; i < _targetCharacter.party.partyMembers.Count; i++) {
				itemsInParty.AddRange (_targetCharacter.party.partyMembers [i].inventory);
			}

			if(itemsInParty.Count > 0){
				return itemsInParty [UnityEngine.Random.Range (0, itemsInParty.Count)];
			}
		}
		return null;
	}

	private void CharacterCaught(){
		_assignedCharacter.AssignTag (CHARACTER_TAG.CRIMINAL);
	}
}
