<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DDT_Interface.aspx.cs" Inherits="ICT.Views.DDT_Interface" %>
<!DOCTYPE html>
<html>
<head runat="server">
    <title>DDT Interface</title>
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
    <form id="form1" runat="server">
        <div class="container mt-3">
            <h2>DDT Interface</h2>
            <!-- Información general -->
            <div class="row mb-3">
                <div class="col-md-6">
                    <label id="LabelTicketCount" runat="server" class="form-label text-success"></label>
                </div>
                <div class="col-md-6 text-end">
                    <asp:Label ID="LabelAlert" runat="server" CssClass="text-primary fw-bold"></asp:Label>
                </div>
            </div>
            <!-- Filtros -->
            <div class="row mb-3">
                <div class="col-md-2">
                    <label for="DropDownArea" class="form-label">Pick Area</label>
                    <asp:DropDownList ID="DropDownArea" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterData">
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label for="DropDownSite" class="form-label">Pick Site</label>
                    <asp:DropDownList ID="DropDownSite" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterData">
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
                    <label for="DropDownStatus" class="form-label">Change Status</label>
                    <asp:DropDownList ID="DropDownStatus" runat="server" CssClass="form-select">
                    </asp:DropDownList>
                </div>
                <div class="col-md-3">
    <label for="TextBoxPriority" class="form-label">Change Priority</label>
    <asp:TextBox ID="TextBoxPriority" runat="server" CssClass="form-control" TextMode="Number" Placeholder="Enter priority (e.g., 1-10)" />
</div>

                 <div class="col-md-2 text-end mt-4">
                    <asp:Button ID="ButtonSaveChanges" runat="server" Text="Save Changes" CssClass="btn btn-success" OnClick="SaveChanges_Click" />
                </div>
            </div>
            <!-- Botón de Refrescar -->
            <div class="text-end mb-3">
                <asp:Button ID="ButtonRefresh" runat="server" Text="Refresh" CssClass="btn btn-primary" OnClick="ButtonRefresh_Click" />
            </div>
            <!-- Tabla de Datos -->
            <asp:GridView ID="GridViewTickets" runat="server" CssClass="table table-bordered" AutoGenerateColumns="False" OnSelectedIndexChanged="GridViewTickets_SelectedIndexChanged">
                <Columns>
                    <asp:BoundField DataField="TICKET" HeaderText="TICKET" />
                    <asp:BoundField DataField="DATE" HeaderText="DATE" />
                    <asp:BoundField DataField="HOUR" HeaderText="HOUR" />
                    <asp:BoundField DataField="Sender" HeaderText="Sender" />
                    <asp:BoundField DataField="PID" HeaderText="PID" />
                    <asp:BoundField DataField="Priority" HeaderText="Priority" />
                    <asp:BoundField DataField="TAG Mins" HeaderText="TAG Mins" />
                    <asp:BoundField DataField="Rework Autor Name" HeaderText="Rework Autor Name" />
                    <asp:BoundField DataField="Autor CELL" HeaderText="Autor CELL" />
                    <asp:BoundField DataField="Autor site" HeaderText="Autor Site" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:BoundField DataField="Inspector user" HeaderText="Inspector user" />
                    <asp:BoundField DataField="Sender Site" HeaderText="Sender Site" />
                    <asp:CommandField ShowSelectButton="True" />
                </Columns>
            </asp:GridView>
        </div>
    </form>
</body>
</html>
