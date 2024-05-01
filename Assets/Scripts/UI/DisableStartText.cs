using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Procedural
{
    public class DisableStartText : MonoBehaviour
    {
        [SerializeField] Canvas canvas;

        private void Start()
        {
            GenerateTerrain.OnWorldGenerated.AddListener(DisableCanvas);
        }

        private void DisableCanvas(Tilemap tilemap)
        {
            canvas.enabled = false;
        }
    }
}
