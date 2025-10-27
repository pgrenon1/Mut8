#!/usr/bin/env python3
"""
Tilesheet Margin Remover
A tool to remove margins between tiles in a tilesheet and create a clean tilesheet without spacing.
Features:
- Load tilesheet images
- Configure tile dimensions and margins
- Remove margins and create a new tilesheet
- Preview the result
- Save the processed tilesheet
"""

import tkinter as tk
from tkinter import ttk, filedialog, messagebox
from PIL import Image, ImageTk
import os


class TilesheetMarginRemover:
    def __init__(self, root):
        self.root = root
        self.root.title("Tilesheet Margin Remover")
        self.root.geometry("1000x700")
        
        # Configuration variables
        self.tile_width = tk.IntVar(value=16)
        self.tile_height = tk.IntVar(value=16)
        self.margin_x = tk.IntVar(value=1)
        self.margin_y = tk.IntVar(value=1)
        
        # Image data
        self.original_image = None
        self.processed_image = None
        self.tiles_per_row = 0
        self.tiles_per_col = 0
        
        self.setup_ui()
        
    def setup_ui(self):
        """Set up the user interface"""
        # Create main frame
        main_frame = ttk.Frame(self.root)
        main_frame.pack(fill=tk.BOTH, expand=True, padx=5, pady=5)
        
        # Create control panel
        self.create_control_panel(main_frame)
        
        # Create preview area
        self.create_preview_area(main_frame)
        
        # Create status bar
        self.create_status_bar()
        
    def create_control_panel(self, parent):
        """Create the control panel with configuration options"""
        control_frame = ttk.LabelFrame(parent, text="Configuration", padding=10)
        control_frame.pack(fill=tk.X, pady=(0, 5))
        
        # File operations
        file_frame = ttk.Frame(control_frame)
        file_frame.pack(fill=tk.X, pady=(0, 10))
        
        ttk.Button(file_frame, text="Load Tilesheet", command=self.load_tilesheet).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(file_frame, text="Process & Preview", command=self.process_tilesheet).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(file_frame, text="Save Processed", command=self.save_processed).pack(side=tk.LEFT, padx=(0, 5))
        ttk.Button(file_frame, text="Clear", command=self.clear_all).pack(side=tk.LEFT)
        
        # Configuration inputs
        config_frame = ttk.Frame(control_frame)
        config_frame.pack(fill=tk.X)
        
        # Tile dimensions
        ttk.Label(config_frame, text="Tile Width:").grid(row=0, column=0, sticky=tk.W, padx=(0, 5))
        width_spin = ttk.Spinbox(config_frame, from_=1, to=512, width=10, textvariable=self.tile_width)
        width_spin.grid(row=0, column=1, padx=(0, 20))
        
        ttk.Label(config_frame, text="Tile Height:").grid(row=0, column=2, sticky=tk.W, padx=(0, 5))
        height_spin = ttk.Spinbox(config_frame, from_=1, to=512, width=10, textvariable=self.tile_height)
        height_spin.grid(row=0, column=3, padx=(0, 20))
        
        # Margins to remove
        ttk.Label(config_frame, text="Margin X:").grid(row=1, column=0, sticky=tk.W, padx=(0, 5), pady=(5, 0))
        margin_x_spin = ttk.Spinbox(config_frame, from_=0, to=64, width=10, textvariable=self.margin_x)
        margin_x_spin.grid(row=1, column=1, padx=(0, 20), pady=(5, 0))
        
        ttk.Label(config_frame, text="Margin Y:").grid(row=1, column=2, sticky=tk.W, padx=(0, 5), pady=(5, 0))
        margin_y_spin = ttk.Spinbox(config_frame, from_=0, to=64, width=10, textvariable=self.margin_y)
        margin_y_spin.grid(row=1, column=3, padx=(0, 20), pady=(5, 0))
        
    def create_preview_area(self, parent):
        """Create the preview area for before/after comparison"""
        preview_frame = ttk.LabelFrame(parent, text="Preview", padding=10)
        preview_frame.pack(fill=tk.BOTH, expand=True)
        
        # Create notebook for tabs
        self.notebook = ttk.Notebook(preview_frame)
        self.notebook.pack(fill=tk.BOTH, expand=True)
        
        # Original tab
        self.original_frame = ttk.Frame(self.notebook)
        self.notebook.add(self.original_frame, text="Original")
        
        self.original_canvas = tk.Canvas(self.original_frame, bg="white")
        self.original_canvas.pack(fill=tk.BOTH, expand=True)
        
        # Processed tab
        self.processed_frame = ttk.Frame(self.notebook)
        self.notebook.add(self.processed_frame, text="Processed")
        
        self.processed_canvas = tk.Canvas(self.processed_frame, bg="white")
        self.processed_canvas.pack(fill=tk.BOTH, expand=True)
        
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
                self.display_original_image()
                self.status_var.set(f"Loaded: {os.path.basename(file_path)} ({self.original_image.width}x{self.original_image.height})")
            except Exception as e:
                messagebox.showerror("Error", f"Failed to load image: {str(e)}")
                
    def display_original_image(self):
        """Display the original image in the original tab"""
        if not self.original_image:
            return
            
        self.original_canvas.delete("all")
        
        # Calculate display size to fit in canvas
        canvas_width = self.original_canvas.winfo_width()
        canvas_height = self.original_canvas.winfo_height()
        
        if canvas_width <= 1 or canvas_height <= 1:
            # Canvas not ready, schedule for later
            self.root.after(100, self.display_original_image)
            return
            
        img_width, img_height = self.original_image.size
        
        # Calculate scale to fit image in canvas
        scale_x = canvas_width / img_width
        scale_y = canvas_height / img_height
        scale = min(scale_x, scale_y, 1.0)  # Don't scale up
        
        display_width = int(img_width * scale)
        display_height = int(img_height * scale)
        
        # Resize image for display
        display_image = self.original_image.resize((display_width, display_height), Image.Resampling.LANCZOS)
        photo = ImageTk.PhotoImage(display_image)
        
        # Center the image
        x = (canvas_width - display_width) // 2
        y = (canvas_height - display_height) // 2
        
        self.original_canvas.create_image(x, y, anchor=tk.NW, image=photo)
        
        # Store reference
        self.original_canvas.image_ref = photo
        
    def process_tilesheet(self):
        """Process the tilesheet to remove margins"""
        if not self.original_image:
            messagebox.showwarning("Warning", "Please load a tilesheet first")
            return
            
        try:
            tile_w = self.tile_width.get()
            tile_h = self.tile_height.get()
            margin_x = self.margin_x.get()
            margin_y = self.margin_y.get()
            
            img_width, img_height = self.original_image.size
            
            # Calculate how many tiles fit
            self.tiles_per_row = (img_width + margin_x) // (tile_w + margin_x)
            self.tiles_per_col = (img_height + margin_y) // (tile_h + margin_y)
            
            # Calculate new image dimensions (no margins)
            new_width = self.tiles_per_row * tile_w
            new_height = self.tiles_per_col * tile_h
            
            # Create new image
            self.processed_image = Image.new('RGBA', (new_width, new_height), (0, 0, 0, 0))
            
            # Extract tiles and place them without margins
            for row in range(self.tiles_per_col):
                for col in range(self.tiles_per_row):
                    # Source position (with margins)
                    src_x = col * (tile_w + margin_x)
                    src_y = row * (tile_h + margin_y)
                    
                    # Check if tile is within original image bounds
                    if src_x + tile_w <= img_width and src_y + tile_h <= img_height:
                        # Extract tile from original
                        tile = self.original_image.crop((src_x, src_y, src_x + tile_w, src_y + tile_h))
                        
                        # Destination position (no margins)
                        dst_x = col * tile_w
                        dst_y = row * tile_h
                        
                        # Paste tile into new image
                        self.processed_image.paste(tile, (dst_x, dst_y))
            
            self.display_processed_image()
            
            original_size = img_width * img_height
            processed_size = new_width * new_height
            reduction = ((original_size - processed_size) / original_size) * 100
            
            self.status_var.set(f"Processed: {new_width}x{new_height} ({reduction:.1f}% size reduction)")
            
        except Exception as e:
            messagebox.showerror("Error", f"Failed to process tilesheet: {str(e)}")
            
    def display_processed_image(self):
        """Display the processed image in the processed tab"""
        if not self.processed_image:
            return
            
        self.processed_canvas.delete("all")
        
        # Calculate display size to fit in canvas
        canvas_width = self.processed_canvas.winfo_width()
        canvas_height = self.processed_canvas.winfo_height()
        
        if canvas_width <= 1 or canvas_height <= 1:
            # Canvas not ready, schedule for later
            self.root.after(100, self.display_processed_image)
            return
            
        img_width, img_height = self.processed_image.size
        
        # Calculate scale to fit image in canvas
        scale_x = canvas_width / img_width
        scale_y = canvas_height / img_height
        scale = min(scale_x, scale_y, 1.0)  # Don't scale up
        
        display_width = int(img_width * scale)
        display_height = int(img_height * scale)
        
        # Resize image for display
        display_image = self.processed_image.resize((display_width, display_height), Image.Resampling.NEAREST)
        photo = ImageTk.PhotoImage(display_image)
        
        # Center the image
        x = (canvas_width - display_width) // 2
        y = (canvas_height - display_height) // 2
        
        self.processed_canvas.create_image(x, y, anchor=tk.NW, image=photo)
        
        # Store reference
        self.processed_canvas.image_ref = photo
        
    def save_processed(self):
        """Save the processed tilesheet"""
        if not self.processed_image:
            messagebox.showwarning("Warning", "No processed image to save")
            return
            
        file_path = filedialog.asksaveasfilename(
            title="Save Processed Tilesheet",
            defaultextension=".png",
            filetypes=[
                ("PNG files", "*.png"),
                ("JPEG files", "*.jpg"),
                ("All files", "*.*")
            ]
        )
        
        if file_path:
            try:
                self.processed_image.save(file_path)
                messagebox.showinfo("Success", f"Processed tilesheet saved to {file_path}")
            except Exception as e:
                messagebox.showerror("Error", f"Failed to save image: {str(e)}")
                
    def clear_all(self):
        """Clear all data and reset"""
        self.original_image = None
        self.processed_image = None
        self.original_canvas.delete("all")
        self.processed_canvas.delete("all")
        self.status_var.set("Ready")


def main():
    """Main function"""
    root = tk.Tk()
    app = TilesheetMarginRemover(root)
    root.mainloop()


if __name__ == "__main__":
    main()
