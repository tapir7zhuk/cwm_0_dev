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

public partial class RecordDialog : Window
{
    public Record? Result { get; private set; }
    private readonly Record? _existing;

    public RecordDialog()
    {
        InitializeComponent();
        DpVisitDate.SelectedDate = DateTime.Today;
    }

    public RecordDialog(Record record) : this()
    {
        _existing = record;
        DialogTitle.Text = "Редагувати запис";
        DpVisitDate.SelectedDate = record.VisitDate;
        TxtComplaints.Text = record.Complaints;
        TxtDiagnosis.Text = record.Diagnosis;
        TxtRecommendations.Text = record.Recommendations;
        DpTreatmentStart.SelectedDate = record.TreatmentStart;
        DpTreatmentEnd.SelectedDate = record.TreatmentEnd;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtDiagnosis.Text))
        {
            MessageBox.Show("Введіть діагноз.", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtDiagnosis.Focus();
            return;
        }
        if (DpVisitDate.SelectedDate is null)
        {
            MessageBox.Show("Вкажіть дату прийому.", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Result = _existing ?? new Record();
        Result.VisitDate = DpVisitDate.SelectedDate.Value;
        Result.Complaints = TxtComplaints.Text.Trim();
        Result.Diagnosis = TxtDiagnosis.Text.Trim();
        Result.Recommendations = TxtRecommendations.Text.Trim();
        Result.TreatmentStart = DpTreatmentStart.SelectedDate;
        Result.TreatmentEnd = DpTreatmentEnd.SelectedDate;

        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}