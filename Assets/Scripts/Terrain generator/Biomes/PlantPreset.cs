using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
namespace Procedural
{
    [CreateAssetMenu(fileName = "PlanetPreset", menuName = "Plant Preset")]
    public class PlantPreset : ScriptableObject
    {
        [Header("Tiles arranged in order from bottom left to upper right")]
        public List<Tile> plantTiles;
        public int xSize, ySize;
        [Range(0f, 1f)] public float appearanceRate; //Percentage chance that the plant will appear


        public void PlacePlant(Tilemap foliageMap, Vector3Int startPosition, BiomePreset biome, Tilemap terrainMap)
        {
            if (foliageMap.GetTile<Tile>(startPosition) != null) //Dont place the plant if space is already occupied
                return;


            for (int y = 0; y < ySize; y++) //Check if any of the spaces in plant's radius are occupied or outside of the biome
            {
                for (int x = 0; x < xSize; x++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    if (foliageMap.GetTile<Tile>(startPosition + tilePos) != null || !biome.CheckIfInBiome(terrainMap.GetTile<Tile>(startPosition + tilePos)))
                        return;
                }
            }

            for (int y = 0; y < ySize; y++) //Place the plant if all previous condtitions are met
            {
                for (int x = 0; x < xSize; x++)
                {

                        Vector3Int tilePos = new Vector3Int(x, y, 0);
                        foliageMap.SetTile(startPosition + tilePos, plantTiles[(y * xSize) + x]);
                }
            }
        }
    }
}