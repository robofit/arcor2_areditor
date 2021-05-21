using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Base;
using UnityEngine;

public abstract class TileOptionMenu : OptionMenu {
    [SerializeField]
    private GameObject AddStarBtn, RemoveStarBtn;


    protected override void Start() {
        base.Start();
        Debug.Assert(AddStarBtn != null);
        Debug.Assert(RemoveStarBtn != null);
    }

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

    protected async Task<bool> WriteLockProjectOrScene(string id) {
        try {
            await WebsocketManager.Instance.WriteLock(id, false);
            return true;
        } catch (RequestFailedException ex) {
            Notifications.Instance.ShowNotification("Failed to lock " + GetLabel(), ex.Message);
            return false;
        }
    }
}
