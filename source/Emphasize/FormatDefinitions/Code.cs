#nullable enable

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

        public const string Name = "Emphasize - Code";


        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Name)]
        public static readonly ClassificationTypeDefinition? ClassificationType;


        public Code() {
            DisplayName = Name;
            BackgroundBrush = CreateBackgroundBrush();
            BackgroundOpacity = GetBackgroundOpacity();
        }


        internal static Brush CreateBackgroundBrush() {
            return new SolidColorBrush(Color.FromRgb(128, 128, 128));
        }


        internal static double GetBackgroundOpacity() {
            return 0.15;
        }

    }

}
