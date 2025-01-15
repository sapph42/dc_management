namespace DCManagement.Classes; 
public class Floorplan {
    public enum Edge {
        None,
        Top,
        Bottom,
        Left,
        Right,
        TopRight,
        TopLeft,
        BottomRight,
        BottomLeft
    }
    private Dictionary<Point, Edge> _points = [];
    private DataManagement _data;
    public Image BaseImage { get; set; }
    public Image? ImageWithLocations { get; set; }
    public Image? ImageMoving { get; set; }
    public LocationCollection Locations { get; set; } = [];
    public Dictionary<Point, Edge> BorderPoints { get => _points; }
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
        _data = new(Program.Source);
        BaseImage = LoadFloorplan();
        AddLocations(_data.GetLocCollection());
        if (ImageWithLocations is not null)
            ImageMoving = (Image)ImageWithLocations.Clone();
    }
    public void AddLocation(Location location) {
        if (Locations.ContainsKey(location.LocID))
            return;
        Locations.Add(location.LocID, location);
        AddPerimeterPoints(location.Rect);
    }
    public void AddLocations(LocationCollection locations) {
        Locations = locations;
        foreach (Location location in Locations.Values) {
            AddPerimeterPoints(location.Rect);
        }
    }
    private void AddPerimeterPoints(Rectangle rect) {
        // Top edge
        for (int x = rect.Left; x <= rect.Right; x++) {
            var point = new Point(x, rect.Top);
            _points[point] = (x == rect.Left) ? Edge.TopLeft :
                             (x == rect.Right) ? Edge.TopRight : Edge.Top;
        }

        // Bottom edge
        for (int x = rect.Left; x <= rect.Right; x++) {
            var point = new Point(x, rect.Bottom);
            _points[point] = (x == rect.Left) ? Edge.BottomLeft :
                             (x == rect.Right) ? Edge.BottomRight : Edge.Bottom;
        }

        // Left edge (excluding corners)
        for (int y = rect.Top + 1; y < rect.Bottom; y++) {
            var point = new Point(rect.Left, y);
            _points[point] = Edge.Left;
        }

        // Right edge (excluding corners)
        for (int y = rect.Top + 1; y < rect.Bottom; y++) {
            var point = new Point(rect.Right, y);
            _points[point] = Edge.Right;
        }
    }
    public Image? DrawLocations() {
        if (BaseImage is null || Locations is null || !Locations.Any())
            return null;
        _points.Clear();
        foreach (var location in Locations.Values) {
            AddPerimeterPoints(location.Rect);
        }
        return DrawLocations(Locations);
    }
    public Image? DrawLocations(LocationCollection usingCollection) {
        if (BaseImage is null || usingCollection is null || !usingCollection.Any())
            return null;
        Bitmap image = new(BaseImage);
        using Graphics graphics = Graphics.FromImage(image);
        using Pen borderPen = new(Color.Cyan);
        using Pen textPen = new(Color.Black);
        using Brush textBrush = textPen.Brush;
        borderPen.Width = 2f;
        foreach (var location in usingCollection.Values) {
            if (location.Clinical)
                borderPen.Color = Color.OliveDrab;
            else
                borderPen.Color = Color.Cyan;
            graphics.DrawRectangle(borderPen, location.Rect);
            graphics.DrawString(location.Name, new Font("Arial", 10f, FontStyle.Bold), textBrush, location.UpperLeft);
        }
        return image;
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
    public Location? FindByPoint(Point point) => Locations.FindByPoint(point);
    public Edge IsPointOnPerimeter(Point point) => _points.TryGetValue(point, out var edge) ? edge : Edge.None;
    public Image LoadFloorplan() {
        BaseImage = _data.GetFloorplan();
        ImageWithLocations = DrawLocations();
        return BaseImage;
    }
    public void RemoveLocation(int locID) {
        if (Locations.TryGetValue(locID, out var location))
            RemoveLocation(location);
    }
    public void RemoveLocation(Location location) {
        RemovePerimeterPoints(location.Rect);
        Locations.Remove(location.LocID);
    }
    private void RemovePerimeterPoints(Rectangle rect) {
        // Top edge
        for (int x = rect.Left; x <= rect.Right; x++) {
            var point = new Point(x, rect.Top);
            _points[point] = (x == rect.Left) ? Edge.TopLeft :
                             (x == rect.Right) ? Edge.TopRight : Edge.Top;
        }

        // Bottom edge
        for (int x = rect.Left; x <= rect.Right; x++) {
            var point = new Point(x, rect.Bottom);
            _points[point] = (x == rect.Left) ? Edge.BottomLeft :
                             (x == rect.Right) ? Edge.BottomRight : Edge.Bottom;
        }

        // Left edge (excluding corners)
        for (int y = rect.Top + 1; y < rect.Bottom; y++) {
            var point = new Point(rect.Left, y);
            _points[point] = Edge.Left;
        }

        // Right edge (excluding corners)
        for (int y = rect.Top + 1; y < rect.Bottom; y++) {
            var point = new Point(rect.Right, y);
            _points[point] = Edge.Right;
        }
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
    public void UpdateLocation(Location location) {
        RemoveLocation(Locations[location.LocID]);
        AddLocation(location);
    }
}
