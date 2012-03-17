<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage" %>

<html>
  <head>
    <title>
      F#oogle
    </title>
  </head>
  <body style="text-align:center;padding:50px;">
    <img src="/Content/foogle.png" />
    <form action="/Search" method="post">
      <input type="text" name="id" size="60" />
      <input type="submit" value="Search" style="margin-top:30px;" />
    </form>
  </body>
</html>
