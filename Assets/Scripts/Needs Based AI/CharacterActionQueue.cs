using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterActionQueue<T> {
    private readonly List<T> list = new List<T>();
    public ECS.Character character { get; private set; }

    public CharacterActionQueue(ECS.Character owner) {
        character = owner;
    }

    public void Enqueue(T value) {
        list.Add(value);
        if (value is ActionQueueItem) {
            Messenger.Broadcast(Signals.ACTION_ADDED_TO_QUEUE, value as ActionQueueItem, character);
        }
        Debug.Log("[" + GameManager.Instance.continuousDays + "] " + character.name + " added action to queue: \n" + value.ToString());
    }

    public void Enqueue(T value, int index) {
        list.Insert(index, value);
        if (value is ActionQueueItem) {
            Messenger.Broadcast(Signals.ACTION_ADDED_TO_QUEUE, value as ActionQueueItem, character);
        }
        Debug.Log("[" + GameManager.Instance.continuousDays + "] " + character.name + " inserted action to queue (" + index + "): \n" + value.ToString());
    }

    public T Dequeue() {
        if (IsEmpty) {
            return default(T);
        } else {
            T firstElement = list[0];
            list.RemoveAt(0);
            if (firstElement is ActionQueueItem) {
                Messenger.Broadcast(Signals.ACTION_REMOVED_FROM_QUEUE, firstElement as ActionQueueItem, character);
            }
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
        if (value is ActionQueueItem) {
            Messenger.Broadcast(Signals.ACTION_REMOVED_FROM_QUEUE, value as ActionQueueItem, character);
        }
        return list.Remove(value);
    }
    public void RemoveAt(int index) {
        T element = list[index];
        if (element is ActionQueueItem) {
            Messenger.Broadcast(Signals.ACTION_REMOVED_FROM_QUEUE, element as ActionQueueItem, character);
        }
        list.RemoveAt(index);
    }

    public bool IsEmpty {
        get { return list.Count <= 0; }
    }

    public void Clear() {
        list.Clear();
    }

    public int Count {
        get { return list.Count; }
    }

    public T GetBasedOnIndex(int index) {
        return list[index];
    }

}
