

namespace Cambios
{
    using Cambios.Servicos;
    using Modelos;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class Form1 : Form
    {

        #region Atributos

        private NetworkService networkService;

        private APIService apiService;

        private List<Rate> Rates;

        #endregion

        public Form1()
        {
            InitializeComponent();
            networkService = new NetworkService();
            apiService = new APIService();
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
                return;
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
                return;
            }


            cb_origem.DataSource = Rates;
            cb_origem.DisplayMember = "Name";

            //corrige bug da Microsoft
            cb_destino.BindingContext = new BindingContext();

            cb_destino.DataSource = Rates;
            cb_destino.DisplayMember = "Name";

            btn_convert.Enabled = true;

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

        }

        private void LoadLocalRates()
        {
            throw new NotImplementedException();
        }

        private async Task LoadApiRates()
        {
            progressBar1.Value = 0;

            var response = await apiService.GetRates("http://cambios.somee.com", "/api/rates");

            Rates = (List<Rate>)response.Result;

            progressBar1.Value = 100;
        }
    }
}
