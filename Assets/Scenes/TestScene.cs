using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScene : MonoBehaviour
{
    [SerializeField] UIRecycleScrollView recycleView;

    [SerializeField] Button button;

    private int num = 0;

    public void Start()
    {
        button.onClick.AddListener(OnClickButton);
    }

    public void OnClickButton()
    {
        recycleView.AddData(new TestData
        {
            text = "Data" + num++
        });
    }
}
