namespace DCManagement.Classes; 
internal class StateString {
    private string _state = "";
    public string Text {
        get { return _state; }
        set {
            if (_state == value) 
                return;
            string oldstate = _state;
            string newstate = value;
            _state = value; 
            OnStateChanged(new StateStringEventArgs(oldstate, newstate));
        }
    }
    public event EventHandler<StateStringEventArgs> StateChanged = delegate { };
    public StateString() { }
    public StateString(string state) {
        _state = state;
    }
    private void OnStateChanged(StateStringEventArgs e) {
        StateChanged?.Invoke(this, e);
    }
    public override string ToString() => _state;
}
internal class StateStringEventArgs(string laststring, string newstring) : EventArgs {
    public string LastText { get; init; } = laststring;
    public string NewText { get; init; } = newstring;
}
