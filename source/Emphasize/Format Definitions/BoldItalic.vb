Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition


Namespace FormatDefinitions

    <ClassificationType(ClassificationTypeNames:=BoldItalic.Name)>
    <Export(GetType(EditorFormatDefinition))>
    <Name(BoldItalic.Name)>
    <Order(After:=Priority.High)>
    <UserVisible(True)>
    Public NotInheritable Class BoldItalic
        Inherits ClassificationFormatDefinition


        Public Const Name As String = "Comment - Bold, Italic Span"


        <Export(GetType(ClassificationTypeDefinition))>
        <BaseDefinition("Comment")>
        <Name(Name)>
        Public Shared ReadOnly ClassificationType As ClassificationTypeDefinition


        Public Sub New()
            DisplayName = Name
            IsBold = True
            IsItalic = True
        End Sub

    End Class

End Namespace
