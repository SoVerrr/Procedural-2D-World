using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    [System.Serializable]
    public struct MapParameters //Parameters used to create various heightmaps
    {
        public float randomRange; //Range of the random values at the start of the algorithm
        public float rougness; //The rate at which random range gets lower, higher roughness means smoother terrain
    }
}
