﻿using System.Collections.Generic;

[System.Serializable]
public class LocalizationData{
	public List<LocalizationItem> items;
}

[System.Serializable]
public class LocalizationItem{
	public string key;
	public string value;
}