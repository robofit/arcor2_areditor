using System;
using UnityEngine.UI;

public class LeftMenuScene : LeftMenu
{

    public Button AddMeshButton;
    protected override void Update() {
        base.Update();

    }

    protected override void UpdateBtns(InteractiveObject selectedObject) {
        base.UpdateBtns(selectedObject);
        if (requestingObject || selectedObject == null) {

        } else {

        }
    }

    protected override void DeactivateAllSubmenus() {
        base.DeactivateAllSubmenus();
        AddMeshButton.GetComponent<Image>().enabled = false;

    }

    public void AddMeshClick() {
        if (AddMeshButton.GetComponent<Image>().enabled) {
            AddMeshButton.GetComponent<Image>().enabled = false;
            SelectorMenu.Instance.gameObject.SetActive(true);
            MeshPicker.SetActive(false);
        } else {
            AddMeshButton.GetComponent<Image>().enabled = true;
            SelectorMenu.Instance.gameObject.SetActive(false);
            MeshPicker.SetActive(true);
        }

    }
}
