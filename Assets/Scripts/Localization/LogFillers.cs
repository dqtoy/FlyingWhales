using UnityEngine;
using System.Collections;

public struct LogFiller {
	public object obj;
	public string value;

	public LogFiller(object obj, string value){
		this.obj = obj;
		this.value = value;
	}
}
