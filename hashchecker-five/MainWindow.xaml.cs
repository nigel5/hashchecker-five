#define DEBUG
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Security.Cryptography;

namespace hashchecker_five
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SolidColorBrush colorSuccess = new SolidColorBrush(Color.FromRgb(0, 100, 0));
        SolidColorBrush colorFailure = new SolidColorBrush(Color.FromRgb(100, 0, 0));
        SolidColorBrush colorWhite = new SolidColorBrush(Colors.White);
        SolidColorBrush colorBlack = new SolidColorBrush(Colors.Black);

        CheckBox[] checkBoxes;
        TextBox[] resultBoxes;

        int initialNumberOfCalculations;
        int numberOfRunningCalculations;

        public MainWindow()
        {
            InitializeComponent();

            checkBoxes = new CheckBox[] { this.md5Checkbox, this.sha256Checkbox, this.sha512Checkbox, this.sha1Checkbox };
            resultBoxes = new TextBox[] { this.md5Result, this.sha256Result, this.sha512Result, this.sha1Result };

            numberOfRunningCalculations = 0;
        }

        /// <summary>
        /// This method handles the open file button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            Stream fs = null;
            using (System.Windows.Forms.OpenFileDialog filePicker = new System.Windows.Forms.OpenFileDialog())
            {
                string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                if (home == null)
                {
                    // Try USERPROFILE if HOMEDRIVE is null
                    home = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);
                    if (home == null)
                    { 
                        home = "c:\\";
                    }
                }
                filePicker.InitialDirectory = home;
                filePicker.Filter = "All Files (*.*) | *.*";

                if(filePicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    filenameTextBox.Text = filePicker.FileName;
                    try
                    {
                        // Reset ...
                        ResetProgressBar();
                        ResetResultTextboxColors();
                        ResetResultTextboxes();
                        ResetUserInputTextbox();
                        if ((fs = filePicker.OpenFile()) != null)
                        {
                            if (checkBoxes[0].IsChecked == true)
                            {
                                numberOfRunningCalculations++;
                                initialNumberOfCalculations++;
                                Thread t0 = new Thread(() => UpdateMD5Hash(filePicker.FileName));
                                t0.Start();
                            }
                            if (checkBoxes[1].IsChecked == true)
                            {
                                numberOfRunningCalculations++;
                                initialNumberOfCalculations++;
                                Thread t1 = new Thread(() => UpdateSHA256Hash(filePicker.FileName));
                                t1.Start();
                            }
                            if (checkBoxes[2].IsChecked == true)
                            {
                                numberOfRunningCalculations++;
                                initialNumberOfCalculations++;
                                Thread t2 = new Thread(() => UpdateSHA512Hash(filePicker.FileName));
                                t2.Start();
                            }
                            if (checkBoxes[3].IsChecked == true)
                            {
                                numberOfRunningCalculations++;
                                initialNumberOfCalculations++;
                                Thread t3 = new Thread(() => UpdateSHA1Hash(filePicker.FileName));
                                t3.Start();
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        throw new Exception("Error MainWindow.xaml.cs:", err);
                    }
                }
            }
        }

        /// <summary>
        /// Resets progress bar.
        /// </summary>
        private void ResetProgressBar()
        {
            progressBar.Value = 0;
            progressBar.IsIndeterminate = false;
        }

        private void ResetUserInputTextbox()
        {
            clientResults.Background = colorWhite;
            clientResults.Foreground = colorBlack;
            clientResults.Text = "";
        }

        /// <summary>
        /// Reset result text box colors back to default.
        /// </summary>
        private void ResetResultTextboxColors()
        {
            foreach (TextBox tb in resultBoxes)
            {
                tb.Background = colorWhite;
                tb.Foreground = colorBlack;
            }
        }

        private void ResetResultTextboxes() {
            foreach (TextBox tb in resultBoxes)
            {
                tb.Background = colorWhite;
                tb.Foreground = colorBlack;
                tb.Text = "";
            }
        }

        /// <summary>
        /// This method controls the progress bar, and is called by each hash method once they are done.
        /// </summary>
        private void Done()
        {
            numberOfRunningCalculations--;
            if (numberOfRunningCalculations > 0)
            {
                this.Dispatcher.Invoke(() =>
                {
                    progressBar.IsIndeterminate = true;
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;
                });
            }
        }

        /// <summary>
        /// This method compares this hash result with the users hash that they entered, and provides an indicator.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompareResults_Click(object sender, RoutedEventArgs e)
        {
            ResetResultTextboxColors();
            string r = this.clientResults.Text;
            // Trim spaces and to upper case
            r = r.Trim().ToUpper();
            // Empty field...
            if (r == "")
            {
                return;
            }
            foreach (TextBox tb in resultBoxes)
            {
                if (tb.Text == r)
                {
                    // Good, set background of user textbox to (0, 100, 0)
                    this.clientResults.Background = colorSuccess;
                    this.clientResults.Foreground = colorWhite;

                    // Indicate to user which textbox it matched
                    tb.Background = colorSuccess;
                    tb.Foreground = colorWhite;
                    return;
                }
            }
            this.clientResults.Background = colorFailure;
            this.clientResults.Foreground = colorWhite;
        }

        /// <summary>
        /// This method calculates the MD5 hash and updates the textbox.
        /// </summary>
        /// <param name="file">Path of the file.</param>
        private void UpdateMD5Hash(string file)
        {
            using (MD5 md5 = MD5.Create())
            {
                Stream fs = File.OpenRead(file);
                var rawData = md5.ComputeHash(fs);

                // Update text box
                this.Dispatcher.Invoke(() => {
                    this.resultBoxes[0].Text = BitConverter.ToString(rawData).Replace("-", "").ToUpper();
                });
            }
            Done();
        }

        /// <summary>
        /// This method calculates the SHA256 hash and updates the textbox.
        /// </summary>
        /// <param name="file">Path of the file.</param>
        private void UpdateSHA256Hash(string file)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                Stream fs = File.OpenRead(file);
                var rawData = sha256.ComputeHash(fs);

                // Update text box
                this.Dispatcher.Invoke(() => {
                    this.resultBoxes[1].Text = BitConverter.ToString(rawData).Replace("-", "").ToUpper();
                });
            }
            Done();
        }


        /// <summary>
        /// This method calculates the SHA512 hash and updates the textbox.
        /// </summary>
        /// <param name="file">Path of the file.</param>
        private void UpdateSHA512Hash(string file)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                Stream fs = File.OpenRead(file);
                var rawData = sha512.ComputeHash(fs);

                // Update text box
                this.Dispatcher.Invoke(() => {
                    this.resultBoxes[2].Text = BitConverter.ToString(rawData).Replace("-", "").ToUpper();
                });
            }
            Done();
        }

        /// <summary>
        /// This method calculates the SHA1 hash and updates the textbox.
        /// </summary>
        /// <param name="file">Path of the file.</param>
        private void UpdateSHA1Hash(string file)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                Stream fs = File.OpenRead(file);
                var rawData = sha1.ComputeHash(fs);

                // Update text box
                this.Dispatcher.Invoke(() => {
                    this.resultBoxes[3].Text = BitConverter.ToString(rawData).Replace("-", "").ToUpper();
                });
            }
            Done();
        }
    }
}
