using Microsoft.AspNetCore.Components;

namespace Lithium.Server.Dashboard.Components;

public partial class StatCard : ComponentBase
{
    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public string Value { get; set; } = string.Empty;
    [Parameter] public string Icon { get; set; } = string.Empty;
}