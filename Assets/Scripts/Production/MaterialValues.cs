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
	public int availableExcessOfOthers;
}
