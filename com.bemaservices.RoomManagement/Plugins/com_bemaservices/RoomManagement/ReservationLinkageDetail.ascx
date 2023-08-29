<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReservationLinkageDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RoomManagement.ReservationLinkageDetail" %>
<%@ Register TagPrefix="BEMA" Assembly="com.bemaservices.RoomManagement" Namespace="com.bemaservices.RoomManagement.Web.UI.Controls" %>
<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>
<asp:UpdatePanel ID="upnlContent" runat="server">
    <Triggers>
        <asp:AsyncPostBackTrigger ControlID="cblCalendars" EventName="SelectedIndexChanged" />
    </Triggers>
    <ContentTemplate>
        <Rock:NotificationBox ID="nbNotAuthorized" runat="server" NotificationBoxType="Danger" />

        <asp:Panel ID="pnlLavaInstructions" runat="server" Visible="false">
            <asp:Literal ID="lLavaInstructions" runat="server" />
        </asp:Panel>
        <asp:Panel ID="pnlDetails" runat="server" CssClass="panel panel-block">

            <asp:HiddenField ID="hfReservationId" runat="server" />
            <asp:HiddenField ID="hfEventItemGuid" runat="server" />
            <asp:HiddenField ID="hfEventItemOccurrenceGuid" runat="server" />
      
            <asp:Panel ID="pnlEvent_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-calendar-check-o"></i>
                    Reservation Event Linkage - Event</h1>
            </asp:Panel>
            <asp:Panel ID="pnlEventOccurrence_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-clock-o"></i>
                    Reservation Event Linkage - Event Occurrence</h1>
            </asp:Panel>
            <asp:Panel ID="pnlSummary_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-list-ul"></i>
                    Reservation Event Linkage - Summary</h1>
            </asp:Panel>
            <asp:Panel ID="pnlFinished_Header" runat="server" CssClass="panel-heading" Visible="false">
                <h1 class="panel-title"><i class="fa fa-check"></i>
                    Reservation Event Linkage - Finished</h1>
            </asp:Panel>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <asp:Panel ID="pnlWizard" runat="server" CssClass="wizard" Visible="false">
                <div id="divEvent" runat="server" class="wizard-item">
                    <asp:LinkButton ID="lbEvent" runat="server" OnClick="lbEvent_Click" CausesValidation="false">
                            <asp:PlaceHolder runat="server">
                                <div class="wizard-item-icon">
                                    <i class="fa fa-fw fa-calendar-check-o"></i>
                                </div>
                                <div class="wizard-item-label">
                                    Event
                                </div>
                            </asp:PlaceHolder>
                    </asp:LinkButton>
                </div>

                <div id="divEventOccurrence" runat="server" class="wizard-item">
                    <asp:LinkButton ID="lbEventOccurrence" runat="server" OnClick="lbEventOccurrence_Click" CausesValidation="false" Enabled="false">
                            <asp:PlaceHolder runat="server">
                                <div class="wizard-item-icon">
                                    <i class="fa fa-fw fa-clock-o"></i>
                                </div>
                                <div class="wizard-item-label">
                                    Event Occurrence
                                </div>
                            </asp:PlaceHolder>
                    </asp:LinkButton>
                </div>

                <div id="divSummary" runat="server" class="wizard-item">
                    <div class="wizard-item-icon">
                        <i class="fa fa-fw fa-list-ul"></i>
                    </div>
                    <div class="wizard-item-label">
                        Summary
                    </div>
                </div>
            </asp:Panel>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                <asp:ValidationSummary ID="valReservationEventOccurrenceSummary" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="ReservationEventOccurrence" />

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                <Rock:NotificationBox ID="nbError" runat="server" NotificationBoxType="Danger" />
                <Rock:NotificationBox ID="nbWarning" runat="server" NotificationBoxType="Warning" />

                <asp:Panel ID="pnlEvent" runat="server" Visible="false">
                    <asp:ValidationSummary ID="vsEvent" runat="server" HeaderText="Please correct the following:" ValidationGroup="ReservationEvent" CssClass="alert alert-warning" />
                    <fieldset>
                        <asp:Panel ID="pnlNewEventSelection" runat="server">
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:Toggle ID="tglEventSelection" runat="server" ActiveButtonCssClass="btn-primary" OnText="New Event" OffText="Existing Event"
                                        OnCheckedChanged="tglEventSelection_CheckedChanged" />
                                    <hr />
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlExistingEvent" runat="server">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:EventItemPicker ID="eipSelectedEvent" runat="server" Label="Event" Required="true" ValidationGroup="ReservationEvent" />
                                </div>
                            </div>
                        </asp:Panel>
                        <asp:Panel ID="pnlNewEvent" runat="server" Visible="false">
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockTextBox ID="tbCalendarEventName" ValidationGroup="ReservationEvent" runat="server" Label="Calendar Event Name" Required="true" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:RockTextBox ID="tbEventSummary" ValidationGroup="ReservationEvent" runat="server" Label="Summary" TextMode="MultiLine" Rows="4" />
                                </div>
                            </div>
                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:HtmlEditor ID="htmlEventDescription" ValidationGroup="ReservationEvent" runat="server" Label="Description" Toolbar="Light" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:RockControlWrapper ID="rcwAudiences" runat="server" Label="Audiences">
                                        <div class="grid">
                                            <Rock:Grid ID="gAudiences" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Audience" ShowHeader="false">
                                                <Columns>
                                                    <Rock:RockBoundField DataField="Value" />
                                                    <Rock:DeleteField OnClick="gAudiences_Delete" />
                                                </Columns>
                                            </Rock:Grid>
                                        </div>
                                    </Rock:RockControlWrapper>

                                    <Rock:RockCheckBoxList ID="cblCalendars" ValidationGroup="ReservationEvent" runat="server" Label="Calendars"
                                        Help="Calendars that this item should be added to (at least one is required)."
                                        AutoPostBack="true" OnSelectedIndexChanged="cblCalendars_SelectedIndexChanged"
                                        RepeatDirection="Horizontal" Required="true" />
                                    <Rock:RockTextBox ID="tbDetailUrl" ValidationGroup="ReservationEvent" runat="server" Label="Details URL"
                                        Help="A custom url to use for showing details of the calendar item (if the default item detail page should not be used)." />
                                </div>
                                <div class="col-md-6">
                                    <Rock:ImageUploader ID="imgupPhoto" ValidationGroup="ReservationEvent" runat="server" Label="Photo" />
                                </div>
                            </div>

                            <div class="row">
                                <div class="col-md-12">
                                    <Rock:PanelWidget ID="wpAttributes" runat="server" Title="Attribute Values">
                                        <Rock:DynamicPlaceholder ID="phEventItemAttributes" runat="server" />
                                    </Rock:PanelWidget>
                                </div>
                            </div>
                        </asp:Panel>
                        <div class="actions">
                            <asp:LinkButton ID="lbPrev_Event" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Event_Click" />
                            <asp:LinkButton ID="lbNext_Event" ValidationGroup="ReservationEvent" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Event_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>

                <asp:Panel ID="pnlEventOccurrence" runat="server" Visible="false">
                    <asp:ValidationSummary ID="vsEventOccurrence" ValidationGroup="ReservationEventOccurrence" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-warning" />

                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="tbLocationDescription" ValidationGroup="ReservationEventOccurrence" runat="server" Label="Location Description" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockControlWrapper ID="rcwEventOccurrenceSchedule" runat="server" Label="Schedule">
                                    <Rock:ScheduleBuilder ID="sbEventOccurrenceSchedule" runat="server" ValidationGroup="Schedule" AllowMultiSelect="true" Required="true" OnSaveSchedule="sbEventOccurrenceSchedule_SaveSchedule" />
                                    <asp:Literal ID="lEventOccurrenceScheduleText" runat="server" />
                                </Rock:RockControlWrapper>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12">
                                <Rock:HtmlEditor ID="htmlOccurrenceNote" ValidationGroup="ReservationEventOccurrence" runat="server" Label="Occurrence Note" Toolbar="Light" />
                            </div>
                        </div>
                        <div class="actions">
                            <asp:LinkButton ID="lbPrev_EventOccurrence" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_EventOccurrence_Click" />
                            <asp:LinkButton ID="lbNext_EventOccurrence" runat="server" AccessKey="n" Text="Next" DataLoadingText="Next" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_EventOccurrence_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>

                <asp:Panel ID="pnlSummary" runat="server" Visible="false">
                    <div class="alert alert-info">
                        <div><strong>Please confirm the following changes:</strong></div>
                        <asp:PlaceHolder ID="phChanges" runat="server" />
                    </div>

                    <fieldset>
                        <div class="actions">
                            <asp:LinkButton ID="lbPrev_Summary" runat="server" AccessKey="p" ToolTip="Alt+p" Text="Previous" CssClass="btn btn-default js-wizard-navigation" CausesValidation="false" OnClick="lbPrev_Summary_Click" />
                            <asp:LinkButton ID="lbNext_Summary" runat="server" AccessKey="n" Text="Finish" DataLoadingText="Finish" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbNext_Summary_Click" />
                        </div>
                    </fieldset>
                </asp:Panel>

                <asp:Panel ID="pnlFinished" runat="server" Visible="false">
                    <div class="alert alert-success">
                        <div><strong>Success</strong></div>
                        <div>
                            Event Created for
                                <asp:Label ID="lblReservationTitle" runat="server" />
                        </div>
                        <hr />

                        <ul>
                            <li id="liEventLink" runat="server">
                                <asp:HyperLink ID="hlEventDetail" runat="server" Text="View Event Detail" /></li>
                            <li id="liExternalEventLink" runat="server">
                                <asp:HyperLink ID="hlExternalEventDetails" runat="server" Text="View External Event Details" /></li>
                            <li id="liEventOccurrenceLink" runat="server">
                                <asp:HyperLink ID="hlEventOccurrence" runat="server" Text="View Event Occurrence" /></li>
                        </ul>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbReturnToReservation" ValidationGroup="ReservationEvent" runat="server" AccessKey="n" Text="Return to Reservation" DataLoadingText="Return to Reservation" CssClass="btn btn-primary pull-right js-wizard-navigation" OnClick="lbReturnToReservation_Click" />
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgAudience" runat="server" ScrollbarEnabled="false" ValidationGroup="Audience" SaveButtonText="Add" OnCancelScript="clearActiveDialog();" OnSaveClick="btnSaveAudience_Click" Title="Select Audience">
            <Content>
                <asp:ValidationSummary ID="vsAudience" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Audience" />
                <Rock:RockDropDownList ID="ddlAudience" runat="server" Label="Select Audience" ValidationGroup="Audience" Required="true" DataValueField="Id" DataTextField="Value" />
            </Content>
        </Rock:ModalDialog>
    </ContentTemplate>
</asp:UpdatePanel>
