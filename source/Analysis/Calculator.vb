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
''' <summary>
''' Performs mathematical operations on time series
''' </summary>
''' <remarks>https://wiki.bluemodel.org/index.php/Wave:Calculator</remarks>
Friend Class Calculator
    Inherits Analysis

#Region "Properties"

    ''' <summary>
    ''' Returns a text description of the analysis function
    ''' </summary>
    Public Overloads Shared Function Description() As String
        Return "Performs mathematical operations on time series and returns a new time series as the result."
    End Function

    ''' <summary>
    ''' Flag indicating whether the analysis function has a result test
    ''' </summary>
    Public Overrides ReadOnly Property hasResultText() As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating whether the analysis function has result values
    ''' </summary>
    Public Overrides ReadOnly Property hasResultValues() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating whether the analysis function has a result diagram
    ''' </summary>
    Public Overrides ReadOnly Property hasResultChart() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating whether the analysis function has result series
    ''' that should be added to the main diagram
    ''' </summary>
    Public Overrides ReadOnly Property hasResultSeries() As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' The mathematical expression used for the calculation
    ''' </summary>
    Private expression As String

    ''' <summary>
    ''' List of variables
    ''' </summary>
    Private tsVariables As List(Of CalculatorVariable)

#End Region 'Properties

#Region "Methods"

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="seriesList">list of TimeSeries</param>
    Public Sub New(ByRef seriesList As List(Of TimeSeries))

        Call MyBase.New(seriesList)

        If seriesList.Count > 26 Then
            ' only 26 variable names are available
            Throw New Exception("The Calculator analysis only supports up to 26 time series!")
        End If

        'assign variable names to time series and store as dictionary
        Me.tsVariables = New List(Of CalculatorVariable)
        Dim varNames As New List(Of String) From
            {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"}
        Dim i As Integer = 0
        For Each ts As TimeSeries In Me.InputTimeSeries
            tsVariables.Add(New CalculatorVariable(varNames(i), ts))
            i += 1
        Next

    End Sub

    ''' <summary>
    ''' Analyse durchf�hren
    ''' </summary>
    Public Overrides Sub ProcessAnalysis()

        'show the ChangeTimeStepDialog
        Dim dlg As New CalculatorDialog(Me.tsVariables)
        If dlg.ShowDialog() = Windows.Forms.DialogResult.OK Then

            'read settings from dialog
            Me.expression = dlg.TextBox_Formula.Text
            Dim title As String = dlg.TextBox_Title.Text
            Dim unit As String = dlg.ComboBox_Unit.Text

            'initialize parser and add custom functions
            Dim parser As New MathParserNet.Parser()
            parser.RegisterCustomDoubleFunction("MAX", AddressOf Calculator.Max)
            parser.RegisterCustomDoubleFunction("MIN", AddressOf Calculator.Min)

            Dim value As Double
            Dim ts_result As New TimeSeries()

            'collect unique timestamps
            Dim timestamps As New HashSet(Of DateTime)
            For Each ts As TimeSeries In Me.InputTimeSeries
                timestamps.UnionWith(ts.Dates)
            Next

            'loop over timestamps
            For Each t As DateTime In timestamps
                'create variables for this timestamp
                For Each tsVariable As CalculatorVariable In tsVariables
                    value = Double.NaN
                    If tsVariable.ts.Dates.Contains(t) Then
                        value = tsVariable.ts.Nodes(t)
                    End If
                    parser.AddVariable(tsVariable.varName, value)
                Next
                'evaluate formula
                value = parser.SimplifyDouble(Me.expression)
                'store as new node in result series
                ts_result.AddNode(t, value)
                'remove variables
                parser.RemoveAllVariables()
            Next

            'clean up
            parser.UnregisterAllCustomFunctions()

            'Store result series
            ts_result.Title = title
            ts_result.Unit = unit
            ts_result.DataSource = New TimeSeriesDataSource(TimeSeriesDataSource.OriginEnum.AnalysisResult)
            Me.ResultSeries.Add(ts_result)

        End If

    End Sub

    ''' <summary>
    ''' Ergebnisse aufbereiten
    ''' </summary>
    Public Overrides Sub PrepareResults()
        Me.ResultText = "Calculator analysis:" & eol
        Me.ResultText &= $"Formula: {Me.expression}" & eol
        Me.ResultText &= "Variables: " & eol
        For Each tsvariable As CalculatorVariable In Me.tsVariables
            Me.ResultText &= $"{tsvariable.varName}: {tsvariable.ts.Title}" & eol
        Next
        Me.ResultText &= $"Result series: {Me.ResultSeries.First.Title}"

    End Sub

#End Region 'Methods

#Region "Custom functions"

    Friend Shared Function Max(val1 As Double, val2 As Double)
        Return Math.Max(val1, val2)
    End Function

    Friend Shared Function Min(val1 As Double, val2 As Double)
        Return Math.Min(val1, val2)
    End Function

#End Region 'Custom functions

End Class