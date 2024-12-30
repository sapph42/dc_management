using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

namespace DCManagement.Forms {
    public partial class LocationManagement : Form {
        private readonly SqlConnection conn;
        private bool _drawing = false;
        private bool _moving = false;
        private Point _lastClick;
        private Point? _firstPoint;
        private Location? _pendingLocation;
        private Location? _lastClickLocation;
        private LocationCollection _locationCollection = [];
        private Image _floorPlan = new Bitmap(1, 1);
        private Image _floorPlanWithLocations = new Bitmap(1, 1);
        private Image _floorPlanMoving = new Bitmap(1, 1);
        public LocationManagement() {
            conn = new(Program.SqlConnectionString);
            InitializeComponent();
        }
        private void ConnOpen() {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }
        #region Internal Helper Functions
        #region Database Interaction
        private void LoadFloorplan() {
            ConnOpen();
            using SqlCommand cmd = new();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT TOP (1) Image FROM Floorplan";
            cmd.Connection = conn;
            Image? floorplan = null;
            using SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                if (!reader.IsDBNull(0))
                    using (MemoryStream stream = new()) {
                        using Stream data = reader.GetStream(0);
                        data.CopyTo(stream);
                        floorplan = Image.FromStream(stream);
                        _floorPlan = (Image)floorplan.Clone();
                        _floorPlanWithLocations = (Image)floorplan.Clone();
                    }
            if (floorplan is null)
                return;
            reader.Close();
            BackgroundImage = floorplan;
            BackgroundImageLayout = ImageLayout.Stretch;
            GetLocations();
            DrawLocations();
        }
        private void GetLocations() {
            ConnOpen();
            using SqlCommand cmd = new();
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location";
            cmd.Connection = conn;
            _locationCollection = [];
            using SqlDataReader reader = cmd.ExecuteReader();
            object[] row = new object[6];
            while (reader.Read()) {
                _ = reader.GetValues(row);
                _locationCollection.Add(new Location(row));
            }
            reader.Close();
        }
        private void WriteNewLocation() {
            if (_pendingLocation is null && _lastClickLocation is null) return;
            if (_pendingLocation is null && _lastClickLocation is not null) {
                ConnOpen();
                using SqlCommand cmd = new();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.UpdateLocation";
                cmd.Parameters.AddRange(_lastClickLocation.GetSqlParameters(true));
                _ = cmd.ExecuteNonQuery();
                _locationCollection[_lastClickLocation.LocID] = _lastClickLocation;
                _lastClickLocation = null;
            } else if (_pendingLocation is not null && _lastClickLocation is null) {
                ConnOpen();
                using SqlCommand cmd = new();
                cmd.Connection = conn;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "dbo.InsertLocation";
                cmd.Parameters.AddRange(_pendingLocation.GetSqlParameters());
                _pendingLocation.LocID = (int)cmd.ExecuteScalar();
                _locationCollection.Add(_pendingLocation);
                _pendingLocation = null;
            }
            LoadFloorplan();
        }
        #endregion
        #region Drawing
        private void DrawLocations() {
            if (BackgroundImage is null || _locationCollection is null || !_locationCollection.Any())
                return;
            Bitmap image = new(_floorPlan);
            using Graphics graphics = Graphics.FromImage(image);
            using Pen borderPen = new(Color.Black);
            using Pen textPen = new(Color.Black);
            using Brush textBrush = textPen.Brush;
            borderPen.Width = 1f;
            foreach (var location in _locationCollection.Values) {
                graphics.DrawRectangle(borderPen, location.Rect);
                graphics.DrawString(location.Name, new Font("Arial", 10f, FontStyle.Bold), textBrush, location.UpperLeft);
            }
            BackgroundImage = image;
            _floorPlanWithLocations = image;
        }
        private void DrawLocations(LocationCollection usingCollection) {
            if (BackgroundImage is null || usingCollection is null || !usingCollection.Any())
                return;
            Bitmap image = new(_floorPlan);
            using Graphics graphics = Graphics.FromImage(image);
            using Pen borderPen = new(Color.Black);
            using Pen textPen = new(Color.Black);
            using Brush textBrush = textPen.Brush;
            borderPen.Width = 1f;
            foreach (var location in usingCollection.Values) {
                graphics.DrawRectangle(borderPen, location.Rect);
                graphics.DrawString(location.Name, new Font("Arial", 10f, FontStyle.Bold), textBrush, location.UpperLeft);
            }
            BackgroundImage = image;
            _floorPlanMoving = image;
        }
        #endregion
        private Point AdjustPointForScaling(Point BasePoint) {
            if (BackgroundImage is null)
                return BasePoint;
            float scaleX = (float)BackgroundImage.Width / ClientSize.Width;
            float scaleY = (float)BackgroundImage.Height / ClientSize.Height;
            return new Point(
                (int)(BasePoint.X * scaleX),
                (int)(BasePoint.Y * scaleY)
            );
        }

