using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Michsky.UI.ModernUIPack;
using System.Linq;

public class DropdownParameterJoints : DropdownParameter
{

    public Sprite ValidIcon, InvalidIcon;
    public void PutData(List<IO.Swagger.Model.ProjectRobotJoints> robotJoints, string selectedItem, UnityAction callback) {
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
}
