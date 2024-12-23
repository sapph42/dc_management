using DCManagement.Classes;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCManagement.Forms {
    public partial class LocationManagement : Form {
        private readonly SqlConnection conn;
        private bool _drawing = false;
        private Point? _firstPoint;
        private Location? _pendingLocation;
        private LocationCollection _locationCollection = [];
        private Image _floorPlan = new Bitmap(1, 1);
        private Image _floorPlanWithLocations = new Bitmap(1, 1);
        public LocationManagement() {
            conn = new(Program.SqlConnectionString);
            InitializeComponent();
        }
        private void ConnOpen() {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }
        private void DrawRect(Rectangle rect, Color color) {
            if (BackgroundImage is null)
                return;
            Bitmap image = new(BackgroundImage);
            using Graphics graphics = Graphics.FromImage(image);
            using Pen pen = new(color);
            rect.Offset(new Point(EditMenu.Height, 0));
            pen.Width = 1f;
            graphics.DrawRectangle(pen, rect);
            BackgroundImage = image;
        }
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
        private void DrawLocations() {
            if (BackgroundImage is null || _locationCollection is null || !_locationCollection.Any())
                return;
            Bitmap image = new(_floorPlan);
            using Graphics graphics = Graphics.FromImage(image);
            using Pen borderPen = new(Color.Black);
            using Pen textPen = new Pen(Color.Black);
            using Brush textBrush = textPen.Brush;
            borderPen.Width = 1f;
            foreach (var location in _locationCollection) {
                graphics.DrawRectangle(borderPen, location.Rect);
                graphics.DrawString(location.Name, new Font("Arial", 10f, FontStyle.Bold), textBrush, location.UpperLeft);
            }
            BackgroundImage = image;
            _floorPlanWithLocations = image;
        }
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
        private void LocationManagement_Load(object sender, EventArgs e) {
            LoadFloorplan();
        }
        private void LocationManagement_FormClosing(object sender, FormClosingEventArgs e) {
            conn.Close();
            conn.Dispose();
        }
        private void setClinicFloorplanToolStripMenuItem_Click(object sender, EventArgs e) {
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
        private void drawNewLocationToolStripMenuItem_Click(object sender, EventArgs e) {
            _drawing = true;
        }
        private void LocationManagement_MouseDown(object sender, MouseEventArgs e) {
            if (BackgroundImage is null)
                return;
            Point click = AdjustPointForScaling(e.Location);
            if (_pendingLocation is not null) {
                if (_pendingLocation.IsPointInside(click))
                    return;
                _pendingLocation = null;
                BackgroundImage = _floorPlanWithLocations;
                Refresh();
            }
            _firstPoint = e.Location;
            _drawing = true;
            Debug.WriteLine($"Drawing active @ {e.X},{e.Y}");
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
            _pendingLocation.Name = box.Text;
            SuspendLayout();
            Controls.Remove(box);
            box.Dispose();
            ResumeLayout();
            WriteNewLocation();
            LoadFloorplan();
        }
        private void LocationManagement_MouseUp(object sender, MouseEventArgs e) {
            Debug.WriteLine($"MouseUp @ {e.X},{e.Y} x ");
            Point click = AdjustPointForScaling(e.Location);
            if (!_drawing && BackgroundImage is not null) {
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
        private void WriteNewLocation() {
            if (_pendingLocation is null) return;
            if (!_pendingLocation.Valid) return;
            ConnOpen();
            using SqlCommand cmd = new();
            cmd.Connection = conn;
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = "dbo.InsertLocation";
            cmd.Parameters.AddRange(_pendingLocation.GetSqlParameters());
            _pendingLocation.LocID = (int)cmd.ExecuteScalar();
            _locationCollection.Add(_pendingLocation);
            _pendingLocation = null;
            LoadFloorplan();
        }
        private void LocationManagement_Click(object sender, EventArgs e) {

        }

        private void LocationManagement_MouseMove(object sender, MouseEventArgs e) {
            MouseCoordTSMI.Text = $"{e.X},{e.Y}";
            Point second = AdjustPointForScaling(e.Location);
            Location? loc = _locationCollection.FindByPoint(second);
            if (loc is not null) {
                TeamTooltip.Active = true;
                TeamTooltip.ToolTipTitle = loc.Name;
            }
            if (!_drawing || BackgroundImage is null || _firstPoint is null)
                return;
            Point first = AdjustPointForScaling((Point)_firstPoint);
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
            Debug.WriteLine($"RectX: {rect.Location.X}, RectY{rect.Location.Y}, RectWidth{rect.Size.Width}, RectHeight{rect.Size.Height}, RectLeft{rect.Left}, RectTop{rect.Top}, RectRight{rect.Right}, RectBottom{rect.Bottom}");
            Bitmap image = new(_floorPlanWithLocations);
            using Graphics graphics = Graphics.FromImage(image);
            using Pen pen = new(Color.Red);
            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot;
            pen.Width = 2f;
            graphics.DrawRectangle(pen, rect);
            BackgroundImage = image;
        }
    }
}
