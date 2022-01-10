'Copyright (c) BlueM Dev Group
'Website: https://bluemodel.org
'
'All rights reserved.
'
'Released under the BSD-2-Clause License:
'
'Redistribution and use in source and binary forms, with or without modification, 
'are permitted provided that the following conditions are met:
'
'* Redistributions of source code must retain the above copyright notice, this list 
'  of conditions and the following disclaimer.
'* Redistributions in binary form must reproduce the above copyright notice, this list 
'  of conditions and the following disclaimer in the documentation and/or other materials 
'  provided with the distribution.
'
'THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY 
'EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES 
'OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT 
'SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
'SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT 
'OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
'HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR 
'TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
'EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
'--------------------------------------------------------------------------------------------
'
Imports System.Text.RegularExpressions
Imports System.IO

''' <summary>
''' Main chart window
''' </summary>
Friend Class MainWindow
    Implements IView

    Private _controller As WaveController

    ''' <summary>
    ''' Flag for preventing unintended feedback loops in the UI. Default is False, can be temporarily set to True.
    ''' </summary>
    ''' <remarks></remarks>
    Friend isInitializing As Boolean

    'ColorBand that is shown while zooming in main chart
    Friend colorBandZoom As Steema.TeeChart.Tools.ColorBand

    'ColorBand representing current view extent of main chart in OverviewChart
    Friend colorBandOverview As Steema.TeeChart.Tools.ColorBand

    'Cursors
    Friend cursor_pan As Cursor
    Friend cursor_zoom As Cursor

    Public Sub SetController(controller As Controller) Implements IView.SetController
        _controller = controller
    End Sub

    ''' <summary>
    ''' Checks whether the option to auto-adjust the Y-axes to the current viewport is activated
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend ReadOnly Property AutoAdjustYAxes() As Boolean
        Get
            Return Me.ToolStripButton_AutoAdjustYAxes.Checked
        End Get
    End Property

    Friend Property ChartMinX As DateTime
        Get
            Try
                Return DateTime.FromOADate(Me.TChart1.Axes.Bottom.Minimum)
            Catch ex As ArgumentException
                Return Constants.minOADate
            End Try
        End Get
        Set(value As DateTime)
            If value < Constants.minOADate Then
                value = Constants.minOADate
            End If
            Me.TChart1.Axes.Bottom.Minimum = value.ToOADate()
        End Set
    End Property

    Friend Property ChartMaxX As DateTime
        Get
            Try
                Return DateTime.FromOADate(Me.TChart1.Axes.Bottom.Maximum)
            Catch ex As ArgumentException
                Return Constants.maxOADate
            End Try
        End Get
        Set(value As DateTime)
            If value > Constants.maxOADate Then
                value = Constants.maxOADate
            End If
            Me.TChart1.Axes.Bottom.Maximum = value.ToOADate()
        End Set
    End Property

    'Konstruktor
    '***********
    Public Sub New()

        Me.isInitializing = True

        ' Dieser Aufruf ist f�r den Windows Form-Designer erforderlich.
        InitializeComponent()

        'Charts einrichten
        '-----------------
        Call Me.Init_Charts()

        'Navigation initialisieren
        Me.ComboBox_NavIncrement.SelectedItem = "Days"

        'set CurrentCulture for MaskedTextBoxes
        Me.MaskedTextBox_NavStart.Culture = Globalization.CultureInfo.CurrentCulture
        Me.MaskedTextBox_NavStart.FormatProvider = Globalization.CultureInfo.CurrentCulture
        Me.MaskedTextBox_NavEnd.Culture = Globalization.CultureInfo.CurrentCulture
        Me.MaskedTextBox_NavEnd.FormatProvider = Globalization.CultureInfo.CurrentCulture
        'set current date as initial values
        Me.MaskedTextBox_NavStart.Text = DateTime.Now
        Me.MaskedTextBox_NavEnd.Text = DateTime.Now

        'Instantiate cursors
        Me.cursor_pan = New Cursor(Me.GetType(), "cursor_pan.cur")
        Me.cursor_zoom = New Cursor(Me.GetType(), "cursor_zoom.cur")

        Me.isInitializing = False

    End Sub


    'Charts neu einrichten
    '*********************
    Friend Sub Init_Charts()

        'Charts zur�cksetzen
        Me.TChart1.Clear()
        Call Helpers.FormatChart(Me.TChart1.Chart)

        Me.TChart2.Clear()
        Call Helpers.FormatChart(Me.TChart2.Chart)
        Me.TChart2.Panel.Brush.Color = Color.FromArgb(239, 239, 239)
        Me.TChart2.Walls.Back.Color = Color.FromArgb(239, 239, 239)
        Me.TChart2.Header.Visible = False
        Me.TChart2.Legend.Visible = False

        'Disable TeeChart builtin zooming and panning functionality
        Me.TChart1.Zoom.Direction = Steema.TeeChart.ZoomDirections.None
        Me.TChart1.Zoom.History = False
        Me.TChart1.Zoom.Animated = True
        Me.TChart1.Panning.Allow = Steema.TeeChart.ScrollModes.None

        Me.TChart2.Zoom.Direction = Steema.TeeChart.ZoomDirections.None
        Me.TChart2.Panning.Allow = Steema.TeeChart.ScrollModes.None

        'Achsen
        Me.TChart1.Axes.Bottom.Automatic = False
        Me.TChart1.Axes.Bottom.Labels.Angle = 90
        Me.TChart1.Axes.Bottom.Labels.DateTimeFormat = "dd.MM.yy HH:mm"
        Me.TChart1.Axes.Right.Title.Angle = 90

        Me.TChart2.Axes.Left.Labels.Font.Color = Color.FromArgb(100, 100, 100)
        Me.TChart2.Axes.Left.Labels.Font.Size = 8
        Me.TChart2.Axes.Bottom.Labels.Font.Color = Color.FromArgb(100, 100, 100)
        Me.TChart2.Axes.Bottom.Labels.Font.Size = 8
        Me.TChart2.Axes.Bottom.Automatic = False
        Me.TChart2.Axes.Bottom.Labels.DateTimeFormat = "dd.MM.yyyy"

        'ColorBand einrichten
        Call Me.Init_ColorBands()

    End Sub

    ''' <summary>
    ''' Initialize color bands
    ''' </summary>
    Friend Sub Init_ColorBands()

        colorBandOverview = New Steema.TeeChart.Tools.ColorBand()
        Me.TChart2.Tools.Add(colorBandOverview)
        colorBandOverview.Axis = Me.TChart2.Axes.Bottom
        colorBandOverview.Brush.Color = Color.Coral
        colorBandOverview.Brush.Transparency = 50
        colorBandOverview.ResizeEnd = False
        colorBandOverview.ResizeStart = False
        colorBandOverview.EndLinePen.Visible = False
        colorBandOverview.StartLinePen.Visible = False

        colorBandZoom = New Steema.TeeChart.Tools.ColorBand()
        Me.TChart1.Tools.Add(colorBandZoom)
        colorBandZoom.Axis = Me.TChart1.Axes.Bottom
        colorBandZoom.Color = Color.Black
        colorBandZoom.Pen.Color = Color.Black
        colorBandZoom.Pen.Style = Drawing2D.DashStyle.Dash
        colorBandZoom.Brush.Visible = False
        colorBandZoom.ResizeEnd = False
        colorBandZoom.ResizeStart = False
        colorBandZoom.EndLinePen.Visible = True
        colorBandZoom.StartLinePen.Visible = True
    End Sub

    Private Overloads Sub Close() Implements IView.Close
        Throw New NotImplementedException()
    End Sub

End Class
