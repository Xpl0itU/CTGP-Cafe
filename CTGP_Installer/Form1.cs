using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace CTGP_Installer
{
  public class Form1 : Form
  {
    private List<DriveInfo> usableDrives = new List<DriveInfo>();
    private List<Form1.CTGPFile> serverFileList = new List<Form1.CTGPFile>();
    private string selectedDrive = "C:\\";
    private List<string> updates = new List<string>();
    public bool ready = true;
    private IContainer components;
    private ComboBox comboBox1;
    private Button button1;
    private ProgressBar progressBar1;
    private TextBox textBox1;
    private Button button2;

    public Form1() => this.InitializeComponent();

    public static void DownloadXML(Form1 this_)
    {
      try
      {
        ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback) ((_param1, _param2, _param3, _param4) => true);
        XmlReader xmlReader = XmlReader.Create(WebRequest.Create("https://rambo6dev.net/ctgp/ctgp_filelist.ver").GetResponse().GetResponseStream());
        while (xmlReader.Read())
        {
          if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "entry")
            this_.serverFileList.Add(new Form1.CTGPFile(xmlReader.GetAttribute("file"), xmlReader.GetAttribute("value")));
        }
      }
      catch (WebException ex)
      {
        int count = ex.Data.Count;
        int num = (int) MessageBox.Show("Couldn't lookup the server version filedata:\n\n" + ex.Message, "NET Error", MessageBoxButtons.OK);
        Environment.Exit(0);
      }
      this_.Text = "CTGP Updater / Installer";
      this_.comboBox1.Enabled = true;
    }

    public void listDrives()
    {
      this.button2.Text = "Update / Install CTGP-Café to selected drive";
      this.usableDrives.Clear();
      this.comboBox1.SelectedIndex = -1;
      this.comboBox1.Items.Clear();
      this.comboBox1.Text = "Select your SDCard ..";
      foreach (DriveInfo drive in DriveInfo.GetDrives())
      {
        if (drive.IsReady)
        {
          this.usableDrives.Add(drive);
          this.comboBox1.Items.Add((object) (drive.Name + " with format " + drive.DriveFormat));
        }
      }
    }

    public void Form1_Load(object sender, EventArgs args)
    {
      this.listDrives();
      this.comboBox1.Text = "Select your SDCard ..";
      Control.CheckForIllegalCrossThreadCalls = false;
      ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback) ((_param1, _param2, _param3, _param4) => true);
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
      this.Text = "Loading server filelist ...";
      new Thread((ThreadStart) (() => Form1.DownloadXML(this))).Start();
    }

    private void button1_Click(object sender, EventArgs e) => this.listDrives();

    private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
      ComboBox comboBox = (ComboBox) sender;
      if (comboBox.SelectedIndex < 0)
        return;
      this.selectedDrive = this.usableDrives[comboBox.SelectedIndex].Name;
      this.button2.Text = "Update / Install CTGP-Café to drive " + this.selectedDrive;
    }

    public static void Updater(Form1 this_)
    {
      this_.updates.Clear();
      if (!Directory.Exists(this_.selectedDrive + "ctgp8"))
        Directory.CreateDirectory(this_.selectedDrive + "ctgp8");
      if (!Directory.Exists(this_.selectedDrive + "ctgp8/file_data"))
        Directory.CreateDirectory(this_.selectedDrive + "ctgp8/file_data");
      int num1 = 1;
      foreach (Form1.CTGPFile serverFile in this_.serverFileList)
      {
        this_.textBox1.Text = string.Format("Checking file {0} / {1} ...", (object) num1, (object) this_.serverFileList.Count);
        if (System.IO.File.Exists(this_.selectedDrive + "ctgp8/file_data/" + serverFile.filename + ".dat"))
        {
          FileStream fileStream = System.IO.File.OpenRead(this_.selectedDrive + "ctgp8/file_data/" + serverFile.filename + ".dat");
          byte[] hash = new SHA1Managed().ComputeHash((Stream) fileStream);
          for (int index = 0; index < 20; ++index)
            hash[index] ^= (byte) 153;
          if (BitConverter.ToString(hash).Replace("-", string.Empty).ToLower() != serverFile.hash)
            this_.updates.Add(serverFile.filename);
          fileStream.Close();
        }
        else
          this_.updates.Add(serverFile.filename);
        ++num1;
      }
      if (this_.updates.Count > 0)
      {
        new Thread((ThreadStart) (() => Form1.UpdateFiles(this_))).Start();
      }
      else
      {
        int num2 = (int) MessageBox.Show("No updates are available!", nameof (Updater), MessageBoxButtons.OK);
      }
    }

    public static void DownloadProgress_Event(
      object sender,
      DownloadProgressChangedEventArgs e,
      Form1 this_,
      int a,
      int b)
    {
      this_.progressBar1.Value = e.ProgressPercentage;
      this_.progressBar1.Maximum = 100;
      this_.progressBar1.Minimum = 0;
      this_.textBox1.Text = string.Format("File {0} / {1} ({2})", (object) a, (object) b, (object) Form1.BytesToString(e.TotalBytesToReceive));
    }

    public static void DownloadComplete_Event(
      object sender,
      AsyncCompletedEventArgs e,
      Form1 this_)
    {
      this_.ready = true;
    }

    public static string BytesToString(long byteCount)
    {
      string[] strArray = new string[7]
      {
        "B",
        "KB",
        "MB",
        "GB",
        "TB",
        "PB",
        "EB"
      };
      if (byteCount == 0L)
        return "0" + strArray[0];
      long num1;
      int int32 = Convert.ToInt32(Math.Floor(Math.Log((double) (num1 = Math.Abs(byteCount)), 1024.0)));
      double num2 = Math.Round((double) num1 / Math.Pow(1024.0, (double) int32), 1);
      return ((double) Math.Sign(byteCount) * num2).ToString() + strArray[int32];
    }

    public static void UpdateFiles(Form1 this_)
    {
      WebClient webClient = new WebClient();
      webClient.DownloadFileCompleted += (AsyncCompletedEventHandler) ((sender, e) => Form1.DownloadComplete_Event(sender, e, this_));
      for (int i = 0; i < this_.updates.Count; i++)
      {
        ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback) ((_param1, _param2, _param3, _param4) => true);
        this_.ready = false;
        DownloadProgressChangedEventHandler changedEventHandler = (DownloadProgressChangedEventHandler) ((sender, e) => Form1.DownloadProgress_Event(sender, e, this_, i + 1, this_.updates.Count));
        webClient.DownloadProgressChanged += changedEventHandler;
        webClient.DownloadFileAsync(new Uri("https://rambo6dev.net/ctgp/file_data/" + this_.updates[i] + ".dat"), this_.selectedDrive + "ctgp8/file_data/" + this_.updates[i] + ".dat");
        do
          ;
        while (!this_.ready);
        this_.ready = false;
        webClient.DownloadProgressChanged -= changedEventHandler;
      }
      int num = (int) MessageBox.Show("Done updating!", "Updater", MessageBoxButtons.OK);
    }

    private void button2_Click(object sender, EventArgs e) => new Thread((ThreadStart) (() => Form1.Updater(this))).Start();

    protected override void Dispose(bool disposing)
    {
      if (disposing && this.components != null)
        this.components.Dispose();
      base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
      this.comboBox1 = new ComboBox();
      this.button1 = new Button();
      this.progressBar1 = new ProgressBar();
      this.textBox1 = new TextBox();
      this.button2 = new Button();
      this.SuspendLayout();
      this.comboBox1.Enabled = false;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.Location = new Point(13, 13);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new Size(291, 23);
      this.comboBox1.TabIndex = 0;
      this.comboBox1.Text = "Select your SDCard ...";
      this.comboBox1.SelectedIndexChanged += new EventHandler(this.comboBox1_SelectedIndexChanged);
      this.button1.Location = new Point(13, 43);
      this.button1.Name = "button1";
      this.button1.Size = new Size(291, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Refresh drive list";
      this.button1.UseVisualStyleBackColor = true;
      this.button1.Click += new EventHandler(this.button1_Click);
      this.progressBar1.Location = new Point(13, 416);
      this.progressBar1.Name = "progressBar1";
      this.progressBar1.Size = new Size(291, 32);
      this.progressBar1.TabIndex = 2;
      this.textBox1.Enabled = false;
      this.textBox1.Location = new Point(13, 387);
      this.textBox1.Name = "textBox1";
      this.textBox1.Size = new Size(291, 23);
      this.textBox1.TabIndex = 3;
      this.textBox1.Text = "File 0 / 0 (0KB)";
      this.textBox1.TextAlign = HorizontalAlignment.Center;
      this.button2.Location = new Point(13, 73);
      this.button2.Name = "button2";
      this.button2.Size = new Size(291, 56);
      this.button2.TabIndex = 4;
      this.button2.Text = "Update / Install CTGP-Café to selected drive";
      this.button2.UseVisualStyleBackColor = true;
      this.button2.Click += new EventHandler(this.button2_Click);
      this.AutoScaleDimensions = new SizeF(7f, 15f);
      this.AutoScaleMode = AutoScaleMode.Font;
      this.ClientSize = new Size(316, 460);
      this.Controls.Add((Control) this.button2);
      this.Controls.Add((Control) this.textBox1);
      this.Controls.Add((Control) this.progressBar1);
      this.Controls.Add((Control) this.button1);
      this.Controls.Add((Control) this.comboBox1);
      this.Name = nameof (Form1);
      this.Text = nameof (Form1);
      this.Load += new EventHandler(this.Form1_Load);
      this.ResumeLayout(false);
      this.PerformLayout();
    }

    public class CTGPFile
    {
      public string filename { get; set; }

      public string hash { get; set; }

      public CTGPFile(string fname, string h)
      {
        this.filename = fname;
        this.hash = h;
      }
    }
  }
}
