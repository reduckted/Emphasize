using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace Emphasize.FormatDefinitions {

    [ClassificationType(ClassificationTypeNames = Name)]
    [Export(typeof(EditorFormatDefinition))]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    public class BoldItalicCode : ClassificationFormatDefinition {

        public const string Name = "Comment - Bold, Italic, Code Span";


        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Name)]
        public static readonly ClassificationTypeDefinition ClassificationType;


        public BoldItalicCode() {
            DisplayName = Name;
            IsBold = true;
            IsItalic = true;
            BackgroundBrush = Code.CreateBackgroundBrush();
            BackgroundOpacity = Code.GetBackgroundOpacity();
        }

    }

}
