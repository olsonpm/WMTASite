﻿<%@ Application Language="C#" %>

<script runat="server">

    /*void Application_Start(object sender, EventArgs e) 
    {
        // Code that runs on application startup

    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  Code that runs on application shutdown

    }*/
    
    //code that runs when an unhandled error occurs
    void Application_Error(object sender, EventArgs e) 
    {
        // Get the exception object.
        Exception exc = Server.GetLastError();

        // Handle HTTP errors
        if (exc.GetType() == typeof(HttpException))
        {
            //Redirect HTTP errors to HttpError page
            Server.Transfer("HttpErrorPage.aspx");
        }

        // For other kinds of errors give the user some information
        // but stay on the default page
        Response.Write("<h2>Global Page Error</h2>\n");
        Response.Write(
            "<p>" + exc.Message + "</p>\n");
        Response.Write("Return to the <a href='WelcomeScreen.aspx'>" +
            "Home Page</a>\n");

        // Log the exception 
        //Utility.LogError("Global.asax", "", "", exc.Message, -1);

        // Clear the error from the server
        Server.ClearError();
    }

    /*void Session_Start(object sender, EventArgs e) 
    {
        // Code that runs when a new session is started

    }

    void Session_End(object sender, EventArgs e) 
    {
        // Code that runs when a session ends. 
        // Note: The Session_End event is raised only when the sessionstate mode
        // is set to InProc in the Web.config file. If session mode is set to StateServer 
        // or SQLServer, the event is not raised.

    }*/
       
</script>
