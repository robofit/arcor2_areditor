using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TransformWheel : MonoBehaviour
{
    public List<TransformWheelItem> TransformWheelItems = new List<TransformWheelItem>();

    private void InitList() {
        TransformWheelItems.Clear();
        foreach (Transform child in List.transform) {
            GameObject.Destroy(child.gameObject);
        }
        TransformWheelItem newItem = Instantiate(ItemPrefab, List.transform).GetComponent<TransformWheelItem>();
        newItem.SetValue(0);
        TransformWheelItems.Add(newItem);
    }

    private void Awake() {
        InitList();
    }

    private void Update() {
        while (TransformWheelItems.Count < 12) {
            GenerateNewItem();
        }
        if (Math.Abs(List.transform.position.y) > ((TransformWheelItems.Count - 10) * 24)) {
            GenerateNewItem();
        }
    }



    public GameObject ItemPrefab, List;

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
