<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<LinqFormlets.Formlet<LinqFormlets.Controllers.Booking>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% using (Html.BeginForm("Register", "Home")) { %>
        <%= Model.Render() %>
        <input type="submit" value="Submit" />
    <% } %>
</asp:Content>
