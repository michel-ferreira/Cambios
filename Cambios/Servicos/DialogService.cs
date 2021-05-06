
namespace Cambios.Servicos
{
    using System.Windows.Forms;
    class DialogService
    {
        public void ShowMessage(string title, string message)
        {
            MessageBox.Show(message, title);
        }
    }
}
