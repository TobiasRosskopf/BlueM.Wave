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
''' Abstract base class for analysis functions
''' </summary>
Friend MustInherit Class Analysis

#Region "Events"

    ''' <summary>
    ''' Is raised when a lengthy process starts
    ''' </summary>
    Friend Event AnalysisStarted(n_steps As Integer)

    ''' <summary>
    ''' Is raised when a lengthy process updates its status
    ''' </summary>
    Friend Event AnalysisUpdated(value As Integer)

    ''' <summary>
    ''' Is raised when a lengthy process finishes
    ''' </summary>
    Friend Event AnalysisFinished()

#End Region

#Region "Eigenschaften"

    ''' <summary>
    ''' List of input TimeSeries for analysis
    ''' </summary>
    Protected InputTimeSeries As List(Of TimeSeries)

    ''' <summary>
    ''' Result text
    ''' is shown in the Wave log if `hasResultText` is True
    ''' </summary>
    ''' <remarks>Optional</remarks>
    Protected ResultText As String

    ''' <summary>
    ''' Result values
    ''' Are shown in the Wave log if `hasResultValues` is True
    ''' </summary>
    ''' <remarks>Optional</remarks>
    Protected ResultValues As Dictionary(Of String, Double)

    ''' <summary>
    ''' Result chart
    ''' is shown in a separate window if `hasResultChart` is True
    ''' </summary>
    ''' <remarks>Optional</remarks>
    Protected ResultChart As Steema.TeeChart.Chart

    ''' <summary>
    ''' List of result series
    ''' are loaded in the main chart if `hasResultSeries` is True
    ''' </summary>
    ''' <remarks>optional</remarks>
    Protected ResultSeries As List(Of TimeSeries)

#End Region 'Eigenschaften

#Region "Properties"

    ''' <summary>
    ''' Returns a text description of the analysis function
    ''' Should be overloaded by inheriting analysis functions
    ''' </summary>
    Public Shared Function Description() As String
        Return "No description found"
    End Function

    ''' <summary>
    ''' Flag indicating whether the analysis function has a result text
    ''' </summary>
    Public MustOverride ReadOnly Property hasResultText() As Boolean

    ''' <summary>
    ''' Flag indicating whether the analysis function has result values
    ''' </summary>
    Public MustOverride ReadOnly Property hasResultValues() As Boolean

    ''' <summary>
    ''' Flag indicating whether the analysis function has a result diagram
    ''' </summary>
    Public MustOverride ReadOnly Property hasResultChart() As Boolean

    ''' <summary>
    ''' Flag indicating whether the analysis function has result series
    ''' that should be added to the main diagram
    ''' </summary>
    Public MustOverride ReadOnly Property hasResultSeries() As Boolean

    ''' <summary>
    ''' Analysis result text
    ''' </summary>
    ''' <remarks>Optional</remarks>
    Public ReadOnly Property getResultText() As String
        Get
            Return Me.ResultText
        End Get
    End Property

    ''' <summary>
    ''' Analysis result values
    ''' </summary>
    ''' <remarks>Optional</remarks>
    Public ReadOnly Property getResultValues() As Dictionary(Of String, Double)
        Get
            Return Me.ResultValues
        End Get
    End Property

    ''' <summary>
    ''' Analysis result chart
    ''' </summary>
    ''' <remarks>Optional</remarks>
    Public ReadOnly Property getResultChart() As Steema.TeeChart.Chart
        Get
            Return Me.ResultChart
        End Get
    End Property

    ''' <summary>
    ''' Analysis result series
    ''' </summary>
    ''' <remarks>Optional</remarks>
    Public ReadOnly Property getResultSeries() As List(Of TimeSeries)
        Get
            Return Me.ResultSeries
        End Get
    End Property

#End Region 'Properties

#Region "Methoden"

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="inputseries">List of input TimeSeries</param>
    Public Sub New(inputseries As List(Of TimeSeries))

        'Zeitreihen 
        Me.InputTimeSeries = inputseries

        'Datenstrukturen initialisieren
        Me.ResultValues = New Dictionary(Of String, Double)
        Me.ResultSeries = New List(Of TimeSeries)
    End Sub

    ''' <summary>
    ''' Processes the analysis
    ''' </summary>
    Public MustOverride Sub ProcessAnalysis()

    ''' <summary>
    ''' Prepares analysis results
    ''' </summary>
    Public MustOverride Sub PrepareResults()

    ''' <summary>
    ''' Starts a lengthy analysis process
    ''' </summary>
    ''' <param name="n_steps">Number of expected steps</param>
    Protected Sub AnalysisProgressStart(n_steps As Integer)
        RaiseEvent AnalysisStarted(n_steps)
    End Sub

    ''' <summary>
    ''' Updates a lengthy analysis process
    ''' </summary>
    ''' <param name="value">Progress value</param>
    Protected Sub AnalysisProgressUpdate(value As Integer)
        RaiseEvent AnalysisUpdated(value)
    End Sub

    ''' <summary>
    ''' Finishes a lengthy analysis process
    ''' </summary>
    Protected Sub AnalysisProgressFinish()
        RaiseEvent AnalysisFinished()
    End Sub



#End Region 'Methoden

End Class