﻿using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace IconMaker
{
    /// <summary>
    /// Main application window for IconMaker.
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private Fields
        /// <summary>
        /// File browser dialog for importing images.
        /// </summary>
        private OpenFileDialog importDialog;

        /// <summary>
        /// File browser dialog for saving icons.
        /// </summary>
        private SaveFileDialog saveDialog;
        
        /// <summary>
        /// Icon currently being edited.
        /// </summary>
        private IconFile currentIcon = new IconFile();
        
        /// <summary>
        /// Indicates whether the current icon has been modified.
        /// </summary>
        private bool modified;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
        #endregion

        #region Protected Methods
        /// <summary>
        /// Invoked when the window is initialized.
        /// </summary>
        /// <param name="e">Empty EventArgs instance.</param>
        protected override void OnInitialized(EventArgs e)
        {
            this.importDialog = (OpenFileDialog)this.Resources["importDialog"];
            this.saveDialog = (SaveFileDialog)this.Resources["saveDialog"];
            this.imageList.ItemsSource = this.currentIcon.Images;
            base.OnInitialized(e);
        }

        /// <summary>
        /// Invoked when the window receives a drop event.
        /// </summary>
        /// <param name="e">DragEventArgs instance with information about the event.</param>
        protected override void OnDrop(DragEventArgs e)
        {
            var fileNames = e.Data.GetData("FileDrop") as string[];
            if(fileNames != null && fileNames.Length > 0)
            {
                foreach(var fileName in fileNames)
                    this.AddImageFile(fileName);
            }

            base.OnDrop(e);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Attempts to load an image file and add it to the icon's image collection.
        /// </summary>
        /// <param name="fileName">Full path of image file to load.</param>
        private void AddImageFile(string fileName)
        {
            try
            {
                var decoder = BitmapDecoder.Create(new Uri(fileName), BitmapCreateOptions.None, BitmapCacheOption.None);
                if(decoder.Frames.Count > 0)
                {
                    var image = decoder.Frames[0];
                    this.currentIcon.Images.Set(image);
                    this.modified = true;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handler for commands which are always enabled.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">CanExecuteRoutedEventArgs instance with information about the event.</param>
        private void CommandAlwaysCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        /// <summary>
        /// Handler for commands which are enabled if an image is selected.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">CanExecuteRoutedEventArgs instance with information about the event.</param>
        private void CanExecuteIfSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.imageList.SelectedItem != null;
        }

        /// <summary>
        /// Determines whether the Save command can execute.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">CanExecuteRoutedEventArgs instance with information about the event.</param>
        private void SaveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if(this.currentIcon != null)
                e.CanExecute = this.currentIcon.Images.Count > 0;
            else
                e.CanExecute = false;
        }

        /// <summary>
        /// Handler for determining when the Paste command is enabled.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">CanExecuteRoutedEventArgs instance with information about the event.</param>
        private void PasteCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var image = Clipboard.GetImage();
                e.CanExecute = image != null && image.PixelWidth == image.PixelHeight && image.PixelWidth <= 256;
            }
            catch
            {
                // I don't like this, but Clipboard.GetImage can throw random exceptions if the clipboard data is invalid.
                e.CanExecute = false;
            }
        }

        /// <summary>
        /// Executes the New command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void NewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if(this.currentIcon != null && this.currentIcon.Images.Count > 0 && this.modified)
            {
                if(MessageBox.Show(this, "Icon has not been saved. Create a new icon?", "Icon Maker", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    this.currentIcon = new IconFile();
                    this.imageList.ItemsSource = this.currentIcon.Images;
                    this.modified = false;
                }
            }
        }

        /// <summary>
        /// Executes the Open command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if(this.importDialog.ShowDialog(this) == true)
            {
                foreach(var fileName in this.importDialog.FileNames)
                    this.AddImageFile(fileName);
            }
        }

        /// <summary>
        /// Executes the Save command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if(this.currentIcon != null && this.currentIcon.Images.Count > 0)
            {
                if(this.saveDialog.ShowDialog(this) == true)
                {
                    this.currentIcon.Save(this.saveDialog.FileName);
                    this.modified = false;
                }
            }
        }

        /// <summary>
        /// Executes the Close command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Executes the Delete command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void DeleteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var image = this.imageList.SelectedItem as BitmapSource;
            if(image != null)
            {
                if(this.currentIcon == null)
                {
                    this.currentIcon = new IconFile();
                    this.imageList.ItemsSource = this.currentIcon.Images;
                }

                this.currentIcon.Images.Remove(image);
                this.modified = true;
            }
        }

        /// <summary>
        /// Executes the Cut command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void CutExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var image = this.imageList.SelectedItem as BitmapSource;
            if(image != null)
            {
                this.currentIcon.Images.Remove(image);
                Clipboard.SetImage(image);
                this.modified = true;
            }
        }

        /// <summary>
        /// Executes the Copy command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var image = this.imageList.SelectedItem as BitmapSource;
            if(image != null)
                Clipboard.SetImage(image);
        }

        /// <summary>
        /// Executes the Paste command.
        /// </summary>
        /// <param name="sender">Source of the event.</param>
        /// <param name="e">ExecutedRoutedEventArgs instance with information about the event.</param>
        private void PasteExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var image = Clipboard.GetImage();
            if(image != null && image.PixelWidth == image.PixelHeight && image.PixelWidth <= 256)
            {
                this.currentIcon.Images.Set(image);
                this.modified = true;
            }
        }
        #endregion
    }
}
