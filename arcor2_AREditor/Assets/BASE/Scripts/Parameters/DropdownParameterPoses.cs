using System.Linq;
using UnityEngine;


public class DropdownParameterPoses : DropdownParameter
{
    public override object GetValue() {
        string value = (string) base.GetValue();
        if (value == null)
            return null;

        Base.ActionPoint actionPoint = Base.Scene.Instance.GetactionpointByName(value.Split('.').First());
        return actionPoint.GetNamedOrientationByName(value.Split('.').Last()).Id;
    }

}
