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
Friend Class PropertiesController
    Inherits Controller

    Private Overloads ReadOnly Property View As PropertiesWindow
        Get
            Return _view
        End Get
    End Property

    Public Sub New(view As IView, ByRef model As Wave)
        Call MyBase.New(view, model)

        _view.SetController(Me)

        'view events
        AddHandler Me.View.SeriesPropertyChanged, AddressOf SeriesPropertiesViewChanged
        AddHandler Me.View.SeriesDeleted, AddressOf SeriesDeleted

        'model events
        AddHandler _model.SeriesAdded, AddressOf UpdateView
        AddHandler _model.SeriesPropertiesChanged, AddressOf UpdateView
        AddHandler _model.SeriesRemoved, AddressOf UpdateView
        AddHandler _model.SeriesCleared, AddressOf UpdateView
    End Sub

    Public Overrides Sub ShowView()
        If IsNothing(_view) Then
            _view = New PropertiesWindow()
        End If
        View.Update(_model.TimeSeriesDict.Values.ToList)
        View.Show()
        View.BringToFront()
    End Sub

    Private Sub UpdateView()
        View.Update(_model.TimeSeriesDict.Values.ToList)
    End Sub

    ''' <summary>
    ''' Handles time series properties changed in the PropertiesDialog
    ''' </summary>
    ''' <param name="id">Id of the time series whose properties have changed</param>
    ''' <remarks>Handles changed interpretation, title and unit</remarks>
    Private Sub SeriesPropertiesViewChanged(id As Integer)
        _model.SeriesPropertiesChangedHandler(id)
    End Sub

    Private Sub SeriesDeleted(id As Integer)
        _model.RemoveTimeSeries(id)
    End Sub

End Class