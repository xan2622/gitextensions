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
    /// Index of the toolbar where this item is located (0=Main, 1=Filters, 2=Scripts, 3=Custom)
    /// </summary>
    [DataMember]
    public int ToolbarIndex { get; set; }

    /// <summary>
    /// Position/order of the item within its toolbar (0-based index)
    /// </summary>
    [DataMember]
    public int Order { get; set; }
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
}
