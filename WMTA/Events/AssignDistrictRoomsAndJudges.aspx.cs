﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace WMTA.Events
{
    public partial class AssignDistrictRoomsAndJudges : System.Web.UI.Page
    {
        private Audition audition;
        /* session variables */
        private string auditionSearch = "AuditionData"; //tracks data returned by latest audition search
        private string auditionSession = "Audition";
        private string roomsTable = "Rooms", theoryRoomsTable = "TheoryRoomsTable", judgesTable = "JudgesTable", judgeRoomsTable = "JudgeRoomsTable";
        private string theoryRooms = "TheoryRooms", judgeRooms = "JudgeRooms", auditionJudges = "AuditionJudges";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                checkPermissions();

                Session[auditionSearch] = null;
                Session[auditionSession] = null;
                Session[roomsTable] = null;
                Session[theoryRoomsTable] = null;
                Session[theoryRooms] = null;
                Session[judgeRooms] = null;


                loadYearDropdown();
                loadDistrictDropdown();
            }

            if (Page.IsPostBack && Session[auditionSession] != null)
            {
                audition = (Audition)Session[auditionSession];
            }

            // If there were rooms added before the postback, add them back to the table
            if (Page.IsPostBack && Session[roomsTable] != null)
            {
                TableRow[] rowArray = (TableRow[])Session[roomsTable];

                for (int i = 1; i < rowArray.Length; i++)
                    tblRooms.Rows.Add(rowArray[i]);
            }

            // If there were theoy rooms added, add them back to the table
            if (Page.IsPostBack && Session[theoryRoomsTable] != null)
            {
                TableRow[] rowArray = (TableRow[])Session[theoryRoomsTable];

                for (int i = 1; i < rowArray.Length; i++)
                    tblTheoryRooms.Rows.Add(rowArray[i]);
            }

            // Reload the available theory test rooms
            if (Page.IsPostBack && Session[theoryRooms] != null)
            {
                string selectedValue = ddlRoom.SelectedValue;
                ddlRoom.Items.Clear();

                ListItem[] itemArray = (ListItem[])Session[theoryRooms];

                for (int i = 0; i < itemArray.Length; i++)
                    ddlRoom.Items.Add(new ListItem(itemArray[i].Text));

                ddlRoom.SelectedValue = selectedValue;
            }

            // Reload the available judging rooms
            if (Page.IsPostBack && Session[judgeRooms] != null)
            {
                string selectedValue = ddlJudgeRoom.SelectedValue;
                ddlJudgeRoom.Items.Clear();

                ListItem[] itemArray = (ListItem[])Session[judgeRooms];

                for (int i = 0; i < itemArray.Length; i++)
                    ddlJudgeRoom.Items.Add(new ListItem(itemArray[i].Text));

                ddlJudgeRoom.SelectedValue = selectedValue;
            }

            // Reload the judges table
            if (Page.IsPostBack && Session[judgesTable] != null)
            {
                TableRow[] rowArray = (TableRow[])Session[judgesTable];

                for (int i = 1; i < rowArray.Length; i++)
                    tblJudges.Rows.Add(rowArray[i]);
            }

            // Reload audition judges dropdown
            if (Page.IsPostBack && Session[auditionJudges] != null)
            {
                string selectedValue = ddlAuditionJudges.SelectedValue;
                ddlAuditionJudges.Items.Clear();

                ListItem[] itemArray = (ListItem[])Session[auditionJudges];

                for (int i = 0; i < itemArray.Length; i++)
                    ddlAuditionJudges.Items.Add(new ListItem(itemArray[i].Text, itemArray[i].Value));

                ddlAuditionJudges.SelectedValue = selectedValue;
            }

            // Reload judge rooms table
            if (Page.IsPostBack && Session[judgeRoomsTable] != null)
            {
                TableRow[] rowArray = (TableRow[])Session[judgeRoomsTable];

                for (int i = 1; i < rowArray.Length; i++)
                    tblJudgeRooms.Rows.Add(rowArray[i]);
            }
        }

        /*
         * Pre:
         * Post: If the user is not logged in they will be redirected to the welcome screen
         */
        private void checkPermissions()
        {
            //if the user is not logged in, send them to login screen
            if (Session[Utility.userRole] == null)
                Response.Redirect("../Default.aspx");
            else
            {
                User user = (User)Session[Utility.userRole];

                if (!(user.permissionLevel.Contains("D") || user.permissionLevel.Contains("A")))
                    Response.Redirect("../Default.aspx");
            }
        }

        /*
         * Pre:
         * Post: Loads the appropriate years in the dropdown
         */
        private void loadYearDropdown()
        {
            int firstYear = DbInterfaceStudentAudition.GetFirstAuditionYear();

            for (int i = DateTime.Now.Year + 1; i >= firstYear; i--)
                ddlYear.Items.Add(new ListItem(i.ToString(), i.ToString()));
        }

        /*
         * Pre:
         * Post:  If the current user is not an administrator, the district
         *        dropdowns are filtered to containing only the current
         *        user's district
         */
        private void loadDistrictDropdown()
        {
            User user = (User)Session[Utility.userRole];

            if (!user.permissionLevel.Contains('A')) //if the user is a district admin, add only their district
            {
                //get own district dropdown info
                string districtName = DbInterfaceStudent.GetStudentDistrict(user.districtId);

                //add new items to dropdown
                ddlDistrictSearch.Items.Add(new ListItem(districtName, user.districtId.ToString()));

                ddlDistrictSearch.SelectedIndex = 1;

                //load the audition
                selectAudition();
            }
            else //if the user is an administrator, add all districts
            {
                ddlDistrictSearch.DataSource = DbInterfaceAudition.GetDistricts();

                ddlDistrictSearch.DataTextField = "GeoName";
                ddlDistrictSearch.DataValueField = "GeoId";

                ddlDistrictSearch.DataBind();
            }
        }

        /*
         * Pre:
         * Post: Perform an audition search with the input criteria.  Display results in gridview
         */
        protected void btnAuditionSearch_Click(object sender, EventArgs e)
        {
            int districtId = -1, year = -1;

            if (!ddlDistrictSearch.SelectedValue.ToString().Equals("")) districtId = Convert.ToInt32(ddlDistrictSearch.SelectedValue);
            if (!ddlYear.SelectedValue.ToString().Equals("")) year = Convert.ToInt32(ddlYear.SelectedValue);

            searchAuditions(gvAuditionSearch, districtId, year, auditionSearch);
        }

        /*
         * Pre:  id must be an integer or the empty string
         * Post: The input parameters are used to search for existing auditions.  Matchin audition
         *       information is displayed in the input gridview
         * @param gridview is the gridview in which the search results will be displayed
         * @param district is the district id of the audition being searched for
         * @param year is the year of the audition being searched for
         */
        private bool searchAuditions(GridView gridview, int districtId, int year, string session)
        {
            bool result = true;

            try
            {
                DataTable table = DbInterfaceAudition.GetAuditionSearchResults("", "", districtId, year);

                //If there are results in the table, display them.  Otherwise clear current
                //results and return false
                if (table != null && table.Rows.Count > 0)
                {
                    gridview.DataSource = table;
                    gridview.DataBind();

                    //save the data for quick re-binding upon paging
                    Session[session] = table;
                }
                else
                {
                    showInfoMessage("The search did not return any results.");
                    clearGridView(gridview);
                    result = false;
                }
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred during the search.");

                Utility.LogError("AssignDistrictRoomsAndJudges", "searchAuditions", "gridView: " + gridview.ID + ", districtId: " + districtId + ", year: " + year, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }

            return result;
        }

        protected void gvAuditionSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectAudition();
        }

        private void selectAudition()
        {
            User user = (User)Session[Utility.userRole];

            if (!user.permissionLevel.Contains('A') && ddlDistrictSearch.SelectedIndex > 0)
            {
                int year = DateTime.Today.Year;
                if (DateTime.Today.Month >= 6 && !Utility.reportSuffix.Equals("Test"))
                    year = year + 1;

                ddlYear.SelectedIndex = ddlYear.Items.IndexOf(ddlYear.Items.FindByValue(year.ToString()));

                loadAuditionData(DbInterfaceAudition.GetAuditionOrgId(Convert.ToInt32(ddlDistrictSearch.SelectedValue), year), year);
            }
            else if (user.permissionLevel.Contains('A'))
            {
                int index = gvAuditionSearch.SelectedIndex;

                if (index >= 0 && index < gvAuditionSearch.Rows.Count)
                {
                    int auditionId = Convert.ToInt32(gvAuditionSearch.Rows[index].Cells[1].Text);

                    //populate event information
                    ddlDistrictSearch.SelectedIndex =
                                ddlDistrictSearch.Items.IndexOf(ddlDistrictSearch.Items.FindByText(
                                gvAuditionSearch.Rows[index].Cells[2].Text));

                    ddlYear.SelectedIndex = ddlYear.Items.IndexOf(ddlYear.Items.FindByValue(
                                gvAuditionSearch.Rows[index].Cells[3].Text));

                    loadAuditionData(auditionId, Convert.ToInt32(ddlYear.SelectedValue));
                }
            }
        }

        /*
         * Pre:  audition must exist as the id of an audition in the system
         * Post: The existing data for the audition associated with the auditionId 
         *       is loaded to the page.
         * @param auditionId is the id of the audition being scheduled
         * @returns the audition data
         */
        private Audition loadAuditionData(int auditionId, int year)
        {
            try
            {
                audition = DbInterfaceAudition.LoadAuditionData(auditionId);

                //load data to page
                if (audition != null)
                {
                    txtIdHidden.Text = audition.auditionId.ToString();
                    lblAuditionSite.Text = audition.venue;
                    lblAuditionDate.Text = audition.auditionDate.ToShortDateString();

                    DataTable timePreferences = DbInterfaceAudition.LoadJudgeTimePreferenceOptions(auditionId);
                    chkLstTime.DataSource = timePreferences;
                    chkLstTime.DataBind();

                    LoadRooms(audition);
                    LoadTheoryRooms(audition);
                    LoadAvailableJudgesToDropdown(audition);
                    LoadAuditionJudges(audition);
                    LoadAuditionJudgeRooms(audition);

                    Session[auditionSession] = audition;
                    pnlMain.Visible = true;
                    upAuditionSearch.Visible = false;
                }
                else
                {
                    showErrorMessage("Error: The audition information could not be loaded. Please ensure one has been created for " + year + ".");
                }
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred while loading the audition data.");

                Utility.LogError("Assign District Rooms and Judges", "loadAuditionData", "auditionId: " + auditionId, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }

            return audition;
        }

        /*
         * Pre:
         * Post: Load all available rooms for an existing audition
         */
        private void LoadRooms(Audition audition)
        {
            ClearRooms();

            try
            {
                List<string> rooms = audition.GetRooms(true);

                // Load each room to the table and all room dropdowns
                foreach (string room in rooms)
                {
                    AddRoom(room);
                }

                if (rooms != null && rooms.Count > 0)
                    pnlRooms.Visible = true;
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred while loading the event's rooms.");
                Utility.LogError("Assign District Rooms and Judges", "loadRooms", "auditionId: " + audition.auditionId, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }
        }

        /*
         * Pre:
         * Post: Load the theory rooms for an existing audition
         */
        private void LoadTheoryRooms(Audition audition)
        {
            ClearTheoryRooms();

            try
            {
                List<Tuple<string, string>> theoryRooms = audition.GetTheoryRooms(true);

                // Load each room to the table
                foreach (Tuple<string, string> room in theoryRooms)
                {
                    AddTheoryRoom(room.Item1, room.Item2);
                }

                if (theoryRooms != null & theoryRooms.Count > 0)
                    pnlTheoryRooms.Visible = true;
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred while loading the event's theory test rooms.");
                Utility.LogError("Assign District Rooms and Judges", "loadTheoryRooms", "auditionId: " + audition.auditionId, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }
        }

        /*
         * Pre:
         * Post: Load the judges for the audition's district
         */
        private void LoadAvailableJudgesToDropdown(Audition audition)
        {
            ClearAvailableJudges();

            try
            {
                List<Judge> judges = audition.GetAvailableJudges(true);

                // Load each judge to the dropdown
                foreach (Judge judge in judges)
                {
                    ddlJudge.Items.Add(new ListItem(judge.lastName + ", " + judge.firstName, judge.id.ToString()));
                }
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred while loading the district's judges.");
                Utility.LogError("AssignDistrictRoomsAndJudges", "LoadAvailableJudgesToDropdown", "auditionId: " + audition.auditionId, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }
        }

        /*
         * Pre:
         * Post: Load the judges for an existing audition
         */
        private void LoadAuditionJudges(Audition audition)
        {
            ClearAuditionJudges();

            try
            {
                List<Judge> judges = audition.GetEventJudges(true);

                //Load each judge to the table
                foreach (Judge judge in judges)
                {
                    AddJudge(judge.id.ToString(), judge.lastName + ", " + judge.firstName);

                    pnlJudges.Visible = true;
                }
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred while loading the event's judges.");
                Utility.LogError("AssignDistrictRoomsAndJudges", "LoadAuditionJudges", "auditionId: " + audition.auditionId, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }
        }

        /*
         * Pre:
         * Post: Load the judge rooms for the input audition
         */
        private void LoadAuditionJudgeRooms(Audition audition)
        {
            ClearAuditionRooms();

            try
            {
                List<JudgeRoomAssignment> roomAssignments = audition.GetEventJudgeRoomAssignments(true);

                //Load each room assignment to the table
                JudgeRoomAssignment[] arr = roomAssignments.ToArray(); // Copy to array because positions are shifting around in list for some reason
                for (int i = 0; i < arr.Count(); i++)
                {
                    JudgeRoomAssignment assignment = arr[i];
                    AddJudgeRoom(assignment.judge.id.ToString(), assignment.judge.lastName + ", " + assignment.judge.firstName, assignment.room, assignment.times, assignment.scheduleOrder);
                }

                if (roomAssignments.Count > 0) pnlJudgeRooms.Visible = true;
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred while loading the event's judging rooms.");
                Utility.LogError("AssignDistrictRoomsAndJudges", "LoadAuditionRooms", "auditionId: " + audition.auditionId, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }
        }

        /*
         * Pre:
         * Post: Add the new room to the table if it doesn't already exist
         */
        protected void btnAddRoom_Click(object sender, EventArgs e)
        {
            string room = txtRoom.Text;

            if (!room.Equals("") && !RoomExists(room))
            {
                AddRoom(room);

                txtRoom.Text = "";
                pnlRooms.Visible = true;
            }
            else if (room.Equals(""))
            {
                showWarningMessage("Please enter a room name.");
            }
        }

        /*
         * Pre:
         * Post: Adds the new theory test room to the table, if the test
         *       has not already been assigned a room
         */
        protected void btnAddTestRoom_Click(object sender, EventArgs e)
        {
            string test = ddlTheoryTest.SelectedValue.ToString();
            string room = ddlRoom.SelectedValue.ToString();
            room = ddlRoom.Text;

            if (test.Equals("All") && !room.Equals("")) // add all tests to the current room
            {
                // Remove all existing rows
                for (int i = 1; i < tblTheoryRooms.Rows.Count; i++)
                {
                    string currentTest = tblTheoryRooms.Rows[i].Cells[1].Text;
                    string currentRoom = tblTheoryRooms.Rows[i].Cells[2].Text;

                    // Remove from the table and audition
                    tblTheoryRooms.Rows.Remove(tblTheoryRooms.Rows[i]);
                    audition.RemoveTheoryRoom(currentTest, currentRoom);

                    i--;
                }

                // Add all theory tests to table with current room
                for (int i = 0; i < ddlTheoryTest.Items.Count; i++)
                {
                    string currentTest = ddlTheoryTest.Items[i].Value;

                    if (!currentTest.Equals("") && !currentTest.Equals("All"))
                        AddTheoryRoom(currentTest, room);
                }

                ddlTheoryTest.SelectedIndex = -1;
                ddlRoom.SelectedIndex = -1;
                pnlTheoryRooms.Visible = true;
            }
            else if (!test.Equals("") && !room.Equals("") && !TheoryTestExists(test))
            {
                AddTheoryRoom(test, room);

                ddlTheoryTest.SelectedIndex = -1;
                ddlRoom.SelectedIndex = -1;
                pnlTheoryRooms.Visible = true;
            }
            else if (test.Equals(""))
            {
                showWarningMessage("Please select a theory test.");
            }
            else if (room.Equals(""))
            {
                showWarningMessage("Please select a room for the theory test.");
            }
        }

        /*
         * Pre:
         * Post: Add the new judge to the judges table and list of judges
         *       in the Judge Rooms section
         */
        protected void btnAddJudge_Click(object sender, EventArgs e)
        {
            string id = ddlJudge.SelectedValue;
            string name = ddlJudge.SelectedItem.Text;

            if (!id.Equals("") && !JudgeExists(id))
            {
                AddJudge(id, name);

                ddlJudge.SelectedIndex = -1;
                pnlJudges.Visible = true;
            }
            else if (id.Equals(""))
            {
                showWarningMessage("Please select a judge.");
            }
        }

        /*
         * Pre:
         * Post: Assign a new judge to a room and add that assignment to table
         */
        protected void btnAddJudgeRoom_Click(object sender, EventArgs e)
        {
            if (ddlAuditionJudges.SelectedIndex > 0 && ddlJudgeRoom.SelectedIndex > 0 && chkLstTime.SelectedIndex >= 0)
            {
                string judgeId = ddlAuditionJudges.SelectedValue.ToString();
                string judge = ddlAuditionJudges.SelectedItem.Text;
                string room = ddlJudgeRoom.SelectedValue.ToString();
                List<Tuple<int, string>> times = new List<Tuple<int, string>>();
                List<string> timeIds = new List<string>();

                //Get schedule order/priority if one was entered
                int priority = -1;
                if (Int32.TryParse(txtSchedulePriority.Text, out priority))
                {
                    if (priority <= 0)
                    {
                        priority = -1;
                        showWarningMessage("Schedule Order must be a positive number.");
                    }
                }

                // Get selected times
                foreach (ListItem time in chkLstTime.Items)
                {
                    if (time.Selected)
                    {
                        string timeId = time.Value;
                        string timeStr = time.Text;

                        times.Add(new Tuple<int, string>(Convert.ToInt32(timeId), timeStr));
                        timeIds.Add(time.Value);
                    }
                }

                bool duplicatePriority = DuplicateJudgePriority(judgeId, priority);
                bool judgesOverlap = JudgesOverlap(judgeId, room, timeIds);
                bool judgeRoomExists = JudgeRoomExists(judgeId, room);
                bool judgeTimeExists = JudgeTimeExists(judgeId, room, timeIds);

                if (duplicatePriority)
                {
                    showWarningMessage("There is another judge with the specified schedule order.");
                }
                else if (judgesOverlap)
                {
                    showWarningMessage("There is another judge scheduled in the selected room at the same time.");
                }
                else if (judgeRoomExists) // Update the times for the room
                {
                    UpdateJudgeRoom(judgeId, room, times, priority);

                    ddlAuditionJudges.SelectedIndex = -1;
                    ddlJudgeRoom.SelectedIndex = -1;
                    txtSchedulePriority.Text = "";
                    foreach (ListItem item in chkLstTime.Items)
                        item.Selected = true;
                }
                else if (judgeTimeExists) // Judge has been assigned a duplicate time in a different room, show error
                {
                    showWarningMessage("The judge has been assigned to a different room for one or more of the specified times.");
                }
                else // Add new judge (can have multiple entries in the table for different rooms)
                {
                    AddJudgeRoom(judgeId, judge, room, times, priority);

                    ddlAuditionJudges.SelectedIndex = -1;
                    ddlJudgeRoom.SelectedIndex = -1;
                    txtSchedulePriority.Text = "";
                    foreach (ListItem item in chkLstTime.Items)
                        item.Selected = true;
                    pnlJudgeRooms.Visible = true;
                }
            }
            else if (ddlAuditionJudges.SelectedIndex <= 0)
            {
                showWarningMessage("Please select a judge.");
            }
            else if (ddlJudgeRoom.SelectedIndex <= 0)
            {
                showWarningMessage("Please select a room for the judge.");
            }
            else if (chkLstTime.SelectedIndex < 0)
            {
                showWarningMessage("Please select at least one time for the judge to be scheduled.");
            }
        }

        /*
         * Pre:
         * Post: Remove selected rooms
         */
        protected void btnRemoveRoom_Click(object sender, EventArgs e)
        {
            bool roomSelected = false, roomScheduled = false;

            // Remove any checked rows, unless a judge or test is scheduled in the room
            for (int i = 1; i < tblRooms.Rows.Count; i++)
            {
                string room = tblRooms.Rows[i].Cells[1].Text;

                if (((CheckBox)tblRooms.Rows[i].Cells[0].Controls[0]).Checked)
                {
                    roomScheduled = RoomScheduled(room);

                    if (!roomScheduled)
                    {
                        // Remove from table
                        tblRooms.Rows.Remove(tblRooms.Rows[i]);

                        // Remove from dropdowns
                        ddlRoom.Items.Remove(new ListItem(room, room));
                        ddlJudgeRoom.Items.Remove(new ListItem(room, room));

                        //Remove from audition
                        audition.RemoveRoom(room);

                        roomSelected = true;
                        i--;
                    }
                }
            }

            // Display a message if no room was selected
            if (!roomSelected && !roomScheduled)
            {
                showWarningMessage("Please select a room to remove.");
            }
            else // Save changes
            {
                saveTableToSession(tblRooms, roomsTable);
                saveDropdownToSession(ddlRoom, theoryRooms);
                saveDropdownToSession(ddlJudgeRoom, judgeRooms);
                Session[auditionSession] = audition;
            }
        }

        /*
         * Pre:
         * Post: Remove selected theory test rooms
         */
        protected void btnRemoveTestRoom_Click(object sender, EventArgs e)
        {
            bool roomSelected = false;

            // Remove any checked rows
            for (int i = 1; i < tblTheoryRooms.Rows.Count; i++)
            {
                if (((CheckBox)tblTheoryRooms.Rows[i].Cells[0].Controls[0]).Checked)
                {
                    string test = tblTheoryRooms.Rows[i].Cells[1].Text;
                    string room = tblTheoryRooms.Rows[i].Cells[2].Text;

                    tblTheoryRooms.Rows.Remove(tblTheoryRooms.Rows[i]);

                    // Remove from the audition
                    audition.RemoveTheoryRoom(test, room);

                    roomSelected = true;
                    i--;
                }
            }

            // Display a message if no room was selected
            if (!roomSelected)
            {
                showWarningMessage("Please select a theory test room to remove.");
            }
            else // Save changes
            {
                saveTableToSession(tblTheoryRooms, theoryRoomsTable);
                Session[auditionSession] = audition;
            }
        }

        /*
         * Pre:
         * Post: Removes the selected judge from the judges table and
         *       the judge dropdown in the Judge Rooms section
         */
        protected void btnRemoveJudge_Click(object sender, EventArgs e)
        {
            bool judgeSelected = false, judgeScheduled = false, messageShown = false;

            // Remove any checked judges
            for (int i = 1; i < tblJudges.Rows.Count; i++)
            {
                string contactId = tblJudges.Rows[i].Cells[1].Text;
                judgeScheduled = JudgeScheduled(contactId);

                if (((CheckBox)tblJudges.Rows[i].Cells[0].Controls[0]).Checked && !judgeScheduled)
                {
                    string name = tblJudges.Rows[i].Cells[2].Text;

                    // Remove from dropdown
                    ddlAuditionJudges.Items.Remove(new ListItem(name, contactId));

                    // Remove from table
                    tblJudges.Rows.Remove(tblJudges.Rows[i]);

                    // Remove from audition
                    audition.RemoveJudge(Convert.ToInt32(contactId));

                    judgeSelected = true;
                    i--;
                }
                else if (((CheckBox)tblJudges.Rows[i].Cells[0].Controls[0]).Checked && judgeScheduled)
                {
                    showWarningMessage("A selected judge has been scheduled in a room. Please edit the judge rooms to continue.");
                    messageShown = true;
                }
            }

            // Hide the table if there are no judges left
            if (tblJudges.Rows.Count == 1)
            {
                pnlJudges.Visible = false;
            }

            // Display a message if no judge was selected
            if (!judgeSelected && !judgeScheduled && !messageShown)
            {
                showWarningMessage("Please select a judge to remove.");
            }
            else // Save changes
            {
                saveTableToSession(tblJudges, judgesTable);
                saveDropdownToSession(ddlAuditionJudges, auditionJudges);
                Session[auditionSession] = audition;
            }
        }

        /*
         * Pre:
         * Post: Removes the selected row(s) from the judge rooms table
         */
        protected void btnRemoveJudgeRoom_Click(object sender, EventArgs e)
        {
            bool rowSelected = false;

            // Remove any checked rows
            for (int i = 1; i < tblJudgeRooms.Rows.Count; i++)
            {
                if (((CheckBox)tblJudgeRooms.Rows[i].Cells[0].Controls[0]).Checked)
                {
                    string contactId = tblJudgeRooms.Rows[i].Cells[1].Text;
                    string room = tblJudgeRooms.Rows[i].Cells[3].Text;
                    string scheduleOrder = !tblJudgeRooms.Rows[i].Cells[6].Text.Equals("") ? tblJudgeRooms.Rows[i].Cells[6].Text : "-1";
                    string[] timeIds = tblJudgeRooms.Rows[i].Cells[4].Text.Split(',');
                    string[] times = tblJudgeRooms.Rows[i].Cells[5].Text.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);

                    tblJudgeRooms.Rows.Remove(tblJudgeRooms.Rows[i]);

                    // Remove from audition
                    List<Tuple<int, string>> timeList = new List<Tuple<int, string>>();
                    for (int j = 0; j < timeIds.Length; j++)
                    {
                        Tuple<int, string> time = new Tuple<int, string>(Convert.ToInt32(timeIds[j]), times[j]);
                        timeList.Add(time);
                    }

                    audition.RemoveJudgeRoom(Convert.ToInt32(contactId), room, timeList, Convert.ToInt32(scheduleOrder));

                    rowSelected = true;
                    i--;
                }
            }

            // Hide the table if there are no rows left
            if (tblJudgeRooms.Rows.Count == 1)
            {
                pnlJudgeRooms.Visible = false;
            }

            // Display a message if no row was selected
            if (!rowSelected)
            {
                showWarningMessage("Please select a row to remove.");
            }
            else // Save changes
            {
                saveTableToSession(tblJudgeRooms, judgeRoomsTable);
                Session[auditionSession] = audition;
            }
        }

        /*
         * Pre:
         * Post: Determines whether or not the input room is being scheduled
         *       for a judge or theory test and displays a message, if necessary
         * @returns true if the room is being used and false otherwise
         */
        private bool RoomScheduled(string room)
        {
            bool roomUsed = false;
            int idx = 1;

            // Check theory test rooms
            while (!roomUsed && idx < tblTheoryRooms.Rows.Count)
            {
                if (tblTheoryRooms.Rows[idx].Cells[2].Text.Equals(room))
                {
                    showWarningMessage(room + " is scheduled for a theory test and cannot be removed. Please edit the theory test rooms to continue.");
                    roomUsed = true;
                }

                idx++;
            }

            // Check judge rooms
            idx = 1;
            while (!roomUsed && idx < tblJudgeRooms.Rows.Count)
            {
                if (tblJudgeRooms.Rows[idx].Cells[3].Text.Equals(room))
                {
                    showWarningMessage(room + " is scheduled for a judge and cannot be removed. Please edit the judge rooms to continue.");
                    roomUsed = true;
                }
                idx++;
            }

            return roomUsed;
        }

        /*
         * Pre:
         * Post: Determines whether or not the input judge has been scheduled and displays a message, if necessary
         * @returns true if the judge has been scheduled and false otherwise
         */
        private bool JudgeScheduled(string contactId)
        {
            bool judgeScheduled = false;
            int idx = 1;

            // Check judge/room schedule
            while (!judgeScheduled && idx < tblJudgeRooms.Rows.Count)
            {
                if (tblJudgeRooms.Rows[idx].Cells[1].Text.Equals(contactId))
                {
                    showWarningMessage("The judge with id " + contactId + " has been scheduled in a room. Please edit the judge rooms to continue.");
                    judgeScheduled = true;
                }

                idx++;
            }

            return judgeScheduled;
        }

        /*
         * Pre:
         * Post: Add a row to the rooms table and dropdowns with the specified room name
         */
        private void AddRoom(string room)
        {
            TableRow row = new TableRow();
            TableCell chkBoxCell = new TableCell();
            TableCell roomCell = new TableCell();
            CheckBox chkBox = new CheckBox();

            // Add a checkbox to column 1
            chkBoxCell.Controls.Add(chkBox);

            // Add the room name to column 2
            roomCell.Text = room;

            // Add the cells to the new row
            row.Cells.Add(chkBoxCell);
            row.Cells.Add(roomCell);

            // Add the new row to the table
            tblRooms.Rows.Add(row);

            // Add the room to the theory and judging rooms dropdown
            ddlRoom.Items.Add(new ListItem(room));
            ddlJudgeRoom.Items.Add(new ListItem(room));

            // Add the room to the audition
            audition.AddRoom(room);

            // Save the updated table, dropdowns, and audition to the session
            saveTableToSession(tblRooms, roomsTable);
            saveDropdownToSession(ddlRoom, theoryRooms);
            saveDropdownToSession(ddlJudgeRoom, judgeRooms);
            Session[auditionSession] = audition;
        }

        /*
         * Pre: 
         * Post: Add a row the theory rooms table for the specified test and room
         */
        private void AddTheoryRoom(string theoryTest, string room)
        {
            TableRow row = new TableRow();
            TableCell chkBoxCell = new TableCell();
            TableCell testCell = new TableCell();
            TableCell roomCell = new TableCell();
            CheckBox chkBox = new CheckBox();

            // Add a checkbox to column 1
            chkBoxCell.Controls.Add(chkBox);

            // Add the test and room names to columns 2 and 3
            testCell.Text = theoryTest;
            roomCell.Text = room;

            // Add the cells to the new row
            row.Cells.Add(chkBoxCell);
            row.Cells.Add(testCell);
            row.Cells.Add(roomCell);

            // Add the new row to the table
            tblTheoryRooms.Rows.Add(row);

            //Add the room to the audition
            audition.AddTheoryRoom(theoryTest, room);

            // Save the updated table and audition to the session
            saveTableToSession(tblTheoryRooms, theoryRoomsTable);
            Session[auditionSession] = audition;
        }

        /*
         * Pre:
         * Post: Add a new row to the judges table
         */
        private void AddJudge(string contactId, string name)
        {
            TableRow row = new TableRow();
            TableCell chkBoxCell = new TableCell();
            TableCell idCell = new TableCell();
            TableCell nameCell = new TableCell();
            CheckBox chkBox = new CheckBox();

            // Add a checkbox to column 1
            chkBoxCell.Controls.Add(chkBox);

            // Add the id and name to columns 2 and 3
            idCell.Text = contactId;
            nameCell.Text = name;

            // Add the cells to the new row
            row.Cells.Add(chkBoxCell);
            row.Cells.Add(idCell);
            row.Cells.Add(nameCell);

            // Add the new row to the table
            tblJudges.Rows.Add(row);

            // Add the new judge to the judges dropdown in the Judge Rooms section
            ddlAuditionJudges.Items.Add(new ListItem(name, contactId));

            // Add the judge to the audition
            audition.AddJudge(Convert.ToInt32(contactId));

            // Save the updated table, dropdown, and audition to the session
            saveTableToSession(tblJudges, judgesTable);
            saveDropdownToSession(ddlAuditionJudges, auditionJudges);
            Session[auditionSession] = audition;
        }

        /*
         * Pre:
         * Post: Add a new row to the judge times table
         */
        private void AddJudgeRoom(string judgeId, string judge, string room, List<Tuple<int, string>> times, int priority)
        {
            TableRow row = new TableRow();
            TableCell chkBoxCell = new TableCell();
            TableCell judgeIdCell = new TableCell();
            TableCell judgeCell = new TableCell();
            TableCell roomCell = new TableCell();
            TableCell timeIdCell = new TableCell();
            TableCell timeCell = new TableCell();
            TableCell priorityCell = new TableCell();
            CheckBox chkBox = new CheckBox();

            // Add a checkbox to column 1
            chkBoxCell.Controls.Add(chkBox);

            // Add columns for judge info, the room, and time info
            judgeIdCell.Text = judgeId;
            judgeCell.Text = judge;
            roomCell.Text = room;

            string timeIds = "", timeStr = "";
            foreach (Tuple<int, string> timeInfo in times)
            {
                if (timeIds.Equals(""))
                {
                    timeIds = timeInfo.Item1.ToString();
                    timeStr = timeInfo.Item2;
                }
                else
                {
                    timeIds += "," + timeInfo.Item1;
                    timeStr += ", " + timeInfo.Item2;
                }
            }

            timeIdCell.Text = timeIds;
            timeIdCell.Visible = false;
            timeCell.Text = timeStr;

            if (priority > 0)
                priorityCell.Text = priority.ToString();

            // Add the cells to the new row
            row.Cells.Add(chkBoxCell);
            row.Cells.Add(judgeIdCell);
            row.Cells.Add(judgeCell);
            row.Cells.Add(roomCell);
            row.Cells.Add(timeIdCell);
            row.Cells.Add(timeCell);
            row.Cells.Add(priorityCell);

            // Add the new row to the table
            tblJudgeRooms.Rows.Add(row);

            // Add the assignment to the audition
            audition.AddJudgeRoom(Convert.ToInt32(judgeId), room, times, priority);

            // Save the updated table and audition to the session
            saveTableToSession(tblJudgeRooms, judgeRoomsTable);
            Session[auditionSession] = audition;
        }

        /*
         * Pre:
         * Post: Update the judge assignment identified by the judge id and room
         */
        private void UpdateJudgeRoom(string judgeId, string room, List<Tuple<int, string>> times, int priority)
        {
            bool found = false;
            int i = 1;

            while (!found && i < tblJudgeRooms.Rows.Count)
            {
                // If the judge id and room match, update the times in the current row
                if (tblJudgeRooms.Rows[i].Cells[1].Text.Equals(judgeId) && tblJudgeRooms.Rows[i].Cells[3].Text.Equals(room))
                {
                    // Construct time strings
                    string timeIds = "", timeStr = "";
                    foreach (Tuple<int, string> timeInfo in times)
                    {
                        if (timeIds.Equals(""))
                        {
                            timeIds = timeInfo.Item1.ToString();
                            timeStr = timeInfo.Item2;
                        }
                        else
                        {
                            timeIds += "," + timeInfo.Item1;
                            timeStr += ", " + timeInfo.Item2;
                        }
                    }

                    tblJudgeRooms.Rows[i].Cells[4].Text = timeIds;
                    tblJudgeRooms.Rows[i].Cells[5].Text = timeStr;

                    if (priority > 0)
                        tblJudgeRooms.Rows[i].Cells[6].Text = priority.ToString();

                    // Add the assignment to the audition
                    audition.AddJudgeRoom(Convert.ToInt32(judgeId), room, times, priority);

                    found = true;
                }

                i++;
            }

            if (found)
            {
                saveTableToSession(tblJudgeRooms, judgeRoomsTable);
                Session[auditionSession] = audition;
            }
        }

        /*
         * Pre:
         * Post: Determine if the input room is already in the table
         * @returns true if it exists and false otherwise
         */
        private bool RoomExists(string room)
        {
            for (int i = 0; i < tblRooms.Rows.Count; i++)
            {
                if (tblRooms.Rows[i].Cells[1].Text.Equals(room))
                {
                    showInfoMessage("The specified room has already been added.");
                    return true;
                }
            }

            return false;
        }

        /*
         * Pre:
         * Post: Determine if the input test is already in the table
         * @returns true if it exists and false otherwise
         */
        private bool TheoryTestExists(string test)
        {
            for (int i = 0; i < tblTheoryRooms.Rows.Count; i++)
            {
                if (tblTheoryRooms.Rows[i].Cells[1].Text.Equals(test))
                {
                    showInfoMessage("The specified theory test has already been added.");
                    return true;
                }
            }

            return false;
        }

        /*
         * Pre:
         * Post: Determine if the input judge is already in the table
         * @return true if the judge exists and false otherwise
         */
        private bool JudgeExists(string contactId)
        {
            for (int i = 0; i < tblJudges.Rows.Count; i++)
            {
                if (tblJudges.Rows[i].Cells[1].Text.Equals(contactId))
                {
                    showInfoMessage("The selected judge has already been added.");
                    return true;
                }
            }

            return false;
        }

        /*
         * Pre:
         * Post: Determines whether or not adding/updating this judge will cause
         *       a duplicated priority/schedule order
         */
        private bool DuplicateJudgePriority(string judgeId, int priority)
        {
            bool duplicate = false;
            int i = 1;

            while (!duplicate && i < tblJudgeRooms.Rows.Count)
            {
                if (!tblJudgeRooms.Rows[i].Cells[1].Text.Equals(judgeId) && tblJudgeRooms.Rows[i].Cells[6].Text.Equals(priority.ToString()))
                {
                    duplicate = true;
                }

                i++;
            }

            return duplicate;
        }

        /*
         * Pre:
         * Post: Determines whether or not adding this judge assignment will cause any
         *       scheduling problems, i.e. two judges being in the same room at the same time
         */
        private bool JudgesOverlap(string judgeId, string room, List<string> timeIds)
        {
            bool overlap = false;
            int i = 1;

            while (!overlap && i < tblJudgeRooms.Rows.Count)
            {
                if (!tblJudgeRooms.Rows[i].Cells[1].Text.Equals(judgeId) && tblJudgeRooms.Rows[i].Cells[3].Text.Equals(room))
                {
                    string[] times = tblJudgeRooms.Rows[i].Cells[4].Text.Split(',');
                    foreach (string time in timeIds)
                    {
                        if (times.Contains(time))
                            overlap = true;
                    }
                }

                i++;
            }

            return overlap;
        }

        /*
         * Pre:
         * Post: Determine if the input judge has already been assigned to the specified room.
         *       If the judge has already been assigned to this room, the times will just be updated.
         */
        private bool JudgeRoomExists(string judgeId, string room)
        {
            bool exists = false;
            int i = 1;

            while (!exists && i < tblJudgeRooms.Rows.Count)
            {
                if (tblJudgeRooms.Rows[i].Cells[1].Text.Equals(judgeId) && tblJudgeRooms.Rows[i].Cells[3].Text.Equals(room))
                {
                    exists = true;
                }

                i++;
            }

            return exists;
        }

        /*
         * Pre:
         * Post: Determine if the input judge has already been assigned to the specified room
         *       If the judge has already been assigned this time in a different room, an error message should be shown
         */
        private bool JudgeTimeExists(string judgeId, string room, List<string> timeIds)
        {
            bool exists = false;
            int i = 1;

            while (!exists && i < tblJudgeRooms.Rows.Count)
            {
                if (tblJudgeRooms.Rows[i].Cells[1].Text.Equals(judgeId) && !tblJudgeRooms.Rows[i].Cells[3].Text.Equals(room))
                {
                    foreach (string timeId in timeIds)
                    {
                        if (tblJudgeRooms.Rows[i].Cells[4].Text.Contains(timeId))
                        {
                            exists = true;
                        }
                    }
                }

                i++;
            }

            return exists;
        }

        /*
         * Pre:
         * Post: Clear selected times when a new judge is selected
         */
        protected void ddlAuditionJudges_SelectedIndexChanged1(object sender, EventArgs e)
        {
            ddlJudgeRoom.SelectedIndex = -1;

            foreach (ListItem chkBox in chkLstTime.Items)
            {
                chkBox.Selected = true;
            }
        }

        private void PageIndexChanging(GridView gv, GridViewPageEventArgs e)
        {
            gv.PageIndex = e.NewPageIndex;
            BindSessionData();
        }

        /*
         * Pre:   The audition search table must have been previously defined
         * Post:  The stored data is bound to the gridView
         */
        protected void BindSessionData()
        {
            try
            {
                DataTable data = (DataTable)Session[auditionSearch];
                gvAuditionSearch.DataSource = data;
                gvAuditionSearch.DataBind();
            }
            catch (Exception e)
            {
                Utility.LogError("AssignDistrictRoomsAndJudges", "BindSessionData", "", "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }
        }

        /*
         * Pre:  The input must be a gridview that exists on the current page
         * Post: The background of the header row is set
         * @param gv is the gridView that will have its header row color changed
         * @param e are the event args for the event fired by the row being bound to data
         */
        private void setHeaderRowColor(GridView gv, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                foreach (TableCell cell in gv.HeaderRow.Cells)
                {
                    cell.BackColor = Color.Black;
                    cell.ForeColor = Color.White;
                }
            }
        }

        /*
         * Pre:
         * Post: The table in the input is saved to a session variable
         * @table is the table being saved
         * @session is the name of the session variable
         */
        private void saveTableToSession(Table table, string session)
        {
            TableRow[] rowArray = new TableRow[table.Rows.Count];
            table.Rows.CopyTo(rowArray, 0);
            Session[session] = rowArray;
        }

        /*
         * Pre:
         * Post: The dropdown list in the input is saved to a session variable
         * @ddl is the dropdown list being saved
         * @session is the name of the session variable
         */
        private void saveDropdownToSession(DropDownList ddl, string session)
        {
            ListItem[] itemArray = new ListItem[ddl.Items.Count];
            ddl.Items.CopyTo(itemArray, 0);
            Session[session] = itemArray;
        }

        #region gridview events
        protected void gvAuditionSearch_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            PageIndexChanging(gvAuditionSearch, e);
        }

        protected void gvAuditionSearch_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            setHeaderRowColor(gvAuditionSearch, e);
        }

        #endregion gridview events

        /*
         * Pre:
         * Post: Save the changes to the audition schedule information
         */
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            try
            {
                bool success = audition.SaveScheduleData();

                if (success)
                {
                    ClearPage();
                    showSuccessMessage("The scheduling data has been successfully saved.");
                }
                else
                {
                    showErrorMessage("Error: Some schedule data could not be saved.  Please verify the entered information.");
                }
            }
            catch (Exception ex)
            {
                Utility.LogError("AssignDistrictRoomsAndJudges", "btnSubmit_Click", "", "Message: " + ex.Message + "   Stack Trace: " + ex.StackTrace, -1);
            }
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearPage();
        }

        /*
         * Pre:
         * Post: Clear the search fields
         */
        protected void btnClearAuditionSearch_Click(object sender, EventArgs e)
        {
            ClearSearch();
        }

        /*
         * Pre:
         * Post: Clear the search fields
         */
        private void ClearSearch()
        {
            ddlDistrictSearch.SelectedIndex = 0;
            ddlYear.SelectedIndex = 0;
        }

        /*
         * Pre: The GridView gv must exist on the current form
         * Post:  The data binding of the GridView is cleared, causing the table to be cleared
         * @param gv is the GridView to be cleared
         */
        private void clearGridView(GridView gv)
        {
            gv.DataSource = null;
            gv.DataBind();
        }

        private void ClearPage()
        {
            ClearSearch();
            ClearRooms();
            ClearTheoryRooms();
            ClearAvailableJudges();
            ClearAuditionJudges();
            ClearAuditionRooms();

            // Clear inputs
            txtRoom.Text = "";
            ddlTheoryTest.SelectedIndex = -1;
            ddlRoom.SelectedIndex = -1;
            ddlJudge.SelectedIndex = -1;
            ddlAuditionJudges.SelectedIndex = -1;
            ddlJudgeRoom.SelectedIndex = -1;
            txtSchedulePriority.Text = "";
            foreach (ListItem item in chkLstTime.Items)
                item.Selected = true;

            // Clear event information
            lblAuditionSite.Text = "";
            lblAuditionDate.Text = "";

            // Go back to search panel
            pnlMain.Visible = false;
            clearGridView(gvAuditionSearch);
            upAuditionSearch.Visible = true;
        }

        private void ClearRooms()
        {
            // Clear the rooms table
            while (tblRooms.Rows.Count > 1)
            {
                tblRooms.Rows.RemoveAt(1);
            }

            // Clear the theory test room dropdown
            ddlRoom.Items.Clear();
            ddlRoom.Items.Add(new ListItem(""));

            // Clear the judging room dropdown
            ddlJudgeRoom.Items.Clear();
            ddlJudgeRoom.Items.Add(new ListItem(""));

            pnlRooms.Visible = false;

            // Save changes to session
            saveTableToSession(tblRooms, roomsTable);
            saveDropdownToSession(ddlRoom, theoryRooms);
            saveDropdownToSession(ddlJudgeRoom, judgeRooms);
        }

        private void ClearTheoryRooms()
        {
            // Clear the theory rooms table
            while (tblTheoryRooms.Rows.Count > 1)
            {
                tblTheoryRooms.Rows.RemoveAt(1);
            }

            pnlTheoryRooms.Visible = false;

            // Save changes to session
            saveTableToSession(tblTheoryRooms, theoryRoomsTable);
        }

        private void ClearAvailableJudges()
        {
            ddlJudge.Items.Clear();
            ddlJudge.Items.Add(new ListItem(""));
        }

        private void ClearAuditionJudges()
        {
            // Clear the judges table
            while (tblJudges.Rows.Count > 1)
            {
                tblJudges.Rows.RemoveAt(1);
            }

            pnlJudges.Visible = false;

            // Clear the judges dropdown
            ddlAuditionJudges.Items.Clear();
            ddlAuditionJudges.Items.Add(new ListItem(""));

            // Save changes to session
            saveTableToSession(tblJudges, judgesTable);
        }

        private void ClearAuditionRooms()
        {
            // Clear the table
            while (tblJudgeRooms.Rows.Count > 1)
            {
                tblJudgeRooms.Rows.RemoveAt(1);
            }

            pnlJudgeRooms.Visible = false;

            // Save changes to session
            saveTableToSession(tblJudgeRooms, judgeRoomsTable);
        }

        /*
         * Pre:
         * Post: Displays the input error message in the top-left corner of the screen
         * @param message is the message text to be displayed
         */
        private void showErrorMessage(string message)
        {
            lblErrorMessage.InnerText = message;

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ShowError", "showMainError()", true);
        }

        /*
         * Pre: 
         * Post: Displays the input warning message in the top left corner of the screen
         * @param message is the message text to be displayed
         */
        private void showWarningMessage(string message)
        {
            lblWarningMessage.InnerText = message;

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ShowWarning", "showWarningMessage()", true);
        }

        /*
         * Pre: 
         * Post: Displays the input success message in the top left corner of the screen
         * @param message is the message text to be displayed
         */
        private void showSuccessMessage(string message)
        {
            lblSuccessMessage.InnerText = message;

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ShowSuccess", "showSuccessMessage()", true);
        }

        /*
         * Pre: 
         * Post: Displays the input informational message in the top left corner of the screen
         * @param message is the message text to be displayed
         */
        private void showInfoMessage(string message)
        {
            lblInfoMessage.InnerText = message;

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "ShowInfo", "showInfoMessage()", true);
        }

        /*
         * Catch unhandled exceptions, add information to error log
         */
        protected override void OnError(EventArgs e)
        {
            //Get last error from the server
            Exception exc = Server.GetLastError();

            //log exception
            Utility.LogError("Assign District Rooms and Judges", "OnError", "", "Message: " + exc.Message + "   Stack Trace: " + exc.StackTrace, -1);

            //Pass error on to error page
            Server.Transfer("ErrorPage.aspx", true);
        }

        protected void chkLstTime_DataBound(object sender, EventArgs e)
        {
            foreach (ListItem item in chkLstTime.Items)
            {
                item.Selected = true;
            }
        }
    }
}