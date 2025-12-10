#nullable enable

using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

namespace Emphasize;

public class OptionsPage : DialogPage {

    [Category("General")]
    [DisplayName("Use markdown-style syntax")]
    [Description("When enabled, use **two asterisks** for bold and *one asterisk* for italic.")]
    [DefaultValue(false)]
    public bool UseMarkdownStyle { get; set; }

}
