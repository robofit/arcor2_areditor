using System.Collections.Generic;

public class ActionMetadata {
    string _name;
    bool _blocking, _free, _composite, _blackbox;

    Dictionary<string, ActionParameterMetadata> _parameters = new Dictionary<string, ActionParameterMetadata>();

    public ActionMetadata() {

    }

    public ActionMetadata(string name, bool blocking, bool free, bool composite, bool blackbox) {
        _name = name;
        _blocking = blocking;
        _free = free;
        _composite = composite;
        _blackbox = blackbox;
    }

    public ActionMetadata(string name, bool blocking, bool free, bool composite, bool blackbox, Dictionary<string, ActionParameterMetadata> parameters) {
        _name = name;
        _blocking = blocking;
        _free = free;
        _composite = composite;
        _blackbox = blackbox;
        Parameters = parameters;
    }

    public bool Blocking {
        get => _blocking; set => _blocking = value;
    }
    public bool Free {
        get => _free; set => _free = value;
    }
    public bool Composite {
        get => _composite; set => _composite = value;
    }
    public bool Blackbox {
        get => _blackbox; set => _blackbox = value;
    }
    public string Name {
        get => _name; set => _name = value;
    }
    public Dictionary<string, ActionParameterMetadata> Parameters {
        get => _parameters; set => _parameters = value;
    }
}
