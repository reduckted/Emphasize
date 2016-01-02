Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition


Namespace FormatDefinitions

    <ClassificationType(ClassificationTypeNames:=BoldItalicCode.Name)>
    <Export(GetType(EditorFormatDefinition))>
    <Name(BoldItalicCode.Name)>
    <Order(After:=Priority.High)>
    <UserVisible(True)>
    Public NotInheritable Class BoldItalicCode
        Inherits ClassificationFormatDefinition


        Public Const Name As String = "Comment - Bold, Italic, Code Span"


        <Export(GetType(ClassificationTypeDefinition))>
        <BaseDefinition("Comment")>
        <Name(Name)>
        Public Shared ReadOnly ClassificationType As ClassificationTypeDefinition


        Public Sub New()
            DisplayName = Name
            IsBold = True
            IsItalic = True
            BackgroundBrush = Code.CreateBackgroundBrush()
            BackgroundOpacity = Code.GetBackgroundOpacity()
        End Sub

    End Class

End Namespace
