using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GitCommands.Settings;

namespace GitUI.CommandsDialogs
{
    /// <summary>
    /// Form for configuring toolbars layout with a visual 2D grid.
    /// Allows users to drag and drop toolbars to reposition them across rows.
    /// </summary>
    public partial class FormToolbarsLayout : Form
    {
        private readonly FormBrowse _formBrowse;
        private readonly Dictionary<string, ToolStrip> _dynamicToolbars;
        private readonly List<ToolbarLayoutItem> _layoutItems = new();
        private readonly List<RowPanel> _rowPanels = new();

        private const int RowHeight = 50;
        private const int RowMargin = 8;
        private const int ToolbarItemWidth = 120;
        private const int ToolbarItemHeight = 36;
        private const int ToolbarItemMargin = 6;

        // Drag and drop state
        private ToolbarItemPanel? _draggedItem;
        private Point _dragStartPoint;
        private bool _isDragging;
        private Panel? _dropIndicator;

        // Selection state
        private ToolbarItemPanel? _selectedItem;

        public FormToolbarsLayout(FormBrowse formBrowse, Dictionary<string, ToolStrip> dynamicToolbars)
        {
            _formBrowse = formBrowse;
            _dynamicToolbars = dynamicToolbars;

            InitializeComponent();
            InitializeToolTips();
            LoadCurrentLayout();
            BuildVisualGrid();
        }

        /// <summary>
        /// Represents a toolbar in the layout grid
        /// </summary>
        private class ToolbarLayoutItem
        {
            public string Name { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public int Row { get; set; }
            public int OrderInRow { get; set; }
            public bool IsBuiltIn { get; set; }
            public bool IsVisible { get; set; } = true;
        }

        /// <summary>
        /// Visual panel representing a row of toolbars
        /// </summary>
        private class RowPanel : Panel
        {
            public int RowIndex { get; set; }
            public Label RowLabel { get; }

            public RowPanel(int rowIndex)
            {
                RowIndex = rowIndex;
                Height = RowHeight;
                BackColor = SystemColors.Control;
                BorderStyle = BorderStyle.FixedSingle;
                Margin = new Padding(0, 0, 0, RowMargin);
                Padding = new Padding(50, 5, 5, 5);

                RowLabel = new Label
                {
                    Text = $"Row {rowIndex + 1}",
                    AutoSize = false,
                    Width = 45,
                    Height = RowHeight - 12,
                    Location = new Point(3, 6),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = SystemColors.GrayText,
                    Font = new Font(SystemFonts.DefaultFont.FontFamily, 8f)
                };
                Controls.Add(RowLabel);
            }

            public void UpdateRowIndex(int newIndex)
            {
                RowIndex = newIndex;
                RowLabel.Text = $"Row {newIndex + 1}";
            }
        }

        /// <summary>
        /// Visual panel representing a toolbar item that can be dragged
        /// </summary>
        private class ToolbarItemPanel : Panel
        {
            public ToolbarLayoutItem LayoutItem { get; }
            private readonly Label _label;
            private readonly Label _gripLabel;
            private bool _isSelected;

            // Highlight color used for both drag and selection
            private static readonly Color HighlightColor = Color.FromArgb(255, 255, 180);

            public bool IsSelected
            {
                get => _isSelected;
                set
                {
                    _isSelected = value;
                    UpdateBackColor();
                }
            }

