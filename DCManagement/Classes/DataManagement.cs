﻿using System.Data;
using DCManagement.Resources;
using Microsoft.Data.SqlClient;
#if EXT
using Microsoft.Data.Sqlite;
#endif

namespace DCManagement.Classes;
public enum DataSource {
    SQL,
    SQLite
}
public class DataManagement {
    private string _sqlConnectionString;
    public DataSource DataSource { get; init; } = DataSource.SQL;
    public DataManagement() : this(DataSource.SQL) { }
    public DataManagement(DataSource source) {
        DataSource = source;
        if (DataSource == DataSource.SQL) {
            SqlConnectionStringBuilder csb = new() {
                DataSource = GlobalResources.Server,
                InitialCatalog = GlobalResources.Database,
                IntegratedSecurity = true,
                TrustServerCertificate = true,
                ConnectTimeout = 30,
                CommandTimeout = 5
            };
            _sqlConnectionString = csb.ConnectionString;
        } else {
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!;
            string target = Path.Combine(path, "DCManagement.sqlite");
            SqliteConnectionStringBuilder csb = new() {
                DataSource = target
            };
            _sqlConnectionString = csb.ConnectionString;
        }
    }
    #region Floorplan Methods
    public Image GetFloorplan() {
        if  (DataSource == DataSource.SQL)
            return GetFloorplan_Sql();
        else
            return GetFloorplan_Sqlite();
    }
    private Image GetFloorplan_Sql() {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        Image image = new Bitmap(1,1);
        try {
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
                        image = Image.FromStream(stream);
                        if (image is null)
                            throw new InvalidDataException("Could not load floorplan from database");
                    }
        } catch {
            throw;
        }
        return image;
    }
    private Image GetFloorplan_Sqlite() {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        Image image = new Bitmap(1, 1);
        try {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = @"SELECT TOP (1) Image FROM Floorplan";
            cmd.Connection = conn;
            conn.Open();
            using SqliteDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
                if (!reader.IsDBNull(0))
                    using (MemoryStream stream = new()) {
                        using Stream data = reader.GetStream(0);
                        data.CopyTo(stream);
                        image = Image.FromStream(stream);
                        if (image is null)
                            throw new InvalidDataException("Could not load floorplan from database");
                    }
        } catch {
            throw;
        }
        return image;
    }
    public void WriteFloorplan(string Filename) {
        if (DataSource == DataSource.SQL)
            WriteFloorplan_Sql(Filename);
        else
            WriteFloorplan_Sqlite(Filename);
    }
    private void WriteFloorplan_Sql(string Filename) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        using FileStream file = File.OpenRead(Filename);
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "DELETE FROM Floorplan; " +
            "INSERT INTO Floorplan (Image) VALUES (@data)";
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
        _ =cmd.ExecuteNonQuery();
    }
    private void WriteFloorplan_Sqlite(string Filename) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using var transaction = conn.BeginTransaction();
        using SqliteCommand cmd = new();
        cmd.Transaction = transaction;
        using FileStream file = File.OpenRead(Filename);
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "DELETE FROM Floorplan; " +
            "INSERT INTO Floorplan (Image) VALUES (@data)";
        cmd.Connection = conn;
        conn.Open();

        byte[] fileData;
        using MemoryStream memoryStream = new();
        file.CopyTo(memoryStream);
        fileData = memoryStream.ToArray();

        SqliteParameter dataParam = new() {
            Direction = ParameterDirection.Input,
            ParameterName = "@data",
            Size = -1,
            SqliteType = SqliteType.Blob,
            Value = fileData
        };
        cmd.Parameters.Add(dataParam);
        _ = cmd.ExecuteNonQuery();
        transaction.Commit();
    }
    #endregion
    #region Location Methods
    public Location GetLocation(int LocationID) {
        if (DataSource == DataSource.SQL)
            return GetLocation_Sql(LocationID);
        else
            return GetLocation_Sqlite(LocationID);
    }
    private Location GetLocation_Sql(int LocationID) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location WHERE LocID=@LocID";
        cmd.Parameters.Add("@LocID", SqlDbType.Int);
        cmd.Parameters["@LocID"].Value = LocationID;
        cmd.Connection = conn;
        conn.Open();
        Location loc = new();
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            loc = new(row);
        }
        reader.Close();
        return loc;
    }
    private Location GetLocation_Sqlite(int LocationID) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location WHERE LocID=@LocID";
        cmd.Parameters.Add("@LocID", SqliteType.Integer);
        cmd.Parameters["@LocID"].Value = LocationID;
        cmd.Connection = conn;
        conn.Open();
        Location loc = new();
        using SqliteDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            loc = new(row);
        }
        reader.Close();
        return loc;
    }
    public void InsertLocation(Location Location) {
        if (DataSource == DataSource.SQL)
            InsertLocation_Sql(Location);
        else
            InsertLocation_Sqlite(Location);
    }
    private void InsertLocation_Sql(Location Location) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.InsertLocation";
        cmd.Parameters.AddRange(GetLocParameters_Sql(Location));
        Location.LocID = (int)cmd.ExecuteScalar();
    }
    private void InsertLocation_Sqlite(Location Location) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"INSERT INTO Location (Name, LocX, LocY, SizeW, SizeH)
                  VALUES (@Name, @LocX, @LocY, @SizeW, @SizeH);
                  SELECT last_insert_rowid();";
        cmd.Parameters.AddRange(GetLocParameters_Sqlite(Location));
        Location.LocID = (int)cmd.ExecuteScalar()!;
    }
    public void UpdateLocation(Location Location) {
        if (DataSource == DataSource.SQL)
            UpdateLocation_Sql(Location);
        else
            UpdateLocation_Sqlite(Location);
    }
    private void UpdateLocation_Sql(Location Location) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.UpdateLocation";
        cmd.Parameters.AddRange(GetLocParameters_Sql(true, Location));
        _ = cmd.ExecuteNonQuery();
    }
    private void UpdateLocation_Sqlite(Location Location) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "UPDATE Location SET [Name]=@Name, LocX=@LocX, LocY=@LocY, SizeW=@SizeW, SizeH=@SizeH WHERE LocID=@ID";
        cmd.Parameters.AddRange(GetLocParameters_Sqlite(true, Location));
        _ = cmd.ExecuteNonQuery();
    }
    public string DeleteLocation(Location Location) {
        if (DataSource == DataSource.SQL)
            return DeleteLocation_Sql(Location);
        else
            return DeleteLocation_Sqlite(Location);
    }
    private string DeleteLocation_Sql(Location Location) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.DeleteLocation";
        cmd.Parameters.AddRange(GetLocParameters_Sql(true, Location));
        return cmd.ExecuteScalar().ToString()!;
    }
    private string DeleteLocation_Sqlite(Location Location) {
        string cascadeCheck = @"SELECT COUNT(TeamID) 
                  FROM Team 
                  WHERE PrimaryLocation = @LocID;";
        string deleteLocation = @"DELETE FROM Location 
                      WHERE LocID = @LocID;";
        using SqliteConnection conn = new(_sqlConnectionString); ;
        using SqliteCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = cascadeCheck;
        cmd.Parameters.AddRange(GetLocParameters_Sql(true, Location));
        long teamCount = (long)cmd.ExecuteScalar()!;
        if (teamCount > 0) {
            return "Cascade";
        }
        cmd.CommandText = deleteLocation;
        _ = cmd.ExecuteNonQuery();
        return "0";
    }
    public static SqlParameter[] GetLocParameters_Sql(Location loc) {
        SqlParameter[] coll =
        [
            new SqlParameter() {
                ParameterName = "@Name",
                SqlDbType = SqlDbType.VarChar,
                Value = loc.Name
            },
            new SqlParameter() {
                ParameterName = "@LocX",
                SqlDbType = SqlDbType.Int,
                Value = loc.UpperLeft.X
            },
            new SqlParameter() {
                ParameterName = "@LocY",
                SqlDbType = SqlDbType.Int,
                Value = loc.UpperLeft.Y
            },
            new SqlParameter() {
                ParameterName = "@SizeH",
                SqlDbType = SqlDbType.Int,
                Value = loc.Size.Height
            },
            new SqlParameter() {
                ParameterName = "@SizeW",
                SqlDbType = SqlDbType.Int,
                Value = loc.Size.Width
            },
        ];
        return coll;
    }
    public static SqliteParameter[] GetLocParameters_Sqlite(Location loc) {
        var coll = new SqliteParameter[5];
        coll[0] = new SqliteParameter() {
            ParameterName = "@Name",
            SqliteType = SqliteType.Text,
            Value = loc.Name
        };
        coll[1] = new SqliteParameter() {
            ParameterName = "@LocX",
            SqliteType = SqliteType.Integer,
            Value = loc.UpperLeft.X
        };
        coll[2] = new SqliteParameter() {
            ParameterName = "@LocY",
            SqliteType = SqliteType.Integer,
            Value = loc.UpperLeft.Y
        };
        coll[3] = new SqliteParameter() {
            ParameterName = "@SizeH",
            SqliteType = SqliteType.Integer,
            Value = loc.Size.Height
        };
        coll[4] = new SqliteParameter() {
            ParameterName = "@SizeW",
            SqliteType = SqliteType.Integer,
            Value = loc.Size.Width
        };
        return coll;
    }
    public static SqlParameter[] GetLocParameters_Sql(bool forUpdate, Location loc) {
        return !forUpdate
            ? GetLocParameters_Sql(loc)
            : ([
                .. GetLocParameters_Sql(loc),
                new SqlParameter() {
                ParameterName = "@ID",
                SqlDbType = SqlDbType.Int,
                Value = loc.LocID
                },
            ]);
    }
    public static SqliteParameter[] GetLocParameters_Sqlite(bool forUpdate, Location loc) {
        return !forUpdate
            ? GetLocParameters_Sqlite(loc)
            : ([
                .. GetLocParameters_Sqlite(loc),
                new SqliteParameter() {
                ParameterName = "@ID",
                SqliteType = SqliteType.Integer,
                Value = loc.LocID
                },
            ]);
    }
    #endregion
    #region LocationCollection Methods
    public LocationCollection GetLocCollection() {
        if (DataSource == DataSource.SQL)
            return GetLocCollection_Sql();
        else
            return GetLocCollection_Sqlite();
    }
    private LocationCollection GetLocCollection_Sql() {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location";
        cmd.Connection = conn;
        conn.Open();
        LocationCollection loc = [];
        loc = [];
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            loc.Add(new Location(row));
        }
        reader.Close();
        return loc;
    }
    private LocationCollection GetLocCollection_Sqlite() {
        using SqliteConnection conn = new(_sqlConnectionString); ;
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT LocID, Name, LocX, LocY, SizeW, SizeH FROM Location";
        cmd.Connection = conn;
        conn.Open();
        LocationCollection loc = [];
        loc = [];
        using SqliteDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            loc.Add(new Location(row));
        }
        reader.Close();
        return loc;
    }
    #endregion
    #region Person Methods
    public int AddNewPerson(Person Person) {
        if (DataSource == DataSource.SQL)
            return AddNewPerson_Sql(Person);
        else
            return AddNewPerson_Sqlite(Person);
    }
    private int AddNewPerson_Sql(Person Person) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.InsertPerson";
        cmd.Parameters.AddRange(GetPersonParameters_Sql(Person)[1..]);
        return (int)cmd.ExecuteScalar();
    }
    private int AddNewPerson_Sqlite(Person Person) {
        throw new NotImplementedException();
    }
    public Person GetPerson(int PersonID) {
        if (DataSource == DataSource.SQL)
            return GetPerson_Sql(PersonID);
        else
            return GetPerson_Sqlite(PersonID);
    }
    private Person GetPerson_Sql(int PersonID) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetPersonData";
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters["@PersonID"].Value = PersonID;
        cmd.Connection = conn;
        conn.Open();
        using SqlDataAdapter adapter = new(cmd);
        DataSet ds = new();
        adapter.Fill(ds);
        DataTable personData = ds.Tables[0];
        DataTable skills = ds.Tables[1];
        if (personData.Rows.Count == 0 || personData.Rows[0].ItemArray is null)
            throw new ArgumentException("No such person exists");
        Person person = new(personData.Rows[0].ItemArray!);
        if (skills.Rows.Count == 0)
            return person;
        foreach (DataRow dataRow in skills.Rows) {
            if (dataRow[0] is null || dataRow[1] is null) continue;
            Skill newSkill = new() {
                SkillID = (int)dataRow[0],
                Description = (string)dataRow[1]
            };
            if (dataRow[2] is not null)
                newSkill.SetSlotColor((string)dataRow[2]);
            if (dataRow[3] is not null)
                newSkill.Priority = (int)dataRow[3];
            person.AddSkill(newSkill);
        }
        return person;
    }
    private Person GetPerson_Sqlite(int PersonID) {
        using SqliteConnection conn = new(_sqlConnectionString); ;
        using SqliteCommand personCmd = new();
        using SqliteCommand skillCmd = new();
        personCmd.CommandType = skillCmd.CommandType = CommandType.Text;
        personCmd.CommandText = "SELECT PersonID, LastName, FirstName, TeamID, Active, Available" +
            "FROM Person " +
            "WHERE PersonID=@PersonID";
        skillCmd.CommandText = "SELECT ps.SlotTypeID, st.Description, st.SlotColor" +
            "FROM PersonSlot ps" +
            "LEFT JOIN SlotType st" +
            "ON ps.SlotTypeID=st.SlotTypeID" +
            "WHERE PersonID=@PersonID";
        SqliteParameter personParam = new("@PersonID", SqliteType.Integer) {
            Value = PersonID
        };
        personCmd.Parameters.Add(personParam);
        skillCmd.Parameters.Add(personParam);
        personCmd.Connection = conn;
        skillCmd.Connection = conn;
        conn.Open();
        using var personReader = personCmd.ExecuteReader();
        using var skillReader = skillCmd.ExecuteReader();
        DataTable personData = new();
        DataTable skillData = new();
        personData.Load(personReader);
        skillData.Load(skillReader);
        if (personData.Rows.Count == 0 || personData.Rows[0].ItemArray is null)
            throw new ArgumentException("No such person exists");
        Person person = new(personData.Rows[0].ItemArray!);
        if (skillData.Rows.Count == 0)
            return person;
        foreach (DataRow dataRow in skillData.Rows) {
            if (dataRow[0] is null || dataRow[1] is null) continue;
            Skill newSkill = new() {
                SkillID = (int)dataRow[0],
                Description = (string)dataRow[1]
            };
            if (dataRow[2] is not null)
                newSkill.SetSlotColor((string)dataRow[2]);
            if (dataRow[3] is not null)
                newSkill.Priority = (int)dataRow[3];
            person.AddSkill(newSkill);
        }
        return person;
    }
    public PersonCollection GetPersonList() {
        if (DataSource == DataSource.SQL)
            return GetPersonList_Sql();
        else
            return GetPersonList_Sqlite();
    }
    private PersonCollection GetPersonList_Sql() {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetPeople";
        cmd.Connection = conn;
        conn.Open();
        PersonCollection people = [];
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[7];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            people.Add(new Person(row));
        }
        reader.Close();
        return people;
    }
    private PersonCollection GetPersonList_Sqlite() {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "SELECT PersonID, LastName, FirstName, TeamID, Active, Available FROM Person";
        cmd.Connection = conn;
        conn.Open();
        PersonCollection people = [];
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[7];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            people.Add(new Person(row));
        }
        reader.Close();
        return people;
    }
    private static SqlParameter[] GetPersonParameters_Sql(Person person) {
        SqlParameter[] coll =
        [
            new SqlParameter() {
                ParameterName = "@PersonID",
                SqlDbType = SqlDbType.Int,
                Value = person.PersonID
            },
            new SqlParameter() {
                ParameterName = "@LastName",
                SqlDbType = SqlDbType.VarChar,
                Value = person.LastName
            },
            new SqlParameter() {
                ParameterName = "@FirstName",
                SqlDbType = SqlDbType.VarChar,
                Value = person.FirstName
            },
            new SqlParameter() {
                ParameterName = "@TeamID",
                SqlDbType = SqlDbType.Int,
                Value = person.TeamID
            },
            new SqlParameter() {
                ParameterName = "@Active",
                SqlDbType = SqlDbType.Bit,
                Value = person.Active
            },
            new SqlParameter() {
                ParameterName = "@Available",
                SqlDbType = SqlDbType.Bit,
                Value = person.Available
            },
        ];
        return coll;
    }
    private static SqliteParameter[] GetPersonParameters_Sqlite(Person person) {
        SqliteParameter[] coll =
        [
            new SqliteParameter() {
                ParameterName = "@PersonID",
                SqliteType = SqliteType.Integer,
                Value = person.PersonID
            },
            new SqliteParameter() {
                ParameterName = "@LastName",
                SqliteType = SqliteType.Text,
                Value = person.LastName
            },
            new SqliteParameter() {
                ParameterName = "@FirstName",
                SqliteType = SqliteType.Text,
                Value = person.FirstName
            },
            new SqliteParameter() {
                ParameterName = "@TeamID",
                SqliteType = SqliteType.Integer,
                Value = person.TeamID
            },
            new SqliteParameter() {
                ParameterName = "@Active",
                SqliteType = SqliteType.Integer,
                Value = person.Active ? 1 : 0
            },
            new SqliteParameter() {
                ParameterName = "@Available",
                SqliteType = SqliteType.Integer,
                Value = person.Available ? 1 : 0
            },
        ];
        return coll;
    }
    public void UpdatePerson(Person Person, List<Skill> Skills) {
        if (DataSource == DataSource.SQL)
            UpdatePerson_Sql(Person, Skills);
        else
            UpdatePerson_Sqlite(Person, Skills);
    }
    private void UpdatePerson_Sql(Person Person, List<Skill> Skills) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.Connection = conn; conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.UpdatePerson";
        cmd.Parameters.AddRange(GetPersonParameters_Sql(Person));
        _ = cmd.ExecuteScalar();

        cmd.CommandText = "dbo.SetPersonSkill";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@IsSet", SqlDbType.Bit);
        cmd.Parameters["@PersonID"].Value = Person.PersonID;
        foreach (Skill st in Skills) {
            cmd.Parameters["@SkillID"].Value = st.SkillID;
            cmd.Parameters["@IsSet"].Value = Person.Skills.Contains(st);
            _ = cmd.ExecuteNonQuery();
        }
    }
    private void UpdatePerson_Sqlite(Person Person, List<Skill> Skills) {
        throw new NotImplementedException();
    }
    public void UpdatePersonSkills(Person Person, List<Skill> Skills) {
        if (DataSource == DataSource.SQL)
            UpdatePersonSkills_Sql(Person, Skills);
        else
            UpdatePersonSkills_Sqlite(Person, Skills);
    }
    private void UpdatePersonSkills_Sql(Person Person, List<Skill> Skills) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandText = "dbo.SetPersonSkill";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@IsSet", SqlDbType.Bit);
        cmd.Parameters["@PersonID"].Value = Person.PersonID;
        foreach (Skill st in Skills) {
            cmd.Parameters["@SkillID"].Value = st.SkillID;
            cmd.Parameters["@IsSet"].Value = Person.Skills.Contains(st);
            _ = cmd.ExecuteNonQuery();
        }
    }
    private void UpdatePersonSkills_Sqlite(Person Person, List<Skill> Skills) {
        using SqliteConnection conn = new(_sqlConnectionString); ;
        using SqliteCommand cmd = new();
        throw new NotImplementedException();
        cmd.CommandText = "dbo.SetPersonSkill";
        cmd.Parameters.Clear();
        cmd.Parameters.Add("@PersonID", SqliteType.Integer);
        cmd.Parameters.Add("@SkillID", SqliteType.Integer);
        cmd.Parameters.Add("@IsSet", SqliteType.Integer);
        cmd.Parameters["@PersonID"].Value = Person.PersonID;
        foreach (Skill st in Skills) {
            cmd.Parameters["@SkillID"].Value = st.SkillID;
            cmd.Parameters["@IsSet"].Value = Person.Skills.Contains(st) ? 1 : 0;
            _ = cmd.ExecuteNonQuery();
        }
    }
    #endregion
    #region Skill Methods
    public List<Skill> GetSkills() {
        if (DataSource == DataSource.SQL)
            return GetSkills_Sql();
        else
            return GetSkills_Sqlite();
    }
    private List<Skill> GetSkills_Sql() {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetSkills";
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        List<Skill> skills = [];
        while (reader.Read()) {
            skills.Add(new() {
                SkillID = reader.GetInt32(0),
                Description = reader.GetString(1),
                SlotColor = ColorTranslator.FromHtml("#" + reader.GetString(2)),
                Priority = reader.GetInt32(3)
            });
        }
        reader.Close();
        return skills;
    }
    private List<Skill> GetSkills_Sqlite() {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "SELECT SkillID, Description, SlotColor, Priority" +
            "FROM Skills" +
            "ORDER BY SlotTypeID";
        cmd.Connection = conn;
        conn.Open();
        using SqliteDataReader reader = cmd.ExecuteReader();
        List<Skill> skills = [];
        while (reader.Read()) {
            skills.Add(new() {
                SkillID = reader.GetInt32(0),
                Description = reader.GetString(1),
                SlotColor = ColorTranslator.FromHtml("#" + reader.GetString(2)),
                Priority = reader.GetInt32(3)
            });
        }
        reader.Close();
        return skills;
    }
    #endregion
    #region Slot Methods
    public bool DeleteSlot(int SlotID) {
        if (DataSource == DataSource.SQL)
            return DeleteSlot_Sql(SlotID);
        else
            return DeleteSlot_Sqlite(SlotID);
    }
    private bool DeleteSlot_Sql(int SlotID) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.DeleteTeamSlot";
        cmd.Parameters.Add("@SlotID", SqlDbType.Int);
        cmd.Parameters["@SlotID"].Value = SlotID;
        cmd.Connection = conn;
        conn.Open();
        return cmd.ExecuteNonQuery() == 1;
    }
    private bool DeleteSlot_Sqlite(int SlotID) {
        throw new NotImplementedException();
    }
    public int InsertSlot(int TeamID, int SkillID, int MinQty, int GoalQty) {
        if (DataSource == DataSource.SQL)
            return InsertSlot_Sql(TeamID, SkillID, MinQty, GoalQty);
        else
            return InsertSlot_Sqlite(TeamID, SkillID, MinQty, GoalQty);
    }
    private int InsertSlot_Sql(int TeamID, int SkillID, int MinQty, int GoalQty) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.InsertTeamSlot";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@MinQty", SqlDbType.Int);
        cmd.Parameters.Add("@GoalQty", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Parameters["@SkillID"].Value = SkillID;
        cmd.Parameters["@MinQty"].Value = MinQty;
        cmd.Parameters["@GoalQty"].Value = GoalQty;
        cmd.Connection = conn;
        conn.Open();
        return (int)cmd.ExecuteScalar();
    }
    private int InsertSlot_Sqlite(int TeamID, int SkillID, int MinQty, int GoalQty) {
        throw new NotImplementedException();
    }
    public int UpdateSlot(int SlotID, int TeamID, int SkillID, int MinQty, int GoalQty) {
        if (DataSource == DataSource.SQL)
            return UpdateSlot_Sql(SlotID, TeamID, SkillID, MinQty, GoalQty);
        else
            return UpdateSlot_Sqlite(SlotID, TeamID, SkillID, MinQty, GoalQty);
    }
    private int UpdateSlot_Sql(int SlotID, int TeamID, int SkillID, int MinQty, int GoalQty) {
        using SqlConnection conn = new(_sqlConnectionString); ;
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.UpdateTeamSlot";
        cmd.Parameters.Add("@SlotID", SqlDbType.Int);
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters.Add("@MinQty", SqlDbType.Int);
        cmd.Parameters.Add("@GoalQty", SqlDbType.Int);
        cmd.Parameters["@SlotID"].Value = SlotID;
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Parameters["@SkillID"].Value = SkillID;
        cmd.Parameters["@MinQty"].Value = MinQty;
        cmd.Parameters["@GoalQty"].Value = GoalQty;
        cmd.Connection = conn;
        conn.Open();
        return cmd.ExecuteNonQuery();
    }
    private int UpdateSlot_Sqlite(int SlotID, int TeamID, int SkillID, int MinQty, int GoalQty) {
        throw new NotImplementedException();
    }
    #endregion
    #region Team Methods
    public int AddNewTeam(string Name, int TeamLeadID, int LocID, bool Fill, bool Active) {
        if (DataSource == DataSource.SQL)
            return AddNewTeam_Sql(Name, TeamLeadID, LocID, Fill, Active);
        else
            return AddNewTeam_Sqlite(Name, TeamLeadID, LocID, Fill, Active);
    }
    private int AddNewTeam_Sql(string Name, int TeamLeadID, int LocID, bool Fill, bool Active) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.InsertTeam";
        cmd.Parameters.Add("@Name", SqlDbType.VarChar);
        cmd.Parameters.Add("@Lead", SqlDbType.Int);
        cmd.Parameters.Add("@LocID", SqlDbType.Int);
        cmd.Parameters.Add("@Fill", SqlDbType.Bit);
        cmd.Parameters.Add("@Active", SqlDbType.Bit);
        cmd.Parameters["@Name"].Value = Name;
        cmd.Parameters["@Lead"].Value = TeamLeadID;
        cmd.Parameters["@LocID"].Value = LocID;
        cmd.Parameters["@Fill"].Value = Fill;
        cmd.Parameters["@Active"].Value = Active;
        return (int)cmd.ExecuteScalar();
    }
    private int AddNewTeam_Sqlite(string Name, int TeamLeadID, int LocID, bool Fill, bool Active) {
        throw new NotImplementedException();
    }
    public Team GetTeam(int TeamID) {
        if (DataSource == DataSource.SQL)
            return GetTeam_Sql(TeamID);
        else
            return GetTeam_Sqlite(TeamID);
    }
    private Team GetTeam_Sql(int TeamID) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT TeamID, TeamName, TeamLead, PrimaryLocation, FillIfNoLead, Active FROM dbo.GetTeamInfo(@TeamID)";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        Team team;
        while (reader.Read()) {
            _ = reader.GetValues(row);
            team = new Team(row);
            reader.Close();
            return team;
        }
        throw new InvalidDataException("No such Team ID exists");
    }
    private Team GetTeam_Sqlite(int TeamID) {
        throw new NotImplementedException();
    }
    public List<Team> GetTeams(bool ShowActive = false) {
        if (DataSource == DataSource.SQL)
            return GetTeams_Sql(ShowActive);
        else
            return GetTeams_Sqlite(ShowActive);
    }
    private List<Team> GetTeams_Sql(bool ShowActive) {
        List<Team> teams = [];
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "SELECT TeamID, " + 
            (ShowActive ? "IIF(Active=1,TeamName,TeamName+'*') AS TeamName, " : "TeamName, ") + 
            "TeamLead, PrimaryLocation, FillIfNoLead, Active FROM Team WHERE Active=1";
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            teams.Add(new Team(row));
        }
        reader.Close();
        return teams;
    }
    private List<Team> GetTeams_Sqlite(bool ShowActive) {
        List<Team> teams = [];
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "SELECT TeamID, " +
            (ShowActive ? "IIF(Active=1,TeamName,TeamName+'*') AS TeamName, " : "TeamName, ") +
            "TeamLead, PrimaryLocation, FillIfNoLead, Active FROM Team WHERE Active=1";
        cmd.Connection = conn;
        conn.Open();
        using SqliteDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            _ = reader.GetValues(row);
            teams.Add(new Team(row));
        }
        reader.Close();
        return teams;
    }
    public static SqlParameter[] GetTeamParameters_Sql(Team team) {
        SqlParameter[] coll =
        [
            new SqlParameter() {
                ParameterName = "@TeamID",
                SqlDbType = SqlDbType.Int,
                Value = team.TeamID
            },
            new SqlParameter() {
                ParameterName = "@Name",
                SqlDbType = SqlDbType.VarChar,
                Value = team.TeamName
            },
            new SqlParameter() {
                ParameterName = "@Lead",
                SqlDbType = SqlDbType.Int,
                Value = team.TeamLeadID
            },
            new SqlParameter() {
                ParameterName = "@LocID",
                SqlDbType = SqlDbType.Int,
                Value = team.LocationID
            },
            new SqlParameter() {
                ParameterName = "@Fill",
                SqlDbType = SqlDbType.Bit,
                Value = team.FillIfNoLead
            },
            new SqlParameter() {
                ParameterName = "@Active",
                SqlDbType = SqlDbType.Bit,
                Value = team.Active
            },
        ];
        return coll;
    }
    public static SqliteParameter[] GetTeamParameters_Sqlite(Team team) {
        SqliteParameter[] coll =
        [
            new SqliteParameter() {
                ParameterName = "@TeamID",
                SqliteType = SqliteType.Integer,
                Value = team.TeamID
            },
            new SqliteParameter() {
                ParameterName = "@Name",
                SqliteType = SqliteType.Text,
                Value = team.TeamName
            },
            new SqliteParameter() {
                ParameterName = "@Lead",
                SqliteType = SqliteType.Integer,
                Value = team.TeamLeadID
            },
            new SqliteParameter() {
                ParameterName = "@LocID",
                SqliteType = SqliteType.Integer,
                Value = team.LocationID
            },
            new SqliteParameter() {
                ParameterName = "@Fill",
                SqliteType = SqliteType.Integer,
                Value = team.FillIfNoLead ? 1 : 0
            },
            new SqliteParameter() {
                ParameterName = "@Active",
                SqliteType = SqliteType.Integer,
                Value = team.Active ? 1 : 0
            },
        ];
        return coll;
    }
    public void UpdateTeam(Team Team) {
        if (DataSource == DataSource.SQL)
            UpdateTeam_Sql(Team);
        else
            UpdateTeam_Sqlite(Team);
    }
    private void UpdateTeam_Sql(Team Team) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.UpdateTeam";
        cmd.Parameters.AddRange(GetTeamParameters_Sql(Team));
        _ = cmd.ExecuteScalar();
    }
    private void UpdateTeam_Sqlite(Team Team) {
        throw new NotImplementedException();
    }
    #endregion
    #region TeamSlots Methods
    public TeamSlots GetTeamSlots(int TeamID, List<Skill> skillList) {
        if (DataSource == DataSource.SQL)
            return GetTeamSlots_Sql(TeamID, skillList);
        else
            return GetTeamSlots_Sqlite(TeamID, skillList);
    }
    private TeamSlots GetTeamSlots_Sql(int TeamID, List<Skill> skillList) {
        TeamSlots slots = [];
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT SlotID, SkillID, MinQty, GoalQty FROM dbo.GetTeamSlots(@TeamID)";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Connection = conn;
        conn.Open();
        using SqlDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Skill thisSkill = skillList.First(st => st.SkillID == reader.GetInt32(1));
            slots.Add(
                new() {
                    SlotID = reader.GetInt32(0),
                    SkillID = thisSkill.SkillID,
                    Description = thisSkill.Description,
                    SlotColor = thisSkill.SlotColor,
                    MinQty = reader.GetInt32(2),
                    GoalQty = reader.GetInt32(3)
                }
            );
        }
        reader.Close();
        return slots;
    }
    private TeamSlots GetTeamSlots_Sqlite(int TeamID, List<Skill> skillList) {
        TeamSlots slots = [];
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "SELECT SlotID, SlotType, MinQty, GoalQty " +
            "FROM LocationSlot " +
            "WHERE TeamID = @TeamID";
        cmd.Parameters.Add("@TeamID", SqliteType.Integer);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Connection = conn;
        conn.Open();
        using SqliteDataReader reader = cmd.ExecuteReader();
        while (reader.Read()) {
            Skill thisSkill = skillList.First(st => st.SkillID == reader.GetInt32(1));
            slots.Add(
                new() {
                    SlotID = reader.GetInt32(0),
                    SkillID = thisSkill.SkillID,
                    Description = thisSkill.Description,
                    SlotColor = thisSkill.SlotColor,
                    MinQty = reader.GetInt32(2),
                    GoalQty = reader.GetInt32(3)
                }
            );
        }
        reader.Close();
        return slots;
    }
    #endregion
    #region DailyAssignment Methods
    public bool CheckForFinalizedAssignments() {
        if (DataSource == DataSource.SQL)
            return CheckForFinalizedAssignments_Sql();
        else
            return CheckForFinalizedAssignments_Sqlite();
    }
    private bool CheckForFinalizedAssignments_Sql() {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.CheckForFinalizedAssignments";
        cmd.Connection = conn;
        return (bool)cmd.ExecuteScalar();
    }
    private bool CheckForFinalizedAssignments_Sqlite() {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        conn.Open();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = @"SELECT 1 FROM PersonAssignments WHERE AssignmentDate=DATE('now', 'localtime')";
        cmd.Connection = conn;
        return ((int?)cmd.ExecuteScalar() ?? 0) == 1;
    }
    public List<Person> GetDefaultSlotAssignments(int TeamID, int SkillID, PersonCollection people) {
        if (DataSource == DataSource.SQL)
            return GetDefaultSlotAssignments_Sql(TeamID, SkillID, people);
        else
            return GetDefaultSlotAssignments_Sqlite(TeamID, SkillID, people);
    }
    private List<Person> GetDefaultSlotAssignments_Sql(int TeamID, int SkillID, PersonCollection people) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = "dbo.GetDefaultSlotAssignments";
        cmd.Parameters.Add("@TeamID", SqlDbType.Int);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Parameters.Add("@SkillID", SqlDbType.Int);
        cmd.Parameters["@SkillID"].Value = SkillID;
        cmd.Connection = conn;
        conn.Open();
        List<Person> members = [];
        using SqlDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            int personID = reader.GetInt32(0);
            if (!people.ContainsKey(personID))
                people.Add(GetPerson(personID));
            members.Add(people[personID]);
        }
        reader.Close();
        return AvailablePeople.CleanUnavailable(members);
    }
    private List<Person> GetDefaultSlotAssignments_Sqlite(int TeamID, int SkillID, PersonCollection people) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.CommandText = "dbo.GetDefaultSlotAssignments";
        cmd.Parameters.Add("@TeamID", SqliteType.Integer);
        cmd.Parameters["@TeamID"].Value = TeamID;
        cmd.Parameters.Add("@SkillID", SqliteType.Integer);
        cmd.Parameters["@SkillID"].Value = SkillID;
        cmd.Connection = conn;
        conn.Open();
        List<Person> members = [];
        using SqliteDataReader reader = cmd.ExecuteReader();
        object[] row = new object[6];
        while (reader.Read()) {
            int personID = reader.GetInt32(0);
            if (!people.ContainsKey(personID))
                people.Add(GetPerson(personID));
            members.Add(people[personID]);
        }
        reader.Close();
        return AvailablePeople.CleanUnavailable(members);
    }
    public (List<Person> Unavailable,
        AvailablePeople Available,
        PersonCollection People) FillAssignmentCollections(
            List<Team> AvailableTeams,
            Team UnavailableTeam,
            Team FloatTeam) {
        if (DataSource == DataSource.SQL)
            return FillAssignmentCollections_Sql(AvailableTeams, UnavailableTeam, FloatTeam);
        else
            return FillAssignmentCollections_Sqlite(AvailableTeams, UnavailableTeam, FloatTeam);
    }
    private (List<Person> Unavailable, 
        AvailablePeople Available, 
        PersonCollection People) FillAssignmentCollections_Sql (
            List<Team> AvailableTeams, 
            Team UnavailableTeam, 
            Team FloatTeam) {
        List<Person> unavailable = [];
        AvailablePeople available = new();
        PersonCollection everyone = [];
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetPeople";
        cmd.Connection = conn;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            int personID = reader.GetInt32(0);
            Person person = GetPerson_Sql(personID);
            if (!person.Available) {
                person.Team = UnavailableTeam;
                unavailable.Add(person); 
            } else {
                person.Team = AvailableTeams.FirstOrDefault(t => t.TeamID == person.TeamID);
                if (person.Team is null && person.Available) {
                    person.Team = FloatTeam;
                    available.People.Add(person); //AvailablePeople
                }
                everyone.Add(person); //PeopleCollection
            }
        }
        return (unavailable, available, everyone);
    }
    private (List<Person> Unavailable,
    AvailablePeople Available,
    PersonCollection People) FillAssignmentCollections_Sqlite(
        List<Team> AvailableTeams,
        Team UnavailableTeam,
        Team FloatTeam) {
        List<Person> unavailable = [];
        AvailablePeople available = new();
        PersonCollection everyone = [];
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.GetPeople";
        cmd.Connection = conn;
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) {
            int personID = reader.GetInt32(0);
            Person person = GetPerson_Sqlite(personID);
            if (!person.Available) {
                person.Team = UnavailableTeam;
                unavailable.Add(person);
            } else {
                person.Team = AvailableTeams.FirstOrDefault(t => t.TeamID == person.TeamID);
                if (person.Team is null && person.Available) {
                    person.Team = FloatTeam;
                    available.People.Add(person);
                }
                everyone.Add(person); 
            }
        }
        return (unavailable, available, everyone);
    }
    public void LoadFinalizedAssignments(List<Team> Teams,
        List<Team> DefunctTeams,
        LocationCollection Locations,
        PersonCollection People,
        AvailablePeople AvailablePeople) {
        if (DataSource == DataSource.SQL)
            LoadFinalizedAssignments_Sql(Teams, DefunctTeams, Locations, People, AvailablePeople);
        else
            LoadFinalizedAssignments_Sqlite(Teams, DefunctTeams, Locations, People, AvailablePeople);
    }
    private void LoadFinalizedAssignments_Sql(List<Team> Teams, 
        List<Team> DefunctTeams, 
        LocationCollection Locations,
        PersonCollection People,
        AvailablePeople AvailablePeople) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        conn.Open();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandText = @"dbo.DailyRead";
        cmd.Connection = conn;
        using SqlDataAdapter adapter = new(cmd);
        DataSet ds = new();
        adapter.Fill(ds);
        DataTable teamData = ds.Tables[0];
        DataTable personData = ds.Tables[1];
        teamData.Columns[0].ColumnName = "TeamID";
        teamData.Columns[1].ColumnName = "LocID";
        personData.Columns[0].ColumnName = "PersonID";
        personData.Columns[1].ColumnName = "TeamID";
        personData.Columns[2].ColumnName = "SlotID";
        var teamsInUse = teamData.AsEnumerable().Select(row => row.Field<int>("TeamID")).Cast<int>().ToList();
        Teams
            .Where(t => !teamsInUse
                            .Contains((int)t.TeamID!))
            .ToList()
            .ForEach(t => {
                Teams.Remove(t);
                DefunctTeams.Add(t);
            });
        foreach (DataRow row in teamData.Rows) {
            Team thisTeam = Teams.Where(t => t.TeamID == (int)row["TeamID"]).First();
            Location assignedLocation = Locations[(int)row["LocID"]];
            thisTeam.CurrentAssignment = assignedLocation;
        }

        var assignedPeople = personData.AsEnumerable().Select(row => row.Field<int>("PersonID")).Cast<int>().ToList();
        People
            .Where(p => !assignedPeople.Contains(p.Key))
            .ToList()
            .ForEach(p => {
                AvailablePeople.People.Add(p.Value);
            });
        foreach (DataRow row in personData.Rows) {
            Person thisPerson = People[(int)row["PersonID"]];
            Team assignedTeam = Teams.Where(t => t.TeamID == (int)row["TeamID"]).First();
            Slot assignedSlot = assignedTeam.Slots.Where(s => s.SlotID == (int)row["SlotID"]).First();
            thisPerson.Team = assignedTeam;
            assignedSlot.AssignToSlot(thisPerson);
            thisPerson.AssignmentLocked = true;
        }
    }
    private void LoadFinalizedAssignments_Sqlite(List<Team> Teams,
    List<Team> DefunctTeams,
    LocationCollection Locations,
    PersonCollection People,
    AvailablePeople AvailablePeople) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand teamCmd = new();
        using SqliteCommand personCmd = new();
        conn.Open();
        teamCmd.CommandType = personCmd.CommandType = CommandType.Text;
        teamCmd.CommandText = "SELECT TeamID, LocID FROM TeamAssignments WHERE AssignmentDate==DATE('now', 'localtime')";
        personCmd.CommandText = "SELECT PersonID, TeamID, SlotID FROM PersonAssignments WHERE AssignmentDate=DATE('now', 'localtime')";
        teamCmd.Connection = conn;
        personCmd.Connection = conn;
        using var teamReader = teamCmd.ExecuteReader();
        using var personReader = personCmd.ExecuteReader();
        DataTable teamData = new();
        DataTable personData = new();
        teamData.Load(teamReader);
        personData.Load(personReader);
        teamData.Columns[0].ColumnName = "TeamID";
        teamData.Columns[1].ColumnName = "LocID";
        personData.Columns[0].ColumnName = "PersonID";
        personData.Columns[1].ColumnName = "TeamID";
        personData.Columns[2].ColumnName = "SlotID";
        var teamsInUse = teamData.AsEnumerable().Select(row => row.Field<int>("TeamID")).Cast<int>().ToList();
        Teams
            .Where(t => !teamsInUse
                            .Contains((int)t.TeamID!))
            .ToList()
            .ForEach(t => {
                Teams.Remove(t);
                DefunctTeams.Add(t);
            });
        foreach (DataRow row in teamData.Rows) {
            Team thisTeam = Teams.Where(t => t.TeamID == (int)row["TeamID"]).First();
            Location assignedLocation = Locations[(int)row["LocID"]];
            thisTeam.CurrentAssignment = assignedLocation;
        }

        var assignedPeople = personData.AsEnumerable().Select(row => row.Field<int>("PersonID")).Cast<int>().ToList();
        People
            .Where(p => !assignedPeople.Contains(p.Key))
            .ToList()
            .ForEach(p => {
                AvailablePeople.People.Add(p.Value);
            });
        foreach (DataRow row in personData.Rows) {
            Person thisPerson = People[(int)row["PersonID"]];
            Team assignedTeam = Teams.Where(t => t.TeamID == (int)row["TeamID"]).First();
            Slot assignedSlot = assignedTeam.Slots.Where(s => s.SlotID == (int)row["SlotID"]).First();
            thisPerson.Team = assignedTeam;
            assignedSlot.AssignToSlot(thisPerson);
            thisPerson.AssignmentLocked = true;
        }
    }
    public void SetPersonUnavailable(Person person) {
        if (DataSource == DataSource.SQL)
            SetPersonUnavailable_Sql(person);
        else
            SetPersonUnavailable_Sqlite(person);
    }
    private void SetPersonUnavailable_Sql(Person person) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters["@PersonID"].Value = person.PersonID;
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandText = "dbo.UnavailableToday";
        _ = cmd.ExecuteNonQuery();
    }
    private void SetPersonUnavailable_Sqlite(Person person) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Add("@PersonID", SqliteType.Integer);
        cmd.Parameters["@PersonID"].Value = person.PersonID;
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandText = "INSERT INTO Unavailability (PersonID, StartDate, EndDate) " +
            "VALUES (@PersonID, DATE('now', 'localtime'), DATE('now', 'localtime'))";
        _ = cmd.ExecuteNonQuery();
    }
    public void SetPersonAvailable(Person person) {
        if (DataSource == DataSource.SQL)
            SetPersonAvailable_Sql(person);
        else
            SetPersonAvailable_Sqlite(person);
    }
    private void SetPersonAvailable_Sql(Person person) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand cmd = new();
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.Add("@PersonID", SqlDbType.Int);
        cmd.Parameters["@PersonID"].Value = person.PersonID;
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandText = "dbo.AvailableToday";
        _ = cmd.ExecuteNonQuery();
    }
    private void SetPersonAvailable_Sqlite(Person person) {
        string deleteExactMatch = @"DELETE FROM Unavailability
                      WHERE PersonID = @PersonID
                        AND StartDate = DATE('now', 'localtime')
                        AND EndDate = DATE('now', 'localtime');";
        string checkOverlap = @"SELECT COUNT(*) 
                      FROM Unavailability
                      WHERE PersonID = @PersonID
                        AND DATE('now', 'localtime') BETWEEN StartDate AND EndDate;";
        string fetchOverlap = @"SELECT StartDate, EndDate
                      FROM Unavailability
                      WHERE PersonID = @PersonID
                        AND DATE('now', 'localtime') BETWEEN StartDate AND EndDate;";
        string deleteOverlap = @"DELETE FROM Unavailability
                          WHERE PersonID = @PersonID
                            AND StartDate = @StartDate AND EndDate = @EndDate;";
        string insertFirst = @"INSERT INTO Unavailability (PersonID, StartDate, EndDate)
                          VALUES (@PersonID, @StartDate, DATE(DATE('now', 'localtime'), '-1 day')))";
        string insertSecond = @"INSERT INTO Unavailability (PersonID, StartDate, EndDate)
                          VALUES (@PersonID, DATE(DATE('now', 'localtime'), '+1 day')), @EndDate)";
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand cmd = new();
        cmd.CommandType = CommandType.Text;
        cmd.Parameters.Add("@PersonID", SqliteType.Integer);
        cmd.Parameters["@PersonID"].Value = person.PersonID;
        cmd.Connection = conn;
        conn.Open();
        cmd.CommandText = deleteExactMatch;
        _ = cmd.ExecuteNonQuery();

        cmd.CommandText = checkOverlap;
        long returnVal = (long)cmd.ExecuteScalar()!;

        if (returnVal > 0) {
            cmd.CommandText = fetchOverlap;
            var reader = cmd.ExecuteReader();
            if (!reader.Read())
                return;
            string originalStart = reader.GetString(0);
            string originalEnd = reader.GetString(1);

            cmd.Parameters.Add("@StartDate", SqliteType.Text);
            cmd.Parameters.Add("@EndDate", SqliteType.Text);
            cmd.Parameters["@StartDate"].Value = originalStart;
            cmd.Parameters["@EndDate"].Value = originalEnd;
            cmd.CommandText = deleteOverlap;
            _ = cmd.ExecuteNonQuery();

            cmd.CommandText = insertFirst;
            _ = cmd.ExecuteNonQuery();

            cmd.CommandText = insertSecond;
            _ = cmd.ExecuteNonQuery();
        }
    }
    public void WriteFinalizedAssignments(List<Team> Teams) {
        if (DataSource == DataSource.SQL)
            WriteFinalizedAssignments_Sql(Teams);
        else
            WriteFinalizedAssignments_Sqlite(Teams);
    }
    private void WriteFinalizedAssignments_Sql(List<Team> Teams) {
        using SqlConnection conn = new(_sqlConnectionString);
        using SqlCommand teamCmd = new();
        using SqlCommand personCmd = new();
        conn.Open();
        teamCmd.CommandType = CommandType.StoredProcedure;
        teamCmd.CommandText = @"dbo.DailyTeamWrite";
        teamCmd.Connection = conn;
        personCmd.CommandType = CommandType.StoredProcedure;
        personCmd.CommandText = @"dbo.DailyPersonWrite";
        personCmd.Connection = conn;

        teamCmd.Parameters.Add("@TeamID", SqlDbType.Int);
        teamCmd.Parameters.Add("@LocID", SqlDbType.Int);
        teamCmd.Parameters.Add("@Active", SqlDbType.Bit);
        personCmd.Parameters.Add("@PersonID", SqlDbType.Int);
        personCmd.Parameters.Add("@TeamID", SqlDbType.Int);
        personCmd.Parameters.Add("@SlotID", SqlDbType.Int);
        foreach (var team in Teams) {
            teamCmd.Parameters["@TeamID"].Value = team.TeamID;
            if (team.CurrentAssignment is null) {
                teamCmd.Parameters["@LocID"].Value = DBNull.Value;
                teamCmd.Parameters["@Active"].Value = false;
            } else {
                teamCmd.Parameters["@LocID"].Value = team.CurrentAssignment.LocID;
                teamCmd.Parameters["@Active"].Value = true;
            }
            _ = teamCmd.ExecuteNonQuery();

            foreach (var slot in team.Slots) {
                if (slot is null || slot.AssignedToSlot == 0)
                    continue;
                foreach (var person in slot.Assigned) {
                    personCmd.Parameters["@PersonID"].Value = person.PersonID;
                    personCmd.Parameters["@TeamID"].Value = person.TeamID;
                    personCmd.Parameters["@SlotID"].Value = slot.SlotID;
                    _ = personCmd.ExecuteNonQuery();
                }
            }
        }
        conn.Close();
    }
    private void WriteFinalizedAssignments_Sqlite(List<Team> Teams) {
        using SqliteConnection conn = new(_sqlConnectionString);
        using SqliteCommand teamCmd = new();
        using SqliteCommand personCmd = new();
        conn.Open();
        teamCmd.CommandType = CommandType.StoredProcedure;
        teamCmd.CommandText = "INSERT OR REPLACE INTO TeamAssignments (TeamID, LocID, AssignmentDate" +
            "VALUES (@TeamID, @LocID, DATE('now', 'localtime'))";
        teamCmd.Connection = conn;
        personCmd.CommandType = CommandType.StoredProcedure;
        personCmd.CommandText = "INSERT OR REPLACE INTO PersonAssignments (PersonID, TeamID, SlotID, AssignmentDate" +
            "VALUES (@PersonID, @TeamID, @SlotID, DATE('now', 'localtime'))";
        personCmd.Connection = conn;

        teamCmd.Parameters.Add("@TeamID", SqliteType.Integer);
        teamCmd.Parameters.Add("@LocID", SqliteType.Integer);
        teamCmd.Parameters.Add("@Active", SqliteType.Integer);
        personCmd.Parameters.Add("@PersonID", SqliteType.Integer);
        personCmd.Parameters.Add("@TeamID", SqliteType.Integer);
        personCmd.Parameters.Add("@SlotID", SqliteType.Integer);
        foreach (var team in Teams) {
            teamCmd.Parameters["@TeamID"].Value = team.TeamID;
            if (team.CurrentAssignment is null) {
                teamCmd.Parameters["@LocID"].Value = DBNull.Value;
                teamCmd.Parameters["@Active"].Value = 0;
            } else {
                teamCmd.Parameters["@LocID"].Value = team.CurrentAssignment.LocID;
                teamCmd.Parameters["@Active"].Value = 1;
            }
            _ = teamCmd.ExecuteNonQuery();

            foreach (var slot in team.Slots) {
                if (slot is null || slot.AssignedToSlot == 0)
                    continue;
                foreach (var person in slot.Assigned) {
                    personCmd.Parameters["@PersonID"].Value = person.PersonID;
                    personCmd.Parameters["@TeamID"].Value = person.TeamID;
                    personCmd.Parameters["@SlotID"].Value = slot.SlotID;
                    _ = personCmd.ExecuteNonQuery();
                }
            }
        }
        conn.Close();
    }
    #endregion
}