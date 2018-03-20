using UnityEngine;
using System.Collections;

public class AttackState : State {

	public AttackState(CharacterTask parentTask): base (parentTask, STATE.ATTACK){
	}

	#region Overrides
	public override bool PerformStateAction (){
		if(!base.PerformStateAction ()){ return false; }
		Attack ();
		return true;
	}
	#endregion

	private void Attack(){
		KillCivilians ();
		ChangeLandmarkOwnership ();
	}
	private bool AreThereStillHostileInLandmark(){
		for (int i = 0; i < _targetLandmark.charactersAtLocation.Count; i++) {
			ICombatInitializer character = _targetLandmark.charactersAtLocation [i];
			if(character is Party){
				Party party = (Party)character;
				if(party.partyLeader.id != _assignedCharacter.id){
					if(party.faction == null || _assignedCharacter.faction == null){
						return true;
					}else{
						FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith (party.faction);
						if(facRel != null){
							if(facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
								return true;
							}
						}else{
							return true;
						}
					}
				}
			}else if (character is ECS.Character){
				ECS.Character currentCharacter = (ECS.Character)character;
				if(currentCharacter.id != _assignedCharacter.id){
					if(currentCharacter.faction == null || _assignedCharacter.faction == null){
						return true;
					}else{
						FactionRelationship facRel = _assignedCharacter.faction.GetRelationshipWith (currentCharacter.faction);
						if(facRel != null){
							if(facRel.relationshipStatus == RELATIONSHIP_STATUS.HOSTILE){
								return true;
							}
						}else{
							return true;
						}
					}
				}
			}
		}
		return false;
	}
	private void KillCivilians(){
		int civiliansInLandmark = _targetLandmark.civilians;
		if(civiliansInLandmark > 0){
			int civilianCasualtiesPercentage = UnityEngine.Random.Range (15, 51);
			int civilianCasualties = (int)(((float)civilianCasualtiesPercentage / 100f) * (float)civiliansInLandmark);
			_targetLandmark.ReduceCivilians (civilianCasualties);
		}
	}
	private void ChangeLandmarkOwnership(){
		if(_parentTask is Attack && !((Attack)_parentTask).canChangeOwnership){
			return;
		}
		if(_targetLandmark is Settlement || _targetLandmark is ResourceLandmark){
			_targetLandmark.ChangeOwner (_assignedCharacter.faction);
		}
	}
}
