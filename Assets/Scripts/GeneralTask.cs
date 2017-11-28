using UnityEngine;
using System.Collections;

public class GeneralTask {
	public GENERAL_TASKS task;
	public City targetCity;

	public GeneralTask(GENERAL_TASKS task, City targetCity){
		this.task = task;
		this.targetCity = targetCity;
	}
}
