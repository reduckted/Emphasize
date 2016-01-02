Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition


Namespace FormatDefinitions

    <ClassificationType(ClassificationTypeNames:=ItalicCode.Name)>
    <Export(GetType(EditorFormatDefinition))>
    <Name(ItalicCode.Name)>
    <Order(After:=Priority.High)>
    <UserVisible(True)>
    Public NotInheritable Class ItalicCode
        Inherits ClassificationFormatDefinition


        Public Const Name As String = "Comment - Italic, Code Span"


        <Export(GetType(ClassificationTypeDefinition))>
        <BaseDefinition("Comment")>
        <Name(Name)>
        Public Shared ReadOnly ClassificationType As ClassificationTypeDefinition


        Public Sub New()
            DisplayName = Name
            IsItalic = True
            BackgroundBrush = Code.CreateBackgroundBrush()
            BackgroundOpacity = Code.GetBackgroundOpacity()
        End Sub

    End Class

End Namespace
