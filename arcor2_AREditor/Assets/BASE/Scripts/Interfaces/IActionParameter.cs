
public interface IActionParameter
{
    void SetLabel(string label, string description);

    string GetName();
    object GetValue();

}
