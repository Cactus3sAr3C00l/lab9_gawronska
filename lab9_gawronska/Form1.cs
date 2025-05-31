using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace lab9_gawronska
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Data Source=egzamin_komisyjny.db;Version=3;";

        public Form1()
        {
            InitializeComponent();
            InitializeDatabase();

        }
        private void InitializeDatabase()
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string createTableQuery = @"
                        CREATE TABLE IF NOT EXISTS WnioskiEgzaminKomisyjny (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Album TEXT,
                            NazwiskoImie TEXT,
                            SemestrRok TEXT,
                            WniosekDzien TEXT,
                            KierunekStopienStudiow TEXT,
                            Przedmiot TEXT,
                            Punkty TEXT,
                            Prowadzacy TEXT,
                            Uzasadnienie TEXT,
                            DataPodpis TEXT,
                            Komisja1 TEXT,
                            Komisja2 TEXT,
                            Komisja3 TEXT,
                            DecyzjaDzien TEXT,
                            Podpis TEXT,
                            DataDodania DATETIME DEFAULT CURRENT_TIMESTAMP
                        )";

                    SQLiteCommand command = new SQLiteCommand(createTableQuery, connection);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas inicjalizacji bazy danych: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }


        private void zapisz_do_bazy_button_Click(object sender, EventArgs e)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    string query = @"
                        INSERT INTO WnioskiEgzaminKomisyjny 
                        (Album, NazwiskoImie, SemestrRok, WniosekDzien, KierunekStopienStudiow, 
                         Przedmiot, Punkty, Prowadzacy, Uzasadnienie, DataPodpis, 
                         Komisja1, Komisja2, Komisja3, DecyzjaDzien, Podpis) 
                        VALUES 
                        (@Album, @NazwiskoImie, @SemestrRok, @WniosekDzien, @KierunekStopienStudiow,
                         @Przedmiot, @Punkty, @Prowadzacy, @Uzasadnienie, @DataPodpis,
                         @Komisja1, @Komisja2, @Komisja3, @DecyzjaDzien, @Podpis)";

                    SQLiteCommand command = new SQLiteCommand(query, connection);

                    command.Parameters.AddWithValue("@Album", album_text.Text ?? "");
                    command.Parameters.AddWithValue("@NazwiskoImie", nazwisko_imie_text.Text ?? "");
                    command.Parameters.AddWithValue("@SemestrRok", semestr_rok_text.Text ?? "");
                    command.Parameters.AddWithValue("@WniosekDzien", wniosek_dzien_text.Text ?? "");
                    command.Parameters.AddWithValue("@KierunekStopienStudiow", kierunek_stopien_studiow_text.Text ?? "");
                    command.Parameters.AddWithValue("@Przedmiot", przedmiot_text.Text ?? "");
                    command.Parameters.AddWithValue("@Punkty", punkty_text.Text ?? "");
                    command.Parameters.AddWithValue("@Prowadzacy", prowadzacy_text.Text ?? "");
                    command.Parameters.AddWithValue("@Uzasadnienie", uzasadnienie_text.Text ?? "");
                    command.Parameters.AddWithValue("@DataPodpis", data_podpis_text.Text ?? "");
                    command.Parameters.AddWithValue("@Komisja1", komisja1_text.Text ?? "");
                    command.Parameters.AddWithValue("@Komisja2", komisja2_text.Text ?? "");
                    command.Parameters.AddWithValue("@Komisja3", komisja3_text.Text ?? "");
                    command.Parameters.AddWithValue("@DecyzjaDzien", decyzja_dzien_text.Text ?? "");
                    command.Parameters.AddWithValue("@Podpis", podpis_text.Text ?? "");

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Dane zostały pomyślnie zapisane do bazy danych!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearAllFields();
                    }
                    else
                    {
                        MessageBox.Show("Nie udało się zapisać danych.", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas zapisywania do bazy danych: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

   


        private void zaladuj_z_bazy_button_Click(object sender, EventArgs e)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    string query = "SELECT * FROM WnioskiEgzaminKomisyjny ORDER BY DataDodania DESC";
                    SQLiteCommand command = new SQLiteCommand(query, connection);

                    connection.Open();
                    SQLiteDataReader reader = command.ExecuteReader();

                    List<string> records = new List<string>();

                    while (reader.Read())
                    {
                        string record = $"ID: {reader["Id"]} - {reader["NazwiskoImie"]} - {reader["Przedmiot"]} - {reader["DataDodania"]}";
                        records.Add(record);
                    }
                    reader.Close();

                    if (records.Count == 0)
                    {
                        MessageBox.Show("Brak zapisanych danych w bazie.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    Form selectForm = new Form();
                    selectForm.Text = "Wybierz rekord do załadowania";
                    selectForm.Size = new Size(500, 300);
                    selectForm.StartPosition = FormStartPosition.CenterParent;

                    ListBox listBox = new ListBox();
                    listBox.Dock = DockStyle.Fill;
                    listBox.DataSource = records;

                    Button loadButton = new Button();
                    loadButton.Text = "Załaduj wybrany";
                    loadButton.Dock = DockStyle.Bottom;
                    loadButton.Height = 40;

                    selectForm.Controls.Add(listBox);
                    selectForm.Controls.Add(loadButton);

                    loadButton.Click += (s, ev) =>
                    {
                        if (listBox.SelectedIndex >= 0)
                        {
                            string selectedRecord = records[listBox.SelectedIndex];
                            int id = int.Parse(selectedRecord.Split(':')[1].Split(' ')[1]);
                            LoadRecordById(id);
                            selectForm.Close();
                        }
                        else
                        {
                            MessageBox.Show("Proszę wybrać rekord z listy.", "Uwaga", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    };

                    selectForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas ładowania danych z bazy: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadRecordById(int id)
        {
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    string query = "SELECT * FROM WnioskiEgzaminKomisyjny WHERE Id = @Id";
                    SQLiteCommand command = new SQLiteCommand(query, connection);
                    command.Parameters.AddWithValue("@Id", id);

                    connection.Open();
                    SQLiteDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        album_text.Text = reader["Album"].ToString();
                        nazwisko_imie_text.Text = reader["NazwiskoImie"].ToString();
                        semestr_rok_text.Text = reader["SemestrRok"].ToString();
                        wniosek_dzien_text.Text = reader["WniosekDzien"].ToString();
                        kierunek_stopien_studiow_text.Text = reader["KierunekStopienStudiow"].ToString();
                        przedmiot_text.Text = reader["Przedmiot"].ToString();
                        punkty_text.Text = reader["Punkty"].ToString();
                        prowadzacy_text.Text = reader["Prowadzacy"].ToString();
                        uzasadnienie_text.Text = reader["Uzasadnienie"].ToString();
                        data_podpis_text.Text = reader["DataPodpis"].ToString();
                        komisja1_text.Text = reader["Komisja1"].ToString();
                        komisja2_text.Text = reader["Komisja2"].ToString();
                        komisja3_text.Text = reader["Komisja3"].ToString();
                        decyzja_dzien_text.Text = reader["DecyzjaDzien"].ToString();
                        podpis_text.Text = reader["Podpis"].ToString();

                        MessageBox.Show("Dane zostały załadowane pomyślnie!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd podczas ładowania konkretnego rekordu: " + ex.Message, "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearAllFields()
        {
            album_text.Clear();
            nazwisko_imie_text.Clear();
            semestr_rok_text.Clear();
            wniosek_dzien_text.Clear();
            kierunek_stopien_studiow_text.Clear();
            przedmiot_text.Clear();
            punkty_text.Clear();
            prowadzacy_text.Clear();
            uzasadnienie_text.Clear();
            data_podpis_text.Clear();
            komisja1_text.Clear();
            komisja2_text.Clear();
            komisja3_text.Clear();
            decyzja_dzien_text.Clear();
            podpis_text.Clear();
        }
    }
}
