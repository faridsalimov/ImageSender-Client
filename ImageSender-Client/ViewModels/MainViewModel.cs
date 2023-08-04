using ImageSender_Client.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace ImageSender_Client.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public RelayCommand ConnectServerCommand { get; set; }
        public RelayCommand SelectImageCommand { get; set; }
        public RelayCommand SendImageCommand { get; set; }

        private BitmapImage image;
        public BitmapImage Image
        {
            get { return image; }
            set { image = value; OnPropertyChanged(); }
        }

        public bool IsConnected { get; set; } = false;
        public Socket Socket { get; set; }

        public MainViewModel()
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();

            ConnectServerCommand = new RelayCommand((obj) =>
            {
                var ipAddress = IPAddress.Parse(myIP);
                var port = 27001;

                if (!IsConnected)
                {
                    Task.Run(() =>
                    {
                        var endPoint = new IPEndPoint(ipAddress, port);

                        try
                        {
                            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            Socket.Connect(endPoint);

                            if (Socket.Connected)
                            {
                                IsConnected = true;
                                MessageBox.Show("Connected!", "Successfully!", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    });
                }

                else
                {
                    MessageBox.Show("You are already connected to the server.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            SelectImageCommand = new RelayCommand((obj) =>
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = "Image";
                dlg.DefaultExt = ".png";

                if (dlg.ShowDialog() == true)
                {
                    Image = new BitmapImage(new Uri(dlg.FileName));
                    ImageBrush brush = new ImageBrush(Image);
                }
            });

            SendImageCommand = new RelayCommand((obj) =>
            {
                if (IsConnected)
                {
                    try
                    {
                        var imageSend = Image;
                        var bytes = GetJPGFromImageControl(Image);
                        Socket.Send(bytes);
                        MessageBox.Show("Image sent successfully!", "Successfully!", MessageBoxButton.OK, MessageBoxImage.Information);
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                else
                {
                    MessageBox.Show("You are already connected to the server.", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }

        public byte[] GetJPGFromImageControl(BitmapImage imageC)
        {
            MemoryStream memStream = new MemoryStream();
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(imageC));
            encoder.Save(memStream);
            return memStream.ToArray();
        }
    }
}
