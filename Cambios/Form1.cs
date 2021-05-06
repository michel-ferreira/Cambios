namespace Cambios
{
    using Cambios.Servicos;
    using Modelos;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {


        #region Atributos

        private NetworkService networkService;

        private APIService apiService;

        private List<Rate> Rates;

        private DialogService dialogservice;

        private DataService dataService;

        #endregion

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new APIService();
            dialogservice = new DialogService();
            dataService = new DataService();
            LoadRates();
        }

        private async void LoadRates()
        {
            bool load;

            lbl_resultado.Text = "A atualizar taxas...";

            var connection = networkService.CheckConnection();

            if (!connection.IsSuccess)
            {
                LoadLocalRates();
                load = false;
            }
            else
            {
                await LoadApiRates();
                load = true;
            }

            if(Rates.Count == 0)
            {
                lbl_resultado.Text = "Não há ligação à Internet" + Environment.NewLine +
                    "e não foram previamente carregadas as taxas." + Environment.NewLine +
                    "Por favor tente mais tarde";
                

                lbl_status.Text = "Primeira inicialização deverá ter à internet";

                return;
            }


            cb_origem.DataSource = Rates;
            cb_origem.DisplayMember = "Name";

            //corrige bug da Microsoft
            cb_destino.BindingContext = new BindingContext();

            cb_destino.DataSource = Rates; 
            cb_destino.DisplayMember = "Name";

            lbl_resultado.Text = "Taxas atualizadas...";

            if (load)
            {
                lbl_status.Text = string.Format("Taxas carregadas da internet em {0:F}", DateTime.Now);
            }
            else
            {
                lbl_status.Text = string.Format("Taxas carregadas da Base da Dados");
            }

            progressBar1.Value = 100;

            btn_convert.Enabled = true;
            btn_troca.Enabled = true;

        }

        private void LoadLocalRates()
        {
            Rates = dataService.GetData();
        }

        private async Task LoadApiRates()
        {
            progressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com", "/api/rates");

            Rates = (List<Rate>)response.Result;

            dataService.DeleteData();

            dataService.SaveData(Rates);

        }

        private void btn_convert_Click(object sender, EventArgs e)
        {
            Converter();
        }

        private void Converter()
        {
            if (string.IsNullOrEmpty(tb_valor.Text))
            {
                dialogservice.ShowMessage("Erro", "Insira um valor a converter");
                return;
            }

            decimal valor;
            if(!decimal.TryParse(tb_valor.Text, out valor))
            {
                dialogservice.ShowMessage("Erro de Conversão", "Valor terá que ser numérico");
                return;
            }

            if(cb_origem.SelectedItem == null)
            {
                dialogservice.ShowMessage("Erro", "Tem que selecionar uma moeda a converter");
                return;
            }

            if (cb_destino.SelectedItem == null)
            {
                dialogservice.ShowMessage("Erro", "Tem que selecionar uma moeda de destino");
                return;
            }

            var taxaOrigem = (Rate)cb_origem.SelectedItem;

            var taxaDestino = (Rate)cb_destino.SelectedItem;

            var valorConvertido = valor / (decimal)taxaOrigem.TaxRate * (decimal)taxaDestino.TaxRate;

            lbl_resultado.Text = string.Format("{0} {1:C2} = {2} {3:C2}", taxaOrigem.Code, valor, taxaDestino.Code, valorConvertido);
        }

        private void btn_troca_Click(object sender, EventArgs e)
        {
            Troca();
        }

        private void Troca()
        {
            var aux = cb_origem.SelectedItem;
            cb_origem.SelectedItem = cb_destino.SelectedItem;
            cb_destino.SelectedItem = aux;
            Converter();
        }
    }
}
