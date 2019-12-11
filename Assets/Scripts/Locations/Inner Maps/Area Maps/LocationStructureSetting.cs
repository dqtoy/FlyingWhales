using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct LocationStructureSetting {
    public Point size;
    public bool hasTemplate;
    public StructureTemplate template;

    public LocationStructureSetting(StructureTemplate t) {
        size = t.size;
        hasTemplate = true;
        template = t;
    }

    public LocationStructureSetting(Point p) {
        size = p;
        hasTemplate = false;
        template = default(StructureTemplate);
    }
}
