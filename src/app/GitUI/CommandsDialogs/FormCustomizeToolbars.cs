using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands;
using GitCommands.Settings;
using GitUI.UserControls;

namespace GitUI.CommandsDialogs
{
    public partial class FormCustomizeToolbars : Form
    {
        private readonly FormBrowse _formBrowse;
        private readonly FormBrowseMenus? _formBrowseMenus;
        private readonly Dictionary<string, List<ToolStripItemWrapper>> _toolbarItems = new();
        private readonly Dictionary<string, ToolStrip> _dynamicToolbars = new();
        private readonly Dictionary<string, ToolStripItem> _originalItems = new(); // Store original items to preserve event handlers
        private string _currentToolbarName = "Standard";

        public FormCustomizeToolbars(FormBrowse formBrowse, FormBrowseMenus? formBrowseMenus = null)
        {
            _formBrowse = formBrowse;
            _formBrowseMenus = formBrowseMenus;
            InitializeComponent();

            // Initialize toolbar combo box with only built-in toolbars
            comboBoxToolbar.Items.AddRange(new object[] { "Standard", "Filters", "Scripts" });
            comboBoxToolbar.SelectedIndex = 0;

            // Initialize category combo box
            comboBoxCategory.Items.AddRange(new object[]
            {
                "All Actions",
                "Start",
                "Repository",
                "Navigate",
                "View",
                "Commands",
                "GitHub",
                "Plugins",
                "Tools",
                "Help",
                "Right click menu",
                "Default Standard toolbar",
                "Default Filters toolbar"
            });
            comboBoxCategory.SelectedIndex = 0; // Default: All Actions

            // Initialize display mode combo box
            comboBoxDisplayMode.SelectedIndex = 1; // Default: "Icons and text"

            // Add triple-click support to search textboxes
            textBoxFilterAvailable.MouseClick += TextBox_TripleClick;
            textBoxFilterCurrent.MouseClick += TextBox_TripleClick;

            // Add event handlers for ListBox selection changes
            listBoxAvailable.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            listBoxCurrent.SelectedIndexChanged += ListBox_SelectedIndexChanged;
            listBoxAvailable.Click += (s, e) => ListBox_SelectedIndexChanged(s, e);
            listBoxCurrent.Click += (s, e) => ListBox_SelectedIndexChanged(s, e);

            // Update button states
            UpdateToolbarButtons();

            LoadToolbarItems();
            LoadCurrentLayout();
        }

        private void ListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Disable Up/Down buttons if left ListBox (Available actions) is selected
            if (sender == listBoxAvailable)
            {
                buttonMoveUp.Enabled = false;
                buttonMoveDown.Enabled = false;
                buttonRemove.Enabled = false; // Disable Remove button when left ListBox is selected

                // Enable Add button only if there's a selection in the left ListBox
                buttonAdd.Enabled = listBoxAvailable.SelectedIndex >= 0;

                // Enable AddAll button if there are items other than separator and spacer
                buttonAddAll.Enabled = listBoxAvailable.Items.Count > 2;
            }
            else if (sender == listBoxCurrent)
            {
                // Enable Up/Down buttons if right ListBox (Current actions) has a selection
                int index = listBoxCurrent.SelectedIndex;
                buttonMoveUp.Enabled = index > 0;
                buttonMoveDown.Enabled = index >= 0 && index < listBoxCurrent.Items.Count - 1;

                // Enable Remove button only if there's a selection in the right ListBox
                buttonRemove.Enabled = index >= 0 && listBoxCurrent.Items.Count > 0;

                // Disable Add button when right ListBox is selected
                buttonAdd.Enabled = false;

                // Enable AddAll button if there are items other than separator and spacer
                buttonAddAll.Enabled = listBoxAvailable.Items.Count > 2;
            }
        }

        private void LoadToolbarItems()
        {
            // Store original items first to preserve event handlers
            StoreOriginalItems(_formBrowse.ToolStripMain);
            StoreOriginalItems(_formBrowse.ToolStripFilters);
            StoreOriginalItems(_formBrowse.ToolStripScripts);

            // Load all items from built-in toolbars only
            _toolbarItems["Standard"] = GetToolStripItems(_formBrowse.ToolStripMain);
            _toolbarItems["Filters"] = GetToolStripItems(_formBrowse.ToolStripFilters);
            _toolbarItems["Scripts"] = GetToolStripItems(_formBrowse.ToolStripScripts);
        }

