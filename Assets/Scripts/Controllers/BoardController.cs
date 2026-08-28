using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };

    public bool IsBusy { get; private set; }

    private Board m_board;

    private GameManager m_gameManager;

    private Camera m_cam;

    private GameSettings m_gameSettings;

    private bool m_gameOver;

    private GameManager.eAutoMode m_autoMode;

    private GameManager.eLevelMode m_levelMode;

    private bool IsTimeAttack => m_levelMode == GameManager.eLevelMode.TIME_ATTACK;

    public void StartGame(GameManager gameManager, GameSettings gameSettings, GameManager.eAutoMode autoMode, GameManager.eLevelMode levelMode = GameManager.eLevelMode.PLAY)
    {
        m_gameManager = gameManager;

        m_gameSettings = gameSettings;

        m_autoMode = autoMode;

        m_levelMode = levelMode;

        m_gameManager.StateChangedAction += OnGameStateChange;

        m_cam = Camera.main;

        m_board = new Board(this.transform, gameSettings);

        Fill();

        if (m_autoMode == GameManager.eAutoMode.WIN)
        {
            StartCoroutine(AutoplayWinCoroutine());
        }
        else if (m_autoMode == GameManager.eAutoMode.LOSE)
        {
            StartCoroutine(AutoplayLoseCoroutine());
        }
    }

    private void Fill()
    {
        m_board.Fill();
        IsBusy = false;
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                break;
        }
    }


    public void Update()
    {
        if (m_gameOver) return;
        if (IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();

                if (cell == null || cell.IsEmpty)
                {
                    return;
                }

                if (cell.BoardY < 0)
                {
                    if (IsTimeAttack)
                    {
                        StartCoroutine(ReturnItemToBoard(cell));
                    }

                    return;
                }

                StartCoroutine(MoveItemToBottom(cell));
            }
        }
    }

    private IEnumerator MoveItemToBottom(Cell boardCell)
    {
        Cell bottomCell = m_board.BottomCells.FirstOrDefault(c => c.IsEmpty);

        if (bottomCell == null)
        {
            if (!IsTimeAttack)
            {
                m_gameManager.LoseGame();
            }

            yield break;
        }

        Item item = boardCell.Item;
        item.OriginCell = boardCell;
        item.IsInTransit = true;
        boardCell.Free();

        bottomCell.Assign(item);
        item.View.DOKill();
        Tween moveTween = item.View.DOMove(bottomCell.transform.position, 0.3f)
            .SetEase(Ease.OutCubic);

        yield return moveTween.WaitForCompletion();

        item.IsInTransit = false;
        if (item.View != null)
        {
            item.SetViewPosition(bottomCell.transform.position);
        }

        bool clearedMatch = ClearBottomMatches();
        if (clearedMatch)
        {
            IsBusy = true;
            yield return new WaitForSeconds(0.25f);
            IsBusy = false;
        }

        if (m_board.IsBoardCleared())
        {
            m_gameManager.WinGame();
        }
        else if (!IsTimeAttack && m_board.BottomCells.All(c => !c.IsEmpty))
        {
            m_gameManager.LoseGame();
        }
    }

    private IEnumerator ReturnItemToBoard(Cell bottomCell)
    {
        Item item = bottomCell.Item;
        Cell originCell = item.OriginCell;

        if (originCell == null || !originCell.IsEmpty)
        {
            yield break;
        }

        bottomCell.Free();
        originCell.Assign(item);
        item.OriginCell = null;

        item.View.DOKill();
        Tween moveTween = item.View.DOMove(originCell.transform.position, 0.3f)
            .SetEase(Ease.OutCubic);

        yield return moveTween.WaitForCompletion();
    }

    private IEnumerator AutoplayWinCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (!m_gameOver && !m_board.IsBoardCleared())
        {
            Cell cell = FindAutoplayWinCell();

            if (cell == null)
            {
                m_gameManager.LoseGame();
                yield break;
            }

            yield return MoveItemToBottom(cell);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator AutoplayLoseCoroutine()
    {
        yield return new WaitForSeconds(0.5f);

        while (!m_gameOver && m_board.BottomCells.Any(c => c.IsEmpty))
        {
            Cell cell = FindAutoplayLoseCell();

            if (cell == null)
            {
                cell = m_board.GetFilledBoardCells().FirstOrDefault();
            }

            if (cell == null)
            {
                yield break;
            }

            yield return MoveItemToBottom(cell);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Cell FindAutoplayWinCell()
    {
        var bottomGroups = m_board.BottomCells
            .Where(c => !c.IsEmpty && c.Item is NormalItem)
            .GroupBy(c => ((NormalItem)c.Item).ItemType);

        foreach (var group in bottomGroups)
        {
            Cell cell = FindBoardCellOfType(group.Key);
            if (cell != null)
            {
                return cell;
            }
        }

        return m_board.GetFilledBoardCells()
            .Where(c => c.Item is NormalItem)
            .GroupBy(c => ((NormalItem)c.Item).ItemType)
            .Where(g => g.Count() >= 3)
            .Select(g => g.First())
            .FirstOrDefault();
    }

    private Cell FindAutoplayLoseCell()
    {
        Dictionary<NormalItem.eNormalType, int> bottomCounts = GetBottomTypeCounts();

        return m_board.GetFilledBoardCells()
            .Where(c => c.Item is NormalItem)
            .FirstOrDefault(c =>
            {
                NormalItem item = (NormalItem)c.Item;
                return !bottomCounts.ContainsKey(item.ItemType) || bottomCounts[item.ItemType] < 2;
            });
    }

    private Dictionary<NormalItem.eNormalType, int> GetBottomTypeCounts()
    {
        Dictionary<NormalItem.eNormalType, int> result = new Dictionary<NormalItem.eNormalType, int>();

        foreach (Cell cell in m_board.BottomCells)
        {
            NormalItem item = cell.Item as NormalItem;
            if (item == null)
            {
                continue;
            }

            if (!result.ContainsKey(item.ItemType))
            {
                result[item.ItemType] = 0;
            }

            result[item.ItemType]++;
        }

        return result;
    }

    private Cell FindBoardCellOfType(NormalItem.eNormalType type)
    {
        return m_board.GetFilledBoardCells()
            .FirstOrDefault(c =>
            {
                NormalItem item = c.Item as NormalItem;
                return item != null && item.ItemType == type;
            });
    }

    private bool ClearBottomMatches()
    {
        var groups = m_board.BottomCells
            .Where(c => !c.IsEmpty && c.Item is NormalItem && !c.Item.IsInTransit)
            .GroupBy(c => ((NormalItem)c.Item).ItemType);

        foreach (var group in groups)
        {
            if (group.Count() == 3)
            {
                foreach (Cell cell in group)
                {
                    cell.ExplodeItem();
                }

                return true;
            }
        }

        return false;
    }

    private void SetSortingLayer(Cell cell1, Cell cell2)
    {
        if (cell1.Item != null) cell1.Item.SetSortingLayerHigher();
        if (cell2.Item != null) cell2.Item.SetSortingLayerLower();
    }

    private bool AreItemsNeighbor(Cell cell1, Cell cell2)
    {
        return cell1.IsNeighbour(cell2);
    }

    internal void Clear()
    {
        m_board.Clear();
    }
}