            public ToolbarItemPanel(ToolbarLayoutItem item)
            {
                LayoutItem = item;
                Width = ToolbarItemWidth;
                Height = ToolbarItemHeight;
                BackColor = Color.FromArgb(220, 235, 252); // Same color for all toolbars (built-in and custom)
                BorderStyle = BorderStyle.FixedSingle;
                Cursor = Cursors.SizeAll;
                Margin = new Padding(ToolbarItemMargin);

                // Grip indicator
                _gripLabel = new Label
                {
                    Text = "⋮⋮",
                    AutoSize = false,
                    Width = 16,
                    Height = ToolbarItemHeight - 4,
                    Location = new Point(2, 2),
                    TextAlign = ContentAlignment.MiddleCenter,
                    ForeColor = SystemColors.GrayText,
                    Cursor = Cursors.SizeAll
                };
                Controls.Add(_gripLabel);

                // Toolbar name
                _label = new Label
                {
                    Text = item.DisplayName,
                    AutoSize = false,
                    Width = ToolbarItemWidth - 24,
                    Height = ToolbarItemHeight - 4,
                    Location = new Point(18, 2),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Cursor = Cursors.SizeAll
                };

                if (!item.IsVisible)
                {
                    _label.ForeColor = SystemColors.GrayText;
                    _label.Text = $"({item.DisplayName})";
                    BackColor = Color.FromArgb(240, 240, 240);
                }

                Controls.Add(_label);

                // Propagate mouse events from child controls
                _label.MouseDown += (s, e) => OnMouseDown(e);
                _label.MouseMove += (s, e) => OnMouseMove(e);
                _label.MouseUp += (s, e) => OnMouseUp(e);
                _gripLabel.MouseDown += (s, e) => OnMouseDown(e);
                _gripLabel.MouseMove += (s, e) => OnMouseMove(e);
                _gripLabel.MouseUp += (s, e) => OnMouseUp(e);
            }

            public void SetHighlight(bool highlight)
            {
                if (highlight)
                {
                    BackColor = HighlightColor;
                }
                else
                {
                    UpdateBackColor();
                }
            }

            private void UpdateBackColor()
            {
                if (_isSelected)
                {
                    BackColor = HighlightColor;
                }
                else if (!LayoutItem.IsVisible)
                {
                    BackColor = Color.FromArgb(240, 240, 240);
                }
                else
                {
                    BackColor = LayoutItem.IsBuiltIn
                        ? Color.FromArgb(220, 235, 252)
                        : Color.FromArgb(235, 252, 220);
                }
            }
        }

        private void InitializeToolTips()
        {
            toolTip.SetToolTip(buttonAddRow, "Add a new empty row at the bottom");
            toolTip.SetToolTip(buttonRemoveRow, "Remove the last empty row");
            toolTip.SetToolTip(buttonReset, "Reset layout to default (all toolbars on row 1)");
            toolTip.SetToolTip(buttonLocate, "Highlight the selected toolbar in the main window (or all toolbars if none selected)");
            toolTip.SetToolTip(buttonOK, "Apply changes and close");
            toolTip.SetToolTip(buttonCancel, "Cancel and close without saving");
            toolTip.SetToolTip(buttonApply, "Apply changes without closing");
            toolTip.SetToolTip(panelToolbarGrid, "Drag toolbars to reposition them");
        }

