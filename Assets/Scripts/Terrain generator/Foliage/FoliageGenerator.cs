using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Procedural {
    public class FoliageGenerator : MonoBehaviour
    {
        [SerializeField] GenerateTerrain terrainGenerator;
        [SerializeField] Tilemap foliageMap; //Secondary tilemap on which the foliage is places, displayed above the terrain tilemap
        private int n;
        private List<BiomePreset> biomes;
        private Tilemap terrainMap;

        private void Start()
        {
            n = terrainGenerator.n;
            biomes = terrainGenerator.biomes;
            terrainMap = terrainGenerator.terrainTiles;
            if (RiverGenerator.WereRiversGenerated) //Check if rivers were already generated to avoid not generating foliage on first generation
                GenerateFoliage();

            RiverGenerator.OnRiversGenerated.AddListener(GenerateFoliage);
        }


        private void GenerateFoliage()
        {
            foliageMap.ClearAllTiles();
            for(int x = 0; x < (int)Mathf.Pow(2, n) + 1; x++) //Iterate through the n^2+1 on each axis for the tiles
            {
                for(int y = 0; y < (int)Mathf.Pow(2, n) + 1; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TryPlacePlant(terrainMap.GetTile<Tile>(pos), pos); 
                }
            }
        }

        private void TryPlacePlant(Tile tile, Vector3Int pos)
        {
            foreach(BiomePreset preset in biomes) 
            {
                if (preset.CheckIfInBiome(tile))
                {
                    foreach (PlantPreset plant in preset.biomeFoliage) 
                    {
                        if (Random.Range(0f, 1f) < plant.appearanceRate) //Check if random below spawn rate of the plant
                        {
                            plant.PlacePlant(foliageMap, pos, preset, terrainMap);
                        }
                    }
                }
            }
        }
    }
}
