<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<CardWall.Models.ChartView>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="Head1" runat="server">
    <title>Charts! Oh My!</title>
    <link rel="stylesheet" type="text/css" href="<%=Url.Content("~/Content/Style.css")%>" />
</head>
<body>
    <div>
    <h1><%=Model.Name%></h1>
    <%=Model.DisplayMarkup %>
    </div>
</body>
</html>
