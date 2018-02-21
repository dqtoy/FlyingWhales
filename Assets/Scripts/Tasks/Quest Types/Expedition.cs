using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Expedition : OldQuest.Quest {

    private string _productionType;
    private HexTile _target;
    private int _gatheredAmt;

    #region getters/setters
    public string productionType {
        get { return _productionType; }
    }
    #endregion

    public Expedition(TaskCreator createdBy, string productionType) : base(createdBy, QUEST_TYPE.EXPEDITION) {
        _productionType = productionType;
    }

    private HexTile GetTargetHexTile() {
        WeightedDictionary<HexTile> tileWeights = new WeightedDictionary<HexTile>();
        List<HexTile> tilesInRange = _postedAt.location.GetTilesInRange(40); //Limit to within 250 tiles around the settlement
        PRODUCTION_TYPE prodTypeToUse;
        if (_productionType.Equals("FOOD")) {
            prodTypeToUse = PRODUCTION_TYPE.TRAINING;
        } else {
            prodTypeToUse = (PRODUCTION_TYPE)System.Enum.Parse(typeof(PRODUCTION_TYPE), _productionType);
        }

        //Search for nearby tiles with the required resource type that still has no structure
        for (int i = 0; i < tilesInRange.Count; i++) {
            HexTile currTile = tilesInRange[i];
            if(currTile.materialOnTile != MATERIAL.NONE && !currTile.HasStructure()) {
                if (_productionType.Equals("FOOD")) {
                    if (!MaterialManager.Instance.materialsLookup[currTile.materialOnTile].isEdible) {
                        continue; //the material on the tile is not edible, and this quest requires materials that are edible, skip this tile.
                    }
                } else {
                    if (!MaterialManager.Instance.CanMaterialBeUsedFor(currTile.materialOnTile, prodTypeToUse)) {
                        continue;
                    }
                }
                int weightForTile = 500;
                List<HexTile> path = PathGenerator.Instance.GetPath(_postedAt.location, currTile, PATHFINDING_MODE.NORMAL);
                if(path != null) {
                    weightForTile -= 2 * path.Count; //-2 weight for every tile distance away from settlement
                    Faction regionOwner = currTile.region.owner;
                    if (regionOwner != null) {
                        if (regionOwner.id != _postedAt.owner.id) {
                            FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(regionOwner, _postedAt.owner);
                            if (rel.relationshipStatus == RELATIONSHIP_STATUS.NEUTRAL) {
                                weightForTile -= 200; //-200 Weight if the tile's region is owned by a Neutral Tribe
                            } else if (rel.relationshipStatus == RELATIONSHIP_STATUS.FRIENDLY) {
                                weightForTile -= 300; //-300 Weight if the tile's region is owned by a Friendly Tribe
                            }
                        }
                    }

                    int preferenceLvl;
                    List<MATERIAL> materials = _postedAt.owner.productionPreferences[prodTypeToUse].prioritizedMaterials;
                    preferenceLvl = materials.IndexOf(currTile.materialOnTile);

                    //+20 x Preference for that Material based on the type (Food uses Training Preference)
                    weightForTile += 20 * (materials.Count - preferenceLvl);

                    if(weightForTile > 0) {
                        tileWeights.AddElement(currTile, weightForTile);
                    }
                }
                
            }
        }
        if(tileWeights.GetTotalOfWeights() <= 0) {
            throw new System.Exception("Could not find tile to expedite!");
        }
        return tileWeights.PickRandomElementGivenWeights();
    }
    #region overrides
    protected override void ConstructQuestLine() {
        base.ConstructQuestLine();

        _target = GetTargetHexTile();
        AddNewLog(_assignedParty.name + " goes to " + _target.locationName);
        GoToLocation goToTarget = new GoToLocation(this); //Go to the target tile
        goToTarget.InititalizeAction(_target);
        goToTarget.SetPathfindingMode(PATHFINDING_MODE.NORMAL_FACTION_RELATIONSHIP);
        goToTarget.onTaskDoAction += goToTarget.Generic;
        goToTarget.onTaskActionDone += StartResourceGathering;

        _questLine.Enqueue(goToTarget);
    }
    protected override void QuestSuccess() {
        _assignedParty.TransferMaterials(_postedAt, _target.materialOnTile, _gatheredAmt); //Transfer materials to the settlement that posted the quest
        base.QuestSuccess();
    }
    #endregion

    private void StartResourceGathering() {
        Messenger.AddListener("OnDayEnd", GatherResource);
    }

    private void GatherResource() {
        _gatheredAmt += Random.Range(0, 6); //The party produces 0 to 5 of the resource per day
        if(_gatheredAmt >= 200) {
            StopGathering(); //They will leave once they have gathered 200 resources.
        }
    }

    private void StopGathering() {
        Messenger.RemoveListener("OnDayEnd", GatherResource);
        _assignedParty.AdjustMaterial(_target.materialOnTile, _gatheredAmt); //take the gathered resources, and give it to the party
        AddNewLog(_assignedParty.name + " succcessfully gathers " + _gatheredAmt.ToString() + " " + Utilities.NormalizeString(_target.materialOnTile.ToString()) + " from " + _target.locationName);
        GoBackToQuestGiver(TASK_STATUS.SUCCESS);
    }
}
