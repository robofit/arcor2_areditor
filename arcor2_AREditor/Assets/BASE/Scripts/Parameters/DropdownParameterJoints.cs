using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using System.Linq;

public class DropdownParameterJoints : DropdownParameter
{

    public Sprite ValidIcon, InvalidIcon;
    private string apName;

    public void PutData(List<IO.Swagger.Model.ProjectRobotJoints> robotJoints, string selectedItem, UnityAction callback, string apName = null) {
        this.apName = apName;
        List<CustomDropdown.Item> items = new List<CustomDropdown.Item>();
        foreach (IO.Swagger.Model.ProjectRobotJoints joints in robotJoints) {
            CustomDropdown.Item item = new CustomDropdown.Item {
                itemName = joints.Name
            };
            if (joints.IsValid) {
                item.itemIcon = ValidIcon;
                
            } else {
                item.itemIcon = InvalidIcon;
            }
            items.Add(item);
            
        }
        PutData(items, selectedItem, callback);
    }

    public override object GetValue() {
        string value = (string) base.GetValue();
        if (value == null)
            return null;

        if (string.IsNullOrEmpty(apName))
            apName = value.Split('.').First();
        Base.ActionPoint actionPoint = Base.ProjectManager.Instance.GetactionpointByName(apName);
        return actionPoint.GetJointsByName(value.Split('.').Last()).Id;
    }
}
