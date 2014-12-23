﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Linq;
using System.Web;

public class DbInterfaceScheduling
{
    #region Audition Rooms
    /*
     * Pre:
     * Post: The new room is associated with the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool AddRoom(int auditionOrgId, string room)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionRoomNew";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);
            cmd.Parameters.AddWithValue("@room", room);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "AddRoom", "auditionOrgId: " + auditionOrgId + ", room: " + room,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    /*
     * Pre:  No judges may be scheduled in the room
     * Post: The room is deleted from the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool DeleteRoom(int auditionOrgId, string room)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionRoomDelete";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);
            cmd.Parameters.AddWithValue("@room", room);

            adapter.Fill(table);

            if (table.Rows.Count == 1)
            {
                success = Convert.ToBoolean(table.Rows[0]["Result"]);
            }
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "DeleteRoom", "auditionOrgId: " + auditionOrgId + ", room: " + room,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    /*
     * Pre:
     * Post: Retrieves the rooms for the input audition id
     * @param auditionId is the id of the audition
     * @returns a list of all rooms currently associated with the audition
     */
    public static List<string> GetAuditionRooms(int auditionOrgId)
    {
        List<string> rooms = new List<string>();
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionRoomSelect";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);

            adapter.Fill(table);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                rooms.Add(table.Rows[i]["Room"].ToString());
            }
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "GetAuditionRooms", "auditionOrgId: " + auditionOrgId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            rooms = null;
        }

        connection.Close();

        return rooms;
    }

    #endregion Audition Rooms
    #region Theory Rooms

    /*
     * Pre:
     * Post: Retrieves the theory rooms for the input audition id
     * @param auditionId is the id of the audition
     * @returns a list of all rooms currently associated with the audition for theory tests
     */
    public static List<Tuple<string, string>> GetAuditionTheoryRooms(int auditionOrgId)
    {
        List<Tuple<string, string>> rooms = new List<Tuple<string, string>>();
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionTheoryRoomSelect";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);

            adapter.Fill(table);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                string test = table.Rows[i]["Test"].ToString();
                string room = table.Rows[i]["Room"].ToString();

                rooms.Add(new Tuple<string, string>(test, room));
            }
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "GetAuditionTheoryRooms", "auditionOrgId: " + auditionOrgId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            rooms = null;
        }

        connection.Close();

        return rooms;
    }

    /*
     * Pre:
     * Post: The new room is associated with a theory test for the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param theoryTest is the theory test
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool AddTheoryRoom(int auditionOrgId, string theoryTest, string room)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionTheoryRoomNew";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);
            cmd.Parameters.AddWithValue("@test", theoryTest);
            cmd.Parameters.AddWithValue("@room", room);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "AddTheoryRoom", "auditionOrgId: " + auditionOrgId + ", theoryTest: " + theoryTest + ", room: " + room,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    /*
     * Pre:  
     * Post: The room is deleted, for theory test purposes from the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param theoryTest is the theory test
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool DeleteTheoryRoom(int auditionOrgId, string theoryTest, string room)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionTheoryRoomDelete";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);
            cmd.Parameters.AddWithValue("@test", theoryTest);
            cmd.Parameters.AddWithValue("@room", room);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "DeleteTheoryRoom", "auditionOrgId: " + auditionOrgId + ", theoryTest: " + theoryTest + ", room: " + room,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    #endregion Theory Rooms
    #region Judges

    /*
     * Pre:
     * Post: Retrieves the judges that could be used for the input audition id
     * @param auditionId is the id of the audition
     * @returns a list of all judges currently associated with the audition's district 
     */
    public static List<Judge> GetDistrictJudges(int auditionOrgId)
    {
        List<Judge> judges = new List<Judge>();
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_DistrictJudgeSelect";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);

            adapter.Fill(table);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                int id = Convert.ToInt32(table.Rows[i]["ContactId"]);
                string firstName = table.Rows[i]["FirstName"].ToString();
                string mi = table.Rows[i]["MI"].ToString();
                string lastName = table.Rows[i]["LastName"].ToString();
                string phone = table.Rows[i]["Phone"].ToString();
                string email = table.Rows[i]["EmailAddress"].ToString();
                int districtId = Convert.ToInt32(table.Rows[i]["GeoId"].ToString());
                string contactType = table.Rows[i]["ContactType"].ToString();
                List<JudgePreference> preferences = new List<JudgePreference>();

                Judge judge = new Judge(id, firstName, mi, lastName, email, phone, districtId, contactType, preferences, true);
                judges.Add(judge);
            }
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "GetDistrictJudges", "auditionOrgId: " + auditionOrgId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            judges = null;
        }

        connection.Close();

        return judges;
    }

    /*
     * Pre:
     * Post: Retrieves the judges that are associated with the input audition id
     * @param auditionId is the id of the audition
     * @returns a list of all judges currently associated with the audition 
     */
    public static List<Judge> GetAuditionJudges(int auditionOrgId)
    {
        List<Judge> judges = new List<Judge>();
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionJudgeSelect";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);

            adapter.Fill(table);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                int id = Convert.ToInt32(table.Rows[i]["ContactId"]);
                string firstName = table.Rows[i]["FirstName"].ToString();
                string mi = table.Rows[i]["MI"].ToString();
                string lastName = table.Rows[i]["LastName"].ToString();
                string phone = table.Rows[i]["Phone"].ToString();
                string email = table.Rows[i]["EmailAddress"].ToString();
                int districtId = Convert.ToInt32(table.Rows[i]["GeoId"].ToString());
                string contactType = table.Rows[i]["ContactType"].ToString();
                List<JudgePreference> preferences = new List<JudgePreference>();

                Judge judge = new Judge(id, firstName, mi, lastName, email, phone, districtId, contactType, preferences, true);
                judges.Add(judge);
            }
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "GetAuditionJudges", "auditionOrgId: " + auditionOrgId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            judges = null;
        }

        connection.Close();

        return judges;
    }

    /*
     * Pre:
     * Post: Retrieves the judge room assignments that are associated with the input audition id
     * @param auditionId is the id of the audition
     * @returns a list of all judge room assignments currently associated with the audition 
     */
    public static List<JudgeRoomAssignment> GetAuditionJudgeRoomAssignments(int auditionOrgId)
    {
        List<JudgeRoomAssignment> judgeRoomAssignments = new List<JudgeRoomAssignment>();
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionJudgeRoomSelect";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);

            adapter.Fill(table);

            for (int i = 0; i < table.Rows.Count; i++)
            {
                // Get the judge information
                int id = Convert.ToInt32(table.Rows[i]["ContactId"]);
                string firstName = table.Rows[i]["FirstName"].ToString();
                string mi = table.Rows[i]["MI"].ToString();
                string lastName = table.Rows[i]["LastName"].ToString();
                string phone = table.Rows[i]["Phone"].ToString();
                string email = table.Rows[i]["EmailAddress"].ToString();
                int districtId = Convert.ToInt32(table.Rows[i]["GeoId"].ToString());
                string contactType = table.Rows[i]["ContactType"].ToString();
                List<JudgePreference> preferences = new List<JudgePreference>();
                Judge judge = new Judge(id, firstName, mi, lastName, email, phone, districtId, contactType, preferences, true);

                // Get the schedule information
                string room = table.Rows[i]["Room"].ToString();
                string startTime = table.Rows[i]["DisplayTimeStart"].ToString();
                string endTime = table.Rows[i]["DisplayTimeEnd"].ToString();
                int scheduleId = Convert.ToInt32(table.Rows[i]["ScheduleId"]);
                int scheduleOrder = 0;
                if (!table.Rows[i]["ScheduleOrder"].ToString().Equals("")) scheduleOrder = Convert.ToInt32(table.Rows[i]["ScheduleOrder"]);

                List<Tuple<int, string>> times = new List<Tuple<int, string>>();
                times.Add(new Tuple<int, string>(scheduleId, startTime + " to " + endTime));

                // If the judge has already been added for the current room, just add the new time
                JudgeRoomAssignment assignment = new JudgeRoomAssignment(judge, room, times, scheduleOrder);
                if (judgeRoomAssignments.Contains(assignment))
                {
                    int idx = judgeRoomAssignments.IndexOf(assignment);
                    JudgeRoomAssignment assignmentToUpdate = judgeRoomAssignments.ElementAt(idx);
                    assignmentToUpdate.times.AddRange(times);
                    judgeRoomAssignments.RemoveAt(idx);
                    judgeRoomAssignments.Add(assignmentToUpdate);
                }
                else
                {
                    judgeRoomAssignments.Add(assignment);
                }
            }
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "GetAuditionJudgeRoomAssignments", "auditionOrgId: " + auditionOrgId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            judgeRoomAssignments = null;
        }

        connection.Close();

        return judgeRoomAssignments;
    }

    /*
     * Pre:
     * Post: The new room is associated with a theory test for the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param theoryTest is the theory test
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool AddJudge(int auditionOrgId, int contactId, int scheduleOrder)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionJudgeNew";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@contactId", contactId);
            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);
            cmd.Parameters.AddWithValue("@order", scheduleOrder);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "AddJudge", "auditionOrgId: " + auditionOrgId + ", contactId: " + contactId + ", scheduleOrder: " + scheduleOrder,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    /*
     * Pre:  
     * Post: The room is deleted, for theory test purposes from the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param theoryTest is the theory test
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool DeleteJudge(int auditionOrgId, int contactId)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionJudgeDelete";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@contactId", contactId);
            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "DeleteJudge", "auditionOrgId: " + auditionOrgId + ", contactId: " + contactId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    /*
     * Pre:
     * Post: The new room is associated with a theory test for the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param theoryTest is the theory test
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool AddJudgeTime(int auditionOrgId, int contactId, int scheduleId, string room)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_JudgePrefTimeNew";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@contactId", contactId);
            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);
            cmd.Parameters.AddWithValue("@schedule", scheduleId);
            cmd.Parameters.AddWithValue("@room", room);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "AddJudgeTime", "auditionOrgId: " + auditionOrgId + ", contactId: " + contactId + ", scheduleOrder: " + scheduleId + ", room: " + room,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    /*
     * Pre:  
     * Post: The room is deleted, for theory test purposes from the audition
     * @param auditionOrgId is the unique id of the audition to assign the room to
     * @param theoryTest is the theory test
     * @param room is the room identifier
     * @returns true if there were no errors
     */
    public static bool DeleteJudgeTime(int auditionOrgId, int contactId, int scheduleId)
    {
        bool success = true;
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_JudgePrefTimeDelete";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@contactId", contactId);
            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);
            cmd.Parameters.AddWithValue("@schedule", scheduleId);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "DeleteJudgeTime", "auditionOrgId: " + auditionOrgId + ", contactId: " + contactId + ", scheduleId: " + scheduleId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            success = false;
        }

        connection.Close();

        return success;
    }

    #endregion Judges

    /*
     * Pre:
     * Post: Returns a data table containing the categories that do not
     *       have enough judges
     */
    public static DataTable ValidateEventJudges(int auditionOrgId)
    {
        DataTable table = new DataTable();
        SqlConnection connection = new
            SqlConnection(ConfigurationManager.ConnectionStrings["WmtaConnectionString"].ConnectionString);

        try
        {
            connection.Open();
            string storedProc = "sp_AuditionValidateJudgeMatrix";

            SqlCommand cmd = new SqlCommand(storedProc, connection);

            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@auditionOrgId", auditionOrgId);

            adapter.Fill(table);
        }
        catch (Exception e)
        {
            Utility.LogError("DbInterfaceScheduling", "ValidateEventJudges", "auditionOrgId: " + auditionOrgId,
                             "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            table = null;
        }

        connection.Close();

        return table;
    }
}