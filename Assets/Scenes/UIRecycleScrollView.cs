using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Linq;

public class UIRecycleScrollView : UIBehaviour
{
    [SerializeField] private ScrollRect ScrollRect;
    [SerializeField] private RectTransform ViewRect;
    [SerializeField] private RectTransform Content;

    private List<IScrollCell> cellList;
    private List<IScrollData> dataList;

    [SerializeField] private GameObject cellPrefab;

    [SerializeField] Vector2 CellMinSize = Vector2.zero;

    private int startIndex = -1;

    protected override void OnEnable()
    {
        if (startIndex == -1)
            startIndex = 0;

        SetUpCell();
        SetUpData();
    }

    public void SetData(List<IScrollData> InDataList)
    {
        if (dataList != null)
        {
            dataList.Clear();
            dataList = null;
        }
        dataList = InDataList;
    }

    public void AddData(IScrollData InData)
    {
        if (dataList == null)
            return;

        dataList.Add(InData);
    }

    //@TODO:
    // 현재는 vertical로 생각하고 작성
    // 후 SetUpCell Op를 작성할 예정
    protected void SetUpCell()
    {
        if (cellList == null)
        {
            cellList = new List<IScrollCell>();

            float height = 0f;
            var scrollRectHeight = ScrollRect.GetComponent<RectTransform>().rect.height;
            
            while(height < scrollRectHeight)
            {
                var go = Instantiate(cellPrefab, Content);
                if (go == null)
                {
                    Debug.LogError("Error: View Item is not exist");
                    break;
                }
                var cellComponent = go.GetComponent<IScrollCell>();
                if (cellComponent == null)
                {
                    Debug.LogError("Error: This prefab don't have cell component");
                    break;
                }

                go.SetActive(false);
                go.GetComponent<RectTransform>().anchorMax = Vector2.up;
                go.GetComponent<RectTransform>().anchorMin = Vector2.up;
                go.GetComponent<RectTransform>().pivot = Vector2.up;

                height += CellMinSize.y;
                StartCoroutine(UpdateCell(cellComponent));
                cellList.Add(cellComponent);
            }

            Content.sizeDelta = new Vector2(Content.sizeDelta.x, height);

            return;
        }
    }

    private void SetUpData()
    {
        if (dataList == null)
            dataList = new List<IScrollData>();
    }

    private IEnumerator UpdateCell(IScrollCell InCell)
    {
        yield return null;

        var waitYieldForIsData = new WaitUntil(() => InCell.GetData() != null);

        var cellIndex = cellList.IndexOf(InCell);
        
        StartCoroutine(BindData(InCell, cellIndex));

        while (true)
        {
            yield return waitYieldForIsData;

            //@TODO: 위치 확인 코드 데이터 교체
        }
    }

    private IEnumerator BindData(IScrollCell cell, int index)
    {
        while(index >= dataList.Count)
            yield return null;

        var beforeHeight = cell.RectTransform.sizeDelta.y;
        cell.SetData(dataList[index]);
        var afterHeihgt = cell.RectTransform.sizeDelta.y;

        Content.sizeDelta += Vector2.up * (afterHeihgt - beforeHeight);

        cell.RectTransform.gameObject.SetActive(true);

        for(int i = startIndex; i < index; ++i)
        {
            cell.RectTransform.anchoredPosition += Vector2.up * cellList[i].RectTransform.rect.height * -1;
        }
    }
}