using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{

    public int rows = 10;
    public int columns = 8;

    public Camera gameCamera;

    public Button shuffleButton;

    BlockGenerator blockGenerator;

    Blocks blocks;

    SelectionManager selectionManager;

    List<Block> explodingBlocks = new List<Block>();

    int[] numBlocksExplodedInColumns;
    int[] numBlocksRespawnedInColumns;

    enum State
    {
        BlocksSelecting,
        BlocksExploding,
        BlocksFalling,
    }
    State state = State.BlocksSelecting;

    void Awake()
    {
        gameCamera.orthographicSize = columns + 2;

        blockGenerator = GetComponentInChildren<BlockGenerator>();
        blockGenerator.Generate(rows, columns, out blocks);

        selectionManager = GetComponent<SelectionManager>();

        numBlocksExplodedInColumns = new int[columns];
        numBlocksRespawnedInColumns = new int[columns];

        shuffleButton.gameObject.SetActive(false);
    }

    void Update()
    {
        switch(state)
        {
            case State.BlocksSelecting:
                UpdateBlocksSelectingState();
                break;

            case State.BlocksExploding:
                UpdateBlocksExplodingState();
                break;

            case State.BlocksFalling:
                UpdateBlocksFallingState();
                break;
        }
    }

    void UpdateBlocksSelectingState()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = gameCamera.ScreenPointToRay(Input.mousePosition);

            bool doneSearching = false;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    Block block = blocks[r, c];

                    if (block.GetComponent<Collider>().Raycast(ray, out RaycastHit hit, 100.0f))
                    {
                        if (!selectionManager.Contains(block))
                        {
                            Block lastSelectedBlock = selectionManager.GetLastSelectedBlock();
                            if (lastSelectedBlock == null ||
                                (lastSelectedBlock.HasSameColor(block) && lastSelectedBlock.IsNextTo(block)))
                            {
                                selectionManager.AddUnique(block);
                                selectionManager.UpdateSelectionLine();
                            }
                        }
                        else
                        {
                            if (ShouldRemoveFollowingBlocks(block))
                            {
                                selectionManager.RemoveFollowingBlocks(block);
                                selectionManager.UpdateSelectionLine();
                            }
                        }

                        doneSearching = true;
                        break;
                    }
                }

                if (doneSearching) break;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (selectionManager.NumSelectedBlocks >= 3)
            {
                BeginBlocksExplodingState();
            }
            else
            {
                selectionManager.Clear();
                selectionManager.UpdateSelectionLine();
            }
        }
    }

    bool ShouldRemoveFollowingBlocks(Block block)
    {
        int listIndex = selectionManager.GetListIndex(block);
        if (listIndex != -1)
        {
            return listIndex < blocks.NumBlocks - 1;
        }

        return false;
    }

    int GetListIndex(Block block)
    {
        return selectionManager.GetListIndex(block);
    }

    void BeginBlocksExplodingState()
    {
        state = State.BlocksExploding;

        selectionManager.CopySelectedBlocks(ref explodingBlocks);

        selectionManager.Clear();
        selectionManager.UpdateSelectionLine();

        Utils.ResetArrayValue<int>(numBlocksExplodedInColumns, 0);
        Utils.ResetArrayValue<int>(numBlocksRespawnedInColumns, 0);

        for (int i = 0; i < explodingBlocks.Count; i++)
        {
            ++numBlocksExplodedInColumns[explodingBlocks[i].Column];

            explodingBlocks[i].Explode();
        }
    }

    void UpdateBlocksExplodingState()
    {
        bool doneExploding = true;
        for (int i = 0; i < explodingBlocks.Count; i++)
        {
            if (explodingBlocks[i].IsExploding)
            {
                doneExploding = false;
                break;
            }
        }

        if (doneExploding)
        {
            BeginBlocksFallingState();
        }
    }

    void BeginBlocksFallingState()
    {
        state = State.BlocksFalling;

        List<FallingBlockRecord> records = new List<FallingBlockRecord>();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                if (blocks[r, c].IsExploded) continue;

                Block block = blocks[r, c];

                int numExplodedBlocks = GetNumExplodedBlocksUnder(block);

                if (numExplodedBlocks > 0)
                {
                    int oldRow = block.Row;
                    int newRow = oldRow - numExplodedBlocks;

                    records.Add(new FallingBlockRecord
                    {
                        fromRow = oldRow,
                        fromColumn = block.Column,
                        toRow = newRow,
                        toColumn = block.Column
                    });
                }
            }
        }

        for (int i = 0; i < explodingBlocks.Count; i++)
        {
            Block block = explodingBlocks[i];

            int oldRow = block.Row;
            int oldCol = block.Column;

            Vector3 position = BlockGenerator.CalculatePositionByRowColumn(rows + numBlocksRespawnedInColumns[block.Column] + 2, block.Column);
            block.Position = position;

            block.Row = rows - numBlocksExplodedInColumns[block.Column] + numBlocksRespawnedInColumns[block.Column];

            ++numBlocksRespawnedInColumns[block.Column];

            block.ResetBlock();
            block.name = BlockGenerator.GetBlockNameByRowColumn(block.Row, block.Column);

            int randomColorIndex = Random.Range(0, BlockGenerator.NumBlockColors);
            BlockGenerator.SetupBlockByColor(block, (BlockGenerator.BlockColor)randomColorIndex);

            Vector3 fallingDestination = BlockGenerator.CalculatePositionByRowColumn(block.Row, block.Column);
            block.Fall(fallingDestination);
        }

        for (int i = 0; i < records.Count; i++)
        {
            FallingBlockRecord record = records[i];

            Block block = blocks[record.fromRow, record.fromColumn];

            block.Row = record.toRow;
            block.Column = record.toColumn;

            block.ResetBlock();
            block.name = BlockGenerator.GetBlockNameByRowColumn(block.Row, block.Column);

            blocks[block.Row, block.Column] = block;

            Vector3 fallingDestination = BlockGenerator.CalculatePositionByRowColumn(block.Row, block.Column);
            block.Fall(fallingDestination);
        }

        for (int i = 0; i < explodingBlocks.Count; i++)
        {
            Block block = explodingBlocks[i];
            blocks[block.Row, block.Column] = block;
        }

        ValidateBlocks();
    }

    int GetNumExplodedBlocksUnder(Block block)
    {
        int count = 0;
        for (int r = block.Row - 1; r >= 0; r--)
        {
            if (blocks[r, block.Column].IsExploded)
            {
                ++count;
            }
        }
        return count;
    }

    void UpdateBlocksFallingState()
    {
        bool doneFalling = true;
        for (int i = 0; i < explodingBlocks.Count; i++)
        {
            if (explodingBlocks[i].IsFalling)
            {
                doneFalling = false;
                break;
            }
        }

        if (doneFalling)
        {
            state = State.BlocksSelecting;

            ValidateBlocks();

            if (!IsBoardPlayable())
            {
                shuffleButton.gameObject.SetActive(true);
            }
        }
    }

    bool IsBoardPlayable()
    {
        int sameColoredBlocks = 0;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                Block block = blocks[r, c];

                sameColoredBlocks = 0;

                if (r + 1 < rows && blocks[r + 1, c].BlockColor == block.BlockColor) ++sameColoredBlocks;
                if (r - 1 >= 0 && blocks[r - 1, c].BlockColor == block.BlockColor) ++sameColoredBlocks;
                if (c + 1 < columns && blocks[r, c + 1].BlockColor == block.BlockColor) ++sameColoredBlocks;
                if (c - 1 >= 0 && blocks[r, c - 1].BlockColor == block.BlockColor) ++sameColoredBlocks;

                if (sameColoredBlocks >= 2)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void OnClickShuffleButton()
    {
        Shuffle();

        if (IsBoardPlayable())
        {
            shuffleButton.gameObject.SetActive(false);
        }
    }

    void Shuffle(int iterations = 10)
    {
        for (int i = 0; i < iterations; i++)
        {
            int row1 = Random.Range(0, rows);
            int row2 = Random.Range(0, rows);
            int col1 = Random.Range(0, columns);
            int col2 = Random.Range(0, columns);

            if (row1 == row2 && col1 == col2)
            {
                row2 = (row1 + 1) % rows;
                col2 = (col1 + 1) % columns;
            }

            BlockGenerator.BlockColor tempColor = blocks[row1, col1].BlockColor;
            BlockGenerator.SetupBlockByColor(blocks[row1, col1], blocks[row2, col2].BlockColor);
            BlockGenerator.SetupBlockByColor(blocks[row2, col2], tempColor);
        }

        ValidateBlocks();
    }

    void ValidateBlocks()
    {
        Block[] allBlocks = FindObjectsOfType<Block>();
        
        for (int i = 0; i < allBlocks.Length; i++)
        {
            for (int j = 0; j < allBlocks.Length; j++)
            {
                if (i == j) continue;

                if (allBlocks[i].Row == allBlocks[j].Row && allBlocks[i].Column == allBlocks[j].Column)
                {
                    Debug.LogError("Found two blocks with the same row and column => " + allBlocks[i].Row + ", " + allBlocks[i].Column);
                }
            }
        }
    }
}

internal class FallingBlockRecord
{
    public int fromRow;
    public int fromColumn;

    public int toRow;
    public int toColumn;
}
