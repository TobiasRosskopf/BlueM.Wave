'BlueM.Wave
'Copyright (C) BlueM Dev Group
'<https://www.bluemodel.org>
'
'This program is free software: you can redistribute it and/or modify
'it under the terms of the GNU Lesser General Public License as published by
'the Free Software Foundation, either version 3 of the License, or
'(at your option) any later version.
'
'This program is distributed in the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty of
'MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
'GNU Lesser General Public License for more details.
'
'You should have received a copy of the GNU Lesser General Public License
'along with this program.  If not, see <https://www.gnu.org/licenses/>.
'
Imports System.IO

Namespace Fileformats

    ''' <summary>
    ''' Class for HYBNAT WEL files
    ''' </summary>
    Public Class HYBNAT_WEL
        Inherits TimeSeriesFile

        ''' <summary>
        ''' The name of the element in the WEL-file
        ''' </summary>
        ''' <remarks></remarks>
        Private _elmentName As String

        ''' <summary>
        ''' The element type in the WEL-file
        ''' </summary>
        ''' <remarks>
        ''' First character of the element name
        ''' (
        ''' N: natural catchment
        ''' K: urban catchment
        ''' E: point discharge
        ''' P: observation station
        ''' T: transport element
        ''' V: branching element
        ''' B: reservoir
        ''' )
        ''' </remarks>
        Private _elementType As Char

        ''' <summary>
        ''' Referencedate for the beginning of the simulation
        ''' </summary>
        ''' <remarks>default: 01.01.2000 00:00:00</remarks>
        Public refDate As DateTime

        ''' <summary>
        ''' Set if the import dialog should be used
        ''' </summary>
        ''' <value></value>
        ''' <returns>True</returns>
        ''' <remarks></remarks>
        Public Overrides ReadOnly Property UseImportDialog() As Boolean
            Get
                Return True
            End Get
        End Property

        ''' <summary>
        ''' Instanciates a new HYBNAT WEL file
        ''' </summary>
        ''' <param name="file">Path to file</param>
        ''' <remarks></remarks>
        Public Sub New(file As String)

            Call MyBase.New(file)

            'Default settings
            UseUnits = True
            IsColumnSeparated = True
            DateTimeColumnIndex = 0
            refDate = New DateTime(2000, 1, 1, 0, 0, 0)

            Call readSeriesInfo()

        End Sub

        ''' <summary>
        ''' Checks if the file is a HYBNAT WEL files
        ''' </summary>
        ''' <param name="file">Path to file</param>
        ''' <returns>Boolean</returns>
        ''' <remarks>Check is based on line 4 (must start with "Ganglinien:")</remarks>
        Public Shared Function verifyFormat(file As String) As Boolean
            'Check if file name ends with .wel
            Dim filename As String = Path.GetFileName(file).ToLower()
            If Not filename.EndsWith(".wel") Then Return False

            'Open file
            Dim stream As New FileStream(file, FileMode.Open, IO.FileAccess.Read)
            Dim reader As New StreamReader(stream, System.Text.Encoding.Default)
            Dim syncReader = TextReader.Synchronized(reader)

            'Loop throgh file and search lines that start with "Ganglinien:" or "Zeit"
            Dim foundGanglinien As Boolean = False
            Dim foundZeit As Boolean = False
            Do
                Dim line As String = syncReader.ReadLine()
                If line.Trim().ToLower().StartsWith("ganglinien:") Then
                    foundGanglinien = True
                ElseIf line.Trim().ToLower().StartsWith("zeit") Then
                    foundZeit = True
                End If
            Loop Until syncReader.Peek() = -1

            'Close file
            syncReader.Close()
            reader.Close()
            stream.Close()

            'Valid file if both lines were found
            Return foundGanglinien And foundZeit
        End Function

        ''' <summary>
        ''' Read number of columns and their names
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub readSeriesInfo()
            'Clear series infos
            TimeSeriesInfos.Clear()

            'Open file
            Dim stream As New FileStream(File, FileMode.Open, IO.FileAccess.Read)
            Dim reader As New StreamReader(stream, System.Text.Encoding.Default)
            Dim syncReader = TextReader.Synchronized(reader)

            'Loop throgh file until line starts with "Ganglinien:" and get element name and type
            Do
                Dim line As String = syncReader.ReadLine()
                If line.Trim().ToLower().StartsWith("ganglinien:") Then
                    'Determine element name and type
                    _elmentName = line.Split(":")(1).Split("/")(0).Trim()
                    _elementType = _elmentName(0)
                    Exit Do
                End If
            Loop Until syncReader.Peek() = -1

            'Check element type
            If Not {"N", "K", "T", "B", "V", "E", "P"}.Contains(_elementType) Then
                Throw New Exception($"Unknown HYBNAT element type '{_elementType}'!")
            End If

            'Jump back to beginning of file
            stream.Seek(0, SeekOrigin.Begin)
            reader = New StreamReader(stream, Me.Encoding)
            syncReader = TextReader.Synchronized(reader)

            'Initialize arrays for column names and units
            Dim columnNames() As String = Nothing
            Dim columnUnits() As String = Nothing
            iLineHeadings = 0

            'Loop throgh file until line starts with "Zeit"
            Do
                Dim line As String = syncReader.ReadLine()
                If line.Trim().ToLower().StartsWith("zeit") Then
                    'Set line numbers for headings, units and data
                    iLineUnits = iLineHeadings + 1
                    iLineData = iLineHeadings + 3

                    'Read headings and units
                    columnNames = line.Split(New Char() {" "}, System.StringSplitOptions.RemoveEmptyEntries)
                    columnUnits = syncReader.ReadLine.Split(New Char() {" "}, System.StringSplitOptions.RemoveEmptyEntries)
                    Exit Do
                End If
                iLineHeadings += 1
            Loop Until syncReader.Peek() = -1

            'Close file
            syncReader.Close()
            reader.Close()
            stream.Close()

            'Store series info
            For i = 0 To columnNames.Count - 1
                'Overjump time column
                If i = DateTimeColumnIndex Then Continue For

                'Rename units
                If columnUnits(i) = "cbm/s" Then columnUnits(i) = "m³/s"

                'Add series info
                Dim seriesInfo = New TimeSeriesInfo With {
                    .Name = $"{_elmentName} ({columnNames(i)})",
                    .Unit = columnUnits(i),
                    .Index = i + 1
                }
                TimeSeriesInfos.Add(seriesInfo)
            Next
        End Sub

        ''' <summary>
        ''' Reads the time series from the file
        ''' </summary>
        ''' <remarks></remarks>
        Public Overrides Sub readFile()
            'Show dialog for setting the reference date
            Dim dlg As New ReferenceDateDialog()
            dlg.ShowDialog()
            refDate = dlg.DateTimePicker_refDate.Value

            'Instantiate time series
            For Each sInfo As TimeSeriesInfo In SelectedSeries
                Dim timeSeries = New TimeSeries(sInfo.Name) With {
                    .Unit = sInfo.Unit,
                    .DataSource = New TimeSeriesDataSource(File, sInfo.Name)
                }

                'Set interpretation mode
                If timeSeries.Unit = "mm" Then
                    timeSeries.Interpretation = BlueM.Wave.TimeSeries.InterpretationEnum.BlockRight
                Else
                    timeSeries.Interpretation = BlueM.Wave.TimeSeries.InterpretationEnum.Instantaneous
                End If

                MyBase.TimeSeries.Add(sInfo.Index, timeSeries)
            Next

            'Open file
            Dim stream As New FileStream(Me.File, FileMode.Open, IO.FileAccess.Read)
            Dim reader As New StreamReader(stream, Me.Encoding)
            Dim syncReader = TextReader.Synchronized(reader)

            'Overjump lines with headings and units
            For i = 1 To iLineData
                syncReader.ReadLine()
            Next

            'Read data
            Do
                'Get values and datetime (calculate from reference date and hours, round to full minutes)
                Dim values() As String = syncReader.ReadLine.Split(New Char() {" "}, System.StringSplitOptions.RemoveEmptyEntries)
                If values(0).StartsWith("-") Then Exit Do
                Dim datetime = refDate + New TimeSpan(0, Helpers.StringToDouble(values(0)) * 60, 0)

                'Add nodes to time series
                For Each seriesInfo As TimeSeriesInfo In SelectedSeries
                    TimeSeries(seriesInfo.Index).AddNode(datetime, Helpers.StringToDouble(values(seriesInfo.Index - 1)))
                Next
            Loop Until syncReader.Peek() = -1

            'Close file
            syncReader.Close()
            reader.Close()
            stream.Close()
        End Sub

    End Class

End Namespace