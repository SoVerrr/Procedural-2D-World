using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Procedural
{
    [CreateAssetMenu(fileName = "BiomePreset", menuName = "Biome Preset")]
    public class BiomePreset : ScriptableObject
    {
        public Tile[] tiles;
        public List<PlantPreset> biomeFoliage;
        public float minHeight;
        public float minMoisture;
        public float minHeat;

        public Tile GetRandomTile()
        {
            return tiles[Random.Range(0, tiles.Length)];
        }
        public bool CheckCondition(float height, float moisture, float temp)
        {
            return (height >= minHeight) && (moisture >= minMoisture) && (temp >= minHeat);
        }
        public bool CheckIfInBiome(Tile tile) //Function that returns if a given tile is a part of the biome
        {
            bool flag = false;
            foreach(Tile p in tiles)
            {
                if (p == tile)
                    flag = true;
            }
            return flag;
        }
    }


}
