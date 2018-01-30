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
			_landmarkName = RandomNameGenerator.Instance.GetAncientRuinName ();
            _encounterables.AddElement(ENCOUNTERABLE.ITEM_CHEST, 30);
            _encounterables.AddElement (ENCOUNTERABLE.PARTY, 50);
			_landmarkEncounterableType = _encounterables.PickRandomElementGivenWeights();
			_landmarkEncounterable = GetNewEncounterable (_landmarkEncounterableType);
        }
    }
    #endregion

	private Party GeneratePartyEncounterable(string partyName){
		EncounterParty encounterParty = null;
		if(partyName == "random"){
			encounterParty = EncounterPartyManager.Instance.GetRandomEncounterParty ();
		}else{
			 encounterParty = EncounterPartyManager.Instance.GetEncounterParty (partyName);
		}
		encounterCharacters = encounterParty.GetAllCharacters (this);
		DungeonParty party = new DungeonParty (encounterCharacters [0], false);
		for (int i = 1; i < encounterCharacters.Count; i++) {
			party.AddPartyMember (encounterCharacters [i]);
		}
		party.SetName (encounterParty.name);
		return party;
	}
	private IEncounterable GetNewEncounterable(ENCOUNTERABLE encounterableType){
		switch (encounterableType){
		case ENCOUNTERABLE.ITEM_CHEST:
			return new ItemChest(1, ITEM_TYPE.ARMOR, 35);
		case ENCOUNTERABLE.PARTY:
			return GeneratePartyEncounterable ("random");
		default:
			return null;
		}
	}
}
