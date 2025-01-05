﻿using DCManagement.Classes;
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
            Renaming
        }
        private enum ActionAllowed {
            None,
            Moving,
            Drawing
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
        #endregion
        public LocationManagement() {
            InitializeComponent();
            _floorplan = new Floorplan() {
                Locations = LocationCollection.GetLocations(),
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
                    Program.OpenConn();
                    using (SqlCommand cmd = new()) {
                        cmd.Connection = Program.conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "dbo.InsertLocation";
                        cmd.Parameters.AddRange(_pendingLocation.GetSqlParameters());
                        _pendingLocation.LocID = (int)cmd.ExecuteScalar();
                        _floorplan.Locations.Add(_pendingLocation);
                        _pendingLocation = null;
                    }
                    break;
                case ActionState.Moving:
                case ActionState.Renaming:
                    if (_lastClickLocation is null)
                        return;
                    Program.OpenConn();
                    using (SqlCommand cmd = new()) {
                        cmd.Connection = Program.conn;
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "dbo.UpdateLocation";
                        cmd.Parameters.AddRange(_lastClickLocation.GetSqlParameters(true));
                        _ = cmd.ExecuteNonQuery();
                        _floorplan.Locations[_lastClickLocation.LocID] = _lastClickLocation;
                        _lastClickLocation = null;
                    }
                    break;
                default:
                    break;
            }
            _floorplan.DrawLocations();
        }
        #endregion
        private void CancelActionStates(bool keepFields = false) {
            _actionAllowed = ActionAllowed.None;
            _actionState = ActionState.None;
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
            _floorplan.ClientSize = this.ClientSize;
        }
        #endregion
        #region Form Event Handlers
        private void LocationManagement_Load(object sender, EventArgs e) {
            LoadFloorplan();
            _state.StateChanged += State_TextChanged;
            CancelActionStates();
        }
        private void LocationManagement_FormClosing(object sender, FormClosingEventArgs e) {
            Program.conn.Close();
            Program.conn.Dispose();
        }
        private void LocationManagement_MouseDown(object sender, MouseEventArgs e) {
            _lastClick = _floorplan.AdjustPointForScaling(e.Location);
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
                    _lastClickLocation = _floorplan.Locations.FindByPoint(_lastClick);
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
                    if (_floorplan.Locations.FindByPoint(_lastClick) is not null) {
                        AlertMessagesTSMI.Text = "Cannot create a new location inside a current location!";
                        _actionState = ActionState.Drawing;
                        return;
                    }
                    _firstPoint = _lastClick;
                    _actionState = ActionState.Drawing;
                    Refresh();
                    break;
                default:
                    break;
            }
        }
        private void LocationManagement_MouseUp(object sender, MouseEventArgs e) {
            Point click = _floorplan.AdjustPointForScaling(e.Location);
            Debug.WriteLine($"MouseUp @ {e.X},{e.Y}, Adjusted {click.X},{click.Y}; Button: {e.Button}, " +
                $"ActionAllowed: {_actionAllowed}, ActionState: {_actionState}, " +
                $"PendingLocation: {(_pendingLocation is null ? "IsNull" : "IsNotNull")} " +
                $"BackgroundImage: {(BackgroundImage is null ? "IsNull" : "IsNotNull")}");
            if (e.Button == MouseButtons.Right || BackgroundImage is null)
                return;
            if (_actionState == ActionState.None)
                return;
            switch (_actionState) {
                case ActionState.Moving:
                    if (_actionAllowed != ActionAllowed.Moving || _lastClickLocation is null) {
                        CancelActionStates();
                        return;
                    }
                    Rectangle proposedRect = new() {
                        Location = click,
                        Size = _lastClickLocation.Size
                    };
                    if (proposedRect.Right > BackgroundImage.Width || proposedRect.Bottom > BackgroundImage.Height) {
                        _actionState = ActionState.None;
                        _lastClickLocation = null;
                        BackgroundImage = _floorplan.ImageWithLocations;
                        AlertMessagesTSMI.Text = "Locations cannot extend out of bounds";
                        return;
                    }
                    if (_floorplan.Locations.IntersectsWithAny(proposedRect) && _floorplan.Locations.IntersectWithByID(proposedRect) != _lastClickLocation.LocID) {
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
                    Point first = (Point)_firstPoint;
                    Point second = click;
                    Rectangle rect = new() {
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
                    NameEditTextbox.Location = _floorplan.AdjustPointforScalingInverse(new(_pendingLocation.CenterLeft.X - 10, _pendingLocation.CenterLeft.Y));
                    NameEditTextbox.Text = "";
                    NameEditTextbox.Visible = true;
                    NameEditTextbox.BringToFront();
                    _state.Text = $"Naming new location";
                    NameEditTextbox.Focus();
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
            Rectangle rect;
            switch (_actionState) {
                case ActionState.Moving:
                    if (_lastClickLocation is null) {
                        CancelActionStates();
                        return;
                    }
                    Point current = _floorplan.AdjustPointForScaling(e.Location);
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
                    Point second = _floorplan.AdjustPointForScaling(e.Location);
                    Location? loc = _floorplan.Locations.FindByPoint(second);
                    if (loc is not null) {
                        TeamTooltip.Active = true;
                        TeamTooltip.ToolTipTitle = loc.Name;
                    }
                    Point first = (Point)_firstPoint;
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
            Location? loc = _floorplan.Locations.FindByPoint(_lastClick);
            if (loc is null) {
                e.Cancel = true;
                return;
            }
            _lastClickLocation = loc;
        }
        private void DeleteLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_lastClickLocation is null)
                return;
            Program.OpenConn();
            using SqlCommand cmd = new();
            cmd.Connection = Program.conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.DeleteLocation";
            cmd.Parameters.AddRange(_lastClickLocation.GetSqlParameters(true));
            var result = cmd.ExecuteScalar().ToString();
            if (result == "0")
                _floorplan.Locations.Remove(_lastClickLocation);
            else if (result == "Cascade")
                AlertMessagesTSMI.Text = "Cannot delete a location while teams are assigned to that location!";
            CancelActionStates();
            _floorplan.DrawLocations();
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
            _floorplan.Locations = LocationCollection.GetLocations();
            _floorplan.DrawLocations();
            CancelActionStates();
        }
        private void RenameLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_lastClickLocation is null)
                return;
            _pendingLocation = null;
            CancelActionStates(true);
            CancelPendingActionToolStripMenuItem.Visible = true;
            _actionState = ActionState.Renaming;
            NameEditTextbox.Location = _floorplan.AdjustPointforScalingInverse(new(_lastClickLocation.CenterLeft.X - 10, _lastClickLocation.CenterLeft.Y));
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
            Program.OpenConn();
            using SqlCommand cmd = new();
            using FileStream file = File.OpenRead(picker.FileName);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.UpdateFloorplan";
            cmd.Connection = Program.conn;
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
