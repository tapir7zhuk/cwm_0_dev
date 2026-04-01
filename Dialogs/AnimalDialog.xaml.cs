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
using Microsoft.Win32;
using Vet_Master.Models;

namespace Vet_Master.Dialogs;

public partial class AnimalDialog : Window
{
    public AnimalCard? Result { get; private set; }
    private readonly AnimalCard? _existing;

    // Додати нову
public AnimalDialog(IEnumerable<string> species)
{
    InitializeComponent();
    CmbSpecies.ItemsSource = species;
}

// Редагувати існуючу
public AnimalDialog(AnimalCard animal, IEnumerable<string> species) : this(species)
{
    _existing = animal;
    DialogTitle.Text = "Редагувати тварину";
    TxtName.Text = animal.Name;
    CmbSpecies.Text = animal.Species;
    TxtBreed.Text = animal.Breed;
    TxtAge.Text = animal.Age.ToString();
    CmbSex.Text = animal.Sex;
    TxtOwner.Text = animal.OwnerName;
    TxtPhone.Text = animal.OwnerPhone;
    TxtMedia.Text = animal.MediaPath;
}

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Title = "Оберіть медіа файл",
            Filter = "Зображення|*.jpg;*.jpeg;*.png;*.bmp;*.gif|Документи|*.pdf;*.docx|Всі файли|*.*"
        };
        if (dlg.ShowDialog() == true)
            TxtMedia.Text = dlg.FileName;
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
        // Валідація
        if (string.IsNullOrWhiteSpace(TxtName.Text))
        {
            MessageBox.Show("Введіть ім'я тварини.", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtName.Focus();
            return;
        }
        if (string.IsNullOrWhiteSpace(CmbSpecies.Text))
        {
            MessageBox.Show("Оберіть або введіть вид тварини.", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(TxtAge.Text, out var age) || age < 0 || age > 100)
        {
            MessageBox.Show("Вік має бути числом від 0 до 100.", "Помилка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            TxtAge.Focus();
            return;
        }

        Result = _existing ?? new AnimalCard();
        Result.Name = TxtName.Text.Trim();
        Result.Species = CmbSpecies.Text.Trim();
        Result.Breed = TxtBreed.Text.Trim();
        Result.Age = age;
        Result.Sex = CmbSex.Text.Trim();
        Result.OwnerName = TxtOwner.Text.Trim();
        Result.OwnerPhone = TxtPhone.Text.Trim();
        Result.MediaPath = TxtMedia.Text.Trim();

        DialogResult = true;
    }

    private void BtnCancel_Click(object sender, RoutedEventArgs e)
        => DialogResult = false;
}