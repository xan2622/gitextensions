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

        void LoadDynamicToolbarsFromConfig()
        {
            ToolbarLayoutConfig? config = AppSettings.ToolbarLayout;

            if (config is null || config.CustomToolbars is null || config.CustomToolbars.Count == 0)
            {
                return; // No custom toolbars to load
            }

            // Create custom toolbars from metadata
            foreach (CustomToolbarMetadata metadata in config.CustomToolbars.OrderBy(m => m.Index))
            {
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

            // 1. Get all toolbars (built-in and custom)
            List<ToolStrip> allToolStrips = new() { ToolStripScripts, ToolStripFilters, ToolStripMain };

            // Add custom toolbars from the panel
            foreach (Control control in toolPanel.TopToolStripPanel.Controls)
            {
                if (control is ToolStrip toolStrip && toolStrip.Name.StartsWith("ToolStripCustom"))
                {
                    allToolStrips.Add(toolStrip);
                }
            }

            // 2. Clear all toolbars
            toolPanel.TopToolStripPanel.Controls.Clear();

            // 3. Add all toolbars in reverse order
            allToolStrips.Reverse();
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

            if (config is null || config.Items is null || config.Items.Count == 0)
            {
                return; // No saved layout, use defaults
            }

            // Apply layout to built-in toolbars (reorganize existing items)
            ApplyLayoutToToolStrip(ToolStripMain, 0, config, isCustomToolbar: false);
            ApplyLayoutToToolStrip(ToolStripFilters, 1, config, isCustomToolbar: false);
            ApplyLayoutToToolStrip(ToolStripScripts, 2, config, isCustomToolbar: false);

            // Apply layout to custom toolbars (move items from built-in toolbars)
            if (config.CustomToolbars is not null)
            {
                foreach (CustomToolbarMetadata metadata in config.CustomToolbars)
                {
                    ToolStrip? customToolStrip = toolPanel.TopToolStripPanel.Controls
                        .Cast<Control>()
                        .OfType<ToolStrip>()
                        .FirstOrDefault(ts => ts.Text == metadata.Name);

                    if (customToolStrip is not null)
                    {
                        ApplyLayoutToToolStrip(customToolStrip, metadata.Index, config, isCustomToolbar: true);
                    }
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

            if (isCustomToolbar)
            {
                // For custom toolbars: Move items from built-in toolbars
                int insertIndex = 0;
                foreach (ToolbarItemConfig itemConfig in itemsForToolbar)
                {
                    ToolStripItem? item = null;

                    // Handle special items
                    if (itemConfig.ItemName.StartsWith("_SEPARATOR_"))
                    {
                        item = new ToolStripSeparator();
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
                    }
                    else
                    {
                        // Search for item in all built-in toolbars
                        item = FindItemInAllToolbars(itemConfig.ItemName);
                    }

                    if (item != null)
                    {
                        // Remove from current owner if different
                        if (item.Owner != null && item.Owner != toolStrip)
                        {
                            item.Owner.Items.Remove(item);
                        }

                        // Add to custom toolbar at correct position
                        int targetIndex = Math.Min(insertIndex, toolStrip.Items.Count);
                        if (!toolStrip.Items.Contains(item))
                        {
                            toolStrip.Items.Insert(targetIndex, item);
                        }

                        insertIndex++;
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
        }

        ToolStripItem? FindItemInAllToolbars(string itemName)
        {
            if (string.IsNullOrWhiteSpace(itemName))
            {
                return null;
            }

            // Search in main toolbar
            ToolStripItem? item = ToolStripMain.Items.Cast<ToolStripItem>()
                .FirstOrDefault(i => i.Name == itemName);

            if (item != null)
            {
                return item;
            }

            // Search in filters toolbar
            item = ToolStripFilters.Items.Cast<ToolStripItem>()
                .FirstOrDefault(i => i.Name == itemName);

            if (item != null)
            {
                return item;
            }

            // Search in scripts toolbar
            item = ToolStripScripts.Items.Cast<ToolStripItem>()
                .FirstOrDefault(i => i.Name == itemName);

            return item;
        }
    }

    private void UpdateTooltipWithShortcut(ToolStripItem button, Command command)
        => UpdateTooltipWithShortcut(button, GetShortcutKeys(command));

    private static void UpdateTooltipWithShortcut(ToolStripItem button, Keys keys)
        => button.ToolTipText = button.ToolTipText.UpdateSuffix(keys.ToShortcutKeyToolTipString());

    private void InsertFetchPullShortcuts()
    {
        int i = ToolStripMain.Items.IndexOf(toolStripButtonPull);
        ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchToolStripMenuItem, Command.QuickFetch));
        ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchAllToolStripMenuItem));
        ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(fetchPruneAllToolStripMenuItem));
        ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(mergeToolStripMenuItem, Command.QuickPull));
        ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(rebaseToolStripMenuItem1));
        ToolStripMain.Items.Insert(i++, CreateCorrespondingToolbarButton(pullToolStripMenuItem1, Command.PullOrFetch));

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
