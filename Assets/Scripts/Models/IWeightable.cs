using UnityEngine;
using System.Collections;

public interface IWeightable<T> {

    WeightedDictionary<T> ToWeightedDictionary();
}
