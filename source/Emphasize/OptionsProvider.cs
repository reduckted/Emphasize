#nullable enable

namespace Emphasize;

public class OptionsProvider {

    private readonly OptionsPage _page;


    public OptionsProvider(OptionsPage page) {
        _page = page;
    }


    public bool UseMarkdownStyle => _page.UseMarkdownStyle;

}
