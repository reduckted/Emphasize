# [Emphasize!](https://gallery.msdn.microsoft.com/3c482774-adaa-40e3-9a62-d32fb41a7a1c)
_A Visual Studio extension that shows emphasis in your comments._

Sick of boring comments in your code? This extension will highlight 
words and phrases that you add emphasis to so that they stand out.

## Types of Emphasis

Three types of emphasis are supported:

|Type      |Character         |Example              |
|----------|------------------|---------------------|
|**Bold**  | Asterisk ( * )   |This \***is bold**\*.|
|*Italic*  | Underscore ( _ ) |*\_This is italic\_*.|
|`Code`    | Backtick ( \` )  |This is \``code`\`.  |

You can even nest the different types of emphasis.


## Language Support

Works in all language that Visual Studio highlights comments in, including:

* C#
* VB.NET
* JavaScript
* TypeScript
* CoffeeScript
* SQL
* XML
* HTML
* CSS
* PowerShell
* and many more!

## Customization

If you're not happy with the colors, you can change this via Visual Studio's 
*Fonts and Colors* options. Just look for these display items:

* Comment - Bold Span
* Comment - Bold, Code Span
* Comment - Bold, Italic Span
* Comment - Bold, Italic, Code Span
* Comment - Code Span
* Comment - Italic Span
* Comment - Italic, Code Span

_Note: The background color of emphasized code spans is always partially transparent._


## Limitations

Emphasized phrases that span multiple lines are not currently supported.
