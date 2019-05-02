using LightBuzz.Vitruvius;
using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;

namespace KinectWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensor _sensor = null;
        private CoordinateMapper _coordinateMapper = null;
        private MultiSourceFrameReader _frameReader = null;
        private PlayersController _playersController = null;

        private FrameCounter _frameCounter = null;
        private FrameRateCounter _frameRateCounter = null;
        private TimeCounter _timeCounter = null;

        private List<WriteableBitmap> _colorFrames = null;
        private List<WriteableBitmap> _depthFrames = null;
        private List<WriteableBitmap> _mappedFrames = null;
        private List<string> _jointFrames = null;

        private const string RGB_PATH = "Color";
        private const string DEPTH_PATH = "Depth";
        private const string MAPPED_PATH = "Mapped";
        private const string JOINTS_PATH = "Joints";
        
        private bool _recording = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            _coordinateMapper = _sensor.CoordinateMapper;
            
            if (_sensor != null)
            {
                _sensor.Open();

                _frameReader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Infrared | FrameSourceTypes.Body | FrameSourceTypes.BodyIndex);
                _frameReader.MultiSourceFrameArrived += MultiSourceFrameArrived;

                _playersController = new PlayersController();
                _playersController.BodyEntered += BodyEntered;
                _playersController.BodyLeft += BodyLeft;
                _playersController.Start();
            }

            _frameCounter = new FrameCounter();
            _frameRateCounter = new FrameRateCounter();
            _timeCounter = new TimeCounter();

            _colorFrames = new List<WriteableBitmap>();
            _depthFrames = new List<WriteableBitmap>();
            _mappedFrames = new List<WriteableBitmap>();
            _jointFrames = new List<string>();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _frameReader.Dispose();
            _sensor.Close();
        }

        private void MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();

            if (reference == null)
                return;

            Show(reference);

            Record(reference);
        }

        private void ElapsedTime(Object source, ElapsedEventArgs e)
        {

        }

        private void BodyEntered(object sender, PlayersControllerEventArgs e)
        {

        }

        private void BodyLeft(object sender, PlayersControllerEventArgs e)
        {
            viewer.Clear();
        }

        private void BtnRecord_Click(object sender, RoutedEventArgs e)
        {
            lblFeedback.Content = "";

            #region [Validar Entradas]

            if (string.IsNullOrEmpty(txtFolder.Text))
            {
                Forms.MessageBox.Show(
                    "Você deve selecionar um diretório para a gravação!!",
                    "Selecionar diretório para gravação!!",
                    Forms.MessageBoxButtons.OK,
                    Forms.MessageBoxIcon.Exclamation);

                return;
            }

            if (string.IsNullOrEmpty(txtRecord.Text))
            {
                Forms.MessageBox.Show(
                    "Você deve selecionar o nome da gravação!!",
                    "Selecionar nome da gravação!!",
                    Forms.MessageBoxButtons.OK,
                    Forms.MessageBoxIcon.Exclamation);

                return;
            }

            #endregion

            if (!_recording)
            {
                var path = Path.Combine(txtFolder.Text, txtRecord.Text);

                if (Directory.Exists(path))
                {
                    var result = Forms.MessageBox.Show(
                        string.Format("Já existe uma gravação com o nome \"{0}\". Deseja sobreescrevê-la?", txtRecord.Text), 
                        "Gravação Já Existente!!", 
                        Forms.MessageBoxButtons.YesNo,
                        Forms.MessageBoxIcon.Exclamation);

                    if (result == Forms.DialogResult.No)
                        return;

                    DirectoryTwo.RecursiveDelete(path);
                }
                
                var rgbPath = Path.Combine(path, RGB_PATH);
                var depthPath = Path.Combine(path, DEPTH_PATH);
                var mappedPath = Path.Combine(path, MAPPED_PATH);
                var jointPath = Path.Combine(path, JOINTS_PATH);

                Directory.CreateDirectory(path);

                Directory.CreateDirectory(rgbPath);
                Directory.CreateDirectory(depthPath);
                Directory.CreateDirectory(mappedPath);
                Directory.CreateDirectory(jointPath);
                
                btnRecord.Content = "Stop";
                
                _recording = true;
            }
            else
            {
                _recording = false;
                
                _frameCounter.Reset();
                _frameRateCounter.Reset();
                _timeCounter.Reset();

                lblFeedback.Content = "Persistindo dados...";
                lblTotalFrames.Content = 0;
                lblFPS.Content = 0;
                lblTime.Content = "00:00";

                var path = Path.Combine(txtFolder.Text, txtRecord.Text);

                var rgbPath = Path.Combine(path, RGB_PATH);
                var depthPath = Path.Combine(path, DEPTH_PATH);
                var mappedPath = Path.Combine(path, MAPPED_PATH);
                var jointsPath = Path.Combine(path, JOINTS_PATH);

                PersistFrame(txtRecord.Text, rgbPath, _colorFrames);
                PersistFrame(txtRecord.Text, depthPath, _depthFrames);
                PersistFrame(txtRecord.Text, mappedPath, _mappedFrames);
                PersistJoints(txtRecord.Text, jointsPath, _jointFrames);

                _colorFrames.Clear();
                _depthFrames.Clear();
                _mappedFrames.Clear();
                _jointFrames.Clear();

                lblFeedback.Content = "Video salvo com sucesso!";
                txtRecord.Text = string.Empty;
                btnRecord.Content = "Record";
            }
        }

        private void BtnFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Forms.FolderBrowserDialog();

            var dialogResult = folderDialog.ShowDialog();

            if (dialogResult == Forms.DialogResult.OK)
            {
                txtRecord.IsEnabled = true;
                txtFolder.Text = folderDialog.SelectedPath;
            }
        }

        private void Show(MultiSourceFrame reference)
        {
            switch (cbMode.SelectedIndex)
            {
                case 0: //RGB

                    viewer.Clear();

                    viewer.Visualization = Visualization.Color;

                    using (var colorFrame = reference.ColorFrameReference.AcquireFrame())
                    {
                        if (colorFrame != null)
                            viewer.Image = colorFrame.ToBitmap();
                    }

                    break;

                case 1: // Depth

                    viewer.Clear();

                    viewer.Visualization = Visualization.Depth;

                    using (var frame = reference.DepthFrameReference.AcquireFrame())
                    {
                        if (frame != null)
                            viewer.Image = frame.ToBitmap();
                    }

                    break;

                case 2: // Background Removal

                    viewer.Clear();

                    using (var colorFrame = reference.ColorFrameReference.AcquireFrame())
                    using (var depthFrame = reference.DepthFrameReference.AcquireFrame())
                    using (var bodyIndexFrame = reference.BodyIndexFrameReference.AcquireFrame())
                    {
                        if (colorFrame != null && depthFrame != null && bodyIndexFrame != null)
                        {
                            var bitmap = colorFrame.GreenScreen(depthFrame, bodyIndexFrame);
                            viewer.Image = bitmap;
                        }
                    }

                    break;

                case 3: // Joints

                    viewer.Clear();

                    viewer.Visualization = Visualization.Color;

                    using (var frame = reference.ColorFrameReference.AcquireFrame())
                    {
                        if (frame != null)
                            viewer.Image = frame.ToBitmap();
                    }

                    using (var frame = reference.BodyFrameReference.AcquireFrame())
                    {
                        if (frame != null)
                        {
                            var bodies = frame.Bodies();

                            _playersController.Update(bodies);

                            foreach (var body in bodies)
                            {
                                var brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));
                                viewer.DrawBody(body, 20, brush, 0, brush);
                            }
                        }
                    }

                    break;

                case 4: // Skeleton

                    viewer.Clear();

                    viewer.Visualization = Visualization.Color;

                    using (var frame = reference.ColorFrameReference.AcquireFrame())
                    {
                        if (frame != null)
                            viewer.Image = frame.ToBitmap();
                    }

                    using (var frame = reference.BodyFrameReference.AcquireFrame())
                    {
                        if (frame != null)
                        {
                            var bodies = frame.Bodies();

                            _playersController.Update(bodies);

                            foreach (var body in bodies)
                            {
                                var brush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                                viewer.DrawBody(body, 20, brush, 10, brush);
                            }
                        }
                    }

                    break;
            }
        }

        private void Record(MultiSourceFrame reference)
        {
            if (!_recording)
                return;

            using (var colorFrame = reference.ColorFrameReference.AcquireFrame())
            using (var depthFrame = reference.DepthFrameReference.AcquireFrame())
            using (var bodyFrame = reference.BodyFrameReference.AcquireFrame())
            using (var bodyIndexFrame = reference.BodyIndexFrameReference.AcquireFrame())
            {
                if (colorFrame != null && depthFrame != null && bodyFrame != null && bodyIndexFrame != null)
                {
                    var totalFrames = _frameCounter.GetTotalFrames();
                    var fps = _frameRateCounter.GetFPS();
                    var time = _timeCounter.GetElapsedTime();

                    lblTotalFrames.Content = totalFrames.ToString();
                    lblFPS.Content = fps.ToString("#");
                    lblTime.Content = time.ToString(@"mm\:ss");

                    var mapped = ImageConverter.ColorToDepth(_coordinateMapper, colorFrame, depthFrame, bodyIndexFrame);
                    var color = colorFrame.ToBitmap(ColorFrameResolution.Resolution_640x360);
                    var depth = depthFrame.ToDepthBitmap();
                    //var mapped = colorFrame.GreenScreen(depthFrame, bodyIndexFrame);
                    var body = bodyFrame.Bodies().Closest().ToUpperBodyCSV();

                    _colorFrames.Add(color);
                    _depthFrames.Add(depth);
                    _mappedFrames.Add(mapped);
                    _jointFrames.Add(body);
                }
            }
        }

        private void PersistFrame(
            string record, 
            string path, 
            List<WriteableBitmap> frames)
        {
            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];

                var file = string.Format(
                    "{0}_{1}.png",
                    record,
                    i);
                
                frame.Save(Path.Combine(path, file), ImageFormats.Png);
            }
        }

        private void PersistJoints(
            string record, 
            string path, 
            List<string> frames)
        {
            for (var i = 0; i < frames.Count; i++)
            {
                var frame = frames[i];

                var file = string.Format(
                    "{0}_{1}.csv",
                    record,
                    i);
                
                File.WriteAllText(Path.Combine(path, file), frame);
            }
        }
    }
}
