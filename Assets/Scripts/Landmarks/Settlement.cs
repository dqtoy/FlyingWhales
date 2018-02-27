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

    private List<Quest> _questBoard;
	private WeightedDictionary<MATERIAL> _materialWeights;

    private const int CHARACTER_LIMIT = 10;

    private RACE _producingPopulationFor;
    private float _currentPopulationProduction;

    #region getters/setters
    public List<Quest> questBoard {
        get { return _questBoard; }
    }
    public List<BaseLandmark> ownedLandmarks {
        get { return _ownedLandmarks; }
    }
	public override int totalPopulation {
		get { return civilians + CharactersCount() + this._ownedLandmarks.Sum(x => x.civilians); }
	}
    public List<MATERIAL> availableMaterials {
        get { return _ownedLandmarks.Where(x => x is ResourceLandmark && x.civilians >= x.GetMinimumCivilianRequirement()).Select(x => (x as ResourceLandmark).materialOnLandmark).ToList(); }
    }
    #endregion

    public Settlement(HexTile location, LANDMARK_TYPE specificLandmarkType) : base(location, specificLandmarkType) {
        _canBeOccupied = true;
        _isHidden = false;
        _questBoard = new List<Quest>();
		_ownedLandmarks = new List<BaseLandmark>();
		_materialWeights = new WeightedDictionary<MATERIAL>();
        _producingPopulationFor = RACE.NONE;
//		ConstructNeededMaterials ();
    }

    #region Ownership
    public override void OccupyLandmark(Faction faction) {
        base.OccupyLandmark(faction);
//		foreach (MATERIAL material in _materialsInventory.Keys) {
//			if(faction.GetHighestMaterialPriority(PRODUCTION_TYPE.WEAPON) == material){
//				_materialsInventory [material].capacity += 200;
//			}
//			if(faction.GetHighestMaterialPriority(PRODUCTION_TYPE.ARMOR) == material){
//				_materialsInventory [material].capacity += 200;
//			}
//			if(faction.GetHighestMaterialPriority(PRODUCTION_TYPE.CONSTRUCTION) == material){
//				_materialsInventory [material].capacity += 200;
//			}
//			if(faction.GetHighestMaterialPriority(PRODUCTION_TYPE.TRAINING) == material){
//				_materialsInventory [material].capacity += 200;
//			}
//
//			if(_materialsInventory [material].capacity == 0){
//				_materialsInventory [material].capacity = 200;
//			}
//		}
        if (location.isHabitable) {
            //Create structures on location
            faction.AddSettlement(this);
            location.region.HighlightRegionTiles(faction.factionColor, 69f / 255f);
            location.CreateStructureOnTile(faction, STRUCTURE_TYPE.CITY);
            location.emptyCityGO.SetActive(false);
            _landmarkName = RandomNameGenerator.Instance.GenerateCityName(faction.race);
            _producingPopulationFor = GetRaceBasedOnProportion();
        }
		//Start OldQuest.Quest Creation
		//ScheduleUpdateAvailableMaterialsToGet ();
		//ScheduleUpdateNeededMaterials ();
		//ScheduleMonthlyQuests ();
		//TrainCharacterInSettlement(); //Start Character Creation Process
        IncreasePopulationPerMonth(); //Start Population Increase Process
    }
    public override void UnoccupyLandmark() {
        base.UnoccupyLandmark();
        _owner.RemoveSettlement(this);
    }
    #endregion

    #region Characters
    protected void TrainCharacterInSettlement(){
		bool canTrainCharacter = false;
		MATERIAL materialToUse = MATERIAL.NONE;
		if (civilians >= 1 && _charactersWithHomeOnLandmark.Count < CHARACTER_LIMIT) {
			//Check first if the settlement has enough civilians to create a new character
			//and that it has not exceeded the max number of characters that consider this settlement as home
			WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary(this.owner, this);
			if (characterRoleProductionDictionary.Count > 0 && characterRoleProductionDictionary.GetTotalOfWeights() > 0) {
				roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights();
				if(roleToCreate != CHARACTER_ROLE.NONE){
					if (Utilities.IsRoleClassless (roleToCreate)) {
						classToCreate = CHARACTER_CLASS.NONE;
						if(CanProduceClass(classToCreate, ref materialToUse)){
							canTrainCharacter = true;
						}
					} else {
						WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary (this, ref materialToUse);
						if (characterClassProductionDictionary.Count > 0 && characterClassProductionDictionary.GetTotalOfWeights () > 0) {
							classToCreate = characterClassProductionDictionary.PickRandomElementGivenWeights ();
							canTrainCharacter = true;
						}else{
							classToCreate = CHARACTER_CLASS.NONE;
						}
					}
				}
			} else {
				roleToCreate = CHARACTER_ROLE.NONE;
				classToCreate = CHARACTER_CLASS.NONE;
			}
		} else {
			roleToCreate = CHARACTER_ROLE.NONE;
			classToCreate = CHARACTER_CLASS.NONE;
		}
		if (canTrainCharacter && materialToUse != MATERIAL.NONE) {
//			if(TrainNewCharacter (roleToCreate, classToCreate, materialToUse)){
//				return;
//			}
			TrainNewCharacter (roleToCreate, classToCreate, materialToUse);
			return;
		}
		ScheduleTrainCharacter ();
	}
	private void ScheduleTrainCharacter(){
		GameDate dueDate = new GameDate(GameManager.Instance.month, 2, GameManager.Instance.year);
		dueDate.AddMonths (1);
		SchedulingManager.Instance.AddEntry(dueDate, () => TrainCharacterInSettlement());
	}
    /*
     At the start of the month, the settlement will
     decide what character class and role to create.
         */
    protected void DecideCharacterToCreate() {
		if (civilians >= 1 && _charactersWithHomeOnLandmark.Count < CHARACTER_LIMIT) {
            //Check first if the settlement has enough civilians to create a new character
            //and that it has not exceeded the max number of characters that consider this settlement as home
			MATERIAL material = MATERIAL.NONE;
            WeightedDictionary<CHARACTER_ROLE> characterRoleProductionDictionary = LandmarkManager.Instance.GetCharacterRoleProductionDictionary(this.owner, this);
			WeightedDictionary<CHARACTER_CLASS> characterClassProductionDictionary = LandmarkManager.Instance.GetCharacterClassProductionDictionary(this, ref material);
            if (characterRoleProductionDictionary.GetTotalOfWeights() > 0 && characterClassProductionDictionary.GetTotalOfWeights() > 0) {
                roleToCreate = characterRoleProductionDictionary.PickRandomElementGivenWeights();
				if(Utilities.IsRoleClassless(roleToCreate)){
					classToCreate = CHARACTER_CLASS.NONE;
				}else{
					classToCreate = characterClassProductionDictionary.PickRandomElementGivenWeights();
				}
            } else {
                roleToCreate = CHARACTER_ROLE.NONE;
                classToCreate = CHARACTER_CLASS.NONE;
            }
        } else {
            roleToCreate = CHARACTER_ROLE.NONE;
            classToCreate = CHARACTER_CLASS.NONE;
        }

        GameDate createCharacterDate = new GameDate(GameManager.Instance.month, GameManager.daysInMonth[GameManager.Instance.month], GameManager.Instance.year);
        SchedulingManager.Instance.AddEntry(createCharacterDate.month, createCharacterDate.day, createCharacterDate.year, () => CreateScheduledCharacter());
    }
    /*
     At the end of the month, the settlement will create a
     character given the attributes it decided at the start of the month.
         */
    protected void CreateScheduledCharacter() {
        if(roleToCreate != CHARACTER_ROLE.NONE) {
            if (civilians >= 1 && _charactersWithHomeOnLandmark.Count < CHARACTER_LIMIT) { 
                //Check first if the settlement has enough civilians to create a new character
                //and that it has not exceeded the max number of characters that consider this settlement as home
				CreateNewCharacter(roleToCreate, Utilities.NormalizeString(classToCreate.ToString()));
            }
        }
        GameDate decideCharacterDate = new GameDate(GameManager.Instance.month, GameManager.Instance.days, GameManager.Instance.year);
        decideCharacterDate.AddDays(1);
        SchedulingManager.Instance.AddEntry(decideCharacterDate.month, decideCharacterDate.day, decideCharacterDate.year, () => DecideCharacterToCreate());
    }
	public ECS.Character TrainCharacter(CHARACTER_ROLE roleType, CHARACTER_CLASS classType, MATERIAL materialUsed, RACE raceOfChar){
        AddHistory ("Completed training for a " + Utilities.NormalizeString (roleType.ToString ()) + " " + (classType != CHARACTER_CLASS.NONE ? Utilities.NormalizeString (classType.ToString ()) : "Classless") + ".");
		int trainingStatBonus = MaterialManager.Instance.materialsLookup [materialUsed].trainingStatBonus;
		ECS.Character newCharacter = CharacterManager.Instance.CreateNewCharacter(roleType, Utilities.NormalizeString(classType.ToString()), raceOfChar, trainingStatBonus);
		newCharacter.SetFaction(_owner);
		newCharacter.SetHome (this);
		this.owner.AddNewCharacter(newCharacter);
		this.AddCharacterToLocation(newCharacter, false);
		this.AddCharacterHomeOnLandmark(newCharacter);
		newCharacter.DetermineAction();
		UIManager.Instance.UpdateFactionSummary();
		TrainCharacterInSettlement ();
		return newCharacter;
	}

	public void TrainNewCharacter(CHARACTER_ROLE charRole, CHARACTER_CLASS charClass, MATERIAL materialToUse){
		TrainingRole trainingRole = ProductionManager.Instance.GetTrainingRole(charRole);
		TrainingClass trainingClass = ProductionManager.Instance.GetTrainingClass(charClass);
        RACE raceForChar = GetRaceBasedOnProportion();

        //ReduceTotalFoodCount (trainingRole.production.foodCost);
        //AdjustPopulation(-trainingRole.production.civilianCost);
        AdjustCivilians(raceForChar, -trainingRole.production.civilianCost);
        //AdjustMaterial (materialToUse, -trainingClass.production.resourceCost);

		AddHistory ("Started training a " + Utilities.NormalizeString (charRole.ToString ()) + " " + ((charClass !=	CHARACTER_CLASS.NONE) ? Utilities.NormalizeString (charClass.ToString ()) : "Classless") + ".");
		GameDate trainCharacterDate = GameManager.Instance.Today ();
		trainCharacterDate.AddDays (trainingRole.production.duration + trainingClass.production.duration);
		SchedulingManager.Instance.AddEntry (trainCharacterDate, () => TrainCharacter (charRole, charClass, materialToUse, raceForChar));
//		return false;
	}
    /*
     Does the settlement have the required technology
     to produce a class?
         */
    public bool CanProduceClass(CHARACTER_CLASS charClass, ref MATERIAL material) {
        if(_owner == null) {
            return false;
        }
        TECHNOLOGY neededTech = Utilities.GetTechnologyForCharacterClass(charClass);
        List<MATERIAL> trainingPreference = this._owner.productionPreferences[PRODUCTION_TYPE.TRAINING].prioritizedMaterials;
        for (int i = 0; i < trainingPreference.Count; i++) {
            MATERIAL currMat = trainingPreference[i];
            if (ProductionManager.Instance.trainingMaterials.Contains(currMat) && HasAccessToMaterial(currMat)) {
                material = trainingPreference[i];
            }
        }
        //if (material == MATERIAL.NONE) {
        //    return false; //this settlement has no access to materials for training.
        //}
        if (neededTech == TECHNOLOGY.NONE) {
            return true;
        } else {
            return _technologies[neededTech];
        }
        //if (neededTech == TECHNOLOGY.NONE || _technologies[neededTech]) {
        //    TrainingClass trainingClass = ProductionManager.Instance.trainingClassesLookup[charClass];
        //    List<MATERIAL> trainingPreference = this._owner.productionPreferences[PRODUCTION_TYPE.TRAINING].prioritizedMaterials;
        //    for (int i = 0; i < trainingPreference.Count; i++) {
        //        if (ProductionManager.Instance.trainingMaterials.Contains(trainingPreference[i]) && trainingClass.production.resourceCost <= _materialsInventory[trainingPreference[i]].count) {
        //            material = trainingPreference[i];
        //            return true;
        //        }
        //    }
        //}
        //return false;
    }
    /*
     Does the settlement have the required technology
     to produce a class?
         */
    public bool CanProduceClass(CHARACTER_CLASS charClass) {
        if (_owner == null) {
            return false;
        }
        TECHNOLOGY neededTech = Utilities.GetTechnologyForCharacterClass(charClass);
        //List<MATERIAL> trainingPreference = this._owner.productionPreferences[PRODUCTION_TYPE.TRAINING].prioritizedMaterials;
        //for (int i = 0; i < trainingPreference.Count; i++) {
        //    MATERIAL currMat = trainingPreference[i];
        //    if (ProductionManager.Instance.trainingMaterials.Contains(currMat) && HasAccessToMaterial(currMat)) {
        //        material = trainingPreference[i];
        //    }
        //}
        //if (material == MATERIAL.NONE) {
        //    return false; //this settlement has no access to materials for training.
        //}
        if (neededTech == TECHNOLOGY.NONE) {
            return true;
        } else {
            return _technologies[neededTech];
        }
        //if (neededTech == TECHNOLOGY.NONE || _technologies[neededTech]) {
        //    TrainingClass trainingClass = ProductionManager.Instance.trainingClassesLookup[charClass];
        //    List<MATERIAL> trainingPreference = this._owner.productionPreferences[PRODUCTION_TYPE.TRAINING].prioritizedMaterials;
        //    for (int i = 0; i < trainingPreference.Count; i++) {
        //        if (ProductionManager.Instance.trainingMaterials.Contains(trainingPreference[i]) && trainingClass.production.resourceCost <= _materialsInventory[trainingPreference[i]].count) {
        //            material = trainingPreference[i];
        //            return true;
        //        }
        //    }
        //}
        //return false;
    }
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

    #region Population
    /*
     Reschedule monthly population growth.
     NOTE: Not to be used when inititally scheduling monthly population increase!
         */
    private void ScheduleMonthlyPopulationIncrease() {
        GameDate increasePopulationDate = GameManager.Instance.Today();
        increasePopulationDate.AddMonths(1);
        SchedulingManager.Instance.AddEntry(increasePopulationDate.month, 1, increasePopulationDate.year, () => IncreasePopulationPerMonth());
    }
    /*
     Increase population of this settlement every end of the month.
         */
    private void IncreasePopulationPerMonth() {
        float populationGrowth = this.totalPopulation * this.location.region.populationGrowth;
        _currentPopulationProduction += populationGrowth;
        if (_currentPopulationProduction >= 1f) {
            float excess = _currentPopulationProduction - 1f;
            AdjustCivilians(_producingPopulationFor, 1);
            _producingPopulationFor = GetRaceBasedOnProportion();
            _currentPopulationProduction = excess;
        }
        //AdjustPopulation(populationGrowth);
        UIManager.Instance.UpdateFactionSummary();
        ScheduleMonthlyPopulationIncrease();
    }
    #endregion

    #region Quests
    internal void AddQuestToBoard(Quest quest) {
        _questBoard.Add(quest);
        //quest.OnQuestPosted(); //Call On OldQuest.Quest Posted after quest is posted
    }
    internal void RemoveQuestFromBoard(Quest quest) {
        _questBoard.Remove(quest);
    }
	internal Quest GetQuestByID(int id){
		for (int i = 0; i < _questBoard.Count; i++) {
			if(_questBoard[i].id == id){
				return _questBoard [i];
			}
		}
		return null;
	}
    internal List<Quest> GetQuestsOnBoardByType(QUEST_TYPE questType) {
        List<Quest> quests = new List<Quest>();
        for (int i = 0; i < _questBoard.Count; i++) {
            Quest currQuest = _questBoard[i];
            if(currQuest.questType == questType) {
                quests.Add(currQuest);
            }
        }
        return quests;
    }
	internal int GetNumberOfQuestsOnBoardByType(QUEST_TYPE questType){
		int count = 0;
		for (int i = 0; i < _questBoard.Count; i++) {
			Quest currQuest = _questBoard[i];
			if(currQuest.questType == questType) {
				count++;
			}
		}
		return count;
	}
	//private void ScheduleUpdateAvailableMaterialsToGet(){
	//	GameDate newSched = GameManager.Instance.Today();
	//	newSched.AddMonths (1);
	//	newSched.SetDay (GameManager.daysInMonth [newSched.month] - 1);
	//	SchedulingManager.Instance.AddEntry (newSched, () => UpdateAvailableMaterialsToGet ());
	//}
	//private void ScheduleUpdateNeededMaterials(){
	//	GameDate newSched = GameManager.Instance.Today();
	//	newSched.AddMonths (1);
	//	newSched.SetDay (GameManager.daysInMonth [newSched.month]);
	//	SchedulingManager.Instance.AddEntry (newSched, () => UpdateNeededMaterials ());
	//}
	private void ScheduleMonthlyQuests(){
		GameDate dueDate = new GameDate(GameManager.Instance.month, 1, GameManager.Instance.year);
		dueDate.AddMonths (1);
		SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
	}
	//private void UpdateAvailableMaterialsToGet(){
	//	foreach (MATERIAL material in _materialsInventory.Keys) {
	//		_materialsInventory [material].availableExcessOfOtherSettlements = (this._owner.settlements.Sum (x => x.materialsInventory [material].excess)) - _materialsInventory[material].excess;
	//		_materialsInventory [material].availableExcessOfResourceLandmarks = this._ownedLandmarks.Sum (x => x.materialsInventory [material].excess);
	//		_materialsInventory [material].capacity = 0;
	//		_materialsInventory [material].isNeeded = false;
	//	}
	//	ScheduleUpdateAvailableMaterialsToGet ();
	//}
	//private void UpdateNeededMaterials(){
	//	int count = this._owner.productionPreferences [PRODUCTION_TYPE.WEAPON].prioritizedMaterials.Count;
	//	List<PRODUCTION_TYPE> productionTypes = this._owner.productionPreferences.Keys.ToList ();
	//	for (int i = 0; i < count; i++) {
	//		for (int j = 0; j < productionTypes.Count; j++) {
	//			MATERIAL material = this._owner.productionPreferences [productionTypes[j]].prioritizedMaterials [i];
	//			if((_materialsInventory[material].availableExcessOfOtherSettlements + _materialsInventory [material].availableExcessOfResourceLandmarks) > 0){
	//				_materialsInventory [material].capacity += 200;
	//				_materialsInventory [material].isNeeded = true;
	//				productionTypes.RemoveAt (j);
	//				j--;
	//			}
	//		}
	//		if(productionTypes.Count <= 0){
	//			break;
	//		}
	//	}
	//	ScheduleUpdateNeededMaterials ();
	//}
	//private MATERIAL GetObtainMaterialTarget(){
	//	_materialWeights.Clear ();
	//	foreach (MATERIAL material in _materialsInventory.Keys) {
	//		if (_materialsInventory [material].isNeeded) {
	//			if (_materialsInventory [material].availableExcessOfOtherSettlements > 0 || materialsInventory [material].availableExcessOfResourceLandmarks > 0) {
	//				if (_materialsInventory [material].count < _materialsInventory [material].capacity) {
	//					_materialWeights.AddElement (material, 200);
	//				} else {
	//					_materialWeights.AddElement (material, 30);
	//				}
	//			}
	//		}else{
	//			if(_materialsInventory [material].availableExcessOfResourceLandmarks > 0){
	//				_materialWeights.AddElement (material, 60);
	//			}
	//		}
	//	}
	//	if(_materialWeights.Count > 0){
	//		return _materialWeights.PickRandomElementGivenWeights ();
	//	}
	//	return MATERIAL.NONE;
	//}
	private MATERIAL RepickObtainMaterialTarget(){
		if(_materialWeights.Count > 0){
			return _materialWeights.PickRandomElementGivenWeights ();
		}
		return MATERIAL.NONE;
	}
	private void GenerateMonthlyQuests() {
//		WeightedDictionary<OldQuest.Quest> questDictionary = new WeightedDictionary<OldQuest.Quest>();
//		questDictionary.LogDictionaryValues("OldQuest.Quest Creation Weights: ");
//		if(questDictionary.GetTotalOfWeights() > 0) {
//			OldQuest.Quest chosenQuestToCreate = questDictionary.PickRandomElementGivenWeights();
//			AddNewQuest(chosenQuestToCreate);
//		}
		CreateQuest(QUEST_TYPE.OBTAIN_MATERIAL);
		ScheduleMonthlyQuests();
	}
	private void CreateQuest(QUEST_TYPE questType){
		int noOfQuestsOnBoard = GetNumberOfQuestsOnBoardByType (questType);
		int maxNoOfQuests = GetMaxQuests (questType);
		//if(questType == QUEST_TYPE.OBTAIN_MATERIAL){
		//	if(noOfQuestsOnBoard < maxNoOfQuests){
  //              MATERIAL material = GetObtainMaterialTarget();
  //              MATERIAL previousMaterial = MATERIAL.NONE;
		//		for (int i = 0; i < 2; i++) {
		//			if(material != MATERIAL.NONE && material != previousMaterial && !AlreadyHasQuestOfType(QUEST_TYPE.OBTAIN_MATERIAL, material)){
		//				previousMaterial = material;
		//				BaseLandmark target = GetTargetObtainMaterial (material);
		//				if(target != null){
		//					ObtainMaterial obtainMaterialQuest = new ObtainMaterial (this, material, target);
		//					obtainMaterialQuest.SetSettlement (this);
		//					AddNewQuest (obtainMaterialQuest);
		//					if((noOfQuestsOnBoard + 1) < maxNoOfQuests){
		//						material = RepickObtainMaterialTarget ();
		//					}else{
		//						break;
		//					}
		//				}else{
		//					if(i == 0){
		//						material = RepickObtainMaterialTarget ();
		//					}
		//				}
		//			}else{
		//				break;
		//			}
		//		}
		//	}
		//}
	}
	private int GetMaxQuests(QUEST_TYPE questType){
		if (questType == QUEST_TYPE.OBTAIN_MATERIAL) {
			return 3;
		}
		return 0;
	}
    //private BaseLandmark GetTargetObtainMaterial(MATERIAL materialToObtain){
    //	WeightedDictionary<BaseLandmark> targetWeights = new WeightedDictionary<BaseLandmark> ();
    //	for (int i = 0; i < this.owner.settlements.Count; i++) {
    //		if(this.id == this.owner.settlements[i].id){
    //			for (int j = 0; j < this.ownedLandmarks.Count; j++) {
    //				if(this.ownedLandmarks[j].materialsInventory[materialToObtain].excess > 0){
    //					targetWeights.AddElement (this.ownedLandmarks [j], this.ownedLandmarks [j].materialsInventory [materialToObtain].excess);
    //				}
    //			}
    //		}else{
    //			if(this.owner.settlements[i].materialsInventory[materialToObtain].excess > 0){
    //				targetWeights.AddElement (this.owner.settlements[i], this.owner.settlements[i].materialsInventory [materialToObtain].excess);
    //			}
    //		}
    //	}
    //	if(targetWeights.Count > 0){
    //		return targetWeights.PickRandomElementGivenWeights ();
    //	}
    //	return null;
    //}
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
    public MATERIAL GetMaterialForConstruction() {
        List<MATERIAL> preferredMats = _owner.productionPreferences[PRODUCTION_TYPE.CONSTRUCTION].prioritizedMaterials;
        for (int i = 0; i < preferredMats.Count; i++) {
            MATERIAL currMat = preferredMats[i];
            if (ProductionManager.Instance.constructionMaterials.Contains(currMat)) {
                if (HasAccessToMaterial(currMat)) {
                    return currMat;
                }
                //if (HasAvailableMaterial(currMat, constructionData.production.resourceCost)) {
                //    return currMat; //Check if this landmark has a resource with the required amount, that can build the structure
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
	internal void SaveALandmark(BaseLandmark landmarkToSave){
		SaveLandmark saveLandmarkQuest = new SaveLandmark (this, landmarkToSave);
		saveLandmarkQuest.SetSettlement (this);
		AddNewQuest (saveLandmarkQuest);
	}
	internal void CancelSaveALandmark(BaseLandmark landmarkToSave){
		for (int i = 0; i < _questBoard.Count; i++) {
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
		}
	}
	#endregion
}