        private void LoadCurrentLayout()
        {
            _layoutItems.Clear();

            // Get current layout from config
            ToolbarLayoutConfig? config = GitCommands.AppSettings.ToolbarLayout;

            System.Diagnostics.Debug.WriteLine($"[FormToolbarsLayout.LoadCurrentLayout] Config is null: {config is null}");
            System.Diagnostics.Debug.WriteLine($"[FormToolbarsLayout.LoadCurrentLayout] ToolbarsVisibility count: {config?.ToolbarsVisibility?.Count ?? 0}");

            // Built-in toolbars with their current positions
            string[] builtInToolbars = { "Standard", "Filters", "Scripts" };

            foreach (string name in builtInToolbars)
            {
                ToolbarMetadata? metadata = config?.ToolbarsVisibility?
                    .FirstOrDefault(t => t.Name == name);

                int row = metadata?.Row ?? 0;
                int orderInRow = metadata?.OrderInRow ?? GetDefaultOrder(name);
                bool visible = metadata?.Visible ?? true;

                System.Diagnostics.Debug.WriteLine($"[FormToolbarsLayout.LoadCurrentLayout] Toolbar '{name}': Row={row}, OrderInRow={orderInRow}, Visible={visible}");

                _layoutItems.Add(new ToolbarLayoutItem
                {
                    Name = name,
                    DisplayName = name,
                    Row = row,
                    OrderInRow = orderInRow,
                    IsBuiltIn = true,
                    IsVisible = visible
                });
            }

            // Custom toolbars
            if (config?.CustomToolbars != null)
            {
                foreach (CustomToolbarMetadata customMeta in config.CustomToolbars)
                {
                    System.Diagnostics.Debug.WriteLine($"[FormToolbarsLayout.LoadCurrentLayout] Custom Toolbar '{customMeta.Name}': Row={customMeta.Row}, OrderInRow={customMeta.OrderInRow}, Visible={customMeta.Visible}");

                    _layoutItems.Add(new ToolbarLayoutItem
                    {
                        Name = customMeta.Name,
                        DisplayName = customMeta.Name,
                        Row = customMeta.Row,
                        OrderInRow = customMeta.OrderInRow,
                        IsBuiltIn = false,
                        IsVisible = customMeta.Visible
                    });
                }
            }

            // Also add custom toolbars from FormBrowse that may not be in config yet
            foreach (var kvp in _dynamicToolbars)
            {
                string toolbarName = kvp.Value.Text;
                if (!string.IsNullOrEmpty(toolbarName) && !_layoutItems.Any(i => i.Name == toolbarName))
                {
                    _layoutItems.Add(new ToolbarLayoutItem
                    {
                        Name = toolbarName,
                        DisplayName = toolbarName,
                        Row = 0,
                        OrderInRow = _layoutItems.Count,
                        IsBuiltIn = false,
                        IsVisible = kvp.Value.Visible
                    });
                }
            }

            // Sort by row, then by order within row
            _layoutItems.Sort((a, b) =>
            {
                int rowCompare = a.Row.CompareTo(b.Row);
                return rowCompare != 0 ? rowCompare : a.OrderInRow.CompareTo(b.OrderInRow);
            });
        }

        private static int GetDefaultOrder(string toolbarName)
        {
            return toolbarName switch
            {
                "Standard" => 0,
                "Filters" => 1,
                "Scripts" => 2,
                _ => 99
            };
        }

        private void BuildVisualGrid()
        {
            panelToolbarGrid.Controls.Clear();
            _rowPanels.Clear();
            _selectedItem = null;

            // Determine max row
            int maxRow = _layoutItems.Any() ? _layoutItems.Max(i => i.Row) : 0;

            // Create row panels
            int yOffset = 5;
            for (int r = 0; r <= maxRow; r++)
            {
                RowPanel rowPanel = new(r)
                {
                    Location = new Point(5, yOffset),
                    Width = panelToolbarGrid.ClientSize.Width - 30,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                // Enable as drop target
                rowPanel.AllowDrop = true;
                rowPanel.DragEnter += RowPanel_DragEnter;
                rowPanel.DragOver += RowPanel_DragOver;
                rowPanel.DragLeave += RowPanel_DragLeave;
                rowPanel.DragDrop += RowPanel_DragDrop;

                _rowPanels.Add(rowPanel);
                panelToolbarGrid.Controls.Add(rowPanel);

                // Add toolbar items to this row
                var itemsInRow = _layoutItems
                    .Where(i => i.Row == r)
                    .OrderBy(i => i.OrderInRow)
                    .ToList();

                int xOffset = 55; // After row label
                foreach (ToolbarLayoutItem item in itemsInRow)
                {
                    ToolbarItemPanel itemPanel = new(item)
                    {
                        Location = new Point(xOffset, 7)
                    };

                    // Enable drag
                    itemPanel.MouseDown += ItemPanel_MouseDown;
                    itemPanel.MouseMove += ItemPanel_MouseMove;
                    itemPanel.MouseUp += ItemPanel_MouseUp;

                    rowPanel.Controls.Add(itemPanel);
                    xOffset += ToolbarItemWidth + ToolbarItemMargin;
                }

                yOffset += RowHeight + RowMargin;
            }

            // Create drop indicator (invisible until dragging)
            _dropIndicator = new Panel
            {
                BackColor = Color.FromArgb(0, 120, 215),
                Height = 3,
                Width = 100,
                Visible = false
            };
            panelToolbarGrid.Controls.Add(_dropIndicator);

            UpdateRemoveRowButton();
        }

        #region Drag and Drop

        private void ItemPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is ToolbarItemPanel itemPanel && e.Button == MouseButtons.Left)
            {
                _draggedItem = itemPanel;
                _dragStartPoint = e.Location;
                _isDragging = false;
            }
        }

