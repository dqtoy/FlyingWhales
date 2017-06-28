using UnityEngine;
using System.Collections;

public struct LogFiller {
	public object obj;
	public string value;
	public LOG_IDENTIFIER identifier;

	public LogFiller(object obj, string value, LOG_IDENTIFIER identifier){
		this.obj = obj;
		this.value = value;
		this.identifier = identifier;
	}
}
