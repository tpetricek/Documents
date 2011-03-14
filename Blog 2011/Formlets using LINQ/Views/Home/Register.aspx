<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<LinqFormlets.Controllers.Booking>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    
    <h2>Booking</h2>
    <div style="margin:0px 0px 20px 30px">
      <strong>Name:</strong> <%=Model.Name %><br />
      <strong>Departure:</strong> <%=Model.Departure.ToString("d") %><br />
      <strong>Return:</strong> <%=Model.Return.ToString("d") %><br />
    </div>
    <a href="../">Make another booking...</a>

</asp:Content>
