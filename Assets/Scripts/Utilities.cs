using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Utilities : MonoBehaviour {

	public static int lastKingdomID = 0;
	public static int lastCitizenID = 0;
	public static int lastCityID = 0;

	/*
	 * Set unique id
	 * */
	public static int SetID<T>(T obj){
		if (obj is Kingdom) {
			lastKingdomID += 1;
			return lastKingdomID;
		} else if (obj is City) {
			lastCityID += 1;
			return lastCityID;
		} else if (obj is Citizen) {
			lastCitizenID += 1;
			return lastCitizenID;
		}
		return 0;
	}

	public static BASE_RESOURCE_TYPE GetBaseResourceType(RESOURCE resourceType){
		if (resourceType == RESOURCE.CORN || resourceType == RESOURCE.WHEAT || resourceType == RESOURCE.RICE ||
			resourceType == RESOURCE.DEER || resourceType == RESOURCE.PIG || resourceType == RESOURCE.BEHEMOTH) {
			return BASE_RESOURCE_TYPE.FOOD;
		} else if (resourceType == RESOURCE.CEDAR || resourceType == RESOURCE.OAK || resourceType == RESOURCE.EBONY) {
			return BASE_RESOURCE_TYPE.WOOD;
		} else if (resourceType == RESOURCE.GRANITE || resourceType == RESOURCE.SLATE || resourceType == RESOURCE.MARBLE) {
			return BASE_RESOURCE_TYPE.STONE;
		}
		return BASE_RESOURCE_TYPE.SPECIAL;
	}

	#region Pathfinding
	public static List<Point> EvenNeighbours {
		get {
			return new List<Point> {
				new Point(-1, 1),
				new Point(0, 1),
				new Point(1, 0),
				new Point(0, -1),
				new Point(-1, -1),
				new Point(-1, 0),

			};
		}
	}

	public static List<Point> OddNeighbours {
		get {
			return new List<Point> {
				new Point(0, 1),
				new Point(1, 1),
				new Point(1, 0),
				new Point(1, -1),
				new Point(0, -1),
				new Point(-1, 0),
			};
		}
	}
	#endregion
}
