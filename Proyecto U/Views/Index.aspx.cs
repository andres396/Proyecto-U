using System;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ICT.Controllers;
using ICT.Models;

namespace ICT.Views
{
    public partial class Index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    // Carga inicial del usuario
                    Label_Name.Text = (HttpContext.Current.User.Identity.Name.ToString()).Substring((HttpContext.Current.User.Identity.Name.ToString()).IndexOf(@"\") + 1);

                    if (UserData.Is_Valid_User(Label_Name.Text))
                    {
                        // Datos de usuario
                        var userInfo = UserData.Get_User_Information(Label_Name.Text);
                        Label_Title.Text = userInfo[1];
                        Label_Badge.Text = userInfo[2];
                        Label_Supervisor.Text = userInfo[3];
                        Label_Name.Text = userInfo[4];
                        Label_Site.Text = userInfo[5];
                        //Label_Area.Text = userInfo[6];

                        // Carga inicial
                        CargarCeldas();
                        CargarDatosIniciales();
                    }
                    else
                    {
                        MostrarMensaje("Sorry, you do not have access to the app.");
                    }
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje($"Error: {ex.Message}");
            }
        }


        /// <summary>
        /// Carga las celdas asignadas al supervisor actual.
        /// </summary>
        private void CargarCeldas()
        {
            DropDownListCells.DataSource = UserData.Cells(Label_Supervisor.Text);
            DropDownListCells.DataTextField = "CELL";
            DropDownListCells.DataValueField = "REGION";
            DropDownListCells.DataBind();
        }

        /// <summary>
        /// Carga los datos iniciales de la interfaz.
        /// </summary>
        private void CargarDatosIniciales()
        {
            // Cargar tabla de datos
            SSControl.Load_RawData(DropDownListCells.SelectedItem.Text);

            // Crear una variable temporal para almacenar los datos filtrados
            DataTable rawData = PRDCR1PEAPPS01Conections.rawdata;

            // Tomar solo los primeros 10 registros si hay más de 10
            if (rawData != null && rawData.Rows.Count > 10)
            {
                rawData = rawData.AsEnumerable().Take(10).CopyToDataTable();
            }

            // Asignar los datos filtrados al GridView
            SSRawData.DataSource = rawData;
            SSRawData.DataBind();

            // Cargar autores con caché
            var authorsCacheKey = "AuthorsData";
            var authorsData = Cache[authorsCacheKey] as System.Data.DataTable;
            if (authorsData == null)
            {
                authorsData = AuthorsData.Load_LoginAuthors();
                Cache.Insert(authorsCacheKey, authorsData, null, DateTime.Now.AddHours(1), TimeSpan.Zero);
            }

            DropDownAuthors.DataSource = authorsData;
            DropDownAuthors.DataTextField = "MES_CADCAM_LOGIN";
            DropDownAuthors.DataBind();
            //add variants of users
            DropDownAuthors.Items.Add("Old CCMOD AMERICAS");
            DropDownAuthors.Items.Add("Old CCMOD EMEA");
            DropDownAuthors.Items.Add("Old CCMOD APAC");
            DropDownAuthors.Items.Add("Auto DDT AMERICAS");
            DropDownAuthors.Items.Add("Auto DDT EMEA");
            DropDownAuthors.Items.Add("Auto DDT APAC");
            DropDownAuthors.Items.Add("I couldnt find the MES user to AMERICAS");
            DropDownAuthors.Items.Add("I couldnt find the MES user to EMEA");
            DropDownAuthors.Items.Add("I couldnt find the MES user to APAC");

            LabelAlert.Text = "";
        }



        /// <summary>
        /// Muestra un mensaje al usuario.
        /// </summary>
        /// <param name="mensaje">Texto del mensaje.</param>
        private void MostrarMensaje(string mensaje)
        {
            Page.ClientScript.RegisterStartupScript(GetType(), "alert", $"alert('{mensaje}');", true);
        }

        protected void Button_Refresh_Click(object sender, EventArgs e)
        {
            CargarDatosIniciales();
        }

        protected void Button_AddInspection_Click(object sender, EventArgs e)
        {
            InternalCorrections.Visible = true;

            // Forzar la actualización con JavaScript para mejorar la UX
            ScriptManager.RegisterStartupScript(this, GetType(), "ShowInternalCorrections", "document.getElementById('InternalCorrections').style.display = 'block';", true);
        }

