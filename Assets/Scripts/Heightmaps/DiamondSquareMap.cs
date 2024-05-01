using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Procedural
{
    public class DiamondSquareMap : MonoBehaviour
    {
        public static float[,] DiamondSquare(int nPower, float randomRange, float roughness) //Generate a heightmap using diamond square algorithm
        {
            int mapSize = (int)Mathf.Pow(2, nPower) + 1;
            float[,] result = new float[mapSize, mapSize];
            int chunkSize = mapSize - 1;

            //Assigning random values to 4 corners of the map
            #region corners 
            result[0, 0] = Random.Range(-randomRange, randomRange);
            result[0, chunkSize] = Random.Range(-randomRange, randomRange);
            result[chunkSize, 0] = Random.Range(-randomRange, randomRange);
            result[chunkSize, chunkSize] = Random.Range(-randomRange, randomRange);
            #endregion

            while(chunkSize > 1)
            {
                int half = chunkSize / 2;
                SquareStep(chunkSize, half, mapSize, randomRange, result);
                DiamondStep(chunkSize, half, mapSize, randomRange, result);
                chunkSize /= 2;
                randomRange -= randomRange * 0.5f * roughness;
            }

            return result;
        }
        private static void SquareStep(int chunkSize, int halfSize, int mapSize, float randomRange, float[,] heightMap) //Generate middle point value using average of corners of current chunk + random value
        {
            for(int y = 0; y < mapSize - 1; y += chunkSize) //Iterate through corners of all chunks
            {
                for(int x = 0; x < mapSize - 1; x += chunkSize)
                {
                    heightMap[y + halfSize, x + halfSize] = //Setting the value of a point in the middle to average of corners + random
                        (heightMap[y, x] +
                        heightMap[y, x + chunkSize] +
                        heightMap[y + chunkSize, x] +
                        heightMap[y + chunkSize, x + chunkSize]) / 4 +
                        (Random.Range(-randomRange, randomRange) * (randomRange * 2.0f)) - randomRange;
                }
            }
        }
        private static void DiamondStep(int chunkSize, int halfSize, int mapSize, float randomRange, float[,] heightMap)
        {
            for(int y = 0; y < mapSize; y += halfSize)
            {
                for (int x = (y + halfSize) % chunkSize; x < mapSize; x += chunkSize)
                {
                    float average = 0f; //Getting the average of the diamond part, different method than squarestep to avoid places with only 3 neighbors
                    int count = 0;
                    if (y - halfSize > 0)
                    {
                        average += heightMap[y - halfSize, x];
                        count++;
                    }
                    if (x - halfSize > 0)
                    {
                        average += heightMap[y, x - halfSize];
                        count++;
                    }
                    if (x + halfSize < mapSize)
                    {
                        average += heightMap[y, x + halfSize];
                        count++;
                    }
                    if (y + halfSize < mapSize)
                    {
                        average += heightMap[y + halfSize, x];
                        count++;
                    }
                    average /= count;
                    average += (Random.Range(-randomRange, randomRange) * (randomRange * 2.0f)) - randomRange;
                    heightMap[y, x] = average;

                }
            }
        }
    }
}
