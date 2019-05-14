using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCMarkerEF;

namespace AddMaskinIDRows
{
    public partial class Form1 : Form
    {
        private int nAdd = 0;
        private int nExist = 0;
        public Form1()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            var maskinId = textBox1.Text.Trim();
            int index = listBox1.FindStringExact(maskinId);
            if (index == ListBox.NoMatches)
            {
                listBox1.Items.Add(maskinId);
            }
        }

        private void DelButton_Click(object sender, EventArgs e)
        {
            var index = listBox1.SelectedIndex;
            listBox1.Items.RemoveAt(index);
        }

        private void executeButton_Click(object sender, EventArgs e)
        {
            List<string> maskinIdLista = new List<string>();

            for (int i=0;i<listBox1.Items.Count;i++)
            {
                var item = listBox1.Items[i]; 
                maskinIdLista.Add(item.ToString());

            }

            DB db = DB.Instance;

            string avdelning = AvdelningTextbox.Text.Trim();
            var dbLista = db.LoadLaserDataFilteredText("Avdelning", avdelning);
            TotalLabel.Text = dbLista.Count.ToString();
            nAdd = 0;
            nExist = 0;
            for(int i= 0;i<dbLista.Count;i++)
            {
                var row = dbLista[i];
                foreach (var maskin in maskinIdLista)
                {
                    LaserData laserData = row;
                    if (!db.ExistsLaserData(maskin, laserData.F1, laserData.Kant))
                    {
                        laserData.MaskinID = maskin;
                        string nMaskin = Regex.Match(maskin, @"\d+$").Value;
                        laserData.Template = $"{laserData.F1}-{nMaskin}";
                        //Debug.WriteLine($"{laserData.F1} - {laserData.Kant} - {laserData.MaskinID} - {laserData.Template}");
                        db.AddLaserData(ref laserData);
                        nAdd++;
                        AddLabel.Text = nAdd.ToString();
                        
                    }
                    else
                    {
                        nExist++;
                        ExistLabel.Text = nExist.ToString();
                    }
                }
            }

             

        }
    }
}
