using Microsoft.AspNetCore.Components;
using Microsoft.Fast.Components.FluentUI.DesignTokens;

namespace FisherTournament.WebServer.Shared;

public partial class MainLayout : LayoutComponentBase
{
    [Inject]
    private BodyFont BodyFont { get; set; } = default!;

    ElementReference container;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await BodyFont.SetValueFor(container, "\"Montserrat\", \"Segoe UI Variable\", \"Segoe UI\", sans-serif");
        }
    }
}