using Relic_IC_Image_Parser.cSharp.data;
using Relic_IC_Image_Parser.cSharp.imaging;
using Relic_IC_Image_Parser.cSharp.ui;
using Relic_IC_Image_Parser.cSharp.util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static Relic_IC_Image_Parser.cSharp.imaging.ImageManager;

namespace Relic_IC_Image_Parser
{
    /// <summary>
    /// Interaction logic for EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : Window
    {
        FileInfo myFile;
        FileType fileType;
        object image;

        private bool isFullImage = true;
        private Image fullImage;
        private bool isCanvasDragging;
        private Point canvasClickPosition;

        /// <summary>
        /// Some basic init stuff
        /// </summary>
        /// <param name="fullFileName"></param>
        public EditorWindow(string fullFileName)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Inithializing");

            InitializeComponent();

            myFile = new FileInfo(fullFileName);
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File name: " + fullFileName);

            Title = myFile.Name + " - " + App.AppName;

            TextBox.Text = "Use your mouse to drag the image on the canvas.\n" +
                "Use your mouse wheel to zoom in and out.";

            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Get image...");
            image = ImageManager.GetImage(ref fileType, fullFileName);
            if (fileType == FileType.Relic)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File is Relic, enable toggle mode");
                BtnToggleView.IsEnabled = ((RelicImage)image).GetSubImages().Count > 1;
            }

            // if we succeeded display the image
            if (image != null)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Image not null, displaying image...");
                DisplayFullImage();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (fileType == FileType.Unknown)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Oops... could not parse file...");

                string msg = "The file you are trying to open is not supported" +
                    " or it might be a damaged file.\n\n" +
                    "If it's an SPT or TXR file it might be written in different format or empty" +
                    " (you can never know with these files...).";
                string title = "Unknown format";

