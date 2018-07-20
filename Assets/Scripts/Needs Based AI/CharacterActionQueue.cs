using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterActionQueue<T> {
    private readonly List<T> list = new List<T>();

    public void Enqueue(T value) {
        list.Add(value);
    }

    public void Enqueue(T value, int index) {
        list.Insert(index, value);
    }

    public T Dequeue() {
        if (IsEmpty) {
            return default(T);
        } else {
            T firstElement = list[0];
            list.RemoveAt(0);
            return firstElement;
        }
    }

    public T Peek() {
        if (IsEmpty) {
            return default(T);
        } else {
            return list[0];
        }
    }

    public bool Remove(T value) {
        return list.Remove(value);
    }

    public bool IsEmpty {
        get { return list.Count <= 0; }
    }

    public void Clear() {
        list.Clear();
    }

}
