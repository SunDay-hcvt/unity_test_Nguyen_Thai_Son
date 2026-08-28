using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnPlay;

    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private Button btnTimeAttack;

    private UIMainManager m_mngr;

    private void Awake()
    {
        AutoAssignButtons();

        if (btnPlay)
        {
            btnPlay.onClick.AddListener(OnClickPlay);
            SetButtonText(btnPlay, "Play");
        }

        if (btnMoves)
        {
            btnMoves.onClick.AddListener(OnClickAutoLose);
            SetButtonText(btnMoves, "Auto Lose");
        }

        if (btnTimer)
        {
            btnTimer.onClick.AddListener(OnClickAutoplay);
            SetButtonText(btnTimer, "Auto Play");
        }

        if (btnTimeAttack)
        {
            btnTimeAttack.onClick.AddListener(OnClickTimeAttack);
            SetButtonText(btnTimeAttack, "Time Attack");
        }
    }

    private void OnDestroy()
    {
        if (btnPlay) btnPlay.onClick.RemoveAllListeners();
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
        if (btnTimeAttack) btnTimeAttack.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickPlay()
    {
        m_mngr.LoadLevelPlay();
    }

    private void OnClickAutoplay()
    {
        m_mngr.LoadLevelTimer();
    }

    private void OnClickAutoLose()
    {
        m_mngr.LoadLevelMoves();
    }

    private void OnClickTimeAttack()
    {
        m_mngr.LoadLevelTimeAttack();
    }

    private void SetButtonText(Button button, string text)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText)
        {
            buttonText.text = text;
        }
    }

    private void AutoAssignButtons()
    {
        if (btnPlay == null)
        {
            Transform play = transform.Find("btnPlay");
            if (play)
            {
                btnPlay = play.GetComponent<Button>();
            }
        }

        if (btnMoves == null)
        {
            Transform moves = transform.Find("btnMoves");
            if (moves)
            {
                btnMoves = moves.GetComponent<Button>();
            }
        }

        if (btnTimer == null)
        {
            Transform autoplay = transform.Find("btnAutoplay");
            if (autoplay == null)
            {
                autoplay = transform.Find("btnAutoPlay");
            }

            if (autoplay)
            {
                btnTimer = autoplay.GetComponent<Button>();
            }
        }

        if (btnTimeAttack == null)
        {
            Transform timeAttack = transform.Find("btnTimeAttack");
            if (timeAttack)
            {
                btnTimeAttack = timeAttack.GetComponent<Button>();
            }
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