        #endregion
        #region Form Event Handlers
        private void LocationManagement_Load(object sender, EventArgs e) {
            LoadFloorplan();
        }
        private void LocationManagement_FormClosing(object sender, FormClosingEventArgs e) {
            conn.Close();
            conn.Dispose();
        }
        private void LocationManagement_MouseDown(object sender, MouseEventArgs e) {
            _lastClick = AdjustPointForScaling(e.Location);
            if (_moving)
                return;
            if (BackgroundImage is null)
                return;
            if (_pendingLocation is not null) {
                if (_pendingLocation.IsPointInside(_lastClick))
                    return;
                _pendingLocation = null;
                BackgroundImage = _floorPlanWithLocations;
                Refresh();
            }
            _firstPoint = e.Location;
            _drawing = true;
            Debug.WriteLine($"Drawing active @ {e.X},{e.Y}");
        }
        private void LocationManagement_MouseUp(object sender, MouseEventArgs e) {
            Debug.WriteLine($"MouseUp @ {e.X},{e.Y} x ");
            Point click = AdjustPointForScaling(e.Location);
            if (!_moving && BackgroundImage is not null) {
                if (_lastClickLocation is null)
                    return;
                Rectangle proposedRect = new() {
                    Location = click,
                    Size = _lastClickLocation.Size
                };
                if (_locationCollection.IntersectsWithAny(proposedRect)) {
                    _moving = false;
                    BackgroundImage = _floorPlanWithLocations;
                    MessageBox.Show("Locations cannot be moved to overlap with existing locations");
                    return;
                }
                _lastClickLocation.UpperLeft = click;
                WriteNewLocation();
            } else if (!_drawing && BackgroundImage is not null) {
                if (_pendingLocation is null || !_pendingLocation.IsPointInside(click))
                    return;
                TextBox newName = new() {
                    Size = new(200, 20),
                    Location = new(_pendingLocation.CenterLeft.X - 10, _pendingLocation.CenterLeft.Y)
                };
                Debug.WriteLine($"Create pending loc name box @ {newName.Location.X},{newName.Location.Y}");
                SuspendLayout();
                Controls.Add(newName);
                ResumeLayout();
                newName.KeyDown += TextKeyDownHandler;
                newName.Focus();
                return;
            } else if (!_drawing || _firstPoint is null || BackgroundImage is null)
                return;
            else {
                Point first = AdjustPointForScaling((Point)_firstPoint);
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
                Debug.WriteLine($"Create pending loc  @ {first.X},{first.Y} x {rect.Right},{rect.Bottom}");
                _pendingLocation = new(rect);
                if (_locationCollection.IntersectsWithAny(_pendingLocation)) {
                    _pendingLocation = null;
                    MessageBox.Show("A new location may not overlap with existing locations.");
                    _drawing = false;
                    return;
                }
                Bitmap image = new(_floorPlanWithLocations);
                using Graphics graphics = Graphics.FromImage(image);
                using Pen pen = new(Color.Red);
                pen.Width = 2f;
                graphics.DrawRectangle(pen, rect);
                BackgroundImage = image;
                _drawing = false;
            }
        }
        private void LocationManagement_Click(object sender, EventArgs e) {

        }
        private void LocationManagement_MouseMove(object sender, MouseEventArgs e) {
            MouseCoordTSMI.Text = $"{e.X},{e.Y}";
            Bitmap image;
            Rectangle rect;
            Pen pen;
            if (_moving) {
                if (_lastClickLocation is null)
                    return;
                Point current = AdjustPointForScaling(e.Location);
                rect = new() {
                    Location = current,
                    Size = _lastClickLocation.Size
                };
                image = new(_floorPlanMoving);
                using Graphics g = Graphics.FromImage(image);
                pen = new(Color.Black) {
                    Width = 2f
                };
                g.DrawRectangle(pen, rect);
                BackgroundImage = image;
                return;
            }
            Point second = AdjustPointForScaling(e.Location);
            Location? loc = _locationCollection.FindByPoint(second);
            if (loc is not null) {
                TeamTooltip.Active = true;
                TeamTooltip.ToolTipTitle = loc.Name;
            }
            if (!_drawing || BackgroundImage is null || _firstPoint is null)
                return;
            Point first = AdjustPointForScaling((Point)_firstPoint);
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
            image = new(_floorPlanWithLocations);
            using Graphics graphics = Graphics.FromImage(image);
            pen = new(Color.Red) {
                DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot,
                Width = 2f
            };
            graphics.DrawRectangle(pen, rect);
            BackgroundImage = image;
        }
        #endregion
        #region Control Event Handlers
        private void ContextMenu_Opening(object sender, CancelEventArgs e) {
            Location? loc = _locationCollection.FindByPoint(_lastClick);
            if (loc is null) {
                e.Cancel = true;
                return;
            }
            _lastClickLocation = loc;
        }
        private void DeleteLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_lastClickLocation is null)
                return;
            ConnOpen();
            using SqlCommand cmd = new();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.DeleteLocation";
            cmd.Parameters.AddRange(_lastClickLocation.GetSqlParameters(true));
            var result = cmd.ExecuteScalar().ToString();
            if (result == "0")
                _locationCollection.Remove(_lastClickLocation);
            else if (result == "Cascade")
                MessageBox.Show("Cannot delete a location while teams are assigned to that location!");
            _lastClickLocation = null;
            _lastClick = new();
        }
        private void DrawNewLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            _drawing = true;
        }
        private void MoveLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_lastClickLocation is null)
                return;
            _moving = true;
            LocationCollection unaffected = _locationCollection.Clone();
            unaffected.Remove(_lastClickLocation);
            DrawLocations(unaffected);
        }
        private void RenameLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            if (_lastClickLocation is null)
                return;
            _pendingLocation = null;
            TextBox newName = new() {
                Size = new(200, 20),
                Location = new(_lastClickLocation.CenterLeft.X - 10, _lastClickLocation.CenterLeft.Y)
            };
            Debug.WriteLine($"Create pending loc name box @ {newName.Location.X},{newName.Location.Y}");
            SuspendLayout();
            Controls.Add(newName);
            ResumeLayout();
            newName.KeyDown += TextKeyDownHandler;
            newName.Focus();
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
            ConnOpen();
            using SqlCommand cmd = new();
            using FileStream file = File.OpenRead(picker.FileName);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.UpdateFloorplan";
            cmd.Connection = conn;
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
        private void TextKeyDownHandler(object? sender, KeyEventArgs e) {
            if (sender is null)
                return;
            if (sender is not TextBox)
                return;
            if (e.KeyCode != Keys.Enter)
                return;
            if (_pendingLocation is null)
                return;
            TextBox box = (TextBox)sender;
            if (_pendingLocation is not null && _lastClickLocation is null)
                _pendingLocation.Name = box.Text;
            else if (_pendingLocation is null && _lastClickLocation is not null)
                _lastClickLocation.Name = box.Text;
            SuspendLayout();
            Controls.Remove(box);
            box.Dispose();
            ResumeLayout();
            WriteNewLocation();
            LoadFloorplan();
        }
        #endregion
    }
}
