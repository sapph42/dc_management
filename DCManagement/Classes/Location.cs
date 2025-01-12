namespace DCManagement.Classes; 
public class Location {
    public int LocID { get; set; }
    public string? Name { get; set; }
    public Point UpperLeft { get; set; }
    public Size Size { get; set; }
    public Rectangle Rect {
        get {
            return new Rectangle(UpperLeft, Size);
        }
    }
    public Point CenterLeft {
        get {
            int Y = Rect.Size.Height / 2 + UpperLeft.Y;
            return new Point(UpperLeft.X, Y);
        }
    }
    public bool Valid {
        get {
            return (Name is not null);
        }
    }
    public Location() { }
    public Location(Rectangle rect) {
        UpperLeft = rect.Location;
        Size = rect.Size;
    }
    public Location(Point Location, Size Size) {
        UpperLeft = Location;
        this.Size = Size;
    }
    public Location(int X, int Y, int Width, int Height) {
        UpperLeft = new Point(X, Y);
        Size = new Size(Width, Height);
    }
    public Location(object[] values) {
        LocID = (int)values[0];
        Name = (string)values[1];
        UpperLeft = new((int)values[2], (int)values[3]);
        Size = new((int)values[4], (int)values[5]);
    }
    public Location Clone() {
        Location clone = new(Rect) {
            LocID = this.LocID,
            Name = this.Name
        };
        return clone;
    }
    public bool Equals(Location otherLocation) {
        return (LocID == otherLocation.LocID &&  Name == otherLocation.Name);
    }
    public bool IsPointInside(Point point) {
        return point.X >= UpperLeft.X && point.X <= Rect.Right &&
            point.Y >= UpperLeft.Y && point.Y <= Rect.Bottom;
    }
    public bool IsPointInside(int X, int Y) {
        return IsPointInside(new Point(X, Y));
    }
    public bool IntersectsWith(Location OtherLocation) {
        return Rect.IntersectsWith(OtherLocation.Rect);
    }
    public bool IntersectsWith(Rectangle rect) {
        return Rect.IntersectsWith(rect);
    }
}