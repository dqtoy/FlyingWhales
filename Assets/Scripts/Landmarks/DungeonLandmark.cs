using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DungeonLandmark : BaseLandmark {

	private DungeonParty _dungeonParty;

    public DungeonLandmark(HexTile location, LANDMARK_TYPE specificLandmarkType, MATERIAL materialMadeOf) : base(location, specificLandmarkType, materialMadeOf) {
        _canBeOccupied = false;
    }

    #region Encounterables
    public override void Initialize() {
        base.Initialize();
		//DungeonEncounterChances dungeonEncounterChances = LandmarkManager.Instance.GetDungeonEncounterChances (specificLandmarkType);
  //      if(specificLandmarkType == LANDMARK_TYPE.ANCIENT_RUIN) {
		//	_landmarkName = RandomNameGenerator.Instance.GetAncientRuinName ();

		//	int chance = UnityEngine.Random.Range (0, 100);
		//	if(chance < dungeonEncounterChances.encounterPartyChance){
		//		_dungeonParty = (DungeonParty)GeneratePartyEncounterable ("random");
		//		_dungeonParty.partyLeader.DetermineAction ();
		//	}
		//	if (chance < dungeonEncounterChances.encounterLootChance) {
		//		LandmarkItemsSpawn ();
		//	}
  //      } else 
        //if (specificLandmarkType == LANDMARK_TYPE.VAMPIRE_TOMB) {
        //    SpawnAncientVampire();
        //}
    }
    #endregion

//	private Party GeneratePartyEncounterable(string partyName){
//		EncounterParty encounterParty = null;
//		if(partyName == "random"){
//			encounterParty = EncounterPartyManager.Instance.GetRandomEncounterParty ();
//		}else{
//			 encounterParty = EncounterPartyManager.Instance.GetEncounterParty (partyName);
//		}
//		List<ECS.Character> encounterCharacters = encounterParty.GetAllCharacters (this);
//		DungeonParty party = new DungeonParty (encounterCharacters [0], false);
//        for (int i = 1; i < encounterCharacters.Count; i++) {
//			party.AddPartyMember (encounterCharacters [i]);
//		}
//		party.SetName (encounterParty.name);
//        //this.location.RemoveCharacterFromLocation(party);
//        this.AddCharacterToLocation(party, false);
//        return party;
//	}
//	private IEncounterable GetNewEncounterable(ENCOUNTERABLE encounterableType){
//		switch (encounterableType){
//		case ENCOUNTERABLE.ITEM_CHEST:
//			return new ItemChest(1, ITEM_TYPE.ARMOR, 35);
//		case ENCOUNTERABLE.PARTY:
//			return GeneratePartyEncounterable ("random");
//		default:
//			return null;
//		}
//	}
//	private void LandmarkItemsSpawn(){
//		int numOfItems = UnityEngine.Random.Range (1, 4);
//		for (int i = 0; i < numOfItems; i++) {
//			ECS.Item item = ItemManager.Instance.GetRandomTier (1, ITEM_TYPE.ITEM);
//			QUALITY equipmentQuality = GetEquipmentQuality();
//			if (item.itemType == ITEM_TYPE.ARMOR) {
//				((ECS.Armor)item).SetQuality(equipmentQuality);
//			} else if (item.itemType == ITEM_TYPE.WEAPON) {
//				((ECS.Weapon)item).SetQuality(equipmentQuality);
//			}
//			AddItemInLandmark(item);
//		}
//	}

	private QUALITY GetEquipmentQuality(){
		int crudeChance = 30;
		int exceptionalChance = crudeChance + 20;
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < crudeChance){
			return QUALITY.CRUDE;
		}else if(chance >= crudeChance && chance < exceptionalChance){
			return QUALITY.EXCEPTIONAL;
		}
		return QUALITY.NORMAL;
	}

    #region Vampire Tomb
    public ECS.Character SpawnAncientVampire() {
        ECS.Character mingonArcanistVampire = CreateNewCharacter(RACE.MINGONS, CHARACTER_ROLE.ANCIENT_VAMPIRE, "Arcanist");
        mingonArcanistVampire.AssignTag(CHARACTER_TAG.VAMPIRE);
        EquipGearForVampire(mingonArcanistVampire);
		Debug.Log("Ancient vampire is at : " + tileLocation.name);
        return mingonArcanistVampire;
    }
    private void EquipGearForVampire(ECS.Character vampire) {
        //Weapon
        ECS.Item weapon = ItemManager.Instance.CreateNewItemInstance("Mithril Staff");
        vampire.EquipItem(weapon);

        //Armor
        ECS.Item boot = ItemManager.Instance.CreateNewItemInstance("Mithril Boot");
        ECS.Item boot2 = ItemManager.Instance.CreateNewItemInstance("Mithril Boot");
        vampire.EquipItem(boot);
        vampire.EquipItem(boot2);

        ECS.Item bracer = ItemManager.Instance.CreateNewItemInstance("Mithril Bracer");
        ECS.Item bracer2 = ItemManager.Instance.CreateNewItemInstance("Mithril Bracer");
        vampire.EquipItem(bracer);
        vampire.EquipItem(bracer2);

        ECS.Item helmet = ItemManager.Instance.CreateNewItemInstance("Mithril Helmet");
        vampire.EquipItem(helmet);

        ECS.Item leggings = ItemManager.Instance.CreateNewItemInstance("Mithril Leggings");
        vampire.EquipItem(leggings);

        ECS.Item shirt = ItemManager.Instance.CreateNewItemInstance("Mithril Shirt");
        vampire.EquipItem(shirt);
    }
    #endregion
}
