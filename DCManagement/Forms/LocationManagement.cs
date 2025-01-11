using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

namespace DCManagement.Forms {
    public partial class LocationManagement : Form {
        private enum ActionState {
            None,
            Moving,
            Drawing,
            NamingNew,
            Renaming,
            Resizing
        }
        private enum ActionAllowed {
            None,
            Moving,
            Drawing,
            Resizing
        }
        #region Fields
    
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
            InitializeComponent();
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
            using SqlConnection conn = new(Program.SqlConnectionString);
            switch (_actionState) {
                case ActionState.NamingNew:
                case ActionState.Drawing:
                    if (_pendingLocation is null)
                        return;
                    using (SqlCommand cmd = new()) {
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "dbo.InsertLocation";
                        cmd.Parameters.AddRange(_pendingLocation.GetSqlParameters());
                        _pendingLocation.LocID = (int)cmd.ExecuteScalar();
                        _floorplan.AddLocation(_pendingLocation);
                        _pendingLocation = null;
                    }
                    break;
                case ActionState.Moving:
                case ActionState.Renaming:
                    if (_lastClickLocation is null)
                        return;
                    using (SqlCommand cmd = new()) {
                        cmd.Connection = conn;
                        conn.Open();
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "dbo.UpdateLocation";
                        cmd.Parameters.AddRange(_lastClickLocation.GetSqlParameters(true));
                        _ = cmd.ExecuteNonQuery();
                        _floorplan.UpdateLocation(_lastClickLocation);
                        _lastClickLocation = null;
                    }
                    break;
                default:
                    break;
            }
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
            if (e.Button == MouseButtons.Right || BackgroundImage is null)
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
                    _floorplan.ImageMoving = (Image)_floorplan.ImageWithLocations!.Clone();
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
                    _firstPoint = _lastClick;
                    _actionState = ActionState.Resizing;
                    Refresh();
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
            Rectangle rect = new();
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
                    _pendingLocation = new(rect);
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
                    if (_actionAllowed != ActionAllowed.Drawing || _firstPoint is null || _lastClickLocation is null) {
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
            var onEdge = _floorplan.IsPointOnPerimeter(adjustedCoord);
            switch (onEdge) {
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
                    if (_firstPoint is null || _lastClickLocation is not null) {
                        CancelActionStates();
                        return;
                    }
                    second = _floorplan.TransformCoordinatesInv(e.Location);
                    first = (Point) _firstPoint;
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
            _lastClickLocation = loc;
        }
        private void DeleteLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_lastClickLocation is null)
                return;
            using SqlConnection conn = new(Program.SqlConnectionString);;
            using SqlCommand cmd = new();
            cmd.Connection = conn;
            conn.Open();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.DeleteLocation";
            cmd.Parameters.AddRange(_lastClickLocation.GetSqlParameters(true));
            var result = cmd.ExecuteScalar().ToString();
            if (result == "0")
                _floorplan.RemoveLocation(_lastClickLocation);
            else if (result == "Cascade")
                AlertMessagesTSMI.Text = "Cannot delete a location while teams are assigned to that location!";
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
            _floorplan.AddLocations(LocationCollection.GetLocations());
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
            using SqlConnection conn = new(Program.SqlConnectionString);;
            using SqlCommand cmd = new();
            using FileStream file = File.OpenRead(picker.FileName);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.UpdateFloorplan";
            cmd.Connection = conn;
            conn.Open();
            SqlParameter dataParam = new() {
                Direction = ParameterDirection.Input,
                ParameterName = "@data",
                Size = -1,
                SqlDbType = SqlDbType.VarBinary,
                Value = file
            };
            cmd.Parameters.Add(dataParam);
            cmd.ExecuteNonQuery();
            LoadFloorplan();
        }
        #endregion
    }
}
