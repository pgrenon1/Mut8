#!/usr/bin/env python3
"""
Tilesheet Splitter and Viewer
A tool to split a tilesheet into individual tiles and display them in a pannable grid.
Features:
- Load tilesheet images
- Configure tile dimensions and margins
- View individual tiles in a pannable grid
- Save individual tiles
- Interactive configuration menu
"""

import tkinter as tk
from tkinter import ttk, filedialog, messagebox
from PIL import Image, ImageTk
import os
import math


class TilesheetSplitter:
    def __init__(self, root):
        self.root = root
        self.root.title("Tilesheet Splitter & Viewer")
        self.root.geometry("1200x1000")
        
        # Configuration variables
        self.tile_width = tk.IntVar(value=16)
        self.tile_height = tk.IntVar(value=16)
        self.margin_x = tk.IntVar(value=0)
        self.margin_y = tk.IntVar(value=0)
        self.background_color = tk.StringVar(value="black")
        
        # Image data
        self.original_image = None
        self.tiles = []
        self.tiles_per_row = 0
        self.tiles_per_col = 0
        
        # Selected tiles data
        self.selected_tiles = []  # List of selected tile indices
        self.selected_tile_objects = []  # List of selected tile data objects
        
        # Display variables
        self.tile_display_size = 64  # Size to display tiles in the grid
        self.zoom_factor = 1.0
        
        self.setup_ui()
        
        # Bind cleanup on window close
        self.root.protocol("WM_DELETE_WINDOW", self.on_closing)
        
    def on_closing(self):
        """Clean up resources when closing the application"""
        try:
            # Clear all image references
            if hasattr(self.canvas, 'image_refs'):
                self.canvas.image_refs.clear()
            if hasattr(self.selected_canvas, 'image_refs'):
                self.selected_canvas.image_refs.clear()
            if hasattr(self.ruleset_canvas, 'image_ref'):
                self.ruleset_canvas.image_ref = None
            
            # Clear all data
            self.tiles.clear()
            self.selected_tiles.clear()
            self.selected_tile_objects.clear()
            self.original_image = None
            
        except Exception as e:
            print(f"Error during cleanup: {e}")
        finally:
            self.root.destroy()
        
    def setup_ui(self):
        """Set up the user interface"""
        # Create main frame
        main_frame = ttk.Frame(self.root)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)
        
        # Create control panel
        self.create_control_panel(main_frame)
        
        # Create canvas for tile display
        self.create_canvas_frame(main_frame)
        
        # Create status bar
        self.create_status_bar()
        
    def load_ruleset_image(self):
        """Load and display the ruleset image"""
        try:
            ruleset_path = os.path.join(os.path.dirname(__file__), "ruleset2.png")
            if os.path.exists(ruleset_path):
                ruleset_image = Image.open(ruleset_path)
                
                # Calculate display size to fit in canvas
                canvas_width = 200  # Fixed width
                canvas_height = 150  # Fixed height
                
                img_width, img_height = ruleset_image.size
                
                # Calculate scale to fit image in canvas
                scale_x = canvas_width / img_width
                scale_y = canvas_height / img_height
                scale = min(scale_x, scale_y)
                
                display_width = int(img_width * scale)
                display_height = int(img_height * scale)
                
                # Resize image for display
                display_image = ruleset_image.resize((display_width, display_height), Image.Resampling.LANCZOS)
                photo = ImageTk.PhotoImage(display_image)
                
                # Center the image
                x = (canvas_width - display_width) // 2
                y = (canvas_height - display_height) // 2
                
                self.ruleset_canvas.create_image(x, y, anchor=tk.NW, image=photo)
                
                # Store reference
                self.ruleset_canvas.image_ref = photo
                
                # Add label below the image
                ttk.Label(self.ruleset_canvas.master, text="Bitmask tile ruleset", 
                         font=("Arial", 8), foreground="gray").pack()
            else:
                # Create placeholder if ruleset.png doesn't exist
                self.ruleset_canvas.create_text(100, 75, text="ruleset.png\nnot found", 
                                               fill="gray", font=("Arial", 10), justify=tk.CENTER)
        except Exception as e:
            print(f"Error loading ruleset image: {e}")
            self.ruleset_canvas.create_text(100, 75, text="Error loading\nruleset image", 
                                           fill="red", font=("Arial", 10), justify=tk.CENTER)
        
    def create_control_panel(self, parent):
        """Create the control panel with configuration options"""
        control_frame = ttk.Frame(parent)
        control_frame.pack(fill=tk.X, pady=(0, 5))
        
        # Left side - Configuration
        config_frame = ttk.LabelFrame(control_frame, text="Configuration", padding=10)
        config_frame.pack(side=tk.LEFT, fill=tk.BOTH, expand=True, padx=(0, 5))
        
        # Middle - Selected Tiles Display
        selected_frame = ttk.LabelFrame(control_frame, text="Selected Tiles", padding=10)
        selected_frame.pack(side=tk.LEFT, fill=tk.Y, padx=(0, 5))
        
        # Create canvas for selected tiles display
        self.selected_canvas = tk.Canvas(selected_frame, width=200, height=150, bg="lightgray")
        self.selected_canvas.pack()
        
        # Selected tiles info label
        self.selected_info_label = ttk.Label(selected_frame, text="No tiles selected", 
                                           font=("Arial", 8), foreground="gray")
        self.selected_info_label.pack()
        
        # Clear selection button
        ttk.Button(selected_frame, text="Clear Selection", 
                  command=self.clear_selection).pack(pady=(5, 0))
        
        # Tile indexes list display
        ttk.Label(selected_frame, text="Tile Indexes:", 
                 font=("Arial", 8, "bold")).pack(anchor=tk.W, pady=(10, 2))
        
        # Create text widget for tile indexes list
        self.tile_indexes_text = tk.Text(selected_frame, height=3, width=25, 
                                       font=("Arial", 8), wrap=tk.WORD)
        self.tile_indexes_text.pack(fill=tk.X, pady=(0, 5))
        
        # Copy button
        ttk.Button(selected_frame, text="Copy Indexes", 
                  command=self.copy_tile_indexes).pack()
        
        # Right side - Ruleset Display
        ruleset_frame = ttk.LabelFrame(control_frame, text="Tile Ruleset Reference", padding=10)
        ruleset_frame.pack(side=tk.RIGHT, fill=tk.Y)
        
        # Create canvas for ruleset image
        self.ruleset_canvas = tk.Canvas(ruleset_frame, width=200, height=150, bg="white")
        self.ruleset_canvas.pack()
        
        # Load and display ruleset image
        self.load_ruleset_image()
        
        # Right side - Information
        info_frame = ttk.LabelFrame(control_frame, text="Tilesheet Info", padding=10)
        info_frame.pack(side=tk.RIGHT, fill=tk.Y)
        
        # Information labels
        self.info_image_size = tk.StringVar(value="Image Size: --")
        self.info_row_count = tk.StringVar(value="Row Count: --")
        self.info_col_count = tk.StringVar(value="Column Count: --")
        self.info_tile_count = tk.StringVar(value="Tile Count: --")
        
        ttk.Label(info_frame, textvariable=self.info_image_size, font=("Arial", 9)).pack(anchor=tk.W, pady=2)
        ttk.Label(info_frame, textvariable=self.info_row_count, font=("Arial", 9)).pack(anchor=tk.W, pady=2)
        ttk.Label(info_frame, textvariable=self.info_col_count, font=("Arial", 9)).pack(anchor=tk.W, pady=2)
        ttk.Label(info_frame, textvariable=self.info_tile_count, font=("Arial", 9)).pack(anchor=tk.W, pady=2)
        
        # File operations
        file_frame = ttk.Frame(config_frame)
        file_frame.pack(fill=tk.X, pady=(0, 10))
        
        ttk.Button(file_frame, text="Load Tilesheet", command=self.load_tilesheet).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(file_frame, text="Clear", command=self.clear_tiles).pack(side=tk.LEFT)
        
        # Configuration inputs
        inputs_frame = ttk.Frame(config_frame)
        inputs_frame.pack(fill=tk.X)
        
        # Tile dimensions
        ttk.Label(inputs_frame, text="Tile Width:").grid(row=0, column=0, sticky=tk.W, padx=(0, 5))
        width_spin = ttk.Spinbox(inputs_frame, from_=1, to=512, width=10, textvariable=self.tile_width, command=self.on_config_change)
        width_spin.grid(row=0, column=1, padx=(0, 20))
        width_spin.bind('<Return>', lambda e: self.on_config_change())
        
        ttk.Label(inputs_frame, text="Tile Height:").grid(row=0, column=2, sticky=tk.W, padx=(0, 5))
        height_spin = ttk.Spinbox(inputs_frame, from_=1, to=512, width=10, textvariable=self.tile_height, command=self.on_config_change)
        height_spin.grid(row=0, column=3, padx=(0, 20))
        height_spin.bind('<Return>', lambda e: self.on_config_change())
        
        # Margins
        ttk.Label(inputs_frame, text="Margin X:").grid(row=1, column=0, sticky=tk.W, padx=(0, 5), pady=(5, 0))
        margin_x_spin = ttk.Spinbox(inputs_frame, from_=0, to=64, width=10, textvariable=self.margin_x, command=self.on_config_change)
        margin_x_spin.grid(row=1, column=1, padx=(0, 20), pady=(5, 0))
        margin_x_spin.bind('<Return>', lambda e: self.on_config_change())
        
        ttk.Label(inputs_frame, text="Margin Y:").grid(row=1, column=2, sticky=tk.W, padx=(0, 5), pady=(5, 0))
        margin_y_spin = ttk.Spinbox(inputs_frame, from_=0, to=64, width=10, textvariable=self.margin_y, command=self.on_config_change)
        margin_y_spin.grid(row=1, column=3, padx=(0, 20), pady=(5, 0))
        margin_y_spin.bind('<Return>', lambda e: self.on_config_change())
        
        # Background color
        ttk.Label(inputs_frame, text="Background:").grid(row=2, column=0, sticky=tk.W, padx=(0, 5), pady=(5, 0))
        bg_combo = ttk.Combobox(inputs_frame, textvariable=self.background_color, width=8, state="readonly")
        bg_combo['values'] = ('white', 'black', 'gray', 'lightgray', 'darkgray', 'red', 'green', 'blue', 'yellow', 'cyan', 'magenta')
        bg_combo.grid(row=2, column=1, padx=(0, 20), pady=(5, 0))
        bg_combo.bind('<<ComboboxSelected>>', lambda e: self.on_config_change())
        
        # Action buttons
        action_frame = ttk.Frame(inputs_frame)
        action_frame.grid(row=3, column=0, columnspan=4, pady=(10, 0))
        
        ttk.Button(action_frame, text="Reset View", command=self.reset_view).pack(side=tk.LEFT, padx=(0, 5))
        
        # Zoom controls
        zoom_frame = ttk.Frame(action_frame)
        zoom_frame.pack(side=tk.RIGHT)
        
        ttk.Label(zoom_frame, text="Zoom:").pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(zoom_frame, text="-", command=self.zoom_out).pack(side=tk.LEFT, padx=(0, 2))
        ttk.Button(zoom_frame, text="+", command=self.zoom_in).pack(side=tk.LEFT, padx=(0, 2))
        ttk.Button(zoom_frame, text="1:1", command=self.zoom_reset).pack(side=tk.LEFT)
        
    def create_canvas_frame(self, parent):
        """Create the canvas frame for displaying tiles"""
        canvas_frame = ttk.Frame(parent)
        canvas_frame.pack(fill=tk.BOTH, expand=True)
        
        # Create canvas with scrollbars
        self.canvas = tk.Canvas(canvas_frame, bg=self.background_color.get())
        
        v_scrollbar = ttk.Scrollbar(canvas_frame, orient=tk.VERTICAL, command=self.canvas.yview)
        h_scrollbar = ttk.Scrollbar(canvas_frame, orient=tk.HORIZONTAL, command=self.canvas.xview)
        
        self.canvas.configure(yscrollcommand=v_scrollbar.set, xscrollcommand=h_scrollbar.set)
        
        # Grid layout
        self.canvas.grid(row=0, column=0, sticky="nsew")
        v_scrollbar.grid(row=0, column=1, sticky="ns")
        h_scrollbar.grid(row=1, column=0, sticky="ew")
        
        canvas_frame.grid_rowconfigure(0, weight=1)
        canvas_frame.grid_columnconfigure(0, weight=1)
        
        # Bind mouse events for scrolling
        self.canvas.bind("<MouseWheel>", self.on_mousewheel)
        self.canvas.bind("<Shift-MouseWheel>", self.on_mousewheel_horizontal)
        self.canvas.bind("<Control-MouseWheel>", self.on_mousewheel_horizontal)
        
        # Bind for touchpad horizontal scrolling (Windows/Linux)
        self.canvas.bind("<Button-4>", self.on_touchpad_scroll)
        self.canvas.bind("<Button-5>", self.on_touchpad_scroll)
        
        # Focus the canvas to receive scroll events
        self.canvas.focus_set()
        
        # Bind keyboard events for navigation
        self.canvas.bind("<KeyPress>", self.on_key_press)
        
    def create_status_bar(self):
        """Create status bar"""
        self.status_var = tk.StringVar(value="Ready")
        status_bar = ttk.Label(self.root, textvariable=self.status_var, relief=tk.SUNKEN, anchor=tk.W)
        status_bar.pack(side=tk.BOTTOM, fill=tk.X)
        
    def load_tilesheet(self):
        """Load a tilesheet image"""
        file_path = filedialog.askopenfilename(
            title="Select Tilesheet Image",
            filetypes=[
                ("Image files", "*.png *.jpg *.jpeg *.bmp *.gif *.tiff"),
                ("All files", "*.*")
            ]
        )
        
        if file_path:
            try:
                self.original_image = Image.open(file_path)
                self.status_var.set(f"Loaded: {os.path.basename(file_path)} ({self.original_image.width}x{self.original_image.height})")
                self.split_tilesheet()
            except Exception as e:
                messagebox.showerror("Error", f"Failed to load image: {str(e)}")
                
    def on_config_change(self):
        """Handle configuration changes"""
        # Update background color
        self.canvas.configure(bg=self.background_color.get())
        
        if self.original_image:
            self.split_tilesheet()
            
    def split_tilesheet(self):
        """Split the loaded tilesheet into individual tiles"""
        if not self.original_image:
            return
            
        try:
            self.tiles.clear()
            
            tile_w = self.tile_width.get()
            tile_h = self.tile_height.get()
            margin_x = self.margin_x.get()
            margin_y = self.margin_y.get()
            
            img_width, img_height = self.original_image.size
            
            # Calculate how many tiles fit
            self.tiles_per_row = (img_width + margin_x) // (tile_w + margin_x)
            self.tiles_per_col = (img_height + margin_y) // (tile_h + margin_y)
            
            # Extract tiles
            for row in range(self.tiles_per_col):
                for col in range(self.tiles_per_row):
                    x = col * (tile_w + margin_x)
                    y = row * (tile_h + margin_y)
                    
                    # Check if tile is within image bounds
                    if x + tile_w <= img_width and y + tile_h <= img_height:
                        tile = self.original_image.crop((x, y, x + tile_w, y + tile_h))
                        self.tiles.append({
                            'image': tile,
                            'row': row,
                            'col': col,
                            'x': x,
                            'y': y
                        })
            
            self.status_var.set(f"Split into {len(self.tiles)} tiles ({self.tiles_per_row}x{self.tiles_per_col})")
            
            # Update information display
            self.update_info_display()
            
            self.display_tiles()
            
        except Exception as e:
            messagebox.showerror("Error", f"Failed to split tilesheet: {str(e)}")
            
    def update_info_display(self):
        """Update the information display panel"""
        if self.original_image:
            img_width, img_height = self.original_image.size
            self.info_image_size.set(f"Image Size: {img_width} x {img_height}")
            self.info_row_count.set(f"Row Count: {self.tiles_per_col}")
            self.info_col_count.set(f"Column Count: {self.tiles_per_row}")
            self.info_tile_count.set(f"Tile Count: {len(self.tiles)}")
        else:
            self.info_image_size.set("Image Size: --")
            self.info_row_count.set("Row Count: --")
            self.info_col_count.set("Column Count: --")
            self.info_tile_count.set("Tile Count: --")
            
    def display_tiles(self):
        """Display tiles in the canvas"""
        if not self.tiles:
            return
            
        # Clear canvas and clean up old image references
        self.canvas.delete("all")
        if hasattr(self.canvas, 'image_refs'):
            self.canvas.image_refs.clear()
        
        # Calculate display size
        display_size = int(self.tile_display_size * self.zoom_factor)
        spacing = 2  # Space between tiles in display
        
        # Calculate total canvas size
        canvas_width = self.tiles_per_row * (display_size + spacing) - spacing
        canvas_height = self.tiles_per_col * (display_size + spacing) - spacing
        
        self.canvas.configure(scrollregion=(0, 0, canvas_width, canvas_height))
        
        # Initialize image references list
        self.canvas.image_refs = []
        
        # Place tiles
        for tile_data in self.tiles:
            row = tile_data['row']
            col = tile_data['col']
            
            try:
                # Resize tile for display
                display_tile = tile_data['image'].resize((display_size, display_size), Image.Resampling.NEAREST)
                photo = ImageTk.PhotoImage(display_tile)
                
                # Calculate position
                x = col * (display_size + spacing)
                y = row * (display_size + spacing)
                
                # Create tile on canvas
                tile_id = self.canvas.create_image(x, y, anchor=tk.NW, image=photo)
                
                # Add selection highlighting if tile is selected
                tile_index = (row * self.tiles_per_row) + col
                if tile_index in self.selected_tiles:
                    # Draw selection border
                    self.canvas.create_rectangle(x-2, y-2, x+display_size+2, y+display_size+2, 
                                               outline="red", width=3, tags="selection")
                
                # Store reference to prevent garbage collection
                self.canvas.image_refs.append(photo)
                
                # Add click and hover handlers for individual tiles
                self.canvas.tag_bind(tile_id, "<Button-1>", lambda e, tile=tile_data: self.on_tile_click(tile))
                self.canvas.tag_bind(tile_id, "<Enter>", lambda e, tile=tile_data: self.on_tile_hover(tile))
                self.canvas.tag_bind(tile_id, "<Leave>", lambda e: self.on_tile_leave())
            except Exception as e:
                print(f"Error displaying tile at row {row}, col {col}: {e}")
                continue
            
    def on_tile_click(self, tile_data):
        """Handle click on individual tile"""
        row, col = tile_data['row'], tile_data['col']
        tile_index = (row * self.tiles_per_row) + col
        
        # Toggle selection
        if tile_index in self.selected_tiles:
            # Remove from selection
            self.selected_tiles.remove(tile_index)
            self.selected_tile_objects = [t for t in self.selected_tile_objects if t['row'] != row or t['col'] != col]
            self.status_var.set(f"Deselected tile: Row {row}, Col {col}, Index {tile_index}")
        else:
            # Check selection limit (exactly 16 tiles)
            max_selections = 16
            if len(self.selected_tiles) >= max_selections:
                self.status_var.set(f"Maximum {max_selections} tiles can be selected. Clear selection first.")
                return
            
            # Add to selection
            self.selected_tiles.append(tile_index)
            self.selected_tile_objects.append(tile_data)
            self.status_var.set(f"Selected tile: Row {row}, Col {col}, Index {tile_index}")
        
        # Update display
        self.update_selected_tiles_display()
        self.display_tiles()  # Redraw to show selection highlighting
        
    def on_tile_hover(self, tile_data):
        """Handle hover over tile"""
        row, col = tile_data['row'], tile_data['col']
        tile_index = (row * self.tiles_per_row) + col
        self.status_var.set(f"Hovering: Row {row}, Col {col}, Index {tile_index}")
        
    def on_tile_leave(self):
        """Handle leaving tile hover"""
        if self.tiles:
            self.status_var.set(f"Split into {len(self.tiles)} tiles ({self.tiles_per_row}x{self.tiles_per_col})")
        else:
            self.status_var.set("Ready")
    
    def update_selected_tiles_display(self):
        """Update the selected tiles display panel"""
        # Clear the canvas and clean up old image references
        self.selected_canvas.delete("all")
        if hasattr(self.selected_canvas, 'image_refs'):
            self.selected_canvas.image_refs.clear()
        
        if not self.selected_tile_objects:
            self.selected_info_label.config(text="No tiles selected")
            self.tile_indexes_text.delete(1.0, tk.END)
            return
        
        # Update info label
        count = len(self.selected_tile_objects)
        self.selected_info_label.config(text=f"{count} tile{'s' if count != 1 else ''} selected")
        
        # Update tile indexes list (in selection order, not sorted)
        indexes_text = ", ".join(map(str, self.selected_tiles))
        self.tile_indexes_text.delete(1.0, tk.END)
        self.tile_indexes_text.insert(1.0, indexes_text)
        
        # Calculate display parameters
        canvas_width = 200
        canvas_height = 150
        tile_size = 32  # Size for selected tiles display
        tiles_per_row = 4  # Maximum 4 tiles per row
        
        # Initialize image references list
        self.selected_canvas.image_refs = []
        
        # Display selected tiles in a grid
        for i, tile_data in enumerate(self.selected_tile_objects):
            row = i // tiles_per_row
            col = i % tiles_per_row
            
            x = col * (tile_size + 4) + 2
            y = row * (tile_size + 4) + 2
            
            # Check if tile fits in canvas
            if y + tile_size > canvas_height:
                break
            
            try:
                # Resize tile for display
                display_tile = tile_data['image'].resize((tile_size, tile_size), Image.Resampling.NEAREST)
                photo = ImageTk.PhotoImage(display_tile)
                
                # Create tile on canvas
                self.selected_canvas.create_image(x, y, anchor=tk.NW, image=photo)
                
                # Store reference to prevent garbage collection
                self.selected_canvas.image_refs.append(photo)
                
                # Add tile info text - show tile index
                tile_index = (tile_data['row'] * self.tiles_per_row) + tile_data['col']
                self.selected_canvas.create_text(x + tile_size//2, y + tile_size + 2, 
                                               text=f"#{tile_index}", 
                                               font=("Arial", 6), fill="blue")
            except Exception as e:
                print(f"Error displaying selected tile: {e}")
                continue
    
    def clear_selection(self):
        """Clear all selected tiles"""
        self.selected_tiles.clear()
        self.selected_tile_objects.clear()
        self.update_selected_tiles_display()
        self.display_tiles()  # Redraw to remove highlighting
        self.status_var.set("Selection cleared")
    
    def copy_tile_indexes(self):
        """Copy tile indexes to clipboard"""
        if not self.selected_tiles:
            self.status_var.set("No tiles selected to copy")
            return
        
        # Copy in selection order, not sorted
        indexes_text = ", ".join(map(str, self.selected_tiles))
        
        # Copy to clipboard
        self.root.clipboard_clear()
        self.root.clipboard_append(indexes_text)
        self.status_var.set(f"Copied {len(self.selected_tiles)} tile indexes to clipboard")
        
        
    def on_mousewheel(self, event):
        """Handle mouse wheel for vertical scrolling"""
        self.canvas.yview_scroll(int(-1 * (event.delta / 120)), "units")
        
    def on_mousewheel_horizontal(self, event):
        """Handle mouse wheel for horizontal scrolling (Shift+Wheel or Ctrl+Wheel)"""
        self.canvas.xview_scroll(int(-1 * (event.delta / 120)), "units")
        
    def on_touchpad_scroll(self, event):
        """Handle touchpad horizontal scrolling (Button-4/5 events)"""
        if event.num == 4:  # Scroll left
            self.canvas.xview_scroll(-1, "units")
        elif event.num == 5:  # Scroll right
            self.canvas.xview_scroll(1, "units")
            
    def on_key_press(self, event):
        """Handle keyboard navigation"""
        if event.keysym == "Left":
            self.canvas.xview_scroll(-1, "units")
        elif event.keysym == "Right":
            self.canvas.xview_scroll(1, "units")
        elif event.keysym == "Up":
            self.canvas.yview_scroll(-1, "units")
        elif event.keysym == "Down":
            self.canvas.yview_scroll(1, "units")
        elif event.keysym == "Home":
            self.canvas.xview_moveto(0)
        elif event.keysym == "End":
            self.canvas.xview_moveto(1)
        
    def zoom_in(self):
        """Zoom in"""
        self.zoom_factor = min(self.zoom_factor * 1.2, 4.0)
        self.display_tiles()
        
    def zoom_out(self):
        """Zoom out"""
        self.zoom_factor = max(self.zoom_factor / 1.2, 0.1)
        self.display_tiles()
        
    def zoom_reset(self):
        """Reset zoom to 1:1"""
        self.zoom_factor = 1.0
        self.display_tiles()
        
    def reset_view(self):
        """Reset zoom and scroll position"""
        self.zoom_factor = 1.0
        self.canvas.xview_moveto(0)
        self.canvas.yview_moveto(0)
        self.display_tiles()
        
            
    def clear_tiles(self):
        """Clear all tiles and reset"""
        self.tiles.clear()
        self.selected_tiles.clear()
        self.selected_tile_objects.clear()
        self.original_image = None
        self.canvas.delete("all")
        self.status_var.set("Ready")
        self.update_info_display()
        self.update_selected_tiles_display()
        self.reset_view()


def main():
    """Main function"""
    root = tk.Tk()
    app = TilesheetSplitter(root)
    root.mainloop()


if __name__ == "__main__":
    main()
