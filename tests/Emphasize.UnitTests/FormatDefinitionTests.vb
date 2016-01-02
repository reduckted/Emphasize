Imports Microsoft.VisualStudio.Text.Classification
Imports Microsoft.VisualStudio.Utilities
Imports System.Reflection


Public Class FormatDefinitionTests

    <Fact()>
    Public Sub AllDefinitionsHaveUniqueNames()
        Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)


        For Each type In GetFormatDefinitionTypes().Select(Function(x) x.Cast(Of Type).First())
            names.Add(GetName(type))
        Next type

        Assert.Equal(7, names.Count)
    End Sub


    <MemberData(NameOf(GetFormatDefinitionTypes))>
    <Theory()>
    Public Sub AllDefinitionNamesStartWithComment(type As Type)
        Assert.StartsWith("Comment - ", GetName(type))
    End Sub


    <MemberData(NameOf(GetFormatDefinitionTypes))>
    <Theory()>
    Public Sub AllDefinitionsExportUsingTheirOwnName(type As Type)
        Dim name As String


        name = GetName(type)

        Assert.Equal(name, type.GetCustomAttribute(Of NameAttribute)().Name)
        Assert.Equal(name, type.GetCustomAttribute(Of ClassificationTypeAttribute)().ClassificationTypeNames)
    End Sub


    <MemberData(NameOf(GetFormatDefinitionTypes))>
    <Theory()>
    Public Sub AllDefinitionsExportUseTheirNameAsTheirDisplayName(type As Type)
        Dim name As String
        Dim definition As ClassificationFormatDefinition


        name = GetName(type)
        definition = DirectCast(Activator.CreateInstance(type), ClassificationFormatDefinition)

        Assert.Equal(name, definition.DisplayName)
    End Sub


    <MemberData(NameOf(GetFormatDefinitionTypes))>
    <Theory()>
    Public Sub AllDefinitionsDoWhatTheySayTheyDo(type As Type)
        Dim definition As ClassificationFormatDefinition
        Dim name As String


        definition = DirectCast(Activator.CreateInstance(type), ClassificationFormatDefinition)

        name = GetName(type)

        If name.IndexOf("bold", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Assert.True(definition.IsBold)
        Else
            Assert.Null(definition.IsBold)
        End If

        If name.IndexOf("italic", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Assert.True(definition.IsItalic)
        Else
            Assert.Null(definition.IsItalic)
        End If

        If name.IndexOf("code", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Assert.NotNull(definition.BackgroundBrush)
            Assert.NotNull(definition.BackgroundOpacity)
        Else
            Assert.Null(definition.BackgroundBrush)
            Assert.Null(definition.BackgroundOpacity)
        End If
    End Sub


    Public Shared Iterator Function GetFormatDefinitionTypes() As IEnumerable(Of Object())
        Yield {GetType(FormatDefinitions.Bold)}
        Yield {GetType(FormatDefinitions.BoldItalic)}
        Yield {GetType(FormatDefinitions.BoldCode)}
        Yield {GetType(FormatDefinitions.BoldItalicCode)}
        Yield {GetType(FormatDefinitions.Italic)}
        Yield {GetType(FormatDefinitions.ItalicCode)}
        Yield {GetType(FormatDefinitions.Code)}
    End Function


    Private Shared Function GetName(definitionType As Type) As String
        Dim flags As BindingFlags


        flags = BindingFlags.Public Or BindingFlags.Static

        Return DirectCast(definitionType.GetField("Name", flags).GetValue(Nothing), String)
    End Function

End Class
