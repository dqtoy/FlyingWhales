using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;


public class CharacterParty : Party {

    public CharacterParty() : base (null){

    }

    public CharacterParty(Character owner): base(owner) {
#if !WORLD_CREATION_TOOL
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
#endif
    }

    #region Overrides
    public void DisbandPartyKeepOwner() {
        while (characters.Count != 1) {
            for (int i = 0; i < characters.Count; i++) {
                Character currCharacter = characters[i];
                if (currCharacter.id != owner.id) {
                    RemoveCharacter(currCharacter);
                    break;
                }
            }
        }
    }
    /*
        Create a new icon for this character.
        Each character owns 1 icon.
            */
    public override void CreateIcon() {
        base.CreateIcon();
        GameObject characterIconGO = GameObject.Instantiate(CharacterManager.Instance.characterIconPrefab,
            Vector3.zero, Quaternion.identity, CharacterManager.Instance.characterIconsParent);

        _icon = characterIconGO.GetComponent<CharacterAvatar>();
        _icon.Init(this);
        //_icon = characterIconGO.GetComponent<CharacterIcon>();
        //_icon.SetCharacter(this);
        //_icon.SetAnimator(CharacterManager.Instance.GetAnimatorByRole(mainCharacter.role.roleType));
        //PathfindingManager.Instance.AddAgent(_icon.aiPath);
        //PathfindingManager.Instance.AddAgent(_icon.pathfinder);

    }
    public override void RemoveListeners() {
        base.RemoveListeners();
        //Messenger.RemoveListener(Signals.DAY_ENDED, EverydayAction);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_SNATCHED, OnCharacterSnatched);
        Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    #endregion

    #region Outside Handlers
    public void OnCharacterDied(Character diedCharacter) {
        if (diedCharacter.id == _owner.id) {
            //character that died was the main character of this party, disband it
            DisbandPartyKeepOwner();
        }
    }
    #endregion
}
