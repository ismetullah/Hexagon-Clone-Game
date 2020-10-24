using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    #region Public Variables
    public string title;
    public int defaultValue;
    public int minValue;
    public int maxValue;

    public TextMeshProUGUI titleTxt;
    public TextMeshProUGUI valueTxt;
    public Button leftBtn;
    public Button rightBtn;
    #endregion

    #region Private Variables
    private int value;
    #endregion

    #region Event Functions
    private void Start()
    {
        value = defaultValue;
        titleTxt.text = title;
        valueTxt.text = value.ToString();
        leftBtn.onClick.AddListener(OnClickLeftBtn);
        rightBtn.onClick.AddListener(OnClickRightBtn);
    }
    #endregion

    #region Private Variables
    private void OnClickLeftBtn()
    {
        if (value == maxValue) rightBtn.interactable = true;
        if (--value == minValue) leftBtn.interactable = false;
        valueTxt.text = value.ToString();
    }

    private void OnClickRightBtn()
    {
        if (value == minValue) leftBtn.interactable = true;
        if (++value == maxValue) rightBtn.interactable = false;
        valueTxt.text = value.ToString();
    }
    #endregion

    #region Public Methods
    public int GetValue()
    {
        return value;
    }
    #endregion
}
