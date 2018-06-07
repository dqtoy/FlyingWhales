using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum Sorting {
    None,
    Alphabetic,
}
public enum SortingOrder {
    Ascending,
    Descending,
}
public class ContentSorter : MonoBehaviour {
    public Sorting sorting;

    /// <summary>
    /// Sorts all children based on sort type
    /// </summary>
    /// <param name="order">Specify sort order, whether ascending or descending</param>
    public void Sort(SortingOrder order) {
        if(sorting == Sorting.None) {
            return;
        }

        List<Transform> children = new List<Transform>();
        for (int i = 0; i < this.transform.childCount; i++) {
            Transform child = this.transform.GetChild(i);
            children.Add(child);
        }
        
        switch (order) {
            case SortingOrder.Ascending:
            children.Sort(SortByName);
            break;
            case SortingOrder.Descending:
            children.Sort(SortByNameReverse);
            break;
        }
        for (int i = 0; i < children.Count; i++) {
            children[i].SetSiblingIndex(i);
        }
    }

    //Sorting functions
    static public int SortByName(Transform a, Transform b) { return string.Compare(a.name, b.name); }
    static public int SortByNameReverse(Transform a, Transform b) { return string.Compare(a.name, b.name) * -1; }
}
