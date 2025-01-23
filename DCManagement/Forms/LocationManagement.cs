using DCManagement.Classes;
using System.ComponentModel;
using System.Diagnostics;

namespace DCManagement.Forms;
public partial class LocationManagement : Form {
    private enum ActionState {
        None,
        Moving,
        Drawing,
        NamingNew,
        Renaming,
        Resizing,
        ClinicalToggle
    }
    private enum ActionAllowed {
        None,
        Moving,
        Drawing,
        Resizing
    }
    #region Fields
    private readonly DataManagement _data;
    private ActionState _actionState;
    private ActionAllowed _actionAllowed;
    private Point _lastClick;
    private Point? _firstPoint;
    private Location? _pendingLocation;
    private Location? _lastClickLocation;
    private readonly Floorplan _floorplan;
    private readonly StateString _state = new();
    private readonly Size _maxSize;
    private Floorplan.Edge _edge = Floorplan.Edge.None;
    #endregion
    public LocationManagement() {
        SetStyle(ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        InitializeComponent();
        _data = new(Program.Source);
        _floorplan = new Floorplan() {
            Client = this
        };
        _maxSize = new() {
            Height = ClientSize.Height,
            Width = ClientSize.Width
        };
        ResizeForm();
    }
    #region Internal Helper Functions
    #region FloorPlan Wrappers
    private void LoadFloorplan() {
        _floorplan.LoadFloorplan();
        ResizeForm();
        BackgroundImage = _floorplan.ImageWithLocations;
    }
    #endregion
    #region Database Interaction
    private void WriteNewLocation() {
        switch (_actionState) {
            case ActionState.NamingNew:
            case ActionState.Drawing:
                if (_pendingLocation is null)
                    return;
                _data.InsertLocation(_pendingLocation);
                _floorplan.AddLocation(_pendingLocation);
                _pendingLocation = null;
                break;
            case ActionState.Moving:
            case ActionState.Renaming:
            case ActionState.Resizing:
            case ActionState.ClinicalToggle:
                if (_lastClickLocation is null)
                    return;
                _data.UpdateLocation(_lastClickLocation);
                _floorplan.UpdateLocation(_lastClickLocation);
                _lastClickLocation = null;
                break;
            default:
                break;
        }
        _actionAllowed = ActionAllowed.None;
        _actionState = ActionState.None;
        _floorplan.ImageWithLocations = _floorplan.DrawLocations();
        BackgroundImage = _floorplan.ImageWithLocations;
    }
    #endregion
    private void CancelActionStates(bool keepFields = false) {
        _actionAllowed = ActionAllowed.None;
        _actionState = ActionState.None;
        Cursor.Current = Cursors.Default;
        _state.Text = "";
        NameEditTextbox.Text = "";
        NameEditTextbox.Visible = false;
        BackgroundImage = _floorplan.ImageWithLocations;
        if (!keepFields) {
            _lastClick = new();
            _lastClickLocation = null;
            _pendingLocation = null;
            CancelPendingActionToolStripMenuItem.Visible = false;
        }
    }
    private void ResizeForm() {
        Size imageSize = _floorplan.ImageSize;
        float aspectRatio = (float)imageSize.Width / (float)imageSize.Height;
        if (imageSize.Width <= _maxSize.Width && imageSize.Height <= _maxSize.Height) {
            Size = imageSize;
            BackgroundImageLayout = ImageLayout.Center;
        } else {
            if (aspectRatio >= 1) {
                Size = new Size() {
                    Width = _maxSize.Width,
                    Height = (int)(_maxSize.Width / aspectRatio)
                };
            } else {
                Size = new Size() {
                    Width = (int)(_maxSize.Height * aspectRatio),
                    Height = _maxSize.Height
                };
            }
            BackgroundImageLayout = ImageLayout.Stretch;
        }
    }
    #endregion
    #region Form Event Handlers
    private void LocationManagement_Load(object sender, EventArgs e) {
        LoadFloorplan();
        _state.StateChanged += State_TextChanged;
        CancelActionStates();
    }
    private void LocationManagement_FormClosing(object sender, FormClosingEventArgs e) { }
    private void LocationManagement_MouseDown(object sender, MouseEventArgs e) {
        _lastClick = _floorplan.TransformCoordinatesInv(e.Location);
        Debug.WriteLine($"MouseDown @ {e.X},{e.Y}, Adjusted {_lastClick.X},{_lastClick.Y}; Button: {e.Button}, " +
            $"ActionAllowed: {_actionAllowed}, ActionState: {_actionState}, " +
            $"PendingLocation: {(_pendingLocation is null ? "IsNull" : "IsNotNull")} " +
            $"BackgroundImage: {(BackgroundImage is null ? "IsNull" : "IsNotNull")}");
        if (e.Button != MouseButtons.Left || BackgroundImage is null)
            return;
        switch (_actionAllowed) {
            case ActionAllowed.Moving:
                if (_actionState == ActionState.Moving)
                    return;
                _lastClickLocation = _floorplan.FindByPoint(_lastClick);
                if (_lastClickLocation is null)
                    return;
                _actionState = ActionState.Moving;
                _state.Text = "Select location to move";
                _floorplan.ImageMoving = _floorplan.DrawLocations(_floorplan.Locations.Except([_lastClickLocation]));
                BackgroundImage = _floorplan.ImageMoving;
                break;
            case ActionAllowed.Drawing:
                if (_pendingLocation is not null) {
                    _pendingLocation = null;
                }
                if (_floorplan.FindByPoint(_lastClick) is not null) {
                    AlertMessagesTSMI.Text = "Cannot create a new location inside a current location!";
                    _actionState = ActionState.Drawing;
                    return;
                }
                _firstPoint = _lastClick;
                _actionState = ActionState.Drawing;
                Refresh();
                break;
            case ActionAllowed.Resizing:
                var onEdge = _floorplan.IsPointOnPerimeter(_lastClick);
                if (onEdge == Floorplan.Edge.None) {
                    Cursor.Current = Cursors.Default;
                    _actionAllowed = ActionAllowed.None;
                    return;
                }
                _lastClickLocation = _floorplan.FindByPoint(_lastClick);
                if (_lastClickLocation is null)
                    return;
                _firstPoint = _lastClick;
                _actionState = ActionState.Resizing;
                _floorplan.ImageMoving = _floorplan.DrawLocations(_floorplan.Locations.Except([_lastClickLocation]));
                BackgroundImage = _floorplan.ImageMoving;
                Refresh();
                MouseEventArgs dummyArgs = new(MouseButtons.Left, 1, e.X, e.Y, 0);
                LocationManagement_MouseMove(this, dummyArgs);
                break;
            default:
                break;
        }
    }
    private void LocationManagement_MouseUp(object sender, MouseEventArgs e) {
        Point click = _floorplan.TransformCoordinatesInv(e.Location);
        Debug.WriteLine($"MouseUp @ {e.X},{e.Y}, Adjusted {click.X},{click.Y}; Button: {e.Button}, " +
            $"ActionAllowed: {_actionAllowed}, ActionState: {_actionState}, " +
            $"PendingLocation: {(_pendingLocation is null ? "IsNull" : "IsNotNull")} " +
            $"BackgroundImage: {(BackgroundImage is null ? "IsNull" : "IsNotNull")}");
        if (e.Button == MouseButtons.Right || BackgroundImage is null)
            return;
        if (_actionState == ActionState.None)
            return;
        Point first;
        Point second;
        Cursor.Current = Cursors.Default;
        Rectangle rect;
        switch (_actionState) {
            case ActionState.Moving:
                if (_actionAllowed != ActionAllowed.Moving || _lastClickLocation is null) {
                    CancelActionStates();
                    return;
                }
                rect = new() {
                    Location = click,
                    Size = _lastClickLocation.Size
                };
                if (rect.Right > BackgroundImage.Width || rect.Bottom > BackgroundImage.Height) {
                    _actionState = ActionState.None;
                    _lastClickLocation = null;
                    BackgroundImage = _floorplan.ImageWithLocations;
                    AlertMessagesTSMI.Text = "Locations cannot extend out of bounds";
                    return;
                }
                if (_floorplan.Locations.IntersectsWithAny(rect) && _floorplan.Locations.IntersectWithByID(rect) != _lastClickLocation.LocID) {
                    _actionState = ActionState.None;
                    _lastClickLocation = null;
                    BackgroundImage = _floorplan.ImageWithLocations;
                    AlertMessagesTSMI.Text = "Locations cannot be moved to overlap with existing locations";
                    return;
                }
                _lastClickLocation.UpperLeft = click;
                WriteNewLocation();
                _actionState = ActionState.None;
                _actionAllowed = ActionAllowed.None;
                _lastClickLocation = null;
                _lastClick = new();
                _state.Text = "";
                break;
            case ActionState.Drawing:
                if (_actionAllowed != ActionAllowed.Drawing || _firstPoint is null) {
                    CancelActionStates();
                    return;
                }
                first = (Point)_firstPoint;
                second = click;
                rect = new() {
                    Location = new() {
                        X = Math.Min(first.X, second.X),
                        Y = Math.Min(first.Y, second.Y)
                    },
                    Size = new() {
                        Width = Math.Abs(first.X - second.X),
                        Height = Math.Abs(first.Y - second.Y)
                    }
                };
                _pendingLocation = new(rect) {
                    Clinical = true
                };
                if (_floorplan.Locations.IntersectsWithAny(_pendingLocation)) {
                    _pendingLocation = null;
                    _actionState = ActionState.None;
                    _firstPoint = null;
                    AlertMessagesTSMI.Text = "A new location may not overlap with existing locations.";
                    return;
                }
                BackgroundImage = _floorplan.DrawNewRectangle(rect);
                _actionState = ActionState.NamingNew;
                NameEditTextbox.Location = _floorplan.TransformCoordinates(new(_pendingLocation.CenterLeft.X - 10, _pendingLocation.CenterLeft.Y));
                NameEditTextbox.Text = "";
                NameEditTextbox.Visible = true;
                NameEditTextbox.BringToFront();
                _state.Text = $"Naming new location";
                NameEditTextbox.Focus();
                break;
            case ActionState.Resizing:
                if (_actionAllowed != ActionAllowed.Resizing || _firstPoint is null || _lastClickLocation is null) {
                    CancelActionStates();
                    return;
                }
                rect = _lastClickLocation.Rect;
                second = _floorplan.TransformCoordinatesInv(e.Location);
                Point upperLeft = rect.Location;
                Point lowerRight = new(rect.X + rect.Width, rect.Y + rect.Height);
                switch (_edge) {
                    case Floorplan.Edge.None:
                        break;
                    case Floorplan.Edge.Left:
                        upperLeft.X = second.X;
                        break;
                    case Floorplan.Edge.Top:
                        upperLeft.Y = second.Y;
                        break;
                    case Floorplan.Edge.Right:
                        lowerRight.X = second.X;
                        break;
                    case Floorplan.Edge.Bottom:
                        lowerRight.Y = second.Y;
                        break;
                    case Floorplan.Edge.TopRight:
                        upperLeft.Y = second.Y;
                        lowerRight.X = second.X;
                        break;
                    case Floorplan.Edge.BottomRight:
                        lowerRight = second;
                        break;
                    case Floorplan.Edge.TopLeft:
                        upperLeft = second;
                        break;
                    case Floorplan.Edge.BottomLeft:
                        upperLeft.X = second.X;
                        lowerRight.Y = second.Y;
                        break;
                    default:
                        break;
                }
                rect = new() {
                    Location = new() {
                        X = Math.Min(upperLeft.X, lowerRight.X),
                        Y = Math.Min(upperLeft.Y, lowerRight.Y)
                    },
                    Size = new() {
                        Width = Math.Abs(upperLeft.X - lowerRight.X),
                        Height = Math.Abs(upperLeft.Y - lowerRight.Y)
                    }
                };
                if (
                            rect.Top < 0 ||
                            rect.Left < 0 ||
                            rect.Right > BackgroundImage.Width ||
                            rect.Bottom > BackgroundImage.Height ||
                        (
                            _floorplan.Locations.IntersectsWithAny(rect) &&
                            _floorplan.Locations.IntersectWithByID(rect) != _lastClickLocation.LocID
                        )
                    ) {
                    _pendingLocation = null;
                    _lastClickLocation = null;
                    _actionAllowed = ActionAllowed.None;
                    Cursor.Current = Cursors.Default;
                    _actionState = ActionState.None;
                    _firstPoint = null;
                    AlertMessagesTSMI.Text = "A new location may not overlap with existing locations.";
                    return;
                }
                _lastClickLocation.UpperLeft = rect.Location;
                _lastClickLocation.Size = rect.Size;
                _pendingLocation = null;
                WriteNewLocation();
                _lastClickLocation = null;
                _lastClick = new();
                _state.Text = "";
                _actionAllowed = ActionAllowed.None;
                Cursor.Current = Cursors.Default;
                _actionState = ActionState.None;
                _firstPoint = null;
                break;
            default:
                break;
        }
        CancelPendingActionToolStripMenuItem.Visible = false;
    }
    private void LocationManagement_MouseMove(object sender, MouseEventArgs e) {
        MouseCoordTSMI.Text = $"{e.X},{e.Y}";
        if (BackgroundImage is null)
            return;
        Point adjustedCoord = _floorplan.TransformCoordinatesInv(e.Location);
        if (_actionState == ActionState.None && (_actionAllowed == ActionAllowed.Resizing || _actionAllowed == ActionAllowed.None)) {
            _edge = _floorplan.IsPointOnPerimeter(adjustedCoord);
            switch (_edge) {
                case Floorplan.Edge.None:
                    if (_actionState == ActionState.None && _actionAllowed == ActionAllowed.None)
                        Cursor.Current = Cursors.Default;
                    if (_actionAllowed == ActionAllowed.Resizing && _actionState == ActionState.None) {
                        _actionAllowed = ActionAllowed.None;
                        Cursor.Current = Cursors.Default;
                        _pendingLocation = null;
                    }
                    break;
                case Floorplan.Edge.Left:
                case Floorplan.Edge.Right:
                    Cursor.Current = Cursors.SizeWE;
                    _actionAllowed = ActionAllowed.Resizing;
                    _pendingLocation = _floorplan.FindByPoint(adjustedCoord);
                    break;
                case Floorplan.Edge.Top:
                case Floorplan.Edge.Bottom:
                    Cursor.Current = Cursors.SizeNS;
                    _actionAllowed = ActionAllowed.Resizing;
                    _pendingLocation = _floorplan.FindByPoint(adjustedCoord);
                    break;
                case Floorplan.Edge.BottomLeft:
                case Floorplan.Edge.TopRight:
                    Cursor.Current = Cursors.SizeNESW;
                    _actionAllowed = ActionAllowed.Resizing;
                    _pendingLocation = _floorplan.FindByPoint(adjustedCoord);
                    break;
                case Floorplan.Edge.BottomRight:
                case Floorplan.Edge.TopLeft:
                    Cursor.Current = Cursors.SizeNWSE;
                    _actionAllowed = ActionAllowed.Resizing;
                    _pendingLocation = _floorplan.FindByPoint(adjustedCoord);
                    break;
                default:
                    break;
            }
        }
        Rectangle rect;
        Point first;
        Point second;
        switch (_actionState) {
            case ActionState.Moving:
                if (_lastClickLocation is null) {
                    CancelActionStates();
                    return;
                }
                Point current = _floorplan.TransformCoordinatesInv(e.Location);
                rect = new() {
                    Location = current,
                    Size = _lastClickLocation.Size
                };
                BackgroundImage = _floorplan.DrawMovingRectangle(rect);
                break;
            case ActionState.Drawing:
                if (_firstPoint is null || _pendingLocation is not null) {
                    CancelActionStates();
                    return;
                }
                second = _floorplan.TransformCoordinatesInv(e.Location);
                Location? loc = _floorplan.FindByPoint(second);
                if (loc is not null) {
                    TeamTooltip.Active = true;
                    TeamTooltip.ToolTipTitle = loc.Name;
                }
                first = (Point)_firstPoint;
                rect = new() {
                    Location = new() {
                        X = Math.Min(first.X, second.X),
                        Y = Math.Min(first.Y, second.Y)
                    },
                    Size = new() {
                        Width = Math.Abs(first.X - second.X),
                        Height = Math.Abs(first.Y - second.Y)
                    }
                };
                Debug.WriteLine($"RectX: {rect.Location.X}, RectY{rect.Location.Y}, RectWidth{rect.Size.Width}, RectHeight{rect.Size.Height}, RectLeft{rect.Left}, RectTop{rect.Top}, RectRight{rect.Right}, RectBottom{rect.Bottom}");
                BackgroundImage = _floorplan.DrawNewRectangle(rect);
                break;
            case ActionState.Resizing:
                if (_firstPoint is null || _lastClickLocation is null) {
                    CancelActionStates();
                    return;
                }
                rect = _lastClickLocation.Rect;
                second = _floorplan.TransformCoordinatesInv(e.Location);
                Point upperLeft = rect.Location;
                Point lowerRight = new(rect.X + rect.Width, rect.Y + rect.Height);
                switch (_edge) {
                    case Floorplan.Edge.None:
                        break;
                    case Floorplan.Edge.Left:
                        upperLeft.X = second.X;
                        break;
                    case Floorplan.Edge.Top:
                        upperLeft.Y = second.Y;
                        break;
                    case Floorplan.Edge.Right:
                        lowerRight.X = second.X;
                        break;
                    case Floorplan.Edge.Bottom:
                        lowerRight.Y = second.Y;
                        break;
                    case Floorplan.Edge.TopRight:
                        upperLeft.Y = second.Y;
                        lowerRight.X = second.X;
                        break;
                    case Floorplan.Edge.BottomRight:
                        lowerRight = second;
                        break;
                    case Floorplan.Edge.TopLeft:
                        upperLeft = second;
                        break;
                    case Floorplan.Edge.BottomLeft:
                        upperLeft.X = second.X;
                        lowerRight.Y = second.Y;
                        break;
                    default:
                        break;
                }
                rect = new() {
                    Location = new() {
                        X = Math.Min(upperLeft.X, lowerRight.X),
                        Y = Math.Min(upperLeft.Y, lowerRight.Y)
                    },
                    Size = new() {
                        Width = Math.Abs(upperLeft.X - lowerRight.X),
                        Height = Math.Abs(upperLeft.Y - lowerRight.Y)
                    }
                };

                BackgroundImage = _floorplan.DrawMovingRectangle(rect);
                break;
            default:
                return;
        }
    }
    #endregion
    #region Class Event Handlers
    private void State_TextChanged(object? sender, StateStringEventArgs e) {
        if (AlertMessagesTSMI.Text == e.LastText || AlertMessagesTSMI.Text == e.NewText) {
            AlertMessagesTSMI.Text = e.NewText;
            AlertMessagesTSMI.ForeColor = SystemColors.ControlText;
            AlertMessageTimer.Enabled = false;
            AlertMessageTimer.Stop();
        }
    }
    #endregion
    #region Control Event Handlers
    private void AlertMessageTimer_Tick(object sender, EventArgs e) {
        if (AlertMessagesTSMI.Text == "" || AlertMessagesTSMI.Text == _state.Text)
            return;
        AlertMessagesTSMI.Text = _state.Text;
    }
    private void AlertMessagesTSMI_TextChanged(object sender, EventArgs e) {
        if (AlertMessagesTSMI.Text == _state.Text) {
            AlertMessagesTSMI.ForeColor = SystemColors.ControlText;
            AlertMessageTimer.Enabled = false;
            AlertMessageTimer.Stop();
        } else if (AlertMessagesTSMI.Text == "") {
            AlertMessagesTSMI.Text = _state.Text;
            AlertMessagesTSMI.ForeColor = SystemColors.ControlText;
            AlertMessageTimer.Enabled = false;
            AlertMessageTimer.Stop();
        } else {
            AlertMessagesTSMI.ForeColor = Color.Red;
            AlertMessageTimer.Enabled = true;
            AlertMessageTimer.Start();
        }
    }
    private void CancelPendingActionToolStripMenuItem_Click(object sender, EventArgs e) {
        CancelActionStates();
    }
    private void ContextMenu_Opening(object sender, CancelEventArgs e) {
        Location? loc = _floorplan.FindByPoint(_lastClick);
        if (loc is null) {
            e.Cancel = true;
            return;
        }
        if (loc.Clinical)
            ToggleClinicalToolStripMenuItem.Text = "Mark As Admin";
        else
            ToggleClinicalToolStripMenuItem.Text = "Mark As Clinical";
        _lastClickLocation = loc;
    }
    private void DeleteLocationToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_lastClickLocation is null)
            return;
        var ack = MessageBox.Show("This will also delete the associated team. All personnel assigned to associated team will become unassigned.", "Warning!", MessageBoxButtons.OKCancel);
        if (ack != DialogResult.OK)
            return;
        _data.DeleteLocation(_lastClickLocation);
        _floorplan.RemoveLocation(_lastClickLocation);
        CancelActionStates();
        _floorplan.ImageWithLocations = _floorplan.DrawLocations();
        BackgroundImage = _floorplan.ImageWithLocations;
    }
    private void DrawNewLocationToolStripMenuItem_Click(object sender, EventArgs e) {
        CancelActionStates();
        CancelPendingActionToolStripMenuItem.Visible = true;
        _actionAllowed = ActionAllowed.Drawing;
        _state.Text = "Drawing new location";
    }
    private void MoveLocationToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_lastClickLocation is null)
            return;
        CancelActionStates(true);
        _actionAllowed = ActionAllowed.Moving;
        _actionState = ActionState.Moving;
        CancelPendingActionToolStripMenuItem.Visible = true;
        _state.Text = $"Moving Location: {_lastClickLocation.Name}";
        LocationCollection unaffected = _floorplan.Locations.Clone();
        unaffected.Remove(_lastClickLocation);
        _floorplan.SetMoving(unaffected);
        BackgroundImage = _floorplan.ImageMoving;
    }
    private void NameEditTextbox_KeyUp(object sender, KeyEventArgs e) {
        if (e.KeyCode == Keys.Escape)
            CancelActionStates();
        if (e.KeyCode != Keys.Enter)
            return;
        switch (_actionState) {
            case ActionState.NamingNew:
                if (_pendingLocation is null) {
                    CancelActionStates();
                    return;
                }
                _pendingLocation.Name = NameEditTextbox.Text;
                break;
            case ActionState.Renaming:
                if (_lastClickLocation is null) {
                    CancelActionStates();
                    return;
                }
                _lastClickLocation.Name = NameEditTextbox.Text;
                break;
            default:
                CancelActionStates();
                return;
        }
        WriteNewLocation();
        _floorplan.AddLocations(_data.GetLocCollection());
        _floorplan.ImageWithLocations = _floorplan.DrawLocations();
        BackgroundImage = _floorplan.ImageWithLocations;
        CancelActionStates();
    }
    private void RenameLocationToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_lastClickLocation is null)
            return;
        _pendingLocation = null;
        CancelActionStates(true);
        CancelPendingActionToolStripMenuItem.Visible = true;
        _actionState = ActionState.Renaming;
        NameEditTextbox.Location = _floorplan.TransformCoordinates(new(_lastClickLocation.CenterLeft.X - 10, _lastClickLocation.CenterLeft.Y));
        NameEditTextbox.Text = "";
        NameEditTextbox.Visible = true;
        NameEditTextbox.BringToFront();
        _state.Text = $"Renaming location: {_lastClickLocation.Name}";
        Debug.WriteLine($"Create pending loc name box @ {NameEditTextbox.Location.X},{NameEditTextbox.Location.Y}");
        NameEditTextbox.Focus();
        return;
    }
    private void SetClinicFloorplanToolStripMenuItem_Click(object sender, EventArgs e) {
        var picker = new OpenFileDialog() {
            AutoUpgradeEnabled = true,
            CheckFileExists = true,
            DereferenceLinks = true,
            Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG",
            Multiselect = false
        };
        if (picker.ShowDialog() != DialogResult.OK || !File.Exists(picker.FileName))
            return;
        _data.WriteFloorplan(picker.FileName);
        LoadFloorplan();
    }
    private void ToggleClinicalToolStripMenuItem_Click(object sender, EventArgs e) {
        if (_lastClickLocation is null)
            return;
        _pendingLocation = null;
        CancelActionStates(true);
        CancelPendingActionToolStripMenuItem.Visible = false;
        _lastClickLocation.Clinical = !_lastClickLocation.Clinical;
        _actionState = ActionState.ClinicalToggle;
        WriteNewLocation();
    }
    #endregion
}
