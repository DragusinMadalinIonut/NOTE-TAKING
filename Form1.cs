using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        private string connectionString = "Data Source=DESKTOP-1LE57OT;Initial Catalog=DB;Integrated Security=True;";
        private SqlConnection connection;
        private SqlDataAdapter dataAdapter;
        private DataTable dataTable;

        private void notesBox_TextChanged(object sender, EventArgs e)
        {
            if (previousNotes.SelectedRows.Count > 0)
            {
                int selectedIndex = previousNotes.SelectedRows[0].Index;
                int noteId = Convert.ToInt32(previousNotes.Rows[selectedIndex].Cells["NoteID"].Value);

                dataTable.Rows[selectedIndex]["Content"] = notesBox.Text; // Update the Content in the DataTable
                dataTable.Rows[selectedIndex]["Title"] = titleBox.Text; // Update the Title in the DataTable
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerifyConnection())
                {
                    dataAdapter.Update(dataTable); // Save changes to the database
                    MessageBox.Show("Changes saved successfully!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while saving changes: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
            LoadNotes(); // Reload the data to refresh the DataGridView
        }


        public Form1()
        {
            InitializeComponent();
            connection = new SqlConnection(connectionString);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadNotes();

            // Set font properties for notesBox TextBox
            notesBox.Font = new System.Drawing.Font("Arial", 12, System.Drawing.FontStyle.Regular);

            // Check if previousNotes DataGridView has columns
            if (previousNotes.Columns.Count > 0)
            {
                // Set font properties for specific columns
                previousNotes.Columns["Title"].DefaultCellStyle.Font = new System.Drawing.Font("Calibri", 10, System.Drawing.FontStyle.Bold);
                previousNotes.Columns["Content"].DefaultCellStyle.Font = new System.Drawing.Font("Calibri", 10, System.Drawing.FontStyle.Regular);
            }
        }

        private void LoadNotes()
        {
            try
            {
                if (VerifyConnection())
                {
                    string selectQuery = "SELECT NoteID, Title, Content FROM Notes";
                    dataAdapter = new SqlDataAdapter(selectQuery, connection);
                    dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);

                    previousNotes.DataSource = dataTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while loading notes: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }

        private void loadBTN_Click(object sender, EventArgs e)
        {
            try
            {
                if (previousNotes.SelectedRows.Count > 0)
                {
                    int selectedIndex = previousNotes.SelectedRows[0].Index;
                    int noteId = Convert.ToInt32(previousNotes.Rows[selectedIndex].Cells["NoteID"].Value);

                    DataRow[] selectedRows = dataTable.Select($"NoteID = {noteId}");
                    if (selectedRows.Length > 0)
                    {
                        titleBox.Text = selectedRows[0]["Title"].ToString();
                        notesBox.Text = selectedRows[0]["Content"].ToString();
                        MessageBox.Show("Note loaded successfully!");
                    }
                    else
                    {
                        titleBox.Text = string.Empty;
                        notesBox.Text = string.Empty;
                        MessageBox.Show("Failed to load note.");
                    }
                }
                else
                {
                    titleBox.Text = string.Empty;
                    notesBox.Text = string.Empty;
                    MessageBox.Show("Please select a note to load.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while loading note: " + ex.Message);
            }
        }

        private void previousNotes_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            DataGridViewRow editedRow = previousNotes.Rows[rowIndex];
            int noteId = Convert.ToInt32(editedRow.Cells["NoteID"].Value);

            DataRow[] selectedRows = dataTable.Select($"NoteID = {noteId}");
            if (selectedRows.Length > 0)
            {
                selectedRows[0]["Title"] = editedRow.Cells["Title"].Value;
                selectedRows[0]["Content"] = editedRow.Cells["Content"].Value;
            }
        }

        private bool VerifyConnection()
        {
            try
            {
                connection.Open();
                connection.Close();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to connect to the database: " + ex.Message);
                return false;
            }
        }

        private void deleteBTN_Click(object sender, EventArgs e)
        {
            if (previousNotes.SelectedRows.Count > 0)
            {
                int selectedIndex = previousNotes.SelectedRows[0].Index;
                int noteId = Convert.ToInt32(previousNotes.Rows[selectedIndex].Cells["NoteID"].Value);

                try
                {
                    if (VerifyConnection())
                    {
                        string deleteQuery = "DELETE FROM Notes WHERE NoteID = @NoteID";
                        SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection);
                        deleteCommand.Parameters.AddWithValue("@NoteID", noteId);

                        connection.Open();
                        deleteCommand.ExecuteNonQuery();
                        connection.Close();

                        MessageBox.Show("Note deleted successfully!");

                        LoadNotes();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred while deleting the note: " + ex.Message);
                }
                finally
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                }
            }
            else
            {
                MessageBox.Show("Please select a note to delete.");
            }
        }

        private void newBTN_Click(object sender, EventArgs e)
        {
            titleBox.Text = string.Empty;
            notesBox.Text = string.Empty;
        }

        private void saveBTN_Click(object sender, EventArgs e)
        {
            try
            {
                if (VerifyConnection())
                {
                    if (previousNotes.SelectedRows.Count > 0)
                    {
                        int selectedIndex = previousNotes.SelectedRows[0].Index;
                        int noteId = Convert.ToInt32(previousNotes.Rows[selectedIndex].Cells["NoteID"].Value);

                        // Update the title in the DataTable
                        dataTable.Rows[selectedIndex]["Title"] = titleBox.Text;

                        string updateQuery = "UPDATE Notes SET Title = @Title, Content = @Content WHERE NoteID = @NoteID";
                        SqlCommand updateCommand = new SqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@Title", titleBox.Text);
                        updateCommand.Parameters.AddWithValue("@Content", notesBox.Text);
                        updateCommand.Parameters.AddWithValue("@NoteID", noteId);

                        connection.Open();
                        updateCommand.ExecuteNonQuery();
                        connection.Close();

                        MessageBox.Show("Note updated successfully!");
                    }
                    else
                    {
                        string insertQuery = "INSERT INTO Notes (Title, Content) VALUES (@Title, @Content)";
                        SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                        insertCommand.Parameters.AddWithValue("@Title", titleBox.Text);
                        insertCommand.Parameters.AddWithValue("@Content", notesBox.Text);

                        connection.Open();
                        insertCommand.ExecuteNonQuery();
                        connection.Close();

                        MessageBox.Show("Note saved successfully!");
                    }

                    LoadNotes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred while saving the note: " + ex.Message);
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                    connection.Close();
            }
        }
    }
}
