﻿<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Index</title>
</head>
<body>
    <div>
    <img src="<%=Model.ToString() %>" />
    <img src="http://chart.apis.google.com/chart?chxr=0,0,46&chxt=y&chs=300x225&cht=lc&chco=3D7930&chd=s:Xhiugtqi&chg=14.3,-1,1,1&chls=2,4,0&chm=o,3D7930,0,-1,10|o,FFFFFF,0,-1,5" />
    </div>
</body>
</html>
