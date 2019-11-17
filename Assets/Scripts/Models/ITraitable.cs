using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    /// <summary>
    /// Interface for objects that can have traits.
    /// </summary>
    public interface ITraitable {
        string name { get; }
        ITraitContainer traitContainer { get; } 
        TraitProcessor traitProcessor { get; }
    }
}

