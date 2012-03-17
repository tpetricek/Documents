<%@ Page Language="C#" 
         Inherits="System.Web.Mvc.ViewPage<IEnumerable<FSharpWeb.Core.SearchResult>>" %>

<html>
  <head>
    <title>
      F#oogle
    </title>
    <style type="text/css">
      body { font-family: Calibri; }
      .hd { font-size: 120%; color: black; }
      h3 { font-size: 100%; color:#808080; font-weight:normal; }
    </style>
  </head>
  <body>
    <table><tr>
      <td><a href="/"><img src="/Content/foogle_sm.png" style="border-style:none;" /></a></td>
      <td>
        <form action="/Search" method="post" style="margin-left: 30px">
          <input type="text" name="id" size="60" />
          <input type="submit" value="Search" />
        </form>
      </td>
    </tr></table>

    <hr />

    <table>
    <% foreach(var item in Model) { %>
    <tr>
      <td style="width: 100px;"><img src="<%= item.Image %>" /></td>
      <td>
        <h3><span class="hd"><a href="<%= item.Link %>"><%= item.Title %></a></span> from <strong><%= item.Source %></strong></h3>
        <p><%= item.Info %></p>
      </td>
    </tr>
    <% } %>
    </table>

  </body>
</html>
