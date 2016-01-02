Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition


Namespace FormatDefinitions

    <ClassificationType(ClassificationTypeNames:=BoldCode.Name)>
    <Export(GetType(EditorFormatDefinition))>
    <Name(BoldCode.Name)>
    <Order(After:=Priority.High)>
    <UserVisible(True)>
    Public NotInheritable Class BoldCode
        Inherits ClassificationFormatDefinition


        Public Const Name As String = "Comment - Bold, Code Span"


        <Export(GetType(ClassificationTypeDefinition))>
        <BaseDefinition("Comment")>
        <Name(Name)>
        Public Shared ReadOnly ClassificationType As ClassificationTypeDefinition


        Public Sub New()
            DisplayName = Name
            IsBold = True
            BackgroundBrush = Code.CreateBackgroundBrush()
            BackgroundOpacity = Code.GetBackgroundOpacity()
        End Sub

    End Class

End Namespace
