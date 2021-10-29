using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit;


namespace Emphasize {

    public class FormatDefinitionTests {

        [Fact]
        public void AllDefinitionsHaveUniqueNames() {
            HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);


            foreach (var type in GetFormatDefinitionTypes().Select((x) => x.Cast<Type>().First())) {
                names.Add(GetName(type));
            }

            Assert.Equal(7, names.Count);
        }


        [Theory]
        [MemberData(nameof(GetFormatDefinitionTypes))]
        public void AllDefinitionNamesStartWithComment(Type type) {
            Assert.StartsWith("Comment - ", GetName(type));
        }


        [Theory]
        [MemberData(nameof(GetFormatDefinitionTypes))]
        public void AllDefinitionsExportUsingTheirOwnName(Type type) {
            string name;


            name = GetName(type);

            Assert.Equal(name, type.GetCustomAttribute<NameAttribute>().Name);
            Assert.Equal(name, type.GetCustomAttribute<ClassificationTypeAttribute>().ClassificationTypeNames);
        }


        [Theory]
        [MemberData(nameof(GetFormatDefinitionTypes))]
        public void AllDefinitionsExportUseTheirNameAsTheirDisplayName(Type type) {
            string name;
            ClassificationFormatDefinition definition;


            name = GetName(type);
            definition = (ClassificationFormatDefinition)Activator.CreateInstance(type);

            Assert.Equal(name, definition.DisplayName);
        }


        [Theory]
        [MemberData(nameof(GetFormatDefinitionTypes))]
        public void AllDefinitionsDoWhatTheySayTheyDo(Type type) {
            ClassificationFormatDefinition definition;
            string name;


            definition = (ClassificationFormatDefinition)Activator.CreateInstance(type);
            name = GetName(type);

            if (name.IndexOf("bold", StringComparison.OrdinalIgnoreCase) >= 0) {
                Assert.True(definition.IsBold);
            } else {
                Assert.Null(definition.IsBold);
            }

            if (name.IndexOf("italic", StringComparison.OrdinalIgnoreCase) >= 0) {
                Assert.True(definition.IsItalic);
            } else {
                Assert.Null(definition.IsItalic);
            }

            if (name.IndexOf("code", StringComparison.OrdinalIgnoreCase) >= 0) {
                Assert.NotNull(definition.BackgroundBrush);
                Assert.NotNull(definition.BackgroundOpacity);
            } else {
                Assert.Null(definition.BackgroundBrush);
                Assert.Null(definition.BackgroundOpacity);
            }
        }


        public static IEnumerable<object[]> GetFormatDefinitionTypes() {
            yield return new object[] { typeof(FormatDefinitions.Bold) };
            yield return new object[] { typeof(FormatDefinitions.BoldItalic) };
            yield return new object[] { typeof(FormatDefinitions.BoldCode) };
            yield return new object[] { typeof(FormatDefinitions.BoldItalicCode) };
            yield return new object[] { typeof(FormatDefinitions.Italic) };
            yield return new object[] { typeof(FormatDefinitions.ItalicCode) };
            yield return new object[] { typeof(FormatDefinitions.Code) };
        }


        private static string GetName(Type definitionType) {
            return definitionType.GetCustomAttribute<NameAttribute>().Name;
        }

    }

}