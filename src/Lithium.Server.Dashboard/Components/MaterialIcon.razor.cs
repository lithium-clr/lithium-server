using Microsoft.AspNetCore.Components;

namespace Lithium.Server.Dashboard.Components;

public partial class MaterialIcon : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public bool Filled { get; set; }
}