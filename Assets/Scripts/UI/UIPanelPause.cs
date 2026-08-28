using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelPause : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnClose;
    [SerializeField] private Button btnMainMenu;

    private UIMainManager m_mngr;

    private void Awake()
    {
        AutoAssignButtons();

        if (btnClose)
        {
            btnClose.onClick.AddListener(OnClickClose);
        }

        if (btnMainMenu)
        {
            btnMainMenu.onClick.AddListener(OnClickMainMenu);
        }
    }

    private void OnDestroy()
    {
        if (btnClose) btnClose.onClick.RemoveAllListeners();
        if (btnMainMenu) btnMainMenu.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickClose()
    {
        m_mngr.ShowGameMenu();
    }

    private void OnClickMainMenu()
    {
        m_mngr.ShowMainMenu();
    }

    private void AutoAssignButtons()
    {
        if (btnClose == null)
        {
            Transform close = transform.Find("Panel/btnOk");
            if (close)
            {
                btnClose = close.GetComponent<Button>();
            }
        }

        if (btnMainMenu == null)
        {
            Transform mainMenu = transform.Find("btnMainMenu");
            mainMenu ??= transform.Find("Panel/btnMainMenu");

            if (mainMenu)
            {
                btnMainMenu = mainMenu.GetComponent<Button>();
            }
        }

        if (btnMainMenu == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                Button button = buttons[i];
                if (button == btnClose)
                {
                    continue;
                }

                if (button.name.IndexOf("Main", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    btnMainMenu = button;
                    break;
                }

                Text buttonText = button.GetComponentInChildren<Text>();
                if (buttonText && buttonText.text.IndexOf("Main", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    btnMainMenu = button;
                    break;
                }
            }
        }
    }

    private void SetButtonText(Button button, string text)
    {
        Text buttonText = button.GetComponentInChildren<Text>();
        if (buttonText)
        {
            buttonText.text = text;
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
