'Copyright (c) BlueM Dev Group
'Website: http://bluemodel.org
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
Imports System.IO

''' <summary>
''' Klasse f�r das WEL-Dateiformat
''' </summary>
''' <remarks>Format siehe http://wiki.bluemodel.org/index.php/WEL-Format</remarks>
Public Class WEL
    Inherits FileFormatBase

#Region "Eigenschaften"

    Private _iLineInfo As Integer = 1
    Private _DateTimeLength As Integer = 17

#End Region

#Region "Properties"

    ''' <summary>
    ''' Gibt an, ob beim Import des Dateiformats der Importdialog angezeigt werden soll
    ''' </summary>
    Public Overrides ReadOnly Property UseImportDialog() As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' Number of the line containing general information
    ''' </summary>
    Public Property iLineInfo() As Integer
        Get
            Return _iLineInfo
        End Get
        Set(ByVal value As Integer)
            _iLineInfo = value
        End Set
    End Property

    ''' <summary>
    ''' Length of date time stamp
    ''' </summary>
    Public Property DateTimeLength() As Integer
        Get
            Return _DateTimeLength
        End Get
        Set(ByVal value As Integer)
            _DateTimeLength = value
        End Set
    End Property

#End Region 'Properties

#Region "Methoden"
    
    ''' <summary>
    ''' Konstruktor
    ''' </summary>
    Public Sub New(ByVal FileName As String, Optional ByVal ReadAllNow As Boolean = False)

        MyBase.New(FileName)

        SpaltenOffset = 1
        
        'Voreinstellungen
        Me.iLineInfo = 1
        Me.iLineHeadings = 2
        Me.UseUnits = True
        Me.iLineUnits = 3
        Me.iLineData = 4
        Me.IsColumnSeparated = True
        Me.Separator = Constants.semicolon
        Me.Dateformat = Helpers.DateFormats("WEL")
        Me.DecimalSeparator = Constants.period
        Me.DateTimeLength = 17

        Call Me.ReadColumns()

        If (ReadAllNow) Then
            'Datei komplett einlesen
            Call Me.selectAllColumns()
            Call Me.Read_File()
        End If

        
    End Sub

    ''' <summary>
    ''' Spalten auslesen
    ''' </summary>
    Public Overrides Sub ReadColumns()

        Dim i As Integer
        Dim Zeile As String = ""
        Dim ZeileSpalten As String = ""
        Dim ZeileEinheiten As String = ""
        Dim LineInfo As String = ""

        Try
            'Datei �ffnen
            Dim FiStr As FileStream = New FileStream(Me.File, FileMode.Open, IO.FileAccess.Read)
            Dim StrRead As StreamReader = New StreamReader(FiStr, System.Text.Encoding.GetEncoding("iso8859-1"))
            Dim StrReadSync As TextReader = TextReader.Synchronized(StrRead)

            'Spalten�berschriften auslesen
            For i = 1 To Me.iLineData
                Zeile = StrReadSync.ReadLine.ToString()
                If (i = Me.iLineInfo) Then LineInfo = Zeile
                If (i = Me.iLineHeadings) Then ZeileSpalten = Zeile
                If (i = Me.iLineUnits) Then ZeileEinheiten = Zeile
            Next

            'Are columns separted by ";" or should fixed format be used?
            If ZeileSpalten.Contains(";") Then
                Me.IsColumnSeparated = True
            Else
                Me.IsColumnSeparated = False
            End If

            'Is it a WEL or EFL_WEL file
            If LineInfo.Contains("EFL-WEL") Then
                Me.ColumnWidth = 41
            Else
                'nothing to do, presets can be applied
            End If

            StrReadSync.Close()
            StrRead.Close()
            FiStr.Close()

            'Spalteninformationen auslesen
            '-----------------------------
            Dim anzSpalten As Integer
            Dim Namen() As String
            Dim Einheiten() As String

            If (Me.IsColumnSeparated) Then
                'Zeichengetrennt
                Namen = ZeileSpalten.Split(New Char() {Me.Separator.ToChar}, StringSplitOptions.RemoveEmptyEntries)
                Einheiten = ZeileEinheiten.Split(New Char() {Me.Separator.ToChar}, StringSplitOptions.RemoveEmptyEntries)
                anzSpalten = Namen.Length
                ReDim Me.Columns(anzSpalten - 1)
                For i = 0 To anzSpalten - 1
                    Me.Columns(i).Name = Namen(i).Trim()
                    Me.Columns(i).Einheit = Einheiten(i).Trim()
                    Me.Columns(i).Index = i
                Next
            Else
                'Spalten mit fester Breite
                anzSpalten = Math.Ceiling((ZeileSpalten.Length - Me.DateTimeLength) / Me.ColumnWidth) + 1
                ReDim Me.Columns(anzSpalten - 1)
                For i = 0 To anzSpalten - 1
                    If i = 0 Then
                        'DateTime need to be considered especially as it can be shorter than the standard "Spaltenbreite"
                        Me.Columns(i).Name = ZeileSpalten.Substring(0, Me.DateTimeLength) '.Trim()
                        Me.Columns(i).Einheit = ZeileEinheiten.Substring(0, Me.DateTimeLength) '.Trim()
                        Me.Columns(i).Index = i
                    Else
                        Me.Columns(i).Name = ZeileSpalten.Substring(Me.DateTimeLength + (i - 1) * Me.ColumnWidth, Me.ColumnWidth).Trim()
                        Me.Columns(i).Einheit = ZeileEinheiten.Substring(Me.DateTimeLength + (i - 1) * Me.ColumnWidth, Me.ColumnWidth).Trim()
                        Me.Columns(i).Index = i
                    End If
                Next
            End If

        Catch ex As Exception
            MsgBox("Konnte die Datei '" & Path.GetFileName(Me.File) & "' nicht einlesen!" & eol & eol & "Fehler: " & ex.Message, MsgBoxStyle.Critical, "Fehler")
        End Try

    End Sub

    ''' <summary>
    ''' WEL-Datei einlesen
    ''' </summary>
    Public Overrides Sub Read_File()

        Dim iZeile, i As Integer
        Dim Zeile As String
        Dim Werte() As String
        Dim timestamp As String
        Dim datum As DateTime
        Dim ok As Boolean

        Dim FiStr As FileStream = New FileStream(Me.File, FileMode.Open, IO.FileAccess.Read)
        Dim StrRead As StreamReader = New StreamReader(FiStr, System.Text.Encoding.GetEncoding("iso8859-1"))
        Dim StrReadSync = TextReader.Synchronized(StrRead)

        'Anzahl Zeitreihen bestimmen
        ReDim Me.TimeSeries(Me.SelectedColumns.Length - 1)

        'Zeitreihen instanzieren
        For i = 0 To Me.SelectedColumns.Length - 1
            Me.TimeSeries(i) = New TimeSeries(Me.SelectedColumns(i).Name)
        Next

        'Einheiten?
        If (Me.UseUnits) Then
            'Alle ausgew�hlten Spalten durchlaufen
            For i = 0 To Me.SelectedColumns.Length - 1
                Me.TimeSeries(i).Unit = Me.SelectedColumns(i).Einheit
            Next
        End If

        'Einlesen
        '--------

        'Header
        For iZeile = 1 To Me.nLinesHeader
            Zeile = StrReadSync.ReadLine.ToString()
        Next

        'Daten
        Do
            Zeile = StrReadSync.ReadLine.ToString()

            If (Me.IsColumnSeparated) Then
                'Zeichengetrennt
                '---------------
                Werte = Zeile.Split(New Char() {Me.Separator.ToChar}, StringSplitOptions.RemoveEmptyEntries)
                'Erste Spalte: Datum_Zeit
                timestamp = Werte(0).Trim()
                ok = DateTime.TryParseExact(timestamp, Me.Dateformat, Helpers.DefaultNumberFormat, Globalization.DateTimeStyles.None, datum)
                If (Not ok) Then
                    Throw New Exception("Unable to parse the timestamp '" & timestamp & "' using the given format '" & Me.Dateformat & "'!")
                End If
                'Restliche Spalten: Werte
                'Alle ausgew�hlten Spalten durchlaufen
                For i = 0 To Me.SelectedColumns.Length - 1
                    Me.TimeSeries(i).AddNode(datum, StringToDouble(Werte(Me.SelectedColumns(i).Index)))
                Next
            Else
                'Spalten mit fester Breite
                '-------------------------
                'Erste Spalte: Datum_Zeit
                timestamp = Zeile.Substring(SpaltenOffset, Me.ColumnWidth).Trim()
                ok = DateTime.TryParseExact(timestamp, Me.Dateformat, Helpers.DefaultNumberFormat, Globalization.DateTimeStyles.None, datum)
                If (Not ok) Then
                    Throw New Exception("Unable to parse the timestamp '" & timestamp & "' using the given format '" & Me.Dateformat & "'!")
                End If
                'Restliche Spalten: Werte
                'Alle ausgew�hlten Spalten durchlaufen
                For i = 0 To Me.SelectedColumns.Length - 1
                    Me.TimeSeries(i).AddNode(datum, StringToDouble(Zeile.Substring((Me.SelectedColumns(i).Index * Me.ColumnWidth) + SpaltenOffset, Math.Min(Me.ColumnWidth, Zeile.Substring((Me.SelectedColumns(i).Index * Me.ColumnWidth) + SpaltenOffset).Length))))
                Next
            End If

        Loop Until StrReadSync.Peek() = -1

        StrReadSync.Close()
        StrRead.Close()
        FiStr.Close()

    End Sub

    ''' <summary>
    ''' Pr�ft, ob es sich um eine WEL-Datei f�r BlueM handelt
    ''' </summary>
    ''' <param name="file">Pfad zur Datei</param>
    ''' <returns></returns>
    Public Shared Function verifyFormat(ByVal file As String) As Boolean

        Dim FiStr As FileStream = New FileStream(file, FileMode.Open, IO.FileAccess.Read)
        Dim StrRead As StreamReader = New StreamReader(FiStr, System.Text.Encoding.GetEncoding("iso8859-1"))
        Dim Zeile As String = ""

        Zeile = StrRead.ReadLine.ToString()
        StrRead.Close()
        FiStr.Close()

        If (Zeile.StartsWith(" *WEL")) Then
            'It's a BlueM WEL file
            Return True
        ElseIf (Zeile.StartsWith("*EFL-WEL")) Then
            'It's a BlueM EFL WEL file
            Return True
        ElseIf (Zeile.StartsWith("*WEL")) Then
            'It's a TALSIM WEL file
            Return True
        Else
            Return False
        End If

    End Function

#End Region 'Methoden

End Class