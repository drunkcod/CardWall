<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CardWall.BadgeConfiguration>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>BADGES!!!</title>
</head>
<body>
<%foreach(var badge in Model.Badges) { %>
    <div><img src="<%=Url.Content(badge.Url)%>" title="<%=badge.Name %>" /> <%=badge.Name%></div>
<%} %>
</body>
</html>
