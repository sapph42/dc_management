using System.Data;
using Microsoft.Data.SqlClient;

namespace DCManagement.Classes; 
public class Floorplan {
    public Image? BaseImage { get; set; }
    public Image? ImageWithLocations { get; set; }
    public Image? ImageMoving { get; set; }
    public LocationCollection Locations { get; set; } = [];
    public Size ClientSize { get; set; }
    public Size ImageSize { 
        get {
            return BaseImage?.Size ?? new Size();
        } 
    }
    public Floorplan () {
        LoadFloorplan();
        if (ImageWithLocations is not null)
            ImageMoving = (Image)ImageWithLocations.Clone();
    }
    public Point AdjustPointForScaling(Point BasePoint) {
        if (BaseImage is null)
            return BasePoint;
        float scaleX = (float)ImageSize.Width / ClientSize.Width;
        float scaleY = (float)ImageSize.Height / ClientSize.Height;
        return new Point(
            (int)(BasePoint.X * scaleX),
            (int)(BasePoint.Y * scaleY)
        );
    }
    public Point AdjustPointforScalingInverse(Point BasePoint) {
        if (BaseImage is null)
            return BasePoint;
        float scaleX = (float)ImageSize.Width / ClientSize.Width;
        float scaleY = (float)ImageSize.Height / ClientSize.Height;
        return new Point(
            (int)(BasePoint.X / scaleX),
            (int)(BasePoint.Y / scaleY)
        );
    }
    public void LoadFloorplan() {
        Program.OpenConn();
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT TOP (1) Image FROM Floorplan";
        cmd.Connection = Program.conn;
        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
            if (!reader.IsDBNull(0))
                using (MemoryStream stream = new()) {
                    using Stream data = reader.GetStream(0);
                    data.CopyTo(stream);
                    BaseImage = Image.FromStream(stream);
                }
        if (BaseImage is null)
            return;
        reader.Close();
        ImageWithLocations = DrawLocations();
    }
    public Image? DrawLocations() {
        if (BaseImage is null || Locations is null || !Locations.Any())
            return null;
        return DrawLocations(Locations);
    }
    public Image? DrawLocations(LocationCollection usingCollection) {
        if (BaseImage is null || usingCollection is null || !usingCollection.Any())
            return null;
        Bitmap image = new(BaseImage);
        using Graphics graphics = Graphics.FromImage(image);
        using Pen borderPen = new(Color.Black);
        using Pen textPen = new(Color.Black);
        using Brush textBrush = textPen.Brush;
        borderPen.Width = 1f;
        foreach (var location in usingCollection.Values) {
            graphics.DrawRectangle(borderPen, location.Rect);
            graphics.DrawString(location.Name, new Font("Arial", 10f, FontStyle.Bold), textBrush, location.UpperLeft);
        }
        return image;
    }
    public void SetMoving(LocationCollection usingCollection) {
        if (BaseImage is null || usingCollection is null || !usingCollection.Any())
            return;
        Bitmap image = new(BaseImage);
        using Graphics g = Graphics.FromImage(image);
        using Pen pen = new(Color.Black);
        using Brush brush = pen.Brush;
        pen.Width = 1f;
        foreach (var location in usingCollection.Values) {
            g.DrawRectangle(pen, location.Rect);
            g.DrawString(location.Name, new Font("Arial", 10f, FontStyle.Bold), brush, location.UpperLeft);
        }
        ImageMoving = image;
    }
    public Image DrawMovingRectangle(Rectangle rect) {
        Pen pen;
        Image image = (Image)(ImageMoving?.Clone() ?? ImageWithLocations?.Clone() ?? BaseImage!.Clone());
        using (Graphics g = Graphics.FromImage(image)) {
            pen = new(Color.Black) {
                Width = 2f
            };
            g.DrawRectangle(pen, rect);
        }
        return image;
    }
    public Image DrawNewRectangle(Rectangle rect) {
        Pen pen;
        Image image = (Image)(ImageWithLocations?.Clone() ?? BaseImage!.Clone());
        using (Graphics graphics = Graphics.FromImage(image)) {
            pen = new(Color.Red) {
                DashStyle = System.Drawing.Drawing2D.DashStyle.DashDot,
                Width = 2f
            };
            graphics.DrawRectangle(pen, rect);
        }
        return image;
    }
}
