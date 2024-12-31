using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCManagement.Classes; 
public class Team {
    private int? _teamLead;
    private int? _primaryLoc;
    public int? TeamID { get; set; }
    public Person? TeamLead { get; set; }
    public int TeamLeadID {
        get {
            return TeamLead?.PersonID ?? _teamLead ?? -1;
        }
        internal set {
            _teamLead = value;
        }
    }
    public Location? PrimaryLocation { get; set; }
    public int LocationID {
        get {
            return PrimaryLocation?.LocID ?? _primaryLoc ?? -1;
        }
        internal set {
            _primaryLoc = value;
        }
    }
    public bool FillIfNoLead { get; set; } = true;
    public bool Active { get; set; } = true;
    public string TeamName { get; set; }
    public Location? CurrentAssignment { get; set; }
    public EntitySlots Slots { get; set; } = [];
    public Team() {
        TeamName = string.Empty;
    }
    public Team (string TeamName) {
        this.TeamName = TeamName;
    }
    public Team (object[] values) {
        TeamID = (int)values[0];
        TeamName = (string)values[1];
        _teamLead = values[2]==DBNull.Value ? null : (int)values[2];
        _primaryLoc = values[3] == DBNull.Value ? null : (int)values[3];
        FillIfNoLead = (bool)values[4];
        Active = (bool)values[5];
    }
    public SqlParameter[] GetSqlParameters() {
        var coll = new SqlParameter[6];
        coll[0] = new SqlParameter() {
            ParameterName = "@TeamID",
            SqlDbType = SqlDbType.Int,
            Value = TeamID
        };
        coll[1] = new SqlParameter() {
            ParameterName = "@Name",
            SqlDbType = SqlDbType.VarChar,
            Value = TeamName
        };
        coll[2] = new SqlParameter() {
            ParameterName = "@Lead",
            SqlDbType = SqlDbType.Int,
            Value = TeamLeadID
        };
        coll[3] = new SqlParameter() {
            ParameterName = "@LocID",
            SqlDbType = SqlDbType.Int,
            Value = LocationID
        };
        coll[4] = new SqlParameter() {
            ParameterName = "@Fill",
            SqlDbType = SqlDbType.Bit,
            Value = FillIfNoLead
        };
        coll[5] = new SqlParameter() {
            ParameterName = "@Active",
            SqlDbType = SqlDbType.Bit,
            Value = Active
        };
        return coll;
    }
}
