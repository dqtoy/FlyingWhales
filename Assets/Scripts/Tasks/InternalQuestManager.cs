/*
 This is the Internal OldQuest.Quest Manager. Each Faction has one,
 it is responsible for generating new quests for each faction.
 Reference: https://trello.com/c/Wf38ZqLM/737-internal-manager-ai
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InternalQuestManager : TaskCreator {

    private Faction _owner;

    private List<OldQuest.Quest> _activeQuests;

    #region getters/setters
    public Faction owner {
        get { return _owner; }
    }
    public List<OldQuest.Quest> activeQuests {
        get { return _activeQuests; }
    }
    #endregion

    public InternalQuestManager(Faction owner) {
        _owner = owner;
        _activeQuests = new List<OldQuest.Quest>();
        //if(owner is Tribe) {
        //    GameDate dueDate = new GameDate(GameManager.Instance.month, 15, GameManager.Instance.year);
        //    SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
        //}
    }

    /*
     Get the maximum number of active quests a faction
     can have.
         */
    private int GetMaxActiveQuests() {
        if(_owner.factionType == FACTION_TYPE.MINOR) {
            return 1 + _owner.settlements.Count;
        } else {
            switch (_owner.factionSize) {
                case FACTION_SIZE.SMALL:
                    return 2 + _owner.settlements.Count;
                case FACTION_SIZE.MEDIUM:
                    return 3 + _owner.settlements.Count;
                case FACTION_SIZE.LARGE:
                    return 4 + _owner.settlements.Count;
                default:
                    return 0;
            }
        }
    }

    #region OldQuest.Quest Generation
    /*
     At the start of each month, if the Faction has not yet reached the cap of active Internal Quests, 
     it will attempt to create a new one.
         */
    private void GenerateMonthlyQuests() {
        if(_activeQuests.Count < GetMaxActiveQuests()) {
            WeightedDictionary<OldQuest.Quest> questDictionary = GetQuestWeightedDictionary();
            questDictionary.LogDictionaryValues("OldQuest.Quest Creation Weights: ");
            if(questDictionary.GetTotalOfWeights() > 0) {
                OldQuest.Quest chosenQuestToCreate = questDictionary.PickRandomElementGivenWeights();
                AddNewQuest(chosenQuestToCreate);
            }
        }

        GameDate dueDate = GameManager.Instance.Today();
        dueDate.AddDays(15);
        SchedulingManager.Instance.AddEntry(dueDate, () => GenerateMonthlyQuests());
    }
    private WeightedDictionary<OldQuest.Quest> GetQuestWeightedDictionary() {
        WeightedDictionary<OldQuest.Quest> questWeights = new WeightedDictionary<OldQuest.Quest>();
        //Loop through each Region that the Faction has a Settlement in.
        for (int i = 0; i < _owner.settlements.Count; i++) {
            Settlement currSettlement = _owner.settlements[i];
            Region regionOfSettlement = currSettlement.location.region;

            //AddExpandWeights(questWeights, currSettlement, regionOfSettlement);
//            AddExploreTileWeights(questWeights, currSettlement, regionOfSettlement);
            //AddBuildStructureWeights(questWeights, currSettlement, regionOfSettlement);
            //AddExpeditionWeights(questWeights, currSettlement, regionOfSettlement);
        }
        return questWeights;
    }
