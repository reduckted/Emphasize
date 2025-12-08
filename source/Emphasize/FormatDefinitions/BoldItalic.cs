#nullable enable

using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace Emphasize.FormatDefinitions {

    [ClassificationType(ClassificationTypeNames = Name)]
    [Export(typeof(EditorFormatDefinition))]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    public class BoldItalic : ClassificationFormatDefinition {

        public const string Name = "Emphasize - Bold, Italic";


        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Name)]
        public static readonly ClassificationTypeDefinition? ClassificationType;


        public BoldItalic() {
            DisplayName = Name;
            IsBold = true;
            IsItalic = true;
        }

    }

}
