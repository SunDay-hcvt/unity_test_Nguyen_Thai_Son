using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelGameOver : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;

    [SerializeField] private Text txtResult;

    private UIMainManager m_mngr;

    private void Awake()
    {
        btnClose.onClick.AddListener(OnClickClose);
    }

    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
    }

    private void OnClickClose()
    {
        m_mngr.ShowMainMenu();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);

        GameManager gameManager = FindObjectOfType<GameManager>();

        if (txtResult == null || gameManager == null)
        {
            return;
        }

        if (gameManager.Result == GameManager.eGameResult.WIN)
        {
            txtResult.text = "You Win!";
        }
        else if (gameManager.Result == GameManager.eGameResult.LOSE)
        {
            txtResult.text = "You Lose!";
        }
        else
        {
            txtResult.text = "Game Over";
        }
    }
}
