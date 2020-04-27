using Relic_IC_Image_Parser.cSharp.data;
using Relic_IC_Image_Parser.cSharp.imaging;
using Relic_IC_Image_Parser.cSharp.ui;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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

        public EditorWindow(string fullFileName)
        {
            InitializeComponent();

            myFile = new FileInfo(fullFileName);

            Title = myFile.Name + " - " + App.AppName;

            TextBox.Text = "Use your mouse to drag the image on the canvas.\n" + 
                "Use your mouse wheel to zoom in and out.";

            image = ImageManager.GetImage(ref fileType, fullFileName);
            if (fileType == FileType.Relic)
            {
                BtnToggleView.IsEnabled = ((RelicImage)image).GetSubImages().Count > 1;
            }

            if (image != null)
            {
                DisplayFullImage();
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (fileType == FileType.Unknown)
            {
                string msg = "The file you are trying to open is not supported" + 
                    " or it might be a damaged file.\n\n" + 
                    "If it's an SPT or TXR file it might be written in different format or empty" + 
                    " (you can never know with these files...).";
                string title = "Unknown format";

                MessageBox.Show(this, msg, title, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

                Close();
            }
        }

        private void DisplayText(string text)
        {
            TextBlock block = new TextBlock();
            block.Foreground = new SolidColorBrush(Color.FromRgb(48, 48, 48));
            block.FontSize = 14.0;
            block.FontWeight = FontWeights.Bold;
            block.Text = text;

            WorkArea.Children.Add(block);
            Canvas.SetTop(block, 20);
            Canvas.SetLeft(block, 40);
        }

        private void DisplayFullImage()
        {
            fullImage = new Image();
            if (fileType == FileType.Relic)
            {
                fullImage.Source = ((RelicImage)image).GetBitmap();
            }
            else //if (fileType == FileType.Standard)
            {
                fullImage.Source = (BitmapImage)image;
            }
            WorkArea.Children.Add(fullImage);
            Canvas.SetTop(fullImage, 40.0);
            Canvas.SetLeft(fullImage, 40.0);

            DisplayText("Full Image");
            BtnExport.IsEnabled = true;
            isFullImage = true;
        }

        private void DisplaySubImages()
        {
            fullImage = null;
            if (fileType == FileType.Relic)
            {
                RelicImage relicImage = (RelicImage)image;
                List<BitmapSource> subImages = relicImage.GetSubBitmaps();

                if (subImages.Count > 1)
                {
                    DisplayText("Inner Images");

                    if (relicImage.GetImageType() == RelicImage.ImageType.SPT)
                    {
                        double startTop = 40;
                        double startLeft = 40;
                        for (int i = 0; i < subImages.Count; i++)
                        {
                            Image image = new Image();
                            image.Source = subImages[i];
                            WorkArea.Children.Add(image);

                            Canvas.SetTop(image, startTop + relicImage.GetSubImages()[i].top * (255 + 40));
                            Canvas.SetLeft(image, startLeft + relicImage.GetSubImages()[i].left * (255 + 40));
                        }
                    }
                    else //if (relicImage.GetImageType() == RelicImage.ImageType.TXR)
                    {
                        double lastLeft = 40;
                        for (int i = 0; i < subImages.Count; i++)
                        {
                            Image image = new Image();
                            image.Source = subImages[i];
                            WorkArea.Children.Add(image);

                            Canvas.SetTop(image, 40.0);
                            Canvas.SetLeft(image, lastLeft);
                            lastLeft += subImages[i].Width + 40;
                        }
                    }
                }
            }
            else
            {
                DisplayText("No Inner Images");
            }
            BtnExport.IsEnabled = false;
            isFullImage = false;
        }

        private MatrixTransform GetCanvasMatrixTransform()
        {
            if (WorkArea.RenderTransform is TransformGroup)
            {
                foreach (Transform transform in ((TransformGroup)WorkArea.RenderTransform).Children)
                {
                    if (transform is MatrixTransform)
                    {
                        return (MatrixTransform) transform;
                    }
                }
            }
            return null;
        }

        private TranslateTransform GetCanvasTranslateTransform()
        {
            if (WorkArea.RenderTransform is TransformGroup)
            {
                foreach (Transform transform in ((TransformGroup)WorkArea.RenderTransform).Children)
                {
                    if (transform is TranslateTransform)
                    {
                        return (TranslateTransform) transform;
                    }
                }
            }
            return null;
        }

        private void ParentGrid_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            MatrixTransform matrix = GetCanvasMatrixTransform();
            TranslateTransform translate = GetCanvasTranslateTransform();
            if (matrix != null && translate != null) {
                Point mousePos = e.GetPosition(ParentGrid);

                double scale = e.Delta > 0 ? 1.1 : 1 / 1.1;

                Matrix mat = matrix.Matrix;
                mat.ScaleAt(scale, scale, mousePos.X - translate.X, mousePos.Y - translate.Y);
                matrix.Matrix = mat;
                e.Handled = true;
            }
        }

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

        private void ParentGrid_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            isCanvasDragging = false;
            WorkArea.ReleaseMouseCapture();
        }

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

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            DataManager.OpenFile(this);
        }

        private void BtnExport_Click(object sender, RoutedEventArgs e)
        {
            if (isFullImage && (fileType == FileType.Relic || fileType == FileType.Standard))
            {
                ExportWindow exportWindow = new ExportWindow(fileType, (BitmapSource)fullImage.Source.Clone());
                exportWindow.Owner = this;
                exportWindow.ShowDialog();
            }
        }

        private void BtnResetView_Click(object sender, RoutedEventArgs e)
        {
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

        private void BtnToggleView_Click(object sender, RoutedEventArgs e)
        {
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

        private void BtnAbout_Click(object sender, RoutedEventArgs e)
        {
            string msg = "Well not much about here...\n\n" + 
                "Just me, Meitar Azar, the creator.\n" + 
                "For you, The Modding communyty.\n" + 
                "Have fun :P\n\n" + 
                "App Version v" + App.VersionName;
            string title = "About " + App.AppName;

            MessageBox.Show(msg, title, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            App.editorsOpen--;
            if (App.editorsOpen == 0)
            {
                LaunchWindow launcher = new LaunchWindow();
                launcher.Show();
            }
            base.OnClosing(e);
        }
    }
}
