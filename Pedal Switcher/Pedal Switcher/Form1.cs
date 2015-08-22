﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;//For file dialog
using System.Windows.Forms;

namespace Pedal_Switcher
{
    
    public partial class Form1 : Form
    {
        PedalList pedalBoard;  //Top tray which contains available pedals on pedalboard
        PedalList pedalConfig; //Bottom tray which contains saved presets
        static System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();
        int[] currentConfig; //array for current preset
        int currentIndex = 0; //index of above array
        int[] savedConfigs; //array for all presets

        public Form1()
        {
            InitializeComponent();
            pedalBoard = new PedalList(pedalBoardHolder);
            pedalConfig = new PedalList(pedalConfigHolder);

            pedalBoard.addPanel("none", true); //add built in buffer to begin with

            //configuring a timer for 1ms
            myTimer.Tick += new EventHandler(TimerEventProcessor);
            myTimer.Interval = 1;
            myTimer.Start();

            Application.DoEvents(); //start timer

            currentConfig = new int[10]; //array for current preset
            savedConfigs = new int[100]; //array for all presets
            
        }

        //This 1ms timer checks to see if a button to add a pedal from the pedalboard to a preset is clicked
        private void TimerEventProcessor(Object myObject, EventArgs myEventArgs)
        {
            if (pedalBoard.getAddingPedal() == false)
            {
                for (int i = 0; i <= pedalBoard.getNumPedals(); i++)
                {
                    int clicked = Int32.Parse(pedalBoard.pedalInfos().ElementAt(i).ElementAt(3));
                    int number = Int32.Parse(pedalBoard.pedalInfos().ElementAt(i).ElementAt(2));
                    if (clicked == number)
                    {
                        bool alreadyUsed = false;   //used to see if a pedal is added as it cannot be used twice
                        for (int j = 0; j < currentIndex; j++)
                        {
                            if (currentConfig[j] == number)
                            {
                                alreadyUsed = true;
                            }
                        }

                        //ignores clicks on pedals that are already added
                        if (alreadyUsed == false)
                        {
                            if (number == 1) //buffer
                            {
                                pedalConfig.addPanel(pedalBoard.pedalInfos().ElementAt(0).ElementAt(1),true,false);
                            }
                            else
                            {
                                pedalConfig.addPanel(pedalBoard.pedalInfos().ElementAt(i).ElementAt(1),false,false);
                            }
                            currentConfig[currentIndex] = number;
                            currentIndex++;
                        }
                    }
                }
            }

        }

        private void AddPedal_Click(object sender, EventArgs e)
        {     
            pedalBoard.addPanel();   
        }

        //opens file dialog and loads pedalboard from that file
        private void Import_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "TXT Files|*.txt";
            openFileDialog1.Title = "Select a Pedalboard";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Read each line of the file into a string array. Each element 
                // of the array is one line of the file. 
                string[] lines = System.IO.File.ReadAllLines(openFileDialog1.FileName);
                
