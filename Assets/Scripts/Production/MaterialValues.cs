using UnityEngine;
using System.Collections;

public class MaterialValues {
	public int excess{
		get { return count - capacity; }
	}
	public int totalCount{
		get{ return count + reserved; }
	}
	public int count;
	public int reserved;
	public int capacity;
	public int availableExcessOfOtherSettlements;
	public int availableExcessOfResourceLandmarks;
	public bool isNeeded;
	
	public MaterialValues() {
        maximumStorage = 300; // A Resource Landmark can only keep up to 300 of the Resource it produces.
    }
}
