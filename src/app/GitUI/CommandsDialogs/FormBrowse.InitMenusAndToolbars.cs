using System.IO;
using GitCommands;
using GitCommands.Settings;
using GitExtensions.Extensibility;
using GitExtensions.Extensibility.Git;
using GitExtUtils;
using GitExtUtils.GitUI.Theming;
using GitUI.Properties;
using GitUI.Shells;
using GitUI.UserControls;
using ResourceManager.Hotkey;

namespace GitUI.CommandsDialogs;

partial class FormBrowse
{
    // This file is dedicated to init logic for FormBrowse menus and toolbars

    internal static readonly string FetchPullToolbarShortcutsPrefix = "pull_shortcut_";

    // Dictionary to store original toolbar items before any manipulation
    // This preserves event handlers and allows items to be found even after they've been moved
    private readonly Dictionary<string, ToolStripItem> _originalToolbarItems = new();

    // Helper method to log to both Debug output and file
    private static void LogToolbar(string message)
    {
        System.Diagnostics.Debug.WriteLine(message);

        // Also write to file for easier debugging
        try
        {
            string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "GitExtensions", "toolbar_debug.log");
            File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}{Environment.NewLine}");
        }
        catch
        {
            // Ignore file write errors
        }
    }

    private void InitMenusAndToolbars(string? revFilter, string? pathFilter)
    {
        commandsToolStripMenuItem.DropDownOpening += CommandsToolStripMenuItem_DropDownOpening;

        InitFilters();

        toolPanel.TopToolStripPanel.MouseClick += (s, e) =>
        {
            if (e.Button == MouseButtons.Right)
            {
                _formBrowseMenus.ShowToolStripContextMenu(Cursor.Position);
            }
        };

        new ToolStripItem[]
        {
            recoverLostObjectsToolStripMenuItem, // Repository->Git maintenance->Recover lost objects
            branchSelect, // main toolbar
        }.ForEach(ColorHelper.AdaptImageLightness);

        pullToolStripMenuItem1.Tag = GitPullAction.None;
        mergeToolStripMenuItem.Tag = GitPullAction.Merge;
        rebaseToolStripMenuItem1.Tag = GitPullAction.Rebase;
        fetchToolStripMenuItem.Tag = GitPullAction.Fetch;
        fetchAllToolStripMenuItem.Tag = GitPullAction.FetchAll;
        fetchPruneAllToolStripMenuItem.Tag = GitPullAction.FetchPruneAll;

        Color toolForeColor = SystemColors.WindowText;
        BackColor = SystemColors.Window;
        ForeColor = toolForeColor;
        mainMenuStrip.ForeColor = toolForeColor;
        InitToolStripStyles(toolForeColor, Color.Transparent);

        UpdateCommitButtonAndGetBrush(status: null, AppSettings.ShowGitStatusInBrowseToolbar);

        FillNextPullActionAsDefaultToolStripMenuItems();
        RefreshDefaultPullAction();

        FillUserShells(defaultShell: BashShell.ShellName);

        // Store all original items BEFORE any manipulation
        StoreOriginalToolbarItems();

        InsertFetchPullShortcuts();

        LoadDynamicToolbarsFromConfig();

        ApplySavedToolbarLayout();

        WorkaroundToolbarLocationBug();

        return;

        void InitToolStripStyles(Color toolForeColor, Color toolBackColor)
        {
            toolPanel.TopToolStripPanel.BackColor = toolBackColor;
            toolPanel.TopToolStripPanel.ForeColor = toolForeColor;

            mainMenuStrip.BackColor = toolBackColor;

            ToolStripMain.BackColor = toolBackColor;
            ToolStripMain.ForeColor = toolForeColor;

            ToolStripFilters.BackColor = toolBackColor;
            ToolStripFilters.ForeColor = toolForeColor;
            ToolStripFilters.InitToolStripStyles(toolForeColor, toolBackColor);

            ToolStripScripts.BackColor = toolBackColor;
            ToolStripScripts.ForeColor = toolForeColor;
        }

        void InitFilters()
        {
            // ToolStripFilters.RefreshRevisionFunction() is init in UICommands_PostRepositoryChanged

            if (!string.IsNullOrWhiteSpace(revFilter))
            {
                ToolStripFilters.SetRevisionFilter(revFilter);
            }

            if (!string.IsNullOrWhiteSpace(pathFilter))
            {
                SetPathFilter(pathFilter.QuoteNE());
            }
        }

        void StoreOriginalToolbarItems()
        {
            // Store all toolbar items with their original references
            // This allows items to be found even after they've been moved to custom toolbars
            StoreItemsFromToolbar(ToolStripMain);
            StoreItemsFromToolbar(ToolStripFilters);
            StoreItemsFromToolbar(ToolStripScripts);

            LogToolbar($"[StoreOriginalToolbarItems] Stored {_originalToolbarItems.Count} items");

            void StoreItemsFromToolbar(ToolStrip toolbar)
            {
                foreach (ToolStripItem item in toolbar.Items)
                {
                    if (!string.IsNullOrWhiteSpace(item.Name) && !_originalToolbarItems.ContainsKey(item.Name))
                    {
                        _originalToolbarItems[item.Name] = item;
                        LogToolbar($"[StoreOriginalToolbarItems] Stored: {item.Name} from {toolbar.Name}");
                    }
                }
            }
        }

        void LoadDynamicToolbarsFromConfig()
        {
            ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;

            LogToolbar($"[LoadDynamicToolbarsFromConfig] Config is null: {config is null}");

            if (config is null || config.CustomToolbars is null || config.CustomToolbars.Count == 0)
            {
                LogToolbar("[LoadDynamicToolbarsFromConfig] No custom toolbars to load");
                return; // No custom toolbars to load
            }

            LogToolbar($"[LoadDynamicToolbarsFromConfig] Loading {config.CustomToolbars.Count} custom toolbars");

            // Create custom toolbars from metadata
            foreach (CustomToolbarMetadata metadata in config.CustomToolbars.OrderBy(m => m.Index))
            {
                LogToolbar($"[LoadDynamicToolbarsFromConfig] Creating toolbar: {metadata.Name}, Index: {metadata.Index}, Visible: {metadata.Visible}");

                ToolStripEx newToolStrip = new()
                {
                    Name = $"ToolStripCustom{metadata.Name.Replace("Custom ", "")}",
                    Text = metadata.Name,
                    Visible = metadata.Visible,
                    GripStyle = ToolStripGripStyle.Visible,
                    GripMargin = new System.Windows.Forms.Padding(2, 0, 2, 0),
                    BackColor = ToolStripMain.BackColor,
                    ForeColor = ToolStripMain.ForeColor
                };

                // Add to panel (will be reordered by WorkaroundToolbarLocationBug)
                toolPanel.TopToolStripPanel.Controls.Add(newToolStrip);
            }
        }

        void WorkaroundToolbarLocationBug()
        {
            // Layout engine bug (?) which may change the order of toolbars
            // if the 1st one becomes longer than the 2nd toolbar's Location.X
            // the layout engine will be place the 2nd toolbar first
            //
            // In ToolStripPanel, controls are added from RIGHT to LEFT.
            // So to get: Standard | Filters | Scripts | Custom (left to right)
            // We need to add in reverse: Custom, Scripts, Filters, Standard

            // 1. Collect custom toolbars first (they should appear on the right)
            List<ToolStrip> customToolStrips = new();
            foreach (Control control in toolPanel.TopToolStripPanel.Controls)
            {
                if (control is ToolStrip toolStrip && toolStrip.Name.StartsWith("ToolStripCustom"))
                {
                    customToolStrips.Add(toolStrip);
                }
            }

            // 2. Build the list in the order we need to ADD them
            // (reverse of visual order because ToolStripPanel adds from right to left)
            List<ToolStrip> allToolStrips = new();

            // Custom toolbars first (will appear rightmost)
            allToolStrips.AddRange(customToolStrips);

            // Then built-in toolbars in reverse visual order
            allToolStrips.Add(ToolStripScripts);  // Will appear before custom
            allToolStrips.Add(ToolStripFilters);  // Will appear before Scripts
            allToolStrips.Add(ToolStripMain);     // Will appear leftmost (Standard)

            // 3. Clear panel
            toolPanel.TopToolStripPanel.Controls.Clear();

            // 4. Add all toolbars (no need to reverse - order is already correct for adding)
            foreach (ToolStrip toolStrip in allToolStrips)
            {
                if (toolStrip.Visible)
                {
                    toolPanel.TopToolStripPanel.Controls.Add(toolStrip);
                }
            }

#if DEBUG
            // 4. Assert toolbars are on the same row
            foreach (ToolStrip toolStrip in allToolStrips.Where(ts => ts.Visible))
            {
                DebugHelpers.Assert(toolStrip.Top == 0, $"{toolStrip.Name} must be placed on the 1st row");
            }

            // 5. Assert the correct order of toolbars
            var visibleToolStrips = allToolStrips.Where(ts => ts.Visible).ToArray();
            for (int i = visibleToolStrips.Length - 1; i > 0; i--)
            {
                DebugHelpers.Assert(visibleToolStrips[i].Left < visibleToolStrips[i - 1].Left,
                    $"{visibleToolStrips[i - 1].Name} must be placed before {visibleToolStrips[i].Name}");
            }
#endif
        }

        void ApplySavedToolbarLayout()
        {
            ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;

            LogToolbar($"[ApplySavedToolbarLayout] Config is null: {config is null}");

            if (config is null || config.Items is null || config.Items.Count == 0)
            {
                LogToolbar("[ApplySavedToolbarLayout] No saved layout, use defaults");
                return; // No saved layout, use defaults
            }

            LogToolbar($"[ApplySavedToolbarLayout] Applying layout with {config.Items.Count} items");

            // Apply layout to built-in toolbars (reorganize existing items)
            ApplyLayoutToToolStrip(ToolStripMain, 0, config, isCustomToolbar: false);
            ApplyLayoutToToolStrip(ToolStripFilters, 1, config, isCustomToolbar: false);
            ApplyLayoutToToolStrip(ToolStripScripts, 2, config, isCustomToolbar: false);

            // Apply layout to custom toolbars (move items from built-in toolbars)
            if (config.CustomToolbars is not null)
            {
                LogToolbar($"[ApplySavedToolbarLayout] Processing {config.CustomToolbars.Count} custom toolbars");

                foreach (CustomToolbarMetadata metadata in config.CustomToolbars)
                {
                    ToolStrip? customToolStrip = toolPanel.TopToolStripPanel.Controls
                        .Cast<Control>()
                        .OfType<ToolStrip>()
                        .FirstOrDefault(ts => ts.Text == metadata.Name);

                    if (customToolStrip is not null)
                    {
                        LogToolbar($"[ApplySavedToolbarLayout] Applying layout to custom toolbar: {metadata.Name}");
                        ApplyLayoutToToolStrip(customToolStrip, metadata.Index, config, isCustomToolbar: true);
                    }
                    else
                    {
                        LogToolbar($"[ApplySavedToolbarLayout] ERROR: Custom toolbar not found: {metadata.Name}");
                    }
                }

                // After items have been added to custom toolbars, refresh the toolbar menus to include them
                Dictionary<string, ToolStrip> dynamicToolbars = new();
                foreach (Control control in toolPanel.TopToolStripPanel.Controls)
                {
                    if (control is ToolStrip toolStrip && toolStrip.Name.StartsWith("ToolStripCustom"))
                    {
                        dynamicToolbars[toolStrip.Name] = toolStrip;
                        LogToolbar($"[ApplySavedToolbarLayout] Added {toolStrip.Name} ({toolStrip.Text}) with {toolStrip.Items.Count} items to dynamic toolbars");
                    }
                }

                if (dynamicToolbars.Count > 0)
                {
                    LogToolbar($"[ApplySavedToolbarLayout] Refreshing toolbar menus with {dynamicToolbars.Count} dynamic toolbars");
                    _formBrowseMenus.RefreshToolbarsMenu(dynamicToolbars);
                }
            }
        }

        void ApplyLayoutToToolStrip(ToolStrip toolStrip, int toolbarIndex, ToolbarLayoutConfig config, bool isCustomToolbar)
        {
            if (toolStrip is null || config is null || config.Items is null)
            {
                return;
            }

            var itemsForToolbar = config.Items
                .Where(ic => ic.ToolbarIndex == toolbarIndex)
                .OrderBy(ic => ic.Order)
                .ToList();

            LogToolbar($"[ApplyLayoutToToolStrip] Toolbar: {toolStrip.Name}, Index: {toolbarIndex}, IsCustom: {isCustomToolbar}, Items to apply: {itemsForToolbar.Count}");

            if (isCustomToolbar)
            {
                // For custom toolbars: Move items from built-in toolbars
                int insertIndex = 0;
                foreach (ToolbarItemConfig itemConfig in itemsForToolbar)
                {
                    ToolStripItem? item = null;

                    LogToolbar($"[ApplyLayoutToToolStrip] Processing item: {itemConfig.ItemName}, Order: {itemConfig.Order}");

                    // Handle special items
                    if (itemConfig.ItemName.StartsWith("_SEPARATOR_"))
                    {
                        item = new ToolStripSeparator();
                        LogToolbar($"[ApplyLayoutToToolStrip] Created separator");
                    }
                    else if (itemConfig.ItemName.StartsWith("_SPACER_"))
                    {
                        item = new ToolStripLabel
                        {
                            Name = itemConfig.ItemName,
                            AutoSize = true,
                            Text = "     ",
                            DisplayStyle = ToolStripItemDisplayStyle.Text
                        };
                        LogToolbar($"[ApplyLayoutToToolStrip] Created spacer");
                    }
                    else
                    {
                        // Search for item in all built-in toolbars
                        item = FindItemInAllToolbars(itemConfig.ItemName);
                        LogToolbar($"[ApplyLayoutToToolStrip] FindItemInAllToolbars({itemConfig.ItemName}) returned: {(item != null ? item.Name : "NULL")}");
                    }

                    if (item != null)
                    {
                        // Remove from current owner if different
                        if (item.Owner != null && item.Owner != toolStrip)
                        {
                            LogToolbar($"[ApplyLayoutToToolStrip] Removing {item.Name} from {item.Owner.Name}");
                            item.Owner.Items.Remove(item);
                        }

                        // Add to custom toolbar at correct position
                        int targetIndex = Math.Min(insertIndex, toolStrip.Items.Count);
                        if (!toolStrip.Items.Contains(item))
                        {
                            toolStrip.Items.Insert(targetIndex, item);
                            LogToolbar($"[ApplyLayoutToToolStrip] Inserted {item.Name} at index {targetIndex} in {toolStrip.Name}");
                        }
                        else
                        {
                            LogToolbar($"[ApplyLayoutToToolStrip] Item {item.Name} already in {toolStrip.Name}");
                        }

                        insertIndex++;
                    }
                    else
                    {
                        LogToolbar($"[ApplyLayoutToToolStrip] WARNING: Item {itemConfig.ItemName} not found!");
                    }
                }
            }
            else
            {
                // For built-in toolbars: Reorganize existing items
                int insertIndex = 0;
                foreach (ToolbarItemConfig itemConfig in itemsForToolbar)
                {
                    ToolStripItem? item = toolStrip.Items.Cast<ToolStripItem>()
                        .FirstOrDefault(i => i.Name == itemConfig.ItemName);

                    if (item is not null)
                    {
                        int currentIndex = toolStrip.Items.IndexOf(item);
                        int targetIndex = Math.Min(insertIndex, toolStrip.Items.Count - 1);

                        if (currentIndex != targetIndex && currentIndex != -1)
                        {
                            toolStrip.Items.Remove(item);
                            toolStrip.Items.Insert(targetIndex, item);
                        }

                        insertIndex++;
                    }
                }
            }

            LogToolbar($"[ApplyLayoutToToolStrip] Final item count in {toolStrip.Name}: {toolStrip.Items.Count}");
        }

        ToolStripItem? FindItemInAllToolbars(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return null;
            }

            // First, try to find in the original items dictionary
            // This works even if items have been moved to custom toolbars
            if (_originalToolbarItems.TryGetValue(itemName, out ToolStripItem? originalItem))
            {
                LogToolbar($"[FindItemInAllToolbars] Found {itemName} in _originalToolbarItems");
                return originalItem;
            }

            LogToolbar($"[FindItemInAllToolbars] {itemName} NOT found in _originalToolbarItems, searching in toolbars...");

            // Fallback: search in all current toolbars
            // Search in main toolbar
            ToolStripItem? item = ToolStripMain.Items.Cast<ToolStripItem>()
                .FirstOrDefault(i => i.Name == itemName);

            if (item != null)
            {
                LogToolbar($"[FindItemInAllToolbars] Found {itemName} in ToolStripMain");
                return item;
            }

            // Search in filters toolbar
            item = ToolStripFilters.Items.Cast<ToolStripItem>()
                .FirstOrDefault(i => i.Name == itemName);

            if (item != null)
            {
                LogToolbar($"[FindItemInAllToolbars] Found {itemName} in ToolStripFilters");
                return item;
            }

            // Search in scripts toolbar
            item = ToolStripScripts.Items.Cast<ToolStripItem>()
                .FirstOrDefault(i => i.Name == itemName);

            if (item != null)
            {
                LogToolbar($"[FindItemInAllToolbars] Found {itemName} in ToolStripScripts");
                return item;
            }

            // If not found in toolbars, search in menus (for menu items added to custom toolbars)
            LogToolbar($"[FindItemInAllToolbars] {itemName} not found in toolbars, searching in menus...");
            item = FindItemInMenus(mainMenuStrip, itemName);

            if (item != null)
            {
                LogToolbar($"[FindItemInAllToolbars] Found {itemName} in menus");
            }
            else
            {
                LogToolbar($"[FindItemInAllToolbars] {itemName} NOT FOUND anywhere!");
            }

            return item;
        }

        // Recursive method to find a menu item by name
        ToolStripItem? FindItemInMenus(ToolStrip menuStrip, string itemName)
        {
            if (menuStrip == null)
            {
                return null;
            }

            // Search in top-level menu items
            foreach (ToolStripItem menuItem in menuStrip.Items)
            {
                if (menuItem.Name == itemName)
                {
                    return menuItem;
                }

                // If it's a menu item with sub-items, search recursively
                if (menuItem is ToolStripMenuItem menuItemWithSubItems && menuItemWithSubItems.DropDownItems.Count > 0)
                {
                    ToolStripItem? found = FindItemInMenuRecursive(menuItemWithSubItems, itemName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        ToolStripItem? FindItemInMenuRecursive(ToolStripMenuItem parentMenuItem, string itemName)
        {
            foreach (ToolStripItem item in parentMenuItem.DropDownItems)
            {
                if (item.Name == itemName)
                {
                    return item;
                }

                // Recursively search in sub-menus
                if (item is ToolStripMenuItem subMenuItem && subMenuItem.DropDownItems.Count > 0)
                {
                    ToolStripItem? found = FindItemInMenuRecursive(subMenuItem, itemName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }
    }

    private void UpdateTooltipWithShortcut(ToolStripItem button, Command command)
        => UpdateTooltipWithShortcut(button, GetShortcutKeys(command));

    private static void UpdateTooltipWithShortcut(ToolStripItem button, Keys keys)
        => button.ToolTipText = button.ToolTipText.UpdateSuffix(keys.ToShortcutKeyToolTipString());

    private void InsertFetchPullShortcuts()
    {
        int i = ToolStripMain.Items.IndexOf(toolStripButtonPull);
        ToolStripButton btn1 = CreateCorrespondingToolbarButton(fetchToolStripMenuItem, Command.QuickFetch);
        ToolStripButton btn2 = CreateCorrespondingToolbarButton(fetchAllToolStripMenuItem);
        ToolStripButton btn3 = CreateCorrespondingToolbarButton(fetchPruneAllToolStripMenuItem);
        ToolStripButton btn4 = CreateCorrespondingToolbarButton(mergeToolStripMenuItem, Command.QuickPull);
        ToolStripButton btn5 = CreateCorrespondingToolbarButton(rebaseToolStripMenuItem1);
        ToolStripButton btn6 = CreateCorrespondingToolbarButton(pullToolStripMenuItem1, Command.PullOrFetch);

        ToolStripMain.Items.Insert(i++, btn1);
        ToolStripMain.Items.Insert(i++, btn2);
        ToolStripMain.Items.Insert(i++, btn3);
        ToolStripMain.Items.Insert(i++, btn4);
        ToolStripMain.Items.Insert(i++, btn5);
        ToolStripMain.Items.Insert(i++, btn6);

        // Store newly created items in the original items dictionary
        _originalToolbarItems[btn1.Name] = btn1;
        _originalToolbarItems[btn2.Name] = btn2;
        _originalToolbarItems[btn3.Name] = btn3;
        _originalToolbarItems[btn4.Name] = btn4;
        _originalToolbarItems[btn5.Name] = btn5;
        _originalToolbarItems[btn6.Name] = btn6;

        ToolStripButton CreateCorrespondingToolbarButton(ToolStripMenuItem toolStripMenuItem, Command? command = null)
        {
            string toolTipText = toolStripMenuItem.Text.Replace("&", string.Empty);
            ToolStripButton clonedToolStripMenuItem = new()
            {
                Image = toolStripMenuItem.Image,
                Name = FetchPullToolbarShortcutsPrefix + toolStripMenuItem.Name,
                Size = toolStripMenuItem.Size,
                Text = toolTipText,
                ToolTipText = toolTipText.UpdateSuffix(command.HasValue ? GetShortcutKeyTooltipString(command.Value) : null),
                DisplayStyle = ToolStripItemDisplayStyle.Image,
            };

            clonedToolStripMenuItem.Click += (_, _) => toolStripMenuItem.PerformClick();
            return clonedToolStripMenuItem;
        }
    }

    private void FillNextPullActionAsDefaultToolStripMenuItems()
    {
        ToolStripDropDownMenu setDefaultPullActionDropDown = (ToolStripDropDownMenu)setDefaultPullButtonActionToolStripMenuItem.DropDown;

        // Show both Check and Image margins in a menu
        setDefaultPullActionDropDown.ShowImageMargin = true;
        setDefaultPullActionDropDown.ShowCheckMargin = true;

        // Prevent submenu from closing while options are changed
        setDefaultPullActionDropDown.Closing += (sender, args) =>
        {
            if (args.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                args.Cancel = true;
            }
        };

        IEnumerable<ToolStripItem> setDefaultPullActionDropDownItems = toolStripButtonPull.DropDownItems
            .OfType<ToolStripMenuItem>()
            .Where(tsmi => tsmi.Tag is GitPullAction)
            .Select(tsmi =>
            {
                ToolStripItem tsi = new ToolStripMenuItem
                {
                    Name = $"{tsmi.Name}SetDefault",
                    Text = tsmi.Text,
                    CheckOnClick = true,
                    Image = tsmi.Image,
                    Tag = tsmi.Tag
                };

                tsi.Click += SetDefaultPullActionMenuItemClick;

                return tsi;
            });

        setDefaultPullActionDropDown.Items.AddRange([.. setDefaultPullActionDropDownItems]);

        void SetDefaultPullActionMenuItemClick(object sender, EventArgs eventArgs)
        {
            ToolStripMenuItem clickedMenuItem = (ToolStripMenuItem)sender;
            AppSettings.DefaultPullAction = (GitPullAction)clickedMenuItem.Tag;
            RefreshDefaultPullAction();
        }
    }

    private void FillUserShells(string defaultShell)
    {
        userShell.DropDownItems.Clear();

        bool userShellAccessible = false;
        ToolStripMenuItem? selectedDefaultShell = null;
        foreach (IShellDescriptor shell in _shellProvider.GetShells())
        {
            if (!shell.HasExecutable)
            {
                continue;
            }

            ToolStripMenuItem toolStripMenuItem = new(shell.Name);
            userShell.DropDownItems.Add(toolStripMenuItem);
            toolStripMenuItem.Tag = shell;
            toolStripMenuItem.Image = shell.Icon;
            toolStripMenuItem.ToolTipText = shell.Name;
            toolStripMenuItem.Click += userShell_Click;

            if (selectedDefaultShell is null || string.Equals(shell.Name, defaultShell, StringComparison.InvariantCultureIgnoreCase))
            {
                userShellAccessible = true;
                selectedDefaultShell = toolStripMenuItem;
            }
        }

        if (selectedDefaultShell is not null)
        {
            userShell.Image = selectedDefaultShell.Image;
            userShell.ToolTipText = selectedDefaultShell.ToolTipText;
            userShell.Tag = selectedDefaultShell.Tag;
        }

        userShell.Visible = userShell.DropDownItems.Count > 0;

        // a user may have a specific shell configured in settings, but the shell is no longer available
        // set the first available shell as default
        if (userShell.Visible && !userShellAccessible)
        {
            IShellDescriptor shell = (IShellDescriptor)userShell.DropDownItems[0].Tag;
            userShell.Image = shell.Icon;
            userShell.ToolTipText = shell.Name;
            userShell.Tag = shell;
        }
    }

    private void RefreshDefaultPullAction()
    {
        if (setDefaultPullButtonActionToolStripMenuItem is null)
        {
            // We may get called while instantiating the form
            return;
        }

        GitPullAction defaultPullAction = AppSettings.DefaultPullAction;

        foreach (ToolStripMenuItem menuItem in setDefaultPullButtonActionToolStripMenuItem.DropDown.Items)
        {
            menuItem.Checked = (GitPullAction)menuItem.Tag == defaultPullAction;
        }

        switch (defaultPullAction)
        {
            case GitPullAction.Fetch:
                toolStripButtonPull.Image = fetchToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullFetch.Text;
                break;

            case GitPullAction.FetchAll:
                toolStripButtonPull.Image = fetchAllToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullFetchAll.Text;
                break;

            case GitPullAction.FetchPruneAll:
                toolStripButtonPull.Image = fetchPruneAllToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullFetchPruneAll.Text;
                break;

            case GitPullAction.Merge:
                toolStripButtonPull.Image = mergeToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullMerge.Text;
                break;

            case GitPullAction.Rebase:
                toolStripButtonPull.Image = rebaseToolStripMenuItem1.Image;
                toolStripButtonPull.ToolTipText = _pullRebase.Text;
                break;

            default:
                toolStripButtonPull.Image = pullToolStripMenuItem.Image;
                toolStripButtonPull.ToolTipText = _pullOpenDialog.Text;
                break;
        }

        UpdateTooltipWithShortcut(toolStripButtonPull, Command.QuickPullOrFetch);
    }

    private Brush UpdateCommitButtonAndGetBrush(IReadOnlyList<GitItemStatus>? status, bool showCount)
    {
        try
        {
            ToolStripMain.SuspendLayout();
            RepoStateVisualiser repoStateVisualiser = new();
            (Image image, Brush brush) = repoStateVisualiser.Invoke(status);

            if (showCount)
            {
                toolStripButtonCommit.Image = image;

                if (status is not null)
                {
                    toolStripButtonCommit.Text = $"{_commitButtonText} ({status.Count})";
                    toolStripButtonCommit.AutoSize = true;
                }
                else
                {
                    int width = toolStripButtonCommit.Width;
                    toolStripButtonCommit.Text = _commitButtonText.Text;
                    if (width > toolStripButtonCommit.Width)
                    {
                        toolStripButtonCommit.AutoSize = false;
                        toolStripButtonCommit.Width = width;
                    }
                }
            }
            else
            {
                toolStripButtonCommit.Image = repoStateVisualiser.Invoke(new List<GitItemStatus>()).image;

                toolStripButtonCommit.Text = _commitButtonText.Text;
                toolStripButtonCommit.AutoSize = true;
            }

            return brush;
        }
        finally
        {
            ToolStripMain.ResumeLayout();
        }
    }
}
