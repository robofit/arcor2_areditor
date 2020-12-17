using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Base;

public class SelectorMenu : Singleton<SelectorMenu>
{
    public GameObject SelectorItemPrefab;

    public GameObject Content;

    public void UpdateMenu(List<Transform> items) {
        List<string> strings = new List<string>();
        
        int count = 0;
        foreach (Transform item in items) {
            InteractiveObject interactiveObject = item.gameObject.GetComponent<InteractiveObject>();
            if (interactiveObject is null) {
                OnClickCollider collider = item.gameObject.GetComponent<OnClickCollider>();
                if (collider is null) {
                    continue;
                }
                interactiveObject = collider.Target.gameObject.GetComponent<InteractiveObject>();
                if (interactiveObject is null) {
                    continue;
                }
            }


            strings.Add(interactiveObject.GetName());
            if (count++ > 6)
                return;
        }
        UpdateMenu(strings);
        
    }

    public void UpdateMenu(List<string> items) {
        foreach (Transform t in Content.transform) {
            if (!t.CompareTag("Persistent")) {
                Destroy(t.gameObject);
            }
        }
        foreach (string item in items) {
            SelectorItem selectorItem = Instantiate(SelectorItemPrefab, Content.transform).GetComponent<SelectorItem>();
            selectorItem.SetText(item);
        }
        
    }
}
