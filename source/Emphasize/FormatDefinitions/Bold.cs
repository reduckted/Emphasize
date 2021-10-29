﻿using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;


namespace Emphasize.FormatDefinitions {

    [ClassificationType(ClassificationTypeNames = Name)]
    [Export(typeof(EditorFormatDefinition))]
    [Name(Name)]
    [Order(After = Priority.High)]
    [UserVisible(true)]
    public class Bold : ClassificationFormatDefinition {

        public const string Name = "Comment - Bold Span";


        [Export(typeof(ClassificationTypeDefinition))]
        [BaseDefinition("Comment")]
        [Name(Name)]
        public static readonly ClassificationTypeDefinition ClassificationType;


        public Bold() {
            DisplayName = Name;
            IsBold = true;
        }

    }

}