                MessageBox.Show(this, msg, title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Close();
            }
        }

        /// <summary>
        /// Add a new text 'title' to the canvas above the image
        /// </summary>
        /// <param name="text">The text to show</param>
        private void DisplayText(string text)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Display: " + text);

            TextBlock block = new TextBlock();

            // set up properties
            block.Foreground = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            block.FontSize = 14.0;
            block.FontWeight = FontWeights.Bold;
            block.Text = text;

            // append to canvas and set position
            WorkArea.Children.Add(block);
            Canvas.SetTop(block, 20);
            Canvas.SetLeft(block, 40);
        }

        /// <summary>
        /// Add a new image object to the canvas
        /// </summary>
        private void DisplayFullImage()
        {
            fullImage = new Image();

            // get the bitmap based on type
            if (fileType == FileType.Relic)
            {

                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Display Relic Full image");
                fullImage.Source = ((RelicImage)image).GetBitmap();
            }
            else //if (fileType == FileType.Standard)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Display standart image");
                fullImage.Source = (BitmapSource)image;
            }

            // append to canvas and set position
            WorkArea.Children.Add(fullImage);
            Canvas.SetTop(fullImage, 40.0);
            Canvas.SetLeft(fullImage, 40.0);

            // add a title
            DisplayText("Full Image");

            // set current state to full image
            BtnExport.IsEnabled = true;
            isFullImage = true;
        }

        /// <summary>
        /// Add a new image object to the canvas
        /// </summary>
        private void DisplaySubImages()
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Display sub images...");

            fullImage = null;

            // only Relic image has sub images
            if (fileType == FileType.Relic)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File is Relic, working...");

                RelicImage relicImage = (RelicImage)image;
                List<BitmapSource> subImages = relicImage.GetSubBitmaps();

                // there is no point to display the sub images if there is only one
                if (subImages.Count > 1)
                {
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "More than one sub image");

                    // set title
                    DisplayText("Inner Images");

                    if (relicImage.GetImageType() == RelicImage.ImageType.SPT)
                    {
                        Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Table layout of SPT");

                        double startTop = 40;
                        double startLeft = 40;
                        for (int i = 0; i < subImages.Count; i++)
                        {
                            Image image = new Image();
                            image.Source = subImages[i];
                            WorkArea.Children.Add(image);

                            RelicSubImage subImage = relicImage.GetSubImages()[i];

                            // using the row and column that we parse while loading the file
                            //   set the position on the canvas
                            Canvas.SetTop(image, startTop + subImage.top * 40 + subImage.GetPosPercent().topLeft.y * relicImage.GetBitmap().Height);
                            Canvas.SetLeft(image, startLeft + subImage.left * 40 + subImage.GetPosPercent().topLeft.x * relicImage.GetBitmap().Width);
                        }
                    }
                    else //if (relicImage.GetImageType() == RelicImage.ImageType.TXR)
                    {
                        Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Horizontal layout of TXR");

                        double lastLeft = 40;
                        for (int i = 0; i < subImages.Count; i++)
                        {
                            Image image = new Image();
                            image.Source = subImages[i];
                            WorkArea.Children.Add(image);

                            // with TXR we only need to move to the left
                            //   because its the same image but smaller every time
                            Canvas.SetTop(image, 40.0);
                            Canvas.SetLeft(image, lastLeft);
                            lastLeft += subImages[i].Width + 40;
                        }
                    }
                }
            }

            // standard image has no inner images
            //  BECAUSE THEY KNOW ITS DUMB!!!
            else
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "File is not Relic, aborting...");
                DisplayText("No Inner Images");
            }

            // set the current state to sub images
            BtnExport.IsEnabled = false;
            isFullImage = false;
        }

        /// <summary>
        /// Search and return the <see cref="MatrixTransform"/> of the canvas.
        /// </summary>
        /// <returns></returns>
        private MatrixTransform GetCanvasMatrixTransform()
        {
            if (WorkArea.RenderTransform is TransformGroup)
            {
                foreach (Transform transform in ((TransformGroup)WorkArea.RenderTransform).Children)
                {
                    if (transform is MatrixTransform)
                    {
                        return (MatrixTransform)transform;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Search and return the <see cref="TranslateTransform"/> of the canvas.
        /// </summary>
        /// <returns></returns>
        private TranslateTransform GetCanvasTranslateTransform()
        {
            if (WorkArea.RenderTransform is TransformGroup)
            {
                foreach (Transform transform in ((TransformGroup)WorkArea.RenderTransform).Children)
                {
                    if (transform is TranslateTransform)
                    {
                        return (TranslateTransform)transform;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Taking the mouse wheel and zooming the canves.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            MatrixTransform matrix = GetCanvasMatrixTransform();
            TranslateTransform translate = GetCanvasTranslateTransform();
            if (matrix != null && translate != null)
            {
                Point mousePos = e.GetPosition(ParentGrid);

                double scale = e.Delta > 0 ? 1.1 : 1 / 1.1;

                Matrix mat = matrix.Matrix;
                mat.ScaleAt(scale, scale, mousePos.X - translate.X, mousePos.Y - translate.Y);
                matrix.Matrix = mat;
                e.Handled = true;
            }
        }

        /// <summary>
        /// Starts to capture mouse movements when mouse pressing on the canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentGrid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isCanvasDragging = true;
            canvasClickPosition = e.GetPosition(ParentGrid);
            TranslateTransform translate = GetCanvasTranslateTransform();
            if (translate != null)
            {
                canvasClickPosition.X -= translate.X;
                canvasClickPosition.Y -= translate.Y;
            }
            WorkArea.CaptureMouse();
        }

        /// <summary>
        /// Stopping to capture mouse movements when releasing the canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isCanvasDragging = false;
            WorkArea.ReleaseMouseCapture();
        }

        /// <summary>
        /// While moving on the canvas, if we are capturing movement move the canvas.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParentGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isCanvasDragging)
            {
                Point currentPosition = e.GetPosition(WorkArea.Parent as UIElement);

                TranslateTransform translate = GetCanvasTranslateTransform();
                if (translate != null)
                {
                    translate.X = currentPosition.X - canvasClickPosition.X;
                    translate.Y = currentPosition.Y - canvasClickPosition.Y;
                }
            }
        }

        /// <summary>
        /// The user always right! Open file please!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Open new file...");
            DataManager.OpenNewFile(this);
        }

        /// <summary>
        /// Well well well... You want to export huh?
        /// <para>Well, we have a pocedure just for that inside Export window.</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Exporting...");

            if (isFullImage && (fileType == FileType.Relic || fileType == FileType.Standard))
            {
                ExportWindow exportWindow = new ExportWindow(fileType, myFile.Name, (BitmapSource)fullImage.Source.Clone());
                exportWindow.Owner = this;
                exportWindow.ShowDialog();
            }
        }

        /// <summary>
        /// If you have made a mess from the canvas, reset the zoom and position to 0.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnResetView_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Reset canvas");

            TransformGroup group = (TransformGroup)WorkArea.RenderTransform;

            for (int i = 0; i < group.Children.Count; i++)
            {
                if (group.Children[i] is MatrixTransform)
                {
                    group.Children[i] = new MatrixTransform();
                }
                else if (group.Children[i] is TranslateTransform)
                {
                    group.Children[i] = new TranslateTransform();
                }
            }
        }

        /// <summary>
        /// Used to switch between the grand image or the sub images of the relic image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnToggleView_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Toggle view");

            BtnResetView_Click(null, null);
            WorkArea.Children.Clear();

            if (isFullImage)
            {
                DisplaySubImages();
            }
            else
            {
                DisplayFullImage();
            }
        }

        /// <summary>
        /// Credits are important.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Open about");

            AboutWindow aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// Keep watch if we are the last Editor. If we are reopen the launcher.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Close.");

            App.editorsOpen--;
            if (App.editorsOpen == 0)
            {
                Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, "Last editor, open LaunchWindow if needed");
                if (DataManager.alwaysReturnToLauncher || !App.openedFromArgs)
                {
                    Logger.Append(MethodBase.GetCurrentMethod().DeclaringType.Name, MethodBase.GetCurrentMethod().Name, 
                        "we need [alwaysReturnToLauncher: " + DataManager.alwaysReturnToLauncher + ", openedFromArgs: " + App.openedFromArgs + "], openning LaunchWindow");
                    LaunchWindow launcher = new LaunchWindow();
                    launcher.Show();
                }
            }
            base.OnClosing(e);
        }
    }
}
