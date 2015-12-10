/*
 * author: shepnerd
 * mail: wygamle@pku.edu.cn
 * date: 2015-11-02
 * update: 2015-12-10
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;

namespace VOCMarker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            check_dots.Checked += check_Checked;
            check_rects.Checked += check_Checked;
        }

        #region public variables
        // 
        private ImageBrush brush;
        private string imageName = null;
        private Bitmap image = null;
        private System.Drawing.Bitmap resultImage = null;
        private System.Drawing.Imaging.ImageFormat imgFormat;

        private Ellipse canvasPoint = null;
        private double radius = 4.0;
        private System.Windows.Media.Color color = System.Windows.Media.Color.FromRgb(255, 0, 0);

        
        private List<System.Windows.Point> markedPoints = new List<System.Windows.Point>();

        private List<System.Windows.Rect> markedRegions = new List<Rect>();

        private System.Windows.Point startPoint;

        private System.Windows.Shapes.Rectangle region_buf = null;
        #endregion
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as MenuItem;
            if(item.Name.Equals("menu_load"))
            {
                System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
                openFileDialog.Title = "选择图片";
                openFileDialog.Filter = "JPEG FIles(*.jpg)|*.jpg|PNG files(*.png)|*.png|BMP Files(*.bmp)|*.bmp";
                openFileDialog.CheckFileExists = true;
                openFileDialog.CheckPathExists = true;

                Stream stream = null;

                if(openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        if((stream = openFileDialog.OpenFile()) != null)
                        {
                            using (stream)
                            {
                                string file = openFileDialog.FileName;
                                imageName = file;
                                brush = new ImageBrush();
                                image = new Bitmap(file);
                                brush.ImageSource = new BitmapImage(new Uri(file, UriKind.Relative));
                                canvas_ink.Height = image.Height;
                                canvas_ink.Width = image.Width;
                                canvas_ink.Background = brush;
                                canvas_ink.Children.Clear();
                                markedPoints.Clear();
                                markedRegions.Clear();
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Error: Could not read file from disk. Original error: "+ex.Message);
                    }
                }
            }
            else if(item.Name.Equals("menu_exit"))
            {
                this.Close();
            }
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Name)
            {
                case "btn_save":
                    if(markedPoints.Count > 0)
                    {
                        double ratioy = image.Height / canvas_ink.Height;
                        double ratiox = image.Width / canvas_ink.Width;

                        Bitmap result = new Bitmap((int)image.Width, (int)image.Height);
                        Graphics g = Graphics.FromImage(result);
                        g.Clear(System.Drawing.Color.Black);
                        System.Drawing.Color red = System.Drawing.Color.Red;

                        string[] filenames = imageName.Split('\\');
                        string filename = filenames[filenames.Length-1];
                        filename = filename.Substring(0, filename.Length - 4);

                        if(!Directory.Exists(filename))
                        {
                            Directory.CreateDirectory(filename);
                        }

                        string targetFolder = Directory.GetCurrentDirectory() + "\\" + filename+"\\";
                        try
                        {
                            FileStream fs = new FileStream(targetFolder + filename + "_voc.txt", FileMode.Create);
                            StreamWriter sw = new StreamWriter(fs);
                            foreach (var p in markedPoints)
                            {
                                int y = (int)Math.Floor(p.Y * ratioy);
                                int x = (int)Math.Floor(p.X * ratiox);
                                sw.WriteLine(string.Format(y.ToString()+" "+x.ToString()));
                                result.SetPixel(x, y, red);
                            }
                            sw.Close();
                            fs.Close();
                            result.Save(targetFolder + filename + ".bmp");
                            MessageBox.Show("保存成功");
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show("Error: Open File error, msg: " + ex.Message);
                            return;
                        } 
                    }

                    if(markedRegions.Count > 0)
                    {
                        double ratioy = image.Height / canvas_ink.Height;
                        double ratiox = image.Width / canvas_ink.Width;

                        //Bitmap result = new Bitmap((int)image.Width, (int)image.Height);
                        //Graphics g = Graphics.FromImage(result);
                        //g.Clear(System.Drawing.Color.Black);
                        //System.Drawing.Color red = System.Drawing.Color.Red;

                        string[] filenames = imageName.Split('\\');
                        string filename = filenames[filenames.Length - 1];
                        filename = filename.Substring(0, filename.Length - 4);

                        if (!Directory.Exists(filename))
                        {
                            Directory.CreateDirectory(filename);
                        }

                        string targetFolder = Directory.GetCurrentDirectory() + "\\" + filename + "\\";
                        try
                        {
                            FileStream fs = new FileStream(targetFolder + filename + "_vod.txt", FileMode.Create);
                            StreamWriter sw = new StreamWriter(fs);
                            foreach (var r in markedRegions)
                            {
                                int y = (int)Math.Floor(r.Y * ratioy);
                                int x = (int)Math.Floor(r.X * ratiox);
                                int w = (int)Math.Floor(r.Width * ratiox);
                                int h = (int)Math.Floor(r.Height * ratioy);
                                sw.WriteLine(string.Format(y.ToString() + " " + x.ToString() + " " + r.Height.ToString() + " " + r.Width.ToString()));
                                //result.SetPixel(x, y, red);
                            }
                            sw.Close();
                            fs.Close();
                            //result.Save(targetFolder + filename + ".bmp");
                            MessageBox.Show("保存成功");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error: Open File error, msg: " + ex.Message);
                            return;
                        } 
                    }

                    break;
                case "btn_revoke":
                    if(check_dots.IsChecked.Value && markedPoints.Count > 0)
                    {
                        canvas_ink.Children.RemoveAt(markedPoints.Count-1);
                        markedPoints.RemoveAt(markedPoints.Count - 1);
                    }
                    if(check_rects.IsChecked.Value && markedRegions.Count > 0)
                    {
                        canvas_ink.Children.RemoveAt(markedRegions.Count - 1);
                        markedRegions.RemoveAt(markedRegions.Count - 1);
                    }

                    break;
                case "btn_clear":
                    canvas_ink.Children.Clear();
                    if(check_dots.IsChecked.Value)
                        markedPoints.Clear();
                    if (check_rects.IsChecked.Value)
                        markedRegions.Clear();

                    break;
            }
        }

        private void check_Checked(object sender, RoutedEventArgs e)
        {
            var checkbox = sender as CheckBox;
            if(checkbox.Name.Equals("check_dots"))
            {
                check_rects.IsChecked = !check_dots.IsChecked;
            }
            else if(checkbox.Name.Equals("check_rects"))
            {
                check_dots.IsChecked = !check_rects.IsChecked;
            }
        }

        private void canvas_ink_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point currentpoint = Mouse.GetPosition(canvas_ink);

            lab_prop.Content = Convert.ToString("X="+Math.Floor(currentpoint.X)+","+"Y="+Math.Floor(currentpoint.Y));
            
            if(check_dots.IsChecked.Value) // choose dots
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                {
                    /*
                    canvasPoint = new Ellipse();
                    canvasPoint.Stroke = new SolidColorBrush(color);
                    canvasPoint.StrokeThickness = 1.0;
                    canvasPoint.Fill = new SolidColorBrush(color);
                    canvasPoint.Width = radius;
                    canvasPoint.Height = radius;
                    Canvas.SetTop(canvasPoint, currentpoint.Y);
                    Canvas.SetLeft(canvasPoint, currentpoint.X);
                    canvas_ink.Children.Add(canvasPoint);
                    */
                }
            }
            else
            {
                if(e.LeftButton == MouseButtonState.Pressed)
                {
                    var region = new System.Windows.Shapes.Rectangle();
                    region.Stroke = new SolidColorBrush(color);
                    region.StrokeThickness = 1.0;
                    //lab_prop.Content = e.GetPosition(canvas_ink).X.ToString() + " " + e.GetPosition(canvas_ink).Y.ToString();
                    region.Width = Math.Abs(e.GetPosition(canvas_ink).X - startPoint.X);
                    region.Height = Math.Abs(e.GetPosition(canvas_ink).Y - startPoint.Y);

                    if(region.Width >= 0 && region.Width <= canvas_ink.Width && region.Height >= 0 && region.Height <= canvas_ink.Height)
                    {
                        Canvas.SetLeft(region, startPoint.X);
                        Canvas.SetTop(region, startPoint.Y);
                        canvas_ink.Children.Add(region);
                        if (canvas_ink.Children.IndexOf(region_buf) >= 0)
                            canvas_ink.Children.Remove(region_buf);
                        region_buf = region;
                    }
                    
                    
                    //canvasPoint = new Ellipse();
                    //canvasPoint.Stroke = new SolidColorBrush(color);
                    //canvasPoint.StrokeThickness = 1.0;
                    //canvasPoint.Fill = new SolidColorBrush(color);
                    //canvasPoint.Width = radius;
                    //canvasPoint.Height = radius;
                    //Canvas.SetTop(canvasPoint, currentpoint.Y);
                    //Canvas.SetLeft(canvasPoint, currentpoint.X);
                    //canvas_ink.Children.Add(canvasPoint);
                }
                else
                {
                    if(region_buf != null)
                    {
                        Rect r = new Rect(startPoint.X,startPoint.Y,region_buf.Width,region_buf.Height);
                        markedRegions.Add(r);
                        region_buf = null;
                    }

                }
            }
        }

        private void canvas_ink_MouseEnter(object sender, MouseEventArgs e)
        {
            if(check_dots.IsChecked.Value)
            {
                canvas_ink.Cursor = Cursors.Pen;
            }
            else
            {
                canvas_ink.Cursor = Cursors.Cross;
            }
        }

        private void canvas_ink_MouseLeave(object sender, MouseEventArgs e)
        {
            lab_prop.Content = string.Empty;
        }

        private void canvas_ink_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point currentpoint = Mouse.GetPosition(canvas_ink);
            if (check_dots.IsChecked.Value) // choose dots
            {
                    
                    canvasPoint = new Ellipse();
                    canvasPoint.Stroke = new SolidColorBrush(color);
                    canvasPoint.StrokeThickness = 1.0;
                    canvasPoint.Fill = new SolidColorBrush(color);
                    canvasPoint.Width = radius;
                    canvasPoint.Height = radius;
                    Canvas.SetTop(canvasPoint, currentpoint.Y);
                    Canvas.SetLeft(canvasPoint, currentpoint.X);
                    canvas_ink.Children.Add(canvasPoint);
                    markedPoints.Add(currentpoint);
            }
            else
            {
                startPoint = e.GetPosition(canvas_ink);
            }

        }


    }
}
