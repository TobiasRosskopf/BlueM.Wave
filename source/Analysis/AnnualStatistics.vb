﻿'Copyright (c) BlueM Dev Group
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
''' Calculates annual statistics (min, max, avg, vol) based on hydrological years
''' </summary>
''' <remarks></remarks>
Friend Class AnnualStatistics
    Inherits Analysis

    Public Structure struct_stat
        Public startDate As Date
        Public endDate As Date
        Public len As Long
        Public min As Double
        Public max As Double
        Public avg As Double
        Public vol As Double
    End Structure

    Private stats As Dictionary(Of String, struct_stat)

    Public Overloads Shared Function Description() As String
        Return "Calculates annual statistics (min, max, avg, vol) based on hydrological years."
    End Function

    Public Overrides ReadOnly Property hasResultChart() As Boolean
        Get
            Return False
        End Get
    End Property

    Public Overrides ReadOnly Property hasResultText() As Boolean
        Get
            Return True
        End Get
    End Property

    Public Overrides ReadOnly Property hasResultValues() As Boolean
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
            Return False
        End Get
    End Property

    Public Sub New(ByRef series As List(Of TimeSeries))
        MyBase.New(series)
        'Check: expects exactly one series
        If (series.Count <> 1) Then
            Throw New Exception("The Annual Statistics analysis requires the selection of exactly 1 time series!")
        End If
        stats = New Dictionary(Of String, struct_stat)
    End Sub

    Private Function calculateStats(ByRef series As TimeSeries) As struct_stat
        Dim stats As struct_stat
        stats.startDate = series.StartDate
        stats.endDate = series.EndDate
        stats.len = series.Length
        stats.min = series.Minimum
        stats.max = series.Maximum
        stats.avg = series.Average
        stats.vol = series.Volume
        Return stats
    End Function

    Public Overrides Sub ProcessAnalysis()
        Dim hyoseries As Dictionary(Of Integer, TimeSeries)
        Dim year As Integer
        Dim series As TimeSeries

        Dim dialog As New AnnualStatistics_Dialog()
        Dim dialogResult As DialogResult = dialog.ShowDialog()
        If dialogResult <> DialogResult.OK Then
            Throw New Exception("User abort!")
        End If

        Dim startMonth As Integer = CType(dialog.ComboBox_startMonth.SelectedItem, Month).number

        'stats for entire series
        Me.stats.Add("Entire series", calculateStats(Me.InputTimeSeries(0)))

        'stats for hydrological years
        hyoseries = Me.InputTimeSeries(0).SplitHydroYears(startMonth)
        For Each kvp As KeyValuePair(Of Integer, TimeSeries) In hyoseries
            year = kvp.Key
            series = kvp.Value
            Me.stats.Add(year.ToString, calculateStats(series))
        Next
    End Sub


    Public Overrides Sub PrepareResults()
        Dim stat As struct_stat

        Const formatstring As String = "F4"

        Me.ResultText = "Annual statistics:" & eol & eol &
                         $"Time series: {Me.InputTimeSeries(0).Title}" & eol & eol
        'output results in CSV format
        Me.ResultText &= "Results:" & eol
        Me.ResultText &= String.Join(Helpers.CurrentListSeparator, "Description", "Start", "End", "Length", "Min", "Max", "Avg", "Volume") & eol
        For Each kvp As KeyValuePair(Of String, struct_stat) In Me.stats
            stat = kvp.Value
            Me.ResultText &= String.Join(Helpers.CurrentListSeparator,
                kvp.Key,
                stat.startDate.ToString(Helpers.CurrentDateFormat),
                stat.endDate.ToString(Helpers.CurrentDateFormat),
                stat.len.ToString(),
                stat.min.ToString(formatstring),
                stat.max.ToString(formatstring),
                stat.avg.ToString(formatstring),
                stat.vol.ToString(formatstring)) & eol
        Next
    End Sub

End Class