using ICT.Controllers;
using ICT.Models;
using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ICT.Views
{
    public partial class Inspection_Interface : System.Web.UI.Page
    {
            protected Label LabelCountDDT;
            protected Label LabelCounttome;
            protected DropDownList DropDownListDRMod;
            protected DropDownList DropDownListIssueCategory;
            protected DropDownList DropDownListIssueSubCategory;
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    Label_Name.Text = (HttpContext.Current.User.Identity.Name.ToString()).Substring((HttpContext.Current.User.Identity.Name.ToString()).IndexOf(@"\") + 1);

                    // Verifica si el usuario es válido
                    if (UserData.Is_Valid_User(Label_Name.Text))
                    {
                        var userInfo = UserData.Get_User_Information(Label_Name.Text);
                        Label_Title.Text = userInfo[1];
                        Label_Badge.Text = userInfo[2];
                        Label_Supervisor.Text = userInfo[3];
                        Label_Name.Text = userInfo[4];
                        Label_Site.Text = userInfo[5];
                        Label_Shift.Text = userInfo[6];
                        Label_Cell.Text = userInfo[7];

                        countreatments();
                    }
                    else
                    {
                        countreatments();
                    }

                    // Ocultar la alerta de error al cargar la página
                    Label1.Visible = false;
                    InitializeFilters();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }


        protected void FilterData(object sender, EventArgs e)
        {
            ClearWindow();
        }

        private void InitializeFilters()
        {
            // Inicializar filtros de áreas
            DropDownListArea.Items.Clear();
            DropDownListArea.Items.Add(new ListItem("Americas", "Americas"));
            DropDownListArea.Items.Add(new ListItem("Celdas Especiales", "Celdas Especiales"));
            DropDownListArea.Items.Add(new ListItem("EMEA", "EMEA"));
            DropDownListArea.Items.Add(new ListItem("APAC", "APAC"));
            DropDownListArea.Items.Add(new ListItem("Training", "Training"));
            DropDownListArea.Items.Add(new ListItem("General Pool", "General Pool"));

            // Inicializar filtros de sitios
            DropDownListSite.Items.Clear();
            DropDownListSite.Items.Add(new ListItem("CR-San Jose (BV)", "CR-San Jose (BV)"));
            DropDownListSite.Items.Add(new ListItem("MX1-Juarez-Salvarcar (BV)", "MX1-Juarez-Salvarcar (BV)"));
            DropDownListSite.Items.Add(new ListItem("MX2-Juarez-Salvarcar (BV)", "MX2-Juarez-Salvarcar (BV)"));
        } //ButtonPick_Click

        protected void ButtonPick_Click(object sender, EventArgs e)
        {
            //check if have treatments to pick
            countreatments();
            //assign the treatmet previous assigned
            if (LabelCounttome.Text != "0")
            {
                //load the data
                InspectionControl.Load_Treatment(DropDownListSite.Text, Label_Name.Text, Label_Name.Text, Label_Shift.Text, Label_Site.Text, Label_Cell.Text);
                TreatmentPick();
                countreatments();
            }
            else
            {
                //assign a new treatmento to pick
                if (LabelCountDDT.Text != "0")
                {
                    InspectionControl.Assign_Treatment(DropDownListArea.Text, DropDownListSite.Text, Label_Name.Text, Label_Name.Text, Label_Shift.Text, Label_Site.Text, Label_Cell.Text);
                    TreatmentPick();
                    countreatments();
                }
                else // no have treatments to pick
                {
                    Page.ClientScript.RegisterStartupScript(this.GetType(), "Blank Data", "alert('No treatments to Pick');", true);
                }
            }


        }

        protected void TreatmentPick()
        {
            LabelTicket.Text = PRDCR1PEAPPS01Conections.Ticket.ToString();
            LabelPID.Text = PRDCR1PEAPPS01Conections.PID.ToString();
            LabelSource.Text = PRDCR1PEAPPS01Conections.Source;
            LabelModification.Text = PRDCR1PEAPPS01Conections.Modification;
            LabelAuthor.Text = PRDCR1PEAPPS01Conections.AuthorUser;
            LabelDetail.Text = PRDCR1PEAPPS01Conections.Detail;
            //Label_sender.Text = PRDCR1PEAPPS01Conections.Sender;

            // Cambiar el color del LabelPID según la prioridad
            int priority = PRDCR1PEAPPS01Conections.Priority;
            if (priority >= 90) LabelPID.ForeColor = Color.Firebrick;
            else if (priority >= 80) LabelPID.ForeColor = Color.SkyBlue;
            else if (priority >= 70) LabelPID.ForeColor = Color.Coral;
            else LabelPID.ForeColor = Color.Black;

            // Mostrar el menú de finalización
            //EndSubMenu.Visible = true;
        }

        protected void countreatments()
        {
            string area = DropDownListArea.SelectedValue;
            DataTable dt = DDTControl.Load_RawData(area);
            LabelCountDDT.Text = dt.Rows.Count.ToString();
                
            try
            {
                int countAssigned = InspectionControl.CountAssignedtome(Label_Name.Text, Label_Name.Text);
                if (LabelCounttome != null)
                {
                    LabelCounttome.Text = countAssigned.ToString();
                }
            }
            catch (Exception ex)
            {
                if (LabelCounttome != null)
                {
                    LabelCounttome.Text = "0";
                }
                System.Diagnostics.Debug.WriteLine("Error en countreatments: " + ex.Message);
            }
        }


        protected void DropDownListNotApplies_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                InspectionControl.EndInspection(int.Parse(LabelTicket.Text), "Done", "Not Applies", "N/A", "N/A");
                ClearWindow();
                ResetDropDowns(); // Limpiar los dropdowns
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void DropDownListIssueCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                InspectionControl.EndInspection(int.Parse(LabelTicket.Text), "Done", "Not Applies", "N/A", "N/A");
                ClearWindow();
                ResetDropDowns(); // Limpiar los dropdowns
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void DropDownListDRMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (DropDownListDRMod.SelectedValue == "")
                {
                    ShowAlert("Please select a valid Doctor Modification option.");
                }
                else
                {
                    InspectionControl.EndInspection(int.Parse(LabelTicket.Text), "Done", "Dr. Modification", DropDownListDRMod.SelectedValue, "N/A");
                    ClearWindow();
                    ResetDropDowns(); // Limpiar los dropdowns
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        protected void DropDownSSModification_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (DropDownSSModification.SelectedValue == "")
                {
                    ShowAlert("Please select a valid SS Modification option.");
                }
                else
                {
                    InspectionControl.EndInspection(int.Parse(LabelTicket.Text), "Done", "SS Modification", DropDownSSModification.SelectedValue, "N/A");
                    ClearWindow();
                    ResetDropDowns(); // Limpiar los dropdowns
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        private void ResetDropDowns()
        {
            DropDownListDRMod.SelectedIndex = 0;  // Regresar a la primera opción
            DropDownListIssueCategory.SelectedIndex = 0;
            DropDownListNotApplies.SelectedIndex = 0;
            DropDownSSModification.SelectedIndex = 0;
        }


        protected void ClearWindow()
        {
            ButtonPick.Visible = true;
            LabelTicket.Text = "N/A";
            LabelPID.Text = "N/A";
            LabelSource.Text = "N/A";
            LabelModification.Text = "N/A";
            LabelAuthor.Text = "N/A";
            LabelDetail.Text = "N/A";
            //DropDownListDRMod.SelectedValue = "Select One";
            //DropDownListIssueCategory.SelectedValue = "Select One";
            //DropDownList1.SelectedValue = "Select One";
            //DropDownListNotApplies.SelectedValue = "Select One";
            //EndSubMenu.Visible = false;
            countreatments();

            //ShowAlert("The inspection was made.");
        }

        protected void ShowAlert(string message)
        {
            Page.ClientScript.RegisterStartupScript(this.GetType(), "AlertMessage", $"alert('{message}');", true);
        }

        protected void ShowError(string message)
        {
            Label1.Text = message;
            Label1.Visible = true;
        }
    }
}
