<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CardWall.Models.IterationView>" %>
<%@ Import Namespace="CardWall.Models" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Iteration Overview</title>
    <link rel="stylesheet" type="text/css" href="<%=Url.Content("~/Content/Style.css")%>" />
</head>
<body>
  <table id="content" cellpadding=0 cellspacing=0 border=0>
    <thead>
        <%foreach(var lane in Model.Lanes){ %>
            <th class="lane-header"><%=lane.Name %></th>
        <%} %>
    </thead>
    <tr>
        <%foreach(var lane in Model.Lanes){ %>
        <td class="lane" valign="top">
            <%foreach(var card in lane.Cards) {
                Html.RenderPartial("Card", card);
            } %>
        </td>
        <%} %>
    </tr>
  </table>
</body>
</html>
