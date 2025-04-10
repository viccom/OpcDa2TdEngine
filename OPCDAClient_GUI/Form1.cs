using System;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace OPCDAClient_GUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Upload_bt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "Select a CSV file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string[] csvLines = File.ReadAllLines(openFileDialog.FileName);

                        // Clear existing rows in the DataGridView
                        tagsTable.Rows.Clear();

                        // Skip the first line (header) and parse the remaining lines
                        for (int i = 1; i < csvLines.Length; i++)
                        {
                            string line = csvLines[i];
                            string[] values = line.Split(',');

                            // Skip rows with empty data
                            if (values.Length == 4 && !string.IsNullOrWhiteSpace(values[0]) &&
                                !string.IsNullOrWhiteSpace(values[1]) &&
                                !string.IsNullOrWhiteSpace(values[2]) &&
                                !string.IsNullOrWhiteSpace(values[3]))
                            {
                                // Add values to the DataGridView in the order of predefined headers
                                tagsTable.Rows.Add(values[0], values[1], values[2], values[3]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error reading CSV file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
