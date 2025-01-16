namespace DCManagement.Classes; 
public record struct Unavailability {
    public int RecordID { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Unavailability() { }
}
