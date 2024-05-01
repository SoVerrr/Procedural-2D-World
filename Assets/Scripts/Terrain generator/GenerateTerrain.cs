using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Procedural
{
    public class GenerateTerrain : MonoBehaviour
    {
        [SerializeField] public Tilemap terrainTiles;
        [SerializeField] public List<BiomePreset> biomes = new List<BiomePreset>();
        [SerializeField] public int n; // Power to which 2 is raised to get (2^n)+1 for the size of the array for the diamond square

        [SerializeField] MapParameters heightMapParameters;
        [SerializeField] MapParameters heatMapParameters;
        [SerializeField] MapParameters moistureMapParameters;
        public static UnityEvent<Tilemap> OnWorldGenerated = new UnityEvent<Tilemap>(); //Params: heightmap, heatmap, moisturemap
        private PlayerInputActions playerInput;

        void Start()
        {
            playerInput = new PlayerInputActions();
            playerInput.Player.Enable();
            playerInput.Player.Generate.performed += GenerateWorldInput;
        }

        public void GenerateWorldInput(InputAction.CallbackContext context)
        {
            CreateWorld();
        }

/*        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                CreateWorld();
            }    
        }*/

        private void CreateWorld()
        {
            terrainTiles.ClearAllTiles();
            //Creating heightmaps needed
            float[,] heightmap = DiamondSquareMap.DiamondSquare(n, heightMapParameters.randomRange, heightMapParameters.rougness);
            float[,] heatmap = DiamondSquareMap.DiamondSquare(n, heatMapParameters.randomRange, heatMapParameters.rougness);
            float[,] moisturemap = DiamondSquareMap.DiamondSquare(n, moistureMapParameters.randomRange, moistureMapParameters.rougness);
            int size = (int)Mathf.Pow(2, n) + 1;
            //Normalizing array values
            NormalizeFloats(heightmap, size);
            NormalizeFloats(heatmap, size);
            NormalizeFloats(moisturemap, size);

            JobHandle job;

            //Populating native arrays for the job
            NativeArray<float> heightNative = ArrayToNative(heightmap, size);
            NativeArray<float> heatNative = ArrayToNative(heatmap, size);
            NativeArray<float> moistNative = ArrayToNative(moisturemap, size);
            NativeArray<float> minHeights = new NativeArray<float>(biomes.Count, Allocator.TempJob);
            NativeArray<float> minHeats = new NativeArray<float>(biomes.Count, Allocator.TempJob);
            NativeArray<float> minMoists = new NativeArray<float>(biomes.Count, Allocator.TempJob);
            NativeArray<int> biomeIndexes = new NativeArray<int>(size * size, Allocator.TempJob);

            PopulateBiomesArray(ref minHeights, ref minHeats, ref minMoists);

            PopulateTileMap populate = new PopulateTileMap
            {
                heightMap = heightNative,
                heatMap = heatNative,
                moisturetMap = moistNative,
                minHeats = minHeats,
                minHeights = minHeights,
                minMoistures = minMoists,
                biomeIndexes = biomeIndexes,
                columns = size
            };
            job = populate.Schedule(size * size, 6400);
            job.Complete();

            //Destroying the native arrays to avoid leaks
            heightNative.Dispose();
            heatNative.Dispose();
            moistNative.Dispose();
            minHeights.Dispose();
            minHeats.Dispose();
            minMoists.Dispose();

            for (int i = 0; i < biomeIndexes.Length; i++) //Setting the tiles in the tilemap
            {
                terrainTiles.SetTile(new Vector3Int(i % size, i / size, 0), biomes[biomeIndexes[i]].GetRandomTile());
            }

            biomeIndexes.Dispose();
            OnWorldGenerated.Invoke(terrainTiles);
        }

        private void PopulateBiomesArray(ref NativeArray<float> minHeights, ref NativeArray<float> minHeats, ref NativeArray<float> minMoists)
        {
            //Setting values of biome values in native arrays
            for (int i = 0; i < biomes.Count; i++)
            {
                minHeights[i] = biomes[i].minHeight;
                minHeats[i] = biomes[i].minHeat;
                minMoists[i] = biomes[i].minMoisture;
            }
        }

        private NativeArray<float> ArrayToNative(float[,] arr, int size) //Returns native array created from a 2d float array
        {
            NativeArray<float> result = new NativeArray<float>(size * size, Allocator.Persistent);
            for (int j = 0; j < size; j++)
            {
                for (int i = 0; i < size; i++)
                {
                    result[j * size + i] = arr[i, j];

                }
            }
            return result;
        }
        private void NormalizeFloats(float[,] arr, int size) //Normalizes float values
        {
            float min = arr.Cast<float>().Min();
            float max = arr.Cast<float>().Max();
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    arr[y, x] = (arr[y, x] - min) / (max - min);
                }
            }
        }

        [BurstCompatible]
        struct PopulateTileMap : IJobParallelFor //Job used to return the indexes of biomes for each tile
        {
            [ReadOnly] public NativeArray<float> heightMap;
            [ReadOnly] public NativeArray<float> heatMap;
            [ReadOnly] public NativeArray<float> moisturetMap;
            [ReadOnly] public NativeArray<float> minHeights;
            [ReadOnly] public NativeArray<float> minHeats;
            [ReadOnly] public NativeArray<float> minMoistures;
            [ReadOnly] public int columns; //Size of 2d array to convert 1d index into 2d
            [WriteOnly] public NativeArray<int> biomeIndexes;

            public bool CheckCondition(float height, float moisture, float temp, int index)
            {
                return (height >= minHeights[index]) && (moisture >= minMoistures[index]) && (temp >= minHeats[index]);
            }

            private float GetDiffValue(float height, float moisture, float heat, int biomeIndex) //Calculate the difference value between maps values and biome min values
            {
                return (height - minHeights[biomeIndex]) + (moisture - minMoistures[biomeIndex]) + (heat - minHeats[biomeIndex]);
            }

            private float[] GetDiffs(int i) //Get the difference value of each biome to determine which is best suited for the tile
            {
                float[] diffs = new float[minHeights.Length];
                for (int j = 0; j < minHeights.Length; j++)
                {
                    if (CheckCondition(heightMap[i], moisturetMap[i], heatMap[i], j))
                    {
                        diffs[j] = GetDiffValue(heightMap[i], moisturetMap[i], heatMap[i], j);
                    }
                    else
                    {
                        diffs[j] = 999; //Set diffs value for the biome to 999 to avoid comparing with default float value
                    }
                }
                return diffs;
            }
            private int FindBiomeIndex(float[] diffs) //Returns the index of biome with lowest value difference
            {
                int minDiffIndex = 0;
                float minDiffValue = diffs[0];
                for (int j = 0; j < minHeights.Length; j++) //Determine lowest value difference
                {
                    if (diffs[j] < minDiffValue)
                    {
                        minDiffIndex = j;
                        minDiffValue = diffs[j];
                    }
                }
                return minDiffIndex;
            }
            public void Execute(int i)
            {
                float[] diffs = GetDiffs(i);
                biomeIndexes[i] = FindBiomeIndex(diffs); //Set biome index to biome index with lowest diff value
            }
        }
    }

}