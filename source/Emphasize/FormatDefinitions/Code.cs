using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Windows.Media;


namespace Emphasize.FormatDefinitions {

    [ClassificationType(ClassificationTypeNames = Name)]
    [Export(typeof(EditorFormatDefinition))]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    public class Code : ClassificationFormatDefinition {

        public const string Name = "Comment - Code Span";


        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Name)]
        public static readonly ClassificationTypeDefinition ClassificationType;


        public Code() {
            DisplayName = Name;
            BackgroundBrush = CreateBackgroundBrush();
            BackgroundOpacity = GetBackgroundOpacity();
        }


        internal static Brush CreateBackgroundBrush() {
            return new SolidColorBrush(Color.FromRgb(0, 128, 0));
        }


        internal static double GetBackgroundOpacity() {
            return 0.1;
        }

    }

}
