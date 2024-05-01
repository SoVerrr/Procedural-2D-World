using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine.Events;

namespace Procedural
{
    public class RiverGenerator : MonoBehaviour
    {
        [SerializeField] int amountOfRivers;
        [SerializeField] BiomePreset mountainBiome;
        [SerializeField] BiomePreset oceanBiome;
        [SerializeField] Tile riverTile;
        [SerializeField] int numberOfCurves;
        [SerializeField] List<Tile> nonWalkable;
        [SerializeField] int riverCurvatureRange;

        private Tilemap terrainTiles;
        private Vector2Int riverEnd;
        public static UnityEvent OnRiversGenerated = new UnityEvent();
        public static bool WereRiversGenerated = false;

        void Start()
        {
            GenerateTerrain.OnWorldGenerated.AddListener(InstantiateRiverGeneration);
        }


        private void InstantiateRiverGeneration(float[,] heightmap, float[,] heatmap, float[,] moistmap, Tilemap terrain) //Assign heightmaps and terrain tilemap to variables within the script
        {
            terrainTiles = terrain;
            GenerateRivers();
            OnRiversGenerated.Invoke();
        }

        private void GenerateRivers()
        {
            for(int i = 0; i < amountOfRivers; i++)
            {
                Vector2Int riverBeginning = FindBiome(mountainBiome);
                riverEnd = FindBiome(oceanBiome);

                GenerateRiver(riverBeginning, riverEnd, numberOfCurves);
                RiverGenerator.WereRiversGenerated = true;
            }
        }

        public void GenerateRiver(Vector2Int riverBeginning, Vector2Int riverEnd, int numberOfCurves)
        {
            List<Vector3Int> riverPath = RiverPath.AStarPathfind(new Vector3Int(riverBeginning.x, riverBeginning.y, 0), new Vector3Int(riverEnd.x, riverEnd.y, 0));

            List<Vector3Int> riverSamples = new List<Vector3Int>();

            numberOfCurves *= 4;

            for (int j = 0; j < numberOfCurves - 1; j++)
            {
                riverSamples.Add(riverPath[Random.Range((j * riverPath.Count / numberOfCurves), (j + 1) * riverPath.Count / numberOfCurves)]);
                if (j % 2 == 0)
                {
                    riverSamples[j] = new Vector3Int(riverSamples[j].x + Random.Range(-riverCurvatureRange, riverCurvatureRange), riverSamples[j].y, 0);
                }
                else
                {
                    riverSamples[j] = new Vector3Int(riverSamples[j].x + Random.Range(-riverCurvatureRange, riverCurvatureRange), riverSamples[j].y, 0);
                }
            }
            riverSamples.Add(new Vector3Int(riverEnd.x, riverEnd.y, 0));
            for(int j = 0; j < numberOfCurves - 3; j += 3)
            {
                Vector3 p0 = riverSamples[j];
                Vector3 p1 = riverSamples[j + 1];
                Vector3 p2 = riverSamples[j + 2];
                Vector3 p3 = riverSamples[j + 3];
                for(float t = 0; t < 1; t += 0.0005f)
                {
                    Vector3Int bezierTile = terrainTiles.WorldToCell(BezierCurve.GetPointOnBezierCurve(p0, p1, p2, p3, t));

                    if (nonWalkable.Contains(terrainTiles.GetTile<Tile>(bezierTile))) //Stop the river generation if tile is in ocean
                        return;

                    terrainTiles.SetTile(bezierTile, riverTile);

                }
            }
        }

        public Vector3Int FindDirection(Vector2Int target, Vector2Int current)
        {
            return new Vector3Int((target.x - current.x), (target.y - current.y), 0);
        }

        private Vector2Int FindBiome(BiomePreset biome, int i = 0)
        {
            int x = Random.Range(0, terrainTiles.size.x);
            int y = Random.Range(0, terrainTiles.size.y);
            i++;
            if (i > 100000) //return 0,0 after 100k iterations to avoid bootloops
                return new Vector2Int(0, 0);
            if (terrainTiles.GetTile(new Vector3Int(x, y, 0)) == biome.GetRandomTile())
            {
                return new Vector2Int(x, y);
            }

            return FindBiome(biome, i);
        }
    }
}
