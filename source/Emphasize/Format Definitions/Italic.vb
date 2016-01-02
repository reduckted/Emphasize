Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition


Namespace FormatDefinitions

    <ClassificationType(ClassificationTypeNames:=Italic.Name)>
    <Export(GetType(EditorFormatDefinition))>
    <Name(Italic.Name)>
    <Order(After:=Priority.High)>
    <UserVisible(True)>
    Public Class Italic
        Inherits ClassificationFormatDefinition


        Public Const Name As String = "Comment - Italic Span"


        <Export(GetType(ClassificationTypeDefinition))>
        <BaseDefinition("Comment")>
        <Name(Name)>
        Public Shared ReadOnly ClassificationType As ClassificationTypeDefinition


        Public Sub New()
            DisplayName = Name
            IsItalic = True
        End Sub

    End Class

End Namespace
