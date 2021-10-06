using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGenerator : MonoBehaviour
{
    public GameObject blockPrototype;

    public enum BlockColor
    {
        Red,
        Green,
        Blue,
        Yellow,
        Purple,
    }

    public Texture[] blockTextures = new Texture[System.Enum.GetValues(typeof(BlockColor)).Length];

    static Material[] blockMaterials;

    static Vector3 startPosition;

    public static int NumBlockColors { get { return System.Enum.GetValues(typeof(BlockColor)).Length; } }

    public int Generate(int rows, int columns, out Blocks outBlocks)
    {
        GenerateMaterials();

        outBlocks = new Blocks();
        outBlocks.Initialize(rows, columns);

        Transform parent = transform;

        Vector3 blockSize = blockPrototype.GetComponent<MeshFilter>().sharedMesh.bounds.size;

        startPosition = new Vector3
        (
            -(columns * 0.5f * blockSize.x) + (blockSize.x * 0.5f), 
            -(rows * 0.5f * blockSize.y) + (blockSize.y * 0.5f), 
            0.0f
        );

        Vector3 position;
        Quaternion rotation = Quaternion.identity;

        const float sameColorPercentage = 0.0f; // 20.0f;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                position = startPosition + new Vector3(c, r, 0.0f);

                GameObject obj = Instantiate(blockPrototype, position, rotation, parent);
                obj.name = GetBlockNameByRowColumn(r, c);
                outBlocks[r, c] = obj.GetComponent<Block>();

                int randomColorIndex = Random.Range(0, blockTextures.Length);

                if (c > 0 && r > 0)
                {
                    if (Random.Range(0, 100) <= sameColorPercentage)
                    {
                        if (Random.Range(0, 2) == 0)
                        {
                            randomColorIndex = (int)outBlocks[r, c - 1].BlockColor;
                        }
                        else
                        {
                            randomColorIndex = (int)outBlocks[r - 1, c].BlockColor;
                        }
                    }
                }

                SetupBlockByColor(outBlocks[r, c], (BlockColor)randomColorIndex);

                outBlocks[r, c].Row = r;
                outBlocks[r, c].Column = c;
            }
        }

        return outBlocks.NumBlocks;
    }

    public static Vector3 CalculatePositionByRowColumn(int row, int column)
    {
        return startPosition + new Vector3(column, row, 0.0f);
    }

    public static void SetupBlockByColor(Block block, BlockColor blockColor)
    {
        block.GetComponent<MeshRenderer>().material = blockMaterials[(int)blockColor];
        block.BlockColor = blockColor;
    }

    public static string GetBlockNameByRowColumn(int row, int column)
    {
        return "Block " + row.ToString("D2") + " " + column.ToString("D2");
    }

    void GenerateMaterials()
    {
        if (blockMaterials == null) blockMaterials = new Material[blockTextures.Length];

        for (int i = 0; i < blockMaterials.Length; i++)
        {
            if (blockMaterials[i] == null) blockMaterials[i] = new Material(Shader.Find("Unlit/Transparent"));

            blockMaterials[i].mainTexture = blockTextures[i];
        }
    }
}
