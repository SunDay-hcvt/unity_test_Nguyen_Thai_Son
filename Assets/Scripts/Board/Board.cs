using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board
{
    public enum eMatchDirection
    {
        NONE,
        HORIZONTAL,
        VERTICAL,
        ALL
    }

    private int boardSizeX;

    private int boardSizeY;

    private Cell[,] m_cells;

    private Transform m_root;

    private List<Cell> m_bottomCells = new List<Cell>();

    public List<Cell> BottomCells => m_bottomCells;

    private int m_matchMin;

    public Board(Transform transform, GameSettings gameSettings)
    {
        m_root = transform;

        m_matchMin = gameSettings.MatchesMin;

        this.boardSizeX = gameSettings.BoardSizeX;
        this.boardSizeY = gameSettings.BoardSizeY;

        m_cells = new Cell[boardSizeX, boardSizeY];

        CreateBoard();
        CreateBottomCells();
    }

    private void CreateBoard()
    {
        Vector3 origin = new Vector3(-boardSizeX * 0.5f + 0.5f, -boardSizeY * 0.5f + 0.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                GameObject go = GameObject.Instantiate(prefabBG);
                go.transform.position = origin + new Vector3(x, y, 0f);
                go.transform.SetParent(m_root);

                Cell cell = go.GetComponent<Cell>();
                cell.Setup(x, y);

                m_cells[x, y] = cell;
            }
        }

        //set neighbours
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (y + 1 < boardSizeY) m_cells[x, y].NeighbourUp = m_cells[x, y + 1];
                if (x + 1 < boardSizeX) m_cells[x, y].NeighbourRight = m_cells[x + 1, y];
                if (y > 0) m_cells[x, y].NeighbourBottom = m_cells[x, y - 1];
                if (x > 0) m_cells[x, y].NeighbourLeft = m_cells[x - 1, y];
            }
        }

    }

    private void CreateBottomCells()
    {
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);

        float startX = -2f;
        float y = -boardSizeY * 0.5f - 1.2f;

        for (int i = 0; i < 5; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = new Vector3(startX + i, y, 0f);
            go.transform.SetParent(m_root);

            Cell cell = go.GetComponent<Cell>();
            cell.Setup(i, -1);

            m_bottomCells.Add(cell);
        }
    }

    internal void Fill()
    {
        List<NormalItem.eNormalType> types = CreateInitialTypes();

        int index = 0;

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];

                NormalItem item = new NormalItem();
                item.SetType(types[index]);
                item.SetView();
                item.SetViewRoot(m_root);

                cell.Assign(item);
                cell.ApplyItemPosition(false);

                index++;
            }
        }
    }

    internal void Shuffle()
    {
        List<Item> list = new List<Item>();
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                list.Add(m_cells[x, y].Item);
                m_cells[x, y].Free();
            }
        }

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                int rnd = UnityEngine.Random.Range(0, list.Count);
                m_cells[x, y].Assign(list[rnd]);
                m_cells[x, y].ApplyItemMoveToPosition();

                list.RemoveAt(rnd);
            }
        }
    }

    public void Clear()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Cell cell = m_cells[x, y];
                cell.Clear();

                GameObject.Destroy(cell.gameObject);
                m_cells[x, y] = null;
            }
        }

        for (int i = 0; i < m_bottomCells.Count; i++)
        {
            m_bottomCells[i].Clear();
            GameObject.Destroy(m_bottomCells[i].gameObject);
        }

        m_bottomCells.Clear();
    }

    public bool IsBoardCleared()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (!m_cells[x, y].IsEmpty)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public List<Cell> GetFilledBoardCells()
    {
        List<Cell> result = new List<Cell>();

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                if (!m_cells[x, y].IsEmpty)
                {
                    result.Add(m_cells[x, y]);
                }
            }
        }

        return result;
    }

    private List<NormalItem.eNormalType> CreateInitialTypes()
    {
        int total = boardSizeX * boardSizeY;
        List<NormalItem.eNormalType> result = new List<NormalItem.eNormalType>();

        NormalItem.eNormalType[] allTypes = (NormalItem.eNormalType[])Enum.GetValues(typeof(NormalItem.eNormalType));

        for (int i = 0; i < allTypes.Length; i++)
        {
            result.Add(allTypes[i]);
            result.Add(allTypes[i]);
            result.Add(allTypes[i]);
            result.Add(allTypes[i]);
            result.Add(allTypes[i]);
            result.Add(allTypes[i]);
        }

        while (result.Count < total)
        {
            NormalItem.eNormalType type = Utils.GetRandomNormalType();

            result.Add(type);
            result.Add(type);
            result.Add(type);
        }

        return result.OrderBy(_ => UnityEngine.Random.value).Take(total).ToList();
    }
}

