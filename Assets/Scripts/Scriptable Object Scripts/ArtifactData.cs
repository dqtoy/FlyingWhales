using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Artifact Data", menuName = "Artifact Data")]
public class ArtifactData : ScriptableObject {
    [SerializeField] private ARTIFACT_TYPE _type;
    [SerializeField] private Sprite _sprite;
    
    #region getters
    public ARTIFACT_TYPE type => _type;
    public Sprite sprite => _sprite;
    #endregion
}
