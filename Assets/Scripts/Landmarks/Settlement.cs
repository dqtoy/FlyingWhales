/*
 This will replace the city script.
 */
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Settlement : BaseLandmark {

    private CHARACTER_CLASS classToCreate;
    private CHARACTER_ROLE roleToCreate;

    private ECS.Character _headOfSettlement;
	private List<BaseLandmark> _ownedLandmarks;

	//private WeightedDictionary<MATERIAL> _materialWeights;

    private const int CHARACTER_LIMIT = 10;

    //private RACE _producingPopulationFor;
    private float _currentPopulationProduction;

	//Crater
	private int _numOfPsytoxinated;

    #region getters/setters
    //public List<Quest> questBoard {
    //    get { return _questBoard; }
    //}
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
    }
	public override int totalPopulation {
		get { return civilians + CharactersCount() + this._ownedLandmarks.Sum(x => x.civilians); }
	}
    public List<MATERIAL> availableMaterials {
        get { return _ownedLandmarks.Where(x => x is ResourceLandmark && x.civilians >= x.GetMinimumCivilianRequirement()).Select(x => (x as ResourceLandmark).materialOnLandmark).ToList(); }
    }
	public int numOfPsytoxinated {
		get { return _numOfPsytoxinated; }
	}
    #endregion

    public Settlement(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
        //_questBoard = new List<Quest>();
		_ownedLandmarks = new List<BaseLandmark>();
		//_materialWeights = new WeightedDictionary<MATERIAL>();
        //_producingPopulationFor = RACE.NONE;
//		ConstructNeededMaterials ();
    }

    #region Overrides
    public override void Initialize (){
		base.Initialize ();
		if (_specificLandmarkType == LANDMARK_TYPE.GOBLIN_CAMP) {
            ////When spawned at the start of World Generation, a Goblin Camp also starts with a random number of civilians between 10 to 30 Goblins.
            //AdjustCivilians(RACE.GOBLIN, Random.Range(10, 31));
            GenerateGoblinCampTechnologies();
        }
    }
	public override void DestroyLandmark (bool putRuinStructure){
		base.DestroyLandmark (putRuinStructure);
		tileLocation.RuinStructureOnTile (!putRuinStructure);
		if(_specificLandmarkType == LANDMARK_TYPE.CRATER){
			DestroyCrater ();
		}
	}
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
        faction.AddSettlement(this);
        if (specificLandmarkType == LANDMARK_TYPE.KINGS_CASTLE) {
            //Create structures on location
            //tileLocation.region.HighlightRegionTiles(faction.factionColor, 69f / 255f);
            //tileLocation.region.ReColorBorderTiles(faction.factionColor);
            _landmarkName = RandomNameGenerator.Instance.GenerateCityName(faction.race);
            tileLocation.CreateStructureOnTile(faction, STRUCTURE_TYPE.CITY);
            landmarkObject.UpdateName();
            //tileLocation.emptyCityGO.SetActive(false);
            //_producingPopulationFor = GetRaceBasedOnProportion();
        }
    }
    public override void UnoccupyLandmark() {
        base.UnoccupyLandmark();
        _owner.RemoveSettlement(this);
    }
    #endregion

    #region Characters
    public ECS.Character CreateNewFollower() {
        if (this.civilians <= 0) {
            throw new System.Exception(this.landmarkName + " no longer has any more civilians for creating a follower!");
        }
        WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(this);
        CHARACTER_CLASS chosenClass = characterClassProductionDictionary.PickRandomElementGivenWeights();
        ECS.Character newFollower = CreateNewCharacter(CHARACTER_ROLE.FOLLOWER, Utilities.NormalizeString(chosenClass.ToString()));
        newFollower.SetFollowerState(true);
		newFollower.AssignInitialTags ();
        CharacterManager.Instance.EquipCharacterWithBestGear(this, newFollower);
        return newFollower;
    }
    ///*
    // Does the settlement have the required technology
    // to produce a class?
    //     */
    //public bool CanProduceClass(CHARACTER_CLASS charClass, ref MATERIAL material) {
    //    if(_owner == null) {
    //        return false;
    //    }
    //    TECHNOLOGY neededTech = Utilities.GetTechnologyForCharacterClass(charClass);
    //    List<MATERIAL> trainingPreference = this._owner.productionPreferences[PRODUCTION_TYPE.TRAINING].prioritizedMaterials;
    //    for (int i = 0; i < trainingPreference.Count; i++) {
    //        MATERIAL currMat = trainingPreference[i];
    //        if (ProductionManager.Instance.trainingMaterials.Contains(currMat) && HasAccessToMaterial(currMat)) {
    //            material = trainingPreference[i];
    //        }
    //    }
    //    //if (material == MATERIAL.NONE) {
    //    //    return false; //this settlement has no access to materials for training.
    //    //}
    //    if (neededTech == TECHNOLOGY.NONE) {
    //        return true;
    //    } else {
    //        return _technologies[neededTech];
    //    }
    //    //if (neededTech == TECHNOLOGY.NONE || _technologies[neededTech]) {
    //    //    TrainingClass trainingClass = ProductionManager.Instance.trainingClassesLookup[charClass];
    //    //    List<MATERIAL> trainingPreference = this._owner.productionPreferences[PRODUCTION_TYPE.TRAINING].prioritizedMaterials;
    //    //    for (int i = 0; i < trainingPreference.Count; i++) {
    //    //        if (ProductionManager.Instance.trainingMaterials.Contains(trainingPreference[i]) && trainingClass.production.resourceCost <= _materialsInventory[trainingPreference[i]].count) {
    //    //            material = trainingPreference[i];
    //    //            return true;
    //    //        }
    //    //    }
    //    //}
    //    //return false;
    //}
    public bool CanProduceRole(CHARACTER_ROLE roleType) {
        TrainingRole trainingRole = ProductionManager.Instance.trainingRolesLookup[roleType];
        if (trainingRole.production.civilianCost <= civilians) {
            return true;
        }
        //if (trainingRole.production.civilianCost <= civilians && trainingRole.production.foodCost <= GetTotalFoodCount()) {
        //    return true;
        //}
        return false;
    }
    public void SetHead(ECS.Character head) {
        _headOfSettlement = head;
        if(_owner.leader != null) {
            if(_headOfSettlement.GetRelationshipWith(_owner.leader) == null) {
                //Create Relationship between the head of the settlement and the leader of the faction that owns the settlement
                CharacterManager.Instance.CreateNewRelationshipBetween(_owner.leader, _headOfSettlement);
            }
        }

        for (int i = 0; i < _charactersWithHomeOnLandmark.Count; i++) {
            ECS.Character otherCharacter = _charactersWithHomeOnLandmark[i];
            if(_headOfSettlement.id != otherCharacter.id) {
                if(_headOfSettlement.GetRelationshipWith(otherCharacter) == null) {
                    //Village Elders will have relationship with the characters within their village.
                    CharacterManager.Instance.CreateNewRelationshipBetween(_headOfSettlement, otherCharacter);
                }
            }
        }
    }
    /*
     Make a character consider this landmark as it's home.
         */
    public override void AddCharacterHomeOnLandmark(ECS.Character character) {
        if (!_charactersWithHomeOnLandmark.Contains(character)) {
            _charactersWithHomeOnLandmark.Add(character);
            //Add new relationship with head of the settlement
            if(_headOfSettlement != null) {
                if (character.GetRelationshipWith(_headOfSettlement) == null) {
                    //Village Elders will have relationship with the characters within their village.
                    CharacterManager.Instance.CreateNewRelationshipBetween(_headOfSettlement, character);
                }
            }
            
        }
    }
    public List<ECS.Character> GetCharactersCreatedInSettlement(CHARACTER_ROLE charRole) {
        List<ECS.Character> characters = new List<ECS.Character>();
        for (int i = 0; i < _charactersWithHomeOnLandmark.Count; i++) {
            ECS.Character currChar = _charactersWithHomeOnLandmark[i];
            if(currChar.role.roleType == charRole) {
                characters.Add(currChar);
            }
        }
        return characters;
    }
    #endregion

    #region Materials
    /*
     Settlements only keep track of Materials that it has access to. 
     This is determined by the resources in his region that has gathering 
     structure and the minimum required civilians of his faction.
         */
    public bool HasAccessToMaterial(MATERIAL material) {
        for (int i = 0; i < _ownedLandmarks.Count; i++) {
            BaseLandmark currLandmark = _ownedLandmarks[i];
            if (currLandmark is ResourceLandmark) {
                ResourceLandmark currResLandmark = currLandmark as ResourceLandmark;
                if (currResLandmark.materialOnLandmark == material && currResLandmark.civilians >= GetMinimumCivilianRequirement()) {
                    return true;
                }
            }
        }
        return false;
    }
    public MATERIAL GetMaterialFor(PRODUCTION_TYPE productionType) {
        List<MATERIAL> preferredMats = _owner.productionPreferences[productionType].prioritizedMaterials;
        List<MATERIAL> materialReference;
        switch (productionType) {
            case PRODUCTION_TYPE.WEAPON:
                materialReference = ProductionManager.Instance.weaponMaterials;
                break;
            case PRODUCTION_TYPE.ARMOR:
                materialReference = ProductionManager.Instance.armorMaterials;
                break;
            case PRODUCTION_TYPE.CONSTRUCTION:
                materialReference = ProductionManager.Instance.constructionMaterials;
                break;
            case PRODUCTION_TYPE.TRAINING:
                materialReference = ProductionManager.Instance.trainingMaterials;
                break;
            default:
                throw new System.Exception("No material reference for " + productionType.ToString());
        }
        for (int i = 0; i < preferredMats.Count; i++) {
            MATERIAL currMat = preferredMats[i];
            if (materialReference.Contains(currMat)) {
                //if (HasAccessToMaterial(currMat)) { //Do not check if the settlement has access to the material for now
                    return currMat;
                //}
            }
        }
        return MATERIAL.NONE;
    }
    #endregion

    #region Landmarks
    public void AddLandmarkAsOwned(BaseLandmark landmark) {
        if (!_ownedLandmarks.Contains(landmark)) {
            _ownedLandmarks.Add(landmark);
        }
    }
    public void RemoveLandmarkAsOwned(BaseLandmark landmark) {
        _ownedLandmarks.Remove(landmark);
    }
    #endregion

    #region Items
    /*
     Produce an item for a character.
     This will automatically reduce the materials used in the settlement,
     and the gold of the character.
         */
    public ECS.Item ProduceItemForCharacter(EQUIPMENT_TYPE specificItem, ECS.Character character) {
        ITEM_TYPE itemType = Utilities.GetItemTypeOfEquipment(specificItem);
        MATERIAL matToUse = GetMaterialForItem(itemType, specificItem, character);
        if(matToUse != MATERIAL.NONE) {
            character.AdjustGold(-ItemManager.Instance.GetGoldCostOfItem(itemType, matToUse));
            return ItemManager.Instance.CreateNewItemInstance(matToUse, specificItem);
        }
        return null;
    }
    private MATERIAL GetMaterialForItem(ITEM_TYPE itemType, EQUIPMENT_TYPE equipmentType, ECS.Character character) {
        List<MATERIAL> elligibleMaterials = new List<MATERIAL>();
        List<MATERIAL> preferredMaterials = new List<MATERIAL>();
        if (itemType == ITEM_TYPE.ARMOR) {
            elligibleMaterials = ProductionManager.Instance.armorMaterials;
            preferredMaterials = _owner.productionPreferences[PRODUCTION_TYPE.ARMOR].prioritizedMaterials;
        } else if (itemType == ITEM_TYPE.WEAPON) {
			elligibleMaterials = ProductionManager.Instance.weaponMaterials;
            preferredMaterials = _owner.productionPreferences[PRODUCTION_TYPE.WEAPON].prioritizedMaterials;
        }
        for (int i = 0; i < preferredMaterials.Count; i++) {
            MATERIAL currMat = preferredMaterials[i];
            if (elligibleMaterials.Contains(currMat)) { //check if the curr preferred material is elligible for the item type
                int goldCost = ItemManager.Instance.GetGoldCostOfItem(itemType, currMat);
                if (HasAccessToMaterial(currMat) && character.gold >= goldCost) {
                    //check if this settlement has enough resources to make the item
                    return currMat;
                }
            }
        }
        return MATERIAL.NONE;
    }
    //private int GetCostToProduceItem(ITEM_TYPE itemType, EQUIPMENT_TYPE equipmentType) {
    //    if (itemType == ITEM_TYPE.ARMOR) {
    //        ArmorProduction productionDetails = ProductionManager.Instance.GetArmorProduction((ARMOR_TYPE)equipmentType);
    //        return productionDetails.production.resourceCost;
    //    } else if (itemType == ITEM_TYPE.WEAPON) {
    //        WeaponProduction productionDetails = ProductionManager.Instance.GetWeaponProduction((WEAPON_TYPE)equipmentType);
    //        return productionDetails.production.resourceCost;
    //    }
    //    return 0;
    //}
    #endregion

	#region Save Landmark
	//internal void SaveALandmark(BaseLandmark landmarkToSave){
	//	SaveLandmark saveLandmarkQuest = new SaveLandmark (this, landmarkToSave);
	//	saveLandmarkQuest.SetSettlement (this);
	//	AddNewQuest (saveLandmarkQuest);
	//}
	//internal void CancelSaveALandmark(BaseLandmark landmarkToSave){
		//for (int i = 0; i < _questBoard.Count; i++) {
			//if(_questBoard[i].questType == QUEST_TYPE.SAVE_LANDMARK){
			//	SaveLandmark saveLandmark = (SaveLandmark)_questBoard [i];
			//	if(saveLandmark.target.id == landmarkToSave.id){
			//		if(saveLandmark.isAccepted){
			//			saveLandmark.assignedParty.GoBackToQuestGiver (TASK_STATUS.FAIL);
			//		}else{
			//			RemoveQuestFromBoard (saveLandmark);
			//			RemoveQuest (saveLandmark);
			//		}
			//		break;
			//	}
			//}
		//}
	//}
	#endregion

	#region Crater
	private void EmitPsytoxin(){
		Region currRegion = this.tileLocation.region;
		for (int i = 0; i < currRegion.adjacentRegions.Count; i++) {
			Region adjacentRegion = currRegion.adjacentRegions [i];
			for (int j = 0; j < adjacentRegion.landmarks.Count; j++) {
				if(adjacentRegion.landmarks[j].charactersAtLocation.Count > 0){
					for (int k = 0; k < adjacentRegion.landmarks[j].charactersAtLocation.Count; k++) {
						ICombatInitializer combatInitializer = adjacentRegion.landmarks [j].charactersAtLocation [k];
						if(combatInitializer is Party){
							Party party = (Party)combatInitializer;
							for (int l = 0; l < party.partyMembers.Count; l++) {
								party.partyMembers [l].AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);
							}
						}else if(combatInitializer is ECS.Character){
							ECS.Character character = (ECS.Character)combatInitializer;
							character.AssignTag (CHARACTER_TAG.MILD_PSYTOXIN);
						}
					}
				}
			}
		}
	}
	public void AdjustNumOfPsytoxinated(int amount){
		_numOfPsytoxinated += amount;
		if(_numOfPsytoxinated <= 0){
			_numOfPsytoxinated = 0;
			DestroyLandmark (false);
		}
	}
	public void SetNumOfPsytoxinated(int amount){
		_numOfPsytoxinated = amount;
	}
	private void DestroyCrater(){
		Messenger.RemoveListener ("Psytoxinated", ListenPsytoxinated);
		Messenger.RemoveListener ("Unpsytoxinated", ListenUnpsytoxinated);
		LandmarkManager.Instance.craterLandmark = null;
		ChangeLandmarkType (LANDMARK_TYPE.TOWN);
		Initialize ();
	}
	public void ListenPsytoxinated(){
		AdjustNumOfPsytoxinated (1);
	}
	public void ListenUnpsytoxinated(){
		AdjustNumOfPsytoxinated (-1);
	}
    #endregion

    #region Goblin Camp
    private Dictionary<int, List<TECHNOLOGY>> classTechnologyChoices = new Dictionary<int, List<TECHNOLOGY>>() {
            {0, new List<TECHNOLOGY>(){
                TECHNOLOGY.ARCHER_CLASS,
                TECHNOLOGY.BOW_MAKING,
                TECHNOLOGY.WILDLING_CLASS,
                TECHNOLOGY.AXE_MAKING,
                TECHNOLOGY.ROGUE_CLASS,
                TECHNOLOGY.DAGGER_MAKING,
                TECHNOLOGY.GOBLIN_LANGUAGE }
            },
            {1, new List<TECHNOLOGY>(){
                TECHNOLOGY.SWORDSMAN_CLASS,
                TECHNOLOGY.SWORD_MAKING,
                TECHNOLOGY.MAGE_CLASS,
                TECHNOLOGY.STAFF_MAKING,
                TECHNOLOGY.WILDLING_CLASS,
                TECHNOLOGY.AXE_MAKING,
                TECHNOLOGY.GOBLIN_LANGUAGE }
            },
            {2, new List<TECHNOLOGY>(){
                TECHNOLOGY.ARCHER_CLASS,
                TECHNOLOGY.BOW_MAKING,
                TECHNOLOGY.SWORDSMAN_CLASS,
                TECHNOLOGY.SWORD_MAKING,
                TECHNOLOGY.SPEARMAN_CLASS,
                TECHNOLOGY.AXE_MAKING,
                TECHNOLOGY.GOBLIN_LANGUAGE }
            },
            {3, new List<TECHNOLOGY>(){
                TECHNOLOGY.SPEARMAN_CLASS,
                TECHNOLOGY.SPEAR_MAKING,
                TECHNOLOGY.ROGUE_CLASS,
                TECHNOLOGY.DAGGER_MAKING,
                TECHNOLOGY.MAGE_CLASS,
                TECHNOLOGY.STAFF_MAKING,
                TECHNOLOGY.GOBLIN_LANGUAGE }
            }
        };
    private List<TECHNOLOGY> commonPoolTech = new List<TECHNOLOGY>() {
        TECHNOLOGY.CHEST_ARMOR_MAKING,
        TECHNOLOGY.LEGGINGS_MAKING,
        TECHNOLOGY.HELMET_MAKING,
        TECHNOLOGY.GLOVE_MAKING,
        TECHNOLOGY.BOOT_MAKING,
        TECHNOLOGY.BASIC_FARMING,
        TECHNOLOGY.BASIC_HUNTING,
        TECHNOLOGY.BASIC_MINING,
        TECHNOLOGY.BASIC_WOODCUTTING,
        TECHNOLOGY.BASIC_QUARRYING,
        TECHNOLOGY.ELVEN_LANGUAGE,
        TECHNOLOGY.HUMAN_LANGUAGE,
        TECHNOLOGY.TROLL_LANGUAGE
    };
    private List<TECHNOLOGY> rarePoolTech = new List<TECHNOLOGY>() {
        TECHNOLOGY.ADVANCED_FARMING,
        TECHNOLOGY.ADVANCED_HUNTING,
        TECHNOLOGY.ADVANCED_MINING,
        TECHNOLOGY.ADVANCED_WOODCUTTING,
        TECHNOLOGY.ADVANCED_QUARRYING,
        TECHNOLOGY.RANGER_CLASS,
        TECHNOLOGY.BATTLEMAGE_CLASS,
        TECHNOLOGY.SCOUT_CLASS,
        TECHNOLOGY.BARBARIAN_CLASS,
        TECHNOLOGY.KNIGHT_CLASS,
        TECHNOLOGY.ARCANIST_CLASS,
        TECHNOLOGY.NIGHTBLADE_CLASS
    };
    private void GenerateGoblinCampTechnologies() {
        List<TECHNOLOGY> generatedTech = new List<TECHNOLOGY>();
        generatedTech.AddRange(classTechnologyChoices[Random.Range(0, 4)]); //Class technologies select from the sets
        //Common Pool Tech (randomly select 4)
        for (int i = 0; i < 4; i++) {
            TECHNOLOGY chosenCommonTech = commonPoolTech[Random.Range(0, commonPoolTech.Count)];
            generatedTech.Add(chosenCommonTech);
            commonPoolTech.Remove(chosenCommonTech);
        }
        //Rare Pool Tech (randomly select 1)
        generatedTech.Add(rarePoolTech[Random.Range(0, rarePoolTech.Count)]);
        for (int i = 0; i < generatedTech.Count; i++) {
            TECHNOLOGY currTech = generatedTech[i];
            _technologies[currTech] = true;
        }
    }
    #endregion
}