//    private void AddExploreTileWeights(WeightedDictionary<OldQuest.Quest> questWeights, Settlement currSettlement, Region regionOfSettlement) {
//        for (int j = 0; j < regionOfSettlement.landmarks.Count; j++) {
//            object currConnection = regionOfSettlement.landmarks[j];
//            if (currConnection is BaseLandmark) {
//                BaseLandmark currLandmark = (BaseLandmark)currConnection;
//                if (currLandmark.isHidden && !currLandmark.isExplored) {
//                    if (currSettlement.GetQuestsOnBoardByType(QUEST_TYPE.EXPLORE_TILE).Count <= 0 && !AlreadyHasQuestOfType(QUEST_TYPE.EXPLORE_TILE, currLandmark)) {
//                        ExploreTile newExploreTileQuest = new ExploreTile(this, currLandmark);
//                        newExploreTileQuest.SetSettlement(currSettlement);
//                        questWeights.AddElement(newExploreTileQuest, GetExploreLandmarkWeight(currLandmark));
//                    }
//                }
//            }
//        }
//    }

    //private void AddExpandWeights(WeightedDictionary<OldQuest.Quest> questWeights, Settlement currSettlement, Region regionOfSettlement) {
    //    List<Region> checkedExpandRegions = new List<Region>();
    //    Construction constructionData = ProductionManager.Instance.GetConstructionDataForCity();
    //    if (currSettlement.CanAffordConstruction(constructionData) && !AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, currSettlement)) {
    //        //checkedExpandRegions.Clear();
    //        for (int j = 0; j < regionOfSettlement.connections.Count; j++) {
    //            object currConnection = regionOfSettlement.connections[j];
    //            if (currConnection is Region) {
    //                Region region = (Region)currConnection;
    //                if (!region.centerOfMass.isOccupied && !AlreadyHasQuestOfType(QUEST_TYPE.EXPAND, region.centerOfMass)) {
    //                    checkedExpandRegions.Add(region);
    //                }
    //            }
    //        }
    //        if (checkedExpandRegions.Count > 0) {
    //            Region chosenRegion = checkedExpandRegions[UnityEngine.Random.Range(0, checkedExpandRegions.Count)];
    //            Expand newExpandQuest = new Expand(this, chosenRegion.centerOfMass, currSettlement.location, currSettlement.GetMaterialForConstruction(constructionData), constructionData);
    //            newExpandQuest.SetSettlement(currSettlement);
    //            questWeights.AddElement(newExpandQuest, GetExpandWeight(currSettlement));
    //        }
    //    }
    //}
    //private void AddBuildStructureWeights(WeightedDictionary<OldQuest.Quest> questWeights, Settlement currSettlement, Region regionOfSettlement) {
    //    for (int i = 0; i < regionOfSettlement.tilesInRegion.Count; i++) {
    //        HexTile currTile = regionOfSettlement.tilesInRegion[i];
    //        //BaseLandmark currLandmark = regionOfSettlement.landmarks[i];
    //        if (currTile.materialOnTile != MATERIAL.NONE && !currTile.HasStructure() && currTile.landmarkOnTile == null) { //Loop through each Resource Tile that doesn't have any structures yet.
    //            int totalWeightForLandmark = 0;
    //            MATERIAL materialOnTile = currTile.materialOnTile;
    //            Materials materialData = MaterialManager.Instance.materialsLookup[materialOnTile];
    //            Construction constructionData = ProductionManager.Instance.GetConstruction(materialData.structure.name);
    //            if (currSettlement.HasTechnology(ProductionManager.Instance.GetConstruction(materialData.structure.name).technology) 
    //                && currSettlement.CanAffordConstruction(constructionData)
    //                && GetQuestsOfType(QUEST_TYPE.BUILD_STRUCTURE).Count <= 0
    //                && !AlreadyHasQuestOfType(QUEST_TYPE.BUILD_STRUCTURE, currTile)) {

    //                totalWeightForLandmark += 60; //Add 60 Weight to Build Structure if relevant technology is available
    //                totalWeightForLandmark -= 30 * regionOfSettlement.GetActivelyHarvestedMaterialsOfType(materialOnTile); //- Subtract 30 Weight to Build Structure for each Resource Tile of the same type already actively being harvested in the same region
    //                totalWeightForLandmark -= 10 * _owner.GetActivelyHarvestedMaterialsOfType(materialOnTile, regionOfSettlement); //- Subtract 10 Weight to Build Structure for each Resource Tile of the same type already actively being harvested in other regions owned by the same Tribe
    //                totalWeightForLandmark = Mathf.Max(0, totalWeightForLandmark);
    //                BuildStructure buildStructureQuest = new BuildStructure(this, currTile, currSettlement.GetMaterialForConstruction(constructionData), constructionData);
    //                buildStructureQuest.SetSettlement(currSettlement);
    //                questWeights.AddElement(buildStructureQuest, totalWeightForLandmark);
    //            }
    //        }
    //    }
    //}
    //private void AddExpeditionWeights(WeightedDictionary<OldQuest.Quest> questWeights, Settlement currSettlement, Region regionOfSettlement) {
    //    //Check if there is a category of Resource Type (Weapon, Armor, Construction, Training, Food) that the Settlement doesnt have any access to.
    //    PRODUCTION_TYPE[] allProdTypes = Utilities.GetEnumValues<PRODUCTION_TYPE>();
    //    for (int i = 0; i < allProdTypes.Length; i++) {
    //        PRODUCTION_TYPE currProdType = allProdTypes[i];
    //        if (!currSettlement.HasAccessTo(currProdType) ) {
    //            if (currSettlement.GetQuestsOnBoardByType(QUEST_TYPE.EXPEDITION).Count <= 0 && !AlreadyHasQuestOfType(QUEST_TYPE.EXPEDITION, currProdType.ToString())) {
    //                Expedition newExpedition = new Expedition(this, currProdType.ToString());
    //                newExpedition.SetSettlement(currSettlement);
    //                questWeights.AddElement(newExpedition, 100);//-Add 100 Weight for each category type
    //            }
    //        }
    //    }
    //    if(!currSettlement.HasAccessToFood()) {
    //        if (currSettlement.GetQuestsOnBoardByType(QUEST_TYPE.EXPEDITION).Count <= 0 && !AlreadyHasQuestOfType(QUEST_TYPE.EXPEDITION, "FOOD")) {
    //            Expedition newExpedition = new Expedition(this, "FOOD");
    //            newExpedition.SetSettlement(currSettlement);
    //            questWeights.AddElement(newExpedition, 100);//-Add 100 Weight for each category type
    //        }
    //    }
    //}

    private int GetExploreLandmarkWeight(BaseLandmark landmark) {
        int weight = 0;
        if (!landmark.isExplored) {
            weight += 50; //Add 50 Weight to Explore Tile Weight
        }
        return weight;
    }
	private int GetExpandWeight(BaseLandmark landmark) {
//		int weight = 1 + (5 * (region.landmarks.Count - 1));
//		for (int i = 0; i < region.connections.Count; i++) {
//			if(region.connections[i] is Region){
//				Region adjacentRegion = (Region)region.connections[i];
//				if (adjacentRegion.centerOfMass.landmarkOnTile.owner != null && adjacentRegion.centerOfMass.landmarkOnTile.owner.id == this._owner.id) {
//					int regionWeight = (int)(adjacentRegion.centerOfMass.landmarkOnTile.civilians - 40f);
//					if(regionWeight > 0){
//						weight += regionWeight;
//					}
//				}
//			}
//		}
		return 5;
	}
	internal void CreateExpandQuest(BaseLandmark originLandmark){
		HexTile unoccupiedTile = originLandmark.GetRandomAdjacentUnoccupiedTile ();
		if(unoccupiedTile != null){
            Settlement settlement = originLandmark as Settlement;
            //Construction constructionData = ProductionManager.Instance.GetConstructionDataForCity();
            MATERIAL matForConstruction = settlement.GetMaterialForConstruction();
            if(matForConstruction != MATERIAL.NONE) {
                Expand expand = new Expand(this, unoccupiedTile, originLandmark.location, matForConstruction);
                expand.SetSettlement((Settlement)originLandmark);
                AddNewQuest(expand);
            }
		}
	}
	internal void CreateExploreTileQuest(BaseLandmark landmarkToExplore){
//        ExploreTile exploreQuest = new ExploreTile(this, landmarkToExplore);
//        exploreQuest.SetSettlement((Settlement)landmarkToExplore.location.region.centerOfMass.landmarkOnTile);
//        AddNewQuest(exploreQuest);
    }
    internal void CreateBuildStructureQuest(BaseLandmark landmarkToExplore) {
        Construction constructionData = ProductionManager.Instance.GetConstruction((landmarkToExplore as ResourceLandmark).materialData.structure.name);
        Settlement settlement = (Settlement)landmarkToExplore.location.region.centerOfMass.landmarkOnTile;
        BuildStructure buildStructure = new BuildStructure(this, landmarkToExplore.location, settlement.GetMaterialForConstruction(), constructionData);
        buildStructure.SetSettlement(settlement);
        AddNewQuest(buildStructure);
    }
    #endregion

    #region OldQuest.Quest Management
    public void AddNewQuest(OldQuest.Quest quest) {
        if (!_activeQuests.Contains(quest)) {
            _activeQuests.Add(quest);
            _owner.AddNewQuest(quest);
            //if(quest.postedAt != null) {
            //    quest.postedAt.AddQuestToBoard(quest);
            //}
            //quest.ScheduleDeadline(); //Once a quest has been added to active quest, scedule it's deadline
        }
    }
    public void RemoveQuest(OldQuest.Quest quest) {
        _activeQuests.Remove(quest);
        _owner.RemoveQuest(quest);
    }
    public List<OldQuest.Quest> GetQuestsOfType(QUEST_TYPE questType) {
        List<OldQuest.Quest> quests = new List<OldQuest.Quest>();
        for (int i = 0; i < _activeQuests.Count; i++) {
            OldQuest.Quest currQuest = _activeQuests[i];
            if(currQuest.questType == questType) {
                quests.Add(currQuest);
            }
        }
        return quests;
    }
	public bool AlreadyHasQuestOfType(QUEST_TYPE questType, object identifier){
		for (int i = 0; i < _activeQuests.Count; i++) {
			OldQuest.Quest currQuest = _activeQuests[i];
			if(currQuest.questType == questType) {
				if(questType == QUEST_TYPE.EXPLORE_REGION){
					Region region = (Region)identifier;
					if(((ExploreRegion)currQuest).regionToExplore.id == region.id){
						return true;
					}
				} else if(questType == QUEST_TYPE.EXPAND){
					if(identifier is HexTile){
						HexTile hexTile = (HexTile)identifier;
						if(((Expand)currQuest).targetUnoccupiedTile.id == hexTile.id){
							return true;
						}
					}else if(identifier is BaseLandmark){
						BaseLandmark landmark = (BaseLandmark)identifier;
						if(((Expand)currQuest).originTile.id == landmark.location.id){
							return true;
						}
					}

				} else if (questType == QUEST_TYPE.BUILD_STRUCTURE) {
                    HexTile tile = (HexTile)identifier;
                    if (((BuildStructure)currQuest).target.id == tile.id) {
                        return true;
                    }
                } else if (questType == QUEST_TYPE.EXPEDITION) {
                    string productionType = (string)identifier;
                    if (((Expedition)currQuest).productionType.Equals(productionType)) {
                        return true;
                    }
				} else if (questType == QUEST_TYPE.SAVE_LANDMARK) {
					BaseLandmark target = (BaseLandmark)identifier;
					if (((SaveLandmark)currQuest).target.id == target.id) {
						return true;
					}
				}
            }
		}
		return false;
	}
    #endregion
}
