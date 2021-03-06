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
    public partial class BadgerSchedule : System.Web.UI.Page
    {
        private string auditionSearch = "AuditionData"; //tracks data returned by latest audition search
        private string judgeValidation = "JudgeValidation";
        private string scheduleData = "ScheduleData";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                checkPermissions();

                Session[auditionSearch] = null;
                Session[judgeValidation] = null;
                Session[scheduleData] = null;
                loadYearDropdown();
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

                if (!(user.permissionLevel.Contains("S") || user.permissionLevel.Contains("A")))
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
         * Pre:  The AuditionId field must be empty or contain an integer
         * Post: Auditions the match the search criteria are displayed
         */
        protected void btnAuditionSearch_Click(object sender, EventArgs e)
        {
            int districtId = -1, year = -1;

            if (!ddlDistrictSearch.SelectedValue.ToString().Equals(""))
                districtId = Convert.ToInt32(ddlDistrictSearch.SelectedValue);

            if (!ddlYear.SelectedValue.ToString().Equals("")) year = Convert.ToInt32(ddlYear.SelectedValue);

            searchAuditions(gvAuditionSearch, districtId, year, auditionSearch);
        }

        /*
        * Pre:  id must be an integer or the empty string
        * Post: The input parameters are used to search for existing auditions.  Matchin audition
        *       information is displayed in the input gridview
        * @param gridview is the gridview in which the search results will be displayed
        * @param auditionType is the type of audition being searched for - district, badger keyboard, or badger Vocal/Instrumental
        * @param district is the district id of the audition being searched for
        * @param year is the year of the audition being searched for
        */
        private bool searchAuditions(GridView gridview, int districtId, int year, string session)
        {
            bool result = true;

            try
            {
                DataTable table = DbInterfaceAudition.GetAuditionSearchResults("", "State", districtId, year);

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
                    showInfoMessage("No events were found matching the search criteria.");

                    clearGridView(gridview);
                    result = false;
                }
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred during the search.");

                Utility.LogError("BadgerSchedule", "searchAuditions", "gridView: " + gridview + ", districtId: " +
                                 districtId + ", year: " + year + ", session: " + session, "Message: " + e.Message +
                                 "   StackTrace: " + e.StackTrace, -1);
            }

            return result;
        }

        protected void gvAuditionSearch_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectAudition();
        }

        private void selectAudition()
        {
            upAuditionSearch.Visible = false;

            int index = gvAuditionSearch.SelectedIndex;

            if (index >= 0 && index < gvAuditionSearch.Rows.Count)
            {
                ddlDistrictSearch.SelectedIndex =
                            ddlDistrictSearch.Items.IndexOf(ddlDistrictSearch.Items.FindByText(
                            gvAuditionSearch.Rows[index].Cells[2].Text));
                ddlYear.SelectedIndex = ddlYear.Items.IndexOf(ddlYear.Items.FindByValue(
                                        gvAuditionSearch.Rows[index].Cells[3].Text));

                lblAudition.Text = ddlDistrictSearch.SelectedItem.Text + " " + ddlYear.Text + " Schedule";
                lblAudition2.Text = lblAudition.Text;
                lblAudition3.Text = lblAudition.Text;
                lblAuditionId.Text = gvAuditionSearch.Rows[index].Cells[1].Text;

                loadSchedule(Convert.ToInt32(lblAuditionId.Text));
            }
        }

        /*
         * Pre:  audition must exist as the id of an audition in the system
         * Post: The existing data for the audition associated with the auditionId 
         *       is loaded to the page.
         * @param auditionId is the id of the audition being edited
         */
        private void loadSchedule(int auditionId)
        {
            try
            {
                DataTable table = DbInterfaceScheduling.ValidateEventJudges(auditionId);

                if (table != null && table.Rows.Count == 0) // No errors, give option to create schedule
                {
                    pnlCreateSchedule.Visible = true;

                    Session[judgeValidation] = table;
                }
                else if (table != null & table.Rows.Count > 0) // Display errors
                {
                    pnlValidateSchedule.Visible = true;

                    gvJudgeValidation.DataSource = table;
                    gvJudgeValidation.DataBind();

                    Session[judgeValidation] = table;
                }
                else
                {
                    upAuditionSearch.Visible = true;
                    showErrorMessage("Error: The event information could not be loaded.");
                    Session[judgeValidation] = null;
                }
            }
            catch (Exception e)
            {
                showErrorMessage("Error: An error occurred while loading the event data.");

                Utility.LogError("BadgerSchedule", "loadSchedule", "auditionId: " + auditionId, "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
            }

            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "RefreshDatepickers", "refreshDatePickers()", true);
        }

        /*
         * Pre:
         * Post: The scheduling routine is ran and the schedule is displayed
         */
        protected void btnCreateSchedule_Click(object sender, EventArgs e)
        {
            DataTable schedule = DbInterfaceScheduling.CreateSchedule(Convert.ToInt32(lblAuditionId.Text), false);

            if (schedule != null && schedule.Rows.Count > 0)
            {
                pnlViewSchedule.Visible = true;
                pnlCreateSchedule.Visible = false;
                pnlMinusSchedule.Visible = false;

                gvSchedule.DataSource = schedule;
                gvSchedule.DataBind();

                Session[scheduleData] = schedule;

                showSuccessMessage("The schedule has been successfully created.");
            }
            else
            {
                showErrorMessage("Error: An error occurred while creating the schedule.  Please ensure that you have students assigned to the audition.");
                Session[scheduleData] = null;
            }
        }

        /*
         * Pre:
         * Post: Commit the created schedule and show a message saying it has been commited
         */
        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (DbInterfaceScheduling.CommitSchedule(Convert.ToInt32(lblAuditionId.Text)))
                showSuccessMessage("The schedule was successfully committed");
            else
                showErrorMessage("The schedule could not be committed");
        }

        protected void gvSchedule_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            setHeaderRowColor(gvSchedule, e);
        }

        protected void gvJudgeValidation_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvJudgeValidation.PageIndex = e.NewPageIndex;
            BindSessionData();
        }

        protected void gvJudgeValidation_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            setHeaderRowColor(gvJudgeValidation, e);
        }

        /*
         * Pre:   
         * Post:  The page of gvAuditionSearch is changed
         */
        protected void gvAuditionSearch_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAuditionSearch.PageIndex = e.NewPageIndex;
            BindSessionData();
        }

        protected void gvAuditionSearch_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            setHeaderRowColor(gvAuditionSearch, e);
        }
        
        /*
         * Pre:   The tables must have been previously defined
         * Post:  The stored data is bound to the gridView
         */
        protected void BindSessionData()
        {
            try
            {
                DataTable data = (DataTable)Session[auditionSearch];
                gvAuditionSearch.DataSource = data;
                gvAuditionSearch.DataBind();

                data = (DataTable)Session[judgeValidation];
                gvJudgeValidation.DataSource = data;
                gvJudgeValidation.DataBind();

                data = (DataTable)Session[scheduleData];
                gvSchedule.DataSource = data;
                gvSchedule.DataBind();
            }
            catch (Exception e)
            {
                Utility.LogError("Schedule", "BindSessionData", "", "Message: " + e.Message + "   Stack Trace: " + e.StackTrace, -1);
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
         * Post: The Audition Search section is cleared
         */
        protected void btnClearAuditionSearch_Click(object sender, EventArgs e)
        {
            clearAuditionSearch();
        }

        /*
         * Pre:
         * Post: The Audition Search section is cleared
         */
        private void clearAuditionSearch()
        {
            ddlDistrictSearch.SelectedIndex = 0;
            ddlYear.SelectedIndex = 0;
            gvAuditionSearch.DataSource = null;
            gvAuditionSearch.DataBind();
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

        #region Messages

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
            Utility.LogError("Schedule", "OnError", "", "Message: " + exc.Message + "   Stack Trace: " + exc.StackTrace, -1);

            //show error label
            showErrorMessage("Error: An error occurred.");

            //Pass error on to error page
            Server.Transfer("ErrorPage.aspx", true);
        }
        #endregion Messages
    }
}