using System.Windows;

namespace bdmanager.Views {
  public partial class RenameWindow : Window {
    public string Value => NameTextBox.Text;

    public RenameWindow(string currentValue) {
      InitializeComponent();
      DarkTitleBar.Apply(this);
      NameTextBox.Text = currentValue ?? string.Empty;
      Loaded += (sender, args) => {
        NameTextBox.Focus();
        NameTextBox.SelectAll();
      };
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) {
      DialogResult = true;
    }
  }
}
