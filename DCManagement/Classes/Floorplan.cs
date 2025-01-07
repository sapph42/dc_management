using System.Data;
using Microsoft.Data.SqlClient;

namespace DCManagement.Classes; 
public class Floorplan {
    public Image BaseImage { get; set; }
    public Image? ImageWithLocations { get; set; }
    public Image? ImageMoving { get; set; }
    public LocationCollection Locations { get; set; } = [];
    public required Form Client { get; set; }
    public Size ClientSize { 
        get {
            return Client.Size;
        }
    }
    public Size ImageSize { 
        get {
            return BaseImage.Size;
        } 
    }
    public PointF Scale {
        get {
            return new PointF(
                (float)BaseImage.Size.Width / Client.Size.Width,
                (float)BaseImage.Size.Height / Client.Size.Height
            );
        }
    }
    public Floorplan () {
        BaseImage = LoadFloorplan();
        if (ImageWithLocations is not null)
            ImageMoving = (Image)ImageWithLocations.Clone();
    }
    public Image LoadFloorplan() {
        using SqlConnection conn = new(Program.SqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT TOP (1) Image FROM Floorplan";
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
            if (!reader.IsDBNull(0))
                using (MemoryStream stream = new()) {
                    using Stream data = reader.GetStream(0);
                    data.CopyTo(stream);
                    BaseImage = Image.FromStream(stream);
                }
        if (BaseImage is null)
            throw new InvalidDataException("Could not load floorplan from database");
        reader.Close();
        ImageWithLocations = DrawLocations();
        return BaseImage;
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
    public PointF GetScale() {
        float scaleX = (float)Client.ClientSize.Width / BaseImage.Size.Width;
        float scaleY = (float)Client.ClientSize.Height / BaseImage.Size.Height;
        return new PointF(scaleX, scaleY);
    }
    public Point TransformCoordinates(Point location) {
        PointF scaleF = GetScale();
        float scaleX = scaleF.X;
        float scaleY = scaleF.Y;
        int offsetX = 0, offsetY = 0;
        if (Client.BackgroundImageLayout == ImageLayout.Center) {
            scaleX = Math.Min(scaleX, scaleY);
            scaleY = scaleX;
            offsetX = (int)((Client.ClientSize.Width - BaseImage.Size.Width * scaleX) / 2);
            offsetY = (int)((Client.ClientSize.Height - BaseImage.Size.Height * scaleY) / 2);
        }

        int newX = (int)(location.X * scaleX) + offsetX;
        int newY = (int)(location.Y * scaleY) + offsetY;
        return new Point(newX, newY);
    }
    public Point TransformCoordinatesInv(Point location) {
        PointF scaleF = GetScale();
        float scaleX = scaleF.X;
        float scaleY = scaleF.Y;
        int offsetX = 0, offsetY = 0;
        if (Client.BackgroundImageLayout == ImageLayout.Center) {
            scaleX = Math.Min(scaleX, scaleY);
            scaleY = scaleX;
            offsetX = (int)((Client.ClientSize.Width - BaseImage.Size.Width * scaleX) / 2);
            offsetY = (int)((Client.ClientSize.Height - BaseImage.Size.Height * scaleY) / 2);
        }
        int newX = (int)((location.X - offsetX) / scaleX);
        int newY = (int)((location.Y - offsetY) / scaleY);
        return new Point(newX, newY);
    }
    public Rectangle TransformRectangle(Rectangle rect) {
        PointF scaleF = GetScale();
        float scaleX = scaleF.X;
        float scaleY = scaleF.Y;
        int offsetX = 0, offsetY = 0;
        if (Client.BackgroundImageLayout == ImageLayout.Center) {
            float scale = Math.Min(scaleX, scaleY);
            scaleX = scaleY = scale;
            offsetX = (int)((Client.ClientSize.Width - BaseImage.Size.Width * scaleX) / 2);
            offsetY = (int)((Client.ClientSize.Height - BaseImage.Size.Height * scaleY) / 2);
        }

        int newX = (int)(rect.Location.X * scaleX) + offsetX;
        int newY = (int)(rect.Location.Y * scaleY) + offsetY;
        int newWidth = (int)(rect.Width * scaleX);
        int newHeight = (int)(rect.Height * scaleY);
        return new Rectangle(newX, newY, newWidth, newHeight);
    }
}
