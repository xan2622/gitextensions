using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GitCommands.Settings;

/// <summary>
/// Configuration for a single toolbar item (button, menu, etc.)
/// </summary>
[DataContract]
public class ToolbarItemConfig
{
    /// <summary>
    /// The name of the ToolStripItem (e.g., "toolStripButtonCommit")
    /// </summary>
    [DataMember]
    public string ItemName { get; set; } = string.Empty;

    /// <summary>
    /// Index of the toolbar where this item is located (0=Main, 1=Filters, 2=Scripts, 3+=Custom)
    /// </summary>
    [DataMember]
    public int ToolbarIndex { get; set; }

    /// <summary>
    /// Name of the toolbar (e.g., "Standard", "Filters", "Scripts", "Custom 01")
    /// </summary>
    [DataMember]
    public string ToolbarName { get; set; } = string.Empty;

    /// <summary>
    /// Position/order of the item within its toolbar (0-based index)
    /// </summary>
    [DataMember]
    public int Order { get; set; }
}

/// <summary>
/// Metadata for a custom toolbar
/// </summary>
[DataContract]
public class CustomToolbarMetadata
{
    /// <summary>
    /// Name of the custom toolbar (e.g., "Custom 01", "Custom 02")
    /// </summary>
    [DataMember]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Index of the toolbar (3+ for custom toolbars)
    /// </summary>
    [DataMember]
    public int Index { get; set; }

    /// <summary>
    /// Whether the toolbar is visible
    /// </summary>
    [DataMember]
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Complete configuration for all toolbars layout
/// </summary>
[DataContract]
public class ToolbarLayoutConfig
{
    /// <summary>
    /// Number of visible toolbars (default: 3)
    /// </summary>
    [DataMember]
    public int ToolbarCount { get; set; } = 3;

    /// <summary>
    /// List of all toolbar items with their positions
    /// </summary>
    [DataMember]
    public List<ToolbarItemConfig> Items { get; set; } = new();

    /// <summary>
    /// List of custom toolbars metadata
    /// </summary>
    [DataMember]
    public List<CustomToolbarMetadata> CustomToolbars { get; set; } = new();
}