                foreach (string line in lines)
                {
                    if (line != "")
                    {
                        pedalBoard.addPanel(line);
                    }
                }
            }
        }

        //Opens a file dialog and saves pedalboard to that location
        private void Save_Click(object sender, EventArgs e)
        {
            string[] paths;
            paths = new string[14]; //Image File Paths
            for (int i = 0; i < pedalBoard.getNumPedals(); i++)
            {
                paths[i] = pedalBoard.pedalInfos().ElementAt(i+1).ElementAt(1);
            }
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";
            saveFileDialog1.Title = "Select a Save Location";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                System.IO.File.WriteAllLines(saveFileDialog1.FileName, paths);
            }
            // List<string> pedal12Info = pedalInfos().ElementAt(11);
            // string pedal12Label = pedal12Info.ElementAt(0);
        }

        //receives serial data of presets on the switcher - TODO
        private void Receive_Click(object sender, EventArgs e)
        {

        }

        //sends serial data of the presets to the switcher - TODO
        private void Send_Click(object sender, EventArgs e)
        {

        }     
    }

    public class PedalList
    {
        int numPanels = 0;
        List<Pedal> pedalList;
        Panel panelHolder;
        bool addingPedal = false;
        public PedalList(Panel holder)
        {
            pedalList = new List<Pedal>();
            panelHolder = holder;
            panelHolder.BorderStyle = BorderStyle.FixedSingle;
         
        }

        //adds a pedal
        public void addPanel(string path = "none", bool buffer = false, bool button = true)
        {
            //http://stackoverflow.com/questions/15385921/add-label-to-panel-programmatically
            addingPedal = true;
            numPanels = pedalList.Count;
            if (numPanels < 14)
            {
                string imagePath;

            
                Pedal pedal = new Pedal(button);
                
                
                pedal.Name = numPanels.ToString();
                // TODO: May not want this functionality for the pedalConfig, so you'll have to rearragne some stuff
                if (buffer == true)
                {
                    pedal.setLabel("buffer");
                    pedal.setImage(@"c:\Users\Kyle\Desktop\pedal.jpg");
                }
                if (buffer != true)
                {
                    if (path == "none")
                    {
                        OpenFileDialog openFileDialog1 = new OpenFileDialog();
                        openFileDialog1.Filter = "JPG Files|*.jpg";
                        openFileDialog1.Title = "Select a Pedal Image";
                        if (openFileDialog1.ShowDialog() == DialogResult.OK)
                        {
                            imagePath = openFileDialog1.FileName;
                            pedal.setImage(imagePath);
                        }
                    }
                    else
                    {
                        imagePath = path;
                        pedal.setImage(imagePath);       
                    }
                  
                }
                pedalList.Add(pedal);
            
                panelHolder.Controls.Add(pedal);

                panelHolder.Controls.SetChildIndex(pedal, numPanels);
                

            }
            addingPedal = false;
 
        }

        public int getNumPedals()
        {
            return numPanels;
        }

        //used to prevent crashing when a file dialog is open
        public bool getAddingPedal()
        {
            return addingPedal;
        }

        public List<List<string>> pedalInfos()
        {
            List<List<string>> infos = new List<List<string>>();
            foreach (Pedal p in pedalList)
            {
                infos.Add(p.getInfo());
                
            }
            return infos;
        }


    }

    public class Pedal : Panel
    {

        int panelWidth = 70;
        int panelHeight = 120;
        TextBox textBox;
        Button addButton;
        string backgroundImagePath;
        static int numPedals = 0;
        static int clicked = 14;

        public Pedal(bool button = true)
        {
            numPedals++;
            this.AllowDrop = true;
            this.Size = new Size(panelWidth, panelHeight);
            textBox = new TextBox();
            this.Controls.Add(textBox);
            if (button == true)
            {
                addButton = new Button();
                addButton.Text = "Add";
                addButton.Name = numPedals.ToString();
                addButton.Location = new Point(-2, 98);
                addButton.Click += new EventHandler(addButton_Click);
                this.Controls.Add(addButton);
            }
            this.Visible = true;
            this.BringToFront();
            this.Show();
            this.BringToFront();
        }

        //http://stackoverflow.com/questions/12276641/count-instances-of-the-class
        ~Pedal()
        {
            numPedals--;
        }

        public void setLabel(string text)
        {
            textBox.Text = text;
        }

        public void setImage(string path)
        {
            backgroundImagePath = path;
            this.BackgroundImage = Image.FromFile(path);
            this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        public List<string> getInfo()
        {
            // Put more stuff here...
            List<string> info = new List<string>();
            info.Add(textBox.Text); // 0
            info.Add(backgroundImagePath); // 1
            info.Add(addButton.Name); // 2
            
            if (addButton.Name == clicked.ToString())
            {
                info.Add(clicked.ToString()); // 3
                
            }
            else
            {
                //14 signifies that no pedal is clicked (TODO - this may need to be 0 or 15)
                info.Add("14");
                
            }
            return info;
        }

        //click handler for button on each pedal in pedalboard
        public void addButton_Click(object sender, EventArgs e)
        {
            clicked = Int32.Parse(addButton.Name);
        }

        public int getButtonClick()
        {
            return clicked;
        }

        

    }

}