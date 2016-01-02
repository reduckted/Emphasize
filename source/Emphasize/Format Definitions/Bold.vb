Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition


Namespace FormatDefinitions

    <ClassificationType(ClassificationTypeNames:=Bold.Name)>
    <Export(GetType(EditorFormatDefinition))>
    <Name(Bold.Name)>
    <Order(After:=Priority.High)>
    <UserVisible(True)>
    Public NotInheritable Class Bold
        Inherits ClassificationFormatDefinition


        Public Const Name As String = "Comment - Bold Span"


        <Export(GetType(ClassificationTypeDefinition))>
        <BaseDefinition("Comment")>
        <Name(Name)>
        Public Shared ReadOnly ClassificationType As ClassificationTypeDefinition


        Public Sub New()
            DisplayName = Name
            IsBold = True
        End Sub

    End Class

End Namespace
