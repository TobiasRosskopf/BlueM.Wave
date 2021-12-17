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
Friend Class GoodnessOfFit_Dialog

    Public Sub New(zre1 As String, zre2 As String)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Me.RadioButton1.Text = zre1
        Me.RadioButton2.Text = zre2

        Me.ComboBox_startMonth.BeginUpdate()
        Me.ComboBox_startMonth.Items.AddRange(Helpers.CalendarMonths.ToArray)
        Me.ComboBox_startMonth.EndUpdate()

        Me.ComboBox_startMonth.SelectedIndex = 10 'November
    End Sub

    Public Function getNrGemesseneReihe() As Integer

        If (Me.RadioButton1.Checked) Then
            Return 1
        Else
            Return 2
        End If

    End Function

    Private Sub CheckBox_Annual_CheckedChanged(sender As Object, e As EventArgs) Handles CheckBox_Annual.CheckedChanged
        If CheckBox_Annual.Checked Then
            Me.Label_startMonth.Enabled = True
            Me.ComboBox_startMonth.Enabled = True
        Else
            Me.Label_startMonth.Enabled = False
            Me.ComboBox_startMonth.Enabled = False
        End If
    End Sub
End Class