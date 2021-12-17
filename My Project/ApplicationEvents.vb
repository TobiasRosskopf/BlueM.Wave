﻿Namespace My
    ' The following events are available for MyApplication:
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(sender As Object, e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) _
            Handles Me.Startup

            If e.CommandLine.Count > 0 Then
                'run the CLI
                Dim showWave As Boolean = CLI.Run(e.CommandLine.ToList)
                If Not showWave Then
                    e.Cancel = True
                End If
            End If

        End Sub

    End Class
End Namespace