        private void ItemPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (_draggedItem != null && e.Button == MouseButtons.Left)
            {
                // Check if moved enough to start drag
                if (!_isDragging &&
                    (Math.Abs(e.X - _dragStartPoint.X) > 5 || Math.Abs(e.Y - _dragStartPoint.Y) > 5))
                {
                    _isDragging = true;
                    _draggedItem.SetHighlight(true);
                    _draggedItem.DoDragDrop(_draggedItem, DragDropEffects.Move);
                }
            }
        }

        private void ItemPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (_draggedItem != null)
            {
                // If not dragging, this is a simple click -> select the item
                if (!_isDragging && sender is ToolbarItemPanel clickedItem)
                {
                    SelectToolbarItem(clickedItem);
                }

                _draggedItem.SetHighlight(false);
                _draggedItem = null;
                _isDragging = false;
                HideDropIndicator();
            }
        }

        private void SelectToolbarItem(ToolbarItemPanel itemToSelect)
        {
            // Deselect previously selected item
            if (_selectedItem != null && _selectedItem != itemToSelect)
            {
                _selectedItem.IsSelected = false;
            }

            // Toggle selection if clicking on the same item, otherwise select the new item
            if (_selectedItem == itemToSelect)
            {
                _selectedItem.IsSelected = !_selectedItem.IsSelected;
                if (!_selectedItem.IsSelected)
                {
                    _selectedItem = null;
                }
            }
            else
            {
                _selectedItem = itemToSelect;
                _selectedItem.IsSelected = true;
            }
        }

        private void RowPanel_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data?.GetDataPresent(typeof(ToolbarItemPanel)) == true)
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void RowPanel_DragOver(object? sender, DragEventArgs e)
        {
            if (sender is not RowPanel rowPanel || _draggedItem == null)
            {
                return;
            }

            e.Effect = DragDropEffects.Move;

            // Convert screen coordinates to row panel coordinates
            Point clientPoint = rowPanel.PointToClient(new Point(e.X, e.Y));

            // Find drop position
            int dropIndex = GetDropIndex(rowPanel, clientPoint.X);

            // Show drop indicator
            ShowDropIndicator(rowPanel, dropIndex);
        }

        private void RowPanel_DragLeave(object? sender, EventArgs e)
        {
            HideDropIndicator();
        }

        private void RowPanel_DragDrop(object? sender, DragEventArgs e)
        {
            if (sender is not RowPanel targetRow ||
                e.Data?.GetData(typeof(ToolbarItemPanel)) is not ToolbarItemPanel droppedPanel)
            {
                HideDropIndicator();
                return;
            }

            // Get drop position
            Point clientPoint = targetRow.PointToClient(new Point(e.X, e.Y));
            int dropIndex = GetDropIndex(targetRow, clientPoint.X);

            // Update layout item
            ToolbarLayoutItem item = droppedPanel.LayoutItem;
            int oldRow = item.Row;

            item.Row = targetRow.RowIndex;

            // Recalculate order for all items in target row
            var itemsInTargetRow = _layoutItems
                .Where(i => i.Row == targetRow.RowIndex && i != item)
                .OrderBy(i => i.OrderInRow)
                .ToList();

            // Insert at drop position
            itemsInTargetRow.Insert(Math.Min(dropIndex, itemsInTargetRow.Count), item);

            // Update order values
            for (int i = 0; i < itemsInTargetRow.Count; i++)
            {
                itemsInTargetRow[i].OrderInRow = i;
            }

            // If we moved from a different row, reorder that row too
            if (oldRow != targetRow.RowIndex)
            {
                var itemsInOldRow = _layoutItems
                    .Where(i => i.Row == oldRow)
                    .OrderBy(i => i.OrderInRow)
                    .ToList();

                for (int i = 0; i < itemsInOldRow.Count; i++)
                {
                    itemsInOldRow[i].OrderInRow = i;
                }
            }

            droppedPanel.SetHighlight(false);
            _draggedItem = null;
            _isDragging = false;
            HideDropIndicator();

            // Remove empty rows and rebuild
            RemoveEmptyRows();
            BuildVisualGrid();
        }

        private int GetDropIndex(RowPanel rowPanel, int clientX)
        {
            var itemPanels = rowPanel.Controls.OfType<ToolbarItemPanel>()
                .OrderBy(p => p.Location.X)
                .ToList();

            if (itemPanels.Count == 0)
            {
                return 0;
            }

            for (int i = 0; i < itemPanels.Count; i++)
            {
                int itemCenterX = itemPanels[i].Location.X + (itemPanels[i].Width / 2);
                if (clientX < itemCenterX)
                {
                    return i;
                }
            }

            return itemPanels.Count;
        }

        private void ShowDropIndicator(RowPanel rowPanel, int dropIndex)
        {
            if (_dropIndicator == null)
            {
                return;
            }

            var itemPanels = rowPanel.Controls.OfType<ToolbarItemPanel>()
                .OrderBy(p => p.Location.X)
                .ToList();

            int xPos;
            if (itemPanels.Count == 0 || dropIndex == 0)
            {
                xPos = 55; // Start position
            }
            else if (dropIndex >= itemPanels.Count)
            {
                xPos = itemPanels.Last().Right + 3;
            }
            else
            {
                xPos = itemPanels[dropIndex].Location.X - 3;
            }

            // Convert to panelToolbarGrid coordinates
            Point gridLocation = panelToolbarGrid.PointToClient(rowPanel.PointToScreen(new Point(xPos, 5)));

            _dropIndicator.Location = gridLocation;
            _dropIndicator.Height = ToolbarItemHeight + 4;
            _dropIndicator.Width = 4;
            _dropIndicator.Visible = true;
            _dropIndicator.BringToFront();
        }

        private void HideDropIndicator()
        {
            if (_dropIndicator != null)
            {
                _dropIndicator.Visible = false;
            }
        }

        #endregion

        #region Button Handlers

        private void ButtonAddRow_Click(object? sender, EventArgs e)
        {
            int newRowIndex = _rowPanels.Count;

            // Add a new empty row
            RowPanel newRow = new(newRowIndex)
            {
                Location = new Point(5, (_rowPanels.Count * (RowHeight + RowMargin)) + 5),
                Width = panelToolbarGrid.ClientSize.Width - 30,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            newRow.AllowDrop = true;
            newRow.DragEnter += RowPanel_DragEnter;
            newRow.DragOver += RowPanel_DragOver;
            newRow.DragLeave += RowPanel_DragLeave;
            newRow.DragDrop += RowPanel_DragDrop;

            _rowPanels.Add(newRow);
            panelToolbarGrid.Controls.Add(newRow);

            UpdateRemoveRowButton();
        }

        private void ButtonRemoveRow_Click(object? sender, EventArgs e)
        {
            // Remove last empty row
            if (_rowPanels.Count > 1)
            {
                RowPanel lastRow = _rowPanels[^1];

                // Check if empty (no toolbar items)
                bool hasItems = lastRow.Controls.OfType<ToolbarItemPanel>().Any();
                if (!hasItems)
                {
                    panelToolbarGrid.Controls.Remove(lastRow);
                    _rowPanels.RemoveAt(_rowPanels.Count - 1);
                    lastRow.Dispose();
                }
            }

            UpdateRemoveRowButton();
        }

        private void ButtonReset_Click(object? sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Reset all toolbars to a single row?\n\nThis will place all toolbars on row 1 in their default order.",
                "Reset Layout",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Reset all items to row 0
                int order = 0;
                foreach (ToolbarLayoutItem item in _layoutItems.OrderBy(i => GetDefaultOrder(i.Name)))
                {
                    item.Row = 0;
                    item.OrderInRow = order++;
                }

                BuildVisualGrid();
            }
        }

        private void ButtonOK_Click(object? sender, EventArgs e)
        {
            ApplyLayout();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ButtonApply_Click(object? sender, EventArgs e)
        {
            ApplyLayout();
        }

        private void ButtonLocate_Click(object? sender, EventArgs e)
        {
            // Get the ToolStripPanel from FormBrowse
            Control? toolPanelContainer = _formBrowse.Controls.Cast<Control>()
                .FirstOrDefault(c => c is ToolStripContainer);

            if (toolPanelContainer is not ToolStripContainer toolPanel)
            {
                MessageBox.Show(
                    "Could not find the toolbar panel.",
                    "Locate Toolbars",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            // If a toolbar is selected, only locate that one
            if (_selectedItem != null)
            {
                ToolStrip? selectedToolStrip = GetToolStripByName(_selectedItem.LayoutItem.Name);
                if (selectedToolStrip != null && selectedToolStrip.Visible)
                {
                    _ = FlashToolbarsAsync(new List<ToolStrip> { selectedToolStrip });
                }
                else
                {
                    MessageBox.Show(
                        $"The selected toolbar '{_selectedItem.LayoutItem.DisplayName}' is not visible in the main window.",
                        "Locate Toolbar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }

                return;
            }

            // No toolbar selected: locate all visible toolbars
            List<ToolStrip> allToolStrips = new();

            // Built-in toolbars
            if (_formBrowse.ToolStripMain.Visible)
            {
                allToolStrips.Add(_formBrowse.ToolStripMain);
            }

            if (_formBrowse.ToolStripFilters.Visible)
            {
                allToolStrips.Add(_formBrowse.ToolStripFilters);
            }

            if (_formBrowse.ToolStripScripts.Visible)
            {
                allToolStrips.Add(_formBrowse.ToolStripScripts);
            }

            // Custom toolbars
            foreach (var kvp in _dynamicToolbars)
            {
                if (kvp.Value.Visible && !allToolStrips.Contains(kvp.Value))
                {
                    allToolStrips.Add(kvp.Value);
                }
            }

            if (allToolStrips.Count == 0)
            {
                MessageBox.Show(
                    "No visible toolbars found.",
                    "Locate Toolbars",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            // Start the flashing animation for all toolbars (fire and forget)
            _ = FlashToolbarsAsync(allToolStrips);
        }

        private ToolStrip? GetToolStripByName(string name)
        {
            return name switch
            {
                "Standard" => _formBrowse.ToolStripMain,
                "Filters" => _formBrowse.ToolStripFilters,
                "Scripts" => _formBrowse.ToolStripScripts,
                _ => _dynamicToolbars.Values.FirstOrDefault(ts => ts.Text == name)
            };
        }

        private async Task FlashToolbarsAsync(List<ToolStrip> toolbars)
        {
            // Save original background colors
            Dictionary<ToolStrip, Color> originalColors = new();
            foreach (ToolStrip toolbar in toolbars)
            {
                originalColors[toolbar] = toolbar.BackColor;
            }

            // Flash parameters
            const int flashDurationMs = 3000; // 3 seconds total
            const int stepDurationMs = 100;    // Update every 100ms
            int steps = flashDurationMs / stepDurationMs;

            try
            {
                // Animate from transparent to red and back
                for (int i = 0; i < steps; i++)
                {
                    // Calculate alpha value using a sine wave for smooth fade in/out
                    double progress = (double)i / steps;
                    double sineWave = Math.Sin(progress * Math.PI * 4); // 4 complete cycles in 3 seconds
                    int alpha = (int)(Math.Abs(sineWave) * 180); // Max alpha = 180 (not fully opaque)

                    // Blend red with original background for each toolbar
                    foreach (ToolStrip toolbar in toolbars)
                    {
                        Color flashColor = Color.FromArgb(alpha, Color.Red);
                        toolbar.BackColor = BlendColors(originalColors[toolbar], flashColor);
                    }

                    // Wait before next frame
                    await Task.Delay(stepDurationMs);
                }
            }
            finally
            {
                // Restore original colors
                foreach (ToolStrip toolbar in toolbars)
                {
                    toolbar.BackColor = originalColors[toolbar];
                }
            }
        }

        private Color BlendColors(Color background, Color overlay)
        {
            // Alpha blending formula
            int alpha = overlay.A;
            int invAlpha = 255 - alpha;

            int r = ((overlay.R * alpha) + (background.R * invAlpha)) / 255;
            int g = ((overlay.G * alpha) + (background.G * invAlpha)) / 255;
            int b = ((overlay.B * alpha) + (background.B * invAlpha)) / 255;

            return Color.FromArgb(r, g, b);
        }

        #endregion

        private void RemoveEmptyRows()
        {
            // Get rows that have items
            var usedRows = _layoutItems.Select(i => i.Row).Distinct().OrderBy(r => r).ToList();

            // Renumber rows to be contiguous
            for (int i = 0; i < usedRows.Count; i++)
            {
                int oldRow = usedRows[i];
                if (oldRow != i)
                {
                    foreach (var item in _layoutItems.Where(it => it.Row == oldRow))
                    {
                        item.Row = i;
                    }
                }
            }
        }

        private void UpdateRemoveRowButton()
        {
            // Enable remove button only if last row is empty
            if (_rowPanels.Count > 1)
            {
                RowPanel lastRow = _rowPanels[^1];
                bool hasItems = lastRow.Controls.OfType<ToolbarItemPanel>().Any();
                buttonRemoveRow.Enabled = !hasItems;
            }
            else
            {
                buttonRemoveRow.Enabled = false;
            }
        }

        private void ApplyLayout()
        {
            // Get current config
            ToolbarLayoutConfig config = GitCommands.AppSettings.ToolbarLayout ?? new ToolbarLayoutConfig();

            // Update visibility metadata with row/order
            config.ToolbarsVisibility ??= new List<ToolbarMetadata>();

            foreach (ToolbarLayoutItem item in _layoutItems)
            {
                // Update or add to ToolbarsVisibility
                ToolbarMetadata? existing = config.ToolbarsVisibility.FirstOrDefault(t => t.Name == item.Name);
                if (existing != null)
                {
                    existing.Row = item.Row;
                    existing.OrderInRow = item.OrderInRow;
                }
                else
                {
                    config.ToolbarsVisibility.Add(new ToolbarMetadata
                    {
                        Name = item.Name,
                        Visible = item.IsVisible,
                        Row = item.Row,
                        OrderInRow = item.OrderInRow
                    });
                }

                // Also update CustomToolbars if it's a custom toolbar
                if (!item.IsBuiltIn)
                {
                    config.CustomToolbars ??= new List<CustomToolbarMetadata>();
                    CustomToolbarMetadata? customMeta = config.CustomToolbars.FirstOrDefault(c => c.Name == item.Name);
                    if (customMeta != null)
                    {
                        customMeta.Row = item.Row;
                        customMeta.OrderInRow = item.OrderInRow;
                    }
                    else
                    {
                        config.CustomToolbars.Add(new CustomToolbarMetadata
                        {
                            Name = item.Name,
                            Index = 3 + config.CustomToolbars.Count,
                            Visible = item.IsVisible,
                            Row = item.Row,
                            OrderInRow = item.OrderInRow
                        });
                    }
                }
            }

            // Save config
            GitCommands.AppSettings.ToolbarLayout = config;

            // Persist to disk immediately
            GitCommands.AppSettings.SettingsContainer.Save();

            // Apply to actual toolbars in FormBrowse
            ApplyToFormBrowse();
        }

        private void ApplyToFormBrowse()
        {
            // Call the public method in FormBrowse to reorganize toolbars
            _formBrowse.ReorganizeToolbars();
        }

        private ToolStrip? GetToolStripByName(string name, List<ToolStrip> allToolStrips)
        {
            return name switch
            {
                "Standard" => _formBrowse.ToolStripMain,
                "Filters" => _formBrowse.ToolStripFilters,
                "Scripts" => _formBrowse.ToolStripScripts,
                _ => allToolStrips.FirstOrDefault(ts => ts.Text == name)
            };
        }

        /// <summary>
        /// Gets the current layout configuration from this form
        /// </summary>
        public List<(string Name, int Row, int OrderInRow)> GetLayoutConfiguration()
        {
            return _layoutItems
                .Select(i => (i.Name, i.Row, i.OrderInRow))
                .ToList();
        }
    }
}
