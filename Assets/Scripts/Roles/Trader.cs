using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Trader : Role {

	public City homeCity;
	public City targetCity;
	public HexTile currentLocation;
	public bool isWorking = false;
	public List<BASE_RESOURCE_TYPE> offeredResources;
	public TradeManager tradeManager;

	private GameObject traderGameObject;
	private List<HexTile> pathToTargetCity;
	private List<BASE_RESOURCE_TYPE> currentlySelling;
	private int goldIncomePerTurn = 0;
	private int currentPathIndex = 0;

	private bool isGoingHome = false;

	public Trader(Citizen citizen, TradeManager tradeManager): base(citizen){
		this.homeCity = citizen.city;
		this.targetCity = null;
		this.currentLocation = this.citizen.city.hexTile;
		this.offeredResources = new List<BASE_RESOURCE_TYPE>();
		this.tradeManager = tradeManager;
		EventManager.Instance.onWeekEnd.AddListener(AssignTask);
	}

	internal void AssignTask(){
		this.offeredResources.Clear();
		this.offeredResources = tradeManager.DetermineOfferedResources();
		this.targetCity = tradeManager.GetTargetCity();
		if (this.targetCity == null) {
			return;
		}
		if (this.currentLocation == null) {
			return;
		}

		this.pathToTargetCity = PathGenerator.Instance.GetPath(currentLocation, targetCity.hexTile, PATHFINDING_MODE.NORMAL);
		this.pathToTargetCity.Insert (0, this.currentLocation);

		this.currentlySelling = this.offeredResources.Intersect(this.targetCity.tradeManager.neededResources).ToList();
		this.goldIncomePerTurn = this.pathToTargetCity.Count * 5;


			
		if (this.currentlySelling.Contains (BASE_RESOURCE_TYPE.FOOD)) {
			this.targetCity.tradeManager.sustainabilityBuff = 5;
		}

		if (this.homeCity.kingdom.id != this.targetCity.kingdom.id) {
			RelationshipKings rel1 = this.homeCity.kingdom.king.GetRelationshipWithCitizen(this.targetCity.kingdom.king);
			RelationshipKings rel2 = this.targetCity.kingdom.king.GetRelationshipWithCitizen(this.homeCity.kingdom.king);
//			rel1.AdjustLikeness(2, EVENT_TYPES.TRADE);
//			rel2.AdjustLikeness(2, EVENT_TYPES.TRADE);
			rel1.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.week,
				GameManager.Instance.year,
				"Successful trade with city " + this.targetCity.name,
				HISTORY_IDENTIFIER.KING_RELATIONS,
				true
			));

			rel2.relationshipHistory.Add (new History (
				GameManager.Instance.month,
				GameManager.Instance.week,
				GameManager.Instance.year,
				"Successful trade with city " + this.homeCity.name,
				HISTORY_IDENTIFIER.KING_RELATIONS,
				true
			));
		}

		this.isWorking = true;
		this.isGoingHome = false;
		if (this.traderGameObject == null) {
			this.traderGameObject = GameObject.Instantiate (Resources.Load ("GameObjects/CitizenAvatar"), this.homeCity.hexTile.transform) as GameObject;
			this.traderGameObject.transform.localScale = new Vector3 (5f, 5f, 5f);
			this.traderGameObject.GetComponent<CitizenAvatar>().AssignCitizen(this.citizen);
		}
		EventManager.Instance.onWeekEnd.RemoveListener(AssignTask);
		EventManager.Instance.onWeekEnd.AddListener(DailyActions);

	}

	internal void DailyActions(){
		//move trader
		if (isGoingHome) {
			this.GoBackHome();
		} else {
			this.GoToTargetCity();
		}
		//add resources
		this.homeCity.AdjustResourceCount(BASE_RESOURCE_TYPE.GOLD, this.goldIncomePerTurn);
		for (int i = 0; i < this.currentlySelling.Count; i++) {
			switch (this.currentlySelling [i]) {
			case BASE_RESOURCE_TYPE.WOOD:
				this.targetCity.AdjustResourceCount (BASE_RESOURCE_TYPE.WOOD, 6);
				break;
			case BASE_RESOURCE_TYPE.STONE:
				this.targetCity.AdjustResourceCount (BASE_RESOURCE_TYPE.STONE, 6);
				break;
			case BASE_RESOURCE_TYPE.COBALT:
				this.targetCity.AdjustResourceCount (BASE_RESOURCE_TYPE.COBALT, 3);
				break;
			case BASE_RESOURCE_TYPE.MANA_STONE:
				this.targetCity.AdjustResourceCount (BASE_RESOURCE_TYPE.MANA_STONE, 3);
				break;
			case BASE_RESOURCE_TYPE.MITHRIL:
				this.targetCity.AdjustResourceCount (BASE_RESOURCE_TYPE.MITHRIL, 3);
				break;
			}
		}
	}

	internal void GoToTargetCity(){
//		int increments = 1;
//		for (int i = 0; i < increments; i++) {
			this.currentPathIndex += 1;
			HexTile nextTile = this.pathToTargetCity [this.currentPathIndex];
			this.traderGameObject.GetComponent<CitizenAvatar>().MakeCitizenMove(this.currentLocation, nextTile);
			this.currentLocation = nextTile;
//			if (this.currentLocation == this.targetCity.hexTile) {
//				break;
//			}
//		}
		if (this.currentLocation == this.targetCity.hexTile) {
			this.isGoingHome = true;
		}
	}

	internal void GoBackHome(){
//		int increments = 1;
//		for (int i = 0; i < increments; i++) {
			this.currentPathIndex -= 1;
			HexTile nextTile = this.pathToTargetCity[this.currentPathIndex];
			this.traderGameObject.GetComponent<CitizenAvatar>().MakeCitizenMove(this.currentLocation, nextTile);
			this.currentLocation = nextTile;
//			if (this.currentLocation == this.homeCity.hexTile) {
//				break;
//			}
//		}
		if (this.currentLocation == this.homeCity.hexTile) {
			this.isGoingHome = false;
			this.isWorking = false;
			targetCity.tradeManager.sustainabilityBuff = 0;
			this.targetCity = null;
			EventManager.Instance.onWeekEnd.RemoveListener(DailyActions);
			EventManager.Instance.onWeekEnd.AddListener(AssignTask);
			this.AssignTask();

		}
	}

	internal override int[] GetResourceProduction(){
		return new int[]{ 0, 0, 0, 0, 0, 0, 40 };
	}

	internal override void OnDeath(){
		if (this.traderGameObject != null) {
			GameObject.Destroy (this.traderGameObject);
		}
		EventManager.Instance.onWeekEnd.RemoveListener(AssignTask);
		EventManager.Instance.onWeekEnd.RemoveListener(DailyActions);
	}
}
