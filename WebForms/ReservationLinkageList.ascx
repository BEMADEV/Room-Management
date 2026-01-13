<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationLinkageList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RoomManagement.ReservationLinkageList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server">

                <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />

                <asp:Panel ID="pnlLinkages" runat="server" CssClass="panel panel-block">
                    <div class="panel-heading">
                        <h1 class="panel-title">
                            <i class="fa fa-link"></i>
                            Events
                        </h1>
                    </div>
                    <div class="panel-body">
                        <Rock:ModalAlert ID="mdLinkagesGridWarning" runat="server" />
                        <div class="grid grid-panel">
                            <Rock:Grid ID="gLinkages" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Linkage" ExportSource="ColumnOutput">
                                <Columns>
                                    <Rock:RockTemplateFieldUnselected HeaderText="Calendar Item">
                                        <ItemTemplate>
                                            <asp:Literal ID="lCalendarItem" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <asp:BoundField HeaderText="Campus" DataField="EventItemOccurrence.Campus.Name" SortExpression="EventItemOccurrence.Campus.Name" NullDisplayText="All Campuses" />
                                    <Rock:RockTemplateFieldUnselected HeaderText="Content Item">
                                        <ItemTemplate>
                                            <asp:Literal ID="lContentItem" runat="server" />
                                        </ItemTemplate>
                                    </Rock:RockTemplateFieldUnselected>
                                    <Rock:DeleteField OnClick="gLinkages_Delete" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>
                </asp:Panel>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
