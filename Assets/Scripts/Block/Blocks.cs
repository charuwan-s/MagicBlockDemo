using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blocks
{
    BlockRow[] blockRows;

    public void Initialize(int rows, int columns)
    {
        blockRows = new BlockRow[rows];

        for (int i = 0; i < rows; i++)
        {
            blockRows[i] = new BlockRow();
            blockRows[i].blocks = new Block[columns];
        }
    }

    public Block this[int r, int c]
    {
        get
        {
            return blockRows[r].blocks[c];
        }

        set
        {
            blockRows[r].blocks[c] = value;
        }
    }

    public int Rows
    {
        get { return blockRows.Length; }
    }

    public int Columns
    {
        get { return blockRows.Length > 0 ? blockRows[0].blocks.Length : 0; }
    }

    public int NumBlocks
    {
        get { return Rows * Columns; }
    }
}

public class BlockRow
{
    public Block[] blocks;
}