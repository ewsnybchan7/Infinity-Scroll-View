using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IScrollCell
{
    public RectTransform RectTransform { get; }
    public object GetData();
    public void SetData(object InData);
}
