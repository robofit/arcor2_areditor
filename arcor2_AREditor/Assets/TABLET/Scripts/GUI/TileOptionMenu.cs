using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TileOptionMenu : OptionMenu {
    [SerializeField]
    private GameObject AddStarBtn, RemoveStarBtn;


    public void Open(Tile tile) {
        AddStarBtn.SetActive(!tile.GetStarred());
        RemoveStarBtn.SetActive(tile.GetStarred());
        Open(tile.GetLabel());
    }

    public abstract void SetStar(bool starred);

    public virtual void SetStar(Tile tile, bool starred) {
        tile.SetStar(starred);
        MainScreen.Instance.FilterTile(tile);
        Close();
    }
}
