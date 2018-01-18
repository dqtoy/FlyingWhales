﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * This is the new weights system.
 * To create a new weighted dictionary, just create
 * a new instance of this class.
 * */
public class WeightedDictionary<T> {

    private Dictionary<T, int> _dictionary;

	#region getters/setters
	public int Count{
		get { return _dictionary.Count; }
	}
	#endregion

    public WeightedDictionary() {
        _dictionary = new Dictionary<T, int>();
    }

    public WeightedDictionary(Dictionary<T, int> dictionary) {
        _dictionary = dictionary;
    }

    /*
     * Add a new element of the given type.
     * If the dictionary already has an element with that key,
     * the specified weight will instead be added to that key.
     * */
    internal void AddElement(T newElement, int weight = 0) {
        if (!_dictionary.ContainsKey(newElement)) {
            _dictionary.Add(newElement, weight);
        } else {
            AddWeightToElement(newElement, weight);
        }
    }

    internal void AddElements(Dictionary<T, int> otherDictionary) {
        foreach (KeyValuePair<T, int> kvp in otherDictionary) {
            T key = kvp.Key;
            int value = kvp.Value;
            AddElement(key, value);
        }
    }

    /*
     * This will remove an element with a specific key
     * */
    internal void RemoveElement(T element) {
        if (_dictionary.ContainsKey(element)) {
            _dictionary.Remove(element);
        }
    }

    internal void AddWeightToElement(T key, int weight) {
        if (_dictionary.ContainsKey(key)) {
            _dictionary[key] += weight;
        }
    }

    internal void SubtractWeightFromElement(T key, int weight) {
        if (_dictionary.ContainsKey(key)) {
            _dictionary[key] -= weight;
        }
    }

    /*
     * This will get a random element in the weighted
     * dictionary.
     * */
    internal T PickRandomElementGivenWeights() {
        if(Utilities.GetTotalOfWeights(_dictionary) > 0) {
            return Utilities.PickRandomElementWithWeights(_dictionary);
        }
        throw new System.Exception("Cannot pick element because dictionary is empty!");
    }

    internal void LogDictionaryValues(string title) {
        Debug.Log(Utilities.GetWeightsSummary(_dictionary, title));
    }

    internal int GetTotalOfWeights() {
        return Utilities.GetTotalOfWeights(_dictionary);
    }
}
