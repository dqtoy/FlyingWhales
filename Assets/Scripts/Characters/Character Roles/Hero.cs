/*
 The hero explores the world for both monsters and treasures. 
 They form party with Warriors to perform their duties. 
 They create Minor Roads when they explore roadless tiles.
 Place functions unique to heroes here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Hero : CharacterRole {

	public Hero(Character character): base (character) {
        _roleType = CHARACTER_ROLE.HERO;
   //     _allowedQuestTypes = new List<QUEST_TYPE>() {
   //         QUEST_TYPE.DEFEND,
   //         QUEST_TYPE.EXPLORE_TILE,
   //         QUEST_TYPE.EXPAND,
			//QUEST_TYPE.EXPEDITION,
			//QUEST_TYPE.SAVE_LANDMARK,
   //     };

   //     _allowedQuestAlignments = new List<ACTION_ALIGNMENT>() {
   //         ACTION_ALIGNMENT.HEROIC,
   //         ACTION_ALIGNMENT.LAWFUL,
   //         ACTION_ALIGNMENT.UNLAWFUL,
			//ACTION_ALIGNMENT.PEACEFUL,
   //     };

        //_roleTasks.Add (new DoNothing (this._character));
        //_roleTasks.Add (new Rest (this._character));
        //_roleTasks.Add (new ExploreTile (this._character, 5));
        //_roleTasks.Add (new UpgradeGear (this._character));
        //_roleTasks.Add (new MoveTo (this._character));
        //_roleTasks.Add (new TakeQuest (this._character));
        //_roleTasks.Add (new Attack (this._character, 10));
        //_roleTasks.Add (new Patrol (this._character, 10));

        //_defaultRoleTask = _roleTasks [1];

        //Messenger.AddListener(Signals.HOUR_ENDED, StartDepletion);
        SetFullness(100);
        SetEnergy(100);
        SetFun(60);
        //SetPrestige(0);
        //SetSanity(100);
        //UpdateSafety();
        UpdateHappiness();
    }

    #region Overrides
    public override void OnAssignRole() {
        base.OnAssignRole();
        CharacterAction _defendAction = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.DEFEND) as DefendAction;
        _character.AddMiscAction(_defendAction);
        Messenger.AddListener<Party, GameEvent>(Signals.LANDMARK_UNDER_ATTACK, LandmarkUnderAttack);
    }
    public override void DeathRole() {
        base.DeathRole();
        _character.RemoveMiscAction(ACTION_TYPE.DEFEND);
        Messenger.RemoveListener<Party, GameEvent>(Signals.LANDMARK_UNDER_ATTACK, LandmarkUnderAttack);
    }
    public override void ChangedRole() {
        base.ChangedRole();
        Messenger.RemoveListener<Party, GameEvent>(Signals.LANDMARK_UNDER_ATTACK, LandmarkUnderAttack);
    }
    #endregion

    private void LandmarkUnderAttack(Party attacker, GameEvent associatedEvent) {
        if(_character.currentParty.currentCombat == null && attacker.specificLocation.tileLocation.landmarkOnTile != null && attacker.specificLocation.tileLocation.areaOfTile.id == _character.homeLandmark.tileLocation.areaOfTile.id) {
            DefendAction defendAction = _character.GetMiscAction(ACTION_TYPE.DEFEND) as DefendAction;
            if (!_character.IsInOwnParty()) {
                _character.currentParty.RemoveCharacter(_character);
            }
            CharacterParty characterParty = _character.ownParty as CharacterParty;
            characterParty.actionData.AssignAction(defendAction, attacker.specificLocation.tileLocation.landmarkOnTile.landmarkObj, null, associatedEvent);
        }
    }
}
