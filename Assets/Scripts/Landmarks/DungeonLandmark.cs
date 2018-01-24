using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonLandmark : BaseLandmark {

	private List<ECS.Character> encounterCharacters;

    public DungeonLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = false;
    }

    #region Encounterables
    protected override void InititalizeEncounterables() {
        base.InititalizeEncounterables();
        if(specificLandmarkType == LANDMARK_TYPE.ANCIENT_RUIN) {
            _encounterables.AddElement(new ItemChest(1, ITEM_TYPE.ARMOR, 35), 30);
            _encounterables.AddElement (GeneratePartyEncounterable ("Goblin Party A"), 50);
        }
    }
    #endregion

	private Party GeneratePartyEncounterable(string partyName){
		EncounterParty encounterParty = EncounterPartyManager.Instance.GetEncounterParty (partyName);
		encounterCharacters = encounterParty.GetAllCharacters (this);
		DungeonParty party = new DungeonParty (encounterCharacters [0], false);
		for (int i = 1; i < encounterCharacters.Count; i++) {
			party.AddPartyMember (encounterCharacters [i]);
		}
		return party;
	}
}
