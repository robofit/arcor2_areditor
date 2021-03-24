using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransformWheel : MonoBehaviour
{
    public List<TransformWheelItem> TransformWheelItems = new List<TransformWheelItem>();
    public GameObject ItemPrefab;
    public TranformWheelUnits Units;
    public TransformWheelList List;

    public void InitList(int value = 0) {
        TransformWheelItems.Clear();
        List.Init();
        foreach (Transform child in List.transform) {
            if (child.gameObject.tag != "Persistent")
                GameObject.Destroy(child.gameObject);
        }
        TransformWheelItem newItem = Instantiate(ItemPrefab, List.transform).GetComponent<TransformWheelItem>();
        newItem.SetValue(0);
        TransformWheelItems.Add(newItem);
        while (TransformWheelItems.Count < 12) {
            GenerateNewItem();
        }

        SetValue(value);
    }

    public void SetValue(int value) {        
        List.transform.localPosition = new Vector2(0, 0 - value * 80);
    }

    public int GetValue() {        
        return 0 - ClosestInteger((int) List.transform.localPosition.y, 80) / 80;        
    }

    private int ClosestInteger(int n, int m) {
        int q = n / m;

        // 1st possible closest number 
        int n1 = m * q;

        // 2nd possible closest number 
        int n2 = (n * m) > 0 ? (m * (q + 1)) : (m * (q - 1));

        // if true, then n1 is the required closest number 
        if (Math.Abs(n - n1) < Math.Abs(n - n2))
            return n1;

        // else n2 is the required closest number 
        return n2;
    }

    private void Awake() {
        InitList();
    }

    private void Update() {
        RectTransform rectTransform = (RectTransform) List.transform;
        if ((rectTransform.localPosition.y + rectTransform.rect.height / 2 < 1500) ||
            (rectTransform.localPosition.y - 500 - rectTransform.rect.height / 2 > -1500)) {
            GenerateNewItem();
        }
    }




    private void GenerateNewItem() {
        TransformWheelItem newFirst = Instantiate(ItemPrefab, List.transform).GetComponent<TransformWheelItem>();
        TransformWheelItem newLast = Instantiate(ItemPrefab, List.transform).GetComponent<TransformWheelItem>();
        newFirst.transform.SetAsFirstSibling();
        newFirst.SetValue(TransformWheelItems.First().Value + 1);
        newLast.SetValue(TransformWheelItems.Last().Value - 1);
        TransformWheelItems.Insert(0, newFirst);
        TransformWheelItems.Add(newLast);
    }
}
