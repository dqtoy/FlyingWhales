using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OpinionComponent {
    public Character owner { get; private set; }
    public Dictionary<Character, Dictionary<string, int>> opinions { get; private set; }

    public OpinionComponent(Character owner) {
        this.owner = owner;
        opinions = new Dictionary<Character, Dictionary<string, int>>();
    }

    public void AdjustOpinion(Character target, string opinionText, int opinionValue) {
        if (!HasOpinion(target)) {
            opinions.Add(target, new Dictionary<string, int>() { { "Base", 0 } });
            Messenger.Broadcast(Signals.OPINION_ADDED, owner, target);
        }
        if (opinions[target].ContainsKey(opinionText)) {
            opinions[target][opinionText] += opinionValue;
        } else {
            opinions[target].Add(opinionText, opinionValue);
        }
        if (opinionValue > 0) {
            Messenger.Broadcast(Signals.OPINION_INCREASED, owner, target);
        } else if (opinionValue < 0) {
            Messenger.Broadcast(Signals.OPINION_DECREASED, owner, target);
        }
        if (!target.opinionComponent.HasOpinion(owner)) {
            target.opinionComponent.AdjustOpinion(owner, "Base", 0);
        }
    }

    public void RemoveOpinion(Character target, string opinionText) {
        if (HasOpinion(target)) {
            if (opinions[target].ContainsKey(opinionText)) {
                opinions[target].Remove(opinionText);
            }
        }
    }
    public void RemoveOpinion(Character target) {
        if (HasOpinion(target)) {
            opinions.Remove(target);
            Messenger.Broadcast(Signals.OPINION_REMOVED, owner, target);
        }
    }
    public bool HasOpinion(Character target) {
        return opinions.ContainsKey(target);
    }
    public bool HasOpinion(Character target, string opinionText) {
        return opinions.ContainsKey(target) && opinions[target].ContainsKey(opinionText);
    }
    public int GetTotalOpinion(Character target) {
        return opinions[target].Sum(x => x.Value);
    }
    public int GetTotalPositiveOpinionWith(Character character) {
        if (HasOpinion(character)) {
            int total = 0;
            Dictionary<string, int> _opinions = GetOpinion(character);
            foreach (KeyValuePair<string, int> pair in _opinions) {
                if (pair.Value > 0) {
                    total += pair.Value;
                }
            }
            return total;
        }
        return 0;
    }
    public int GetTotalNegativeOpinionWith(Character character) {
        if (HasOpinion(character)) {
            int total = 0;
            Dictionary<string, int> _opinions = GetOpinion(character);
            foreach (KeyValuePair<string, int> pair in _opinions) {
                if (pair.Value < 0) {
                    total += pair.Value;
                }
            }
            return total;
        }
        return 0;
    }
    public Dictionary<string, int> GetOpinion(Character target) {
        return opinions[target];
    }
}