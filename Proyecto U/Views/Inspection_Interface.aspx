<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Inspection_Interface.aspx.cs" Inherits="ICT.Views.Inspection_Interface" %>

<!DOCTYPE html>
<html lang="es">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Inspection Interface</title>
    <!-- Bootstrap 5 -->
    <link href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css" rel="stylesheet">
</head>
<body class="bg-light">

    <form id="form1" runat="server">
        <div class="container mt-5">
            <div class="card shadow-lg">
                <div class="card-header bg-primary text-white text-center">
                    <h3>Inspection Interface</h3>
                </div>
                <div class="card-body">

                    <!-- User Info Section -->
                    <div class="bg-light p-3 rounded mb-4 shadow-sm">
                        <h1 class="text-center">Welcome, <asp:Label ID="Label_Name" runat="server" CssClass="fw-bold"></asp:Label></h1>
                        <div class="alert alert-info text-center">
                            <strong>Role:</strong> <asp:Label ID="Label_Title" runat="server" CssClass="fw-bold"></asp:Label> |
                            <strong>Badge:</strong> <asp:Label ID="Label_Badge" runat="server" CssClass="fw-bold"></asp:Label> |
                            <strong>Supervisor:</strong> <asp:Label ID="Label_Supervisor" runat="server" CssClass="fw-bold"></asp:Label> |
                            <strong>Site:</strong> <asp:Label ID="Label_Site" runat="server" CssClass="fw-bold"></asp:Label> |
                            <strong>Cell:</strong> <asp:Label ID="Label_Cell" runat="server" CssClass="fw-bold"></asp:Label> |
                            <strong>Shift:</strong> <asp:Label ID="Label_Shift" runat="server" CssClass="fw-bold"></asp:Label>
                        </div>
                    </div>

                    <!-- Filters Section -->
                <div class="row mb-3">
                    <div class="col-md-2">
                    <label for="DropDownListArea" class="form-label">Pick Area</label>
                    <asp:DropDownList ID="DropDownListArea" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterData">
                    </asp:DropDownList>
                </div>
                <div class="col-md-2">
                    <label for="DropDownListSite" class="form-label">Pick Site</label>
                    <asp:DropDownList ID="DropDownListSite" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="FilterData">
                    </asp:DropDownList>
                </div>
                        <div class="col-md-4 text-center">
                            <label class="fw-bold">Total IC:</label>
                             <asp:Label ID="LabelCountDDT" runat="server" CssClass="badge bg-danger fs-5"></asp:Label>
                            <label class="fw-bold">My IC's:</label>
                            <asp:Label ID="LabelCounttome" runat="server" CssClass="badge bg-primary fs-5"></asp:Label>
                        </div>
                    </div>

                    <!-- Pick Treatment Button -->
                    <div class="text-center mb-4">
                        <asp:Button ID="ButtonPick" runat="server" CssClass="btn btn-success btn-lg px-4" Text="Pick Treatment" OnClick="ButtonPick_Click"/>
                    </div>

                    <!-- Modification Sections -->
                    <div class="row">
                        <!-- Doctor Modification -->
                        <div class="col-md-6 mb-4">
                            <div class="border border-secondary p-3 shadow-sm rounded">
                                <div class="bg-info text-white text-center py-2 rounded">
                                    <h5>Doctor Modification</h5>
                                </div>
                                <asp:DropDownList ID="DropDownListDRMod" runat="server" CssClass="form-select form-select-lg" AutoPostBack="True" OnSelectedIndexChanged="DropDownListDRMod_SelectedIndexChanged">
                                    <asp:ListItem Text="Select One" Value="" />
                                    <asp:ListItem Text="Remove or recover the doctor attachment" />
                                    <asp:ListItem Text="Modification of Bite set by Drs instruction" />
                                    <asp:ListItem Text="Retrieve lingual bar at the Drs suggestion" />
                                    <asp:ListItem Text="Include Pieces by Drs instruction" />
                                    <asp:ListItem Text="Spaces by Drs Instruction" />
                                    <asp:ListItem Text="Dr Instruction Detailing" />
                                </asp:DropDownList>
                            </div>
                        </div>
                         <!-- SS Modification -->
                        <div class="col-md-6 mb-4">
                            <div class="border border-secondary p-3 shadow-sm rounded">
                                <div class="bg-info text-white text-center py-2 rounded">
                                    <h5>SS Modification</h5>
                                </div>
                                <asp:DropDownList ID="DropDownSSModification" runat="server" CssClass="form-select form-select-lg" AutoPostBack="True" OnSelectedIndexChanged="DropDownSSModification_SelectedIndexChanged">
                                    <asp:ListItem Text="Select One" Value="" />
                                    <asp:ListItem Text="Detalling" />
                                    <asp:ListItem Text="Attachments & Features" />
                                    <asp:ListItem Text="Cuts" />
                                    <asp:ListItem Text="Bite set" />
                                    <asp:ListItem Text="Axis" />
                                    <asp:ListItem Text="SQV - Unusual Anatomies" />
                                    <asp:ListItem Text="SQV - Detalling" />
                                    <asp:ListItem Text="SQV - Axis" />
                                    <asp:ListItem Text="SQV - Trimming" />
                                    <asp:ListItem Text="Others" />
                                </asp:DropDownList>
                            </div>
                        </div>
                         <!-- Not Applies Section -->
                    <div class="col-md-6 mb-4">
                            <div class="border border-secondary p-3 shadow-sm rounded">
                                <div class="bg-info text-white text-center py-2 rounded">
                                <h5>Not Applies</h5>
                            </div>
                            <asp:DropDownList ID="DropDownListNotApplies" runat="server" CssClass="form-select form-select-lg" AutoPostBack="True" OnSelectedIndexChanged="DropDownListNotApplies_SelectedIndexChanged">
                                <asp:ListItem Text="Select One" Value="" />
                                <asp:ListItem Text="Another CAD work on it same Ticket" />
                                <asp:ListItem Text="Another CAD work on it distinct Ticket" />
                                <asp:ListItem Text="Treatment on SS" />
                                <asp:ListItem Text="Treatment in Clinchek" />
                                <asp:ListItem Text="PID not found" />
                                <asp:ListItem Text="Unnecessary Correction" />
                            </asp:DropDownList>
                        </div>
                    </div>
                        <!-- Issue by Technician -->
                        <div class="col-md-6 mb-4">
                            <div class="border border-secondary p-3 shadow-sm rounded">
                                <div class="bg-info text-white text-center py-2 rounded">
                                    <h5>Issue by Technician</h5>
                                </div>
                                <asp:DropDownList ID="DropDownListIssueCategory" runat="server" CssClass="form-select form-select-lg" AutoPostBack="True" OnSelectedIndexChanged="DropDownListIssueCategory_SelectedIndexChanged">
                                    <asp:ListItem Text="Select One" Value="" />
                                    <asp:ListItem Text="Design Execution" />
                                    <asp:ListItem Text="Aligner-Retainer Fit Issues" />
                                    <asp:ListItem Text="Incomplete Case" />
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <hr>

                    <div class="row">
                        <div class="col-md-6">
                            <label class="fw-bold">Ticket:</label>
                            <asp:Label ID="LabelTicket" runat="server" CssClass="form-label"></asp:Label>
                        </div>
                        <div class="col-md-6">
                            <label class="fw-bold">PID:</label>
                            <asp:Label ID="LabelPID" runat="server" CssClass="form-label text-danger"></asp:Label>
                        </div>
                    </div>

                    <hr>

                    <div class="row">
                        <div class="col-md-6">
                            <label class="fw-bold">Source:</label>
                            <asp:Label ID="LabelSource" runat="server" CssClass="form-label"></asp:Label>
                        </div>
                        <div class="col-md-6">
                            <label class="fw-bold">Modification:</label>
                            <asp:Label ID="LabelModification" runat="server" CssClass="form-label"></asp:Label>
                        </div>
                    </div>

                    <hr>

                    <div class="row">
                        <div class="col-md-6">
                            <label class="fw-bold">Author:</label>
                            <asp:Label ID="LabelAuthor" runat="server" CssClass="form-label"></asp:Label>
                        </div>
                        <div class="col-md-6">
                            <label class="fw-bold">Details:</label>
                            <asp:Label ID="LabelDetail" runat="server" CssClass="form-label"></asp:Label>
                        </div>
                    </div>

                    <div class="alert alert-danger mt-3" role="alert" runat="server" id="Div1" visible="false">
                        <asp:Label ID="Label1" runat="server"></asp:Label>
                    </div>

                </div>
            </div>
        </div>
    </form>

    <!-- Bootstrap JS -->
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js"></script>
</body>
</html>