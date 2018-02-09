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
    public int maximumStorage; //This is the maximum amount of materials that can be stored

    public MaterialValues() {
        maximumStorage = 300; // A Resource Landmark can only keep up to 300 of the Resource it produces.
    }
}
