Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.ComponentModel.Composition
Imports System.Windows.Media


Namespace FormatDefinitions

    <ClassificationType(ClassificationTypeNames:=Code.Name)>
    <Export(GetType(EditorFormatDefinition))>
    <Name(Code.Name)>
    <Order(After:=Priority.High)>
    <UserVisible(True)>
    Public Class Code
        Inherits ClassificationFormatDefinition


        Public Const Name As String = "Comment - Code Span"


        <Export(GetType(ClassificationTypeDefinition))>
        <BaseDefinition("Comment")>
        <Name(Name)>
        Public Shared ReadOnly ClassificationType As ClassificationTypeDefinition


        Public Sub New()
            DisplayName = Name
            BackgroundBrush = CreateBackgroundBrush()
            BackgroundOpacity = GetBackgroundOpacity()
        End Sub


        Friend Shared Function CreateBackgroundBrush() As Brush
            Return New SolidColorBrush(Color.FromRgb(0, 128, 0))
        End Function


        Friend Shared Function GetBackgroundOpacity() As Double
            Return 0.1
        End Function

    End Class

End Namespace
