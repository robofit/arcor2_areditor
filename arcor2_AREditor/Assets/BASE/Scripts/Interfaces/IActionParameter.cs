
using UnityEngine;

public interface IParameter
{
    void SetLabel(string label, string description);

    string GetName();

    object GetValue();

    void SetValue(object value);

    void SetDarkMode(bool dark);

    string GetCurrentType();

    Transform GetTransform();

    void SetInteractable(bool interactable);

}
