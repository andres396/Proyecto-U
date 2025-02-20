using ICT.Controllers;
using System;
using System.Data;
using System.Web.UI.WebControls;

namespace ICT.Views
{
    public partial class DDT_Interface : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                InitializeFilters();
                LoadGridData();
            }
        }

        private void InitializeFilters()
        {
            // Inicializar filtros de áreas
            DropDownArea.Items.Clear();
            DropDownArea.Items.Add(new ListItem("Americas", "Americas"));
            DropDownArea.Items.Add(new ListItem("Celdas Especiales", "Celdas Especiales"));
            DropDownArea.Items.Add(new ListItem("EMEA", "EMEA"));
            DropDownArea.Items.Add(new ListItem("APAC", "APAC"));
            DropDownArea.Items.Add(new ListItem("Training", "Training"));
            DropDownArea.Items.Add(new ListItem("General Pool", "General Pool"));

            // Inicializar filtros de sitios
            DropDownSite.Items.Clear();
            DropDownSite.Items.Add(new ListItem("CR-San Jose (BV)", "CR-San Jose (BV)"));
            DropDownSite.Items.Add(new ListItem("MX1-Juarez-Salvarcar (BV)", "MX1-Juarez-Salvarcar (BV)"));
            DropDownSite.Items.Add(new ListItem("MX2-Juarez-Salvarcar (BV)", "MX2-Juarez-Salvarcar (BV)"));

            // Inicializar lista de usuarios
            DropDownStatus.Items.Clear();
            DropDownStatus.Items.Add(new ListItem("-- Status --", ""));
            DropDownStatus.Items.Add(new ListItem("Done"));
        }

        private void LoadGridData()
        {
            string area = DropDownArea.SelectedValue;

            DataTable dt = DDTControl.Load_RawData(area);
            LabelTicketCount.InnerText = $"Pending Tickets: {dt.Rows.Count}";

            if (dt.Rows.Count > 0)
            {
                GridViewTickets.DataSource = dt;
                GridViewTickets.DataBind();
            }
            else
            {
                GridViewTickets.DataSource = null;
                GridViewTickets.DataBind();
            }
        }

        protected void FilterData(object sender, EventArgs e)
        {
            LoadGridData();
        }

        protected void ButtonRefresh_Click(object sender, EventArgs e)
        {
            LoadGridData();
            LabelAlert.Text = "Data refreshed successfully.";
        }

        protected void SaveChanges_Click(object sender, EventArgs e)
        {
            // Obtener el número de ticket desde el LabelAlert
            GridViewRow selectedRow = GridViewTickets.SelectedRow;
            string _ticket = selectedRow.Cells[0].Text;
            string ticketText = _ticket;

            // Obtener los valores ingresados
            string Status = DropDownStatus.SelectedValue; // status asignado
            string priorityText = TextBoxPriority.Text; // Nueva prioridad

            // Validar que el ticket sea un número válido
            if (!string.IsNullOrEmpty(ticketText) && int.TryParse(ticketText, out int ticket))
            {
                // Si se ingresó una prioridad válida, actualizamos la prioridad
                if (!string.IsNullOrEmpty(priorityText) && int.TryParse(priorityText, out int priority))
                {
                    DDTControl.UpdatePriority(priority, ticket);
                }

                //Si se seleccionó un usuario válido, actualizamos el usuario asignado
                if (!string.IsNullOrEmpty(Status))
                {
                    DDTControl.updateInspector(Status, ticket);
                }

                // Mostrar mensaje de confirmación
                LabelAlert.Text = "Changes saved successfully.";
                LoadGridData();
            }
            else
            {
                // Si el ticket no es válido, mostramos un mensaje de error
                LabelAlert.Text = "Please provide a valid ticket ID.";
            }
        }

        protected void GridViewTickets_SelectedIndexChanged(object sender, EventArgs e)
        {
            GridViewRow selectedRow = GridViewTickets.SelectedRow;
            string ticket = selectedRow.Cells[0].Text; // ID del Ticket seleccionado
            LabelAlert.Text = $"Selected Ticket: {ticket}";
        }
    }
}
