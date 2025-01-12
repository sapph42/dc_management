using DCManagement.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCManagement.Forms {
    public partial class Test : Form {
        private Floorplan _floorplan;
        private Size _maxSize;
        private Label[] _testLabels = new Label[3];
        private DataManagement _data;
        public Test() {
            InitializeComponent();
            _data = new(Program.Source);
            _floorplan = new Floorplan() {
                Locations = _data.GetLocCollection(),
                Client = this
            };
            _maxSize = new() {
                Height = 900,
                Width = 900
            };
            _floorplan.LoadFloorplan();
            ResizeForm();
            BackgroundImage = _floorplan.ImageWithLocations;
            SuspendLayout();

            ResumeLayout();
        }
        private void ResizeForm() {
            Size imageSize = _floorplan.ImageSize;
            float aspectRatio = (float)imageSize.Width / (float)imageSize.Height;
            Size adjustment = new(Width - ClientSize.Width, Height - ClientSize.Height);
            Size maxClientSize = new(_maxSize.Width - adjustment.Width, _maxSize.Height - adjustment.Height);
            if (imageSize.Width <= maxClientSize.Width && imageSize.Height <= maxClientSize.Height) {
                Size = imageSize + adjustment;
                BackgroundImageLayout = ImageLayout.Center;
            } else {
                if (aspectRatio >= 1) {
                    Size = new Size() {
                        Width = maxClientSize.Width,
                        Height = (int)(maxClientSize.Width / aspectRatio)
                    } + adjustment;
                } else {
                    Size = new Size() {
                        Width = (int)(maxClientSize.Height * aspectRatio),
                        Height = maxClientSize.Height
                    } + adjustment;
                }
                BackgroundImageLayout = ImageLayout.Stretch;
            }
        }

        private void Test_MouseMove(object sender, MouseEventArgs e) {
            Point actualPoint = PointToClient(e.Location);
            MouseCoordTSMI.Text = $"{e.X},{e.Y};{actualPoint.X},{actualPoint.Y}";
        }

        private void Test_MouseUp(object sender, MouseEventArgs e) {
            Debug.WriteLine("");
        }
    }
}