        private void StoreOriginalItems(ToolStrip? toolStrip)
        {
            if (toolStrip == null)
            {
                return;
            }

            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (!string.IsNullOrWhiteSpace(item.Name) && !_originalItems.ContainsKey(item.Name))
                {
                    _originalItems[item.Name] = item;
                }
            }
        }

        private void RestoreOriginalToolbarLayouts()
        {
            // Clear all toolbars first
            _formBrowse.ToolStripMain.Items.Clear();
            _formBrowse.ToolStripFilters.Items.Clear();
            _formBrowse.ToolStripScripts.Items.Clear();

            // Restore items to their original toolbars based on stored layout
            foreach (ToolStripItemWrapper wrapper in _toolbarItems["Standard"])
            {
                if (wrapper.Item != null && _originalItems.ContainsKey(wrapper.Item.Name))
                {
                    _formBrowse.ToolStripMain.Items.Add(_originalItems[wrapper.Item.Name]);
                }
            }

            foreach (ToolStripItemWrapper wrapper in _toolbarItems["Filters"])
            {
                if (wrapper.Item != null && _originalItems.ContainsKey(wrapper.Item.Name))
                {
                    _formBrowse.ToolStripFilters.Items.Add(_originalItems[wrapper.Item.Name]);
                }
            }

            foreach (ToolStripItemWrapper wrapper in _toolbarItems["Scripts"])
            {
                if (wrapper.Item != null && _originalItems.ContainsKey(wrapper.Item.Name))
                {
                    _formBrowse.ToolStripScripts.Items.Add(_originalItems[wrapper.Item.Name]);
                }
            }
        }

        private List<ToolStripItemWrapper> GetToolStripItems(ToolStrip? toolStrip)
        {
            if (toolStrip == null)
            {
                return new List<ToolStripItemWrapper>();
            }

            List<ToolStripItemWrapper> items = new();
            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item is ToolStripSeparator)
                {
                    // Add separator as wrapper with null item
                    items.Add(new ToolStripItemWrapper(null, "--- separator ---"));
                }
                else if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    items.Add(new ToolStripItemWrapper(item));
                }
            }

            return items;
        }

        private void LoadCurrentLayout()
        {
            listBoxAvailable.Items.Clear();
            listBoxCurrent.Items.Clear();

            if (!_toolbarItems.ContainsKey(_currentToolbarName))
            {
                return;
            }

            List<ToolStripItemWrapper> currentItems = _toolbarItems[_currentToolbarName];

            // Add current items to the right list
            foreach (ToolStripItemWrapper wrapper in currentItems)
            {
                listBoxCurrent.Items.Add(wrapper);
            }

            // Load available actions from menu based on selected category
            FilterAvailableActionsByCategory();

            // Initialize button states (all buttons disabled initially)
            buttonMoveUp.Enabled = false;
            buttonMoveDown.Enabled = false;
            buttonRemove.Enabled = false;
            buttonAddAll.Enabled = listBoxAvailable.Items.Count > 2;
        }

        private void ComboBoxToolbar_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Save current layout before switching
            if (!string.IsNullOrEmpty(_currentToolbarName) && _toolbarItems.ContainsKey(_currentToolbarName))
            {
                SaveCurrentToolbarLayout();
            }

            _currentToolbarName = comboBoxToolbar.SelectedItem?.ToString() ?? "Standard";
            LoadCurrentLayout();
            UpdateToolbarButtons();
        }

        private void ButtonClearCurrent_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Remove all items from the current toolbar?",
                "Clear Toolbar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Clear all items from current toolbar
                listBoxCurrent.Items.Clear();

                // Refresh the available actions list to show all items that match the current category
                FilterAvailableActionsByCategory();

                // Update button states since ListBox is now empty
                buttonMoveUp.Enabled = false;
                buttonMoveDown.Enabled = false;
                buttonRemove.Enabled = false;
            }
        }

        private void ButtonAddToolbar_Click(object? sender, EventArgs e)
        {
            // Find next available custom toolbar number (starting from 01)
            int nextNumber = 1;
            while (comboBoxToolbar.Items.Cast<string>().Any(name => name == $"Custom {nextNumber:D2}"))
            {
                nextNumber++;
            }

            string newToolbarName = $"Custom {nextNumber:D2}";
            comboBoxToolbar.Items.Add(newToolbarName);

            // Create a new physical ToolStrip for this custom toolbar
            ToolStripEx newToolStrip = new()
            {
                Name = $"ToolStripCustom{nextNumber:D2}",
                Text = newToolbarName,
                Visible = true,
                GripStyle = ToolStripGripStyle.Visible,
                GripMargin = new System.Windows.Forms.Padding(2, 0, 2, 0),
                BackColor = _formBrowse.ToolStripMain.BackColor,
                ForeColor = _formBrowse.ToolStripMain.ForeColor
            };

            // Store the new toolbar
            _dynamicToolbars[newToolbarName] = newToolStrip;

            // Initialize empty toolbar items list
            _toolbarItems[newToolbarName] = new List<ToolStripItemWrapper>();

            // Switch to the new toolbar (this will trigger ComboBoxToolbar_SelectedIndexChanged)
            comboBoxToolbar.SelectedItem = newToolbarName;
        }

        private void ButtonRemoveToolbar_Click(object? sender, EventArgs e)
        {
            if (_currentToolbarName.StartsWith("Custom "))
            {
                DialogResult result = MessageBox.Show(
                    $"Delete toolbar '{_currentToolbarName}'?",
                    "Delete Toolbar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Get the physical toolbar to remove from dynamic toolbars
                    if (_dynamicToolbars.TryGetValue(_currentToolbarName, out ToolStrip? toolStripToRemove))
                    {
                        // Remove the physical toolbar from its parent
                        toolStripToRemove.Parent?.Controls.Remove(toolStripToRemove);
                        toolStripToRemove.Items.Clear();
                        toolStripToRemove.Visible = false;
                        toolStripToRemove.Dispose();

                        _dynamicToolbars.Remove(_currentToolbarName);
                    }

                    comboBoxToolbar.Items.Remove(_currentToolbarName);
                    _toolbarItems.Remove(_currentToolbarName);
                    comboBoxToolbar.SelectedIndex = 0;

                    // Refresh the toolbars menu to remove the deleted toolbar
                    _formBrowseMenus?.RefreshToolbarsMenu(_dynamicToolbars);
                }
            }
        }

        private void UpdateToolbarButtons()
        {
            // Disable remove button for built-in toolbars
            buttonRemoveToolbar.Enabled = _currentToolbarName.StartsWith("Custom ");
        }

        private void ButtonToolbarLayout_Click(object? sender, EventArgs e)
        {
            MessageBox.Show(
                "This feature will allow you to customize the position and layout of toolbars.\n\nComing soon!",
                "Toolbar Layout",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void ComboBoxDisplayMode_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Get the currently selected toolbar
            ToolStrip? currentToolbar = GetToolStripByName(_currentToolbarName);
            if (currentToolbar == null)
            {
                return;
            }

            // Determine the display style based on selection
            ToolStripItemDisplayStyle displayStyle = comboBoxDisplayMode.SelectedIndex switch
            {
                0 => ToolStripItemDisplayStyle.Image, // Icons only
                1 => ToolStripItemDisplayStyle.ImageAndText, // Icons and text
                _ => ToolStripItemDisplayStyle.ImageAndText
            };

            // Apply the display style to all items in the current toolbar
            foreach (ToolStripItem item in currentToolbar.Items)
            {
                // Skip separators and labels (they don't have images)
                if (item is not ToolStripSeparator && item is not ToolStripLabel)
                {
                    item.DisplayStyle = displayStyle;
                }
            }
        }

        private void ButtonAdd_Click(object? sender, EventArgs e)
        {
            if (listBoxAvailable.SelectedItem is ToolStripItemWrapper wrapper)
            {
                int currentIndex = listBoxAvailable.SelectedIndex;
                ToolStripItemWrapper itemToAdd;

                // Create a new instance for special items (separator, spacer)
                if (wrapper.DisplayName == "--- separator ---" || wrapper.DisplayName == "--- expanding spacer ---")
                {
                    itemToAdd = new ToolStripItemWrapper(null, wrapper.DisplayName);
                    listBoxCurrent.Items.Add(itemToAdd);

                    // Don't remove from available list - keep selection on same item
                }
                else
                {
                    itemToAdd = wrapper;
                    listBoxCurrent.Items.Add(itemToAdd);
                    listBoxAvailable.Items.Remove(wrapper);

                    // Select next item in left ListBox if available, otherwise select previous
                    if (listBoxAvailable.Items.Count > 0)
                    {
                        if (currentIndex < listBoxAvailable.Items.Count)
                        {
                            listBoxAvailable.SelectedIndex = currentIndex;
                        }
                        else
                        {
                            listBoxAvailable.SelectedIndex = listBoxAvailable.Items.Count - 1;
                        }
                    }
                }

                // Select the newly added item in the right ListBox
                listBoxCurrent.SelectedItem = itemToAdd;
            }
        }

        private void ButtonAddAll_Click(object? sender, EventArgs e)
        {
            // Collect all items to add (except separator and spacer)
            List<ToolStripItemWrapper> itemsToAdd = new();

            foreach (ToolStripItemWrapper wrapper in listBoxAvailable.Items)
            {
                if (wrapper.DisplayName != "--- separator ---" && wrapper.DisplayName != "--- expanding spacer ---")
                {
                    itemsToAdd.Add(wrapper);
                }
            }

            // Add all items to current list
            foreach (ToolStripItemWrapper wrapper in itemsToAdd)
            {
                listBoxCurrent.Items.Add(wrapper);
            }

            // Refresh the available actions list
            FilterAvailableActionsByCategory();

            // Select the first newly added item in the right ListBox if any were added
            if (itemsToAdd.Count > 0 && listBoxCurrent.Items.Count > 0)
            {
                listBoxCurrent.SelectedIndex = listBoxCurrent.Items.Count - itemsToAdd.Count;
            }
        }

        private void ButtonRemove_Click(object? sender, EventArgs e)
        {
            if (listBoxCurrent.SelectedItem is ToolStripItemWrapper wrapper)
            {
                int currentIndex = listBoxCurrent.SelectedIndex;
                listBoxCurrent.Items.Remove(wrapper);

                // Refresh the available actions list to show the removed item if it matches the current category
                FilterAvailableActionsByCategory();

                // Select next item if available, otherwise select previous
                if (listBoxCurrent.Items.Count > 0)
                {
                    if (currentIndex < listBoxCurrent.Items.Count)
                    {
                        listBoxCurrent.SelectedIndex = currentIndex;
                    }
                    else
                    {
                        listBoxCurrent.SelectedIndex = listBoxCurrent.Items.Count - 1;
                    }
                }
                else
                {
                    // No items left, disable buttons
                    buttonMoveUp.Enabled = false;
                    buttonMoveDown.Enabled = false;
                    buttonRemove.Enabled = false;
                }
            }
        }

        private void ButtonMoveUp_Click(object? sender, EventArgs e)
        {
            int index = listBoxCurrent.SelectedIndex;
            if (index > 0)
            {
                object item = listBoxCurrent.Items[index];
                listBoxCurrent.Items.RemoveAt(index);
                listBoxCurrent.Items.Insert(index - 1, item);
                listBoxCurrent.SelectedIndex = index - 1;

                // Update button states
                ListBox_SelectedIndexChanged(listBoxCurrent, EventArgs.Empty);
            }
        }

        private void ButtonMoveDown_Click(object? sender, EventArgs e)
        {
            int index = listBoxCurrent.SelectedIndex;
            if (index >= 0 && index < listBoxCurrent.Items.Count - 1)
            {
                object item = listBoxCurrent.Items[index];
                listBoxCurrent.Items.RemoveAt(index);
                listBoxCurrent.Items.Insert(index + 1, item);
                listBoxCurrent.SelectedIndex = index + 1;

                // Update button states
                ListBox_SelectedIndexChanged(listBoxCurrent, EventArgs.Empty);
            }
        }

        private void ListBoxAvailable_DoubleClick(object? sender, EventArgs e)
        {
            ButtonAdd_Click(sender, e);
        }

        private void ListBoxCurrent_DoubleClick(object? sender, EventArgs e)
        {
            ButtonRemove_Click(sender, e);
        }

        private void ButtonDefaults_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "This will reset all toolbars to their default layout. Continue?",
                "Reset to Defaults",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Clear saved layout
                AppSettings.ToolbarLayout = new ToolbarLayoutConfig();

                // Reload default layouts for built-in toolbars only
                _toolbarItems["Standard"] = GetToolStripItems(_formBrowse.ToolStripMain);
                _toolbarItems["Filters"] = GetToolStripItems(_formBrowse.ToolStripFilters);
                _toolbarItems["Scripts"] = GetToolStripItems(_formBrowse.ToolStripScripts);

                LoadCurrentLayout();
            }
        }

        private void ButtonOK_Click(object? sender, EventArgs e)
        {
            ApplyChanges();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonApply_Click(object? sender, EventArgs e)
        {
            ApplyChanges();
        }

        private void SaveCurrentToolbarLayout()
        {
            // Save the current ListBox contents to _toolbarItems for the current toolbar
            List<ToolStripItemWrapper> items = listBoxCurrent.Items.Cast<ToolStripItemWrapper>().ToList();
            _toolbarItems[_currentToolbarName] = items;
        }

        private void TextBoxFilterAvailable_TextChanged(object? sender, EventArgs e)
        {
            // If filter is cleared, reload all actions from the current category
            if (string.IsNullOrWhiteSpace(textBoxFilterAvailable.Text))
            {
                listBoxAvailable.Tag = null;
                FilterAvailableActionsByCategory();
            }
            else
            {
                FilterListBox(listBoxAvailable, textBoxFilterAvailable.Text);
            }
        }

        private void TextBoxFilterCurrent_TextChanged(object? sender, EventArgs e)
        {
            // If filter is cleared, reload all items from the current toolbar
            if (string.IsNullOrWhiteSpace(textBoxFilterCurrent.Text))
            {
                listBoxCurrent.Tag = null;

                // Reload current toolbar items
                listBoxCurrent.BeginUpdate();
                listBoxCurrent.Items.Clear();

                if (_toolbarItems.ContainsKey(_currentToolbarName))
                {
                    foreach (ToolStripItemWrapper wrapper in _toolbarItems[_currentToolbarName])
                    {
                        listBoxCurrent.Items.Add(wrapper);
                    }
                }

                listBoxCurrent.EndUpdate();
            }
            else
            {
                FilterListBox(listBoxCurrent, textBoxFilterCurrent.Text);
            }
        }

        private void FilterListBox(ListBox listBox, string filterText)
        {
            // Store all items if not already stored
            if (listBox.Tag is not List<ToolStripItemWrapper> allItems)
            {
                allItems = listBox.Items.Cast<ToolStripItemWrapper>().ToList();
                listBox.Tag = allItems;
            }

            // Filter items
            listBox.BeginUpdate();
            listBox.Items.Clear();

            if (string.IsNullOrWhiteSpace(filterText))
            {
                // Show all items
                listBox.Items.AddRange(allItems.ToArray());
            }
            else
            {
                // Show only matching items
                var filtered = allItems.Where(item =>
                    item.DisplayName.Contains(filterText, StringComparison.OrdinalIgnoreCase)).ToArray();
                listBox.Items.AddRange(filtered);
            }

            listBox.EndUpdate();
        }

        private void ApplyChanges()
        {
            // First, save the current toolbar's layout from the ListBox
            SaveCurrentToolbarLayout();

            // Restore all original items to their original toolbars first
            RestoreOriginalToolbarLayouts();

            // Build the new layout from all toolbars
            ToolbarLayoutConfig config = new();

            // Get all toolbar names (including dynamically created ones)
            List<string> allToolbarNames = comboBoxToolbar.Items.Cast<string>().ToList();

            foreach (string toolbarName in allToolbarNames)
            {
                ToolStrip? toolStrip = GetToolStripByName(toolbarName);
                if (toolStrip == null)
                {
                    continue;
                }

                // Add dynamic toolbars to the FormBrowse panel if not already added
                if (_dynamicToolbars.ContainsKey(toolbarName) && toolStrip.Parent == null)
                {
                    // Access the toolPanel from FormBrowse
                    Control? toolPanelContainer = _formBrowse.Controls.Cast<Control>()
                        .FirstOrDefault(c => c is ToolStripContainer);

                    if (toolPanelContainer is ToolStripContainer toolPanel)
                    {
                        toolPanel.TopToolStripPanel.Join(toolStrip, toolPanel.TopToolStripPanel.Rows.Length);
                    }
                }

                // Clear current items
                toolStrip.Items.Clear();

                // Get items from saved layout
                List<ToolStripItemWrapper> wrappers;

                if (_toolbarItems.ContainsKey(toolbarName))
                {
                    wrappers = _toolbarItems[toolbarName];
                }
                else
                {
                    wrappers = new List<ToolStripItemWrapper>();
                }

                int order = 0;
                foreach (ToolStripItemWrapper wrapper in wrappers)
                {
                    ToolStripItem? itemToAdd = null;

                    // Handle special items (separator, expanding spacer)
                    if (wrapper.Item == null)
                    {
                        if (wrapper.DisplayName == "--- separator ---")
                        {
                            itemToAdd = new ToolStripSeparator();
                        }
                        else if (wrapper.DisplayName == "--- expanding spacer ---")
                        {
                            // Create a special label that will expand to fill space
                            // ToolStripLabel doesn't have Spring property in this .NET version
                            // We'll use a label with fixed width that acts as a spacer
                            itemToAdd = new ToolStripLabel
                            {
                                Name = $"expandingSpacer_{order}",
                                AutoSize = true,
                                Text = "     ",
                                DisplayStyle = ToolStripItemDisplayStyle.Text
                            };
                        }
                    }
                    else
                    {
                        // Use the original item from _originalItems to preserve event handlers
                        if (!string.IsNullOrWhiteSpace(wrapper.Item.Name) && _originalItems.ContainsKey(wrapper.Item.Name))
                        {
                            itemToAdd = _originalItems[wrapper.Item.Name];

                            // Remove from current owner if different
                            if (itemToAdd.Owner != null && itemToAdd.Owner != toolStrip)
                            {
                                itemToAdd.Owner.Items.Remove(itemToAdd);
                            }
                        }
                        else
                        {
                            itemToAdd = wrapper.Item;
                        }
                    }

                    if (itemToAdd != null)
                    {
                        toolStrip.Items.Add(itemToAdd);

                        if (!string.IsNullOrWhiteSpace(itemToAdd.Name))
                        {
                            config.Items.Add(new ToolbarItemConfig
                            {
                                ItemName = itemToAdd.Name,
                                ToolbarIndex = GetToolbarIndex(toolbarName),
                                Order = order++
                            });
                        }
                    }
                }

                // Hide custom toolbars if they are empty
                if (toolbarName.StartsWith("Custom "))
                {
                    toolStrip.Visible = toolStrip.Items.Count > 0;
                }
            }

            // Save the configuration
            AppSettings.ToolbarLayout = config;

            // Refresh the toolbars menu to show dynamically created toolbars
            _formBrowseMenus?.RefreshToolbarsMenu(_dynamicToolbars);
        }

        private ToolStrip? GetToolStripByName(string name)
        {
            return name switch
            {
                "Standard" => _formBrowse.ToolStripMain,
                "Filters" => _formBrowse.ToolStripFilters,
                "Scripts" => _formBrowse.ToolStripScripts,
                _ => name.StartsWith("Custom ") && _dynamicToolbars.TryGetValue(name, out ToolStrip? toolStrip) ? toolStrip : null
            };
        }

        private int GetToolbarIndex(string name)
        {
            if (name == "Standard")
            {
                return 0;
            }

            if (name == "Filters")
            {
                return 1;
            }

            if (name == "Scripts")
            {
                return 2;
            }

            if (name == "Custom 01")
            {
                return 3;
            }

            // For dynamic toolbars (Custom 02, Custom 03, etc.)
            if (name.StartsWith("Custom "))
            {
                // Extract number from "Custom XX"
                if (int.TryParse(name.Substring(7), out int number))
                {
                    return 3 + number - 1; // Custom 01 = 3, Custom 02 = 4, etc.
                }
            }

            return 0;
        }

        private class ToolStripItemWrapper
        {
            public ToolStripItem? Item { get; }
            public string DisplayName { get; }

            public ToolStripItemWrapper(ToolStripItem? item, string? displayName = null)
            {
                Item = item;
                DisplayName = displayName ?? GetDisplayName(item);
            }

            private static string GetDisplayName(ToolStripItem? item)
            {
                if (item is null)
                {
                    return "Unknown";
                }

                if (item is ToolStripSeparator)
                {
                    return "--- separator ---";
                }

                // Use Text if available
                if (!string.IsNullOrWhiteSpace(item.Text))
                {
                    // Remove & character used for keyboard shortcuts in menus
                    return item.Text.Replace("&", "");
                }

                // Try to get friendly name from Name property
                if (!string.IsNullOrWhiteSpace(item.Name))
                {
                    return GetFriendlyName(item.Name);
                }

                // Fallback to class name
                return $"[{item.GetType().Name}]";
            }

            private static string GetFriendlyName(string name)
            {
                // Map of known names to friendly names
                var friendlyNames = new Dictionary<string, string>
                {
                    // Main toolbar items
                    { "RefreshButton", "Refresh" },
                    { "toggleLeftPanel", "Toggle left panel" },
                    { "toggleSplitViewLayout", "Toggle split view layout" },
                    { "menuCommitInfoPosition", "Commit info position" },
                    { "toolStripButtonLevelUp", "Level up" },
                    { "_NO_TRANSLATE_WorkingDir", "Working directory" },
                    { "branchSelect", "Branch selector" },
                    { "toolStripSplitStash", "Stash" },
                    { "toolStripButtonCommit", "Commit" },
                    { "toolStripButtonPull", "Pull" },
                    { "toolStripButtonPush", "Push" },
                    { "toolStripFileExplorer", "File Explorer" },
                    { "userShell", "Shell" },
                    { "EditSettings", "Settings" },

                    // Filter toolbar items
                    { "tsbShowReflog", "Show all reflog references" },
                    { "tsbRevisionFilter", "Advanced filter" },
                    { "tsmiShowOnlyFirstParent", "Show only first parent" },
                    { "ToolStripLabel1", "Branch type" },
                    { "ToolStripLabel2", "Filter type" },
                    { "tscboBranchFilter", "Show all branches" },
                    { "toolStripTextBoxFilter", "Text Filter" }
                };

                if (friendlyNames.TryGetValue(name, out string? friendlyName))
                {
                    return friendlyName;
                }

                // Try to convert camelCase or PascalCase to readable format
                return ConvertToReadable(name);
            }

            private static string ConvertToReadable(string name)
            {
                // Remove common prefixes
                name = name.Replace("toolStrip", "")
                          .Replace("ToolStrip", "")
                          .Replace("Button", "")
                          .Replace("Split", "")
                          .Replace("tsb", "")
                          .Replace("tsmi", "");

                // Insert spaces before capital letters
                var result = System.Text.RegularExpressions.Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");
                result = System.Text.RegularExpressions.Regex.Replace(result, "([A-Z]+)([A-Z][a-z])", "$1 $2");

                // Capitalize first letter
                if (result.Length > 0)
                {
                    result = char.ToUpper(result[0]) + result.Substring(1);
                }

                return result;
            }

            public override string ToString() => DisplayName;
        }

        private void ComboBoxCategory_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Clear search filters when changing category
            textBoxFilterAvailable.Clear();

            FilterAvailableActionsByCategory();
        }

        private void FilterAvailableActionsByCategory()
        {
            string? selectedCategory = comboBoxCategory.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(selectedCategory))
            {
                return;
            }

            // Get all items from main menu
            MenuStrip? mainMenu = _formBrowse.mainMenuStrip;
            if (mainMenu == null)
            {
                return;
            }

            // Get currently used item names from all toolbars
            HashSet<string> usedItemNames = new();

            // First, add items from saved toolbar items
            foreach (var toolbarItems in _toolbarItems.Values)
            {
                foreach (ToolStripItemWrapper wrapper in toolbarItems)
                {
                    if (wrapper.Item != null && !string.IsNullOrWhiteSpace(wrapper.Item.Name))
                    {
                        usedItemNames.Add(wrapper.Item.Name);
                    }
                }
            }

            // Then, add items currently in listBoxCurrent (unsaved changes for current toolbar)
            foreach (ToolStripItemWrapper wrapper in listBoxCurrent.Items.Cast<ToolStripItemWrapper>())
            {
                if (wrapper.Item != null && !string.IsNullOrWhiteSpace(wrapper.Item.Name))
                {
                    usedItemNames.Add(wrapper.Item.Name);
                }
            }

            // Clear and rebuild Available Actions based on category
            listBoxAvailable.BeginUpdate();
            listBoxAvailable.Items.Clear();

            // Reset Tag to null (used by FilterListBox)
            listBoxAvailable.Tag = null;

            // Always add special items
            listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, "--- separator ---"));
            listBoxAvailable.Items.Add(new ToolStripItemWrapper(null, "--- expanding spacer ---"));

            // Add menu items based on category, excluding already used items
            HashSet<string> addedItemNames = new();

            if (selectedCategory == "All Actions")
            {
                // Show all menu items from all categories
                foreach (ToolStripMenuItem menuItem in mainMenu.Items.OfType<ToolStripMenuItem>())
                {
                    AddMenuItemsRecursively(menuItem, usedItemNames, addedItemNames);
                }

                // Also add toolbar items that might not be in menus
                AddToolbarOnlyItems(usedItemNames, addedItemNames);
            }
            else if (selectedCategory == "Default Standard toolbar")
            {
                // Show all items from the Standard toolbar (ToolStripMain)
                // Don't filter by usedItemNames for default toolbars - allow duplicates
                AddItemsFromToolbar(_formBrowse.ToolStripMain, new HashSet<string>(), addedItemNames);
            }
            else if (selectedCategory == "Default Filters toolbar")
            {
                // Show all items from the Filters toolbar (ToolStripFilters)
                // Don't filter by usedItemNames for default toolbars - allow duplicates
                AddItemsFromToolbar(_formBrowse.ToolStripFilters, new HashSet<string>(), addedItemNames);
            }
            else if (selectedCategory == "Right click menu")
            {
                // For now, show all actions (right-click menu items can be added later if needed)
                foreach (ToolStripMenuItem menuItem in mainMenu.Items.OfType<ToolStripMenuItem>())
                {
                    AddMenuItemsRecursively(menuItem, usedItemNames, addedItemNames);
                }

                AddToolbarOnlyItems(usedItemNames, addedItemNames);
            }
            else
            {
                // Special handling for Plugins category
                if (selectedCategory == "Plugins")
                {
                    // First, add items from Plugins menu itself
                    ToolStripMenuItem? pluginsMenu = mainMenu.Items.OfType<ToolStripMenuItem>()
                        .FirstOrDefault(item => item.Text.Replace("&", "") == "Plugins");

                    if (pluginsMenu != null)
                    {
                        AddMenuItemsRecursively(pluginsMenu, usedItemNames, addedItemNames);
                    }

                    // Then, add plugin items from Tools menu (they are dynamically added between separators)
                    ToolStripMenuItem? toolsMenu = mainMenu.Items.OfType<ToolStripMenuItem>()
                        .FirstOrDefault(item => item.Text.Replace("&", "") == "Tools");

                    if (toolsMenu != null)
                    {
                        // Find plugin items in Tools menu - they are typically between first separator and "PuTTY" item
                        bool inPluginSection = false;
                        foreach (ToolStripItem toolItem in toolsMenu.DropDownItems)
                        {
                            if (toolItem is ToolStripSeparator)
                            {
                                if (!inPluginSection)
                                {
                                    inPluginSection = true; // Start of plugin section
                                }
                                else
                                {
                                    break; // End of plugin section (second separator)
                                }

                                continue;
                            }

                            // Add items in plugin section, but skip PuTTY submenu and other non-plugin items
                            if (inPluginSection && toolItem is ToolStripMenuItem pluginItem)
                            {
                                string itemText = pluginItem.Text.Replace("&", "");

                                // Skip PuTTY submenu and other known non-plugin items
                                if (itemText == "PuTTY" || itemText == "Git bash" || itemText == "Extensions")
                                {
                                    continue;
                                }

                                if (!string.IsNullOrWhiteSpace(pluginItem.Name) &&
                                    !usedItemNames.Contains(pluginItem.Name) &&
                                    !addedItemNames.Contains(pluginItem.Name))
                                {
                                    listBoxAvailable.Items.Add(new ToolStripItemWrapper(pluginItem));
                                    addedItemNames.Add(pluginItem.Name);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Find the matching top-level menu item
                    ToolStripMenuItem? categoryMenu = mainMenu.Items.OfType<ToolStripMenuItem>()
                        .FirstOrDefault(item => item.Text.Replace("&", "") == selectedCategory);

                    if (categoryMenu != null)
                    {
                        AddMenuItemsRecursively(categoryMenu, usedItemNames, addedItemNames);
                    }
                }
            }

            listBoxAvailable.EndUpdate();

            // Update AddAll button state
            buttonAddAll.Enabled = listBoxAvailable.Items.Count > 2;
        }

        private void AddItemsFromToolbar(ToolStrip? toolbar, HashSet<string> usedItemNames, HashSet<string> addedItemNames)
        {
            if (toolbar == null)
            {
                return;
            }

            foreach (ToolStripItem item in toolbar.Items)
            {
                if (item is not ToolStripSeparator &&
                    !string.IsNullOrWhiteSpace(item.Name) &&
                    !usedItemNames.Contains(item.Name) &&
                    !addedItemNames.Contains(item.Name))
                {
                    listBoxAvailable.Items.Add(new ToolStripItemWrapper(item));
                    addedItemNames.Add(item.Name);
                }
            }
        }

        private void AddToolbarOnlyItems(HashSet<string> usedItemNames, HashSet<string> addedItemNames)
        {
            // Add items that exist in toolbars but not in menus
            ToolStrip[] allToolbars = { _formBrowse.ToolStripMain, _formBrowse.ToolStripFilters, _formBrowse.ToolStripScripts };

            foreach (ToolStrip toolbar in allToolbars)
            {
                AddItemsFromToolbar(toolbar, usedItemNames, addedItemNames);
            }
        }

        private void AddMenuItemsRecursively(ToolStripMenuItem menuItem, HashSet<string> usedItemNames, HashSet<string> addedItemNames, string? parentName = null)
        {
            string? selectedCategory = comboBoxCategory.SelectedItem?.ToString();

            foreach (ToolStripItem subItem in menuItem.DropDownItems)
            {
                if (subItem is ToolStripMenuItem subMenuItem && !string.IsNullOrEmpty(subMenuItem.Text))
                {
                    // Skip separators and items without text
                    if (subMenuItem.Text.StartsWith("-"))
                    {
                        continue;
                    }

                    string currentParent = parentName ?? menuItem.Text.Replace("&", "");

                    // Filter items based on category and parent
                    if (ShouldSkipItem(subMenuItem, selectedCategory, currentParent))
                    {
                        continue;
                    }

                    // Skip if already used in a toolbar or already added to available list
                    if (!string.IsNullOrWhiteSpace(subMenuItem.Name) &&
                        !usedItemNames.Contains(subMenuItem.Name) &&
                        !addedItemNames.Contains(subMenuItem.Name))
                    {
                        // Create wrapper with modified display name if needed
                        string displayName = GetModifiedDisplayName(subMenuItem, currentParent, selectedCategory);
                        ToolStripItemWrapper wrapper = string.IsNullOrEmpty(displayName)
                            ? new ToolStripItemWrapper(subMenuItem)
                            : new ToolStripItemWrapper(subMenuItem, displayName);

                        listBoxAvailable.Items.Add(wrapper);
                        addedItemNames.Add(subMenuItem.Name);
                    }

                    // Recursively add sub-items if any
                    if (subMenuItem.DropDownItems.Count > 0)
                    {
                        AddMenuItemsRecursively(subMenuItem, usedItemNames, addedItemNames, currentParent);
                    }
                }
            }
        }

        private bool ShouldSkipItem(ToolStripMenuItem item, string? category, string parentName)
        {
            string itemText = item.Text.Replace("&", "");

            // Skip "..." entries in Start category
            if (category == "Start" && itemText == "...")
            {
                return true;
            }

            // Skip specific items in View category
            if (category == "View")
            {
                string[] excludedItems = { "Branches", "Commits", "Grid labels", "Grid info", "Columns", "Sorting", "Settings persistence" };
                if (excludedItems.Contains(itemText))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetModifiedDisplayName(ToolStripMenuItem item, string parentName, string? category)
        {
            string itemText = item.Text.Replace("&", "");

            // Rename items in Git Maintenance submenu (regardless of category)
            if (parentName == "Git Maintenance")
            {
                return $"Git Maintenance > {itemText}";
            }

            // Rename items in PuTTY submenu (regardless of category)
            if (parentName == "PuTTY")
            {
                return $"PuTTY > {itemText}";
            }

            return string.Empty; // Return empty to use default display name
        }

        private void TextBox_TripleClick(object? sender, MouseEventArgs e)
        {
            if (sender is TextBox textBox && e.Clicks >= 3)
            {
                textBox.SelectAll();
            }
        }

        private void ButtonClearAvailableFilter_Click(object? sender, EventArgs e)
        {
            textBoxFilterAvailable.Clear();
        }

        private void ButtonClearCurrentFilter_Click(object? sender, EventArgs e)
        {
            textBoxFilterCurrent.Clear();
        }

        private void ListBox_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (sender is not ListBox listBox || e.Index < 0 || e.Index >= listBox.Items.Count)
            {
                return;
            }

            e.DrawBackground();

            if (listBox.Items[e.Index] is not ToolStripItemWrapper wrapper)
            {
                return;
            }

            // Get icon
            Image? icon = null;

            // Don't show icons for separators and spacers
            if (wrapper.DisplayName != "--- separator ---" && wrapper.DisplayName != "--- expanding spacer ---")
            {
                icon = wrapper.Item?.Image;
                if (icon == null)
                {
                    // Use default icon for items without icons
                    icon = global::GitUI.Properties.Images.ApplicationBlue;
                }
            }

            // Draw icon
            const int iconSize = 20;
            const int iconPadding = 2;
            Rectangle iconRect = new(
                e.Bounds.Left + iconPadding,
                e.Bounds.Top + ((e.Bounds.Height - iconSize) / 2),
                iconSize,
                iconSize);

            if (icon != null)
            {
                e.Graphics.DrawImage(icon, iconRect);
            }

            // Draw text
            int textLeft = iconRect.Right + iconPadding;
            Rectangle textRect = new(
                textLeft,
                e.Bounds.Top,
                e.Bounds.Width - textLeft,
                e.Bounds.Height);

            Brush textBrush = (e.State & DrawItemState.Selected) != 0
                ? SystemBrushes.HighlightText
                : SystemBrushes.WindowText;

            e.Graphics.DrawString(
                wrapper.DisplayName,
                e.Font ?? listBox.Font,
                textBrush,
                textRect,
                StringFormat.GenericDefault);

            e.DrawFocusRectangle();
        }
    }
}
