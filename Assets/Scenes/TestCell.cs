using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TestCell : UIBehaviour, IScrollCell
{
    [SerializeField] Text Text_Data;

    private RectTransform _rectTransform;
    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            return _rectTransform;
        }
    }

    private TestData Data;
    public object GetData()
    {
        return Data;
    }

    public void SetData(object InData)
    {
        if (!(InData is TestData InTestData))
            return;

        Data = InTestData;

        Text_Data.text = Data.text;

        Canvas.ForceUpdateCanvases();
    }
}
