<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Index.aspx.cs" Inherits="ICT.Views.Index" %>
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Index</title>
    <!-- Bootstrap CSS -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body>
    <div class="container mt-4">
        <form id="form1" runat="server">  
            <div class="row">
                <div class="col-md-12 text-center">
                    <h1>Welcome, <asp:Label ID="Label_Name" runat="server" CssClass="fw-bold"></asp:Label></h1>
                    <div class="alert alert-info">
                        Role: <asp:Label ID="Label_Title" runat="server" CssClass="fw-bold"></asp:Label> |
                        Badge: <asp:Label ID="Label_Badge" runat="server" CssClass="fw-bold"></asp:Label> |
                        Supervisor: <asp:Label ID="Label_Supervisor" runat="server" CssClass="fw-bold"></asp:Label>
                        Site: <asp:Label ID="Label_Site" runat="server" CssClass="fw-bold"></asp:Label>
                    </div>
                </div>
            </div>

            <div class="row mt-3">
                <div class="col-md-6">
                    <h3>Assigned Cells</h3>
                    <asp:DropDownList ID="DropDownListCells" CssClass="form-select" runat="server"></asp:DropDownList>
                </div>
            </div>
            <div class="col-md-3 text-end">
                    <asp:Label ID="LabelAlert" runat="server" CssClass="text-primary fw-bold"></asp:Label>
                </div>
            <div class="row mt-4">
                <div class="col-md-12">
                    <asp:GridView ID="SSRawData" runat="server" CssClass="table table-bordered" AutoGenerateColumns="False"  AutoGenerateSelectButton="True" OnSelectedIndexChanged="SSRawData_SelectedIndexChanged">
                <Columns>
                    <asp:BoundField DataField="TICKET" HeaderText="TICKET" />
                    <asp:BoundField DataField="PID" HeaderText="PID" />
                    <asp:BoundField DataField="DATE" HeaderText="DATE" />
                    <asp:BoundField DataField="Send by user" HeaderText="Send by user" />
                    <asp:BoundField DataField="Rework Autor User" HeaderText="Rework Autor User" />
                    <asp:BoundField DataField="Autor CELL" HeaderText="Autor CELL" />
                    <asp:BoundField DataField="Status" HeaderText="Status" />
                    <asp:BoundField DataField="Inspector Name" HeaderText="Inspector Name" />
                    
                </Columns>
            </asp:GridView>
                </div>
            </div>

            <div class="row mt-4">
                <div class="col-md-12 text-center">
                    <asp:Button ID="Button_Refresh" Text="Refresh" CssClass="btn btn-primary" runat="server" OnClick="Button_Refresh_Click" />
                    <asp:Button ID="Button_AddInspection" Text="Add Inspection" CssClass="btn btn-success" runat="server" OnClick="Button_AddInspection_Click" />
                    <asp:Button ID="ButtonSend" Text="Send Correction" CssClass="btn btn-warning" runat="server" OnClick="ButtonSend_Click" />
                    <asp:Button ID="ButtonDeleteInspection" Text="Delete" CssClass="btn btn-danger" runat="server" OnClick="ButtonDeleteInspection_Click" />

                    <div id="InternalCorrections" runat="server" style="display: none;" class="card mt-4 shadow-sm">
    <div class="card-header bg-primary text-white text-center">
        <h3 class="mb-0">Add Internal Corrections</h3>
    </div>
    <div class="card-body">
        <div class="row">
            <!-- Authors Dropdown -->
            <div class="col-md-6 mb-3">
                <label class="form-label fw-bold d-block">Authors</label>
                <asp:DropDownList ID="DropDownAuthors" CssClass="form-select" runat="server"></asp:DropDownList>
            </div>
            <!-- PID Input -->
            <div class="col-md-6 mb-3">
                <label for="TextBoxPID" class="form-label fw-bold d-block">PID:</label>
                <asp:TextBox ID="TextBoxPID" CssClass="form-control" runat="server"></asp:TextBox>
            </div>
        </div>

        <div class="row">
            <!-- Source Selection -->
            <div class="col-md-6 mb-3">
                <label class="form-label fw-bold d-block">Source:</label>
                <div class="d-flex align-items-center gap-2">
                    <asp:CheckBox ID="CheckBoxACS" runat="server" CssClass="form-check-input" /><span>ACS</span>
                    <asp:CheckBox ID="CheckBoxQS" runat="server" CssClass="form-check-input" /><span>QS</span>
                </div>
            </div>

            <!-- Priority Selection -->
            <div class="col-md-6 mb-3">
                <label class="form-label fw-bold d-block">Priority:</label>
                <div class="d-flex align-items-center gap-2 flex-wrap">
                    <asp:CheckBox ID="CheckBoxNone" runat="server" CssClass="form-check-input" /><span>None</span>
                    <asp:CheckBox ID="CheckBoxExpedite" runat="server" CssClass="form-check-input" /><span>Expedite</span>
                    <asp:CheckBox ID="CheckBoxLate" runat="server" CssClass="form-check-input" /><span>Late</span>
                    <asp:CheckBox ID="CheckBoxLeadTime" runat="server" CssClass="form-check-input" /><span>Lead Time</span>
                </div>
            </div>
        </div>

        <div class="row">
            <!-- Modification Selection -->
            <div class="col-md-6 mb-3">
                <label class="form-label fw-bold d-block">Modification:</label>
                <div class="d-flex align-items-center gap-2">
                    <asp:CheckBox ID="CheckBoxCut" runat="server" CssClass="form-check-input" /><span>CUT</span>
                    <asp:CheckBox ID="CheckBoxUsTreat" runat="server" CssClass="form-check-input" /><span>UsTreat</span>
                </div>
            </div>

            <!-- Instructions Input -->
            <div class="col-md-6 mb-3">
                <label for="TextBoxInstructions" class="form-label fw-bold d-block">Instructions:</label>
                <asp:TextBox ID="TextBoxInstructions" CssClass="form-control" runat="server" TextMode="MultiLine" Rows="4" placeholder="Enter detailed instructions..."></asp:TextBox>
            </div>
        </div>
    </div>
</div>


                </div>
            </div>
        </form> 
    </div>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0-alpha1/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>
