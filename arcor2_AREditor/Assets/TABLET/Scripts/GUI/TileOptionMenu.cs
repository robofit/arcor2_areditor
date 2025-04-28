using System.Threading.Tasks;
using Arcor2.ClientSdk.Communication;
using Arcor2.ClientSdk.Communication.OpenApi.Models;
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
            var response = await CommunicationManager.Instance.Client.WriteLockAsync(new WriteLockRequestArgs(id));
            if (!response.Result) {
                Debug.LogError(string.Join(",", response.Messages));
                Notifications.Instance.ShowNotification("Failed to lock " + GetLabel(), string.Join(",", response.Messages));
                return false;
            }
            return true;
        } catch (Arcor2ConnectionException ex) {
            Notifications.Instance.ShowNotification("Failed to lock " + GetLabel(), ex.Message);
            return false;
        }
    }
}
