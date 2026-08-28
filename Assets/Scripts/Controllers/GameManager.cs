using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public event Action<eStateGame> StateChangedAction = delegate { };

    public enum eLevelMode
    {
        PLAY,
        TIMER,
        MOVES,
        TIME_ATTACK
    }

    public enum eAutoMode
    {
        NONE,
        WIN,
        LOSE
    }

    public enum eStateGame
    {
        SETUP,
        MAIN_MENU,
        GAME_STARTED,
        PAUSE,
        GAME_OVER,
    }

    private eStateGame m_state;
    public eStateGame State
    {
        get { return m_state; }
        private set
        {
            m_state = value;

            StateChangedAction(m_state);
        }
    }

    public enum eGameResult
    {
        NONE,
        WIN,
        LOSE
    }

    public eGameResult Result { get; private set; }

    public void WinGame()
    {
        Result = eGameResult.WIN;
        GameOver();
    }

    public void LoseGame()
    {
        Result = eGameResult.LOSE;
        GameOver();
    }

    private GameSettings m_gameSettings;


    private BoardController m_boardController;

    private UIMainManager m_uiMenu;

    private LevelCondition m_levelCondition;

    private void Awake()
    {
        State = eStateGame.SETUP;

        m_gameSettings = Resources.Load<GameSettings>(Constants.GAME_SETTINGS_PATH);

        m_uiMenu = FindObjectOfType<UIMainManager>();
        m_uiMenu.Setup(this);
    }

    void Start()
    {
        State = eStateGame.MAIN_MENU;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_boardController != null) m_boardController.Update();
    }


    internal void SetState(eStateGame state)
    {
        State = state;

        if (State == eStateGame.PAUSE)
        {
            DOTween.PauseAll();
        }
        else
        {
            DOTween.PlayAll();
        }
    }

    public void LoadLevel(eLevelMode mode)
    {
        LoadLevel(mode, eAutoMode.NONE);
    }

    public void LoadLevel(eAutoMode autoMode)
    {
        LoadLevel(eLevelMode.PLAY, autoMode);
    }

    public void LoadLevel(eLevelMode mode, eAutoMode autoMode)
    {
        Result = eGameResult.NONE;

        Text conditionView = m_uiMenu.GetLevelConditionView();
        if (conditionView)
        {
            conditionView.text = string.Empty;
        }

        m_uiMenu.SetTimerVisible(mode == eLevelMode.TIME_ATTACK);

        m_boardController = new GameObject("BoardController").AddComponent<BoardController>();
        m_boardController.StartGame(this, m_gameSettings, autoMode, mode);

        if (mode == eLevelMode.TIME_ATTACK)
        {
            m_levelCondition = gameObject.AddComponent<LevelTime>();
            m_levelCondition.Setup(60f, conditionView, this);
            m_levelCondition.ConditionCompleteEvent += LoseGame;
        }

        State = eStateGame.GAME_STARTED;
    }

    public void GameOver()
    {
        StartCoroutine(WaitBoardController());
    }

    internal void ClearLevel()
    {
        DOTween.KillAll();

        if (m_boardController)
        {
            m_boardController.Clear();
            Destroy(m_boardController.gameObject);
            m_boardController = null;
        }

        ClearLevelCondition();
    }

    private void ClearLevelCondition()
    {
        if (m_levelCondition == null)
        {
            return;
        }

        m_levelCondition.ConditionCompleteEvent -= GameOver;
        m_levelCondition.ConditionCompleteEvent -= LoseGame;
        Destroy(m_levelCondition);
        m_levelCondition = null;
    }

    private IEnumerator WaitBoardController()
    {
        while (m_boardController != null && m_boardController.IsBusy)
        {
            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(1f);

        State = eStateGame.GAME_OVER;

        ClearLevelCondition();
    }
}