        protected void ButtonSend_Click(object sender, EventArgs e)
        {
            // Variables necesarias
            string Source;
            int Priority;
            string Modification;
            string CaseTYpe;

            // Validación de datos antes de enviar
            if (string.IsNullOrWhiteSpace(TextBoxPID.Text))
            {
                LabelAlert.Text = ("Please copy or paste a PID before continuing.");
                return;
            }

            if (!CheckBoxACS.Checked && !CheckBoxQS.Checked)
            {
                LabelAlert.Text = ("Please select a source.");
                return;
            }

            if (!CheckBoxNone.Checked && !CheckBoxExpedite.Checked && !CheckBoxLate.Checked && !CheckBoxLeadTime.Checked)
            {
                LabelAlert.Text = ("Please select the priority.");
                return;
            }

            if (!CheckBoxCut.Checked && !CheckBoxUsTreat.Checked)
            {
                LabelAlert.Text = ("Please select where the modification is needed.");
                return;
            }

            if (string.IsNullOrWhiteSpace(TextBoxInstructions.Text))
            {
                LabelAlert.Text = ("Please provide more information to help with this internal correction.");
                return;
            }

            // Procesar los datos de entrada
            Source = CheckBoxACS.Checked ? "ACS" : "QS";
            Priority = CheckBoxLate.Checked ? 90 :
                       CheckBoxExpedite.Checked ? 80 :
                       CheckBoxLeadTime.Checked ? 70 : 0;
            CaseTYpe = CheckBoxLate.Checked ? "Late" :
                       CheckBoxExpedite.Checked ? "Expedite" :
                       CheckBoxLeadTime.Checked ? "Lead Time" : "None";
            Modification = CheckBoxCut.Checked ? "CUT" : "UsTreat";

            // Enviar los datos
            try
            {
                SSControl.Send_IC(
                    int.Parse(TextBoxPID.Text),
                    DropDownAuthors.SelectedItem.Text,
                    CheckBoxACS.Checked ? "ACS" : "QS",
                    CheckBoxLate.Checked ? 90 : (CheckBoxExpedite.Checked ? 80 : (CheckBoxLeadTime.Checked ? 70 : 0)),
                    CheckBoxCut.Checked ? "CUT" : "UsTreat",
                    TextBoxInstructions.Text.Replace("<", "").Replace(">", "").Replace("'", ""),
                    Label_Name.Text,
                    DropDownListCells.SelectedItem.Text,
                    Label_Site.Text,
                    DropDownListCells.SelectedValue,
                    Label_Supervisor.Text,
                    Label_Name.Text,
                    "IC"
                );
                LabelAlert.Text = ("Your data was sent to inspection.");
            }
            catch (Exception ex)
            {
                LabelAlert.Text = ($"Error sending data: {ex.Message}");
            }
            LimpiarFormulario();
        }

        protected void ButtonDeleteInspection_Click(object sender, EventArgs e)
        {
            try
            {
                // Verificar si hay un ticket seleccionado
                if (Session["SelectedTicket"] == null)
                {
                    LabelAlert.Text = ("Please select a row before deleting.");
                    return;
                }

                string selectedTicket = Session["SelectedTicket"].ToString();

                // Eliminar el ticket en la base de datos
                PRDCR1PEAPPS01Conections.Execute_Request($@"
            DELETE FROM [DB_RMT].[dbo].[Reworks Report Phase II]
            WHERE Ticket = '{selectedTicket}'");

                // Recargar la tabla después de la eliminación
                SSControl.Load_RawData(DropDownListCells.SelectedItem.Text);
                SSRawData.DataSource = PRDCR1PEAPPS01Conections.rawdata;
                SSRawData.DataBind();

                // Eliminar la sesión del ticket seleccionado
                Session["SelectedTicket"] = null;

                LabelAlert.Text = ("Inspection successfully deleted.");
            }
            catch (Exception ex)
            {
                LabelAlert.Text = ($"Error deleting inspection: {ex.Message}");
            }
            CargarDatosIniciales();
        }


        protected void SSRawData_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Obtener el índice de la fila seleccionada
                int selectedIndex = SSRawData.SelectedIndex;

                // Obtener el valor del TICKET desde la primera celda
                string selectedTicket = SSRawData.SelectedRow.Cells[1].Text;

                // Guardar el ticket seleccionado en una variable de sesión para usarlo en la eliminación
                Session["SelectedTicket"] = selectedTicket;

                LabelAlert.Text = ($"Selected Ticket: {selectedTicket}");
            }
            catch (Exception ex)
            {
                LabelAlert.Text = ("Error selecting ticket: " + ex.Message);
            }
        }



        /// <summary>
        /// Limpia los campos del formulario de corrección interna.
        /// </summary>
        private void LimpiarFormulario()
        {
            TextBoxPID.Text = "";
            TextBoxInstructions.Text = "";
            CheckBoxACS.Checked = false;
            CheckBoxQS.Checked = false;
            CheckBoxNone.Checked = true;
            CheckBoxExpedite.Checked = false;
            CheckBoxLate.Checked = false;
            CheckBoxCut.Checked = false;
            CheckBoxUsTreat.Checked = false;
            CheckBoxLeadTime.Checked = false;
        }
    }
}
