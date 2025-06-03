using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static bool CompareLayerAndMask(LayerMask mask, int layer)
    {
        return mask == (mask | (1 << layer));
    }
}
