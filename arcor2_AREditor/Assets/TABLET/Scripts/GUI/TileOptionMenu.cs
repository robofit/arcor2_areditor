using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileOptionMenu : OptionMenu
{
    [SerializeField]
    private GameObject AddStarBtn, RemoveStarBtn;


    public void Open(string label, bool starred) {
        AddStarBtn.SetActive(!starred);
        RemoveStarBtn.SetActive(starred);
        Open(label);
    }

    public abstract void SetStar(bool starred);
}
