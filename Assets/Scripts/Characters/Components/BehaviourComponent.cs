using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourComponent {

	public Character owner { get; private set; }
    public List<CharacterBehaviourComponent> currentBehaviourComponents { get; private set; }

    public BehaviourComponent (Character owner) {
        this.owner = owner;
        currentBehaviourComponents = new List<CharacterBehaviourComponent>();
    }

    #region General
    public void PopulateInitialBehaviourComponents() {
        System.Type[] classBehaviourComponents = CharacterManager.Instance.GetClassBehaviourComponents(owner.characterClass.className);
        for (int i = 0; i < classBehaviourComponents.Length; i++) {
            CharacterBehaviourComponent behaviourComponent = CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]);
            AddBehaviourComponent(behaviourComponent);
        }
    }
    public void OnChangeClass(CharacterClass newClass, CharacterClass oldClass) {
        if(oldClass == newClass) {
            return;
        }
        if(oldClass != null && newClass != null) {
            string oldClassBehaviourComponentKey = CharacterManager.Instance.GetClassBehaviourComponentKey(oldClass.className);
            string newClassBehaviourComponentKey = CharacterManager.Instance.GetClassBehaviourComponentKey(newClass.className);
            if (oldClassBehaviourComponentKey == newClassBehaviourComponentKey) {
                return;
            }
        }
        if (oldClass != null) {
            System.Type[] classBehaviourComponents = CharacterManager.Instance.GetClassBehaviourComponents(oldClass.className);
            for (int i = 0; i < classBehaviourComponents.Length; i++) {
                RemoveBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]));
            }
        }
        if(newClass != null) {
            System.Type[] classBehaviourComponents = CharacterManager.Instance.GetClassBehaviourComponents(newClass.className);
            for (int i = 0; i < classBehaviourComponents.Length; i++) {
                AddBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(classBehaviourComponents[i]));
            }
        }
    }
    public bool AddBehaviourComponent(CharacterBehaviourComponent component) {
        if(component == null) {
            throw new System.Exception(GameManager.Instance.TodayLogString() + owner.name + " is trying to add a new behaviour component but it is null!");
        }
        return AddBehaviourComponentInOrder(component);
    }
    public bool AddBehaviourComponent(System.Type componentType) {
        return AddBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(componentType));
    }
    public bool RemoveBehaviourComponent(CharacterBehaviourComponent component) {
        return currentBehaviourComponents.Remove(component);
    }
    public bool RemoveBehaviourComponent(System.Type componentType) {
        return RemoveBehaviourComponent(CharacterManager.Instance.GetCharacterBehaviourComponent(componentType));
    }
    public bool ReplaceBehaviourComponent(CharacterBehaviourComponent componentToBeReplaced, CharacterBehaviourComponent componentToReplace) {
        if (RemoveBehaviourComponent(componentToBeReplaced)) {
            return AddBehaviourComponent(componentToReplace);
        }
        return false;
    }
    public bool ReplaceBehaviourComponent(System.Type componentToBeReplaced, System.Type componentToReplace) {
        if (RemoveBehaviourComponent(componentToBeReplaced)) {
            return AddBehaviourComponent(componentToReplace);
        }
        return false;
    }
    public bool ReplaceBehaviourComponent(List<CharacterBehaviourComponent> newComponents) {
        currentBehaviourComponents.Clear();
        for (int i = 0; i < newComponents.Count; i++) {
            AddBehaviourComponent(newComponents[i]);
        }
        return true;
    }
    private bool AddBehaviourComponentInOrder(CharacterBehaviourComponent component) {
        if (currentBehaviourComponents.Count > 0) {
            for (int i = 0; i < currentBehaviourComponents.Count; i++) {
                if (component.priority < currentBehaviourComponents[i].priority) {
                    currentBehaviourComponents.Insert(i, component);
                    return true;
                }
            }
        }
        currentBehaviourComponents.Add(component);
        return true;
    }
    #endregion

    #region Processes
    public string RunBehaviour() {
        string log = $"{GameManager.Instance.TodayLogString()}{owner.name} Idle Plan Decision Making:";
        for (int i = 0; i < currentBehaviourComponents.Count; i++) {
            CharacterBehaviourComponent component = currentBehaviourComponents[i];
            if (component.IsDisabledFor(owner)) {
                log += $"\nBehaviour Component: {component.ToString()} is disabled for {owner.name} skipping it...";
                continue; //skip component
            }
            if (!component.CanDoBehaviour(owner)) {
                log += $"\nBehaviour Component: {component.ToString()} cannot be done by {owner.name} skipping it...";
                continue; //skip component
            }
            if (component.TryDoBehaviour(owner, ref log)) {
                component.PostProcessAfterSucessfulDoBehaviour(owner);
                if (!component.WillContinueProcess()) { break; }
            }
        }
        return log;
    }
    #endregion
}
