using System;
using System.Collections.Generic;
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
using Vet_Master.Models;

namespace Vet_Master.Dialogs;

public partial class VaccinationDialog : Window
{
    public Vaccination? Result { get; private set; }

    public VaccinationDialog(IEnumerable<string> vaccines)
    {
        InitializeComponent();
        DpDate.SelectedDate = DateTime.Today;
        CmbVaccine.ItemsSource = vaccines;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CmbVaccine.Text))
        {
            MessageBox.Show("Введіть або оберіть назву вакцини.", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (DpDate.SelectedDate is null)
        {
            MessageBox.Show("Вкажіть дату щеплення.", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Result = new Vaccination
        {
            VaccineName = CmbVaccine.Text.Trim(),
            VaccinationDate = DpDate.SelectedDate.Value
        };
        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}