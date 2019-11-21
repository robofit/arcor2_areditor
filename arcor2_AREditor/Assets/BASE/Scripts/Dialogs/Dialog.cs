using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Michsky.UI.ModernUIPack;
using UnityEngine.UI;

public class Dialog : MonoBehaviour
{
    protected virtual void UpdateToggleGroup(GameObject togglePrefab, GameObject toggleGroup, List<IO.Swagger.Model.IdDesc> idDescs) {
        foreach (Transform toggle in toggleGroup.transform) {
            Destroy(toggle.gameObject);
        }
        foreach (IO.Swagger.Model.IdDesc scene in idDescs) {

            GameObject toggle = Instantiate(togglePrefab, toggleGroup.transform);
            foreach (TMPro.TextMeshProUGUI text in toggle.GetComponentsInChildren<TMPro.TextMeshProUGUI>()) {
                text.text = scene.Id;
            }
            toggle.GetComponent<Toggle>().group = toggleGroup.GetComponent<ToggleGroup>();
            toggle.transform.SetAsFirstSibling();
        }
    }

    protected virtual void UpdateToggleGroup(GameObject togglePrefab, GameObject toggleGroup, List<IO.Swagger.Model.ListProjectsResponseData> projects) {
        List<IO.Swagger.Model.IdDesc> idDescs = new List<IO.Swagger.Model.IdDesc>();
        foreach (IO.Swagger.Model.ListProjectsResponseData project in projects) {
            idDescs.Add(new IO.Swagger.Model.IdDesc(id: project.Id, desc: project.Desc));
        }
        UpdateToggleGroup(togglePrefab, toggleGroup, idDescs);
    }

    protected virtual string GetSelectedValue(GameObject toggleGroup) {
        foreach (Toggle toggle in toggleGroup.GetComponentsInChildren<Toggle>()) {
            if (toggle.isOn) {
                return toggle.GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
            }
        }
        throw new Base.ItemNotFoundException("Nothing selected");
    }
}
